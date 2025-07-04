using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class POSService : IPOSService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public POSService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<POSReceiptSettingResponse> GetPOSReceiptSettingAsync(int businessId)
        {
            try
            {
                POSReceiptSettingResponse receiptSetting = await _apiManager.GetAsync<POSReceiptSettingResponse>(string.Format("{0}PosReceipt/{1}", _configuration["App:SettingsApiUrl"], businessId));
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return null;
            }
        }
        public async Task<POSReceiptSettingResponse> UpdatePOSReceiptSettingAsync(POSReceiptViewModel model)
        {
            try
            {
                POSReceiptSettingResponse result = await _apiManager.PostAsync<POSReceiptSettingResponse>(string.Format("{0}PosReceipt", _configuration["App:SettingsApiUrl"]), model);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return null;
            }
        }



        public async Task<UploadImageResponse> UploadReceiptLogoAsync(ImageUploadRequestModel model)
        {
            try
            {
                UploadImageResponse result = await _apiManager.PostAsync<ImageUploadRequestModel, UploadImageResponse>(string.Format("{0}PosReceipt/upload-receipt-logo", _configuration["App:SettingsApiUrl"]), model);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UploadImageResponse { success = false};
            }
        }
    }
}
