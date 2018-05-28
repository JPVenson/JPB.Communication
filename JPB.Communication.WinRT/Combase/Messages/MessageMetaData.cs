using System.Collections.Generic;

namespace JPB.Communication.WinRT.combase.Messages
{
	internal class MessageMetaData
	{
		public virtual IEnumerable<KeyValuePair<string, string>> GetMetaData()
		{
			yield break;
		}
	}
}