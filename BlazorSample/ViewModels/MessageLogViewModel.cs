using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class MessageLogViewModel:IMessageLogViewModel
    {
        private IMessageLogService _messageLogService;
        public MessageLogViewModel(IMessageLogService messageLogService)
        {
            _messageLogService = messageLogService;
        }
        public async Task<MessageLogResult> SendSMSMessage(MessageLogModel model)
        {
            MessageLogResult messageLog = await _messageLogService.SendSMSMessageAsync(model);
            return messageLog;
        }
        public async Task<List<SeatStatusModel>> GetTableStatus(int floorplan_id, string start_date_time)
        {
            List<SeatStatusModel> messageLog = await _messageLogService.GetTableStatusAsync(floorplan_id, start_date_time);
            return messageLog;
        }
    }
}
