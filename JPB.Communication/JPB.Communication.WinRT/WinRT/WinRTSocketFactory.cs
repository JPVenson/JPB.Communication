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
            var sock = WinRtSocket.Create();
            sock.Wait();
            sock.Result.Connect(ipOrHost, port);
            return sock.Result;
        }

        public async Task<ISocket> CreateAndConnectAsync(string ipOrHost, ushort port)
        {
            var sock = await WinRtSocket.Create();
            await sock.ConnectAsync(ipOrHost, port);
            return sock;
        }

        public ISocket Create()
        {
            var sock = WinRtSocket.Create();
            sock.Wait();
            return sock.Result;
        }

        public async Task<ISocket> CreateAsync()
        {
            return await WinRtSocket.Create();
        }
    }
}
