using System.Threading.Tasks;
using JPB.Communication.ComBase.TCP;

namespace JPB.Communication.Contracts
{
    /// <summary>
    /// </summary>
    public interface ISocketFactory
    {
        ISocket CreateAndConnect(string ipOrHost, ushort port);
        Task<ISocket> CreateAndConnectAsync(string ipOrHost, ushort port);

        ISocket Create();
        Task<ISocket> CreateAsync();
    }
}