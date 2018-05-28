/*
 Created by Jean-Pierre Bachmann
 Visit my GitHub page at:
 
 https://github.com/JPVenson/

 Please respect the Code and Work of other Programers an Read the license carefully

 GNU AFFERO GENERAL PUBLIC LICENSE
                       Version 3, 19 November 2007

 Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>
 Everyone is permitted to copy and distribute verbatim copies
 of this license document, but changing it is not allowed.

 READ THE FULL LICENSE AT:

 https://github.com/JPVenson/JPB.Communication/blob/master/LICENSE
 */

using System;
using JPB.Communication.WinRT.combase.Messages.Wrapper;
using JPB.Communication.WinRT.Contracts.Intigration;

namespace JPB.Communication.WinRT.combase
{
    internal abstract class ConnectionBase : Networkbase
    {
        public const string ErrorDueParse = "ERR / Message is Corrupt";
	    public bool MessageParsed { get; set; }

		internal bool ParseAndPublish(byte[] received, ISocket sok)
        {
            object item;
            try
            {
                item = DeSerialize(received);

                if (item != null)
				{
					MessageParsed = true;
					RaiseNewItemLoadedSuccess(item, new MessageBaseInfo() { RecievedAt = DateTime.Now, Sender = sok.RemoteEndPoint.Address.AddressContent });
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                string source;
                try
                {
                    source = Serlilizer.ResolveStringContent(received);
                }
                catch (Exception)
                {
                    source = ErrorDueParse;
                }

                RaiseNewItemLoadedFail(source);
                return false;
            }
        }

        internal LargeMessage ParseLargeObject(byte[] received)
        {
            object item;
            try
            {
                item = DeSerialize(received);

                if (item != null)
                {
                    return RaiseNewLargeItemLoadedSuccess(item);
                }
                return null;
            }
            catch (Exception)
            {
                string source;
                try
                {
                    source = Serlilizer.ResolveStringContent(received);
                }
                catch (Exception)
                {
                    source = ErrorDueParse;
                }

                RaiseNewItemLoadedFail(source);
                return null;
            }
        }
    }
}