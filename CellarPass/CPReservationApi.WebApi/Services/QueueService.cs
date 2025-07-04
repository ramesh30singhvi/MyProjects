using Azure.Storage.Queues;
using CPReservationApi.Common;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using static CPReservationApi.Common.Email;

namespace CPReservationApi.WebApi.Services
{
    /// <summary>
    /// Create queue, insert message, read message, get queue length, delete queue.
    /// </summary>
    public class QueueService
    {
        public static IOptions<AppSettings> _appSetting;
        public QueueService(IOptions<AppSettings> appSetting)
        {
            _appSetting = appSetting;
        }

        public QueueService()
        {

        }
        private readonly string queueName = "emailqueue";
        /// <summary>
        /// Create a queue for the sample application to process messages in. 
        /// </summary>
        /// <returns>A QueueClient object</returns>
        public async Task<QueueClient> CreateQueueAsync(string queueName, string StorageConnectionString)
        {
 
            QueueClient queue = new QueueClient(StorageConnectionString, queueName, new QueueClientOptions {MessageEncoding = QueueMessageEncoding.Base64 });
            try
            {
                //queue = new QueueClient(StorageConnectionString, queueName);
                await queue.CreateIfNotExistsAsync();
            }
            catch
            {
                // If you are running with the default configuration please make sure you have started the storage emulator.  ess the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.
                throw;
            }

            return queue;
        }

        /// <summary>
        /// Create a queue for the sample application to process messages in. 
        /// </summary>
        /// <returns>A CloudQueue object</returns>
        public async Task AddMessageIntoQueue(AppSettings appsettings, string data)
        {
            EmailServiceDAL emailDAL = new EmailServiceDAL(Common.Common.ConnectionString);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            var queueModel = new EmailQueue();
            queueModel = JsonConvert.DeserializeObject<EmailQueue>(data);

            try
            {
                if (queueModel.EType == (int)EmailType.Rsvp && queueModel.GuestEmail)
                {
                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                    var reservationData = eventDAL.GetReservationEmailDataByReservationId(queueModel.RsvpId, queueModel.UId, queueModel.BCode);

                    if (reservationData.GuestEmail.IndexOf("@noemail") == -1)
                    {
                        EmailLogModel emailLog = new EmailLogModel();
                        emailLog.RefId = queueModel.RsvpId;
                        emailLog.EmailType = (int)EmailType.Rsvp;
                        emailLog.EmailProvider = (int)Common.Email.EmailProvider.Mailgun;
                        emailLog.EmailStatus = (int)EmailStatus.added;
                        emailLog.EmailSender = "";
                        emailLog.EmailRecipient = reservationData.GuestEmail;
                        emailLog.LogNote = "Added. Thank you.";
                        emailLog.LogDate = DateTime.UtcNow;
                        emailLog.MemberId = reservationData.WineryID;
                        emailLog.EmailContentId = reservationData.EmailContentID;

                        emailDAL.SaveEmailLog(emailLog);

                        logDAL.InsertLog("WebApi", "AddMessageIntoQueue data:" + data, "", 3, reservationData.WineryID);
                    }
                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "AddMessageIntoQueue data:" + data + ",Message:" + ex.Message.ToString(), "",1, 0);
            }

            // Create or reference an existing queue.
            //CloudQueue queue = CreateQueueAsync(appsettings.QueueName, appsettings.StorageConnectionString).Result;
            //Reservations model = new Reservations();
            //model.EmailType = (int)Email.EmailType.Rsvp;
            //model.data.RefundAmount = 20;
            //model.data.ReservationId = 5;
            //model.data.RsvpType = 4;
            //string json = JsonConvert.SerializeObject(model);
            //await queue.AddMessageAsync(new CloudQueueMessage(data));

            if (queueModel.EType == (int)EmailType.CreateThirdPartyContact || queueModel.EType == (int)EmailType.GoogleCalendar || queueModel.EType == (int)EmailType.UploadOrderTobLoyal || queueModel.EType == (int)EmailType.MailChimpOrder || queueModel.EType == (int)EmailType.CPMailChimpOrder || queueModel.EType == (int)EmailType.AbanondedCartRsvp || queueModel.EType == (int)EmailType.AbanondedCartTickets || queueModel.EType == (int)EmailType.RsvpExport || queueModel.EType == (int)EmailType.EventsUpdateRequest)
            {
                logDAL.InsertLog("WebApi", "AddMessageIntoQueue data " + queueModel.EType.ToString() + ":" + data, "", 3, 0);
                QueueClient queue = CreateQueueAsync(appsettings.QueueName, appsettings.StorageConnectionString).Result;
                //await queue.AddMessageAsync(new CloudQueueMessage(data));

                await queue.SendMessageAsync(data);
            }   
            else
            {
                logDAL.InsertLog("WebApi", "AddMessageIntoQueue data:" + data, "", 3, 0);

                ServiceBusQueueService service = new ServiceBusQueueService();
                await service.SendMessage(appsettings, data);

                if (appsettings.QueueName.IndexOf("dev") > -1)
                {
                    ReservationEmailModel rsvpmodel = new ReservationEmailModel();
                    rsvpmodel.EType = queueModel.EType;
                    rsvpmodel.MailConfig.Domain = appsettings.MailGunPostUrl;
                    rsvpmodel.MailConfig.ApiKey = appsettings.MainGunApiKey;
                    rsvpmodel.RsvpId = queueModel.RsvpId;
                    rsvpmodel.UId = queueModel.UId;

                    rsvpmodel.data = new ReservationEmailModel();
                    rsvpmodel.data.BCode = queueModel.BCode;
                    rsvpmodel.data.UId = queueModel.UId;
                    rsvpmodel.data.RsvpId = queueModel.RsvpId;
                    rsvpmodel.data.GuestEmail = queueModel.GuestEmail;
                    rsvpmodel.data.AdminEmail = queueModel.AdminEmail;
                    rsvpmodel.data.SendAffiliateEmail = queueModel.AffiliateEmail;
                    rsvpmodel.data.perMsg = queueModel.PerMsg;
                    rsvpmodel.data.RefundAmount = queueModel.Ramt;
                    rsvpmodel.data.ActionSource = queueModel.Src;
                    rsvpmodel.data.isRsvpType = queueModel.ActionType;

                    rsvpmodel.perMsg = queueModel.PerMsg;
                    rsvpmodel.RefundAmount = queueModel.Ramt;
                    rsvpmodel.ActionSource = queueModel.Src;
                    rsvpmodel.isRsvpType = queueModel.ActionType;
                    rsvpmodel.SendAffiliateEmail = queueModel.AffiliateEmail;
                    rsvpmodel.GuestEmail = queueModel.GuestEmail;
                    await Task.Run(() => EmailSend(rsvpmodel));
                }

                /*****Commented below code to introduce Azure Service queue***********
                ReservationEmailModel rsvpmodel = new ReservationEmailModel();
                rsvpmodel.EType = queueModel.EType;
                rsvpmodel.MailConfig.Domain = appsettings.MailGunPostUrl;
                rsvpmodel.MailConfig.ApiKey = appsettings.MainGunApiKey;
                rsvpmodel.RsvpId = queueModel.RsvpId;
                rsvpmodel.UId = queueModel.UId;

                rsvpmodel.data = new ReservationEmailModel();
                rsvpmodel.data.BCode = queueModel.BCode;
                rsvpmodel.data.UId = queueModel.UId;
                rsvpmodel.data.RsvpId = queueModel.RsvpId;
                rsvpmodel.data.GuestEmail = queueModel.GuestEmail;
                rsvpmodel.data.AdminEmail = queueModel.AdminEmail;
                rsvpmodel.data.SendAffiliateEmail = queueModel.AffiliateEmail;
                rsvpmodel.data.perMsg = queueModel.PerMsg;
                rsvpmodel.data.RefundAmount = queueModel.Ramt;
                rsvpmodel.data.ActionSource = queueModel.Src;
                rsvpmodel.data.isRsvpType = queueModel.ActionType;

                rsvpmodel.perMsg = queueModel.PerMsg;
                rsvpmodel.RefundAmount = queueModel.Ramt;
                rsvpmodel.ActionSource = queueModel.Src;
                rsvpmodel.isRsvpType = queueModel.ActionType;
                rsvpmodel.SendAffiliateEmail = queueModel.AffiliateEmail;
                rsvpmodel.GuestEmail = queueModel.GuestEmail;
                await Task.Run(() => EmailSend(rsvpmodel));
                *************************************************/
                
            }
        }

        /// <summary>
        /// Send email by email type
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<EmailResponse> EmailSend(ReservationEmailModel model,string ShopifyUrl = "", string ShopifyAuthToken = "")
        {
            var response = new EmailResponse();
            AuthMessageSender messageService = new AuthMessageSender();
            //await Task.Run(() => messageService.SendTRSVPEmail(model));
            switch (model.EType)
            {
                case (int)Email.EmailType.Rsvp:
                    response = await Task.Run(() => messageService.SendTRSVPEmail(model));
                    break;
                case (int)Email.EmailType.RsvpUpdate:
                    response = await Task.Run(() => messageService.SendEmailOnReservationUpdate(model));
                    break;
                case (int)Email.EmailType.RsvpCancel:
                    response = await Task.Run(() => messageService.SendCpRsvpCancelEmailV2(model));
                    break;
                case (int)Email.EmailType.RsvpReminder:
                    response = await Task.Run(() => messageService.SendCpRsvpReminderEmailV2(model));
                    break;
                case (int)Email.EmailType.NoShowNotice:
                    response = await Task.Run(() => messageService.SendCpRsvpNoShowEmailV2(model));
                    break;
                case (int)Email.EmailType.AbanondedCartRsvp:
                    response = await Task.Run(() => messageService.ProcessAbandonedCartRsvpEmail(model));
                    break;
                case (int)Email.EmailType.AbanondedCartTickets:
                    response = await Task.Run(() => messageService.ProcessAbandonedCartTicketsEmail(model));
                    break;
                case (int)Email.EmailType.WaitListNotification:
                    response = await Task.Run(() => messageService.ProcessWaitlistNotificationEmail(model));
                    break;
                case (int)Email.EmailType.WaitListCancellation:
                    response = await Task.Run(() => messageService.ProcessWaitListCancellationEmail(model));
                    break;
                case (int)Email.EmailType.TicketSale:
                    response = await Task.Run(() => messageService.ProcessSendCpRsvpTicketSaleEmail(model));
                    break;
                case (int)Email.EmailType.TicketPostCapture:
                    response = await Task.Run(() => messageService.ProcessSendCpTicketPostCaptureEmail(model));
                    break;
                case (int)Email.EmailType.TicketWaitlistOffer:
                    response = await Task.Run(() => messageService.SendCpTicketWaitlistOfferEmail(model));
                    break;
                case (int)Email.EmailType.SysContactEventOrganizer:
                    response = await Task.Run(() => messageService.SendContactEventOrganizerEmail(model));
                    break;
                case (int)Email.EmailType.RsvpTicketSalesConfirmation:
                    response = await Task.Run(() => messageService.ProcessTicketSalesConfirmationEmail(model));
                    break;
                case (int)Email.EmailType.EventReviewInvitation:
                    response = await Task.Run(() => messageService.ProcessEventReviewInviteEmail(model));
                    break;
                case (int)Email.EmailType.MailChimpOrder:
                    response = await Task.Run(() => messageService.ProcessCreateMailChimpOrder(model));
                    break;
                case (int)Email.EmailType.MailChimpTicketOrder:
                    response = await Task.Run(() => messageService.ProcessCreateMailChimpTicketOrder(model));
                    break;
                case (int)Email.EmailType.CreateThirdPartyContact:
                    response = await Task.Run(() => messageService.ProcessCreateThirdPartyContact(model,ShopifyUrl,ShopifyAuthToken));
                    break;
                case (int)Email.EmailType.UploadOrderTobLoyal:
                    response = await Task.Run(() => messageService.ProcessUploadOrderTobLoyal(model));
                    break;
                case (int)Email.EmailType.SysSubscriptionSignup:
                    response = await Task.Run(() => messageService.ProcessBusinessSubscriptionEmail(model));
                    break;
                //case (int)Email.EmailType.GoogleCalendar:
                //    response = await Task.Run(() => messageService.ProcessGoogleCalendar(model));
                //    break;
                case (int)Email.EmailType.PrivateEventRequest:
                    response = await Task.Run(() => messageService.ProcessPrivateBookingRequest(model));
                    break;
                case (int)Email.EmailType.TicketEventFollowingReminder:
                    response = await Task.Run(() => messageService.ProcessSendEventFollowingReminder(model));
                    break;
                case (int)Email.EmailType.SysNewAdminUser:
                    response = await Task.Run(() => messageService.ProcessSendSysNewAdminUser(model));
                    break;
                case (int)Email.EmailType.InviteReminder:
                    response = await Task.Run(() => messageService.SendReservationInviteReminder(model));
                    break;
                case (int)Email.EmailType.RsvpExport:
                    response = await Task.Run(() => messageService.ProcessSendExportReservations(model));
                    break;
                case (int)Email.EmailType.TicketedEventEndedNotification:
                    response = await Task.Run(() => messageService.ProcessSendTicketedEventEndedNotification(model));
                    break;
                case (int)Email.EmailType.ReservationChangesUpdate:
                    response = await Task.Run(() => messageService.ProcessReservationChangesUpdate(model));
                    break;
                default:
                    break;
            }
            return response;
        }
    }
}
