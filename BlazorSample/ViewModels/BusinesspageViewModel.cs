using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinesspageViewModel : IBusinessPageViewModel
    {
        private IBusinessPageService _businessPageService;
        public BusinesspageViewModel(IBusinessPageService businessPageService)
        {
            _businessPageService = businessPageService;
        }
        public async Task<List<CP_Attribute>> GetAttributeMastersAsync()
        {
            return await _businessPageService.GetAttributeMastersAsync();
        }

        public async Task<BusinessPageViewModel> GetBusinessContent(int businessId, int businessProperty)
        {
            return await _businessPageService.GetBusinessContent(businessId, businessProperty);
        }

        public async Task<BusinessPageViewModel> SaveBusinessContent(BusinessPageViewModel businessPageViewModel)
        {
            return await _businessPageService.SaveBusinessContent(businessPageViewModel);
        }

        public async Task<UploadImageResponse> UpdateBusinessBannerImage(ImageUploadRequestModel model)
        {
            return await _businessPageService.UpdateBusinessBannerImage(model);
        }

        public async Task<BusinessPageViewModel> UpdateBusinessContent(BusinessPageViewModel businessPageViewModel)
        {
            return await _businessPageService.UpdateBusinessContent(businessPageViewModel);
        }

        public async Task<UploadImageResponse> UpdateBusinessImage(ImageUploadRequestModel model)
        {
            return await _businessPageService.UpdateBusinessImage(model);
        }
    }
}
