using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.Communication.Contracts.Intigration;
using JPB.Communication.PCLIntigration.ComBase.Messages;

namespace JPB.Communication.PCLIntigration.ComBase
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
    public enum AuditState
    {
        AccessAllowed,
        AccessDenyed,
        /// <summary>
        /// This means that the next time the User trys to login, your handle will be invoked again istead of using the Session ID
        /// </summary>
        Unknown
    }
    public enum DefaultLoginBevavior
    {
        AllowAllways,
        DenyAllways,
        IpNameCheckOnly
    }

    public class NetworkAuthentificator
    {
        static NetworkAuthentificator()
        {
            NameBufferSize = 128;
            PasswordBufferSize = 128;
            SessionBufferSize = 128;
        }
        private NetworkAuthentificator()
        {
            _logins = new List<LoginMessageEx>();
            DefaultLoginBevavior = DefaultLoginBevavior.DenyAllways;
        }

        private static NetworkAuthentificator _instance;

        public static int ReceiveBufferSize
        {
            get
            {
                return NameBufferSize + PasswordBufferSize + SessionBufferSize;
            }
        }
        internal static readonly int NameBufferSize;
        internal static readonly int PasswordBufferSize;
        internal static readonly int SessionBufferSize;
        public DefaultLoginBevavior DefaultLoginBevavior { get; set; }

        public bool ShouldCacheResults { get; set; }

        private List<LoginMessageEx> _logins;
        public event Func<object, LoginMessage, AuditState> OnValidateUnknownLogin;
        public event Action<object, LoginMessage> OnLoginInbound;
        public event Func<LoginMessage, LoginMessage, bool> OnValidateUserPassword;

        public IEnumerable<LoginMessageEx> GetLogins()
        {
            return _logins;
        }

        public static NetworkAuthentificator Instance
        {
            get
            {
                return _instance ?? (_instance = new NetworkAuthentificator());
            }
        }

        public void AddUser(string host, ushort port, string username, string password, AuditState state)
        {
            var login = new LoginMessageEx(host, port)
            {
                State = state,
                Username = username,
                Password = password
            };
            _logins.Add(login);
        }

        private LoginMessageEx RaiseOnValidateUnknownLogin(LoginMessage message, string host, ushort port)
        {
            var handler = OnValidateUnknownLogin;
            AuditState state;
            if (handler != null)
            {
                try
                {
                    state = handler(this, message);
                }
                catch (Exception)
                {
                    state = AuditState.Unknown;
                }
            }
            else
            {
                switch (DefaultLoginBevavior)
                {
                    case DefaultLoginBevavior.AllowAllways:
                        state = AuditState.AccessAllowed;
                        break;
                    case DefaultLoginBevavior.DenyAllways:
                        state = AuditState.AccessDenyed;
                        break;
                    case DefaultLoginBevavior.IpNameCheckOnly:
                        var fod = _logins.FirstOrDefault(s => s.Username == message.Username);
                        state = fod == null ? AuditState.AccessAllowed : AuditState.AccessDenyed;
                        if (fod != null)
                            return fod;
                        break;
                    default:
                        state = AuditState.AccessDenyed;
                        break;
                }
            }

            var login = new LoginMessageEx(host, port)
            {
                SessionID = message.SessionID,
                State = state,
                Username = message.Username,
                Password = message.Password
            };
            _logins.Add(login);
            return login;
        }

        private void RaiseOnLogin(LoginMessage message)
        {
            var handler = OnLoginInbound;
            if (handler != null)
            {
                try
                {
                    handler(this, message);
                }
                catch (Exception)
                {

                }
            }
        }

        private bool RaiseOnUserPasswordValidate(LoginMessage mess, LoginMessage original)
        {
            var handler = OnValidateUserPassword;
            if (handler != null)
            {
                return handler(mess, original);
            }
            else
            {
                throw new NotImplementedException("Please use the OnValidateUserPassword to validate your password");
            }
        }

        internal bool CheckCredentials(LoginMessage credMessage, string host, ushort port)
        {
            RaiseOnLogin(credMessage);
            var fod = _logins.FirstOrDefault(s => s.Username == credMessage.Username);
            if (fod == null || (fod != null && fod.State == AuditState.Unknown))
            {
                fod = RaiseOnValidateUnknownLogin(credMessage, host, port);
            }

            return fod.State == AuditState.AccessAllowed && RaiseOnUserPasswordValidate(credMessage, fod);
        }

        internal LoginMessageEx GetUser(LoginMessage _calle)
        {
            return this._logins.FirstOrDefault(s => s.Username == _calle.Username);
        }
    }
}
