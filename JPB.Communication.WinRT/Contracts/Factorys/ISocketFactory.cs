using System.Threading.Tasks;
using JPB.Communication.WinRT.Contracts.Intigration;

namespace JPB.Communication.WinRT.Contracts.Factorys
{
    /// <summary>
    /// </summary>
    public interface ISocketFactory
    {
        ISocket CreateAndConnect(string ipOrHost, ushort port);
        Task<ISocket> CreateAndConnectAsync(string ipOrHost, ushort port);

        ISocket Create();
        Task<ISocket> CreateAsync();
        SharedStateSupport SupportsSharedState { get; }
    }
}