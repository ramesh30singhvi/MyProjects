using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessPropertyViewModel
    {
        Task<BusinessPropertySettingsResponse> GetBusinessPropertySettingsAsync(int businessPropertyId, string metaNamespace, string metaKey);
        Task<BusinessPropertySettingsListResponse> GetBusinessPropertySettingsListAsync(int businessPropertyId, string metaNamespace);
        Task<List<BusinessPropertyModel>> GetBusinessProperties(int businessId);
        Task<BusinessPropertySettingsResponse> CreateBusinessPropertySettingsAsync(BusinessPropertySettingsRequestModel model);
        Task<BusinessPropertySettingsListResponse> CreateBusinessPropertySettingsListAsync(List<BusinessPropertySettingsRequestModel> models);

        
        Task<BusinessProfileViewModel> CreateBusinessPropertyAsync(BusinessProfileViewModel businessProfileDetails);
        Task<BusinessProfileViewModel> GetBusinessProfilesDetails(Guid propertyGuid, string metanamespace);
        Task<List<BusinessPropertyModel>> DeleteBusinessProperty(int businessId, int id);
       
    }
}
