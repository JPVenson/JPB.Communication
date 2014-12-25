using System;
using System.Threading;

namespace JPB.Communication.ComBase
{
    public class AsyncResultWrapper : IAsyncResult
    {
        public bool IsCompleted { get { return false; } }
        public WaitHandle AsyncWaitHandle { get; private set; }
        public object AsyncState { get; set; }
        public bool CompletedSynchronously { get; private set; }
    }
}