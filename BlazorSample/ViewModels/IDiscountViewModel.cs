using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IDiscountViewModel
    {
        Task<AddUpdateBusinessDiscountResponse> AddUpdateDiscount(BusinessDiscountRequestModel model);
        Task<GetBusinessDiscountListResponse> GetDiscounts(int businessId, DateTime? startDate, DateTime? endDate, string status, string searchText, string salesChannel = "");
        Task<InactiveDiscountResponse> SetDiscountToInactive(SetInactiveDiscountRequestModel model);
        Task<GetDiscountDetailResponse> GetDiscountDetail(string discountGuid);
        Task<AddUpdateBusinessDiscountGroupResponse> AddUpdateDiscountGroup(BusinessDiscountGroupRequestModel model);
        Task<GetBusinessDiscountGroupListResponse> GetDiscountGroups(int businessId);
        Task<GetDiscountGroupDetailResponse> GetDiscountGroupDetail(int discountGroupId);
        Task<BaseResponse> DeleteDiscountGroup(int id);

    }
}
