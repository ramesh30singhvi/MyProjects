using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IProductModifierGroupViewModel
    {
        Task<ProductModifierGroupListResponse> GetProductModifierGroupListAsync(int? productId);      
        Task<ProductModifierGroupListResponse> AddUpdateProductModifierGroupAsync(ProductModifierGroupRequestModel model);
        Task<ProductModifierGroupItemListResponse> GetProductModifierGroupItemListAsync(int? productModifierGroupId, int? businessModifierGroupid);
        Task<ProductModifierGroupItemListResponse> AddUpdateProductModifierGroupItemAsync(ProductModifierGroupItemRequestModel model);
        Task<ProductModifierGroupListResponse> DeleteProductModifierGroupByIdAsync(int id);
    }
}
