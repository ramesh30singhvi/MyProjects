using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class BusinessPositionNameService : IBusinessPositionNameService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public BusinessPositionNameService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<BusinessPositionNameListResponse> GetBusinessPositionNameListAsync(int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessPositionNameListResponse>(_configuration["App:PeopleApiUrl"] + "BusinessPositionName/get-business-position-name/" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessPositionNameListResponse();
            }
        }
        public async Task<BusinessPositionNameResponse> AddUpdateBusinessPositionNameListAsync(BusinessPositionNameRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessPositionNameResponse>(_configuration["App:PeopleApiUrl"] + "BusinessPositionName/add-update-business-position-name", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessPositionNameResponse();
            }
        }
    }
}
