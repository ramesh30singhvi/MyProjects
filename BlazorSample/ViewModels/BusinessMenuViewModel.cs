using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessMenuViewModel : IBusinessMenuViewModel
    {
        private IBusinessMenuService _businessMenuService;

        public BusinessMenuViewModel(IBusinessMenuService businessMenuService)
        {
            _businessMenuService = businessMenuService;
        }
        public async Task<BusinessMenuListResponse> GetBusinessMenuListAsync()
        {
            return await _businessMenuService.GetBusinessMenuListAsync();
        }
        public async Task<BusinessMenuResponse> GetBusinessMenuAsync(int? id, Guid? idGUID)
        {
            return await _businessMenuService.GetBusinessMenuAsync(id, idGUID);
        }
        public async Task<BusinessMenuResponse> AddUpdateBusinessMenuAsync(BusinessMenuRequestModel model)
        {
            return await _businessMenuService.AddUpdateBusinessMenuAsync(model);
        }
        public async Task<BusinessMenuListResponse> DeleteBusinessMenuByIdAsync(int id)
        {
            return await _businessMenuService.DeleteBusinessMenuByIdAsync(id);
        }
        public async Task<BusinessMenuGroupListResponse> GetBusinessMenuGroupListAsync(int businessMenuId)
        {
            return await _businessMenuService.GetBusinessMenuGroupListAsync(businessMenuId);
        }
        public async Task<BusinessMenuGroupResponse> GetBusinessMenuGroupAsync(int? id, Guid? idGUID)
        {
            return await _businessMenuService.GetBusinessMenuGroupAsync(id, idGUID);
        }
        public async Task<BusinessMenuGroupResponse> AddUpdateBusinessMenuGroupAsync(BusinessMenuGroupRequestModel model)
        {
            return await _businessMenuService.AddUpdateBusinessMenuGroupAsync(model);
        }
        public async Task<BusinessMenuGroupListResponse> DeleteBusinessMenuGroupByIdAsync(int id)
        {
            return await _businessMenuService.DeleteBusinessMenuGroupByIdAsync(id);
        }
        public async Task<BusinessMenuGroupListResponse> ArrangeBusinessMenuGroupAsync(List<ArrangeBusinessMenuGroupRequestModel> models)
        {
            return await _businessMenuService.ArrangeBusinessMenuGroupAsync(models);
        }
        public async Task<BusinessMenuGroupItemListResponse> GetBusinessMenuGroupItemListAsync(int? businessMenuGroupId, int? productId)
        {
            return await _businessMenuService.GetBusinessMenuGroupItemListAsync(businessMenuGroupId, productId);
        }
        public async Task<BusinessMenuGroupItemResponse> GetBusinessMenuGroupItemAsync(int? id)
        {
            return await _businessMenuService.GetBusinessMenuGroupItemAsync(id);
        }
        public async Task<BusinessMenuGroupItemResponse> AddUpdateBusinessMenuGroupItemAsync(BusinessMenuGroupItemRequestModel model)
        {
            return await _businessMenuService.AddUpdateBusinessMenuGroupItemAsync(model);
        }
        public async Task<BusinessMenuGroupItemListResponse> DeleteBusinessMenuGroupItemByIdAsync(int id)
        {
            return await _businessMenuService.DeleteBusinessMenuGroupItemByIdAsync(id);
        }
        public async Task<BusinessMenuGroupItemListResponse> ArrangeBusinessMenuGroupItemAsync(List<BusinessMenuGroupItemRequestModel> models)
        {
            return await _businessMenuService.ArrangeBusinessMenuGroupItemAsync(models);
        }
        public async Task<BusinessMenuGroupItemListResponse> AddUpdateBusinessMenuGroupItemsAsync(BusinessMenuGroupProductRequestModel model)
        {
            return await _businessMenuService.AddUpdateBusinessMenuGroupItemsAsync(model);
        }
    }
}
