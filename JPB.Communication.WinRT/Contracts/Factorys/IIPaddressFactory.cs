using JPB.Communication.WinRT.Contracts.Intigration;

namespace JPB.Communication.WinRT.Contracts.Factorys
{
    public interface IIPaddressFactory
    {
        bool TryParse(string hostOrIp, out IPAddress ipAddress);
        IPAddress Parse(string p);
    }
}
