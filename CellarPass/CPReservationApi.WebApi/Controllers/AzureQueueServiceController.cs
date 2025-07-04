using CPReservationApi.Common;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.Services;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using static CPReservationApi.Common.Email;

namespace CPReservationApi.WebApi.Controllers
{
    /// <summary>
    /// azurequeueservice controller
    /// </summary>
    [Produces("application/json")]
    [Route("api/azurequeueservice")]
    public class AzureQueueServiceController : BaseController
    {

        private static IOptions<AppSettings> _appSetting;
        /// <summary>
        /// Appsettings
        /// </summary>
        /// <param name="appSetting"></param>
        public AzureQueueServiceController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }
        AuthMessageSender objEmailSend = new AuthMessageSender();
        QueueService getStarted = new QueueService();

        /// <summary>
        /// This method is used to add messages into azure storage queue service
        /// </summary>
        /// <param name="queuemodel"></param>
        /// <returns></returns>
        [Route("addqueuemessage")]
        [HttpPost]
        public IActionResult AddMessageintoQueue([FromBody]Queue queuemodel)
        {
            BaseResponse model = new BaseResponse();
            try
            {
                AppSettings _appsettings = _appSetting.Value;
                getStarted.AddMessageIntoQueue(_appsettings, queuemodel.QueueData).Wait();
                if (!string.IsNullOrEmpty(queuemodel.QueueData))
                {
                    model.success = true;
                }
                else
                {
                    model.success = true;
                    model.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    model.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError.ToString();
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "AddMessageintoQueue:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(model);
        }

        [Route("addqueuemessage2")]
        [HttpPost]
        public IActionResult AddMessageintoQueue2([FromBody] EmailQueue queue)
        {
            BaseResponse model = new BaseResponse();
            try
            {
                AppSettings _appsettings = _appSetting.Value;
                var qData = JsonConvert.SerializeObject(queue);
                getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError.ToString();
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "AddMessageintoQueue:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(model);
        }

        /// <summary>
        /// This method is used to read messages from azure storage service
        /// </summary>
        /// <returns></returns>
        //[Route("readqueuemessage")]
        //[HttpGet]
        //public async Task<IActionResult> ReadMessagefromQueue()
        //{
        //    BaseResponse model = new BaseResponse();
        //    try
        //    {
        //        ReservationEmailModel rsvpmodel = new ReservationEmailModel();
        //        rsvpmodel.MailConfig.Domain = _appSetting.Value.MailGunPostUrl;
        //        rsvpmodel.MailConfig.ApiKey = _appSetting.Value.MainGunApiKey;
        //        rsvpmodel.MailConfig.StorageConnectionString = _appSetting.Value.StorageConnectionString;
        //        await Task.Run(() => getStarted.GetMessageFromQueue(rsvpmodel).Wait());
        //        model.success = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
        //        model.error_info.extra_info = Common.Common.InternalServerError.ToString();
        //        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
        //        logDAL.InsertLog("WebApi", "ReadMessagefromQueue:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"]);
        //    }
        //    return new ObjectResult(model);
        //}

        /// <summary>
        ///  This method is used for processing queue data 
        /// </summary>
        /// <param name="rsvpmodel">Queue data for email processing</param>
        /// <returns></returns>
        [Route("processqueuemessage")]
        [HttpPost]
        public async Task<IActionResult> ProcessQueueMessage([FromBody]EmailQueue queue)
        {
            BaseResponse model = new BaseResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            try
            {
                //var data = new EmailQueue();
                //data.BCode = "YBLWBYFM";
                //data.UId = 578676;
                //data.RsvpId = 931721;
                //data.GuestEmail = true;
                //data.AdminEmail = true;
                //var rsvpmodel = new ReservationEmailModel(); 
                //var dt = JsonConvert.SerializeObject(rsvpmodel.data);
                //var strJson = data.ToString();

                getStarted = new QueueService(_appSetting);
                //logDAL.InsertLog("WebApi", "ProcessQueueMessage data 11:" + JsonConvert.SerializeObject(queue), "", 3, 0);

                if ((EmailType)queue.EType == EmailType.EventsUpdateRequest)
                {
                    Model.EventsUpdateRequest eventsUpdateRequest = JsonConvert.DeserializeObject<Model.EventsUpdateRequest>(queue.PerMsg);

                    logDAL.InsertLog("WebApi", "ProcessQueueMessage data:" + JsonConvert.SerializeObject(eventsUpdateRequest), "", 3, 0);

                    if (eventsUpdateRequest != null && eventsUpdateRequest.EventId > 0)
                    {
                        logDAL.InsertLog("WebApi", "ProcessQueueMessage data  22:" + JsonConvert.SerializeObject(eventsUpdateRequest), "", 3, 0);
                        EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                        eventDAL.EventsUpdate(eventsUpdateRequest);
                    }
                }
                else
                {
                    ReservationEmailModel rsvpmodel = new ReservationEmailModel();
                    rsvpmodel.EType = queue.EType;
                    rsvpmodel.MailConfig.Domain = _appSetting.Value.MailGunPostUrl;
                    rsvpmodel.MailConfig.ApiKey = _appSetting.Value.MainGunApiKey;
                    rsvpmodel.RsvpId = queue.RsvpId;
                    rsvpmodel.UId = queue.UId;

                    rsvpmodel.data = new ReservationEmailModel();
                    rsvpmodel.data.BCode = queue.BCode;
                    rsvpmodel.data.UId = queue.UId;
                    rsvpmodel.data.RsvpId = queue.RsvpId;
                    rsvpmodel.data.GuestEmail = queue.GuestEmail;
                    rsvpmodel.data.AdminEmail = queue.AdminEmail;
                    rsvpmodel.data.SendAffiliateEmail = queue.AffiliateEmail;
                    rsvpmodel.data.perMsg = queue.PerMsg;
                    rsvpmodel.data.RefundAmount = queue.Ramt;
                    rsvpmodel.data.ActionSource = queue.Src;
                    rsvpmodel.data.isRsvpType = queue.ActionType;
                    rsvpmodel.data.CCGuestEmail = queue.AlternativeEmail;

                    rsvpmodel.BCode = queue.BCode;
                    rsvpmodel.perMsg = queue.PerMsg;
                    rsvpmodel.RefundAmount = queue.Ramt;
                    rsvpmodel.ActionSource = queue.Src;
                    rsvpmodel.isRsvpType = queue.ActionType;
                    rsvpmodel.SendAffiliateEmail = queue.AffiliateEmail;
                    rsvpmodel.GuestEmail = queue.GuestEmail;
                    rsvpmodel.CCGuestEmail = queue.AlternativeEmail;

                    await Task.Run(() => getStarted.EmailSend(rsvpmodel, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken).Wait());
                }
                
                model.success = true;
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError.ToString();
                model.error_info.description = ex.Message.ToString();
                
                logDAL.InsertLog("WebApi", "ProcessMessagefromQueue:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(model);
        }

        [Route("processmessagefrommailgun")]
        [HttpPost]
        public async Task<IActionResult> ProcessMessageFromMailgun([FromBody]MailGunWebhookRequest req)
        {
            BaseResponse model = new BaseResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            try
            {
                Email.EmailStatus emailStatus = Email.EmailStatus.na;
                Email.EmailType emailType = Email.EmailType.NA;
                string mailgunEvent = "";
                int referenceId = 0;
                string emailRecipient = "";
                string emailLogNote = "";
                int emailContentId = 0;

                mailgunEvent = req.Event;
                referenceId = req.CPId;
                emailContentId = req.CPEmailContentId;
                emailRecipient = req.Recipient;
                emailLogNote = req.CPMailTypeDesc;
                emailType = req.CPMailType;

                if (mailgunEvent.ToLower() == "delivered") //Delivered messages
                {
                    emailStatus = Email.EmailStatus.delivered;
                    //emailLogNote = "Delivered";
                }
                else if (mailgunEvent.ToLower() == "bounced") //Hard bounces
                {
                    emailStatus = Email.EmailStatus.hardbounce;
                }
                else if (mailgunEvent.ToLower() == "dropped") //Dropped messages
                {
                    emailStatus = Email.EmailStatus.dropped;
                }
                else if (mailgunEvent.ToLower() == "complained") //Spam complaints 
                {
                    emailStatus = Email.EmailStatus.complained;
                    //emailLogNote = "Spam Complaint";
                }
                else if (mailgunEvent.ToLower() == "unsubscribed")
                {
                    emailStatus = Email.EmailStatus.unsubscribed;
                    //emailLogNote = "Unsubscribed";
                }
                else if (mailgunEvent.ToLower() == "clicked")
                {
                    emailStatus = Email.EmailStatus.clicked;
                    //emailLogNote = "Clicked";
                }
                else if (mailgunEvent.ToLower() == "opened")
                {
                    emailStatus = Email.EmailStatus.opened;
                    //emailLogNote = "Opened";
                }

                if (emailStatus != EmailStatus.na)
                {
                    EmailServiceDAL emailDAL = new EmailServiceDAL(Common.Common.ConnectionString);

                    EmailLogModel emailLog = new EmailLogModel();
                    emailLog.RefId = referenceId;
                    emailLog.EmailType = (int)emailType;
                    emailLog.EmailProvider = (int)Common.Email.EmailProvider.Mailgun;
                    emailLog.EmailStatus = (int)emailStatus;
                    emailLog.EmailSender = "";
                    emailLog.EmailRecipient = emailRecipient;
                    emailLog.LogNote = emailLogNote ?? "";
                    emailLog.LogDate = DateTime.UtcNow;
                    emailLog.EmailContentId = emailContentId;

                    emailDAL.SaveEmailLog(emailLog);
                }

                model.success = true;
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError.ToString();
                model.error_info.description = ex.Message.ToString();

                logDAL.InsertLog("WebApi", "ProcessMessageFromMailgun:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(model);
        }
    }
}