using System.Diagnostics;

namespace JPB.Communication.ComBase.Messages
{
    [DebuggerDisplay("{SessionID}")]
    public class LoginMessage
    {
        public LoginMessage()
        {

        }
        public string SessionID { get; set; }

        public string Username { get; set; }

        /// <summary>
        /// This should !never be a plain Password and always a Password Hash
        /// </summary>
        public string Password { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1}", SessionID.Trim(), Username.Trim());
        }
    }
}
