using System.Net;
using SHARP.BusinessLogic.Interfaces;
using SHARP.DAL;
using SHARP.DAL.Models;

namespace SHARP.BusinessLogic.Services
{
    public class FEClientService : IFEClientService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FEClientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool IsTrusted(string userAgent, IPAddress ip, int userId)
        {
            var ipString = ip.ToString();

            return _unitOfWork.TrsutedFEClientRespository.Any(client => client.UserAgent == userAgent && client.IP == ipString && client.UserID == userId);
        }

        public void Trust(string userAgent, IPAddress ip, int userId)
        {
            var client = new TrustedFEClient
            {
                UserAgent = userAgent,
                IP = ip.ToString(),
                UserID = userId
            };

            _unitOfWork.TrsutedFEClientRespository.Add(client);

            _unitOfWork.SaveChanges();
        }
    }
}
