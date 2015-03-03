using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.PCLIntigration.Shared.CrossPlatform
{
    public class PclTrace
    {
        public TextWriter LogWriter { get; private set; }

        public PclTrace(TextWriter writer)
        {
            SetTracer(writer);
        }

        public PclTrace() { }

        public void SetTracer(TextWriter writer)
        {
            LogWriter = writer;
            if (LogWriter is StreamWriter)
                (LogWriter as StreamWriter).AutoFlush = true;
        }

        public async Task WriteAsync(string message)
        {
            if (LogWriter != null) await LogWriter.WriteAsync(message);
            OnMessageWritten(new PclTraceWriteEventArgs(message));
        }

        public async Task WriteLineAsync(string message)
        {
            if (LogWriter != null) await LogWriter.WriteLineAsync(message);
            OnMessageWritten(new PclTraceWriteEventArgs(message));
        }

        public void Write(string message)
        {
            if (LogWriter != null) LogWriter.Write(message);
            OnMessageWritten(new PclTraceWriteEventArgs(message));
        }

        public void WriteLine(string message)
        {
            if (LogWriter != null) LogWriter.WriteLine(message);
            OnMessageWritten(new PclTraceWriteEventArgs(message));
        }

        protected virtual void OnMessageWritten(PclTraceWriteEventArgs args)
        {
            NetworkFactory.PlatformFactory.RaiseTraceMessage(this, args);
        }

        internal static void WriteLine(string p, string TraceCategory)
        {
            throw new NotImplementedException();
        }
    }


    public class PclTraceWriteEventArgs : EventArgs
    {
        public PclTraceWriteEventArgs(string message)
        {
            WrittenMessage = message;
        }
        public string WrittenMessage { get; private set; }
    }
}
