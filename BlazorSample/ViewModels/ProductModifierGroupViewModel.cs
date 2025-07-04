using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class ProductModifierGroupViewModel : IProductModifierGroupViewModel
    {
        private IProductModifierGroupService _productModifierGroupService;

        public ProductModifierGroupViewModel(IProductModifierGroupService productModifierGroupService)
        {
            _productModifierGroupService = productModifierGroupService;
        }
        public async Task<ProductModifierGroupListResponse> GetProductModifierGroupListAsync(int? productId)
        {
            return await _productModifierGroupService.GetProductModifierGroupListAsync(productId);
        }        
        public async Task<ProductModifierGroupListResponse> AddUpdateProductModifierGroupAsync(ProductModifierGroupRequestModel model)
        {
            return await _productModifierGroupService.AddUpdateProductModifierGroupAsync(model);
        }

        public async Task<ProductModifierGroupItemListResponse> GetProductModifierGroupItemListAsync(int? productModifierGroupId, int? businessModifierGroupid)
        {
            return await _productModifierGroupService.GetProductModifierGroupItemListAsync(productModifierGroupId, businessModifierGroupid);
        }
        public async Task<ProductModifierGroupItemListResponse> AddUpdateProductModifierGroupItemAsync(ProductModifierGroupItemRequestModel model)
        {
            return await _productModifierGroupService.AddUpdateProductModifierGroupItemAsync(model);
        }
        public async Task<ProductModifierGroupListResponse> DeleteProductModifierGroupByIdAsync(int id)
        {
            return await _productModifierGroupService.DeleteProductModifierGroupByIdAsync(id);
        }
    }
}
