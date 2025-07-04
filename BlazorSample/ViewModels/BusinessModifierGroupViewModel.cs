using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessModifierGroupViewModel : IBusinessModifierGroupViewModel
    {
        private IBusinessModifierGroupService _businessModifierGroupService;

        public BusinessModifierGroupViewModel(IBusinessModifierGroupService businessModifierGroupService)
        {
            _businessModifierGroupService = businessModifierGroupService;
        }
        public async Task<BusinessModifierGroupListResponse> GetBusinessModifierGroupListAsync()
        {
            return await _businessModifierGroupService.GetBusinessModifierGroupListAsync();
        }
        public async Task<BusinessModifierGroupResponse> GetBusinessModifierGroupAsync(int? id, Guid? idGUID)
        {
            return await _businessModifierGroupService.GetBusinessModifierGroupAsync(id, idGUID);
        }
        public async Task<BusinessModifierGroupResponse> AddUpdateBusinessModifierGroupAsync(BusinessModifierGroupRequestModel model)
        {
            return await _businessModifierGroupService.AddUpdateBusinessModifierGroupAsync(model);
        }
        public async Task<BusinessModifierGroupListResponse> DeleteBusinessModifierGroupByIdAsync(int id)
        {
            return await _businessModifierGroupService.DeleteBusinessModifierGroupByIdAsync(id);
        }
        public async Task<BusinessModifierGroupItemListResponse> GetBusinessModifierGroupItemListAsync(int businessModifierGroupId)
        {
            return await _businessModifierGroupService.GetBusinessModifierGroupItemListAsync(businessModifierGroupId);
        }
    }
}
