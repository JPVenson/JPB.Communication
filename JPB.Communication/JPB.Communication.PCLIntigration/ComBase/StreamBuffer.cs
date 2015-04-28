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

using System.IO;
using System.Threading.Tasks;

namespace JPB.Communication.ComBase
{
    /// <summary>
    ///     Buffers a Chunck in Memory and allows the Adjustment of the bytes that are conent
    ///     Due the fact that we are init a buffer with a fixed size but the complete size of written bytes are not that high
    ///     we got some empty space at the end
    /// </summary>
    public class StreamBuffer : Stream
    {
        public readonly Stream UnderlyingStream;

        private Task _writeAsync;

        public StreamBuffer()
        {
            //UnderlyingStream = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite,
            //    FileShare.ReadWrite);
            //TODO Move WinRT

            UnderlyingStream = NetworkFactory.PlatformFactory.CreatePlatformFileStream();
        }

        public byte[] Last { get; private set; }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return UnderlyingStream.Length; }
        }

        public override long Position
        {
            get { return -1; }
            set { }
        }

        public override void Flush()
        {
            Flush(Last.Length);
        }

        /// <summary>
        ///     Possible Blocking
        /// </summary>
        /// <param name="adjustContent"></param>
        public void Flush(int adjustContent)
        {
            if (_writeAsync != null)
            {
                _writeAsync.Wait();
            }

            //we are writing async as long as there is no other writing process
            if (Last != null)
                _writeAsync = UnderlyingStream.WriteAsync(Last, 0, adjustContent);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return -1;
        }

        public override void SetLength(long value)
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return -1;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Write(buffer);
        }

        public void Write(byte[] buffer)
        {
            Last = buffer;
        }

        internal void FlushWait()
        {
            this.Flush();
            if (_writeAsync != null)
                _writeAsync.Wait();
        }

        internal void Wait()
        {
            if (_writeAsync != null)
                _writeAsync.Wait();
        }
    }
}