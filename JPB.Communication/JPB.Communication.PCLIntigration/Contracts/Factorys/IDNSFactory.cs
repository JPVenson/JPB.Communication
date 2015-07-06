using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.Contracts.Factorys
{
    public interface IDNSFactory
    {
        string GetHostName();
        IPHostEntry GetHostEntry(string p);
        IPAddress[] GetHostAddresses(string host);
    }
}
