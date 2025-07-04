using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessViewModel
    {       
        Task<BusinessDetailResponse> GetBusinessDetail(Guid businessGuid, int id);
        Task<List<BusinessSelectModel>> GetBusinessList();
        Task<BusinessConfigurationOptionResponse> GetBusinessConfigurationOptions();
        Task<BusinessConfigurationOptionDetailResponse> GetBusinessConfigurationOptionDetail(string settingNamespace, string settingValue);
        Task<BusinessNotesResponse> GetBusinessNotes(int businessId);
        Task<AddUpdateBusinessNoteResponse> AddUpdateBusinessNote(BusinessNoteRequestModel model);
        Task<BusinessNotesResponse> DeleteBusinessNote(int businessId, int id);
        Task<BaseResponse> UpdateBusinessName(int id, string businessName);
        Task<BaseResponse> UpdateAccountOwner(int id, int accountOwnerId);
        Task<UpdateBusinessPhoneResponse> UpdateBusinessPhone(BusinessPhoneRequestModel request);
        Task<BaseResponse> UpdateBusinessAddress(BusinessAddressRequestModel address);
        Task<BaseResponse> UpdateBusinessWebsiteURL(BusinessWebsiteURLRequestModel model);
        Task<BaseResponse> UpdateBusinessWeatherFeed(BusinessWeatherFeedRequestModel model);
        Task<GetAddressByZipCodeResponse> GetAddressByZipCode(string zipCode);
        Task<GetBusinessWeatherFeedResponse> GetBusinessWeatherFeedData(int businessId);
        Task<GetBusinessRecentOrdersAndMembersDataResponse> GetBusinessRecentOrdersAndMembersData(int businessId, DateTime StartDate, DateTime EndDate, bool getNewClubMembers, bool getNewReservations);
    }
}
