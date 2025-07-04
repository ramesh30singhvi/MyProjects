using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IPOSLicenseViewModel
    {
        Task<GridPaginatedResponseModel<POSLicenceViewModel>> GetPosLicenseKeys(GridPaginatedRequestViewModel gridPaginatedRequestViewModel);
        Task<POSLicenceResponse> GetPosLicenseKeys(int businessId);
        Task<POSLicenceAddResponse> AddPosLicenceKey(POSLicenceAddRequest pOSLicenceViewModel);
        Task<POSLicenceAddResponse> UpdatePosLicenceKey(int id, POSLicenceAddRequest pOSLicenceViewModel);
        Task<POSLicenceAddResponse> ActivatePosLicenceKey(int id, POSLicenceActivateRequest pOSLicenceActivateRequest);
        Task<POSLicenceViewModel> VerifyPosLicenceKey(POSLicenceVerifyRequest pOSLicenceViewModel);
        Task<POSLicenceAddResponse> ResetPosLicenceKey(POSLicenceResetRequest model);
    }
}
