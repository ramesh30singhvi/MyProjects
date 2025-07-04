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
    public class BusinessPropertyViewModel : IBusinessPropertyViewModel
    {
        private IBusinessPropertyService _businessPropertyService;

        public BusinessPropertyViewModel(IBusinessPropertyService businessPropertyService)
        {
            _businessPropertyService = businessPropertyService;
        }
        public async Task<BusinessPropertySettingsResponse> GetBusinessPropertySettingsAsync(int businessPropertyId, string metaNamespace, string metaKey)
        {
            return await _businessPropertyService.GetBusinessPropertySettingsAsync(businessPropertyId, metaNamespace, metaKey);
        }
        public async Task<BusinessPropertySettingsListResponse> GetBusinessPropertySettingsListAsync(int businessPropertyId, string metaNamespace)
        {
            return await _businessPropertyService.GetBusinessPropertySettingsListAsync(businessPropertyId, metaNamespace);
        }
        public async Task<List<BusinessPropertyModel>> GetBusinessProperties(int businessId)
        {
            return await _businessPropertyService.GetBusinessProperties(businessId);
        }
        public async Task<BusinessPropertySettingsResponse> CreateBusinessPropertySettingsAsync(BusinessPropertySettingsRequestModel model)
        {
            return await _businessPropertyService.CreateBusinessPropertySettingsAsync(model);
        }
        public async Task<BusinessPropertySettingsListResponse> CreateBusinessPropertySettingsListAsync(List<BusinessPropertySettingsRequestModel> models)
        {
            return await _businessPropertyService.CreateBusinessPropertySettingsListAsync(models);
        }

        
        public async Task<BusinessProfileViewModel> CreateBusinessPropertyAsync(BusinessProfileViewModel businessProfileDetails)
        {
            return await _businessPropertyService.CreateBusinessPropertyAsync(businessProfileDetails);
        }

        public async Task<BusinessProfileViewModel> GetBusinessProfilesDetails(Guid propertyGuid, string metanamespace)
        {
            return await _businessPropertyService.GetBusinessProfilesDetails(propertyGuid, metanamespace);
        }

        public async Task<List<BusinessPropertyModel>> DeleteBusinessProperty(int businessId, int id)
        {
            return await _businessPropertyService.DeleteBusinessProperty(businessId, id);
        }
    }
}
