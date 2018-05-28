/*
 Created by Jean-Pierre Bachmann
 Visit my GitHub page at:
 
 https://github.com/JPVenson/

 Please respect the Code and Work of other Programers an Read the license carefully

 GNU AFFERO GENERAL PUBLIC LICENSE
                       Version 3, 19 November 2007

 Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>
 Everyone is permitted to copy and distribute verbatim copies
 of this license document, but changing it is not allowed.

 READ THE FULL LICENSE AT:

 https://github.com/JPVenson/JPB.Communication/blob/master/LICENSE
 */

using System;
using System.IO;
using System.Linq;
using System.Text;
using JPB.Communication.WinRT.combase.Messages;
using JPB.Communication.WinRT.combase.Messages.Wrapper;
using JPB.Communication.WinRT.combase.Security;
using JPB.Communication.WinRT.Contracts;
using JPB.Communication.WinRT.Contracts.Intigration;
using JPB.Communication.WinRT.Contracts.Security;
using JPB.Communication.WinRT.Shared.CrossPlatform;

namespace JPB.Communication.WinRT.combase
{
	/// <summary>
	///     The base class for Multible Network instances
	/// </summary>
	public abstract class Networkbase
	{
		public const string TraceCategoryCriticalSerilization = "JPB.Communication.Critical.Serilization";
		public const string TraceCategoryLowSerilization = "JPB.Communication.Low.Serilization";

		/// <summary>
		///     The default Serializer
		/// </summary>
		public static IMessageSerializer DefaultMessageSerializer;

		private IMessageSerializer _serlilizer;

		static Networkbase()
		{

		}

		protected byte[] ApplyHeader(MessageMeta message)
		{
			return message.ToHeader();
		}

		/// <summary>
		/// </summary>
		protected Networkbase()
		{
			Serlilizer = DefaultMessageSerializer;
		}

		internal LoginMessage _assosciatedLogin;

		/// <summary>
		///     Defines the Port the Instance is working on
		/// </summary>
		public abstract ushort Port { get; internal set; }

		public static bool CheckSocketSharedState(ISocket sock)
		{
			switch (sock.SupportsSharedState)
			{
				case Contracts.Factorys.SharedStateSupport.Full:
					return true;
				case Contracts.Factorys.SharedStateSupport.PartialCheck:
					return sock.CheckSharedStateSupport();
				case Contracts.Factorys.SharedStateSupport.Non:
					return false;
				default:
					return false;
			}
		}

		public static void CheckSocketSharedStateThrow(ISocket sock)
		{
			var check = CheckSocketSharedState(sock);
			if (!check)
			{
				throw new UnsuportedNetworkFeatureException(string.Format("The socket of type {0} does not support the feature of SharedSockets", sock.GetType()));
			}
		}

		public ISecureMessageProvider Security { get; set; }

		/// <summary>
		///     When Serlilizaion is request this Interface will be used
		/// </summary>
		public IMessageSerializer Serlilizer
		{
			get
			{
				if (_serlilizer == null)
				{
					throw new Exception("Please define an Default Serializer");
				}

				return _serlilizer;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value", "Value cannot be null");
				}
				_serlilizer = value;
			}
		}

		public static event MessageDelegate OnNewItemLoadedSuccess;
		public static event EventHandler<string> OnNewItemLoadedFail;
		public static event EventHandler<MessageBase> OnIncommingMessage;
		public static event MessageDelegate OnMessageSend;
		public static event LargeMessageDelegate OnNewLargeItemLoadedSuccess;

		protected EventHandler<MessageBase> IncommingMessageHandler()
		{
			return OnIncommingMessage;
		}

		/// <summary>
		/// </summary>
		/// <param name="metaData"></param>
		/// <param name="contendLoaded"></param>
		protected virtual LargeMessage RaiseNewLargeItemLoadedSuccess(object metaData)
		{
			try
			{
				LargeMessageDelegate handler = OnNewLargeItemLoadedSuccess;
				var largeMessage = new LargeMessage(metaData as StreamMetaMessage, null);
				if (handler != null)
				{
					handler(largeMessage, Port);
				}

				if (largeMessage.InfoLoaded == null)
				{
					largeMessage.InfoLoaded = new StreamBuffer();
				}

				return largeMessage;
			}
			catch (Exception e)
			{
				PclTrace.WriteLine(string.Format("> Networkbase> RaiseNewLargeItemLoadedSuccess>{0}", e), TraceCategoryCriticalSerilization);
				return null;
			}
		}

		protected virtual void RaiseMessageSend(object message)
		{
			try
			{
				MessageDelegate handler = OnMessageSend;
				if (handler != null)
				{
					handler(message, Port, null);
				}
			}
			catch (Exception e)
			{
				PclTrace.WriteLine(string.Format("> Networkbase> RaiseMessageSended>{0}", e), TraceCategoryCriticalSerilization);
			}
		}

		protected virtual void RaiseIncommingMessage(MessageBase strReceived)
		{
			try
			{
				var handler = OnIncommingMessage;
				if (handler != null)
				{
					handler(this, strReceived);
				}
			}
			catch (Exception e)
			{
				PclTrace.WriteLine(string.Format("> Networkbase> RaiseIncommingMessage>{0}", e), TraceCategoryCriticalSerilization);
			}
		}

		protected virtual void RaiseNewItemLoadedFail(string strReceived)
		{
			try
			{
				var handler = OnNewItemLoadedFail;
				if (handler != null)
				{
					handler(this, strReceived);
				}
			}
			catch (Exception e)
			{
				PclTrace.WriteLine(string.Format("> Networkbase> RaiseNewItemLoadedFail>{0}", e), TraceCategoryCriticalSerilization);
			}
		}

		protected virtual void RaiseNewItemLoadedSuccess(object loadMessageBaseFromBinary, MessageBaseInfo info)
		{
			try
			{
				MessageDelegate handler = OnNewItemLoadedSuccess;
				if (handler != null)
				{
					handler(loadMessageBaseFromBinary, Port, info);
				}
			}
			catch (Exception e)
			{
				PclTrace.WriteLine(string.Format("> Networkbase> RaiseNewItemLoadedSuccess>{0}", e), TraceCategoryCriticalSerilization);
			}
		}


		public object DeSerialize(byte[] source)
		{
			try
			{
				var sor = source;
				if (Security != null)
				{
					sor = Security.Decrypt(sor);
				}
				return Serlilizer.DeSerializeMessage(sor);
			}
			catch (Exception e)
			{
				PclTrace.WriteLine(e.ToString(), TraceCategoryLowSerilization);
				return null;
			}
		}

		public byte[] Serialize(object networkMessage)
		{
			try
			{
				var sor = Serlilizer.SerializeMessage(networkMessage);
				if (Security != null)
				{
					sor = Security.Encrypt(sor);
				}
				return sor;
			}
			catch (Exception e)
			{
				PclTrace.WriteLine(e.ToString(), TraceCategoryLowSerilization);
				return new byte[0];
			}
		}

		private Encoding PasswordEncoding = Encoding.Unicode;

		internal LoginMessage DeSerializeLogin(byte[] maybeLoginMessage)
		{
			var message = maybeLoginMessage;
			if (Security != null)
			{
				message = Security.Decrypt(maybeLoginMessage);
			}

			var passwordEncoded = message
				.Take(NetworkAuthentificator.PasswordBufferSize)
				.ToArray();
			var usernameEncoded = message
				.Skip(NetworkAuthentificator.PasswordBufferSize)
				.Take(NetworkAuthentificator.NameBufferSize)
				.ToArray();

			var sessionEncoded = message
				.Skip(NetworkAuthentificator.NameBufferSize)
				.Skip(NetworkAuthentificator.PasswordBufferSize)
				.Take(NetworkAuthentificator.SessionBufferSize)
				.ToArray();

			var passwordPlain = PasswordEncoding.GetString(passwordEncoded, 0, passwordEncoded.Length).Replace("\0", string.Empty).Trim();
			var usernamePlain = PasswordEncoding.GetString(usernameEncoded, 0, usernameEncoded.Length).Replace("\0", string.Empty).Trim();
			var sessionPlain = PasswordEncoding.GetString(sessionEncoded, 0, sessionEncoded.Length).Replace("\0", string.Empty).Trim();

			return _assosciatedLogin = new LoginMessage()
			{
				Password = passwordPlain,
				Username = usernamePlain,
				SessionID = sessionPlain
			};
		}

		internal byte[] SerializeLogin(LoginMessage mess)
		{
			mess.SessionID = Guid.NewGuid().ToString();

			var passwordPlain = PasswordEncoding.GetBytes(mess.Password);
			var usernamePlain = PasswordEncoding.GetBytes(mess.Username);
			var sessionPlain = PasswordEncoding.GetBytes(mess.SessionID);

			if (passwordPlain.Length > NetworkAuthentificator.PasswordBufferSize)
			{
				throw new ArgumentException("The password must be smaller then the maximum Password buffer size", "mess.Password");
			}

			if (usernamePlain.Length > NetworkAuthentificator.NameBufferSize)
			{
				throw new ArgumentException("The Username must be smaller then the maximum Username buffer size", "mess.Username");
			}

			if (sessionPlain.Length > NetworkAuthentificator.SessionBufferSize)
			{
				throw new ArgumentException("The Session ID must be smaller then the maximum Session ID buffer size", "mess.SessionID");
			}

			var passwordEncoded = new byte[NetworkAuthentificator.PasswordBufferSize];
			var usernameEncoded = new byte[NetworkAuthentificator.NameBufferSize];
			var sessionEncoded = new byte[NetworkAuthentificator.SessionBufferSize];

			passwordPlain.CopyTo(passwordEncoded, 0);
			usernamePlain.CopyTo(usernameEncoded, 0);
			sessionPlain.CopyTo(sessionEncoded, 0);

			var credData = passwordEncoded
				.Concat(usernameEncoded)
				.Concat(sessionEncoded)
				.ToArray();

			if (credData.Length != (NetworkAuthentificator.PasswordBufferSize + NetworkAuthentificator.SessionBufferSize + NetworkAuthentificator.NameBufferSize))
			{
				throw new Exception("Internal Error. The Cred data array has not the expected size");
			}

			if (Security != null)
			{
				credData = Security.Encrypt(credData);
			}

			return credData;
		}
	}
}