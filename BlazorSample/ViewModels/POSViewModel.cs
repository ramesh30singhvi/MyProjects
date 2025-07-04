using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class POSViewModel : IPOSViewModel
    {
        private IPOSService _posService;
        public POSViewModel(IPOSService posService)
        {
            _posService = posService;
        }

        public async Task<POSReceiptSettingResponse> GetPOSReceiptSetting(int memberId)
        {
                POSReceiptSettingResponse receiptSetting = await _posService.GetPOSReceiptSettingAsync(memberId);
                return receiptSetting;
        }

        public async Task<POSReceiptSettingResponse> UpdatePOSReceiptSetting(POSReceiptViewModel model)
        {
            POSReceiptSettingResponse result = await _posService.UpdatePOSReceiptSettingAsync(model);
            return result;
        }

        public async Task<UploadImageResponse> UploadReceiptLogo(ImageUploadRequestModel model)
        {
            UploadImageResponse result = await _posService.UploadReceiptLogoAsync(model);
            return result;
        }
    }
}
