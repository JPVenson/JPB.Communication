using JPB.Communication.Contracts.Intigration;

namespace JPB.Communication.Contracts.Factorys
{
    public interface IIPaddressFactory
    {
        bool TryParse(string hostOrIp, out IPAddress ipAddress);
        IPAddress Parse(string p);
    }
}
