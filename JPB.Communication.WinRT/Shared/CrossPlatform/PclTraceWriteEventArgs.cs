using System;

namespace JPB.Communication.WinRT.Shared.CrossPlatform
{
	public class PclTraceWriteEventArgs : EventArgs
	{
		public PclTraceWriteEventArgs(string message)
		{
			WrittenMessage = message;
		}
		public string WrittenMessage { get; private set; }
	}
}