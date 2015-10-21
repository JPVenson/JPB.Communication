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
using System.Linq;
using System.Threading.Tasks;

namespace JPB.Communication.ComBase
{
    public class InternalMemoryHolder : IDisposable
    {
        /// <summary>
        ///     Maximum bytes in storeage
        ///     This are 20 Mbit
        /// </summary>
        public const int MaximumStoreageInMemory = 20971520;

        private List<byte[]> _datarec = new List<byte[]>();

        private Task _writeAsync;

        internal byte[] Last { get; set; }

        internal bool IsSharedMem { get; set; }

        /// <summary>
        ///     Forceses the usage of FIles
        /// </summary>
        public bool ForceSharedMem { get; set; }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            if (Disposed)
                return;

            Disposed = true;

            if (_writeAsync != null)
                _writeAsync.Wait();
           
            _datarec = null;
        }

        private bool ShouldPageToDisk()
        {
            if (Disposed)
                return false;

            return ForceSharedMem || Last.Length*_datarec.Count >= MaximumStoreageInMemory;
        }

        /// <summary>
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="adjustLast"></param>
        public async void Add(byte[] bytes, int adjustLast)
        {
            if (Disposed)
                return;

            //this will write the content async to the Buffer as long as there is no other write action to do
            //if we are still writing async inside an other Add, wait for the last one
            if (_writeAsync != null)
            {
                await _writeAsync;
            }
            AdjustSize(adjustLast);
            Last = bytes;
        }

        private void AdjustSize(int adjustLast)
        {
            if (Last != null && adjustLast > 0)
            {
                var consumedBytes = new byte[adjustLast];
                if (adjustLast < Last.Length)
                {
                    for (int i = 0; i < adjustLast; i++)
                    {
                        consumedBytes[i] = Last[i];
                    }
                }
                else
                {
                    consumedBytes = Last;
                }
                _datarec.Add(consumedBytes);
            }
        }

        private byte[] privateGet(int adjustLast)
        {
            if (_writeAsync != null)
                _writeAsync.Wait();

            AdjustSize(adjustLast);
            if (!IsSharedMem)
                return _datarec.SelectMany(s => s).ToArray();
            return Last;
        }

        public byte[] Get(int adjustLast)
        {
            return privateGet(adjustLast);
        }
      
        public void Clear()
        {
            if (Disposed)
                return;

            IsSharedMem = false;           
            if (_datarec != null)
                _datarec.Clear();
        }
    }
}

//if (ShouldPageToDisk() && !IsSharedMem)
//{
//    _fileStream = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete);
//    var completeBytes = privateGet();
//    _writeAsync = _fileStream.WriteAsync(completeBytes, 0, completeBytes.Length);
//    _datarec.Clear();
//    IsSharedMem = true;
//}
//if (IsSharedMem)
//{
//    _writeAsync = _fileStream.WriteAsync(bytes, 0, bytes.Length);
//}
//else
//{
//    _datarec.Add(bytes);
//}