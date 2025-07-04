using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class BusinessTeamPositionService : IBusinessTeamPositionService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public BusinessTeamPositionService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<BusinessTeamPositionListResponse> GetBusinessTeamPositionListAsync(int businessTeamCompensationId)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessTeamPositionListResponse>(_configuration["App:PeopleApiUrl"] + "BusinessTeamPosition/get-business-team-position/" + businessTeamCompensationId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTeamPositionListResponse();
            }
        }
        public async Task<BusinessTeamPositionResponse> AddUpdateBusinessTeamPositionListAsync(BusinessTeamPositionRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessTeamPositionResponse>(_configuration["App:PeopleApiUrl"] + "BusinessTeamPosition/add-update-business-team-position", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTeamPositionResponse();
            }
        }
    }
}
