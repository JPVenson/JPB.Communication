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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JPB.Communication.ComBase
{
    public class InternalMemoryHolder : IDisposable
    {
        private List<byte[]> _datarec = new List<byte[]>();

        internal byte[] Last { get; set; }

        internal bool IsSharedMem { get; set; }

        private FileStream _fileStream;

        private Task _writeAsync;

        /// <summary>
        /// Maximum bytes in storeage
        /// This are 20 Mbit
        /// </summary>
        public const int MaximumStoreageInMemory = 20971520;

        private bool ShouldPageToDisk()
        {
            return Last.Length*_datarec.Count >= MaximumStoreageInMemory;
        }

        public async void Add(byte[] bytes)
        {
            Last = bytes;

            //this will write the content async to the Buffer as long as there is no other write action to do
            //if we are still writing async inside an other Add, wait for the last one
            if (_writeAsync != null)
                await _writeAsync;

            if (ShouldPageToDisk() && !IsSharedMem)
            {
                _fileStream = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete);
                var completeBytes = privateGet();
                _writeAsync = _fileStream.WriteAsync(completeBytes, 0, completeBytes.Length);
                IsSharedMem = true;
            }
            if (IsSharedMem)
            {
                _writeAsync = _fileStream.WriteAsync(bytes, 0, bytes.Length);
            }
            else
            {
                _datarec.Add(bytes);
            }
        }

        private byte[] privateGet()
        {
            return !IsSharedMem ? _datarec.SelectMany(s => s).ToArray() : Last;
        }

        public byte[] Get()
        {
            return privateGet();
        }

        public void Dispose()
        {
            if (_writeAsync != null)
                _writeAsync.Wait();

            if (_fileStream != null)
            {
                try
                {
                    if (File.Exists(_fileStream.Name))
                        File.Delete(_fileStream.Name);
                }
                catch (Exception e)
                {
                    throw;
                }
                finally
                {
                    _fileStream.Dispose();
                }
            }
            _datarec = null;
        }
    }
}