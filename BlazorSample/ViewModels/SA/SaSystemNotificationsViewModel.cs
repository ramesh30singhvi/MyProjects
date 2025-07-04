using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public class SaSystemNotificationsViewModel : ISaSystemNotificationsViewModel
    {
        private readonly ISaSystemNotificationsService _saSystemNotificationsService;
        public SaSystemNotificationsViewModel(ISaSystemNotificationsService saSystemNotificationsService)
        {
            _saSystemNotificationsService = saSystemNotificationsService;
        }
        public async Task<CPNotificationListResponse> GetCPNotificationListAsync(int? id, Guid? idGuid, int? notificationType)
        {
            return await _saSystemNotificationsService.GetCPNotificationListAsync(id, idGuid, notificationType);
        }
        public async Task<CPNotificationResponse> AddUpdateCPNotificationAsync(CPNotificationRequestModel model)
        {
            return await _saSystemNotificationsService.AddUpdateCPNotificationAsync(model);
        }
        public async Task<BaseResponse> SendPreviewEmailToBusQueue(PreviewEmailRequestModel requestModel)
        {
            return await _saSystemNotificationsService.SendPreviewEmailToBusQueue(requestModel);
        }
    }
}
