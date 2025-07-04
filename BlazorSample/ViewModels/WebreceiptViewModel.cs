using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class WebreceiptViewModel : IWebReceiptViewModel
    {
        private IWebReceiptService _webReceiptService;
        public WebreceiptViewModel(IWebReceiptService webReceiptService)
        {
            _webReceiptService = webReceiptService;
        }
        public async Task<WebReceiptViewModel> CreateWebReceiptAsync(WebReceiptViewModel model)
        {
            return await _webReceiptService.CreateWebReceiptAsync(model);
        }

        public async Task<BusinessSettingsResponse> CreateWebReceiptLogoAsync(BusinessSettingsRequestModel model)
        {
            return await _webReceiptService.CreateWebReceiptLogoAsync(model);
        }

        public async Task<WebReceiptViewModel> GetWebReceiptAsync(int businessId)
        {
            return await _webReceiptService.GetWebReceiptAsync(businessId);
        }
    }
}
