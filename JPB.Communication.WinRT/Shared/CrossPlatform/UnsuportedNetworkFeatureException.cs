using System;

namespace JPB.Communication.WinRT.Shared.CrossPlatform
{
    public class UnsuportedNetworkFeatureException : Exception
    {
        public UnsuportedNetworkFeatureException() { }
        public UnsuportedNetworkFeatureException(string message) : base(message) { }
        public UnsuportedNetworkFeatureException(string message, Exception inner) : base(message, inner) { }
    }
}
