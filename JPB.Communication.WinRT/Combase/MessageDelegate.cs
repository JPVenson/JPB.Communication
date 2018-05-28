using JPB.Communication.WinRT.combase.Messages.Wrapper;

namespace JPB.Communication.WinRT.combase
{
	/// <summary>
	///     Delegate for Incomming or Outging messages
	/// </summary>
	/// <param name="mess"></param>
	/// <param name="port"></param>
	public delegate void MessageDelegate(object mess, ushort port, MessageBaseInfo info);
}