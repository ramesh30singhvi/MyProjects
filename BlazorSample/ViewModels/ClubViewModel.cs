using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class ClubViewModel : IClubViewModel
    {
        private readonly IClubService _clubService;

        public ClubViewModel(IClubService clubService)
        {
            _clubService = clubService;
        }
        public async Task<CreateClubResponse> CreateClubAsync(BusinessClubRequestModel model)
        {
            var response = await _clubService.CreateClubAsync(model);
            return response;
        }
        public async Task<CreateClubResponse> UpdateClubAsync(UpdateClubRequestModel model)
        {
            var response = await _clubService.UpdateClubAsync(model);
            return response;
        }
        public async Task<GetClubListResponse> GetClubListAsync(int businessId)
        {
            return await _clubService.GetClubListAsync(businessId);
        }

        public async Task<GetClubDetailsResponse> GetClubDetailsAsync(int clubId, string clubGuid)
        {
            return await _clubService.GetClubDetailsAsync(clubId, clubGuid);
        }
        public async Task<GetClubMembersListResponse> GetClubMembersListAsync(int businessId, string keyword)
        {
            return await _clubService.GetClubMembersListAsync(businessId, keyword);
        }
        public async Task<GetClubCancellationReasonListResponse> GetClubCancellationReasonListAsync(int businessId)
        {
            return await _clubService.GetClubCancellationReasonListAsync(businessId);
        }
        public async Task<GetClubSubscriptionTagListResponse> GetClubSubscriptionTagListAsync(int businessId)
        {
            return await _clubService.GetClubSubscriptionTagListAsync(businessId);
        }
        public async Task<GetClubSignUpListResponse> GetClubSignUpListAsync(int businessId, int status, string keyword)
        {
            return await _clubService.GetClubSignUpListAsync(businessId, status, keyword);
        }
        public async Task<ExportClubMembersResponse> ExportClubMembers(int businessId, int status, string keyword)
        {
            return await _clubService.ExportClubMembers(businessId, status, keyword);
        }
        public async Task<CreateClubSignUpResponse> CreateClubSignUpAsync(BusinessClubSignUpRequestModel model)
        {
            return await _clubService.CreateClubSignUpAsync(model);
        }
        public async Task<CreateClubSubscriptionResponse> CreateClubSubscriptionAsync(BusinessClubSubscriptionRequestModel model)
        {
            return await _clubService.CreateClubSubscriptionAsync(model);
        }
        public async Task<GetClubSignUpDetailsResponse> GetSignupById(int signupId = 0, string signupGuid = "")
        {
            return await _clubService.GetSignupById(signupId, signupGuid);
        }
        public async Task<CreateClubSignUpResponse> UpdateClubSignUpAsync(UpdateClubSignUpRequestModel model)
        {
            return await _clubService.UpdateClubSignUpAsync(model);
        }
        public async Task<UploadClubImageResponse> UploadBusinessClubImage(UploadClubImageRequestModel model)
        {
            return await _clubService.UploadBusinessClubImage(model);
        }
        public async Task<UploadClubImageResponse> UploadClubFile(IFormFile file)
        {
            return await _clubService.UploadClubFile(file);
        }
        public async Task<CancelClubMembershipResponse> CancelClubMembershipAsync(CancelClubMembershipRequestModel model)
        {
            return await _clubService.CancelClubMembershipAsync(model);
        }
        public async Task<ClubSubscriptionListResponse> GetClubSubscriptions(Guid? clubCustomerGuid = null, Guid? subscriptionsGuid = null, bool cancelledSubscriptions = true)
        {
            return await _clubService.GetClubSubscriptions(clubCustomerGuid, subscriptionsGuid, cancelledSubscriptions);
        }
        public async Task<ClubCancelledSubscriptionsResponse> GetClubCancelledSubscriptions(Guid clubCustomerGuid)
        {
            return await _clubService.GetClubCancelledSubscriptions(clubCustomerGuid);
        }
        public async Task<CreateShipmentResponse> CreateShipmentAsync(CreateShipmentRequestModel model)
        {
            return await _clubService.CreateShipmentAsync(model);
        }
        public async Task<UpdateShipmentResponse> UpdateShipmentAsync(UpdateShipmentRequestModel model)
        {
            return await _clubService.UpdateShipmentAsync(model);
        }
        public async Task<BaseResponse> UpdateSubscriptionShippingPreference(UpdateShippingPreferenceRequestModel model)
        {
            return await _clubService.UpdateSubscriptionShippingPreference(model);
        }
        public async Task<BaseResponse> UpdateSubscriptionBillShipAddress(UpdateBillShipToAddressRequestModel model)
        {
            return await _clubService.UpdateSubscriptionBillShipAddress(model);
        }
        public async Task<BaseResponse> UpdateSubscriptionDeliveryType(UpdateSubscriptionDeliveryTypeRequestModel model)
        {
            return await _clubService.UpdateSubscriptionDeliveryType(model);
        }
        public async Task<BaseResponse> AddClubSubscriptionNote(AddSubscriptionNoteRequestModel model)
        {
            return await _clubService.AddClubSubscriptionNote(model);
        }
        public async Task<BaseResponse> UpdateClubSubscriptionTags(UpdateSubscriptionTagsRequestModel model)
        {
            return await _clubService.UpdateClubSubscriptionTags(model);
        }
        public async Task<AddUpdatePaymentMethodResponse> AddUpdateSubscriptionPaymentDetail(AddUpdatePaymentMethodRequestModel model)
        {
            return await _clubService.AddUpdateSubscriptionPaymentDetail(model);
        }
        public async Task<GetClubShipmentDetailsResponse> GetClubShipmentDetails(string shipmentGuid)
        {
            return await _clubService.GetClubShipmentDetails(shipmentGuid);
        }
        public async Task<GetShipmentListResponse> GetShipmentListAsync(int businessId, string clubGuid, string keyword)
        {
            return await _clubService.GetShipmentListAsync(businessId, clubGuid, keyword);
        }
        public async Task<UploadShipmentImageResponse> UploadShipmentImage(UploadShipmentImageRequestModel model)
        {
            return await _clubService.UploadShipmentImage(model);
        }
        public async Task<GetClubShipmentInventoryResponse> GetShipmentInventoryData(Guid shipmentGuid)
        {
            return await _clubService.GetShipmentInventoryData(shipmentGuid);
        }
        public async Task<GetBusinessDiscountResponse> GetBusinessDiscountsForShipment(Guid shipmentGuid)
        {
            return await _clubService.GetBusinessDiscountsForShipment(shipmentGuid);
        }
        public async Task<BaseResponse> UpdateShipmentStatus(UpdateShipmentStatusRequestModel model)
        {
            return await _clubService.UpdateShipmentStatus(model);
        }
        public async Task<GetClubShipmentTagListResponse> GetClubShipmentTagList(int businessId)
        {
            return await _clubService.GetClubShipmentTagList(businessId);
        }
        public async Task<GetClubWaitListResponse> GetClubWaitListAsync(int businessId, string keyword)
        {
            return await _clubService.GetClubWaitListAsync(businessId, keyword);
        }
        public async Task<AddEditVacationResponse> AddEditVacationAsync(AddEditVacationRequestModel model)
        {
            return await _clubService.AddEditVacationAsync(model);
        }
        public async Task<BaseResponse> CancelVacationHold(int id)
        {
            return await _clubService.CancelVacationHold(id);
        }
        public async Task<BaseResponse> DeleteVacation(int id)
        {
            return await _clubService.DeleteVacation(id);
        }

        #region Club Members

        public async Task<BusinessClubMemberResponse> GetBusinessClubMembers(int businessId, int count)
        {
            return await _clubService.GetBusinessClubMembers(businessId, count);
        }

        public async Task<BusinessClubMemberResponse> GetBusinessCancelledClubMembers(int businessId)
        {
            return await _clubService.GetBusinessCancelledClubMembers(businessId);
        }

        public async Task<BusinessClubMemberResponse> GetBusinessVacationHoldClubMembers(int businessId)
        {
            return await _clubService.GetBusinessVacationHoldClubMembers(businessId);
        }
        public async Task<ClubVacationHoldListResponse> GetVacationHoldByClubId(int clubId)
        {
            return await _clubService.GetVacationHoldByClubId(clubId);
        }
        public async Task<GetCustomerLTVResponse> GetCustomerLTV(int customerId)
        {
            return await _clubService.GetCustomerLTV(customerId);
        }
        #endregion Club Members
        public async Task<BusinessClubSubscriptionMetaListResponse> AddUpdateBusinessClubSubscriptionMetaListAsync(List<BusinessClubSubscriptionMetaRequestModel> models)
        {
            return await _clubService.AddUpdateBusinessClubSubscriptionMetaListAsync(models);
        }
    }
}
