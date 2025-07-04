using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IEmailNotificationViewModel
    {
        Task<GetEmailNotificationResponse> GetEmailNotificationByCategoryId(int categoryId, int businessId);
        Task<GetEmailNotificationDetailResponse> GetEmailNotificationDetail(Guid? idGUID = null, int? notificationType = null, int? categoryId = null);
        Task<BusinessNotificationEmailListResponse> AddUpdateBusinessNotificationEmail(List<AddUpdateBusinessEmailRequestModel> requestModels);
        Task<BusinessNotificationEmailResponse> GetBusinessNotificationEmailById(Guid idGuid);
        Task<BusinessNotificationEmailListResponse> GetBusinessNotificationEmails(int businessId, int? notificationEmailId = null, int? category = null, int? notificationType = null);
        Task<BaseResponse> UpdateBusinessNotificationEmailStatus(Guid idGuid, bool active);
        Task<AddNotificationEmailResponse> AddCPNotificationEmailTemplate(AddCPNotificationEmailRequestModel requestModel);
        Task<AddNotificationEmailResponse> UpdateCPNotificationEmailTemplate(UpdateCPNotificationEmailRequestModel requestModel);
    }
}
