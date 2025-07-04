using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Enums;
using CellarPassAppAdmin.Shared.Helpers;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class ClubService : IClubService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public ClubService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<CreateClubResponse> CreateClubAsync(BusinessClubRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CreateClubResponse>(_configuration["App:PeopleApiUrl"] + "Club/create-club", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CreateClubResponse();
            }
        }
        public async Task<CreateClubResponse> UpdateClubAsync(UpdateClubRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CreateClubResponse>(_configuration["App:PeopleApiUrl"] + "Club/update-club", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CreateClubResponse();
            }
        }
        public async Task<GetClubDetailsResponse> GetClubDetailsAsync(int clubId, string clubGuid)
        {
            try
            {
                return await _apiManager.GetAsync<GetClubDetailsResponse>(_configuration["App:PeopleApiUrl"] + "Club/details?clubId=" + clubId + "&clubGuid=" + clubGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetClubDetailsResponse();
            }
        }
        public async Task<GetClubListResponse> GetClubListAsync(int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<GetClubListResponse>(_configuration["App:PeopleApiUrl"] + "Club/list?BusinessId=" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetClubListResponse();
            }
        }
        public async Task<GetClubMembersListResponse> GetClubMembersListAsync(int businessId, string keyword)
        {
            try
            {
                return await _apiManager.GetAsync<GetClubMembersListResponse>(_configuration["App:PeopleApiUrl"] + "Club/members-list?BusinessId=" + businessId + "&keyword=" + keyword);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetClubMembersListResponse();
            }
        }

        public async Task<GetClubSignUpListResponse> GetClubSignUpListAsync(int businessId, int Status, string keyword)
        {
            try
            {
                return await _apiManager.GetAsync<GetClubSignUpListResponse>(_configuration["App:PeopleApiUrl"] + "Club/signup-list?businessId=" + businessId + "&status=" + Status + "&keyword=" + keyword);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetClubSignUpListResponse();
            }
        }

        public async Task<ExportClubMembersResponse> ExportClubMembers(int businessId, int Status, string keyword)
        {
            try
            {
                return await _apiManager.GetAsync<ExportClubMembersResponse>(_configuration["App:PeopleApiUrl"] + "Club/export-club-members?businessId=" + businessId + "&status=" + Status + "&keyword=" + keyword);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ExportClubMembersResponse();
            }
        }

        public async Task<CreateClubSignUpResponse> CreateClubSignUpAsync(BusinessClubSignUpRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CreateClubSignUpResponse>(_configuration["App:PeopleApiUrl"] + "Club/create-signup", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CreateClubSignUpResponse();
            }
        }
        public async Task<UploadClubImageResponse> UploadBusinessClubImage(UploadClubImageRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<UploadClubImageResponse>(_configuration["App:PeopleApiUrl"] + "Club/upload-business-club-image", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UploadClubImageResponse();
            }
        }

        public async Task<UploadClubImageResponse> UploadClubFile(IFormFile file)
        {
            try
            {
                string fileExtension = ".jpg";
                UploadClubImageResponse response = new UploadClubImageResponse();
                if (!string.IsNullOrWhiteSpace(file.FileName) && file.FileName.IndexOf(".") > -1)
                {
                    fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();
                    if (!fileExtension.StartsWith("."))
                        fileExtension = "." + fileExtension;
                }

                string fileName = string.Format("{0}_{1}{2}", Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("MMddyyyyhhmm"), fileExtension);
                string folderPath = _configuration["CDNStorageSettings:ClubImagesFolder"];
                string containerName = _configuration["CDNStorageSettings:ClubImageContainer"];
                string storageConnstring = _configuration["CDNStorageSettings:ConnectionString"];
                string accessKey = _configuration["CDNStorageSettings:AccessKey"];
                string fullfileName = Path.Combine(folderPath, fileName);

                using (var stream = file.OpenReadStream())
                {
                    var fileUploaded = await CDNImageHelper.UploadFileToCDN(fullfileName, stream, storageConnstring, accessKey, containerName);
                    if (fileUploaded.Success)
                    {
                        response.success = true;
                        response.data = new ClubDataModel { ClubFileName = fileName };
                    }
                    else
                    {
                        response.success = false;
                        response.error_info = new ErrorInfo
                        {
                            error_type = ErrorType.InvalidData,
                            description = "Data could not be uploaded to CDN"
                        };
                    }
                }
                return response;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task<DownloadClubImageResponse> DownloadClubFile(string fileName)
        {
            DownloadClubImageResponse response = new DownloadClubImageResponse();
            string folderPath = _configuration["CDNStorageSettings:SignatureFilesFolder"];
            string containerName = _configuration["CDNStorageSettings:SignatureContainer"];
            string storageConnstring = _configuration["CDNStorageSettings:ConnectionString"];
            string accessKey = _configuration["CDNStorageSettings:AccessKey"];
            string fullfileName = Path.Combine(folderPath, fileName);
            byte[] fileData = await CDNImageHelper.DownloadFileFromCDN(fullfileName, storageConnstring, accessKey, containerName);

            if (fileData != null && fileData.Length > 0)
            {
                response.success = true;
                response.data = new DownloadClubDataModel { ClubFileBytes = fileData };
            }
            else
            {
                response.success = false;
                response.error_info = new ErrorInfo
                {
                    error_type = ErrorType.InvalidData,
                    description = "Data could not be downloaded from CDN"
                };
            }
            return response;
        }

        public async Task<GetClubWaitListResponse> GetClubWaitListAsync(int businessId, string keyword)
        {
            try
            {
                return await _apiManager.GetAsync<GetClubWaitListResponse>(_configuration["App:PeopleApiUrl"] + "Club/waitlist-list?BusinessId=" + businessId + "&keyword=" + keyword);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetClubWaitListResponse();
            }
        }

        public async Task<GetClubCancellationReasonListResponse> GetClubCancellationReasonListAsync(int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<GetClubCancellationReasonListResponse>(_configuration["App:PeopleApiUrl"] + "Club/cancellation-reason-list?BusinessId=" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetClubCancellationReasonListResponse();
            }
        }

        public async Task<GetClubSubscriptionTagListResponse> GetClubSubscriptionTagListAsync(int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<GetClubSubscriptionTagListResponse>(_configuration["App:PeopleApiUrl"] + "Club/subscription-tag-list?BusinessId=" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetClubSubscriptionTagListResponse();
            }
        }

        public async Task<CreateClubSubscriptionResponse> CreateClubSubscriptionAsync(BusinessClubSubscriptionRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CreateClubSubscriptionResponse>(_configuration["App:PeopleApiUrl"] + "Club/create-Subscription", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CreateClubSubscriptionResponse();
            }
        }

        public async Task<GetClubSignUpDetailsResponse> GetSignupById(int signupId = 0, string signupGuid = "")
        {
            try
            {
                return await _apiManager.GetAsync<GetClubSignUpDetailsResponse>(_configuration["App:PeopleApiUrl"] + "Club/signup-details?signupId=" + signupId + "&signupGuid=" + signupGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetClubSignUpDetailsResponse();
            }
        }

        public async Task<CreateClubSignUpResponse> UpdateClubSignUpAsync(UpdateClubSignUpRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CreateClubSignUpResponse>(_configuration["App:PeopleApiUrl"] + "Club/update-signup", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CreateClubSignUpResponse();
            }
        }
        public async Task<CancelClubMembershipResponse> CancelClubMembershipAsync(CancelClubMembershipRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CancelClubMembershipResponse>(_configuration["App:PeopleApiUrl"] + "Club/cancel-club-membership", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CancelClubMembershipResponse();
            }
        }
        public async Task<ClubSubscriptionListResponse> GetClubSubscriptions(Guid? clubCustomerGuid = null, Guid? subscriptionsGuid = null, bool cancelledSubscriptions = true)
        {
            try
            {
                return await _apiManager.GetAsync<ClubSubscriptionListResponse>(_configuration["App:PeopleApiUrl"] + "Club/get-club-subscriptions?clubCustomerGuid=" + clubCustomerGuid + "&subscriptionsGuid=" + subscriptionsGuid + "&cancelledSubscriptions=" + cancelledSubscriptions);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ClubSubscriptionListResponse();
            }
        }
        public async Task<ClubCancelledSubscriptionsResponse> GetClubCancelledSubscriptions(Guid clubCustomerGuid)
        {
            try
            {
                return await _apiManager.GetAsync<ClubCancelledSubscriptionsResponse>(_configuration["App:PeopleApiUrl"] + "Club/get-cancelled-subscriptions/" + clubCustomerGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ClubCancelledSubscriptionsResponse();
            }
        }
        public async Task<CreateShipmentResponse> CreateShipmentAsync(CreateShipmentRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CreateShipmentResponse>(_configuration["App:PeopleApiUrl"] + "Club/create-shipment", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CreateShipmentResponse();
            }
        }
        public async Task<UpdateShipmentResponse> UpdateShipmentAsync(UpdateShipmentRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<UpdateShipmentResponse>(_configuration["App:PeopleApiUrl"] + "Club/update-shipment", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UpdateShipmentResponse();
            }
        }
        public async Task<GetShipmentListResponse> GetShipmentListAsync(int businessId, string clubGuid, string keyword)
        {
            try
            {
                return await _apiManager.GetAsync<GetShipmentListResponse>(_configuration["App:PeopleApiUrl"] + "Club/shipment-list?BusinessId=" + businessId + "&clubGuid=" + clubGuid + "&keyword=" + keyword);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetShipmentListResponse();
            }
        }
        public async Task<AddEditVacationResponse> AddEditVacationAsync(AddEditVacationRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<AddEditVacationResponse>(_configuration["App:PeopleApiUrl"] + "Club/add-update-vacation-hold", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddEditVacationResponse();
            }
        }
        public async Task<BaseResponse> DeleteVacation(int id)
        {
            try
            {
                BaseResponse response = await _apiManager.DeleteAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "Club/delete-vacation/" + id);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddEditVacationResponse();
            }
        }
        public async Task<BaseResponse> CancelVacationHold(int id)
        {
            try
            {
                BaseResponse response = await _apiManager.GetAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "Club/cancel-vacation-hold/" + id);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddEditVacationResponse();
            }
        }
        public async Task<GetClubShipmentTagListResponse> GetClubShipmentTagList(int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<GetClubShipmentTagListResponse>(_configuration["App:PeopleApiUrl"] + "Club/club-shipment-tag-list?BusinessId=" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetClubShipmentTagListResponse();
            }
        }
        public async Task<GetClubShipmentDetailsResponse> GetClubShipmentDetails(string shipmentGuid)
        {
            try
            {
                return await _apiManager.GetAsync<GetClubShipmentDetailsResponse>(_configuration["App:PeopleApiUrl"] + "Club/shipment-details?shipmentGuid=" + shipmentGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetClubShipmentDetailsResponse();
            }
        }
        public async Task<BaseResponse> UpdateSubscriptionShippingPreference(UpdateShippingPreferenceRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "Club/update-subscription-shipping-preference", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<BaseResponse> UpdateSubscriptionBillShipAddress(UpdateBillShipToAddressRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "Club/update-subscription-bill-ship-address", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<BaseResponse> UpdateSubscriptionDeliveryType(UpdateSubscriptionDeliveryTypeRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "Club/update-subscription-delivery-type", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<BaseResponse> AddClubSubscriptionNote(AddSubscriptionNoteRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "Club/add-subscription-note", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<BaseResponse> UpdateClubSubscriptionTags(UpdateSubscriptionTagsRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "Club/update-subscription-tags", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<AddUpdatePaymentMethodResponse> AddUpdateSubscriptionPaymentDetail(AddUpdatePaymentMethodRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<AddUpdatePaymentMethodResponse>(_configuration["App:PeopleApiUrl"] + "Club/add-update-subscription-payment", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdatePaymentMethodResponse();
            }
        }
        public async Task<BaseResponse> UpdateShipmentStatus(UpdateShipmentStatusRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "Club/update-shipment-status", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<UploadShipmentImageResponse> UploadShipmentImage(UploadShipmentImageRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<UploadShipmentImageResponse>(_configuration["App:PeopleApiUrl"] + "Club/upload-shipment-image", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UploadShipmentImageResponse();
            }
        }
        public async Task<GetClubShipmentInventoryResponse> GetShipmentInventoryData(Guid shipmentGuid)
        {
            try
            {
                return await _apiManager.GetAsync<GetClubShipmentInventoryResponse>(_configuration["App:PeopleApiUrl"] + "Club/get-shipment-inventory-data/" + shipmentGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetClubShipmentInventoryResponse();
            }
        }
        public async Task<GetBusinessDiscountResponse> GetBusinessDiscountsForShipment(Guid shipmentGuid)
        {
            try
            {
                return await _apiManager.GetAsync<GetBusinessDiscountResponse>(_configuration["App:PeopleApiUrl"] + "Club/get-discount-for-shipment/" + shipmentGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetBusinessDiscountResponse();
            }
        }

        #region Club Members

        public async Task<BusinessClubMemberResponse> GetBusinessClubMembers(int businessId, int count)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessClubMemberResponse>(_configuration["App:PeopleApiUrl"] + "Club/club-members/" + businessId + "/" + count);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessClubMemberResponse();
            }
        }

        public async Task<BusinessClubMemberResponse> GetBusinessCancelledClubMembers(int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessClubMemberResponse>(_configuration["App:PeopleApiUrl"] + "Club/cancelled-club-members/" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessClubMemberResponse();
            }
        }
        public async Task<BusinessClubMemberResponse> GetBusinessVacationHoldClubMembers(int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessClubMemberResponse>(_configuration["App:PeopleApiUrl"] + "Club/vacation-hold-club-members/" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessClubMemberResponse();
            }
        } 
        public async Task<ClubVacationHoldListResponse> GetVacationHoldByClubId(int clubId)
        {
            try
            {
                return await _apiManager.GetAsync<ClubVacationHoldListResponse>(_configuration["App:PeopleApiUrl"] + "Club/club-vacation-holds/" + clubId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ClubVacationHoldListResponse();
            }
        }
        public async Task<GetCustomerLTVResponse> GetCustomerLTV(int customerId)
        {
            try
            {
                return await _apiManager.GetAsync<GetCustomerLTVResponse>(_configuration["App:PeopleApiUrl"] + "Club/get-customer-ltv/" + customerId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetCustomerLTVResponse();
            }
        }
        #endregion Club Members

        public async Task<BusinessClubSubscriptionMetaListResponse> AddUpdateBusinessClubSubscriptionMetaListAsync(List<BusinessClubSubscriptionMetaRequestModel> models)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessClubSubscriptionMetaListResponse>(_configuration["App:PeopleApiUrl"] + "Club/add-update-business-club-subscription-meta", models);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessClubSubscriptionMetaListResponse();
            }
        }
    }
}
