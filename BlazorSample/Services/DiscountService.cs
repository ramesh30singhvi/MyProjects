using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;


namespace CellarPassAppAdmin.Client.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public DiscountService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<AddUpdateBusinessDiscountResponse> AddUpdateDiscount(BusinessDiscountRequestModel model)
        {
            try
            {
                AddUpdateBusinessDiscountResponse response = await _apiManager.PostAsync<BusinessDiscountRequestModel, AddUpdateBusinessDiscountResponse>("discount/add-update-discount", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdateBusinessDiscountResponse();
            }
        }
        public async Task<GetBusinessDiscountListResponse> GetDiscounts(int businessId, DateTime? startDate, DateTime? endDate, string status, string searchText, string salesChannel = "")
        {
            try
            {
                GetBusinessDiscountListResponse response = await _apiManager.GetAsync<GetBusinessDiscountListResponse>("discount/list?businessId=" + businessId + "&status=" + status + "&startDate=" + startDate + "&endDate=" + endDate + "&searchText=" + searchText + "&salesChannel=" + salesChannel);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetBusinessDiscountListResponse();
            }
        }
        public async Task<InactiveDiscountResponse> SetDiscountToInactive(SetInactiveDiscountRequestModel model)
        {
            try
            {
                InactiveDiscountResponse response = await _apiManager.PostAsync<SetInactiveDiscountRequestModel, InactiveDiscountResponse>("discount/set-discount-inactive", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new InactiveDiscountResponse();
            }
        }
        public async Task<GetDiscountDetailResponse> GetDiscountDetail(string discountGuid)
        {
            try
            {
                GetDiscountDetailResponse response = await _apiManager.GetAsync<GetDiscountDetailResponse>("discount/detail?discountGuid=" + discountGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetDiscountDetailResponse();
            }
        }
        public async Task<AddUpdateBusinessDiscountGroupResponse> AddUpdateDiscountGroup(BusinessDiscountGroupRequestModel model)
        {
            try
            {
                AddUpdateBusinessDiscountGroupResponse response = await _apiManager.PostAsync<BusinessDiscountGroupRequestModel, AddUpdateBusinessDiscountGroupResponse>("discount/add-update-discount-group", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdateBusinessDiscountGroupResponse();
            }
        }
        public async Task<GetBusinessDiscountGroupListResponse> GetDiscountGroups(int businessId)
        {
            try
            {
                GetBusinessDiscountGroupListResponse response = await _apiManager.GetAsync<GetBusinessDiscountGroupListResponse>("discount/group-list?businessId=" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetBusinessDiscountGroupListResponse();
            }
        }
        public async Task<GetDiscountGroupDetailResponse> GetDiscountGroupDetail(int discountGroupId)
        {
            try
            {
                GetDiscountGroupDetailResponse response = await _apiManager.GetAsync<GetDiscountGroupDetailResponse>("discount/group-detail?discountGroupId=" + discountGroupId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetDiscountGroupDetailResponse();
            }
        }
        public async Task<BaseResponse> DeleteDiscountGroup(int id)
        {
            try
            {
                BaseResponse response = await _apiManager.DeleteAsync<BaseResponse>("discount/delete-discount-group?id=" + id);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
    }
}
