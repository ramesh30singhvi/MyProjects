using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class EmailNotificationViewModel : IEmailNotificationViewModel
    {
        private readonly IEmailNotificationService _emailNotificationService;
        public EmailNotificationViewModel(IEmailNotificationService emailNotificationService)
        {
            _emailNotificationService = emailNotificationService;
        }
        public async Task<GetEmailNotificationResponse> GetEmailNotificationByCategoryId(int categoryId, int businessId)
        {
            return await _emailNotificationService.GetEmailNotificationByCategoryId(categoryId, businessId);
        }
        public async Task<GetEmailNotificationDetailResponse> GetEmailNotificationDetail(Guid? idGUID = null, int? notificationType = null, int? categoryId = null)
        {
            return await _emailNotificationService.GetEmailNotificationDetail(idGUID, notificationType, categoryId);
        }
        public async Task<BusinessNotificationEmailListResponse> AddUpdateBusinessNotificationEmail(List<AddUpdateBusinessEmailRequestModel> requestModels)
        {
            return await _emailNotificationService.AddUpdateBusinessNotificationEmail(requestModels);
        }
        public async Task<BusinessNotificationEmailResponse> GetBusinessNotificationEmailById(Guid idGuid)
        {
            return await _emailNotificationService.GetBusinessNotificationEmailById(idGuid);
        }
        public async Task<BusinessNotificationEmailListResponse> GetBusinessNotificationEmails(int businessId, int? notificationEmailId = null, int? category = null, int? notificationType = null)
        {
            return await _emailNotificationService.GetBusinessNotificationEmails(businessId, notificationEmailId, category, notificationType);
        }
        public async Task<BaseResponse> UpdateBusinessNotificationEmailStatus(Guid idGuid, bool active)
        {
            return await _emailNotificationService.UpdateBusinessNotificationEmailStatus(idGuid, active);
        }
        #region CP Notification Email
        public async Task<AddNotificationEmailResponse> AddCPNotificationEmailTemplate(AddCPNotificationEmailRequestModel requestModel)
        {
            return await _emailNotificationService.AddCPNotificationEmailTemplate(requestModel);
        }
        public async Task<AddNotificationEmailResponse> UpdateCPNotificationEmailTemplate(UpdateCPNotificationEmailRequestModel requestModel)
        {
            return await _emailNotificationService.UpdateCPNotificationEmailTemplate(requestModel);
        }
        #endregion CP Notification Email
    }
}
