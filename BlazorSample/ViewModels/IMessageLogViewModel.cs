using CellarPassAppAdmin.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IMessageLogViewModel
    {
        public Task<MessageLogResult> SendSMSMessage(MessageLogModel model);
        public Task<List<SeatStatusModel>> GetTableStatus(int floorplan_id, string start_date_time);
    }
}
