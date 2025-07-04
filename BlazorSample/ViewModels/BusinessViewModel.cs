using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessViewModel : IBusinessViewModel
    {
        private IBusinessService _businessService;

        public BusinessViewModel(IBusinessService businessService)
        {
            _businessService = businessService;
        }
        public async Task<BusinessDetailResponse> GetBusinessDetail(Guid businessGuid, int id)
        {
            BusinessDetailResponse response = await _businessService.GetBusinessDetail(businessGuid, id);
            return response;
        }

        public async Task<List<BusinessSelectModel>> GetBusinessList()
        {
            List<BusinessSelectModel> response = await _businessService.GetBusinessSelectList();
            return response;
        }

        public async Task<BusinessConfigurationOptionResponse> GetBusinessConfigurationOptions()
        {
            return await _businessService.GetBusinessConfigurationOptions();
        }

        public async Task<BusinessConfigurationOptionDetailResponse> GetBusinessConfigurationOptionDetail(string settingNamespace, string settingValue)
        {
            return await _businessService.GetBusinessConfigurationOptionDetail(settingNamespace, settingValue);
        }

        public async Task<BusinessNotesResponse> GetBusinessNotes(int businessId)
        {
            return await _businessService.GetBusinessNotes(businessId);
        }

        public async Task<AddUpdateBusinessNoteResponse> AddUpdateBusinessNote(BusinessNoteRequestModel model)
        {
            return await _businessService.AddUpdateBusinessNote(model);
        }

        public async Task<BusinessNotesResponse> DeleteBusinessNote(int businessId, int id)
        {
            return await _businessService.DeleteBusinessNote(businessId, id);
        }

        public async Task<BaseResponse> UpdateBusinessName(int id, string businessName)
        {
            return await _businessService.UpdateBusinessName(id, businessName);
        }

        public async Task<BaseResponse> UpdateAccountOwner(int id, int accountOwnerId)
        {
            return await _businessService.UpdateAccountOwner(id, accountOwnerId);
        }

        public async Task<UpdateBusinessPhoneResponse> UpdateBusinessPhone(BusinessPhoneRequestModel request)
        {
            return await _businessService.UpdateBusinessPhone(request);
        }

        public async Task<BaseResponse> UpdateBusinessAddress(BusinessAddressRequestModel address)
        {
            return await _businessService.UpdateBusinessAddress(address);
        }

        public async Task<BaseResponse> UpdateBusinessWebsiteURL(BusinessWebsiteURLRequestModel model)
        {
            return await _businessService.UpdateBusinessWebsiteURL(model);
        }

        public async Task<GetAddressByZipCodeResponse> GetAddressByZipCode(string zipCode)
        {
            return await _businessService.GetAddressByZipCode(zipCode);
        }

        public async Task<BaseResponse> UpdateBusinessWeatherFeed(BusinessWeatherFeedRequestModel model)
        {
            return await _businessService.UpdateBusinessWeatherFeed(model);
        }

        public async Task<GetBusinessWeatherFeedResponse> GetBusinessWeatherFeedData(int businessId)
        {
            return await _businessService.GetBusinessWeatherFeedData(businessId);
        }

        public async Task<GetBusinessRecentOrdersAndMembersDataResponse> GetBusinessRecentOrdersAndMembersData(int businessId, DateTime StartDate, DateTime EndDate, bool getNewClubMembers, bool getNewReservations)
        {
            return await _businessService.GetBusinessRecentOrdersAndMembersData(businessId, StartDate, EndDate, getNewClubMembers, getNewClubMembers);
        }
    }
}
