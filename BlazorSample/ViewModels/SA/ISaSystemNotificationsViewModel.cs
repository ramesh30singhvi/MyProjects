using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public interface ISaSystemNotificationsViewModel
    {
        Task<CPNotificationListResponse> GetCPNotificationListAsync(int? id, Guid? idGuid, int? notificationType);
        Task<CPNotificationResponse> AddUpdateCPNotificationAsync(CPNotificationRequestModel model);
        Task<BaseResponse> SendPreviewEmailToBusQueue(PreviewEmailRequestModel requestModel);
    }
}
