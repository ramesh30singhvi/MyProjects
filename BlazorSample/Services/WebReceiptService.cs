using CellarPassAppAdmin.Client.Exceptions;
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
    public class WebReceiptService : IWebReceiptService
    {

        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public WebReceiptService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<WebReceiptViewModel> CreateWebReceiptAsync(WebReceiptViewModel model)
        {
            try
            {
                var response = await _apiManager.PostAsync<WebReceiptViewModel, WebReceiptResponse>(_configuration["App:SettingsApiUrl"] + "WebReceipt/add-update-webreceipt", model);
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new WebReceiptViewModel();
            }
        }

        public async Task<BusinessSettingsResponse> CreateWebReceiptLogoAsync(BusinessSettingsRequestModel model)
        {
            try
            {
                BusinessSettingsResponse response = await _apiManager.PostAsync<BusinessSettingsRequestModel, BusinessSettingsResponse>(_configuration["App:SettingsApiUrl"] + "WebReceipt/add-web-receipt-logo", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessSettingsResponse();
            }
        }

        public async Task<WebReceiptViewModel> GetWebReceiptAsync(int businessId)
        {
            try
            {
                var response = await _apiManager.GetAsync<WebReceiptResponse>(_configuration["App:SettingsApiUrl"] + $"WebReceipt/get-web-receipt-details/{businessId}");
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new WebReceiptViewModel();
            }
        }
    }
}
