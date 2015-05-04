using JPB.Communication.ComBase.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace JPB.Communication.PCLIntigration.ComBase.Messages
{
    [DebuggerDisplay("{SessionID}")]
    public class LoginMessage
    {
        public LoginMessage()
        {

        }
        public string SessionID { get; set; }

        public string Username { get; set; }
        /// <summary>
        /// This should never be a plain Password and Ever a Password Hash
        /// </summary>
        public string Password { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", SessionID.Trim(), Username.Trim(), Password.Trim());
        }
    }
}
