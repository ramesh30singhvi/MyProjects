using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.Marketplace.ShippoProvider;
using CellarPassAppAdmin.Shared.Services.Marketplace;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services.MarketPlace
{
    public class ShippoService : IShippoService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public ShippoService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<bool> ShippoAuthorize(string code, string businessId)
        {
            try
            {
                var response = await _apiManager.GetAsync<BaseResponse>(_configuration["App:SettingsApiUrl"] + string.Format("Shippo/shippo-authorize/{0}/{1}",code,businessId));
                return true;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return false;
            }
        }
    }
}
