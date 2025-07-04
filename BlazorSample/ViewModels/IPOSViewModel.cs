using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IPOSViewModel
    {
        Task<POSReceiptSettingResponse> GetPOSReceiptSetting(int memberId);
        Task<POSReceiptSettingResponse> UpdatePOSReceiptSetting(POSReceiptViewModel model);
        Task<UploadImageResponse> UploadReceiptLogo(ImageUploadRequestModel model);
    }
}
