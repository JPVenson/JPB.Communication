using JPB.Communication.WinRT.combase.Messages;

namespace JPB.Communication.WinRT.combase.Security
{
	public class LoginMessageEx : LoginMessage
	{
		public LoginMessageEx(string host, ushort port)
		{
			Host = host;
			Port = port;
		}
		public AuditState State { get; set; }

		public string Host { get; private set; }
		public ushort Port { get; private set; }
	}
}