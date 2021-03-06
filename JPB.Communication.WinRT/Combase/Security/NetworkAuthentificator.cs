﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JPB.Communication.WinRT.combase.Messages;

namespace JPB.Communication.WinRT.combase.Security
{
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
            _logins = new ConcurrentBag<LoginMessageEx>();
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

        /// <summary>
        /// Should the Authentificator provide a Cache ot suppress new Logins that provide the Same session state and from the same Origin
        /// </summary>
        public bool ShouldCacheResults { get; set; }

        private ConcurrentBag<LoginMessageEx> _logins;
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
            var fod = _logins.FirstOrDefault(s =>
                    s.Username == message.Username
                    && s.Host == host
                    && s.Password == message.Password
                    && s.SessionID == message.SessionID
                    );
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
                        fod = _logins.FirstOrDefault(s => s.Username == message.Username);
                        state = fod == null ? AuditState.AccessAllowed : AuditState.AccessDenyed;
                        if (fod != null)
                        {
	                        return fod;
                        }

	                    break;
                    default:
                        state = AuditState.AccessDenyed;
                        break;
                }
            }


            if(fod == null)
            {
                fod = new LoginMessageEx(host, port)
                {
                    SessionID = message.SessionID,
                    State = state,
                    Username = message.Username,
                    Password = message.Password,
                };
                _logins.Add(fod);
            }
          
            return fod;
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
            var fod = _logins.FirstOrDefault(s =>
                            s.Username == credMessage.Username 
                        &&  s.Host == host
                        &&  s.Password == credMessage.Password
                        &&  s.SessionID == credMessage.SessionID
            );
            if (ShouldCacheResults)
            {
                if (fod == null || (fod != null && fod.State == AuditState.Unknown))
                {
                    fod = RaiseOnValidateUnknownLogin(credMessage, host, port);
                }
            }
            else
            {
                fod = RaiseOnValidateUnknownLogin(credMessage, host, port);
            }

            if (fod.State == AuditState.AccessAllowed)
            {
	            return true;
            }

	        if (fod.State == AuditState.CheckPassword)
	        {
		        return RaiseOnUserPasswordValidate(credMessage, fod);
	        }

	        return false;
        }

        internal LoginMessageEx GetUser(LoginMessage _calle)
        {
            return this._logins.FirstOrDefault(s => s.Username == _calle.Username);
        }
    }
}
