﻿using JPB.Communication.Contracts.Factorys;
using JPB.Communication.Contracts.Intigration;
using System.Threading.Tasks;

namespace JPB.Communication.NativeWin.WinRT
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

        public SharedStateSupport SupportsSharedState
        {
            get
            {
                return SharedStateSupport.Full;
            }
        }
    }
}
