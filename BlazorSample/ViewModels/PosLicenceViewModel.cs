using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class PosLicenceViewModel : IPOSLicenseViewModel
    {
        private IPOSLicenceService _POSLicenceService;
        public PosLicenceViewModel(IPOSLicenceService pOSLicenceService)
        {
            _POSLicenceService = pOSLicenceService;
        }

        public async Task<POSLicenceAddResponse> ActivatePosLicenceKey(int id, POSLicenceActivateRequest pOSLicenceActivateRequest)
        {
            return await _POSLicenceService.ActivatePosLicenceKey(id, pOSLicenceActivateRequest);
        }

        public async Task<POSLicenceAddResponse> AddPosLicenceKey(POSLicenceAddRequest pOSLicenceViewModel)
        {
            return await _POSLicenceService.AddPosLicenceKey(pOSLicenceViewModel);
        }

        public async Task<GridPaginatedResponseModel<POSLicenceViewModel>> GetPosLicenseKeys(GridPaginatedRequestViewModel gridPaginatedRequestViewModel)
        {
            return await _POSLicenceService.GetPosLicenseKeys(gridPaginatedRequestViewModel);
        }

        public async Task<POSLicenceResponse> GetPosLicenseKeys(int businessId)
        {
            return await _POSLicenceService.GetPosLicenseKeys(businessId);
        }

        public async Task<POSLicenceAddResponse> ResetPosLicenceKey(POSLicenceResetRequest model)
        {
            return await _POSLicenceService.ResetPosLicenceKey(model);
        }

        public async Task<POSLicenceAddResponse> UpdatePosLicenceKey(int id, POSLicenceAddRequest pOSLicenceViewModel)
        {
            return await _POSLicenceService.UpdatePosLicenceKey(id, pOSLicenceViewModel);
        }

        public async Task<POSLicenceViewModel> VerifyPosLicenceKey(POSLicenceVerifyRequest pOSLicenceViewModel)
        {
            return await _POSLicenceService.VerifyPosLicenceKey(pOSLicenceViewModel);
        }
    }
}
