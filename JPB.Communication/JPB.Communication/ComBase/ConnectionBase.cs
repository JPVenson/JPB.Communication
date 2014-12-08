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
using System.Collections.ObjectModel;
using System.Text;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    internal abstract class ConnectionBase : Networkbase
    {
        protected ConnectionBase()
        {

        }

        public const string ErrorDueParse = "ERR / Message is Corrupt";
        
        internal bool Parse(byte[] received)
        {
            TcpMessage item;
            try
            {
                item = DeSerialize(received);
                RaiseIncommingMessage(item);

                if (item != null)
                {
                    var loadMessageBaseFromBinary = base.LoadMessageBaseFromBinary(item.MessageBase);
                    RaiseNewItemLoadedSuccess(loadMessageBaseFromBinary);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                string source;
                try
                {
                    source = this.Serlilizer.ResolveStringContent(received);
                }
                catch (Exception)
                {
                    source = ErrorDueParse;
                }

                RaiseNewItemLoadedFail(source);
                return false;
            }
        }
    }
}