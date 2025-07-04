using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class DiscountViewModel : IDiscountViewModel
    {
        private IDiscountService _discountService;
        public DiscountViewModel(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        public async Task<AddUpdateBusinessDiscountResponse> AddUpdateDiscount(BusinessDiscountRequestModel model)
        {
            AddUpdateBusinessDiscountResponse response = await _discountService.AddUpdateDiscount(model);
            return response;
        }

        public async Task<GetBusinessDiscountListResponse> GetDiscounts(int businessId, DateTime? startDate, DateTime? endDate, string status, string searchText, string salesChannel = "")
        {
            GetBusinessDiscountListResponse response = await _discountService.GetDiscounts(businessId, startDate, endDate, status, searchText, salesChannel);
            return response;
        }

        public async Task<InactiveDiscountResponse> SetDiscountToInactive(SetInactiveDiscountRequestModel model)
        {
            InactiveDiscountResponse response = await _discountService.SetDiscountToInactive(model);
            return response;
        }

        public async Task<GetDiscountDetailResponse> GetDiscountDetail(string discountGuid)
        {
            GetDiscountDetailResponse response = await _discountService.GetDiscountDetail(discountGuid);
            return response;
        }

        public async Task<AddUpdateBusinessDiscountGroupResponse> AddUpdateDiscountGroup(BusinessDiscountGroupRequestModel model)
        {
            AddUpdateBusinessDiscountGroupResponse response = await _discountService.AddUpdateDiscountGroup(model);
            return response;
        }

        public async Task<GetBusinessDiscountGroupListResponse> GetDiscountGroups(int businessId)
        {
            GetBusinessDiscountGroupListResponse response = await _discountService.GetDiscountGroups(businessId);
            return response;
        }

        public async Task<GetDiscountGroupDetailResponse> GetDiscountGroupDetail(int discountGroupId)
        {
            GetDiscountGroupDetailResponse response = await _discountService.GetDiscountGroupDetail(discountGroupId);
            return response;
        }
        public async Task<BaseResponse> DeleteDiscountGroup(int id)
        {
            return await _discountService.DeleteDiscountGroup(id);
        }
    }
}
