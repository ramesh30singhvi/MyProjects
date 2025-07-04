using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class MessageLogService: IMessageLogService
    {
        private readonly IApiManager _apiManager;

        public MessageLogService(IApiManager apiManager)
        {
            _apiManager = apiManager;
        }
        public async Task<MessageLogResult> SendSMSMessageAsync(MessageLogModel model)
        {
            try
            {
                MessageLogResult receiptSetting = await _apiManager.PostAsync<MessageLogModel, MessageLogResult>("Message/sendsmsmessage", model);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new MessageLogResult();
            }
        }
        public async Task<List<SeatStatusModel>> GetTableStatusAsync(int floorplan_id, string start_date_time)
        {
            try
            {
                List<SeatStatusModel> receiptSetting = await _apiManager.GetAsync<List<SeatStatusModel>>("Message/gettablestatus?floorplan_id="+ floorplan_id+ "&start_date_time="+ start_date_time);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            { 
           
                Debug.WriteLine(e.HttpCode);
                return new List<SeatStatusModel>();
            }
        }
    }
}
