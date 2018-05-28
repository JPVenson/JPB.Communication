namespace JPB.Communication.WinRT.combase.Security
{
	public enum AuditState
	{
		AccessAllowed,
		AccessDenyed,
		CheckPassword,
		/// <summary>
		/// This means that the next time the User trys to login, your handle will be invoked again istead of using the Session ID
		/// </summary>
		Unknown
	}
}