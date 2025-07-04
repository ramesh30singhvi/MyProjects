using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessMenuViewModel
    {
        Task<BusinessMenuListResponse> GetBusinessMenuListAsync();
        Task<BusinessMenuResponse> GetBusinessMenuAsync(int? id, Guid? idGUID);
        Task<BusinessMenuResponse> AddUpdateBusinessMenuAsync(BusinessMenuRequestModel model);
        Task<BusinessMenuListResponse> DeleteBusinessMenuByIdAsync(int id);
        Task<BusinessMenuGroupListResponse> GetBusinessMenuGroupListAsync(int businessMenuId);
        Task<BusinessMenuGroupResponse> GetBusinessMenuGroupAsync(int? id, Guid? idGUID);
        Task<BusinessMenuGroupResponse> AddUpdateBusinessMenuGroupAsync(BusinessMenuGroupRequestModel model);
        Task<BusinessMenuGroupListResponse> DeleteBusinessMenuGroupByIdAsync(int id);
        Task<BusinessMenuGroupListResponse> ArrangeBusinessMenuGroupAsync(List<ArrangeBusinessMenuGroupRequestModel> models);
        Task<BusinessMenuGroupItemListResponse> GetBusinessMenuGroupItemListAsync(int? businessMenuGroupId, int? productId);
        Task<BusinessMenuGroupItemResponse> GetBusinessMenuGroupItemAsync(int? id);
        Task<BusinessMenuGroupItemResponse> AddUpdateBusinessMenuGroupItemAsync(BusinessMenuGroupItemRequestModel model);
        Task<BusinessMenuGroupItemListResponse> DeleteBusinessMenuGroupItemByIdAsync(int id);
        Task<BusinessMenuGroupItemListResponse> ArrangeBusinessMenuGroupItemAsync(List<BusinessMenuGroupItemRequestModel> models);
        Task<BusinessMenuGroupItemListResponse> AddUpdateBusinessMenuGroupItemsAsync(BusinessMenuGroupProductRequestModel model);
    }
}
