using JPB.Communication.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.WinPhone
{
    public class WinPhoneSocketFactory : ISocketFactory
    {
        public ISocket CreateAndConnect(string ipOrHost, ushort port)
        {
            var sock = new WinPhoneSocket();
            sock.Connect(ipOrHost, port);
            return sock;
        }

        public async Task<ISocket> CreateAndConnectAsync(string ipOrHost, ushort port)
        {
            var sock = new WinPhoneSocket();
            await sock.ConnectAsync(ipOrHost, port);
            return sock;
        }

        public ISocket Create()
        {
            throw new NotImplementedException();
        }

        public Task<ISocket> CreateAsync()
        {
            throw new NotImplementedException();
        }
    }
}
