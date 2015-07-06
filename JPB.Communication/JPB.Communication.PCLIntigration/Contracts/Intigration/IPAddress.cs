using System;

namespace JPB.Communication.Contracts.Intigration
{
    public class IPAddress
    {
        private IPAddress()
        {
        }

        public long Address { get; private set; }

        public bool IsFake { get; set; }

        public string AddressContent
        {
            get { return ToString(); }
            private set { m_ToString = value; }
        }

        private string m_ToString;

        public static bool TryParse(string ipString, out IPAddress ipAddress)
        {
            ipAddress = null;
            return InternalParse(ipString, true) != null;
        }

        public static IPAddress Parse(string ipString)
        {
            return InternalParse(ipString, false);
        }

        public static IPAddress ParseToFake(string ipString)
        {
            var ipAddress = new IPAddress();
            ipAddress.AddressContent = ipString;
            ipAddress.IsFake = true;
            return ipAddress;
        }

        //
        // we need this internally since we need to interface with winsock
        // and winsock only understands Int32
        //
        internal IPAddress(int newAddress)
        {
            Address = (long)newAddress & 0x00000000FFFFFFFF;
        }

        /// <summary>
        /// Currently IPV4 Only
        /// </summary>
        /// <param name="newAddress"></param>
        public IPAddress(long newAddress)
        {
            if (newAddress < 0 || newAddress > 0x00000000FFFFFFFF)
            {
                throw new ArgumentOutOfRangeException("newAddress");
            }
            Address = newAddress;
        }
        
        private static IPAddress InternalParse(string ipString, bool tryParse)
        {
            if (ipString == null)
            {
                if (tryParse)
                {
                    return null;
                }
                throw new ArgumentNullException("ipString");
            }

            // The new IPv4 parser is better than the native one, it can parse 0xFFFFFFFF. (It's faster too).

            // App-Compat: The .NET 4.0 parser used Winsock.  When we removed this initialization in 4.5 it 
            // uncovered bugs in IIS's management APIs where they failed to initialize Winsock themselves.
            // DDCC says we need to keep this for an in place release, but to remove it in the next SxS release.
            ///////////////////////////

            int end = ipString.Length;
            long result;
            unsafe
            {
                fixed (char* name = ipString)
                {
                    result = ParseNonCanonical(name, 0, ref end, true);
                }
            }

            if (result == Invalid || end != ipString.Length)
            {
                if (tryParse)
                {
                    return null;
                }

                throw new FormatException("Bad IP");
            }

            // IPv4AddressHelper always returns IP address in a format that we need to reverse.
            result = (((result & 0x000000FF) << 24) | (((result & 0x0000FF00) << 8)
                                                       | (((result & 0x00FF0000) >> 8) | ((result & 0xFF000000) >> 24))));

            return new IPAddress(result);

        } // Parse

        internal const long Invalid = -1;
        // Note: the native parser cannot handle MaxIPv4Value, only MaxIPv4Value - 1
        private const long MaxIPv4Value = UInt32.MaxValue;
        private const int Octal = 8;
        private const int Decimal = 10;
        private const int Hex = 16;

        private const int NumberOfLabels = 4;

        internal static unsafe long ParseNonCanonical(char* name, int start, ref int end, bool notImplicitFile)
        {
            int numberBase = Decimal;
            char ch;
            long[] parts = new long[4];
            long currentValue = 0;
            bool atLeastOneChar = false;

            // Parse one dotted section at a time
            int dotCount = 0; // Limit 3
            int current = start;
            for (; current < end; current++)
            {
                ch = name[current];
                currentValue = 0;

                // Figure out what base this section is in
                numberBase = Decimal;
                if (ch == '0')
                {
                    numberBase = Octal;
                    current++;
                    atLeastOneChar = true;
                    if (current < end)
                    {
                        ch = name[current];
                        if (ch == 'x' || ch == 'X')
                        {
                            numberBase = Hex;
                            current++;
                            atLeastOneChar = false;
                        }
                    }
                }

                // Parse this section
                for (; current < end; current++)
                {
                    ch = name[current];
                    int digitValue;

                    if ((numberBase == Decimal || numberBase == Hex) && '0' <= ch && ch <= '9')
                    {
                        digitValue = ch - '0';
                    }
                    else if (numberBase == Octal && '0' <= ch && ch <= '7')
                    {
                        digitValue = ch - '0';
                    }
                    else if (numberBase == Hex && 'a' <= ch && ch <= 'f')
                    {
                        digitValue = ch + 10 - 'a';
                    }
                    else if (numberBase == Hex && 'A' <= ch && ch <= 'F')
                    {
                        digitValue = ch + 10 - 'A';
                    }
                    else
                    {
                        break; // Invalid/terminator
                    }

                    currentValue = (currentValue*numberBase) + digitValue;

                    if (currentValue > MaxIPv4Value) // Overflow
                    {
                        return Invalid;
                    }

                    atLeastOneChar = true;
                }

                if (current < end && name[current] == '.')
                {
                    if (dotCount >= 3 // Max of 3 dots and 4 segments
                        || !atLeastOneChar // No empty segmets: 1...1
                        // Only the last segment can be more than 255 (if there are less than 3 dots)
                        || currentValue > 0xFF)
                    {
                        return Invalid;
                    }
                    parts[dotCount] = currentValue;
                    dotCount++;
                    atLeastOneChar = false;
                    continue;
                }
                // We don't get here unless We find an invalid character or a terminator
                break;
            }

            // Terminators
            if (!atLeastOneChar)
            {
                return Invalid; // Empty trailing segment: 1.1.1.
            }
            else if (current >= end)
            {
                // end of string, allowed
            }
            else if ((ch = name[current]) == '/' || ch == '\\' ||
                     (notImplicitFile && (ch == ':' || ch == '?' || ch == '#')))
            {
                end = current;
            }
            else
            {
                // not a valid terminating character
                return Invalid;
            }

            parts[dotCount] = currentValue;

            // Parsed, reassemble and check for overflows
            switch (dotCount)
            {
                case 0: // 0xFFFFFFFF
                    if (parts[0] > MaxIPv4Value)
                    {
                        return Invalid;
                    }
                    return parts[0];
                case 1: // 0xFF.0xFFFFFF
                    if (parts[1] > 0xffffff)
                    {
                        return Invalid;
                    }
                    return (parts[0] << 24) | (parts[1] & 0xffffff);
                case 2: // 0xFF.0xFF.0xFFFF
                    if (parts[2] > 0xffff)
                    {
                        return Invalid;
                    }
                    return (parts[0] << 24) | ((parts[1] & 0xff) << 16) | (parts[2] & 0xffff);
                case 3: // 0xFF.0xFF.0xFF.0xFF
                    if (parts[3] > 0xff)
                    {
                        return Invalid;
                    }
                    return (parts[0] << 24) | ((parts[1] & 0xff) << 16) | ((parts[2] & 0xff) << 8) | (parts[3] & 0xff);
                default:
                    return Invalid;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Converts the Internet address to standard dotted quad format
        ///    </para>
        /// </devdoc>
        public override string ToString()
        {
            if (m_ToString == null)
            {
                unsafe
                {
                    const int MaxSize = 15;
                    int offset = MaxSize;
                    char* addressString = stackalloc char[MaxSize];
                    int number = (int)((Address >> 24) & 0xFF);
                    do
                    {
                        addressString[--offset] = (char)('0' + number % 10);
                        number = number / 10;
                    } while (number > 0);
                    addressString[--offset] = '.';
                    number = (int)((Address >> 16) & 0xFF);
                    do
                    {
                        addressString[--offset] = (char)('0' + number % 10);
                        number = number / 10;
                    } while (number > 0);
                    addressString[--offset] = '.';
                    number = (int)((Address >> 8) & 0xFF);
                    do
                    {
                        addressString[--offset] = (char)('0' + number % 10);
                        number = number / 10;
                    } while (number > 0);
                    addressString[--offset] = '.';
                    number = (int)(Address & 0xFF);
                    do
                    {
                        addressString[--offset] = (char)('0' + number % 10);
                        number = number / 10;
                    } while (number > 0);
                    m_ToString = new string(addressString, offset, MaxSize - offset);
                }
            }
            return m_ToString;
        }
    }
}
