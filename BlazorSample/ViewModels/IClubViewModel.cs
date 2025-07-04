using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IClubViewModel
    {
        Task<CreateClubResponse> CreateClubAsync(BusinessClubRequestModel model);
        Task<CreateClubResponse> UpdateClubAsync(UpdateClubRequestModel model);
        Task<GetClubListResponse> GetClubListAsync(int businessId);
        Task<GetClubDetailsResponse> GetClubDetailsAsync(int clubId, string clubGuid);
        Task<GetClubMembersListResponse> GetClubMembersListAsync(int businessId, string keyword);
        Task<GetClubCancellationReasonListResponse> GetClubCancellationReasonListAsync(int businessId);
        Task<GetClubSubscriptionTagListResponse> GetClubSubscriptionTagListAsync(int businessId);
        Task<GetClubSignUpListResponse> GetClubSignUpListAsync(int businessId, int status, string keyword);
        Task<ExportClubMembersResponse> ExportClubMembers(int businessId, int status, string keyword);
        Task<CreateClubSignUpResponse> CreateClubSignUpAsync(BusinessClubSignUpRequestModel model);
        Task<CreateClubSubscriptionResponse> CreateClubSubscriptionAsync(BusinessClubSubscriptionRequestModel model);
        Task<GetClubSignUpDetailsResponse> GetSignupById(int signupId = 0, string signupGuid = "");
        Task<CreateClubSignUpResponse> UpdateClubSignUpAsync(UpdateClubSignUpRequestModel model);
        Task<UploadClubImageResponse> UploadClubFile(IFormFile file);
        Task<UploadClubImageResponse> UploadBusinessClubImage(UploadClubImageRequestModel model);
        Task<CancelClubMembershipResponse> CancelClubMembershipAsync(CancelClubMembershipRequestModel model);
        Task<ClubSubscriptionListResponse> GetClubSubscriptions(Guid? clubCustomerGuid = null, Guid? subscriptionsGuid = null, bool cancelledSubscriptions = true);
        Task<ClubCancelledSubscriptionsResponse> GetClubCancelledSubscriptions(Guid clubCustomerGuid);
        Task<CreateShipmentResponse> CreateShipmentAsync(CreateShipmentRequestModel model);
        Task<UpdateShipmentResponse> UpdateShipmentAsync(UpdateShipmentRequestModel model);
        Task<BaseResponse> UpdateSubscriptionShippingPreference(UpdateShippingPreferenceRequestModel model);
        Task<BaseResponse> UpdateSubscriptionBillShipAddress(UpdateBillShipToAddressRequestModel model);
        Task<BaseResponse> UpdateSubscriptionDeliveryType(UpdateSubscriptionDeliveryTypeRequestModel model);
        Task<BaseResponse> AddClubSubscriptionNote(AddSubscriptionNoteRequestModel model);
        Task<BaseResponse> UpdateClubSubscriptionTags(UpdateSubscriptionTagsRequestModel model);
        Task<AddUpdatePaymentMethodResponse> AddUpdateSubscriptionPaymentDetail(AddUpdatePaymentMethodRequestModel model);
        Task<GetClubShipmentDetailsResponse> GetClubShipmentDetails(string shipmentGuid);
        Task<GetShipmentListResponse> GetShipmentListAsync(int businessId, string clubGuid, string keyword);
        Task<UploadShipmentImageResponse> UploadShipmentImage(UploadShipmentImageRequestModel model);
        Task<GetClubShipmentInventoryResponse> GetShipmentInventoryData(Guid shipmentGuid);
        Task<GetBusinessDiscountResponse> GetBusinessDiscountsForShipment(Guid shipmentGuid);
        Task<BaseResponse> UpdateShipmentStatus(UpdateShipmentStatusRequestModel model);
        Task<GetClubShipmentTagListResponse> GetClubShipmentTagList(int businessId);
        Task<GetClubWaitListResponse> GetClubWaitListAsync(int businessId, string keyword);
        Task<AddEditVacationResponse> AddEditVacationAsync(AddEditVacationRequestModel model);
        Task<BaseResponse> CancelVacationHold(int id);
        Task<BaseResponse> DeleteVacation(int id);

        #region Club Members

        Task<BusinessClubMemberResponse> GetBusinessClubMembers(int businessId, int count);
        Task<BusinessClubMemberResponse> GetBusinessCancelledClubMembers(int businessId);
        Task<BusinessClubMemberResponse> GetBusinessVacationHoldClubMembers(int businessId);
        Task<ClubVacationHoldListResponse> GetVacationHoldByClubId(int clubId);
        Task<GetCustomerLTVResponse> GetCustomerLTV(int customerId);
        #endregion Club Members
        Task<BusinessClubSubscriptionMetaListResponse> AddUpdateBusinessClubSubscriptionMetaListAsync(List<BusinessClubSubscriptionMetaRequestModel> models);
    }
}
