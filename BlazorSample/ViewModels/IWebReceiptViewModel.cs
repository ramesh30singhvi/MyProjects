using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IWebReceiptViewModel
    {
        Task<BusinessSettingsResponse> CreateWebReceiptLogoAsync(BusinessSettingsRequestModel model);
        Task<WebReceiptViewModel> GetWebReceiptAsync(int businessId);
        Task<WebReceiptViewModel> CreateWebReceiptAsync(WebReceiptViewModel model);
    }
}
