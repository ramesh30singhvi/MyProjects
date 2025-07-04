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
    public class OrderSettingsService : IOrderSettingsService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public OrderSettingsService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<CheckOrderStartingNumberResponse> CheckOrderCreatedByStartingNumber(int startingNumber)
        {
            try
            {
                CheckOrderStartingNumberResponse response = await _apiManager.GetAsync<CheckOrderStartingNumberResponse>(_configuration["App:SettingsApiUrl"] + $"OrderSettings/check-order-by-starting-number?startingNumber={startingNumber}");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CheckOrderStartingNumberResponse();
            }
        }
    }
}
