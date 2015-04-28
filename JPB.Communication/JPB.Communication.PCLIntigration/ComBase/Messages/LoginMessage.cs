using JPB.Communication.ComBase.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.PCLIntigration.ComBase.Messages
{
    public class LoginMessage
    {
        public string Username { get; set; }
        /// <summary>
        /// This should never be a plain Password and Ever a Password Hash
        /// </summary>
        public string Password { get; set; }
    }
}
