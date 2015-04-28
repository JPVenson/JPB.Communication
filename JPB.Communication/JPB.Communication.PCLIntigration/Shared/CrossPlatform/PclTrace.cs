using System;
using System.IO;
using System.Threading.Tasks;

namespace JPB.Communication.Shared.CrossPlatform
{
    public class PclTrace
    {
        public static TextWriter LogWriter { get; private set; }

        public const string CategoryLineTemplate = "{0} -> {1}";

        private static PclTrace _instance = new PclTrace();

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

        public static async Task WriteAsync(string message)
        {
            if (LogWriter != null) await LogWriter.WriteAsync(message);
            OnMessageWritten(new PclTraceWriteEventArgs(message));
        }

        public static async Task WriteLineAsync(string message)
        {
            if (LogWriter != null) await LogWriter.WriteLineAsync(message);
            OnMessageWritten(new PclTraceWriteEventArgs(message));
        }

        public static void Write(string message)
        {
            if (LogWriter != null) LogWriter.Write(message);
            OnMessageWritten(new PclTraceWriteEventArgs(message));
        }

        public static void WriteLine(string message)
        {
            if (LogWriter != null) LogWriter.WriteLine(message);
            OnMessageWritten(new PclTraceWriteEventArgs(message));
        }

        public static void WriteLine(string message, string category)
        {
            if (LogWriter != null) LogWriter.WriteLine(CategoryLineTemplate, category, message);
            OnMessageWritten(new PclTraceWriteEventArgs(message));
        }

        protected static void OnMessageWritten(PclTraceWriteEventArgs args)
        {
            NetworkFactory.PlatformFactory.RaiseTraceMessage(_instance, args);
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
