using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.Shared.CrossPlatform
{
    public class UnsuportedNetworkFeatureException : Exception
    {
        public UnsuportedNetworkFeatureException() { }
        public UnsuportedNetworkFeatureException(string message) : base(message) { }
        public UnsuportedNetworkFeatureException(string message, Exception inner) : base(message, inner) { }
    }
}
