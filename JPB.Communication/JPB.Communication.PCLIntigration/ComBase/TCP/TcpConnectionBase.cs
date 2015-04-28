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
using System.Collections.Generic;

namespace JPB.Communication.ComBase.TCP
{
    public enum HandeldMode
    {
        NoDateAvailble,
        MaybeMoreData,
        Handled,
        Exception
    }

    internal abstract class TcpConnectionBase : ConnectionBase
    {
        public TcpConnectionBase()
        {
            _datarec = new InternalMemoryHolder();
        }

        public event EventHandler EndReceiveInternal;
        protected readonly InternalMemoryHolder _datarec;
        protected int _receiveBufferSize;
        public bool IsSharedConnection { get; set; }

        public abstract void BeginReceive();
        protected void RaiseEndReceiveInternal()
        {
            var handler = EndReceiveInternal;

            if (handler != null)
                handler(this, new EventArgs());
        }

        protected HandeldMode HandleRec(int rec)
        {
            //0 is: No more Data
            //1 is: Part Complted
            //<1 is: Content

            if (rec == -1)
                return HandeldMode.Exception;

            //to incomming data left
            //try to concat the message
            if ((rec == 0 || rec == 1) || (IsSharedConnection && rec < _receiveBufferSize))
            {
                //Wrong Partial byte only call?
                byte[] buff = _datarec.Get(rec);
                if (buff.Length <= 2)
                {
                    RaiseEndReceiveInternal();
                    _datarec.Clear();
                    return HandeldMode.NoDateAvailble;
                }

                int count = buff.Length;
                var compltearray = new List<byte>();
                bool beginContent = false;
                for (int i = 0, f = 0; f < count; f++)
                {
                    byte b = buff[f];
                    if (!beginContent)
                    {
                        if (b == 0x00)
                        {
                            continue;
                        }
                        beginContent = true;
                    }
                    compltearray.Add(b);
                    i++;
                }

                var content = compltearray.ToArray();

                if (content.Length <= 10)
                {
                    _datarec.Clear();
                    return HandeldMode.MaybeMoreData;
                }

                var parsedCorretly = Parse(content);
                if (!parsedCorretly)
                {
                    if (IsSharedConnection)
                        _datarec.Clear();
                    //Maybe not full message
                    return HandeldMode.MaybeMoreData;
                }
                RaiseEndReceiveInternal();
                return HandeldMode.Handled;
            }


            return HandeldMode.MaybeMoreData;
        }
    }
}