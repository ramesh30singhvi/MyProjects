using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessPageViewModel
    {
        Task<List<CP_Attribute>> GetAttributeMastersAsync();
        Task<BusinessPageViewModel> GetBusinessContent(int businessId, int businessProperty);
        Task<BusinessPageViewModel> SaveBusinessContent(BusinessPageViewModel businessPageViewModel);
        Task<BusinessPageViewModel> UpdateBusinessContent(BusinessPageViewModel businessPageViewModel);
        Task<UploadImageResponse> UpdateBusinessBannerImage(ImageUploadRequestModel model);
        Task<UploadImageResponse> UpdateBusinessImage(ImageUploadRequestModel model);
    }
}
