using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public interface ISaIntegrationPartnerViewModel
    {
        Task<IntegrationCategoryListResponse> GetIntegrationCategoryListAsync();
        Task<AddUpdateIntegrationPartnerResponse> AddUpdateIntegrationPartner(IntegrationPartnerRequestModel model);
        Task<IntegrationPartnerResponse> GetIntegrationPartnerDetail(Guid partnerId);
        Task<IntegrationPartnerListResponse> GetIntegrationPartnerListAsync(bool? active = null);
        Task<IntegrationPartnerListResponse> DeleteIntegrationPartnerAsync(Guid partnerGUID);
        Task<IntegrationPartnerCategoryListResponse> GetIntegrationPartnerCategoryListAsync(bool? isActivePartner = null);
    }
}
