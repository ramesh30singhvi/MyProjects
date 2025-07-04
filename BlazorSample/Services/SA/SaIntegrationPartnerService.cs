using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services.SA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services.SA
{
    public class SaIntegrationPartnerService : ISaIntegrationPartnerService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public SaIntegrationPartnerService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<IntegrationCategoryListResponse> GetIntegrationCategoryListAsync()
        {
            try
            {
                return await _apiManager.GetAsync<IntegrationCategoryListResponse>("SaIntegrationPartner/get-integration-category-list");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new IntegrationCategoryListResponse();
            }
        }
        public async Task<AddUpdateIntegrationPartnerResponse> AddUpdateIntegrationPartner(IntegrationPartnerRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<AddUpdateIntegrationPartnerResponse>("SaIntegrationPartner/add-update-integration-partner", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdateIntegrationPartnerResponse();
            }
        }
        public async Task<IntegrationPartnerResponse> GetIntegrationPartnerDetail(Guid partnerId)
        {
            try
            {
                return await _apiManager.GetAsync<IntegrationPartnerResponse>("SaIntegrationPartner/get-integration-partner-detail/" + partnerId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new IntegrationPartnerResponse();
            }
        }
        public async Task<IntegrationPartnerListResponse> GetIntegrationPartnerListAsync(bool? active = null)
        {
            try
            {
                return await _apiManager.GetAsync<IntegrationPartnerListResponse>("SaIntegrationPartner/get-integration-partner-list?active=" + active);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new IntegrationPartnerListResponse();
            }
        }
        public async Task<IntegrationPartnerListResponse> DeleteIntegrationPartnerAsync(Guid partnerGUID)
        {
            try
            {
                return await _apiManager.DeleteAsync<IntegrationPartnerListResponse>("SaIntegrationPartner/delete-integration-partner/" + partnerGUID);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new IntegrationPartnerListResponse();
            }
        }
        public async Task<IntegrationPartnerCategoryListResponse> GetIntegrationPartnerCategoryListAsync(bool? isActivePartner = null)
        {
            try
            {
                return await _apiManager.GetAsync<IntegrationPartnerCategoryListResponse>("SaIntegrationPartner/get-integration-partner-category-list?isActivePartner=" + isActivePartner);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new IntegrationPartnerCategoryListResponse();
            }
        }
    }
}
