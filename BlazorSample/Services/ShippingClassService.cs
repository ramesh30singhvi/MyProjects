using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
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
    public class ShippingClassService : IShippingClassService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public ShippingClassService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<ShippingClassResponse> AddUpdateShippingClassAsync(ShippingClassRequestModel model)
        {
            try
            {
                var result = await _apiManager.PostAsync<ShippingClassRequestModel, ShippingClassResponse>(_configuration["App:SettingsApiUrl"] + "ShippingClass", model);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ShippingClassResponse();
            }
        }

        public async Task<BaseResponse> DeleteShippingClass(int Id)
        {
            try
            {
               return  await _apiManager.DeleteAsync<BaseResponse>(string.Format("{0}ShippingClass/{1}", _configuration["App:SettingsApiUrl"], Id));
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new BaseResponse();
            }
        }

        public Task<GridPaginatedResponseModel<ShippingClassModel>> GetShippingClasses
        (GridPaginatedRequestViewModel gridPaginatedRequestViewModel) => throw new NotImplementedException();

        public async Task<ShippingClassListResponse> GetShippingClasses(int businessId)
        {
            try
            {
                var result = await _apiManager.GetAsync<ShippingClassListResponse>(_configuration["App:SettingsApiUrl"] + "ShippingClass/get-all-shipping-classes/"+businessId);
                return result;
            }catch(HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new ShippingClassListResponse();
            }
        }
    }
}
