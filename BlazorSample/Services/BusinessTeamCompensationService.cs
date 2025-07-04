using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class BusinessTeamCompensationService : IBusinessTeamCompensationService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public BusinessTeamCompensationService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<BusinessTeamCompensationListResponse> GetBusinessTeamCompensationListAsync(int businessTeamId)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessTeamCompensationListResponse>(_configuration["App:PeopleApiUrl"] + "BusinessTeamCompensation/get-business-team-compensation/" + businessTeamId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTeamCompensationListResponse();
            }
        }
        public async Task<BusinessTeamCompensationResponse> AddUpdateBusinessTeamCompensationListAsync(BusinessTeamCompensationRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessTeamCompensationResponse>(_configuration["App:PeopleApiUrl"] + "BusinessTeamCompensation/add-update-business-team-compensation", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTeamCompensationResponse();
            }
        }
    }
}
