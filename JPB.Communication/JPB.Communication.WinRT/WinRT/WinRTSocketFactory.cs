using JPB.Communication.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.WinRT.WinRT
{
    public class WinRtSocketFactory : ISocketFactory
    {
        public ISocket CreateAndConnect(string ipOrHost, ushort port)
        {
            var sock = new WinRtSocket();
            sock.Connect(ipOrHost, port);
            return sock;
        }

        public async Task<ISocket> CreateAndConnectAsync(string ipOrHost, ushort port)
        {
            var sock = new WinRtSocket();
            await sock.ConnectAsync(ipOrHost, port);
            return sock;
        }

        public ISocket Create()
        {
            return new WinRtSocket();
        }

        public async Task<ISocket> CreateAsync()
        {
            return new WinRtSocket();
        }
    }
}
