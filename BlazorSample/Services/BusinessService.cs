using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IApiManager _apiManager;

        public BusinessService(IApiManager apiManager)
        {
            _apiManager = apiManager;
        }
        public async Task<BusinessDetailResponse> GetBusinessDetail(Guid businessGuid, int id)
        {
            var businessGuidUrlString = businessGuid != Guid.Empty ? "?businessGuid=" + businessGuid : "";
            var businessIdUrlString = id != 0 ? "?id=" + id : "";
            try
            {
                BusinessDetailResponse response = await _apiManager.GetAsync<BusinessDetailResponse>("Business/detail" + businessGuidUrlString + businessIdUrlString);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessDetailResponse();
            }
        }

        public async Task<List<BusinessSelectModel>> GetBusinessSelectList()
        {
            try
            {
                BusinessSelectModelResponse response = await _apiManager.GetAsync<BusinessSelectModelResponse>("Business/select-business");
                return response.BusinessSelectModel;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<BusinessSelectModel>();
            }
        }

        public async Task<BusinessConfigurationOptionResponse> GetBusinessConfigurationOptions()
        {
            try
            {
                BusinessConfigurationOptionResponse response = await _apiManager.GetAsync<BusinessConfigurationOptionResponse>("Business/configuration-options");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessConfigurationOptionResponse();
            }
        }

        public async Task<BusinessConfigurationOptionDetailResponse> GetBusinessConfigurationOptionDetail(string settingNamespace, string settingValue)
        {
            try
            {
                BusinessConfigurationOptionDetailResponse response = await _apiManager.GetAsync<BusinessConfigurationOptionDetailResponse>("Business/configuration-option-detail/" + settingNamespace + "/" + settingValue);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessConfigurationOptionDetailResponse();
            }
        }

        public async Task<BusinessNotesResponse> GetBusinessNotes(int businessId)
        {
            try
            {
                BusinessNotesResponse response = await _apiManager.GetAsync<BusinessNotesResponse>("Business/notes/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessNotesResponse();
            }
        }

        public async Task<AddUpdateBusinessNoteResponse> AddUpdateBusinessNote(BusinessNoteRequestModel model)
        {
            try
            {
                AddUpdateBusinessNoteResponse response = await _apiManager.PostAsync<BusinessNoteRequestModel, AddUpdateBusinessNoteResponse>("Business/add-update-note", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdateBusinessNoteResponse();
            }
        }

        public async Task<BusinessNotesResponse> DeleteBusinessNote(int businessId, int id)
        {
            try
            {
                BusinessNotesResponse response = await _apiManager.GetAsync<BusinessNotesResponse>("Business/delete-business-note/" + businessId + "/" + id);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessNotesResponse();
            }
        }

        public async Task<BaseResponse> UpdateBusinessName(int id, string businessName)
        {
            try
            {
                BaseResponse response = await _apiManager.GetAsync<BaseResponse>("Business/update-name/" + id + "/" + businessName);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<BaseResponse> UpdateAccountOwner(int id, int accountOwnerId)
        {
            try
            {
                BaseResponse response = await _apiManager.GetAsync<BaseResponse>("Business/update-account-owner/" + id + "/" + accountOwnerId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<UpdateBusinessPhoneResponse> UpdateBusinessPhone(BusinessPhoneRequestModel request)
        {
            try
            {
                UpdateBusinessPhoneResponse response = await _apiManager.PutAsync<BusinessPhoneRequestModel, UpdateBusinessPhoneResponse>("Business/update-phone/", request);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UpdateBusinessPhoneResponse();
            }
        }

        public async Task<BaseResponse> UpdateBusinessAddress(BusinessAddressRequestModel address)
        {
            try
            {
                BaseResponse response = await _apiManager.PutAsync<BusinessAddressRequestModel, BaseResponse>("Business/update-address", address);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<BaseResponse> UpdateBusinessWebsiteURL(BusinessWebsiteURLRequestModel model)
        {
            try
            {
                return await _apiManager.PutAsync<BusinessWebsiteURLRequestModel, BaseResponse>("Business/update-website-url", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<GetAddressByZipCodeResponse> GetAddressByZipCode(string zipCode)
        {
            try
            {
                GetAddressByZipCodeResponse response = await _apiManager.GetAsync<GetAddressByZipCodeResponse>("Business/get-address-by-zipcode/" + zipCode);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetAddressByZipCodeResponse();
            }
        }

        public async Task<BaseResponse> UpdateBusinessWeatherFeed(BusinessWeatherFeedRequestModel model)
        {
            try
            {
                BaseResponse response = await _apiManager.PutAsync<BusinessWeatherFeedRequestModel, BaseResponse>("Business/update-weather-feed", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<GetBusinessWeatherFeedResponse> GetBusinessWeatherFeedData(int businessId)
        {
            try
            {
                GetBusinessWeatherFeedResponse response = await _apiManager.GetAsync<GetBusinessWeatherFeedResponse>("Business/get-business-weather-feed/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetBusinessWeatherFeedResponse();
            }
        }

        public async Task<GetBusinessRecentOrdersAndMembersDataResponse> GetBusinessRecentOrdersAndMembersData(int businessId, DateTime StartDate, DateTime EndDate, bool getNewClubMembers, bool getNewReservations)
        {
            try
            {
                GetBusinessRecentOrdersAndMembersDataResponse response = await _apiManager.GetAsync<GetBusinessRecentOrdersAndMembersDataResponse>(string.Format("Business/get-recent-orders-members-count?businessId={0}&StartDate={1}&EndDate={2}&getNewClubMembers={3}&getNewReservations={4}",
                                                                                     businessId, StartDate.ToString("MM-dd-yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture),
                                                                                    EndDate.ToString("MM-dd-yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture),
                                                                                    getNewClubMembers, getNewReservations));
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetBusinessRecentOrdersAndMembersDataResponse();
            }
        }
    }
}
