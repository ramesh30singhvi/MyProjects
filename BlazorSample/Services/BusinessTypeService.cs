using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class BusinessTypeService : IBusinessTypeService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public BusinessTypeService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;

        }
        public async Task<List<BusinessTypeViewModel>> GetBusinessTypes()
        {
            try
            {
                var response = await _apiManager.GetAsync<BusinessTypesResponse>(_configuration["App:SettingsApiUrl"] + "BusinessType/get-business-types");
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<BusinessTypeViewModel>();
            }
        }
    }
}
