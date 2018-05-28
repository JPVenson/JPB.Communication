using JPB.Communication.WinRT.Contracts.Intigration;

namespace JPB.Communication.WinRT.Contracts.Factorys
{
    public interface IDNSFactory
    {
        string GetHostName();
        IPHostEntry GetHostEntry(string p);
        IPAddress[] GetHostAddresses(string host);
    }
}
