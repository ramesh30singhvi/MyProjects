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
    public class PreferredShippingOptionsService : IPreferredShippingOptionsService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public PreferredShippingOptionsService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<GetPreferredShippingOptionsListResponse> GetPreferredShippingOptions(int businessId)
        {
            try
            {
                var result = await _apiManager.GetAsync<GetPreferredShippingOptionsListResponse>(_configuration["App:SettingsApiUrl"] + "PreferredShippingOptions/" + businessId);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetPreferredShippingOptionsListResponse();
            }
        }
    }
}
