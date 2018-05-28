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
using System.IO;
using System.Threading.Tasks;

namespace JPB.Communication.WinRT.combase
{
	public class InternalMemoryHolder : IDisposable
	{
		public InternalMemoryHolder()
		{
			DataBuffer = new MemoryStream();
		}

		/// <summary>
		///     Maximum bytes in storeage
		///     This are 20 Mbit
		/// </summary>
		public const int MaximumStoreageInMemory = 20971520;

		public MemoryStream DataBuffer { get; private set; }

		private Task _writeAsync;

		internal byte[] WriteBuffer { get; set; }

		public bool First { get; internal set; }

		public int DataWritten { get; private set; }

		/// <summary>
		///     Forceses the usage of FIles
		/// </summary>
		public bool ForceSharedMem { get; set; }

		public bool Disposed { get; private set; }

		public void Dispose()
		{
			if (Disposed)
			{
				return;
			}

			Disposed = true;

			if (_writeAsync != null)
			{
				_writeAsync.Wait();
			}

			DataBuffer = null;
		}

		private bool ShouldPageToDisk()
		{
			if (Disposed)
			{
				return false;
			}

			return ForceSharedMem || WriteBuffer.Length + DataBuffer.Length >= MaximumStoreageInMemory;
		}

		/// <summary>
		/// </summary>
		/// <param name="adjustWriteBuffer"></param>
		public async Task Add(int adjustWriteBuffer)
		{
			if (adjustWriteBuffer < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(adjustWriteBuffer));
			}

			if (adjustWriteBuffer == 0)
			{
				return;
			}

			if (Disposed)
			{
				return;
			}

			//this will write the content async to the Buffer as long as there is no other write action to do
			//if we are still writing async inside an other Add, wait for the last one
			if (_writeAsync != null)
			{
				await _writeAsync;
			}
			DataBuffer.Write(WriteBuffer, 0, adjustWriteBuffer);
			DataWritten += adjustWriteBuffer;
		}

		public byte[] Get(int adjustLast)
		{
			if (_writeAsync != null)
			{
				_writeAsync.Wait();
			}
			return DataBuffer.ToArray();
		}

		public void Clear()
		{
			if (Disposed)
			{
				return;
			}

			//IsSharedMem = false;
			DataWritten = 0;
			if (DataBuffer != null)
			{
				DataBuffer.Seek(0, SeekOrigin.Begin);
				DataBuffer.SetLength(0);
			}
		}

		private void AdujustDataBuffer(int numberOfBytesToRemove)
		{
			var retainBufferSize = DataBuffer.Length - numberOfBytesToRemove;

			if (retainBufferSize == 0)
			{
				Clear();
				return;
			}

			var oldData = new byte[retainBufferSize];
			DataBuffer.Read(oldData, numberOfBytesToRemove, oldData.Length);
			DataBuffer.SetLength(0);
			DataBuffer.Write(oldData, 0, oldData.Length);
		}

		public byte[] Splice(int messageSize)
		{
			var dataBuffer = new byte[messageSize];
			DataBuffer.Seek(-messageSize, SeekOrigin.Current);
			DataBuffer.Read(dataBuffer, 0, messageSize);
			AdujustDataBuffer(messageSize);
			return dataBuffer;
		}

		public void SetBuffer(int receiveBufferSize)
		{
			WriteBuffer = new byte[receiveBufferSize];
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