using System.Threading.Tasks;
using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.Contracts.Factorys
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