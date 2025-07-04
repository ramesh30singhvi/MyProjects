using System.Net;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IFEClientService
    {
        public void Trust(string userAgent, IPAddress ip, int userId);

        public bool IsTrusted(string userAgent, IPAddress ip, int userId);
    }
}
