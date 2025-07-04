using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessModifierGroupViewModel
    {
        Task<BusinessModifierGroupListResponse> GetBusinessModifierGroupListAsync();
        Task<BusinessModifierGroupResponse> GetBusinessModifierGroupAsync(int? id, Guid? idGUID);
        Task<BusinessModifierGroupResponse> AddUpdateBusinessModifierGroupAsync(BusinessModifierGroupRequestModel model);
        Task<BusinessModifierGroupListResponse> DeleteBusinessModifierGroupByIdAsync(int id);
        Task<BusinessModifierGroupItemListResponse> GetBusinessModifierGroupItemListAsync(int businessModifierGroupId);
    }
}
