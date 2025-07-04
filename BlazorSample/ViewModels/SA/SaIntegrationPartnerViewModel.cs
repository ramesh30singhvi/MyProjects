using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public class SaIntegrationPartnerViewModel : ISaIntegrationPartnerViewModel
    {
        private readonly ISaIntegrationPartnerService _saIntegrationPartnerService;
        public SaIntegrationPartnerViewModel(ISaIntegrationPartnerService saIntegrationPartnerService)
        {
            _saIntegrationPartnerService = saIntegrationPartnerService;
        }
        public async Task<IntegrationCategoryListResponse> GetIntegrationCategoryListAsync()
        {
            return await _saIntegrationPartnerService.GetIntegrationCategoryListAsync();
        }
        public async Task<AddUpdateIntegrationPartnerResponse> AddUpdateIntegrationPartner(IntegrationPartnerRequestModel model)
        {
            return await _saIntegrationPartnerService.AddUpdateIntegrationPartner(model);
        }
        public async Task<IntegrationPartnerResponse> GetIntegrationPartnerDetail(Guid partnerId)
        {
            return await _saIntegrationPartnerService.GetIntegrationPartnerDetail(partnerId);
        }
        public async Task<IntegrationPartnerListResponse> GetIntegrationPartnerListAsync(bool? active = null)
        {
            return await _saIntegrationPartnerService.GetIntegrationPartnerListAsync(active);
        }
        public async Task<IntegrationPartnerListResponse> DeleteIntegrationPartnerAsync(Guid partnerGUID)
        {
            return await _saIntegrationPartnerService.DeleteIntegrationPartnerAsync(partnerGUID);
        }
        public async Task<IntegrationPartnerCategoryListResponse> GetIntegrationPartnerCategoryListAsync(bool? isActivePartner = null)
        {
            return await _saIntegrationPartnerService.GetIntegrationPartnerCategoryListAsync(isActivePartner);
        }
    }
}
