using CPReservationApi.Common;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.ViewModels;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static CPReservationApi.Common.Common;
using static CPReservationApi.Common.Email;
using static CPReservationApi.Common.Payments;
using Microsoft.Extensions.Options;
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Engines.Chrome;
using Microsoft.CodeAnalysis.CSharp;

namespace CPReservationApi.WebApi.Services
{

    /// <summary>
    /// This class is used by the application to send Email
    /// </summary>
    public class AuthMessageSender
    {
        //static private AppSettings _appSettings;
        //public AuthMessageSender(IOptions<AppSettings> settings)
        //{
        //    _appSettings = settings.Value;
        //}
        private const string _IronPDFLicenseKey = "IRONPDF.CELLARPASS.IX7218-C60350CBBA-DU4XAJLR4A-VRLE6JRENC36-Y2WAQ2ODPV5U-GFYILTHDTGLI-FAER2LTEQTOU-TFBR5N-LAJ2C555BGONEA-IRO230803.2800.42128.IRONPDF.DOTNET.LITE.SUB-U6SXGA.RENEW.SUPPORT.03.AUG.2024";
        public static IOptions<AppSettings> _appSetting;
        public AuthMessageSender(IOptions<AppSettings> appSetting)
        {
            _appSetting = appSetting;
        }

        public AuthMessageSender()
        {

        }

        /// <summary>
        /// This method check valid email and save email log
        /// </summary>
        /// <param name="emailType"></param>
        /// <param name="referenceId"></param>
        /// <param name="fromEmail"></param>
        /// <param name="toEmail"></param>
        /// <param name="subject"></param>
        /// <param name="bodyHtml"></param>
        /// <param name="memberId"></param>
        /// <param name="attachement"></param>
        /// <returns></returns>
        public async Task<EmailResponse> SendEmailAndSaveEmailLog(MailConfig config, EmailType emailType, int referenceId, string fromEmail, string toEmail, string subject, string bodyHtml, int memberId, EmailAttachment attachement, int emailContentId, String replyTo = "", String fromName = "")
        {
            LogDAL logDAL = new LogDAL(ConnectionString);
            var response = new EmailResponse();

            if (toEmail.Length > 0)
            {
                //if (replyTo.Length > 0)
                //    fromEmail = replyTo;

                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                string PreMessageContent = string.Empty; //"** Please do not reply to this automated message. The mailbox is not monitored.**";

                string messageContent = PreMessageContent + "<br><br>" + bodyHtml;

                //GLOBAL TAGS - If these tags are found in an message content they will be repaces
                //"[[CurrentYear]]"
                //"[[CopyrightMsg]]"

                string currentYear = DateTime.UtcNow.Year.ToString();
                string copyrightMsg = string.Format("&copy;2009 - {0} CellarPass, Inc. All Rights Reserved", currentYear);

                currentYear = Times.ToTimeZoneTime(DateTime.UtcNow).Year.ToString();

                SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);

                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)SettingGroup.system);
                string savedSetting = Settings.GetStrValue(settingsGroup, SettingKey.system_copyright_msg);

                if (!string.IsNullOrEmpty(savedSetting))
                {
                    copyrightMsg = savedSetting;
                }

                //Subject
                subject = subject.Replace("[[CurrentYear]]", currentYear);
                subject = subject.Replace("[[CopyrightMsg]]", "");

                //Body
                messageContent = messageContent.Replace("[[CurrentYear]]", currentYear);
                messageContent = messageContent.Replace("[[CopyrightMsg]]", copyrightMsg);

                //To Email List
                List<string> toEmailList = new List<string>();

                //is email valid
                bool invalidEmail = false;

                //Check for valid to email(s)
                if ((toEmail != null))
                {
                    if (!(toEmail.Trim() == string.Empty))
                    {
                        //If comma found split 
                        if (toEmail.IndexOf(",") > 0)
                        {
                            string[] eList = toEmail.Split(',');
                            foreach (string email in eList)
                            {
                                if (email.Trim().Length > 0)
                                {
                                    if (email.IndexOf("@noemail") == -1)
                                    {
                                        if (Email.EmailIsValid(email))
                                        {
                                            toEmailList.Add(email);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Just add single email
                            if (toEmail.IndexOf("@noemail") == -1)
                            {
                                if (Email.EmailIsValid(toEmail))
                                {
                                    toEmailList.Add(toEmail);
                                }
                            }
                        }
                    }
                    else
                    {
                        invalidEmail = true;
                    }
                }
                else
                {
                    invalidEmail = true;
                }

                if (invalidEmail == true)
                {
                    //Log Email
                    if (!(emailType == Email.EmailType.NA))
                    {
                        EmailLogModel emailLog = new EmailLogModel();
                        emailLog.RefId = referenceId;
                        emailLog.EmailType = (int)emailType;
                        emailLog.EmailProvider = (int)Common.Email.EmailProvider.Mailgun;
                        emailLog.EmailStatus = (int)EmailStatus.cpInvalid;
                        emailLog.EmailSender = fromEmail;
                        emailLog.EmailRecipient = Common.Common.Left(toEmail == null ? "" : toEmail, 60);
                        emailLog.LogNote = "Email is invalid or missing";
                        emailLog.LogDate = DateTime.UtcNow;
                        emailLog.MemberId = memberId;
                        emailLog.EmailContentId = emailContentId;

                        emailDAL.SaveEmailLog(emailLog);
                    }
                }
                else
                {
                    try
                    {
                        if ((toEmailList != null))
                        {
                            //Send email to each in list
                            foreach (var strToEmail in toEmailList)
                            {
                                response = await SendMailAsync(fromEmail, strToEmail, subject, messageContent, attachement, (int)emailType, "", referenceId, emailContentId, replyTo, fromName);

                                //Log Email
                                if ((response != null))
                                {
                                    EmailLogModel emailLog = new EmailLogModel();
                                    emailLog.RefId = referenceId;
                                    emailLog.EmailType = (int)emailType;
                                    emailLog.EmailProvider = (int)Common.Email.EmailProvider.Mailgun;
                                    emailLog.EmailStatus = (int)response.emailStatus;
                                    emailLog.EmailSender = fromEmail;
                                    emailLog.EmailRecipient = Common.Common.Left(strToEmail, 60);
                                    emailLog.LogNote = response.message ?? "";
                                    emailLog.LogDate = DateTime.UtcNow;
                                    emailLog.EmailContentId = emailContentId;

                                    emailDAL.SaveEmailLog(emailLog);

                                    response.emailSent = true;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logDAL.InsertLog("WebApi", "SendEmailAndSaveEmailLog:  " + ex.Message.ToString(), "", 1, memberId);
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// This method is used for sending email from mailgun
        /// </summary>
        /// <param name="config"></param>
        /// <param name="fromEmail"></param>
        /// <param name="toEmail"></param>
        /// <param name="subject"></param>
        /// <param name="bodyHtml"></param>
        /// <param name="attachement"></param>
        /// <returns></returns>
        public async Task<EmailResponse> SendMailAsync(string fromEmail, string toEmail, string subject, string bodyHtml, EmailAttachment attachement, int emailType, string emailTypeDesc, int referenceId, int emailContentId, String replyTo = "", String fromName = "")
        {
            var response = new EmailResponse();

            string emailDomain = fromEmail.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[1].ToString();

            MailConfig mailConfig = new MailConfig();

            SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);


            List<Settings.Setting> settingsGroup = new List<Settings.Setting>();
            if ((EmailType)emailType == EmailType.TicketEventInvite)
            {
                settingsGroup = settingsDAL.GetSettingGroup(0, (int)SettingGroup.mailgun).ToList();
                mailConfig.ApiKey = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_02_key);
                mailConfig.Domain = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_02_domain);
            }
            else
            {
                settingsGroup = settingsDAL.GetSettingGroup(0, (int)SettingGroup.mailgun).OrderBy(x => x.Key).ToList();

                if (settingsGroup != null)
                {
                    if (settingsGroup.Count > 0)
                    {
                        int settingGrp = 0;

                        if (toEmail.ToLower() != "sales@cellarpass.com")
                        {
                            foreach (var item in settingsGroup)
                            {
                                if (item.Value.Trim().ToLower().Contains("cellarpass.com"))
                                    settingGrp = settingGrp + 1;

                                if (item.Value.Trim().ToLower() == emailDomain.Trim().ToLower())
                                    break;
                            }

                            if (settingGrp == 0)
                                settingGrp = 1;
                        }
                        else
                        {
                            settingGrp = 1;
                        }

                        switch ((MailgunDomain)settingGrp)
                        {
                            case MailgunDomain.SystemMessages:
                                mailConfig.ApiKey = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_01_key);
                                mailConfig.Domain = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_01_domain);
                                break;
                            case MailgunDomain.Invitations:
                                mailConfig.ApiKey = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_02_key);
                                mailConfig.Domain = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_02_domain);
                                break;
                            case MailgunDomain.Reservations:
                                mailConfig.ApiKey = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_03_key);
                                mailConfig.Domain = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_03_domain);
                                break;
                            case MailgunDomain.Ticketing:
                                mailConfig.ApiKey = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_04_key);
                                mailConfig.Domain = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_04_domain);
                                break;
                            case MailgunDomain.Reminder:
                                mailConfig.ApiKey = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_05_key);
                                mailConfig.Domain = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_05_domain);
                                break;
                            case MailgunDomain.Notification:
                                mailConfig.ApiKey = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_06_key);
                                mailConfig.Domain = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_06_domain);
                                break;
                            case MailgunDomain.Reviews:
                                mailConfig.ApiKey = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_07_key);
                                mailConfig.Domain = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_07_domain);
                                break;
                            case MailgunDomain.Billing:
                                mailConfig.ApiKey = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_08_key);
                                mailConfig.Domain = Settings.GetStrValue(settingsGroup, SettingKey.mailgun_08_domain);
                                break;
                        }
                    }
                }
            }

            settingsGroup = settingsDAL.GetSettingGroup(0, (int)SettingGroup.MailGunEmailValidation);
            bool savedSetting = Settings.GetBoolValue(settingsGroup, SettingKey.EnableMailGunEmailValidation);

            if (savedSetting)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                //check if email is valid by first checking in local DB
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                bool isValid = true;
                var status = eventDAL.IsValidEmail(toEmail);
                if (status == EmailValidStatus.na)
                {
                    //not found in local DB, check with mailgun
                    isValid = await this.CheckEmailValid(mailConfig, toEmail);
                }
                else
                {
                    isValid = (status == EmailValidStatus.valid);
                }

                if (!isValid)
                {
                    logDAL.InsertLog("WebApi", "SendMail:  " + toEmail + " is invalid email. Cannot send email", "", 3, 0);
                    response.emailSent = false;
                    response.emailStatus = EmailStatus.cpInvalid;
                    response.emailRecipient = toEmail;
                    response.message = "Invalid email address. Email address not deliverable.";
                    return response;
                }
            }

            //From Name?
            //string sendFromName = fromEmail;
            string sendFromName = "";
            if (fromName != "")
            {
                sendFromName = fromName;
            }

            string ApiKey = string.Format("api:{0}", mailConfig.ApiKey);
            string Domain = string.Format("https://api.mailgun.net/v3/{0}/messages", mailConfig.Domain);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(ApiKey))); //mailConfig.ApiKey api:key-3d26662828b5e8d316d926db8e7162fd

                var content = new MultipartFormDataContent();
                content.Add(new StringContent(sendFromName + "<" + fromEmail + ">"), "from");
                content.Add(new StringContent(toEmail), "to");
                content.Add(new StringContent(subject), "subject");
                content.Add(new StringContent(bodyHtml), "html");

                if (!String.IsNullOrEmpty(replyTo))
                    content.Add(new StringContent(replyTo), "h:Reply-To");

                content.Add(new StringContent(string.Format("{{'cpmailtype': {0},'cpmailtypedesc': '{1}','cpid':{2} ,'cpemailcontentid':{3}}}", emailType, emailTypeDesc, referenceId, emailContentId)), "v:cellarpass-data");

                if (attachement != null)
                {
                    if (!string.IsNullOrWhiteSpace(attachement.Contents))
                    {
                        ByteArrayContent fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(attachement.Contents));
                        content.Add(fileContent, "attachment", attachement.Name);
                    }
                    else if (attachement.ContentBytes != null)
                    {
                        ByteArrayContent fileContent = new ByteArrayContent(attachement.ContentBytes);
                        content.Add(fileContent, "attachment", attachement.Name);
                    }

                }
                var apiResponse = await client.PostAsync(new Uri(Domain), content).ConfigureAwait(false);//"https://api.mailgun.net/v3/messages.cellarpass.com/messages"
                string message = await apiResponse.Content.ReadAsStringAsync();

                if (apiResponse.IsSuccessStatusCode)
                {
                    var resp = new MailGunMessagesResponse();
                    resp = JsonConvert.DeserializeObject<MailGunMessagesResponse>(message);

                    response.emailSent = true;
                    response.emailStatus = EmailStatus.accepted;
                    response.emailRecipient = toEmail;
                    response.message = resp.message;
                }

                else
                {
                    response.emailStatus = EmailStatus.failed;
                    response.message = "";
                }
            }
            return response;
        }

        public async Task<bool> ValidateEmail(string email)
        {
            string APIKey = "YXBpOnB1YmtleS04MDFkOTNlNmEyZDUzODA2MmY5N2E4OTRhMDNmMjk5YQ==";
            bool isValid = false;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", APIKey);
                string APIURL = $"https://api.mailgun.net/v3/address/validate?address={email}";
                var response = await client.GetAsync(APIURL);
                var res = await response.Content.ReadAsStringAsync();
                dynamic content = JsonConvert.DeserializeObject(res);

                if (Convert.ToBoolean(content.is_valid))
                {
                    isValid = true;
                }
            }
            
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            EmailValidationLogModel data = new EmailValidationLogModel
            {
                Email = email,
                UserId = 0,
                EmailStatus = isValid ? EmailValidStatus.valid : EmailValidStatus.invalid,
                StatusType = EmailStatusType.validation,
                WebhookEvent = EmailWebhookEvent.na
            };
            eventDAL.AddEmailValidationLog(data);

            return isValid;
        }

        public async Task<bool> CheckEmailValid(MailConfig config, string email, int userId = 0)
        {
            bool isValid = false;
            string apiKey = string.Format("api:{0}", config.ApiKey);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(Encoding.ASCII.GetBytes(apiKey))); //mailConfig.ApiKey => api:key-3d26662828b5e8d316d926db8e7162fd

                    string requestURL = "https://api.mailgun.net/v4/address/validate?address=" + email;

                    var apiResponse = await client.GetAsync(requestURL).ConfigureAwait(false);
                    string message = await apiResponse.Content.ReadAsStringAsync();

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        var resp = new MailGunEmailValidationResponse();
                        resp = JsonConvert.DeserializeObject<MailGunEmailValidationResponse>(message);

                        if (resp.result.ToLower().Equals("deliverable"))
                        {
                            isValid = true;
                        }


                        // Insert into emailvalidation log
                        EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                        EmailValidationLogModel data = new EmailValidationLogModel
                        {
                            Email = email,
                            UserId = userId,
                            EmailStatus = isValid ? EmailValidStatus.valid : EmailValidStatus.invalid,
                            StatusType = EmailStatusType.validation,
                            WebhookEvent = EmailWebhookEvent.na
                        };
                        eventDAL.AddEmailValidationLog(data);

                    }


                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "CheckEmailValid:  " + ex.Message.ToString(), "", 1, 0);
                isValid = true;
            }
            return isValid;
        }

        public static string ExportRsvpV2ToICalendarGuest(ReservationDetailModel rsvp, WineryModel winery, string calendarAddress, string destinationName)
        {
            string SrlText = "";

            try
            {
                var iCal = new Ical.Net.Calendar();

                // Set to version 1.0 to make it compatible with Outlook 2003
                iCal.Version = "2.0";

                var evt = new CalendarEvent
                {
                    Start = new CalDateTime(rsvp.event_start_date),
                    End = new CalDateTime(rsvp.event_end_date),
                };

                // evt.Summary = Winery.DisplayName & " - " & Rsvp.EventName
                evt.Summary = rsvp.event_name + " - " + rsvp.user_detail.last_name + " - " + rsvp.total_guests + (rsvp.total_guests > 1 ? " Guests" : " Guest");
                evt.Location = destinationName.Trim() + " - " + rsvp.location_name;

                // Format Desc
                string desc = "";
                desc = rsvp.event_description.Trim() + @"\n \n ";
                desc += "Booking Code: " + rsvp.booking_code + @"\n ";
                desc += "Guest Name: " + rsvp.user_detail.first_name + " " + rsvp.user_detail.last_name + @"\n ";
                desc += "Guest Phone: " + rsvp.user_detail.phone_number + @"\n ";
                desc += "Guest Email: " + rsvp.user_detail.email + @"\n ";
                desc += "Guest Count: " + rsvp.total_guests + @"\n \n ";

                desc += destinationName.Trim() + @"\n ";
                desc += calendarAddress;
                desc += Utility.FormatPhoneNumber(winery.BusinessPhone.ToString().Length < 8 ? "" : winery.BusinessPhone.ToString()) + @"\n ";
                desc += winery.WebSiteUrl + @"\n \n ";


                //if (!string.IsNullOrWhiteSpace(rsvp.guest_note))
                //{
                //    desc += "Guest Note: " + rsvp.guest_note.Trim() + @"\n \n  ";
                //}

                //if (!string.IsNullOrWhiteSpace(rsvp.internal_note))
                //{
                //    desc += "Internal Note: " + rsvp.internal_note.Trim() + @"\n \n  ";
                //}

                //Add-Ons
                //string addOnItems = "";

                if (rsvp.reservation_addon != null && rsvp.reservation_addon.Count > 0)
                {
                    desc += "ADD-ONS:  " + @"\n ";
                    int idx = 0;
                    foreach (var addon in rsvp.reservation_addon)
                    {
                        idx += 1;
                        string numberedList = "";
                        if (addon.item_type == (int)Common.AddOnGroupType.menu)
                        {
                            numberedList = string.Format("{0}. ", idx);
                        }
                        desc += string.Format("{0}{1} ({2}) {3}  ", numberedList, addon.name, addon.qty, string.IsNullOrWhiteSpace(Convert.ToString(addon.price)) ? "0.00" : addon.price.ToString("N2")) + @"\n ";
                    }
                }

                //if (!string.IsNullOrWhiteSpace(addOnItems))
                //{
                //    desc += "ADD-ONS:  " + @"\n ";
                //    desc += addOnItems;
                //}

                evt.Description = desc;
                iCal.Events.Add(evt);

                var serializer = new CalendarSerializer();
                var serializedCalendar = serializer.SerializeToString(iCal);

                SrlText = serializedCalendar.Replace("-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN", "-//CellarPass.com//NONSGML iCal//EN");
            }
            catch (Exception ex)
            {
                SrlText = "Sorry, there was a problem creating the iCalendar file";
            }

            return SrlText;
        }

        public static string ExportRsvpV2ToICalendar(ReservationDetailModel rsvp, string destinationName, DateTime LocalBookingDate)
        {
            string SrlText = "";

            try
            {
                var iCal = new Ical.Net.Calendar();

                // Set to version 1.0 to make it compatible with Outlook 2003
                iCal.Version = "2.0";

                var evt = new CalendarEvent
                {
                    Start = new CalDateTime(rsvp.event_start_date),
                    End = new CalDateTime(rsvp.event_end_date),
                };

                // evt.Summary = Winery.DisplayName & " - " & Rsvp.EventName
                evt.Summary = rsvp.event_name + " - " + rsvp.user_detail.last_name + " - " + rsvp.total_guests + (rsvp.total_guests > 1 ? " Guests" : " Guest");
                evt.Location = destinationName.Trim() + " - " + rsvp.location_name;

                // Format Desc
                string desc = "";
                desc = rsvp.event_description.Trim() + @"\n \n ";
                desc += "Booking Code: " + rsvp.booking_code + @"\n ";
                desc += "Booking Date: " + LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt") + @"\n ";
                desc += "Guest Name: " + rsvp.user_detail.first_name + " " + rsvp.user_detail.last_name + @"\n ";
                desc += "Guest Phone: " + rsvp.user_detail.phone_number + @"\n ";
                desc += "Guest Email: " + rsvp.user_detail.email + @"\n ";
                desc += "Guest Count: " + rsvp.total_guests + @"\n \n  ";
                desc += destinationName.Trim() + @"\n ";

                if (!string.IsNullOrWhiteSpace(rsvp.guest_note))
                {
                    desc += "\n \n " + "Guest Note:\n " + rsvp.guest_note.Trim();
                }

                if (!string.IsNullOrWhiteSpace(rsvp.internal_note))
                {
                    desc += "\n \n " + "Internal Note:\n " + rsvp.internal_note.Trim();
                }

                //Add-Ons
                string addOnItems = "";

                if (rsvp.reservation_addon != null && rsvp.reservation_addon.Count > 0)
                {
                    int idx = 0;
                    foreach (var addon in rsvp.reservation_addon)
                    {
                        idx += 1;
                        string numberedList = "";
                        if (addon.item_type == (int)Common.AddOnGroupType.menu)
                        {
                            numberedList = string.Format("{0}. ", idx);
                        }
                        addOnItems += string.Format("{0}{1} ({2}) {3}\n", numberedList, addon.name, addon.qty, string.IsNullOrWhiteSpace(Convert.ToString(addon.price)) ? "0.00" : addon.price.ToString("N2"));
                    }
                }

                if (!string.IsNullOrWhiteSpace(addOnItems))
                {
                    desc += "\n \n " + "Add-Ons:\n " + addOnItems;
                }

                evt.Description = desc;
                iCal.Events.Add(evt);

                var serializer = new CalendarSerializer();
                var serializedCalendar = serializer.SerializeToString(iCal);

                SrlText = serializedCalendar.Replace("-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN", "-//CellarPass.com//NONSGML iCal//EN");
            }
            catch (Exception ex)
            {
                SrlText = "Sorry, there was a problem creating the iCalendar file";
            }

            return SrlText;
        }


        /// <summary>
        /// This method is used for sending email on create reservation
        /// </summary>
        /// <returns></returns>
        public async Task<EmailResponse> SendTRSVPEmail(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            if ((!string.IsNullOrEmpty(model.data.BCode) || !(model.data.UId > 0)) && model.data.RsvpId == 0)
            {
                response.message = InternalServerError;
                return response;
            }
            EventDAL eventDAL = new EventDAL(ConnectionString);
            EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
            string bookingCode = model.data.BCode;
            int userID = model.data.UId;
            int iReservationId = model.data.RsvpId;
            bool isMobile = model.isMobile;
            bool SendCPEmail = model.SendCPEmail;
            bool SendGuestEmail = model.data.GuestEmail;
            bool SendAdminEmail = model.data.AdminEmail;
            bool SendAffiliateEmail = model.data.SendAffiliateEmail;
            int isRsvpType = model.data.isRsvpType;
            bool SendToFriendMode = model.SendToFriendMode;
            string ShareMessage = model.ShareMessage;
            List<ShareFriends> ShareEmails = model.share_friends;
            string CCGuestEmail = model.CCGuestEmail;
            int isfaq = 0;
            bool SendCCOnly = CCGuestEmail.Length > 0;
            string perMsg = model.perMsg;

            if (string.IsNullOrEmpty(perMsg))
            {
                perMsg = model.data.perMsg;
                if (string.IsNullOrEmpty(perMsg))
                {
                    perMsg = "None";
                }
            }

            int member_id = eventDAL.GetWineryIdByReservationId(iReservationId);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            logDAL.InsertLog("Queservice::SendRSVPEmail", "data:" + JsonConvert.SerializeObject(model), "", 3, member_id);

            int alternativeEmailTemplate = model.alternativeEmailTemplate;

            string bookingGUID = "";
            string confirmationMessage = "";
            string cancellationMessage = "";

            string MapURL = "";
            string map_and_directions_url = "";
            string mapInageURl = "";
            //Note: In the rare chance that our auto generated booking codes should ever duplicate we pass the user id
            //to make sure what we get is unique.

            List<ReservationChangeLog> listreservationChangeLog = eventDAL.GetReservationEmailLogs(iReservationId);

            if (isRsvpType == 0 && listreservationChangeLog.Count > 0)
            {
                if (SendGuestEmail == true && SendAdminEmail == true)
                {
                    return response;
                }
            }

            //Get Data for Email
            var reservationData = eventDAL.GetReservationEmailDataByReservationId(iReservationId, userID, bookingCode);

            string notesSection = "";
            string GratuityContent = "";
            string RSVPQuestionsSection = "";

            DateTime LocalBookingDate = DateTime.UtcNow;
            if (reservationData != null)
            {
                if (reservationData.GratuityAmount > 0)
                {
                    string GratuityHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"419\" style=\"background-color: #ffffff;width: 419px;padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-69p84\" style=\"max-width: 320px;min-width: 419.04px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\">Gratuity:      </span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"180\" style=\"background-color: #ffffff;width: 180px;padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-30p16\" style=\"max-width: 320px;min-width: 180.96px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\">[[GratuityAmt]]</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

                    GratuityHtml = GratuityHtml.Replace("[[GratuityAmt]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.GratuityAmount));

                    GratuityContent = GratuityHtml;

                }

                isfaq = emailDAL.CheckFAQExistsForWinery(reservationData.WineryID);

                //Preview sample messages
                string personalMSg = model.perMsg;
                string guestNote = reservationData.Notes;

                //RSVPQuestions
                string RSVPQuestionsHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">     <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\">         <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">             <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"419\" style=\"background-color: #ffffff;width: 419px;padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->              <div class=\"u-col u-col-69p84\" style=\"max-width: 320px;min-width: 419.04px;display: table-cell;vertical-align: top;\">                 <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                     <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                         <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                             <tbody>                                 <tr>                                     <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\">                                         <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\">                                             <p style=\"font-size: 14px; line-height: 120%;\">                                             <span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\"><strong>Additional details Regarding Visit</strong></span>                                             </p>                                             <p style=\"font-size: 14px; line-height: 120%;\"><br><span style=\"font-family: Poppins, sans-serif;\">[[RSVPQuestions]]</span></p>                                             <p style=\"font-size: 14px; line-height: 120%;\">&nbsp;</p>                                         </div>                                     </td>                                 </tr>                             </tbody>                         </table> <!--[if (!mso)&(!IE)]><!-->                     </div> <!--<![endif]-->                 </div>             </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"180\" style=\"background-color: #ffffff;width: 180px;padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->             <div class=\"u-col u-col-30p16\" style=\"max-width: 320px;min-width: 180.96px;display: table-cell;vertical-align: top;\">                 <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                     <!--[if (!mso)&(!IE)]><!-->                     <div style=\"box-sizing: border-box; height: 100%; padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                         <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                             <tbody>                                 <tr>                                     <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\">                                         <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\"></span></p> </div>                                     </td>                                 </tr>                             </tbody>                         </table> <!--[if (!mso)&(!IE)]><!-->                     </div> <!--<![endif]-->                 </div>             </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->         </div>     </div> </div>";

                string QAndAHtml = ""; 
                foreach (var item in reservationData.attendee_questions)
                {
                    if (!string.IsNullOrEmpty(QAndAHtml))
                        QAndAHtml += "<br><br>";

                    QAndAHtml += item.question + "<br>" + item.answer;
                }

                if (!string.IsNullOrEmpty(QAndAHtml))
                {
                    RSVPQuestionsHtml = RSVPQuestionsHtml.Replace("[[RSVPQuestions]]", QAndAHtml);

                    RSVPQuestionsSection = RSVPQuestionsHtml;
                }
                
                //NOTES

                    string notesSectionHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\">[[Notes]]</p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                string notesCombined = "";
                string personlMsgHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Personal Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[Personal_Message]]</span><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"></span></p>";
                string guestNoteHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Guest Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[RsvpNote]]</span></p>";

                //If either personal Message or guest note is available we need to render this section
                bool showNoteSection = false;

                if (!string.IsNullOrEmpty(personalMSg))
                {
                    showNoteSection = true;
                    //Replace message tags
                    personlMsgHtml = personlMsgHtml.Replace("[[Personal_Message]]", personalMSg);
                    //Add to notes
                    notesCombined += personlMsgHtml;
                }

                if (!string.IsNullOrEmpty(guestNote))
                {
                    showNoteSection = true;

                    //Replace message tags
                    guestNoteHtml = guestNoteHtml.Replace("[[RsvpNote]]", guestNote);

                    //If personl msg is not empty add this space html first before adding the guest note to create some separation
                    if (notesCombined.Length > 0)
                    {
                        notesCombined += "<p style=\"font-size: 14px; line-height: 120%;\"> </p>";
                    }

                    //Add to notes
                    notesCombined += guestNoteHtml;
                }

                //If either personal or guest note was provided this should be true and we combine it all.
                if (showNoteSection)
                {
                    //replace tag in notes section html with notes combined
                    notesSectionHtml = notesSectionHtml.Replace("[[Notes]]", notesCombined);
                    //Set noteSection to Html
                    notesSection = notesSectionHtml;
                }

                if (string.IsNullOrWhiteSpace(bookingCode))
                    bookingCode = reservationData.BookingCode;

                bookingGUID = reservationData.BookingGUID;
                confirmationMessage = reservationData.EventConfirmationMessage;
                cancellationMessage = reservationData.EventCancellationMessage;
                LocalBookingDate = Times.ToTimeZoneTime(reservationData.BookingDate, (Times.TimeZone)reservationData.TimeZoneId);

                //if (reservationData.referralType == ReferralType.BackOffice || reservationData.referralType == ReferralType.TablePro)
                //    SendGuestEmail = model.data.GuestEmail;
                //else
                //    SendGuestEmail = true;

                SendGuestEmail = model.data.GuestEmail;

                string googleAPIKey = "AIzaSyDKGd8SyV0GhpzD-onvM0ItLpzq5GhCFfA";
                if (_appSetting != null && _appSetting.Value != null && !string.IsNullOrEmpty(_appSetting.Value.GoogleAPIKey))
                {
                    googleAPIKey = _appSetting.Value.GoogleAPIKey;
                }


                if (reservationData.LocationId > 0)
                {
                    MapURL = await Utility.GetMapImageHtmlByLocation(reservationData.LocationId, googleAPIKey);

                    var location = eventDAL.GetLocationMapDataByID(reservationData.LocationId);

                    if (location != null && location.location_id > 0)
                    {
                        map_and_directions_url = location.map_and_directions_url;
                        mapInageURl = "https://cdncellarpass.blob.core.windows.net/photos/location_maps/" + reservationData.LocationId.ToString() + "_dot.jpg";
                    }
                }
            }

            string inviteSection = "";
            bool hasInvite = reservationData.HasInvite;

            if (hasInvite)
            {
                string inviteHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 20px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"line-height: 140%;\"><strong>IMPORTANT!</strong></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\"><strong>Your reservation is currently pending and requires additional action to be confirmed.</strong><br /></span></p> <p style=\"line-height: 140%;\"> </p> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\">You must take immediate action by [[ExpirationDateTime]] in order to confirm your appointment or your reservation will be automatically cancelled and released to others.</span></p> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\"><br />Click on the 'Complete Reservation' button to complete your reservation.</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[CompeteRsvpLink]]\" style=\"height:47px; v-text-anchor:middle; width:540px;\" arcsize=\"8.5%\" stroke=\"f\" fillcolor=\"#1069b0\"><w:anchorlock/><center style=\"color:#FFFFFF;font-family:'Poppins', sans-serif;;\"><![endif]--> <a href=\"[[CompleteRSVPLink]]\" target=\"_blank\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #FFFFFF; background-color: #1069b0; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:100%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;font-size: 14px;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"line-height: 16.8px;\">COMPLETE RESERVATION</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                DateTime expirationdateTime = reservationData.ReservationInviteExpirationDateTime;

                inviteHtml = inviteHtml.Replace("[[ExpirationDateTime]]", String.Format("{0} {1}", expirationdateTime.ToShortDateString(), expirationdateTime.ToString("hh:mm tt")));
                inviteHtml = inviteHtml.Replace("[[CompleteRSVPLink]]", "https://www.cellarpass.com");
                inviteSection = inviteHtml;
            }


            string addOnItems = "";
            string AddOnDetails = "";
            StringBuilder AddOnItemDetails = new StringBuilder();
            string addOnHeading = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 10px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\"><strong><span style=\"font-size: 14px; line-height: 14px;\">ADDITIONAL SELECTIONS</span></strong> </span> </p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
            string addOnItem = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"439\" style=\"background-color: #ffffff;width: 439px;padding: 0px 0px 0px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-73p33\" style=\"max-width: 320px;min-width: 439.98px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 0px 0px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">{{AddOnQty}} X {{AddOnItem}}</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"160\" style=\"background-color: #ffffff;width: 160px;padding: 0px 30px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-26p67\" style=\"max-width: 320px;min-width: 160.02px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 30px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">{{AddOnPrice}}</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

            if (iReservationId > 0)
            {

                var listAddOns = eventDAL.GetReservationAddOnItems(iReservationId);
                if (listAddOns != null)
                {
                    int idx = 0;
                    foreach (var addon in listAddOns)
                    {
                        string item1 = addOnItem;
                        item1 = item1.Replace("{{AddOnQty}}", addon.Qty.ToString());
                        item1 = item1.Replace("{{AddOnItem}}", addon.Name);
                        item1 = item1.Replace("{{AddOnPrice}}", addon.Price.ToString("C", new CultureInfo("en-US")));
                        AddOnItemDetails.Append(item1);

                        idx += 1;
                        string numberedList = "";
                        if (addon.ItemType == (int)Common.AddOnGroupType.menu)
                        {
                            numberedList = string.Format("{0}. ", idx);
                        }
                        addOnItems += string.Format("{0}{1} ({2}) {3}<br />", numberedList, addon.Name, addon.Qty, (string.IsNullOrWhiteSpace(Convert.ToString(addon.Price)) ? "0.00" : addon.Price.ToString("N2")));
                    }
                }
            }

            AddOnDetails = addOnHeading + AddOnItemDetails.ToString();

            if (string.IsNullOrEmpty(addOnItems))
                AddOnDetails = "";

            string PaymentDetails = "";
            string paymentItem = "<p style=\" font-size:14px; line-height: 100%;\"><span style=\"font-family:Poppins, sans-serif; font-size: 14px; line-height: 14px;\">{{PaymentDesc}}</span></p>";

            if (reservationData.Fee == 0)
            {
                string PaymentStatus = paymentItem;
                PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", "Complimentary");

                PaymentDetails = PaymentDetails + PaymentStatus;
            }
            else
            {
                foreach (var item in eventDAL.GetPaymentStatusV2byReservationId(reservationData.ReservationId))
                {
                    string PaymentStatus = paymentItem;
                    PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", item);

                    PaymentDetails = PaymentDetails + PaymentStatus;
                }
            }

            //'LOCATION / ZOOM 'NOTE: We do not show the zoom in the preview unless you swich this manually here. 
            bool sendPreviewWithZoom = false;

            if (reservationData.EventId > 0)
            {
                EventModel eventModelValidate = eventDAL.GetEventById(reservationData.EventId);

                if (eventModelValidate != null && eventModelValidate.EventID > 0)
                {
                    if (eventModelValidate.EventTypeId == 34 && eventModelValidate.MeetingBehavior == 2)
                    {
                        sendPreviewWithZoom = true;
                    }
                }
            }

            //LOCATION ADDRESS
            string lAddress1 = reservationData.MemberAddress1;
            string lAddress2 = reservationData.MemberAddress2;
            string lCity = reservationData.MemberCity;
            string lState = reservationData.MemberState;
            string lZip = reservationData.MemberZipCode;
            string calendarAddress = "";
            //Use Location based address instead of winery address if provided for location
            if ((reservationData.locAddress1 != null))
            {
                if (reservationData.locAddress1.Trim().Length > 0)
                {
                    lAddress1 = (reservationData.locAddress1 == null ? "" : reservationData.locAddress1);
                    lAddress2 = (reservationData.locAddress2 == null ? "" : reservationData.locAddress2);
                    lCity = (reservationData.locCity == null ? "" : reservationData.locCity);
                    lState = (reservationData.locState == null ? "" : reservationData.locState);
                    lZip = (reservationData.locZip == null ? "" : reservationData.locZip);
                }
            }

            //Calendar address
            calendarAddress += lAddress1 + "\\n ";
            if (lAddress2.Trim().Length > 0)
            {
                calendarAddress += lAddress2 + "\\n ";
            }
            calendarAddress += lCity + ", " + lState + " " + lZip + "\\n ";

            string lFullAddress = string.Empty;
            lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);

            //Replace here so that we remove it if it's blank
            string DirectionsURL = "";
            if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
            {
                DirectionsURL = reservationData.MapAndDirectionsURL;
            }
            string DestinationName = reservationData.DestinationName;

            string locationSection = "";

            string zoomHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Zoom Meeting Information</strong></span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">We are using Zoom to host our virtual tastings which will require you to take some additional steps to ensure you can connect to our virtual tasting without any delays. This will require you to enter the Zoom MeetingID and Zoom passport assigned to you.</span></p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[ZoomMeeting]] </span></p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>[[MemberName]] Technical Support</strong></span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">Should you require any assistance with connecting to our virtual tasting, please contact us immediately at [[MemberPhone]]</span></p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Zoom Technical Support</strong></span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">If you have questions or need additional assistance with using Zoom, please contact their <a rel=\"noopener\" href=\"https://support.zoom.us/hc/en-us\" target=\"_blank\">Technical Support</a>.</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

            if (reservationData.EventTypeId != 34)
            {
                string locationHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #ffffff;width: 300px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>[[DestinationName]]</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[LocationAddress]]</span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[MemberPhone]]</span></p> <p style=\"font-size: 14px; line-height: 140%;\"> </p> </div> </td> </tr> </tbody> </table> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[DirectionsURL]]\" style=\"height:48px; v-text-anchor:middle; width:238px;\" arcsize=\"8.5%\" strokecolor=\"#236fa1\" strokeweight=\"1px\" fillcolor=\"#ffffff\"><w:anchorlock/><center style=\"color:#236fa1;font-family:'Poppins', sans-serif;;\"><![endif]--> <a href=\"[[DirectionsURL]]\" target=\"_blank\" class=\"v-button\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #236fa1; background-color: #ffffff; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:100%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;border-top-color: #236fa1; border-top-style: solid; border-top-width: 1px; border-left-color: #236fa1; border-left-style: solid; border-left-width: 1px; border-right-color: #236fa1; border-right-style: solid; border-right-width: 1px; border-bottom-color: #236fa1; border-bottom-style: solid; border-bottom-width: 1px;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"font-size: 14px; line-height: 16.8px; font-family: Poppins, sans-serif;\">GET DIRECTIONS</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #ffffff;width: 300px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;bordear-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div> [[MapURL]] </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                
                string mapHtml = "<table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"> <tr> <td style=\"padding-right: 0px;padding-left: 0px;\" align=\"center\"> <a href=\"[[map_and_directions_url]]\" target=\"_blank\"> <img align=\"center\" border=\"0\" src=\"[[mapInageURl]]\" alt=\"Map Image\" title=\"Map Image\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 100%;max-width: 280px;\" width=\"280\" /> </a> </td> </tr> </table> </td> </tr> </tbody> </table>";

                //'Replace tags in location html
                locationHtml = locationHtml.Replace("[[DestinationName]]", DestinationName);
                locationHtml = locationHtml.Replace("[[LocationAddress]]", lFullAddress);
                locationHtml = locationHtml.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                locationHtml = locationHtml.Replace("[[DirectionsURL]]", DirectionsURL);

                mapHtml = mapHtml.Replace("[[map_and_directions_url]]", map_and_directions_url);
                mapHtml = mapHtml.Replace("[[mapInageURl]]", mapInageURl);

                locationHtml = locationHtml.Replace("[[MapURL]]", mapHtml);

                //'by default it's set to location
                locationSection = locationHtml;
            }
           
            string zoomContent = string.Empty;

            ZoomMeetingInfo zoomMeetingInfo = eventDAL.GetZoomMeetingInfo(reservationData.slotid, reservationData.slottype, reservationData.EventDate, reservationData.ReservationId);

            if (zoomMeetingInfo != null && zoomMeetingInfo.Id > 0)
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)Common.Common.SettingGroup.member);


                bool requirePassword = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_zoom_require_password);
                string zoomMeetingPassword = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_zoom_password);

                if (requirePassword && !string.IsNullOrEmpty(zoomMeetingPassword))
                {
                    zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}<br>Password: {3}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()), zoomMeetingPassword);
                }
                else
                {
                    zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()));
                }
            }

            //'if set to show zoom we use it instead
            if (sendPreviewWithZoom)
            {
                //'Replace tags in Zoom html
                zoomHtml = zoomHtml.Replace("[[ZoomMeeting]]", zoomContent);
                zoomHtml = zoomHtml.Replace("[[MemberName]]", DestinationName);
                zoomHtml = zoomHtml.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));

                locationSection = zoomHtml;
            }
            //Guests Attending

            //Need to get the guests attending each reservation in the booking
            //Get Guests Detail
            string GuestsAttending = eventDAL.GetGuestAttending(reservationData.ReservationId, reservationData.GuestName);

            bool CheckSendmail = true;

            //Get Affiliate Information
            string AffiliateEmail = "";
            string AffiliateName = "";
            string AffiliateCompany = "";

            int AffID = reservationData.AffiliateID;

            if (AffID > 0)
            {
                var aff = eventDAL.GetUser(reservationData.AffiliateID);

                if ((aff != null))
                {
                    AffiliateEmail = aff.AffiliateEmail;
                    AffiliateName = aff.AffiliateName;
                    AffiliateCompany = aff.AffiliateCompany;
                }
            }

            if (SendToFriendMode && ShareEmails != null && ShareEmails.Count > 0)
            {
                //#### RSVP Guest Email - START ####

                try
                {
                    var strOFEmails = ShareEmails.Select(e => e.share_friend_email).ToList();

                    foreach (var item in ShareEmails)
                    {
                        string share_friend_email = item.share_friend_email;
                        string share_friend_first_name = item.share_friend_first_name;
                        string share_friend_last_name = item.share_friend_last_name;

                        string PhoneNumber = "";

                        if (share_friend_email.IndexOf("@noemail") == -1 && !string.IsNullOrEmpty(share_friend_email))
                        {
                            string RsvpGuestContent = "";
                            //Dim GuestsAttending As String = ""
                            ArrayList alGuestsAttendingEmail = new ArrayList();
                            DateTime StartDate = reservationData.EventDate.Add(reservationData.StartTime);
                            DateTime EndDate = reservationData.EventDate.Add(reservationData.EndTime);
                            TimeSpan Duration = EndDate.Subtract(StartDate);
                            string RsvpGuestSubjectContent = "";

                            string FormatFee = reservationData.Fee.ToString();
                            string DepositPolicy = "Complimentary";

                            if (reservationData.Fee > 0)
                                DepositPolicy = GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                            //Format Fee
                            //if (reservationData.Fee > 0)
                            //{
                            //    FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")); //+ ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                            //    //FormatFee = string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.Fee) + ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                            //}
                            //else
                            //{
                            //    FormatFee = "Complimentary";
                            //}

                            FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US"));

                            //Phone Number - check and use home first then work if phone is empty
                            if (!string.IsNullOrEmpty(reservationData.GuestPhone))
                            {
                                PhoneNumber = reservationData.GuestPhone;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(reservationData.GuestWPhone))
                                {
                                    PhoneNumber = Utility.FormatTelephoneNumber(reservationData.GuestWPhone.ToString(), "US");
                                }
                            }

                            EmailContent ew = emailDAL.GetEmailContent((int)EmailTemplates.ShareWithFriend, 0);

                            //If Not ew Is Nothing And ew.Active = True Then

                            if (ew != null)
                            {
                                //Configure/Format CancellationPolicy
                                string CancellationPolicy = "";
                                string CancelByDate = reservationData.cancel_message;

                                if (!string.IsNullOrEmpty(CancelByDate))
                                    CancelByDate = "You must cancel by " + CancelByDate;

                                if ((reservationData.Content != null))
                                {
                                    if (reservationData.Content.Trim().Length > 0)
                                    {
                                        CancellationPolicy = reservationData.Content;
                                        if (reservationData.CancelLeadTime > 50000)
                                        {
                                            CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                        }
                                        else
                                        {
                                            CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                                        }
                                    }
                                }

                                //CancellationPolicy = CancellationPolicy + "<br><br>" + reservationData.cancel_message;

                                //Replace Content Tags
                                RsvpGuestSubjectContent = ew.EmailSubject;
                                RsvpGuestContent = ew.EmailBody;

                                //Subject
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingCode]]", bookingCode);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DestinationName]]", DestinationName);

                                RsvpGuestSubjectContent = ReservationTagReplace(RsvpGuestSubjectContent, bookingGUID);

                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpGuid]]", bookingGUID);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestName]]", reservationData.GuestName);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestPhone]]", PhoneNumber);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestsAttending]]", GuestsAttending);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FormatFee);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DepositPolicy]]", DepositPolicy);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Gratuity]]", GratuityContent);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DestinationName]]", DestinationName);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[PaymentStatus]]", PaymentDetails);

                                string bannerImageHtml = Utility.GetBannerImageHtmlByWineryId(reservationData.WineryID);

                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberName]]", reservationData.MemberName);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress1]]", lAddress1);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress2]]", lAddress2);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressCity]]", lCity);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressState]]", lState);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressZipCode]]", lZip);

                                string timezoneName = reservationData.timezone_name;

                                if (!string.IsNullOrEmpty(timezoneName))
                                {
                                    timezoneName = " (" + timezoneName + ")";
                                }
                                else
                                {
                                    timezoneName = "";
                                }

                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[LocationAddress]]", lFullAddress);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName)); //{0:t}

                                if (reservationData.CancelLeadTime > 50000)
                                {
                                    RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                }
                                else
                                {
                                    RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                                }

                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDuration]]", Duration.ToString());
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                                //RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpHost]]", Host)
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[NotesSection]]", notesSection);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RSVPQuestions]]", RSVPQuestionsSection);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RSVPQuestions]]", RSVPQuestionsSection);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InviteSection]]", inviteSection);

                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancellationPolicy]]", "");
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Personal_Message]]", perMsg);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[ZoomMeeting]]", zoomContent);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[AffiliateName]]", AffiliateName);
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[AffiliateCompany]]", AffiliateCompany);
                                //Remove Admin Only Tags
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddress1]]", "");
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[[GuestAddress2]]]", "");
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressCity]]", "");
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressState]]", "");
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressZipCode]]", "");
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InternalNotes]]", "");
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[[AffiliateNote]]]", "");
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookedBy]]", "");
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[AdminConfirmStatus]]", "");
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                                //Body
                                RsvpGuestContent = RsvpGuestContent.Replace("[[BookingCode]]", bookingCode);

                                RsvpGuestContent = ReservationTagReplace(RsvpGuestContent, bookingGUID);

                                RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpGuid]]", bookingGUID);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                                RsvpGuestContent = RsvpGuestContent.Replace("[[GuestName]]", reservationData.GuestName);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[GuestPhone]]", PhoneNumber);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                                RsvpGuestContent = RsvpGuestContent.Replace("[[GuestsAttending]]", GuestsAttending);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FormatFee);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[DepositPolicy]]", DepositPolicy);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                                RsvpGuestContent = RsvpGuestContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                                RsvpGuestContent = RsvpGuestContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                                RsvpGuestContent = RsvpGuestContent.Replace("[[Gratuity]]", GratuityContent);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[DestinationName]]", DestinationName);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[PaymentStatus]]", PaymentDetails);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", reservationData.MemberName);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress1]]", lAddress1);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress2]]", lAddress2);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressCity]]", lCity);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressState]]", lState);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressZipCode]]", lZip);

                                RsvpGuestContent = RsvpGuestContent.Replace("[[LocationAddress]]", lFullAddress);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                                RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));

                                if (reservationData.CancelLeadTime > 50000)
                                {
                                    RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                }
                                else
                                {
                                    RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                                }

                                RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDuration]]", Duration.ToString());
                                RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                                //RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpHost]]", Host)
                                RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                                RsvpGuestContent = RsvpGuestContent.Replace("[[NotesSection]]", notesSection);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[RSVPQuestions]]", RSVPQuestionsSection);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[InviteSection]]", inviteSection);

                                RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                                RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                                RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateName]]", AffiliateName);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateCompany]]", AffiliateCompany);

                                RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationPolicy]]", CancellationPolicy);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[CancelByDate]]", CancelByDate);

                                RsvpGuestContent = RsvpGuestContent.Replace("[[Personal_Message]]", perMsg);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[ZoomMeeting]]", zoomContent);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));

                                string strpath = "https://typhoon.cellarpass.com/";
                                if (ConnectionString.IndexOf("live") > -1)
                                    strpath = "https://www.cellarpass.com/";

                                string profileUrl = string.Format("{1}business/{0}", reservationData.ProfileUrl, strpath);

                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[MemberLinkButton]]", profileUrl);
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[MemberLinkButton]]", profileUrl);

                                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberLinkButton]]", profileUrl);
                                if (isfaq == 1)
                                {
                                    RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                                    RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");
                                    RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                                    RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");

                                    RsvpGuestContent = RsvpGuestContent.Replace("[[FAQButton]]", profileUrl + "?f=1");
                                }
                                else
                                {
                                    RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                                    RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl);
                                    RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                                    RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl);

                                    RsvpGuestContent = RsvpGuestContent.Replace("[[FAQButton]]", profileUrl);
                                }


                                //Add-Ons:
                                RsvpGuestContent = RsvpGuestContent.Replace("[[Add-Ons]]", addOnItems);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[AddOnItems]]", AddOnDetails);

                                //Remove Admin Only Tags

                                RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddress1]]", "");
                                RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddress2]]", "");
                                RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressCity]]", "");
                                RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressState]]", "");
                                RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressZipCode]]", "");
                                RsvpGuestContent = RsvpGuestContent.Replace("[[InternalNotes]]", "");
                                RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateNote]]", "");
                                RsvpGuestContent = RsvpGuestContent.Replace("[[BookedBy]]", "");
                                RsvpGuestContent = RsvpGuestContent.Replace("[[AdminConfirmStatus]]", "");
                                RsvpGuestContent = RsvpGuestContent.Replace("[[ConfirmationMessage]]", confirmationMessage);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationMessage]]", cancellationMessage);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));

                                //Attach iCal File
                                //Dim attach As New Mail.Attachment
                                EmailAttachment emailAttachment = new EmailAttachment();

                                //Directions URL
                                //string DirectionsURL = "";


                                try
                                {
                                    if (reservationData != null)
                                    {
                                        var winery = new WineryModel();
                                        winery = eventDAL.GetWineryById(reservationData.WineryID);
                                        //We replace this with nothing as it's just used as a flag to attach
                                        //attach.Name = "CellarPass_Confirm.ics"
                                        //attach.Contents = Events.ExportRsvpV2ToICalendarGuest(rsvpResult, winery, calendarAddress, DestinationName)
                                        emailAttachment.Name = "CellarPass_Confirm.ics";
                                        var reservation = eventDAL.GetReservationDetailsbyReservationId(iReservationId);
                                        emailAttachment.Contents = ExportRsvpV2ToICalendarGuest(reservation, winery, calendarAddress, reservationData.DestinationName);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //attach = Nothing
                                    emailAttachment = null;
                                }



                                RsvpGuestContent = RsvpGuestContent.Replace("[[DirectionsURL]]", DirectionsURL);

                                RsvpGuestContent = RsvpGuestContent.Replace("[[MapURl]]", MapURL);
                                RsvpGuestContent = RsvpGuestContent.Replace("[[LocationSection]]", locationSection);
                                //We replace this with nothing as it's just used as a flag to attach
                                RsvpGuestContent = RsvpGuestContent.Replace("[[?Attach_iCal]]", "");

                                string emails = "";
                                if (SendToFriendMode)
                                {
                                    if (strOFEmails != null)
                                    {
                                        if (strOFEmails.Count() > 0)
                                        {
                                            foreach (var email_loopVariable in strOFEmails)
                                            {
                                                if (!string.IsNullOrEmpty(email_loopVariable.Trim()))
                                                    emails += email_loopVariable.Trim() + ",";
                                            }

                                            if (emails.EndsWith(","))
                                            {
                                                emails = emails.Remove(emails.Length - 1);
                                            }

                                            RsvpGuestContent = RsvpGuestContent.Replace("[[ShareEmailList]]", emails);
                                            RsvpGuestContent = RsvpGuestContent.Replace("[[ShareFirstName]]", share_friend_first_name);
                                            RsvpGuestContent = RsvpGuestContent.Replace("[[ShareLastName]]", share_friend_last_name);
                                            RsvpGuestContent = RsvpGuestContent.Replace("[[ShareMessage]]", ShareMessage);
                                        }
                                    }
                                }

                                string emailTo = "";

                                if (SendToFriendMode)
                                {
                                    //string of multiple emails
                                    emailTo = share_friend_email;
                                    if (model.SendCopyToGuest && !string.IsNullOrWhiteSpace(reservationData.GuestEmail))
                                        emailTo += ", " + reservationData.GuestEmail;
                                }

                                SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);

                                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)SettingGroup.member);
                                string member_rsvp_contact_email = Settings.GetStrValue(settingsGroup, SettingKey.member_rsvp_contact_email);
                                //Send with New Mail (Mailgun)
                                response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.Rsvp, iReservationId, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent.Replace("[[AdminMessage]]", ""), reservationData.WineryID, emailAttachment, ew.Id, member_rsvp_contact_email, DestinationName));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logDAL.InsertLog("RSVP Friend Email", "data:" + JsonConvert.SerializeObject(model) + ", " + ex.ToString(), "", 1, member_id);
                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpEmail", "SendGuestEmail: " + ex.ToString());
                }
                //#### RSVP Guest Email - END ####
            }
            else if (SendGuestEmail)
            {
                //#### RSVP Guest Email - START ####

                try
                {
                    string PhoneNumber = "";

                    if (reservationData.GuestEmail.IndexOf("@noemail") == -1 || SendCCOnly == true)
                    {

                        string RsvpGuestContent = "";
                        //Dim GuestsAttending As String = ""
                        ArrayList alGuestsAttendingEmail = new ArrayList();
                        DateTime StartDate = reservationData.EventDate.Add(reservationData.StartTime);
                        DateTime EndDate = reservationData.EventDate.Add(reservationData.EndTime);
                        TimeSpan Duration = EndDate.Subtract(StartDate);
                        string RsvpGuestSubjectContent = "";

                        //LOCATION ADDRESS
                        //string lAddress1 = reservationData.MemberAddress1;
                        //string lAddress2 = reservationData.MemberAddress2;
                        //string lCity = reservationData.MemberCity;
                        //string lState = reservationData.MemberState;
                        //string lZip = reservationData.MemberZipCode;
                        //string calendarAddress = "";
                        //Use Location based address instead of winery address if provided for location
                        if ((reservationData.locAddress1 != null))
                        {
                            if (reservationData.locAddress1.Trim().Length > 0)
                            {
                                lAddress1 = (reservationData.locAddress1 == null ? "" : reservationData.locAddress1);
                                lAddress2 = (reservationData.locAddress2 == null ? "" : reservationData.locAddress2);
                                lCity = (reservationData.locCity == null ? "" : reservationData.locCity);
                                lState = (reservationData.locState == null ? "" : reservationData.locState);
                                lZip = (reservationData.locZip == null ? "" : reservationData.locZip);
                            }
                        }

                        //Calendar address
                        calendarAddress += lAddress1 + "\\n ";
                        if (lAddress2.Trim().Length > 0)
                        {
                            calendarAddress += lAddress2 + "\\n ";
                        }
                        calendarAddress += lCity + ", " + lState + " " + lZip + "\\n ";

                        string FormatFee = reservationData.Fee.ToString();
                        string DepositPolicy = GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;

                        //Format Fee
                        //if (reservationData.Fee > 0)
                        //{
                        //    FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")); //+ ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                        //    //FormatFee = string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.Fee) + ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                        //}
                        //else
                        //{
                        //    FormatFee = "Complimentary";
                        //}

                        FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US"));

                        //Phone Number - check and use home first then work if phone is empty
                        if (!string.IsNullOrEmpty(reservationData.GuestPhone))
                        {
                            PhoneNumber = reservationData.GuestPhone;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(reservationData.GuestWPhone))
                            {
                                PhoneNumber = Utility.FormatTelephoneNumber(reservationData.GuestWPhone.ToString(), "US");
                            }
                        }



                        EmailContent ew = new EmailContent();

                        //if (alternativeEmailTemplate > 0)
                        //{
                        //    ew = emailDAL.GetEmailContentByID(alternativeEmailTemplate);
                        //}
                        //else
                        //{
                        //    ew = emailDAL.GetEmailContent((int)EmailTemplates.RsvpGuest, -9999);
                        //}

                        ew = emailDAL.GetEmailContent((int)EmailTemplates.RsvpGuest, -9999);

                        //If Not ew Is Nothing And ew.Active = True Then

                        if ((ew != null))
                        {

                            //Configure/Format CancellationPolicy
                            string CancellationPolicy = "";
                            string CancelByDate = reservationData.cancel_message;

                            if (!string.IsNullOrEmpty(CancelByDate))
                                CancelByDate = "You must cancel by " + CancelByDate;

                            if ((reservationData.Content != null))
                            {
                                if (reservationData.Content.Trim().Length > 0)
                                {
                                    CancellationPolicy = reservationData.Content;

                                    if (reservationData.CancelLeadTime > 50000)
                                    {
                                        CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                    }
                                    else
                                    {
                                        CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                                    }
                                }
                            }

                            //CancellationPolicy = CancellationPolicy + "<br><br>" + reservationData.cancel_message;

                            //Replace Content Tags
                            RsvpGuestSubjectContent = ew.EmailSubject;
                            RsvpGuestContent = ew.EmailBody;

                            //Subject
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingCode]]", bookingCode);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DestinationName]]", DestinationName);

                            RsvpGuestSubjectContent = ReservationTagReplace(RsvpGuestSubjectContent, bookingGUID);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpGuid]]", bookingGUID);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestName]]", reservationData.GuestName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestPhone]]", PhoneNumber);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestsAttending]]", GuestsAttending);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FormatFee);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DepositPolicy]]", DepositPolicy);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Gratuity]]", GratuityContent);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[PaymentStatus]]", PaymentDetails);

                            string bannerImageHtml = Utility.GetBannerImageHtmlByWineryId(reservationData.WineryID);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberName]]", reservationData.MemberName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress1]]", lAddress1);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress2]]", lAddress2);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressCity]]", lCity);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressState]]", lState);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressZipCode]]", lZip);

                            //string lFullAddress = string.Empty;
                            lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);
                            string timezoneName = reservationData.timezone_name;
                            if (!string.IsNullOrEmpty(timezoneName))
                            {
                                timezoneName = " (" + timezoneName + ")";
                            }
                            else
                            {
                                timezoneName = "";
                            }
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[LocationAddress]]", lFullAddress);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName)); //{0:t}

                            if (reservationData.CancelLeadTime > 50000)
                            {
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                            }
                            else
                            {
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                            }

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDuration]]", Duration.ToString());
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                            //RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpHost]]", Host)
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[NotesSection]]", notesSection);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RSVPQuestions]]", RSVPQuestionsSection);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InviteSection]]", inviteSection);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancellationPolicy]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Personal_Message]]", perMsg);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[ZoomMeeting]]", zoomContent);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[AffiliateName]]", AffiliateName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[AffiliateCompany]]", AffiliateCompany);
                            //Remove Admin Only Tags
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddress1]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[[GuestAddress2]]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressCity]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressState]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressZipCode]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InternalNotes]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[[AffiliateNote]]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookedBy]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[AdminConfirmStatus]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));

                            //Body
                            RsvpGuestContent = RsvpGuestContent.Replace("[[BookingCode]]", bookingCode);

                            RsvpGuestContent = ReservationTagReplace(RsvpGuestContent, bookingGUID);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[DestinationName]]", DestinationName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpGuid]]", bookingGUID);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestName]]", reservationData.GuestName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestPhone]]", PhoneNumber);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestsAttending]]", GuestsAttending);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FormatFee);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[DepositPolicy]]", DepositPolicy);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Gratuity]]", GratuityContent);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[PaymentStatus]]", PaymentDetails);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", reservationData.MemberName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress1]]", lAddress1);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress2]]", lAddress2);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressCity]]", lCity);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressState]]", lState);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressZipCode]]", lZip);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[LocationAddress]]", lFullAddress);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));

                            if (reservationData.CancelLeadTime > 50000)
                            {
                                RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                            }
                            else
                            {
                                RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                            }

                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDuration]]", Duration.ToString());
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                            //RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpHost]]", Host)
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[NotesSection]]", notesSection);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RSVPQuestions]]", RSVPQuestionsSection);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[InviteSection]]", inviteSection);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateName]]", AffiliateName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateCompany]]", AffiliateCompany);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationPolicy]]", CancellationPolicy);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancelByDate]]", CancelByDate);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Personal_Message]]", perMsg);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[ZoomMeeting]]", zoomContent);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));

                            string strpath = "https://typhoon.cellarpass.com/";
                            if (ConnectionString.IndexOf("live") > -1)
                                strpath = "https://www.cellarpass.com/";

                            string profileUrl = string.Format("{1}business/{0}", reservationData.ProfileUrl, strpath);

                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[MemberLinkButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[MemberLinkButton]]", profileUrl);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberLinkButton]]", profileUrl);

                            if (isfaq == 1)
                            {
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");

                                RsvpGuestContent = RsvpGuestContent.Replace("[[FAQButton]]", profileUrl + "?f=1");
                            }
                            else
                            {
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl);
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl);

                                RsvpGuestContent = RsvpGuestContent.Replace("[[FAQButton]]", profileUrl);
                            }


                            //Add-Ons:
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Add-Ons]]", addOnItems);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AddOnItems]]", AddOnDetails);

                            //Remove Admin Only Tags

                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddress1]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddress2]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressCity]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressState]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressZipCode]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[InternalNotes]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateNote]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[BookedBy]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AdminConfirmStatus]]", "");

                            RsvpGuestContent = RsvpGuestContent.Replace("[[ConfirmationMessage]]", confirmationMessage);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationMessage]]", cancellationMessage);
                            //Attach iCal File
                            //Dim attach As New Mail.Attachment
                            EmailAttachment emailAttachment = new EmailAttachment();

                            //Directions URL
                            //string DirectionsURL = "";


                            try
                            {
                                if (reservationData != null)
                                {
                                    var winery = new WineryModel();
                                    winery = eventDAL.GetWineryById(reservationData.WineryID);
                                    //We replace this with nothing as it's just used as a flag to attach
                                    //attach.Name = "CellarPass_Confirm.ics"
                                    //attach.Contents = Events.ExportRsvpV2ToICalendarGuest(rsvpResult, winery, calendarAddress, DestinationName)
                                    emailAttachment.Name = "CellarPass_Confirm.ics";
                                    var reservation = eventDAL.GetReservationDetailsbyReservationId(iReservationId);
                                    emailAttachment.Contents = ExportRsvpV2ToICalendarGuest(reservation, winery, calendarAddress, reservationData.DestinationName);
                                }

                            }
                            catch (Exception ex)
                            {
                                //attach = Nothing
                                emailAttachment = null;
                            }

                            //Replace here so that we remove it if it's blank
                            //string DirectionsURL = "";
                            if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
                            {
                                DirectionsURL = reservationData.MapAndDirectionsURL;
                            }
                            RsvpGuestContent = RsvpGuestContent.Replace("[[DirectionsURL]]", DirectionsURL);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MapURl]]", MapURL);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[LocationSection]]", locationSection);
                            //We replace this with nothing as it's just used as a flag to attach
                            RsvpGuestContent = RsvpGuestContent.Replace("[[?Attach_iCal]]", "");

                            //Send CCEmail
                            string strOFEmails = "";
                            //Dim CCEmailList As New ArrayList
                            if (!string.IsNullOrEmpty(CCGuestEmail))
                            {
                                if (CCGuestEmail.IndexOf(",") > 0)
                                {
                                    string[] emails = CCGuestEmail.Trim().Split(',');
                                    foreach (string em in emails)
                                    {
                                        if (em.Trim().Length > 0)
                                        {
                                            //CCEmailList.Add(em.Trim)
                                            strOFEmails += em.Trim() + ", ";
                                            if (strOFEmails.EndsWith(","))
                                            {
                                                strOFEmails = strOFEmails.Remove(strOFEmails.Length - 1);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //CCEmailList.Add(CCGuestEmail)
                                    strOFEmails = CCGuestEmail.Trim();
                                }
                            }
                            else
                            {
                                //CCEmailList = Nothing
                            }

                            string emailTo = "";

                            if (SendCCOnly || SendToFriendMode)
                            {
                                //string of multiple emails
                                emailTo = strOFEmails;
                                if (model.SendCopyToGuest && !string.IsNullOrWhiteSpace(reservationData.GuestEmail))
                                    emailTo += ", " + reservationData.GuestEmail;
                            }
                            else
                            {
                                //Single Email
                                emailTo = reservationData.GuestEmail;
                                //If there are cc emails add them to the guest email
                                if ((!object.ReferenceEquals(strOFEmails.Trim(), string.Empty)))
                                {
                                    emailTo = emailTo + "," + strOFEmails;
                                }
                            }

                            SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);

                            List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)SettingGroup.member);
                            string member_rsvp_contact_email = Settings.GetStrValue(settingsGroup, SettingKey.member_rsvp_contact_email);
                            //Send with New Mail (Mailgun)
                            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.Rsvp, iReservationId, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent.Replace("[[AdminMessage]]", ""), reservationData.WineryID, emailAttachment, ew.Id, member_rsvp_contact_email, DestinationName));

                        }
                    }
                }
                catch (Exception ex)
                {
                    logDAL.InsertLog("RSVP Guest Email", "data:" + JsonConvert.SerializeObject(model) + ", " + ex.ToString(), "", 1, member_id);
                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpEmail", "SendGuestEmail: " + ex.ToString());
                }
                //#### RSVP Guest Email - END ####
            }


            if (SendAffiliateEmail)
            {
                //#### RSVP Affiliate Email (uses Guest for now) - START ####
                try
                {
                    string PhoneNumber = "";


                    string RsvpGuestContent = "";
                    //Dim GuestsAttending As String = ""
                    ArrayList alGuestsAttendingEmail = new ArrayList();
                    DateTime StartDate = reservationData.EventDate.Add(reservationData.StartTime);
                    DateTime EndDate = reservationData.EventDate.Add(reservationData.EndTime);
                    TimeSpan Duration = EndDate.Subtract(StartDate);
                    string RsvpGuestSubjectContent = "";

                    //LOCATION ADDRESS
                    //string lAddress1 = reservationData.MemberAddress1;
                    //string lAddress2 = reservationData.MemberAddress2;
                    //string lCity = reservationData.MemberCity;
                    //string lState = reservationData.MemberState;
                    //string lZip = reservationData.MemberZipCode;
                    //string calendarAddress = "";

                    //Use Location based address instead of winery address if provided for location
                    if ((reservationData.locAddress1 != null))
                    {
                        if (reservationData.locAddress1.Trim().Length > 0)
                        {
                            lAddress1 = (reservationData.locAddress1 == null ? "" : reservationData.locAddress1);
                            lAddress2 = (reservationData.locAddress2 == null ? "" : reservationData.locAddress2);
                            lCity = (reservationData.locCity == null ? "" : reservationData.locCity);
                            lState = (reservationData.locState == null ? "" : reservationData.locState);
                            lZip = (reservationData.locZip == null ? "" : reservationData.locZip);
                        }
                    }

                    //Calendar address
                    calendarAddress += lAddress1 + "\\n ";
                    if (lAddress2.Trim().Length > 0)
                    {
                        calendarAddress += lAddress2 + "\\n ";
                    }
                    calendarAddress += lCity + ", " + lState + " " + lZip + "\\n ";


                    //Format Fee
                    string FormatFee = reservationData.Fee.ToString();
                    string DepositPolicy = GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;

                    //if (reservationData.Fee > 0)
                    //{
                    //    FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")); //+ ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                    //    //FormatFee = string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.Fee) + ", " + GetDepositPoliciesEmail().Where(f => f.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                    //}
                    //else
                    //{
                    //    FormatFee = "Complimentary";
                    //}

                    FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US"));

                    //Phone Number - check and use home first then work if phone is empty
                    if (!string.IsNullOrEmpty(reservationData.GuestPhone))
                    {
                        PhoneNumber = reservationData.GuestPhone;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(reservationData.GuestWPhone))
                        {
                            PhoneNumber = Utility.FormatTelephoneNumber(reservationData.GuestWPhone.ToString(), "US");
                        }
                    }


                    //Mail.EmailTemplates MailTemplate = reservationData.EmailTemplateID;

                    EmailContent ew = default(EmailContent);
                    //if (alternativeEmailTemplate > 0)
                    //{
                    //    ew = emailDAL.GetEmailContentByID(alternativeEmailTemplate);
                    //}
                    //else
                    //{
                    //    ew = emailDAL.GetEmailContent((int)EmailTemplates.RsvpGuest, -9999);
                    //}
                    ew = emailDAL.GetEmailContent((int)EmailTemplates.RsvpGuest, -9999);

                    if ((ew != null) && ew.Active == true)
                    {
                        //Configure/Format CancellationPolicy
                        string CancellationPolicy = "";
                        string CancelByDate = reservationData.cancel_message;

                        if (!string.IsNullOrEmpty(CancelByDate))
                            CancelByDate = "You must cancel by " + CancelByDate;

                        if ((reservationData.Content != null))
                        {
                            if (reservationData.Content.Trim().Length > 0)
                            {
                                CancellationPolicy = reservationData.Content;

                                if (reservationData.CancelLeadTime > 50000)
                                {
                                    CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                }
                                else
                                {
                                    CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                                }
                            }
                        }

                        //CancellationPolicy = CancellationPolicy + "<br><br>" + reservationData.cancel_message;
                        //Replace Content Tags
                        RsvpGuestSubjectContent = ew.EmailSubject;
                        RsvpGuestContent = ew.EmailBody;

                        //Subject
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingCode]]", bookingCode);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DestinationName]]", DestinationName);

                        RsvpGuestSubjectContent = ReservationTagReplace(RsvpGuestSubjectContent, bookingGUID);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpGuid]]", bookingGUID);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestName]]", reservationData.GuestName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestPhone]]", PhoneNumber);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestsAttending]]", GuestsAttending);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FormatFee);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DepositPolicy]]", DepositPolicy);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Gratuity]]", GratuityContent);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[PaymentStatus]]", PaymentDetails);

                        string bannerImageHtml = Utility.GetBannerImageHtmlByWineryId(reservationData.WineryID);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberName]]", reservationData.MemberName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress1]]", lAddress1);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress2]]", lAddress2);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressCity]]", lCity);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressState]]", lState);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressZipCode]]", lZip);

                        //string lFullAddress = string.Empty;
                        lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);
                        string timezoneName = reservationData.timezone_name;

                        if (!string.IsNullOrEmpty(timezoneName))
                        {
                            timezoneName = " (" + timezoneName + ")";
                        }
                        else
                        {
                            timezoneName = "";
                        }
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[LocationAddress]]", lFullAddress);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName)); //{0:t}

                        if (reservationData.CancelLeadTime > 50000)
                        {
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                        }
                        else
                        {
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                        }

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDuration]]", Duration.ToString());
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                        //RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpHost]]", Host)
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[NotesSection]]", notesSection);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RSVPQuestions]]", RSVPQuestionsSection);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InviteSection]]", inviteSection);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancellationPolicy]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Personal_Message]]", perMsg);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[ZoomMeeting]]", zoomContent);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[AffiliateName]]", AffiliateName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[AffiliateCompany]]", AffiliateCompany);
                        //Remove Admin Only Tags
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddress1]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[[GuestAddress2]]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressCity]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressState]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressZipCode]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InternalNotes]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[[AffiliateNote]]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookedBy]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[AdminConfirmStatus]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));

                        //Body
                        RsvpGuestContent = RsvpGuestContent.Replace("[[BookingCode]]", bookingCode);

                        RsvpGuestContent = ReservationTagReplace(RsvpGuestContent, bookingGUID);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[DestinationName]]", DestinationName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpGuid]]", bookingGUID);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestName]]", reservationData.GuestName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestPhone]]", PhoneNumber);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestsAttending]]", GuestsAttending);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FormatFee);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[DepositPolicy]]", DepositPolicy);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[Gratuity]]", GratuityContent);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[PaymentStatus]]", PaymentDetails);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", reservationData.MemberName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress1]]", lAddress1);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress2]]", lAddress2);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressCity]]", lCity);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressState]]", lState);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressZipCode]]", lZip);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[LocationAddress]]", lFullAddress);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));

                        if (reservationData.CancelLeadTime > 50000)
                        {
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                        }
                        else
                        {
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                        }

                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDuration]]", Duration.ToString());
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                        //RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpHost]]", Host)
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[NotesSection]]", notesSection);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RSVPQuestions]]", RSVPQuestionsSection);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[InviteSection]]", inviteSection);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateName]]", AffiliateName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateCompany]]", AffiliateCompany);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationPolicy]]", CancellationPolicy);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[CancelByDate]]", CancelByDate);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[Personal_Message]]", perMsg);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[ZoomMeeting]]", zoomContent);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));

                        string strpath = "https://typhoon.cellarpass.com/";
                        if (ConnectionString.IndexOf("live") > -1)
                            strpath = "https://www.cellarpass.com/";

                        string profileUrl = string.Format("{1}business/{0}", reservationData.ProfileUrl, strpath);

                        RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                        RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[MemberLinkButton]]", profileUrl);
                        RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                        RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[MemberLinkButton]]", profileUrl);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberLinkButton]]", profileUrl);

                        if (isfaq == 1)
                        {
                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");

                            RsvpGuestContent = RsvpGuestContent.Replace("[[FAQButton]]", profileUrl + "?f=1");
                        }
                        else
                        {
                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[FAQButton]]", profileUrl);
                        }


                        //Add-Ons:
                        RsvpGuestContent = RsvpGuestContent.Replace("[[Add-Ons]]", addOnItems);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[AddOnItems]]", AddOnDetails);

                        //Remove Admin Only Tags

                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddress1]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddress2]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressCity]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressState]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressZipCode]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[InternalNotes]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateNote]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[BookedBy]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[AdminConfirmStatus]]", "");

                        RsvpGuestContent = RsvpGuestContent.Replace("[[ConfirmationMessage]]", confirmationMessage);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationMessage]]", cancellationMessage);
                        //Attach iCal File
                        //Dim attach As New Mail.Attachment
                        EmailAttachment emailAttachment = new EmailAttachment();

                        //Directions URL
                        //string DirectionsURL = "";


                        try
                        {
                            if (reservationData != null)
                            {
                                var winery = new WineryModel();
                                winery = eventDAL.GetWineryById(reservationData.WineryID);
                                //We replace this with nothing as it's just used as a flag to attach
                                //attach.Name = "CellarPass_Confirm.ics"
                                //attach.Contents = Events.ExportRsvpV2ToICalendarGuest(rsvpResult, winery, calendarAddress, DestinationName)
                                emailAttachment.Name = "CellarPass_Confirm.ics";
                                var reservation = eventDAL.GetReservationDetailsbyReservationId(iReservationId);
                                emailAttachment.Contents = ExportRsvpV2ToICalendarGuest(reservation, winery, calendarAddress, reservationData.DestinationName);
                            }

                        }
                        catch (Exception ex)
                        {
                            //attach = Nothing
                            emailAttachment = null;
                        }

                        //Replace here so that we remove it if it's blank
                        //string DirectionsURL = "";
                        if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
                        {
                            DirectionsURL = reservationData.MapAndDirectionsURL;
                        }
                        RsvpGuestContent = RsvpGuestContent.Replace("[[DirectionsURL]]", DirectionsURL);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MapURl]]", MapURL);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[LocationSection]]", locationSection);
                        //We replace this with nothing as it's just used as a flag to attach
                        RsvpGuestContent = RsvpGuestContent.Replace("[[?Attach_iCal]]", "");

                        string emailTo = "";

                        //Single Email
                        emailTo = AffiliateEmail;
                        //If there are cc emails add them to the guest email
                        if ((!object.ReferenceEquals(CCGuestEmail.Trim(), string.Empty)))
                        {
                            emailTo = emailTo + "," + CCGuestEmail.Trim();
                        }
                        //Send with New Mail (Mailgun)

                        SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);

                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)SettingGroup.member);
                        string member_rsvp_contact_email = Settings.GetStrValue(settingsGroup, SettingKey.member_rsvp_contact_email);

                        response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.RsvpAffiliate, iReservationId, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent.Replace("[[AdminMessage]]", ""), reservationData.WineryID, emailAttachment, ew.Id, member_rsvp_contact_email, DestinationName));


                    }
                }
                catch (Exception ex)
                {
                    logDAL.InsertLog("RSVP Affiliate Email", "data:" + JsonConvert.SerializeObject(model) + ", " + ex.ToString(), "", 1, member_id);
                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpEmail", "SendAffiliateEmail: " + ex.ToString());
                }
                //#### RSVP Guest Email - END ####
            }


            if (SendAdminEmail)
            {
                //#### RSVP Admin Email - START ####
                try
                {
                    //NOTE: Commented Out and moved inside of rsvp loop - Using regular Guest Email Template Now instead of RsvpAdmin
                    //Dim MailTemplate As Mail.EmailTemplates = Mail.EmailTemplates.RsvpAdmin

                    string PhoneNumber = "";

                    string RsvpAdminContent = "";
                    string RsvpAdminSubjectContent = "";
                    //Dim GuestsAttending As String = ""
                    DateTime StartDate = reservationData.EventDate.Add(reservationData.StartTime);
                    DateTime EndDate = reservationData.EventDate.Add(reservationData.EndTime);
                    TimeSpan Duration = EndDate.Subtract(StartDate);
                    string BookedBy = "";
                    BookedBy = eventDAL.GetUser(reservationData.BookedById).AffiliateName;

                    //Format Fee
                    string FormatFee = reservationData.Fee.ToString();
                    string DepositPolicy = GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;

                    //if (reservationData.Fee > 0)
                    //{
                    //    FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")); //+ ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                    //    //FormatFee = string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.Fee) + ", " + GetDepositPoliciesEmail().Where(f => f.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                    //}
                    //else
                    //{
                    //    FormatFee = "Complimentary";
                    //}

                    FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US"));

                    //LOCATION ADDRESS
                    //string lAddress1 = reservationData.MemberAddress1;
                    //string lAddress2 = reservationData.MemberAddress2;
                    //string lCity = reservationData.MemberCity;
                    //string lState = reservationData.MemberState;
                    //string lZip = reservationData.MemberZipCode;
                    //string calendarAddress = "";

                    //Use Location based address instead of winery address if provided for location
                    if ((reservationData.locAddress1 != null))
                    {
                        if (reservationData.locAddress1.Trim().Length > 0)
                        {
                            lAddress1 = (reservationData.locAddress1 == null ? "" : reservationData.locAddress1);
                            lAddress2 = (reservationData.locAddress2 == null ? "" : reservationData.locAddress2);
                            lCity = (reservationData.locCity == null ? "" : reservationData.locCity);
                            lState = (reservationData.locState == null ? "" : reservationData.locState);
                            lZip = (reservationData.locZip == null ? "" : reservationData.locZip);
                        }
                    }

                    //Phone Number - check and use home first then work if phone is empty
                    if (!string.IsNullOrEmpty(reservationData.GuestPhone))
                    {
                        PhoneNumber = reservationData.GuestPhone;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(reservationData.GuestWPhone))
                        {
                            PhoneNumber = Utility.FormatTelephoneNumber(reservationData.GuestWPhone.ToString(), "US");
                        }
                    }

                    EmailContent ea = default(EmailContent);

                    //if (alternativeEmailTemplate > 0)
                    //{
                    //    ea = emailDAL.GetEmailContentByID(alternativeEmailTemplate);
                    //}
                    //else
                    //{
                    //    ea = emailDAL.GetEmailContent((int)EmailTemplates.RsvpGuest, -9999);
                    //}

                    ea = emailDAL.GetEmailContent((int)EmailTemplates.RsvpGuest, -9999);

                    if ((ea != null) && ea.Active == true)
                    {
                        //Value for Admin Confirm Status tag
                        string AdminConfirmMsg = "New Reservation Received";
                        if (isRsvpType == 1)
                        {
                            AdminConfirmMsg = "Rescheduled";
                        }
                        else if (isRsvpType == 2)
                        {
                            AdminConfirmMsg = "Reservation Updated";
                        }

                        //Configure/Format CancellationPolicy
                        string CancellationPolicy = "";
                        string CancelByDate = reservationData.cancel_message;

                        if (!string.IsNullOrEmpty(CancelByDate))
                            CancelByDate = "You must cancel by " + CancelByDate;

                        if ((reservationData.Content != null))
                        {
                            if (reservationData.Content.Trim().Length > 0)
                            {
                                CancellationPolicy = reservationData.Content;

                                if (reservationData.CancelLeadTime > 50000)
                                {
                                    CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                }
                                else
                                {
                                    CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                                }

                            }
                        }

                        //CancellationPolicy = CancellationPolicy + "<br><br>" + reservationData.cancel_message;

                        //Replace Content Tags

                        //Admin Email Subject
                        string adminEmailSubject = ea.EmailSubject;
                        if (!object.ReferenceEquals(ea.EmailSubjectAdmin.Trim(), string.Empty))
                        {
                            adminEmailSubject = ea.EmailSubjectAdmin;
                        }
                        //Subject
                        RsvpAdminSubjectContent = adminEmailSubject;
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[BookingCode]]", bookingCode);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[DestinationName]]", DestinationName);

                        RsvpAdminSubjectContent = ReservationTagReplace(RsvpAdminSubjectContent, bookingGUID);

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpGuid]]", bookingGUID);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestName]]", reservationData.GuestName);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestPhone]]", PhoneNumber);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestAddress1]]", reservationData.GuestAddress1);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestAddress2]]", reservationData.GuestAddress2);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestAddressCity]]", reservationData.GuestCity);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestAddressState]]", reservationData.GuestState);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestAddressZipCode]]", reservationData.GuestZipCode);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestsAttending]]", GuestsAttending);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[Fee]]", FormatFee);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[DepositPolicy]]", DepositPolicy);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[Gratuity]]", GratuityContent);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[PaymentStatus]]", PaymentDetails);

                        string bannerImageHtml = Utility.GetBannerImageHtmlByWineryId(reservationData.WineryID);

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberName]]", reservationData.MemberName);

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberAddress1]]", lAddress1);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberAddress2]]", lAddress2);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberAddressCity]]", lCity);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberAddressState]]", lState);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberAddressZipCode]]", lZip);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));

                        //string lFullAddress = string.Empty;
                        lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);

                        string timezoneName = reservationData.timezone_name;

                        if (!string.IsNullOrEmpty(timezoneName))
                        {
                            timezoneName = " (" + timezoneName + ")";
                        }
                        else
                        {
                            timezoneName = "";
                        }
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[LocationAddress]]", lFullAddress);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName)); //{0:t}

                        if (reservationData.CancelLeadTime > 50000)
                        {
                            RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                        }
                        else
                        {
                            RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                        }

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpDuration]]", Duration.ToString());
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                        //RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpHost]]", Host)
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[NotesSection]]", notesSection);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RSVPQuestions]]", RSVPQuestionsSection);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[InviteSection]]", inviteSection);

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                        //No need for notes in subject - RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[Inter'nalNotes]]", InternalNote)
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[BookedBy]]", BookedBy);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[CancellationPolicy]]", "");
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[AdminConfirmStatus]]", AdminConfirmMsg);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[ZoomMeeting]]", zoomContent);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[AffiliateName]]", AffiliateName);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[AffiliateCompany]]", AffiliateCompany);

                        //body
                        RsvpAdminContent = ea.EmailBody;
                        RsvpAdminContent = RsvpAdminContent.Replace("[[BookingCode]]", bookingCode);

                        RsvpAdminContent = ReservationTagReplace(RsvpAdminContent, bookingGUID);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[DestinationName]]", DestinationName);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpGuid]]", bookingGUID);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestName]]", reservationData.GuestName);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestPhone]]", PhoneNumber);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestsAttending]]", GuestsAttending);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[Fee]]", FormatFee);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[DepositPolicy]]", DepositPolicy);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[Gratuity]]", GratuityContent);

                        RsvpAdminContent = RsvpAdminContent.Replace("[[PaymentStatus]]", PaymentDetails);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberName]]", reservationData.MemberName);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberAddress1]]", lAddress1);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberAddress2]]", lAddress2);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberAddressCity]]", lCity);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberAddressState]]", lState);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberAddressZipCode]]", lZip);

                        RsvpAdminContent = RsvpAdminContent.Replace("[[LocationAddress]]", lFullAddress);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));

                        if (reservationData.CancelLeadTime > 50000)
                        {
                            RsvpAdminContent = RsvpAdminContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                        }
                        else
                        {
                            RsvpAdminContent = RsvpAdminContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                        }

                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpDuration]]", Duration.ToString());
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                        //RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpHost]]", Host)
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                        RsvpAdminContent = RsvpAdminContent.Replace("[[NotesSection]]", notesSection);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RSVPQuestions]]", RSVPQuestionsSection);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[InviteSection]]", inviteSection);

                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[AffiliateName]]", AffiliateName);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[AffiliateCompany]]", AffiliateCompany);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[CancellationPolicy]]", CancellationPolicy);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[CancelByDate]]", CancelByDate);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[Personal_Message]]", perMsg);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[ZoomMeeting]]", zoomContent);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));

                        string strpath = "https://typhoon.cellarpass.com/";
                        if (ConnectionString.IndexOf("live") > -1)
                            strpath = "https://www.cellarpass.com/";

                        string profileUrl = string.Format("{1}business/{0}", reservationData.ProfileUrl, strpath);

                        RsvpAdminContent = RsvpAdminContent.Replace("https://www.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                        RsvpAdminContent = RsvpAdminContent.Replace("https://www.cellarpass.com/[[MemberLinkButton]]", profileUrl);
                        RsvpAdminContent = RsvpAdminContent.Replace("https://dev.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                        RsvpAdminContent = RsvpAdminContent.Replace("https://dev.cellarpass.com/[[MemberLinkButton]]", profileUrl);

                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberLinkButton]]", profileUrl);

                        if (isfaq == 1)
                        {
                            RsvpAdminContent = RsvpAdminContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                            RsvpAdminContent = RsvpAdminContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");
                            RsvpAdminContent = RsvpAdminContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                            RsvpAdminContent = RsvpAdminContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");

                            RsvpAdminContent = RsvpAdminContent.Replace("[[FAQButton]]", profileUrl + "?f=1");
                        }
                        else
                        {
                            RsvpAdminContent = RsvpAdminContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                            RsvpAdminContent = RsvpAdminContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl);
                            RsvpAdminContent = RsvpAdminContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                            RsvpAdminContent = RsvpAdminContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl);

                            RsvpAdminContent = RsvpAdminContent.Replace("[[FAQButton]]", profileUrl);
                        }


                        //Add-Ons:
                        RsvpAdminContent = RsvpAdminContent.Replace("[[Add-Ons]]", addOnItems);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[AddOnItems]]", AddOnDetails);

                        //Remove Admin Only Tags

                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestAddress1]]", "");
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestAddress2]]", "");
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestAddressCity]]", "");
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestAddressState]]", "");
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestAddressZipCode]]", "");
                        RsvpAdminContent = RsvpAdminContent.Replace("[[InternalNotes]]", "");
                        RsvpAdminContent = RsvpAdminContent.Replace("[[AffiliateNote]]", "");
                        RsvpAdminContent = RsvpAdminContent.Replace("[[BookedBy]]", "");
                        RsvpAdminContent = RsvpAdminContent.Replace("[[AdminConfirmStatus]]", "");

                        RsvpAdminContent = RsvpAdminContent.Replace("[[ConfirmationMessage]]", confirmationMessage);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[CancellationMessage]]", cancellationMessage);
                        //Attach iCal File
                        //Dim attach As New Mail.Attachment
                        EmailAttachment emailAttachment = new EmailAttachment();

                        //Directions URL
                        //string DirectionsURL = "";


                        try
                        {
                            if (reservationData != null)
                            {
                                var winery = new WineryModel();
                                winery = eventDAL.GetWineryById(reservationData.WineryID);
                                //We replace this with nothing as it's just used as a flag to attach
                                //attach.Name = "CellarPass_Confirm.ics"
                                //attach.Contents = Events.ExportRsvpV2ToICalendarGuest(rsvpResult, winery, calendarAddress, DestinationName)
                                emailAttachment.Name = "CellarPass_Confirm.ics";
                                var reservation = eventDAL.GetReservationDetailsbyReservationId(iReservationId);
                                emailAttachment.Contents = ExportRsvpV2ToICalendarGuest(reservation, winery, calendarAddress, reservationData.DestinationName);
                            }

                        }
                        catch (Exception ex)
                        {
                            //attach = Nothing
                            emailAttachment = null;
                        }

                        //Replace here so that we remove it if it's blank
                        //string DirectionsURL = "";
                        if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
                        {
                            DirectionsURL = reservationData.MapAndDirectionsURL;
                        }
                        RsvpAdminContent = RsvpAdminContent.Replace("[[DirectionsURL]]", DirectionsURL);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MapURl]]", MapURL);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[LocationSection]]", locationSection);
                        //We replace this with nothing as it's just used as a flag to attach
                        RsvpAdminContent = RsvpAdminContent.Replace("[[?Attach_iCal]]", "");

                        //Admin Only Section
                        string RsvpAdminOnlyContent = ea.EmailBodyAdmin;
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[GuestAddress1]]", reservationData.GuestAddress1);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[GuestAddress2]]", reservationData.GuestAddress2);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[GuestAddressCity]]", reservationData.GuestCity);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[GuestAddressState]]", reservationData.GuestState);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[GuestAddressZipCode]]", reservationData.GuestZipCode);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[InternalNotes]]", reservationData.InternalNote);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[AffiliateNote]]", reservationData.ConciergeNote);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[BookedBy]]", BookedBy);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[AdminConfirmStatus]]", AdminConfirmMsg);

                        //Add admin section to end of other content
                        //RsvpAdminContent += "<br />" + RsvpAdminOnlyContent;
                        RsvpAdminContent = RsvpAdminContent.Replace("[[AdminMessage]]", RsvpAdminOnlyContent);

                        string RsvpAdminEmailTo = string.Empty;

                        if (reservationData.EventId > 0 && reservationData.ConfirmationUsers.Length > 0)
                        {
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                            List<string> listUsers = reservationData.ConfirmationUsers.Split(",").ToList();

                            foreach (var item in listUsers)
                            {
                                if (RsvpAdminEmailTo.Length > 0)
                                    RsvpAdminEmailTo = RsvpAdminEmailTo + ",";

                                RsvpAdminEmailTo = RsvpAdminEmailTo + userDAL.GetUserEmailById(Convert.ToInt32(item));
                            }
                        }

                        SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);
                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)SettingGroup.member);

                        if (reservationData.EventId == 0)
                            RsvpAdminEmailTo = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_private_booking_notifications_receiver);

                        //if (RsvpAdminEmailTo.Length == 0)
                        //    RsvpAdminEmailTo = ea.EmailTo;

                        string member_rsvp_contact_email = Settings.GetStrValue(settingsGroup, SettingKey.member_rsvp_contact_email);

                        if (RsvpAdminEmailTo.Length != 0)
                            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.RsvpAdmin, iReservationId, ea.EmailFrom, RsvpAdminEmailTo, RsvpAdminSubjectContent, RsvpAdminContent, reservationData.WineryID, emailAttachment, ea.Id, member_rsvp_contact_email, DestinationName));


                    }
                }
                catch (Exception ex)
                {
                    logDAL.InsertLog("RSVP Admin Email", "data:" + JsonConvert.SerializeObject(model) + ", " + ex.ToString(), "", 1, member_id);
                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpEmail", "SendAdminEmail: " + ex.ToString());
                }
                //#### RSVP Admin Email - END ####
            }
            return response;
        }

        /// <summary>
        /// This method is used for sending emails on reservation update
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<EmailResponse> SendEmailOnReservationUpdate(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            if ((!string.IsNullOrEmpty(model.data.BCode) || !(model.data.UId > 0)) && model.data.RsvpId == 0)
            {
                response.message = InternalServerError;
                return response;
            }
            EventDAL eventDAL = new EventDAL(ConnectionString);
            EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
            string bookingCode = model.data.BCode;
            int userID = model.data.UId;
            int iReservationId = model.data.RsvpId;
            bool isMobile = model.isMobile;
            bool SendCPEmail = model.SendCPEmail;
            bool SendGuestEmail = model.data.GuestEmail;
            bool SendAdminEmail = model.data.AdminEmail;
            bool SendToFriendMode = model.SendToFriendMode;
            string ShareMessage = model.ShareMessage;
            List<ShareFriends> ShareEmails = model.share_friends;
            string CCGuestEmail = model.CCGuestEmail;
            bool SendAffiliateEmail = model.data.SendAffiliateEmail;
            int isRsvpType = model.data.isRsvpType;
            bool SendCCOnly = CCGuestEmail.Length > 0;
            string perMsg = model.perMsg;

            if (string.IsNullOrEmpty(perMsg))
            {
                perMsg = model.data.perMsg;
            }

            int alternativeEmailTemplate = model.alternativeEmailTemplate;

            bool IsRsvpUpdate = model.data.isRsvpUpdate;
            string bookingGUID = "";
            //Note: In the rare chance that our auto generated booking codes should ever duplicate we pass the user id
            //to make sure what we get is unique.

            //Get Data for Email
            var reservationData = eventDAL.GetReservationEmailDataByReservationId(iReservationId, userID, bookingCode);
            DateTime LocalBookingDate = DateTime.UtcNow;
            if (reservationData != null)
            {
                bookingGUID = reservationData.BookingGUID;
                LocalBookingDate = Times.ToTimeZoneTime(reservationData.BookingDate, (Times.TimeZone)reservationData.TimeZoneId);
            }

            string GratuityContent = "";
            if (reservationData.GratuityAmount > 0)
            {
                string GratuityHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"419\" style=\"background-color: #ffffff;width: 419px;padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-69p84\" style=\"max-width: 320px;min-width: 419.04px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\">Gratuity:      </span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"180\" style=\"background-color: #ffffff;width: 180px;padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-30p16\" style=\"max-width: 320px;min-width: 180.96px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\">[[GratuityAmt]]</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

                GratuityHtml = GratuityHtml.Replace("[[GratuityAmt]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.GratuityAmount));

                GratuityContent = GratuityHtml;

            }

            string notesSection = "";

            //Preview sample messages
            string personalMSg = model.perMsg;
            string guestNote = reservationData.Notes;

            //NOTES

            string notesSectionHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\">[[Notes]]</p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
            string notesCombined = "";
            string personlMsgHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Personal Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[Personal_Message]]</span><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"></span></p>";
            string guestNoteHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Guest Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[RsvpNote]]</span></p>";

            //If either personal Message or guest note is available we need to render this section
            bool showNoteSection = false;

            if (!string.IsNullOrEmpty(personalMSg))
            {
                showNoteSection = true;
                //Replace message tags
                personlMsgHtml = personlMsgHtml.Replace("[[Personal_Message]]", personalMSg);
                //Add to notes
                notesCombined += personlMsgHtml;
            }

            if (!string.IsNullOrEmpty(guestNote))
            {
                showNoteSection = true;

                //Replace message tags
                guestNoteHtml = guestNoteHtml.Replace("[[RsvpNote]]", guestNote);

                //If personl msg is not empty add this space html first before adding the guest note to create some separation
                if (notesCombined.Length > 0)
                {
                    notesCombined += "<p style=\"font-size: 14px; line-height: 120%;\"> </p>";
                }

                //Add to notes
                notesCombined += guestNoteHtml;
            }

            //If either personal or guest note was provided this should be true and we combine it all.
            if (showNoteSection)
            {
                //replace tag in notes section html with notes combined
                notesSectionHtml = notesSectionHtml.Replace("[[Notes]]", notesCombined);
                //Set noteSection to Html
                notesSection = notesSectionHtml;
            }
            //Guests Attending

            //Need to get the guests attending each reservation in the booking
            //Get Guests Detail
            string GuestsAttending = eventDAL.GetGuestAttending(iReservationId, reservationData.GuestName);

            bool CheckSendmail = true;

            string zoomContent = string.Empty;

            ZoomMeetingInfo zoomMeetingInfo = eventDAL.GetZoomMeetingInfo(reservationData.slotid, reservationData.slottype, reservationData.EventDate, reservationData.ReservationId);

            if (zoomMeetingInfo != null && zoomMeetingInfo.Id > 0)
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)Common.Common.SettingGroup.member);


                bool requirePassword = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_zoom_require_password);
                string zoomMeetingPassword = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_zoom_password);

                if (requirePassword && !string.IsNullOrEmpty(zoomMeetingPassword))
                {
                    zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}<br>Password: {3}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()), zoomMeetingPassword);
                }
                else
                {
                    zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()));
                }
            }

            string PaymentDetails = "";
            string paymentItem = "<p style=\" font-size:14px; line-height: 100%;\"><span style=\"font-family:Poppins, sans-serif; font-size: 14px; line-height: 14px;\">{{PaymentDesc}}</span></p>";

            if (reservationData.Fee == 0)
            {
                string PaymentStatus = paymentItem;
                PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", "Complimentary");

                PaymentDetails = PaymentDetails + PaymentStatus;
            }
            else
            {
                foreach (var item in eventDAL.GetPaymentStatusV2byReservationId(reservationData.ReservationId))
                {
                    string PaymentStatus = paymentItem;
                    PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", item);

                    PaymentDetails = PaymentDetails + PaymentStatus;
                }
            }


            if (SendGuestEmail)
            {
                //#### RSVP Guest Email - START ####

                try
                {
                    string PhoneNumber = "";



                    if (reservationData.GuestEmail.IndexOf("@noemail") == -1 || SendCCOnly == true)
                    {

                        string RsvpGuestContent = "";
                        //Dim GuestsAttending As String = ""
                        ArrayList alGuestsAttendingEmail = new ArrayList();
                        DateTime StartDate = reservationData.EventDate.Add(reservationData.StartTime);
                        DateTime EndDate = reservationData.EventDate.Add(reservationData.EndTime);
                        TimeSpan Duration = EndDate.Subtract(StartDate);
                        string RsvpGuestSubjectContent = "";

                        string timezoneName = reservationData.timezone_name;

                        if (!string.IsNullOrEmpty(timezoneName))
                        {
                            timezoneName = " (" + timezoneName + ")";
                        }
                        else
                        {
                            timezoneName = "";
                        }

                        //Get Affiliate Information
                        string AffiliateEmail = "";
                        string AffiliateName = "";
                        string AffiliateCompany = "";

                        int AffID = reservationData.AffiliateID;

                        if (AffID > 0)
                        {
                            var affiliate = eventDAL.GetUser(reservationData.AffiliateID);
                        }

                        //LOCATION ADDRESS
                        string lAddress1 = reservationData.MemberAddress1;
                        string lAddress2 = reservationData.MemberAddress2;
                        string lCity = reservationData.MemberCity;
                        string lState = reservationData.MemberState;
                        string lZip = reservationData.MemberZipCode;
                        string calendarAddress = "";


                        //Use Location based address instead of winery address if provided for location
                        if ((reservationData.locAddress1 != null))
                        {
                            if (reservationData.locAddress1.Trim().Length > 0)
                            {
                                lAddress1 = (reservationData.locAddress1 == null ? "" : reservationData.locAddress1);
                                lAddress2 = (reservationData.locAddress2 == null ? "" : reservationData.locAddress2);
                                lCity = (reservationData.locCity == null ? "" : reservationData.locCity);
                                lState = (reservationData.locState == null ? "" : reservationData.locState);
                                lZip = (reservationData.locZip == null ? "" : reservationData.locZip);
                            }
                        }

                        //Calendar address
                        calendarAddress += lAddress1 + "\\n ";
                        if (lAddress2.Trim().Length > 0)
                        {
                            calendarAddress += lAddress2 + "\\n ";
                        }
                        calendarAddress += lCity + ", " + lState + " " + lZip + "\\n ";

                        string FormatFee = reservationData.Fee.ToString();
                        //string DepositPolicy = GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;

                        //Format Fee
                        if (reservationData.Fee > 0)
                        {
                            FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")) + ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                            //FormatFee = string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.Fee) + ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                        }
                        else
                        {
                            FormatFee = "Complimentary";
                        }

                        //Phone Number - check and use home first then work if phone is empty
                        if (!string.IsNullOrEmpty(reservationData.GuestPhone))
                        {
                            PhoneNumber = reservationData.GuestPhone;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(reservationData.GuestWPhone))
                            {
                                PhoneNumber = Utility.FormatTelephoneNumber(reservationData.GuestWPhone.ToString(), "US");
                            }
                        }



                        EmailContent ew = new EmailContent();

                        if (SendToFriendMode)
                        {
                            ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.ShareWithFriend, 0);
                        }
                        else
                        {
                            if (alternativeEmailTemplate > 0)
                            {
                                ew = emailDAL.GetEmailContentByID(alternativeEmailTemplate);
                            }
                            else
                            {
                                ew = emailDAL.GetEmailContentByID(reservationData.EmailContentID);
                            }
                        }

                        //If Not ew Is Nothing And ew.Active = True Then

                        if ((ew != null))
                        {

                            //Configure/Format CancellationPolicy
                            string CancellationPolicy = "";
                            string CancelByDate = "";
                            if ((reservationData.Content != null))
                            {
                                if (reservationData.Content.Trim().Length > 0)
                                {
                                    CancellationPolicy = reservationData.Content;

                                    if (reservationData.CancelLeadTime > 50000)
                                    {
                                        CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                    }
                                    else
                                    {
                                        CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                                    }
                                }
                            }

                            //Replace Content Tags
                            RsvpGuestSubjectContent = ew.EmailSubject;
                            RsvpGuestContent = ew.EmailBody;

                            //Subject
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingCode]]", bookingCode);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DestinationName]]", reservationData.DestinationName);

                            RsvpGuestSubjectContent = ReservationTagReplace(RsvpGuestSubjectContent, bookingGUID);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpGuid]]", bookingGUID);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestName]]", reservationData.GuestName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestPhone]]", PhoneNumber);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestsAttending]]", GuestsAttending);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FormatFee);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Gratuity]]", GratuityContent);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[PaymentStatus]]", PaymentDetails);
                            string bannerImageHtml = Utility.GetBannerImageHtmlByWineryId(reservationData.WineryID);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberName]]", reservationData.MemberName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress1]]", lAddress1);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress2]]", lAddress2);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressCity]]", lCity);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressState]]", lState);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressZipCode]]", lZip);

                            string lFullAddress = string.Empty;
                            lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[LocationAddress]]", lFullAddress);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));

                            if (reservationData.CancelLeadTime > 50000)
                            {
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                            }
                            else
                            {
                                RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                            }

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDuration]]", Duration.ToString());
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                            //RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpHost]]", Host)
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[NotesSection]]", notesSection);
                            //RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InviteSection]]", inviteSection);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancellationPolicy]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Personal_Message]]", perMsg);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[ZoomMeeting]]", zoomContent);

                            //Remove Admin Only Tags
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddress1]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[[GuestAddress2]]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressCity]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressState]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressZipCode]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InternalNotes]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[[AffiliateNote]]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookedBy]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[AdminConfirmStatus]]", "");

                            //Body
                            RsvpGuestContent = RsvpGuestContent.Replace("[[BookingCode]]", bookingCode);

                            RsvpGuestContent = ReservationTagReplace(RsvpGuestContent, bookingGUID);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[DestinationName]]", reservationData.DestinationName);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpGuid]]", bookingGUID);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestName]]", reservationData.GuestName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestPhone]]", PhoneNumber);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestsAttending]]", GuestsAttending);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FormatFee);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Gratuity]]", GratuityContent);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[PaymentStatus]]", PaymentDetails);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", reservationData.MemberName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress1]]", lAddress1);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress2]]", lAddress2);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressCity]]", lCity);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressState]]", lState);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressZipCode]]", lZip);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[LocationAddress]]", lFullAddress);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));

                            if (reservationData.CancelLeadTime > 50000)
                            {
                                RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                            }
                            else
                            {
                                RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                            }

                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDuration]]", Duration.ToString());
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                            //RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpHost]]", Host)
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[NotesSection]]", notesSection);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateName]]", AffiliateName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateCompany]]", AffiliateCompany);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationPolicy]]", CancellationPolicy);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Personal_Message]]", perMsg);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[ZoomMeeting]]", zoomContent);

                            //Remove Admin Only Tags
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddress1]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddress2]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressCity]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressState]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressZipCode]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[InternalNotes]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateNote]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[BookedBy]]", "");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AdminConfirmStatus]]", "");

                            //Attach iCal File
                            //Dim attach As New Mail.Attachment
                            EmailAttachment emailAttachment = new EmailAttachment();

                            //Directions URL
                            //string DirectionsURL = "";


                            try
                            {
                                if (RsvpGuestContent.IndexOf("[[?Attach_iCal]]") > 0)
                                {
                                    if ((reservationData != null))
                                    {
                                        if (RsvpGuestContent.IndexOf("[[?Attach_iCal]]") > 0)
                                        {
                                            var winery = new WineryModel();
                                            winery = eventDAL.GetWineryById(reservationData.WineryID);
                                            //We replace this with nothing as it's just used as a flag to attach
                                            //attach.Name = "CellarPass_Confirm.ics"
                                            //attach.Contents = Events.ExportRsvpV2ToICalendarGuest(rsvpResult, winery, calendarAddress, DestinationName)
                                            emailAttachment.Name = "CellarPass_Confirm.ics";
                                            var reservation = eventDAL.GetReservationDetailsbyReservationId(iReservationId);
                                            emailAttachment.Contents = ExportRsvpV2ToICalendarGuest(reservation, winery, calendarAddress, reservationData.DestinationName);

                                            //emailAttachment.Contents = Events.ExportRsvpV2ToICalendarGuest(rsvpResult, winery, calendarAddress, reservationData.DestinationName);
                                        }
                                        else
                                        {
                                            //attach = Nothing
                                            emailAttachment = null;
                                        }

                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                //attach = Nothing
                                emailAttachment = null;
                            }

                            //Replace here so that we remove it if it's blank
                            string DirectionsURL = "";
                            if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
                            {
                                DirectionsURL = "<a href=\"" + reservationData.MapAndDirectionsURL.Trim() + "\" target=\"_blank\">Get Directions</a>";
                            }
                            RsvpGuestContent = RsvpGuestContent.Replace("[[DirectionsURL]]", DirectionsURL);

                            //We replace this with nothing as it's just used as a flag to attach
                            RsvpGuestContent = RsvpGuestContent.Replace("[[?Attach_iCal]]", "");

                            //Send CCEmail
                            string strOFEmails = "";
                            //Dim CCEmailList As New ArrayList
                            if (!string.IsNullOrEmpty(CCGuestEmail))
                            {
                                if (CCGuestEmail.IndexOf(",") > 0)
                                {
                                    string[] emails = CCGuestEmail.Trim().Split(',');
                                    foreach (string em in emails)
                                    {
                                        if (em.Trim().Length > 0)
                                        {
                                            //CCEmailList.Add(em.Trim)
                                            strOFEmails += em.Trim() + ", ";
                                            if (strOFEmails.EndsWith(","))
                                            {
                                                strOFEmails = strOFEmails.Remove(strOFEmails.Length - 1);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //CCEmailList.Add(CCGuestEmail)
                                    strOFEmails = CCGuestEmail.Trim();
                                }
                            }
                            else
                            {
                                //CCEmailList = Nothing
                            }

                            string emailTo = "";

                            if (SendCCOnly)
                            {
                                //string of multiple emails
                                emailTo = strOFEmails;
                            }
                            else
                            {
                                //Single Email
                                emailTo = reservationData.GuestEmail;
                                //If there are cc emails add them to the guest email
                                if ((!object.ReferenceEquals(strOFEmails.Trim(), string.Empty)))
                                {
                                    emailTo = emailTo + "," + strOFEmails;
                                }
                            }
                            //Send with New Mail (Mailgun)
                            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.RsvpUpdate, iReservationId, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent, reservationData.WineryID, emailAttachment, ew.Id));

                        }
                    }
                }
                catch (Exception ex)
                {
                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpEmail", "SendGuestEmail: " + ex.ToString());
                }
                //#### RSVP Guest Email - END ####
            }


            if (SendAffiliateEmail)
            {
                //#### RSVP Affiliate Email (uses Guest for now) - START ####
                try
                {
                    string PhoneNumber = "";


                    string RsvpGuestContent = "";
                    //Dim GuestsAttending As String = ""
                    ArrayList alGuestsAttendingEmail = new ArrayList();
                    DateTime StartDate = reservationData.EventDate.Add(reservationData.StartTime);
                    DateTime EndDate = reservationData.EventDate.Add(reservationData.EndTime);
                    TimeSpan Duration = EndDate.Subtract(StartDate);
                    string RsvpGuestSubjectContent = "";

                    string timezoneName = reservationData.timezone_name;

                    if (!string.IsNullOrEmpty(timezoneName))
                    {
                        timezoneName = " (" + timezoneName + ")";
                    }
                    else
                    {
                        timezoneName = "";
                    }

                    //Get Affiliate Information
                    string AffiliateEmail = "";
                    string AffiliateName = "";
                    string AffiliateCompany = "";

                    int AffID = reservationData.AffiliateID;

                    if (AffID > 0)
                    {
                        var aff = eventDAL.GetUser(reservationData.AffiliateID);

                        if ((aff != null))
                        {
                            AffiliateEmail = aff.AffiliateEmail;
                            AffiliateName = aff.AffiliateName;
                            AffiliateCompany = aff.AffiliateCompany;
                        }
                    }

                    //LOCATION ADDRESS
                    string lAddress1 = reservationData.MemberAddress1;
                    string lAddress2 = reservationData.MemberAddress2;
                    string lCity = reservationData.MemberCity;
                    string lState = reservationData.MemberState;
                    string lZip = reservationData.MemberZipCode;
                    string calendarAddress = "";

                    //Use Location based address instead of winery address if provided for location
                    if ((reservationData.locAddress1 != null))
                    {
                        if (reservationData.locAddress1.Trim().Length > 0)
                        {
                            lAddress1 = (reservationData.locAddress1 == null ? "" : reservationData.locAddress1);
                            lAddress2 = (reservationData.locAddress2 == null ? "" : reservationData.locAddress2);
                            lCity = (reservationData.locCity == null ? "" : reservationData.locCity);
                            lState = (reservationData.locState == null ? "" : reservationData.locState);
                            lZip = (reservationData.locZip == null ? "" : reservationData.locZip);
                        }
                    }

                    //Calendar address
                    calendarAddress += lAddress1 + "\\n ";
                    if (lAddress2.Trim().Length > 0)
                    {
                        calendarAddress += lAddress2 + "\\n ";
                    }
                    calendarAddress += lCity + ", " + lState + " " + lZip + "\\n ";


                    //Format Fee
                    string FormatFee = reservationData.Fee.ToString();
                    //string DepositPolicy = GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;

                    if (reservationData.Fee > 0)
                    {
                        FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")) + ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                        //FormatFee = string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.Fee) + ", " + GetDepositPoliciesEmail().Where(f => f.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                    }
                    else
                    {
                        FormatFee = "Complimentary";
                    }

                    //Phone Number - check and use home first then work if phone is empty
                    if (!string.IsNullOrEmpty(reservationData.GuestPhone))
                    {
                        PhoneNumber = reservationData.GuestPhone;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(reservationData.GuestWPhone))
                        {
                            PhoneNumber = Utility.FormatTelephoneNumber(reservationData.GuestWPhone.ToString(), "US");
                        }
                    }


                    //Mail.EmailTemplates MailTemplate = reservationData.EmailTemplateID;

                    EmailContent ew = default(EmailContent);
                    if (alternativeEmailTemplate > 0)
                    {
                        ew = emailDAL.GetEmailContentByID(alternativeEmailTemplate);
                    }
                    else
                    {
                        ew = emailDAL.GetEmailContentByID(reservationData.EmailContentID);
                    }


                    if ((ew != null) && ew.Active == true)
                    {
                        //Configure/Format CancellationPolicy
                        string CancellationPolicy = "";
                        string CancelByDate = "";
                        if ((reservationData.Content != null))
                        {
                            if (reservationData.Content.Trim().Length > 0)
                            {
                                CancellationPolicy = reservationData.Content;

                                if (reservationData.CancelLeadTime > 50000)
                                {
                                    CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                }
                                else
                                {
                                    CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                                }
                            }
                        }


                        //Replace Content Tags
                        RsvpGuestSubjectContent = ew.EmailSubject;
                        RsvpGuestContent = ew.EmailBody;

                        //Subject
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingCode]]", bookingCode);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DestinationName]]", reservationData.DestinationName);

                        RsvpGuestSubjectContent = ReservationTagReplace(RsvpGuestSubjectContent, bookingGUID);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpGuid]]", bookingGUID);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestName]]", reservationData.GuestName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestPhone]]", PhoneNumber);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestsAttending]]", GuestsAttending);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FormatFee);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Gratuity]]", GratuityContent);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[PaymentStatus]]", PaymentDetails);
                        string bannerImageHtml = Utility.GetBannerImageHtmlByWineryId(reservationData.WineryID);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberName]]", reservationData.MemberName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress1]]", lAddress1);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress2]]", lAddress2);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressCity]]", lCity);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressState]]", lState);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressZipCode]]", lZip);

                        string lFullAddress = string.Empty;
                        lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[LocationAddress]]", lFullAddress);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));

                        if (reservationData.CancelLeadTime > 50000)
                        {
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                        }
                        else
                        {
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                        }

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDuration]]", Duration.ToString());
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                        //RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpHost]]", Host)
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[NotesSection]]", notesSection);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancellationPolicy]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[ZoomMeeting]]", zoomContent);
                        //Body
                        RsvpGuestContent = RsvpGuestContent.Replace("[[BookingCode]]", bookingCode);

                        RsvpGuestContent = ReservationTagReplace(RsvpGuestContent, bookingGUID);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[DestinationName]]", reservationData.DestinationName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpGuid]]", bookingGUID);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestName]]", reservationData.GuestName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestPhone]]", PhoneNumber);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestsAttending]]", GuestsAttending);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FormatFee);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[Gratuity]]", GratuityContent);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[PaymentStatus]]", PaymentDetails);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", reservationData.MemberName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress1]]", lAddress1);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress2]]", lAddress2);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressCity]]", lCity);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressState]]", lState);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressZipCode]]", lZip);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[LocationAddress]]", lFullAddress);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));

                        if (reservationData.CancelLeadTime > 50000)
                        {
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                        }
                        else
                        {
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                        }

                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDuration]]", Duration.ToString());
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                        //RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpHost]]", Host)
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[NotesSection]]", notesSection);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateName]]", AffiliateName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateCompany]]", AffiliateCompany);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationPolicy]]", CancellationPolicy);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[Personal_Message]]", perMsg);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[ZoomMeeting]]", zoomContent);

                        //Attach iCal File
                        //Dim attach As New Mail.Attachment
                        EmailAttachment emailAttachment = new EmailAttachment();

                        //string DirectionsURL = "";

                        try
                        {

                            if (RsvpGuestContent.IndexOf("[[?Attach_iCal]]") > 0)
                            {

                                if ((reservationData != null))
                                {
                                    if (RsvpGuestContent.IndexOf("[[?Attach_iCal]]") > 0)
                                    {
                                        var winery = new WineryModel();
                                        winery = eventDAL.GetWineryById(reservationData.WineryID);
                                        //We replace this with nothing as it's just used as a flag to attach
                                        //attach.Name = "CellarPass_Confirm.ics"
                                        //attach.Contents = Events.ExportRsvpV2ToICalendarGuest(rsvpResult, winery, calendarAddress, DestinationName)
                                        emailAttachment.Name = "CellarPass_Confirm.ics";
                                        var reservation = eventDAL.GetReservationDetailsbyReservationId(iReservationId);
                                        emailAttachment.Contents = ExportRsvpV2ToICalendarGuest(reservation, winery, calendarAddress, reservationData.DestinationName);

                                        //emailAttachment.Contents = Events.ExportRsvpV2ToICalendarGuest(rsvpResult, winery, calendarAddress, reservationData.DestinationName);
                                    }
                                    else
                                    {
                                        //attach = Nothing
                                        emailAttachment = null;
                                    }

                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            //attach = Nothing
                            emailAttachment = null;
                        }

                        //Replace here so that we remove it if it's blank
                        string DirectionsURL = "";
                        if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
                        {
                            DirectionsURL = "<a href=\"" + reservationData.MapAndDirectionsURL.Trim() + "\" target=\"_blank\">Get Directions</a>";
                        }
                        RsvpGuestContent = RsvpGuestContent.Replace("[[DirectionsURL]]", DirectionsURL);

                        //We replace this with nothing as it's just used as a flag to attach
                        RsvpGuestContent = RsvpGuestContent.Replace("[[?Attach_iCal]]", "");

                        string emailTo = "";

                        //Single Email
                        emailTo = AffiliateEmail;
                        //If there are cc emails add them to the guest email
                        if ((!object.ReferenceEquals(CCGuestEmail.Trim(), string.Empty)))
                        {
                            emailTo = emailTo + "," + CCGuestEmail.Trim();
                        }
                        //Send with New Mail (Mailgun)
                        response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.RsvpUpdate, iReservationId, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent, reservationData.WineryID, emailAttachment, ew.Id));


                    }
                }
                catch (Exception ex)
                {
                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpEmail", "SendAffiliateEmail: " + ex.ToString());
                }
                //#### RSVP Guest Email - END ####
            }


            if (SendAdminEmail)
            {
                //#### RSVP Admin Email - START ####
                try
                {
                    //NOTE: Commented Out and moved inside of rsvp loop - Using regular Guest Email Template Now instead of RsvpAdmin
                    //Dim MailTemplate As Mail.EmailTemplates = Mail.EmailTemplates.RsvpAdmin

                    string PhoneNumber = "";

                    string RsvpAdminContent = "";
                    string RsvpAdminSubjectContent = "";
                    //Dim GuestsAttending As String = ""
                    DateTime StartDate = reservationData.EventDate.Add(reservationData.StartTime);
                    DateTime EndDate = reservationData.EventDate.Add(reservationData.EndTime);
                    TimeSpan Duration = EndDate.Subtract(StartDate);
                    string BookedBy = "";
                    BookedBy = eventDAL.GetUser(reservationData.BookedById).AffiliateName;

                    //Get Affiliate Information
                    string AffiliateEmail = "";
                    string AffiliateName = "";
                    string AffiliateCompany = "";

                    int AffID = reservationData.AffiliateID;

                    if (AffID > 0)
                    {
                        var aff = eventDAL.GetUser(reservationData.AffiliateID);

                        if ((aff != null))
                        {
                            AffiliateEmail = aff.AffiliateEmail;
                            AffiliateName = aff.AffiliateName;
                            AffiliateCompany = aff.AffiliateCompany;
                        }
                    }

                    //Format Fee
                    string FormatFee = reservationData.Fee.ToString();
                    //string DepositPolicy = GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;

                    if (reservationData.Fee > 0)
                    {
                        FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")) + ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                        //FormatFee = string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.Fee) + ", " + GetDepositPoliciesEmail().Where(f => f.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                    }
                    else
                    {
                        FormatFee = "Complimentary";
                    }

                    //LOCATION ADDRESS
                    string lAddress1 = reservationData.MemberAddress1;
                    string lAddress2 = reservationData.MemberAddress2;
                    string lCity = reservationData.MemberCity;
                    string lState = reservationData.MemberState;
                    string lZip = reservationData.MemberZipCode;
                    string calendarAddress = "";

                    //Use Location based address instead of winery address if provided for location
                    if ((reservationData.locAddress1 != null))
                    {
                        if (reservationData.locAddress1.Trim().Length > 0)
                        {
                            lAddress1 = (reservationData.locAddress1 == null ? "" : reservationData.locAddress1);
                            lAddress2 = (reservationData.locAddress2 == null ? "" : reservationData.locAddress2);
                            lCity = (reservationData.locCity == null ? "" : reservationData.locCity);
                            lState = (reservationData.locState == null ? "" : reservationData.locState);
                            lZip = (reservationData.locZip == null ? "" : reservationData.locZip);
                        }
                    }

                    //Phone Number - check and use home first then work if phone is empty
                    if (!string.IsNullOrEmpty(reservationData.GuestPhone))
                    {
                        PhoneNumber = reservationData.GuestPhone;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(reservationData.GuestWPhone))
                        {
                            PhoneNumber = Utility.FormatTelephoneNumber(reservationData.GuestWPhone.ToString(), "US");
                        }
                    }

                    EmailContent ea = default(EmailContent);

                    if (alternativeEmailTemplate > 0)
                    {
                        ea = emailDAL.GetEmailContentByID(alternativeEmailTemplate);
                    }
                    else
                    {
                        ea = emailDAL.GetEmailContentByID(reservationData.EmailContentID);
                    }



                    if ((ea != null) && ea.Active == true)
                    {
                        //Value for Admin Confirm Status tag
                        string AdminConfirmMsg = "New Reservation Received";
                        if (isRsvpType == 1)
                        {
                            AdminConfirmMsg = "Rescheduled";
                        }
                        else if (isRsvpType == 2)
                        {
                            AdminConfirmMsg = "Reservation Updated";
                        }

                        //Configure/Format CancellationPolicy
                        string CancellationPolicy = "";
                        string CancelByDate = "";
                        if ((reservationData.Content != null))
                        {
                            if (reservationData.Content.Trim().Length > 0)
                            {
                                CancellationPolicy = reservationData.Content;

                                if (reservationData.CancelLeadTime > 50000)
                                {
                                    CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                }
                                else
                                {
                                    CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                                }
                            }
                        }

                        //Replace Content Tags

                        //Admin Email Subject
                        string adminEmailSubject = ea.EmailSubject;
                        if (!object.ReferenceEquals(ea.EmailSubjectAdmin.Trim(), string.Empty))
                        {
                            adminEmailSubject = ea.EmailSubjectAdmin;
                        }
                        //Subject
                        RsvpAdminSubjectContent = adminEmailSubject;
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[BookingCode]]", bookingCode);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[DestinationName]]", reservationData.DestinationName);

                        RsvpAdminSubjectContent = ReservationTagReplace(RsvpAdminSubjectContent, bookingGUID);

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpGuid]]", bookingGUID);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestName]]", reservationData.GuestName);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestPhone]]", PhoneNumber);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestAddress1]]", reservationData.GuestAddress1);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestAddress2]]", reservationData.GuestAddress2);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestAddressCity]]", reservationData.GuestCity);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestAddressState]]", reservationData.GuestState);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestAddressZipCode]]", reservationData.GuestZipCode);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[GuestsAttending]]", GuestsAttending);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[Fee]]", FormatFee);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[Gratuity]]", GratuityContent);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[PaymentStatus]]", PaymentDetails);

                        string bannerImageHtml = Utility.GetBannerImageHtmlByWineryId(reservationData.WineryID);

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberName]]", reservationData.MemberName);

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberAddress1]]", lAddress1);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberAddress2]]", lAddress2);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberAddressCity]]", lCity);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberAddressState]]", lState);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[MemberAddressZipCode]]", lZip);

                        string lFullAddress = string.Empty;
                        lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[LocationAddress]]", lFullAddress);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}", StartDate));

                        if (reservationData.CancelLeadTime > 50000)
                        {
                            RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                        }
                        else
                        {
                            RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                        }

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpDuration]]", Duration.ToString());
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                        //RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpHost]]", Host)
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[NotesSection]]", notesSection);

                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                        //No need for notes in subject - RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[Inter'nalNotes]]", InternalNote)
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[BookedBy]]", BookedBy);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[CancellationPolicy]]", "");
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[AdminConfirmStatus]]", AdminConfirmMsg);
                        RsvpAdminSubjectContent = RsvpAdminSubjectContent.Replace("[[ZoomMeeting]]", zoomContent);

                        //body
                        RsvpAdminContent = ea.EmailBody;
                        RsvpAdminContent = RsvpAdminContent.Replace("[[BookingCode]]", bookingCode);

                        RsvpAdminContent = ReservationTagReplace(RsvpAdminContent, bookingGUID);

                        RsvpAdminContent = RsvpAdminContent.Replace("[[DestinationName]]", reservationData.DestinationName);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpGuid]]", bookingGUID);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestName]]", reservationData.GuestName);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestPhone]]", PhoneNumber);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[GuestsAttending]]", GuestsAttending);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[Fee]]", FormatFee);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[Gratuity]]", GratuityContent);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[PaymentStatus]]", PaymentDetails);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberName]]", reservationData.MemberName);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberAddress1]]", lAddress1);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberAddress2]]", lAddress2);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberAddressCity]]", lCity);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberAddressState]]", lState);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[MemberAddressZipCode]]", lZip);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[LocationAddress]]", lFullAddress);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}", StartDate));

                        if (reservationData.CancelLeadTime > 50000)
                        {
                            RsvpAdminContent = RsvpAdminContent.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                        }
                        else
                        {
                            RsvpAdminContent = RsvpAdminContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                        }

                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpDuration]]", Duration.ToString());
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                        //RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpHost]]", Host)
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                        RsvpAdminContent = RsvpAdminContent.Replace("[[NotesSection]]", notesSection);

                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                        RsvpAdminContent = RsvpAdminContent.Replace("[[AffiliateName]]", AffiliateName);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[AffiliateCompany]]", AffiliateCompany);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[CancellationPolicy]]", CancellationPolicy);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[Personal_Message]]", perMsg);
                        RsvpAdminContent = RsvpAdminContent.Replace("[[ZoomMeeting]]", zoomContent);

                        //Admin Only Section
                        string RsvpAdminOnlyContent = ea.EmailBodyAdmin;
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[GuestAddress1]]", reservationData.GuestAddress1);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[GuestAddress2]]", reservationData.GuestAddress2);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[GuestAddressCity]]", reservationData.GuestCity);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[GuestAddressState]]", reservationData.GuestState);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[GuestAddressZipCode]]", reservationData.GuestZipCode);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[InternalNotes]]", reservationData.InternalNote);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[AffiliateNote]]", reservationData.ConciergeNote);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[BookedBy]]", BookedBy);
                        RsvpAdminOnlyContent = RsvpAdminOnlyContent.Replace("[[AdminConfirmStatus]]", AdminConfirmMsg);

                        //Add admin section to end of other content
                        RsvpAdminContent += "<br />" + RsvpAdminOnlyContent;

                        //Attach iCal File
                        //Dim attach As New Mail.Attachment
                        EmailAttachment emailAttachment = new EmailAttachment();

                        //string DirectionsURL = "";

                        try
                        {
                            if (RsvpAdminContent.IndexOf("[[?Attach_iCal]]") > 0)
                            {

                                if ((reservationData != null))
                                {
                                    if (RsvpAdminContent.IndexOf("[[?Attach_iCal]]") > 0)
                                    {
                                        var winery = new WineryModel();
                                        winery = eventDAL.GetWineryById(reservationData.WineryID);
                                        //We replace this with nothing as it's just used as a flag to attach
                                        //attach.Name = "CellarPass_Confirm.ics"
                                        //attach.Contents = Events.ExportRsvpV2ToICalendar(rsvpResult, DestinationName)
                                        emailAttachment.Name = "CellarPass_Confirm.ics";
                                        var reservation = eventDAL.GetReservationDetailsbyReservationId(iReservationId);
                                        emailAttachment.Contents = ExportRsvpV2ToICalendar(reservation, reservationData.DestinationName, LocalBookingDate);
                                        //emailAttachment.Contents = Events.ExportRsvpV2ToICalendar(rsvpResult, reservationData.DestinationName);
                                    }
                                    else
                                    {
                                        //attach = Nothing
                                        emailAttachment = null;
                                    }

                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            //attach = Nothing
                            emailAttachment = null;
                        }

                        //Replace here so that we remove it if it's blank
                        string DirectionsURL = "";
                        if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
                        {
                            DirectionsURL = "<a href=\"" + reservationData.MapAndDirectionsURL.Trim() + "\" target=\"_blank\">Get Directions</a>";
                        }
                        RsvpAdminContent = RsvpAdminContent.Replace("[[DirectionsURL]]", DirectionsURL);

                        //We replace this with nothing as it's just used as a flag to attach
                        RsvpAdminContent = RsvpAdminContent.Replace("[[?Attach_iCal]]", "");
                        for (int value = 1; value <= 3; value++)
                        {
                            if ((value == 1) || (CheckSendmail == false))
                            {
                                try
                                {
                                    response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.RsvpUpdate, iReservationId, ea.EmailFrom, ea.EmailTo, RsvpAdminSubjectContent, RsvpAdminContent, reservationData.WineryID, emailAttachment, ea.Id));
                                    //CheckSendmail = Mail.SendMail(MailTemplate, RsvpAdminContent, RsvpAdminSubjectContent, ea.EmailFrom, ea.EmailTo, null, 0, , attach);
                                }
                                catch (Exception ex)
                                {
                                    if (value == 3) { }
                                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpEmail", "SendAdminEmail: " + ex.ToString);
                                }
                            }
                            if (CheckSendmail == false)
                            {
                                if (value == 3)
                                {
                                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpEmail", "SendAdminEmail: ReservationId" + r.ReservationID);
                                }
                                else
                                {
                                    System.Threading.Thread.Sleep(30000);
                                }
                            }
                            else
                            {
                                break; // TODO: might not be correct. Was : Exit For
                            }
                        }
                        //Send with New Mail (Mailgun)
                        //response = await Task.Run(() => SendMailAsync(model.MailConfig, ea.EmailFrom, ea.EmailTo, RsvpAdminSubjectContent, RsvpAdminContent,emailAttachment));


                    }
                }
                catch (Exception ex)
                {
                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpEmail", "SendAdminEmail: " + ex.ToString());
                }
                //#### RSVP Admin Email - END ####
            }
            return response;
        }

        /// <summary>
        /// This method is used to send email on cancel reservation
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<EmailResponse> SendCpRsvpCancelEmailV2(ReservationEmailModel model)
        {

            var response = new EmailResponse();
            if (model.data.RsvpId == 0)
            {
                return response;
            }

            //Get Data for Email
            EventDAL eventDAL = new EventDAL(ConnectionString);
            EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);

            //if (eventDAL.AlreadySentRsvpCancelEmail(model.data.RsvpId))
            //{
            //    return response;
            //}

            //Get Data for Email
            var reservationData = await Task.Run(() => eventDAL.GetReservationEmailDataByReservationId(model.data.RsvpId, 0, ""));
            int WineryID = reservationData.WineryID;
            string BookingCode = reservationData.BookingCode;
            string bookingGUID = reservationData.BookingGUID;
            DateTime LocalBookingDate = Times.ToTimeZoneTime(reservationData.BookingDate, (Times.TimeZone)reservationData.TimeZoneId);
            string GuestName = reservationData.GuestName;
            string GuestEmail = reservationData.GuestEmail;
            string GuestPhone = reservationData.GuestPhone;
            string GuestWPhone = reservationData.GuestWPhone;
            string GuestAddress1 = reservationData.GuestAddress1;
            string GuestAddress2 = reservationData.GuestAddress2;
            string GuestCity = reservationData.GuestCity;
            string GuestState = reservationData.GuestState;
            string GuestZipCode = reservationData.GuestZipCode;
            decimal Fee = reservationData.Fee;
            int ChargeFee = reservationData.ChargeFee;
            decimal FeePaid = reservationData.FeePaid;
            short GuestCount = reservationData.GuestCount;
            string MemberName = reservationData.MemberName;
            string MemberPhone = reservationData.MemberPhone;
            string MemberEmail = reservationData.MemberEmail;
            string MemberAddress1 = reservationData.MemberAddress1;
            string MemberAddress2 = reservationData.MemberAddress2;
            string MemberCity = reservationData.MemberCity;
            string MemberState = reservationData.MemberState;
            string MemberZipCode = reservationData.MemberZipCode;
            DateTime EventDate = reservationData.EventDate;
            TimeSpan StartTime = reservationData.StartTime;
            TimeSpan EndTime = reservationData.EndTime;
            string EventName = reservationData.EventName;
            string EventLocation = reservationData.EventLocation;
            string Notes = reservationData.Notes;
            int Status = Convert.ToInt32(reservationData.Status);
            string InternalNote = reservationData.InternalNote;
            int BookedById = reservationData.BookedById;
            int CancelLeadTime = reservationData.CancelLeadTime;
            int EmailContentID = reservationData.EmailContentID;
            int EmailTemplateID = reservationData.EmailTemplateID;
            string Host = reservationData.Host;
            int AffiliateID = reservationData.AffiliateID;
            string Content = reservationData.Content;
            string DestinationName = reservationData.DestinationName;
            string locAddress1 = reservationData.locAddress1;
            string locAddress2 = reservationData.locAddress2;
            string locCity = reservationData.locCity;
            string locState = reservationData.locState;
            string locZip = reservationData.locZip;
            string CancellationMessage = reservationData.EventCancellationMessage;
            string MapURL = "";
            string zoomContent = string.Empty;
            bool SendGuestEmail = model.data.GuestEmail;
            string CancellationReason = reservationData.CancellationReason;

            if (string.IsNullOrEmpty(CancellationReason))
                CancellationReason = "Not provided";

            string GratuityContent = "";
            if (reservationData.GratuityAmount > 0)
            {
                string GratuityHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"419\" style=\"background-color: #ffffff;width: 419px;padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-69p84\" style=\"max-width: 320px;min-width: 419.04px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\">Gratuity:      </span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"180\" style=\"background-color: #ffffff;width: 180px;padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-30p16\" style=\"max-width: 320px;min-width: 180.96px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\">[[GratuityAmt]]</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

                GratuityHtml = GratuityHtml.Replace("[[GratuityAmt]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.GratuityAmount));

                GratuityContent = GratuityHtml;

            }

            ZoomMeetingInfo zoomMeetingInfo = eventDAL.GetZoomMeetingInfo(reservationData.slotid, reservationData.slottype, reservationData.EventDate, reservationData.ReservationId);

            if (zoomMeetingInfo != null && zoomMeetingInfo.Id > 0)
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)Common.Common.SettingGroup.member);


                bool requirePassword = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_zoom_require_password);
                string zoomMeetingPassword = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_zoom_password);

                if (requirePassword && !string.IsNullOrEmpty(zoomMeetingPassword))
                {
                    zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}<br>Password: {3}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()), zoomMeetingPassword);
                }
                else
                {
                    zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()));
                }
            }

            string googleAPIKey = "AIzaSyDKGd8SyV0GhpzD-onvM0ItLpzq5GhCFfA";
            if (_appSetting != null && _appSetting.Value != null && !string.IsNullOrEmpty(_appSetting.Value.GoogleAPIKey))
            {
                googleAPIKey = _appSetting.Value.GoogleAPIKey;
            }

            string map_and_directions_url = "";
            string mapInageURl = "";
            if (reservationData.LocationId > 0)
            {
                MapURL = await Utility.GetMapImageHtmlByLocation(reservationData.LocationId, googleAPIKey);

                var location = eventDAL.GetLocationMapDataByID(reservationData.LocationId);

                if (location != null && location.location_id > 0)
                {
                    map_and_directions_url = location.map_and_directions_url;
                    mapInageURl = "https://cdncellarpass.blob.core.windows.net/photos/location_maps/" + reservationData.LocationId.ToString() + "_dot.jpg";
                }
            }

            string addOnItems = "";
            string AddOnDetails = "";
            StringBuilder AddOnItemDetails = new StringBuilder();
            string addOnHeading = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 10px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\"><strong><span style=\"font-size: 14px; line-height: 14px;\">ADDITIONAL SELECTIONS</span></strong> </span> </p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
            string addOnItem = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"439\" style=\"background-color: #ffffff;width: 439px;padding: 0px 0px 0px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-73p33\" style=\"max-width: 320px;min-width: 439.98px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 0px 0px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">{{AddOnQty}} X {{AddOnItem}}</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"160\" style=\"background-color: #ffffff;width: 160px;padding: 0px 30px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-26p67\" style=\"max-width: 320px;min-width: 160.02px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 30px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">{{AddOnPrice}}</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

            if (reservationData.ReservationId > 0)
            {
                var listAddOns = eventDAL.GetReservationAddOnItems(reservationData.ReservationId);
                if (listAddOns != null)
                {
                    int idx = 0;
                    foreach (var addon in listAddOns)
                    {
                        string item1 = addOnItem;
                        item1 = item1.Replace("{{AddOnQty}}", addon.Qty.ToString());
                        item1 = item1.Replace("{{AddOnItem}}", addon.Name);
                        item1 = item1.Replace("{{AddOnPrice}}", addon.Price.ToString("C", new CultureInfo("en-US")));
                        AddOnItemDetails.Append(item1);

                        idx += 1;
                        string numberedList = "";
                        if (addon.ItemType == (int)Common.AddOnGroupType.menu)
                        {
                            numberedList = string.Format("{0}. ", idx);
                        }
                        addOnItems += string.Format("{0}{1} ({2}) {3}<br />", numberedList, addon.Name, addon.Qty, (string.IsNullOrWhiteSpace(Convert.ToString(addon.Price)) ? "0.00" : addon.Price.ToString("N2")));
                    }
                }
            }

            AddOnDetails = addOnHeading + AddOnItemDetails.ToString();

            if (string.IsNullOrEmpty(addOnItems))
                AddOnDetails = "";

            string PaymentDetails = "";
            string paymentItem = "<p style=\" font-size:14px; line-height: 100%;\"><span style=\"font-family:Poppins, sans-serif; font-size: 14px; line-height: 14px;\">{{PaymentDesc}}</span></p>";

            if (reservationData.Fee == 0)
            {
                string PaymentStatus = paymentItem;
                PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", "Complimentary");

                PaymentDetails = PaymentDetails + PaymentStatus;
            }
            else
            {
                foreach (var item in eventDAL.GetPaymentStatusV2byReservationId(reservationData.ReservationId))
                {
                    string PaymentStatus = paymentItem;
                    PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", item);

                    PaymentDetails = PaymentDetails + PaymentStatus;
                }
            }

            //'LOCATION / ZOOM 'NOTE: We do not show the zoom in the preview unless you swich this manually here. 
            bool sendPreviewWithZoom = false;

            if (reservationData.EventId > 0)
            {
                EventModel eventModelValidate = eventDAL.GetEventById(reservationData.EventId);

                if (eventModelValidate != null && eventModelValidate.EventID > 0)
                {
                    if (eventModelValidate.EventTypeId == 34 && eventModelValidate.MeetingBehavior == 2)
                    {
                        sendPreviewWithZoom = true;
                    }
                }
            }

            //LOCATION ADDRESS
            string lAddress1 = reservationData.MemberAddress1;
            string lAddress2 = reservationData.MemberAddress2;
            string lCity = reservationData.MemberCity;
            string lState = reservationData.MemberState;
            string lZip = reservationData.MemberZipCode;
            string calendarAddress = "";
            //Use Location based address instead of winery address if provided for location
            if ((reservationData.locAddress1 != null))
            {
                if (reservationData.locAddress1.Trim().Length > 0)
                {
                    lAddress1 = (reservationData.locAddress1 == null ? "" : reservationData.locAddress1);
                    lAddress2 = (reservationData.locAddress2 == null ? "" : reservationData.locAddress2);
                    lCity = (reservationData.locCity == null ? "" : reservationData.locCity);
                    lState = (reservationData.locState == null ? "" : reservationData.locState);
                    lZip = (reservationData.locZip == null ? "" : reservationData.locZip);
                }
            }

            //Calendar address
            calendarAddress += lAddress1 + "\\n ";
            if (lAddress2.Trim().Length > 0)
            {
                calendarAddress += lAddress2 + "\\n ";
            }
            calendarAddress += lCity + ", " + lState + " " + lZip + "\\n ";

            string lFullAddress = string.Empty;
            lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);

            //Replace here so that we remove it if it's blank
            string DirectionsURL = "";
            if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
            {
                DirectionsURL = reservationData.MapAndDirectionsURL;
            }
            //string DestinationName = reservationData.DestinationName;
            string locationSection = "";
            string locationHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #ffffff;width: 300px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>[[DestinationName]]</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[LocationAddress]]</span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[MemberPhone]]</span></p> <p style=\"font-size: 14px; line-height: 140%;\"> </p> </div> </td> </tr> </tbody> </table> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[DirectionsURL]]\" style=\"height:48px; v-text-anchor:middle; width:238px;\" arcsize=\"8.5%\" strokecolor=\"#236fa1\" strokeweight=\"1px\" fillcolor=\"#ffffff\"><w:anchorlock/><center style=\"color:#236fa1;font-family:'Poppins', sans-serif;;\"><![endif]--> <a href=\"[[DirectionsURL]]\" target=\"_blank\" class=\"v-button\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #236fa1; background-color: #ffffff; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:100%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;border-top-color: #236fa1; border-top-style: solid; border-top-width: 1px; border-left-color: #236fa1; border-left-style: solid; border-left-width: 1px; border-right-color: #236fa1; border-right-style: solid; border-right-width: 1px; border-bottom-color: #236fa1; border-bottom-style: solid; border-bottom-width: 1px;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"font-size: 14px; line-height: 16.8px; font-family: Poppins, sans-serif;\">GET DIRECTIONS</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #ffffff;width: 300px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;bordear-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div> [[MapURL]] </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
            string zoomHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Zoom Meeting Information</strong></span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">We are using Zoom to host our virtual tastings which will require you to take some additional steps to ensure you can connect to our virtual tasting without any delays. This will require you to enter the Zoom MeetingID and Zoom passport assigned to you.</span></p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[ZoomMeeting]] </span></p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>[[MemberName]] Technical Support</strong></span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">Should you require any assistance with connecting to our virtual tasting, please contact us immediately at [[MemberPhone]]</span></p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Zoom Technical Support</strong></span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">If you have questions or need additional assistance with using Zoom, please contact their <a rel=\"noopener\" href=\"https://support.zoom.us/hc/en-us\" target=\"_blank\">Technical Support</a>.</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
            string mapHtml = "<table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"> <tr> <td style=\"padding-right: 0px;padding-left: 0px;\" align=\"center\"> <a href=\"[[map_and_directions_url]]\" target=\"_blank\"> <img align=\"center\" border=\"0\" src=\"[[mapInageURl]]\" alt=\"Map Image\" title=\"Map Image\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 100%;max-width: 280px;\" width=\"280\" /> </a> </td> </tr> </table> </td> </tr> </tbody> </table>";

            //'Replace tags in location html
            locationHtml = locationHtml.Replace("[[DestinationName]]", DestinationName);
            locationHtml = locationHtml.Replace("[[LocationAddress]]", lFullAddress);
            locationHtml = locationHtml.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
            locationHtml = locationHtml.Replace("[[DirectionsURL]]", DirectionsURL);

            mapHtml = mapHtml.Replace("[[map_and_directions_url]]", map_and_directions_url);
            mapHtml = mapHtml.Replace("[[mapInageURl]]", mapInageURl);

            locationHtml = locationHtml.Replace("[[MapURL]]", mapHtml);

            //'by default it's set to location
            locationSection = locationHtml;

            //string zoomContent = string.Empty;

            //ZoomMeetingInfo zoomMeetingInfo = eventDAL.GetZoomMeetingInfo(reservationData.slotid, reservationData.slottype, reservationData.EventDate, reservationData.ReservationId);

            //if (zoomMeetingInfo != null && zoomMeetingInfo.Id > 0)
            //{
            //    SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

            //    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)Common.Common.SettingGroup.member);


            //    bool requirePassword = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_zoom_require_password);
            //    string zoomMeetingPassword = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_zoom_password);

            //    if (requirePassword && !string.IsNullOrEmpty(zoomMeetingPassword))
            //    {
            //        zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}<br>Password: {3}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()), zoomMeetingPassword);
            //    }
            //    else
            //    {
            //        zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()));
            //    }
            //}

            //'if set to show zoom we use it instead
            if (sendPreviewWithZoom)
            {
                //'Replace tags in Zoom html
                zoomHtml = zoomHtml.Replace("[[ZoomMeeting]]", zoomContent);
                zoomHtml = zoomHtml.Replace("[[MemberName]]", DestinationName);
                zoomHtml = zoomHtml.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));

                locationSection = zoomHtml;
            }

            string notesSection = "";

            //Preview sample messages
            string personalMSg = model.perMsg;
            string guestNote = reservationData.Notes;

            //NOTES

            string notesSectionHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\">[[Notes]]</p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
            string notesCombined = "";
            string personlMsgHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Personal Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[Personal_Message]]</span><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"></span></p>";
            string guestNoteHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Guest Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[RsvpNote]]</span></p>";

            //If either personal Message or guest note is available we need to render this section
            bool showNoteSection = false;

            if (!string.IsNullOrEmpty(personalMSg))
            {
                showNoteSection = true;
                //Replace message tags
                personlMsgHtml = personlMsgHtml.Replace("[[Personal_Message]]", personalMSg);
                //Add to notes
                notesCombined += personlMsgHtml;
            }

            if (!string.IsNullOrEmpty(guestNote))
            {
                showNoteSection = true;

                //Replace message tags
                guestNoteHtml = guestNoteHtml.Replace("[[RsvpNote]]", guestNote);

                //If personl msg is not empty add this space html first before adding the guest note to create some separation
                if (notesCombined.Length > 0)
                {
                    notesCombined += "<p style=\"font-size: 14px; line-height: 120%;\"> </p>";
                }

                //Add to notes
                notesCombined += guestNoteHtml;
            }

            //If either personal or guest note was provided this should be true and we combine it all.
            if (showNoteSection)
            {
                //replace tag in notes section html with notes combined
                notesSectionHtml = notesSectionHtml.Replace("[[Notes]]", notesCombined);
                //Set noteSection to Html
                notesSection = notesSectionHtml;
            }

            if (model.data.ActionSource == (int)ActionSource.Consumer)
            {
                //#### CP RSVP Email - START ####
                try
                {
                    //Get Email Template Content for CP RSVP Confirmation
                    EmailContent ec = new EmailContent();
                    ec = emailDAL.GetEmailContent((int)EmailTemplates.RsvpSysCancel, 0);

                    if (ec == null || ec.Active == false)
                    {
                        //break; // TODO: might not be correct. Was : Exit Try
                    }

                    string PhoneNumber = "";

                    //Parse Content
                    ArrayList BccEmailList = new ArrayList();
                    string StartToken = "{Repeat}";
                    string EndToken = "{/Repeat}";
                    string EmailContent = ec.EmailBody;
                    string BeforeRepeat = "";
                    string AfterRepeat = "";
                    string RepeatSection = "";
                    string RepeatReplaced = "";
                    string RepeatCompleted = "";
                    int RepeatStartInt = 0;
                    int RepeatEndInt = 0;
                    int LoopCount = 0;
                    string EmailSubject = ec.EmailSubject;

                    //See if there is a repeating section
                    RepeatStartInt = EmailContent.IndexOf(StartToken);
                    RepeatEndInt = EmailContent.IndexOf(EndToken);

                    if (RepeatStartInt > 0 && RepeatEndInt > 0)
                    {
                        BeforeRepeat = Common.Common.Left(EmailContent, RepeatStartInt - 1);
                        AfterRepeat = Common.Common.Right(EmailContent, EmailContent.Length - ((RepeatEndInt + EndToken.Length) - 1));
                        RepeatSection = Common.StringHelpers.ParseBetweenTags(StartToken, EndToken, EmailContent);
                    }
                    else
                    {
                        BeforeRepeat = EmailContent;
                        RepeatSection = "";
                        AfterRepeat = "";
                    }



                    //Format Fee
                    string FormatFee = Fee.ToString();
                    string DepositPolicy = GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;

                    //if (Fee > 0)
                    //{
                    //    FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")); //+ ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                    //    //FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")) + ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                    //}
                    //else
                    //{
                    //    FormatFee = "Complimentary";
                    //}

                    FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US"));

                    string GuestsAttending = "";

                    //Phone Number - check and use home first then work if phone is empty
                    if (!string.IsNullOrEmpty(GuestPhone))
                    {
                        PhoneNumber = GuestPhone;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(GuestWPhone))
                        {
                            PhoneNumber = Utility.FormatTelephoneNumber(GuestWPhone.ToString(), "US");
                        }
                    }

                    //Need to get the guests attending each reservation in the booking
                    //Get Guests Detail
                    //Dim guests = From g In db.RsvpGuestsDetails _
                    //Where g.ReservationId = reservationId

                    //Configure/Format CancellationPolicy
                    string CancellationPolicy = "";
                    string CancelByDate = "";
                    if ((Content != null))
                    {
                        if (Content.Trim().Length > 0)
                        {
                            CancellationPolicy = Content;

                            if (reservationData.CancelLeadTime > 50000)
                            {
                                CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                            }
                            else
                            {
                                CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(RepeatSection))
                    {
                        DateTime StartDate = EventDate.Add(StartTime);
                        DateTime EndDate = EventDate.Add(EndTime);
                        TimeSpan Duration = EndDate.Subtract(StartDate);

                        RepeatReplaced = RepeatSection;
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_Fee]]", Fee.ToString());
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_GuestCount]]", GuestCount.ToString());
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_MemberName]]", MemberName);
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_MemberPhone]]", Utility.FormatTelephoneNumber(MemberPhone.ToString(), "US"));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_MemberEmail]]", MemberEmail);
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpTime]]", string.Format("{0:hh:mm tt}", StartDate));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpDuration]]", Duration.ToString());
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpEvent]]", EventName);
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpDestination]]", DestinationName);
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpLocation]]", EventLocation);
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_GuestsAttending]]", GuestsAttending);
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpNote]]", (Notes == null ? "" : Notes));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpStatus]]", GetReservationStatus(Status));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_CancellationPolicy]]", CancellationPolicy);

                        //if (rsvp.Count > 1)
                        //{
                        //    //Append <br>
                        //    RepeatReplaced += "<br>";
                        //}

                        RepeatCompleted += RepeatReplaced;
                    }

                    //This is the same for each returned item so we only set it on the first loop.
                    if (LoopCount == 0)
                    {
                        if (EmailSubject.Trim().Length > 0)
                        {
                            EmailSubject = EmailSubject.Replace("[[BookingCode]]", BookingCode);

                            EmailSubject = ReservationTagReplace(EmailSubject, bookingGUID);

                            EmailSubject = EmailSubject.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            EmailSubject = EmailSubject.Replace("[[GuestName]]", GuestName);
                            EmailSubject = EmailSubject.Replace("[[GuestEmail]]", GuestEmail);
                            EmailSubject = EmailSubject.Replace("[[GuestPhone]]", PhoneNumber);
                            EmailSubject = EmailSubject.Replace("[[DestinationName]]", DestinationName);
                        }

                        if (!string.IsNullOrEmpty(BeforeRepeat))
                        {
                            BeforeRepeat = BeforeRepeat.Replace("[[BookingCode]]", BookingCode);

                            BeforeRepeat = ReservationTagReplace(BeforeRepeat, bookingGUID);

                            BeforeRepeat = BeforeRepeat.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            BeforeRepeat = BeforeRepeat.Replace("[[GuestName]]", GuestName);
                            BeforeRepeat = BeforeRepeat.Replace("[[GuestEmail]]", GuestEmail);
                            BeforeRepeat = BeforeRepeat.Replace("[[GuestPhone]]", PhoneNumber);
                            BeforeRepeat = BeforeRepeat.Replace("[[DestinationName]]", reservationData.DestinationName);
                        }

                        if (!string.IsNullOrEmpty(AfterRepeat))
                        {
                            AfterRepeat = AfterRepeat.Replace("[[BookingCode]]", BookingCode);

                            AfterRepeat = ReservationTagReplace(AfterRepeat, bookingGUID);

                            AfterRepeat = AfterRepeat.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            AfterRepeat = AfterRepeat.Replace("[[GuestName]]", GuestName);
                            AfterRepeat = AfterRepeat.Replace("[[GuestEmail]]", GuestEmail);
                            AfterRepeat = AfterRepeat.Replace("[[GuestPhone]]", PhoneNumber);
                            AfterRepeat = AfterRepeat.Replace("[[DestinationName]]", reservationData.DestinationName);
                        }
                    }
                    LoopCount += 1;

                    //Remove Trailing <br>
                    if (RepeatCompleted.Length > 5)
                    {
                        RepeatCompleted = Common.Common.Left(RepeatCompleted, RepeatCompleted.Length - 6);
                    }

                    string FinalContent = BeforeRepeat + RepeatCompleted + AfterRepeat;

                    string emailTo = GuestEmail;

                    string RsvpAdminEmailTo = string.Empty;

                    if (reservationData.CancellationUsers.Length > 0)
                    {
                        UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                        List<string> listUsers = reservationData.CancellationUsers.Split(",").ToList();

                        foreach (var item in listUsers)
                        {
                            if (RsvpAdminEmailTo.Length > 0)
                                RsvpAdminEmailTo = RsvpAdminEmailTo + ",";

                            RsvpAdminEmailTo = RsvpAdminEmailTo + userDAL.GetUserEmailById(Convert.ToInt32(item));
                        }
                    }

                    SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);

                    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)SettingGroup.member);

                    if (reservationData.EventId == 0)
                        RsvpAdminEmailTo = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_private_booking_notifications_receiver);

                    if (RsvpAdminEmailTo.Length == 0)
                        RsvpAdminEmailTo = ec.EmailTo;

                    //Send Mail

                    string member_rsvp_contact_email = Settings.GetStrValue(settingsGroup, SettingKey.member_rsvp_contact_email);


                    if (SendGuestEmail)
                        response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.RsvpCancel, model.RsvpId, ec.EmailFrom, emailTo, EmailSubject, FinalContent, reservationData.WineryID, null, ec.Id, member_rsvp_contact_email, DestinationName));

                    response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.RsvpCancel, model.RsvpId, ec.EmailFrom, RsvpAdminEmailTo, EmailSubject, FinalContent, reservationData.WineryID, null, ec.Id, member_rsvp_contact_email, DestinationName));

                }
                catch (Exception ex)
                {
                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpCancelEmail", "RsvpSysCancel: " + ex.ToString);
                }
                //#### CP RSVP Email - END ####
            }

            if (model.data.ActionSource == (int)ActionSource.BackOffice)
            {
                //#### RSVP Guest Email - START ####
                try
                {
                    EmailTemplates MailTemplate = EmailTemplates.RsvpGuestCancel;

                    string PhoneNumber = "";


                    string RsvpGuestContent = "";
                    string GuestsAttending = "";
                    ArrayList BccEmailList = new ArrayList();
                    DateTime StartDate = EventDate.Add(StartTime);
                    DateTime EndDate = EventDate.Add(EndTime);
                    TimeSpan Duration = EndDate.Subtract(StartDate);
                    string RsvpGuestSubjectContent = "";

                    string timezoneName = reservationData.timezone_name;

                    if (!string.IsNullOrEmpty(timezoneName))
                    {
                        timezoneName = " (" + timezoneName + ")";
                    }
                    else
                    {
                        timezoneName = "";
                    }

                    //Format Fee
                    string FormatFee = Fee.ToString();
                    string DepositPolicy = GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;

                    //if (Fee > 0)
                    //{
                    //    FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")); //+ ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                    //    //FormatFee = string.Format(new CultureInfo("en-US"), "{0:C}", Fee) + ", " + GetDepositPoliciesEmail().Where(f => f.ID == ChargeFee).FirstOrDefault().Name;
                    //}
                    //else
                    //{
                    //    FormatFee = "Complimentary";
                    //}

                    FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US"));

                    //Phone Number - check and use home first then work if phone is empty
                    if (!string.IsNullOrEmpty(GuestPhone))
                    {
                        PhoneNumber = GuestPhone;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(GuestWPhone))
                        {
                            PhoneNumber = Utility.FormatTelephoneNumber(GuestWPhone.ToString(), "US");
                        }
                    }

                    EmailContent ew = default(EmailContent);
                    ew = emailDAL.GetEmailContent((int)EmailTemplates.RsvpSysCancel, 0);


                    if ((ew != null) && ew.Active == true)
                    {
                        //Need to get the guests attending each reservation in the booking
                        //Get Guests Detail

                        string CancellationPolicy = "";
                        string CancelByDate = "";
                        if ((Content != null))
                        {
                            if (Content.Trim().Length > 0)
                            {
                                CancellationPolicy = Content;

                                if (reservationData.CancelLeadTime > 50000)
                                {
                                    CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                }
                                else
                                {
                                    CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                                }
                            }
                        }

                        //LOCATION ADDRESS
                        //string lAddress1 = MemberAddress1;
                        //string lAddress2 = MemberAddress2;
                        //string lCity = MemberCity;
                        //string lState = MemberState;
                        //string lZip = MemberZipCode;

                        //Use Location based address instead of winery address if provided for location
                        if ((locAddress1 != null))
                        {
                            if (locAddress1.Trim().Length > 0)
                            {
                                lAddress1 = locAddress1;
                                lAddress2 = locAddress2;
                                lCity = locCity;
                                lState = locState;
                                lZip = locZip;
                            }
                        }

                        //Replace Content Tags

                        //Subject
                        RsvpGuestSubjectContent = ew.EmailSubject;
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingCode]]", BookingCode);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DestinationName]]", DestinationName);

                        RsvpGuestSubjectContent = ReservationTagReplace(RsvpGuestSubjectContent, bookingGUID);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestName]]", GuestName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestEmail]]", GuestEmail);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestPhone]]", PhoneNumber);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestCount]]", GuestCount.ToString());
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestsAttending]]", GuestsAttending);
                        //RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FormatFee);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Gratuity]]", GratuityContent);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeeRefund]]", (model.RefundAmount > 0 ? string.Format(new CultureInfo("en-US"), "{0:C}", model.RefundAmount) : ""));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[PaymentStatus]]", PaymentDetails);

                        //if (eventDAL.GetPaymentStatusV2byReservationId(model.RsvpId) == "Refunded") {
                        //    var FeeFormat = reservationData.Fee.ToString("C", new CultureInfo("en-US")) + ", " + "Fee(s) Refunded";
                        //    RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FeeFormat);
                        //}
                        //else
                        //{
                        //    RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FormatFee);
                        //}

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FormatFee);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DepositPolicy]]", DepositPolicy);

                        string bannerImageHtml = Utility.GetBannerImageHtmlByWineryId(reservationData.WineryID);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberName]]", MemberName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(MemberPhone.ToString(), "US"));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberEmail]]", MemberEmail);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress1]]", lAddress1);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress2]]", lAddress2);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressCity]]", lCity);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressState]]", lState);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressZipCode]]", lZip);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));

                        //string lFullAddress = string.Empty;
                        lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[LocationAddress]]", lFullAddress);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDuration]]", Duration.ToString());
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpEvent]]", EventName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancellationReason]]", CancellationReason);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[rpt_RsvpEvent]]", EventName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDestination]]", DestinationName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpLocation]]", EventLocation);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[NotesSection]]", notesSection);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(Notes) ? "None" : Notes));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpStatus]]", GetReservationStatus(Status));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancellationPolicy]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[ZoomMeeting]]", zoomContent);

                        //body
                        RsvpGuestContent = ew.EmailBody;
                        RsvpGuestContent = RsvpGuestContent.Replace("[[BookingCode]]", BookingCode);

                        RsvpGuestContent = ReservationTagReplace(RsvpGuestContent, bookingGUID);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[DestinationName]]", reservationData.DestinationName);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestName]]", GuestName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", GuestEmail);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestPhone]]", PhoneNumber);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestCount]]", GuestCount.ToString());
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestsAttending]]", GuestsAttending);
                        //RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FormatFee);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[Gratuity]]", GratuityContent);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[FeeRefund]]", (model.RefundAmount > 0 ? string.Format(new CultureInfo("en-US"), "{0:C}", model.RefundAmount) : ""));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[PaymentStatus]]", PaymentDetails);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[Add-Ons]]", addOnItems);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[AddOnItems]]", AddOnDetails);

                        //if (eventDAL.GetPaymentStatusV2byReservationId(model.RsvpId) == "Refunded")
                        //{
                        //    var FeeFormat = reservationData.Fee.ToString("C", new CultureInfo("en-US")) + ", " + "Fee(s) Refunded";
                        //    RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FeeFormat);
                        //}
                        //else
                        //{
                        //    RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FormatFee);
                        //}

                        RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FormatFee);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[DepositPolicy]]", DepositPolicy);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", MemberName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(MemberPhone.ToString(), "US"));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", MemberEmail);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress1]]", lAddress1);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress2]]", lAddress2);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressCity]]", lCity);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressState]]", lState);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressZipCode]]", lZip);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[LocationAddress]]", lFullAddress);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDuration]]", Duration.ToString());
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpEvent]]", EventName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationReason]]", CancellationReason);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[rpt_RsvpEvent]]", EventName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDestination]]", DestinationName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpLocation]]", EventLocation);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(Notes) ? "None" : Notes));

                        RsvpGuestContent = RsvpGuestContent.Replace("[[NotesSection]]", notesSection);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpStatus]]", GetReservationStatus(Status));

                        RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationPolicy]]", CancellationPolicy);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[ZoomMeeting]]", zoomContent);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationMessage]]", CancellationMessage);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MapURl]]", MapURL);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[LocationSection]]", locationSection);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[ConfirmationMessage]]", reservationData.EventConfirmationMessage);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationMessage]]", reservationData.EventCancellationMessage);

                        string strpath = "https://typhoon.cellarpass.com/";
                        if (ConnectionString.IndexOf("live") > -1)
                            strpath = "https://www.cellarpass.com/";

                        string profileUrl = string.Format("{1}business/{0}", reservationData.ProfileUrl, strpath);

                        RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                        RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[MemberLinkButton]]", profileUrl);
                        RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                        RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[MemberLinkButton]]", profileUrl);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberLinkButton]]", profileUrl);

                        int isfaq = emailDAL.CheckFAQExistsForWinery(WineryID);

                        if (isfaq == 1)
                        {
                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");

                            RsvpGuestContent = RsvpGuestContent.Replace("[[FAQButton]]", profileUrl + "?f=1");
                        }
                        else
                        {
                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[FAQButton]]", profileUrl);
                        }

                        string emailTo = GuestEmail;

                        //Add Email To fields to BCC so Admin can get a copy of cancel email
                        //if ((ew.EmailTo != null))
                        //{
                        //    if (ew.EmailTo.Trim().Length > 0)
                        //    {
                        //        emailTo = emailTo + "," + ew.EmailTo;
                        //    }
                        //}

                        SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);

                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)SettingGroup.member);



                        //Send Mail
                        //Email.SendEmail(Email.EmailType.RsvpCancel, Email.EmailProvider.Mailgun, reservationId, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent, WineryID, null);
                        if ((model.CCGuestEmail + "").Length > 0 && model.CCGuestEmail.IndexOf("@noemail") == -1)
                        {
                            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.RsvpCancel, model.RsvpId, ew.EmailFrom, model.CCGuestEmail, RsvpGuestSubjectContent, RsvpGuestContent, reservationData.WineryID, null, ew.Id, Settings.GetStrValue(settingsGroup, SettingKey.member_rsvp_contact_email), DestinationName));
                        }
                        else
                        {
                            if (GuestEmail.IndexOf("@noemail") == -1 && SendGuestEmail)
                            {
                                response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.RsvpCancel, model.RsvpId, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent, reservationData.WineryID, null, ew.Id, Settings.GetStrValue(settingsGroup, SettingKey.member_rsvp_contact_email), DestinationName));
                            }

                            string RsvpAdminEmailTo2 = "";

                            if (reservationData.EventId > 0 && reservationData.CancellationUsers.Length > 0)
                            {
                                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                                List<string> listUsers = reservationData.CancellationUsers.Split(",").ToList();

                                foreach (var item in listUsers)
                                {
                                    if (RsvpAdminEmailTo2.Length > 0)
                                        RsvpAdminEmailTo2 = RsvpAdminEmailTo2 + ",";

                                    RsvpAdminEmailTo2 = RsvpAdminEmailTo2 + userDAL.GetUserEmailById(Convert.ToInt32(item));
                                }
                            }

                            if (reservationData.EventId == 0)
                                RsvpAdminEmailTo2 = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_private_booking_notifications_receiver);

                            if (RsvpAdminEmailTo2.Length == 0)
                                RsvpAdminEmailTo2 = ew.EmailTo;

                            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.RsvpCancel, model.RsvpId, ew.EmailFrom, RsvpAdminEmailTo2, RsvpGuestSubjectContent, RsvpGuestContent, reservationData.WineryID, null, ew.Id, Settings.GetStrValue(settingsGroup, SettingKey.member_rsvp_contact_email), DestinationName));

                        }
                    }

                }
                catch (Exception ex)
                {
                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpCancelEmail", "RsvpGuestCancel: " + ex.ToString);
                }
                //#### RSVP Guest Email - END ####
            }
            return response;
        }

        /// <summary>
        /// This method is used to send cprsvp reminder emailv2
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<EmailResponse> SendCpRsvpReminderEmailV2(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            if ((!string.IsNullOrEmpty(model.data.BCode) || !(model.data.UId > 0)) && model.data.RsvpId == 0)
            {
                response.message = InternalServerError;
                return response;
            }
            EventDAL eventDAL = new EventDAL(ConnectionString);
            EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
            //Get Data for Email
            //#### RSVP Guest Email Reminder - START ####
            try
            {
                var reservationData = await Task.Run(() => eventDAL.GetReservationEmailDataByReservationId(model.data.RsvpId, model.data.UId, model.data.BCode));
                if (reservationData != null)
                {
                    int ReservationId = model.data.RsvpId;
                    int WineryID = reservationData.WineryID;
                    string BookingCode = reservationData.BookingCode;
                    string bookingGUID = reservationData.BookingGUID;
                    DateTime LocalBookingDate = Times.ToTimeZoneTime(reservationData.BookingDate, (Times.TimeZone)reservationData.TimeZoneId);
                    string GuestName = reservationData.GuestName;
                    string GuestEmail = reservationData.GuestEmail;
                    string GuestPhone = reservationData.GuestPhone;
                    string GuestWPhone = reservationData.GuestWPhone;
                    string GuestAddress1 = reservationData.GuestAddress1;
                    string GuestAddress2 = reservationData.GuestAddress2;
                    string GuestCity = reservationData.GuestCity;
                    string GuestState = reservationData.GuestState;
                    string GuestZipCode = reservationData.GuestZipCode;
                    decimal Fee = reservationData.Fee;
                    int ChargeFee = reservationData.ChargeFee;
                    decimal FeePaid = reservationData.FeePaid;
                    short GuestCount = reservationData.GuestCount;
                    string MemberName = reservationData.MemberName;
                    string MemberPhone = reservationData.MemberPhone;
                    string MemberEmail = reservationData.MemberEmail;
                    string MemberAddress1 = reservationData.MemberAddress1;
                    string MemberAddress2 = reservationData.MemberAddress2;
                    string MemberCity = reservationData.MemberCity;
                    string MemberState = reservationData.MemberState;
                    string MemberZipCode = reservationData.MemberZipCode;
                    DateTime EventDate = reservationData.EventDate;
                    TimeSpan StartTime = reservationData.StartTime;
                    TimeSpan EndTime = reservationData.EndTime;
                    string EventName = reservationData.EventName;
                    string EventLocation = reservationData.EventLocation;
                    string Notes = reservationData.Notes;
                    int Status = reservationData.Status;
                    string InternalNote = reservationData.InternalNote;
                    int BookedById = reservationData.BookedById;
                    int CancelLeadTime = reservationData.CancelLeadTime;
                    int EmailContentID = reservationData.EmailContentID;
                    int EmailTemplateID = reservationData.EmailTemplateID;
                    string Host = reservationData.Host;
                    int AffiliateID = reservationData.AffiliateID;
                    string Content = reservationData.Content;
                    string DestinationName = reservationData.DestinationName;
                    string locAddress1 = reservationData.locAddress1;
                    string locAddress2 = reservationData.locAddress2;
                    string locCity = reservationData.locCity;
                    string locState = reservationData.locState;
                    string locZip = reservationData.locZip;
                    string timezoneName = reservationData.timezone_name;

                    if(!string.IsNullOrEmpty(timezoneName))
                    {
                        timezoneName = " (" + timezoneName + ")";
                    }
                    else
                    {
                        timezoneName = "";
                    }

                    string addOnItems = "";
                    string AddOnDetails = "";
                    StringBuilder AddOnItemDetails = new StringBuilder();
                    string addOnHeading = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 10px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\"><strong><span style=\"font-size: 14px; line-height: 14px;\">ADDITIONAL SELECTIONS</span></strong> </span> </p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string addOnItem = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"439\" style=\"background-color: #ffffff;width: 439px;padding: 0px 0px 0px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-73p33\" style=\"max-width: 320px;min-width: 439.98px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 0px 0px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">{{AddOnQty}} X {{AddOnItem}}</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"160\" style=\"background-color: #ffffff;width: 160px;padding: 0px 30px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-26p67\" style=\"max-width: 320px;min-width: 160.02px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 30px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">{{AddOnPrice}}</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

                    string map_and_directions_url = "";
                    string mapInageURl = "";
                    string MapURL = "";

                    string googleAPIKey = "AIzaSyDKGd8SyV0GhpzD-onvM0ItLpzq5GhCFfA";
                    if (_appSetting != null && _appSetting.Value != null && !string.IsNullOrEmpty(_appSetting.Value.GoogleAPIKey))
                    {
                        googleAPIKey = _appSetting.Value.GoogleAPIKey;
                    }

                    if (reservationData.LocationId > 0)
                    {
                        MapURL = await Utility.GetMapImageHtmlByLocation(reservationData.LocationId, googleAPIKey);

                        var location = eventDAL.GetLocationMapDataByID(reservationData.LocationId);

                        if (location != null && location.location_id > 0)
                        {
                            map_and_directions_url = location.map_and_directions_url;
                            mapInageURl = "https://cdncellarpass.blob.core.windows.net/photos/location_maps/" + reservationData.LocationId.ToString() + "_dot.jpg";
                        }
                    }

                    var listAddOns = eventDAL.GetReservationAddOnItems(reservationData.ReservationId);
                    if (listAddOns != null)
                    {
                        int idx = 0;
                        foreach (var addon in listAddOns)
                        {
                            string item1 = addOnItem;
                            item1 = item1.Replace("{{AddOnQty}}", addon.Qty.ToString());
                            item1 = item1.Replace("{{AddOnItem}}", addon.Name);
                            item1 = item1.Replace("{{AddOnPrice}}", addon.Price.ToString("C", new CultureInfo("en-US")));
                            AddOnItemDetails.Append(item1);

                            idx += 1;
                            string numberedList = "";
                            if (addon.ItemType == (int)Common.AddOnGroupType.menu)
                            {
                                numberedList = string.Format("{0}. ", idx);
                            }
                            addOnItems += string.Format("{0}{1} ({2}) {3}<br />", numberedList, addon.Name, addon.Qty, (string.IsNullOrWhiteSpace(Convert.ToString(addon.Price)) ? "0.00" : addon.Price.ToString("N2")));
                        }
                    }

                    AddOnDetails = addOnHeading + AddOnItemDetails.ToString();

                    if (string.IsNullOrEmpty(addOnItems))
                        AddOnDetails = "";

                    string PaymentDetails = "";
                    string paymentItem = "<p style=\" font-size:14px; line-height: 100%;\"><span style=\"font-family:Poppins, sans-serif; font-size: 14px; line-height: 14px;\">{{PaymentDesc}}</span></p>";

                    if (reservationData.Fee == 0)
                    {
                        string PaymentStatus = paymentItem;
                        PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", "Complimentary");

                        PaymentDetails = PaymentDetails + PaymentStatus;
                    }
                    else
                    {
                        foreach (var item in eventDAL.GetPaymentStatusV2byReservationId(reservationData.ReservationId))
                        {
                            string PaymentStatus = paymentItem;
                            PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", item);

                            PaymentDetails = PaymentDetails + PaymentStatus;
                        }
                    }

                    //'LOCATION / ZOOM 'NOTE: We do not show the zoom in the preview unless you swich this manually here. 
                    bool sendPreviewWithZoom = false;

                    if (reservationData.EventId > 0)
                    {
                        EventModel eventModelValidate = eventDAL.GetEventById(reservationData.EventId);

                        if (eventModelValidate != null && eventModelValidate.EventID > 0)
                        {
                            if (eventModelValidate.EventTypeId == 34 && eventModelValidate.MeetingBehavior == 2)
                            {
                                sendPreviewWithZoom = true;
                            }
                        }
                    }

                    string GratuityContent = "";
                    if (reservationData.GratuityAmount > 0)
                    {
                        string GratuityHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"419\" style=\"background-color: #ffffff;width: 419px;padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-69p84\" style=\"max-width: 320px;min-width: 419.04px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\">Gratuity:      </span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"180\" style=\"background-color: #ffffff;width: 180px;padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-30p16\" style=\"max-width: 320px;min-width: 180.96px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\">[[GratuityAmt]]</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

                        GratuityHtml = GratuityHtml.Replace("[[GratuityAmt]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.GratuityAmount));

                        GratuityContent = GratuityHtml;

                    }

                    //LOCATION ADDRESS
                    string lAddress1 = reservationData.MemberAddress1;
                    string lAddress2 = reservationData.MemberAddress2;
                    string lCity = reservationData.MemberCity;
                    string lState = reservationData.MemberState;
                    string lZip = reservationData.MemberZipCode;
                    string calendarAddress = "";
                    //Use Location based address instead of winery address if provided for location
                    if ((reservationData.locAddress1 != null))
                    {
                        if (reservationData.locAddress1.Trim().Length > 0)
                        {
                            lAddress1 = (reservationData.locAddress1 == null ? "" : reservationData.locAddress1);
                            lAddress2 = (reservationData.locAddress2 == null ? "" : reservationData.locAddress2);
                            lCity = (reservationData.locCity == null ? "" : reservationData.locCity);
                            lState = (reservationData.locState == null ? "" : reservationData.locState);
                            lZip = (reservationData.locZip == null ? "" : reservationData.locZip);
                        }
                    }

                    //Calendar address
                    calendarAddress += lAddress1 + "\\n ";
                    if (lAddress2.Trim().Length > 0)
                    {
                        calendarAddress += lAddress2 + "\\n ";
                    }
                    calendarAddress += lCity + ", " + lState + " " + lZip + "\\n ";

                    string lFullAddress = string.Empty;
                    lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);

                    //Replace here so that we remove it if it's blank
                    string DirectionsURL = "";
                    if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
                    {
                        DirectionsURL = reservationData.MapAndDirectionsURL;
                    }
                    string locationSection = "";
                    string locationHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #ffffff;width: 300px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>[[DestinationName]]</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[LocationAddress]]</span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[MemberPhone]]</span></p> <p style=\"font-size: 14px; line-height: 140%;\"> </p> </div> </td> </tr> </tbody> </table> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[DirectionsURL]]\" style=\"height:48px; v-text-anchor:middle; width:238px;\" arcsize=\"8.5%\" strokecolor=\"#236fa1\" strokeweight=\"1px\" fillcolor=\"#ffffff\"><w:anchorlock/><center style=\"color:#236fa1;font-family:'Poppins', sans-serif;;\"><![endif]--> <a href=\"[[DirectionsURL]]\" target=\"_blank\" class=\"v-button\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #236fa1; background-color: #ffffff; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:100%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;border-top-color: #236fa1; border-top-style: solid; border-top-width: 1px; border-left-color: #236fa1; border-left-style: solid; border-left-width: 1px; border-right-color: #236fa1; border-right-style: solid; border-right-width: 1px; border-bottom-color: #236fa1; border-bottom-style: solid; border-bottom-width: 1px;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"font-size: 14px; line-height: 16.8px; font-family: Poppins, sans-serif;\">GET DIRECTIONS</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #ffffff;width: 300px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;bordear-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div> [[MapURL]] </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string zoomHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Zoom Meeting Information</strong></span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">We are using Zoom to host our virtual tastings which will require you to take some additional steps to ensure you can connect to our virtual tasting without any delays. This will require you to enter the Zoom MeetingID and Zoom passport assigned to you.</span></p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[ZoomMeeting]] </span></p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>[[MemberName]] Technical Support</strong></span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">Should you require any assistance with connecting to our virtual tasting, please contact us immediately at [[MemberPhone]]</span></p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Zoom Technical Support</strong></span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">If you have questions or need additional assistance with using Zoom, please contact their <a rel=\"noopener\" href=\"https://support.zoom.us/hc/en-us\" target=\"_blank\">Technical Support</a>.</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string mapHtml = "<table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"> <tr> <td style=\"padding-right: 0px;padding-left: 0px;\" align=\"center\"> <a href=\"[[map_and_directions_url]]\" target=\"_blank\"> <img align=\"center\" border=\"0\" src=\"[[mapInageURl]]\" alt=\"Map Image\" title=\"Map Image\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 100%;max-width: 280px;\" width=\"280\" /> </a> </td> </tr> </table> </td> </tr> </tbody> </table>";

                    //'Replace tags in location html
                    locationHtml = locationHtml.Replace("[[DestinationName]]", DestinationName);
                    locationHtml = locationHtml.Replace("[[LocationAddress]]", lFullAddress);
                    locationHtml = locationHtml.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                    locationHtml = locationHtml.Replace("[[DirectionsURL]]", DirectionsURL);

                    mapHtml = mapHtml.Replace("[[map_and_directions_url]]", map_and_directions_url);
                    mapHtml = mapHtml.Replace("[[mapInageURl]]", mapInageURl);

                    locationHtml = locationHtml.Replace("[[MapURL]]", mapHtml);

                    //'by default it's set to location
                    locationSection = locationHtml;

                    string zoomContent = string.Empty;

                    ZoomMeetingInfo zoomMeetingInfo = eventDAL.GetZoomMeetingInfo(reservationData.slotid, reservationData.slottype, reservationData.EventDate, reservationData.ReservationId);

                    if (zoomMeetingInfo != null && zoomMeetingInfo.Id > 0)
                    {
                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)Common.Common.SettingGroup.member);


                        bool requirePassword = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_zoom_require_password);
                        string zoomMeetingPassword = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_zoom_password);

                        if (requirePassword && !string.IsNullOrEmpty(zoomMeetingPassword))
                        {
                            zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}<br>Password: {3}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()), zoomMeetingPassword);
                        }
                        else
                        {
                            zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()));
                        }
                    }

                    //'if set to show zoom we use it instead
                    if (sendPreviewWithZoom)
                    {
                        //'Replace tags in Zoom html
                        zoomHtml = zoomHtml.Replace("[[ZoomMeeting]]", zoomContent);
                        zoomHtml = zoomHtml.Replace("[[MemberName]]", DestinationName);
                        zoomHtml = zoomHtml.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));

                        locationSection = zoomHtml;
                    }

                    string inviteSection = "";
                    bool hasInvite = reservationData.HasInvite;

                    if (hasInvite)
                    {
                        string inviteHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 20px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"line-height: 140%;\"><strong>IMPORTANT!</strong></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\"><strong>Your reservation is currently pending and requires additional action to be confirmed.</strong><br /></span></p> <p style=\"line-height: 140%;\"> </p> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\">You must take immediate action by [[ExpirationDateTime]] in order to confirm your appointment or your reservation will be automatically cancelled and released to others.</span></p> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\"><br />Click on the 'Complete Reservation' button to complete your reservation.</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[CompeteRsvpLink]]\" style=\"height:47px; v-text-anchor:middle; width:540px;\" arcsize=\"8.5%\" stroke=\"f\" fillcolor=\"#1069b0\"><w:anchorlock/><center style=\"color:#FFFFFF;font-family:'Poppins', sans-serif;;\"><![endif]--> <a href=\"[[CompleteRSVPLink]]\" target=\"_blank\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #FFFFFF; background-color: #1069b0; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:100%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;font-size: 14px;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"line-height: 16.8px;\">COMPLETE RESERVATION</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                        DateTime expirationdateTime = reservationData.ReservationInviteExpirationDateTime;

                        inviteHtml = inviteHtml.Replace("[[ExpirationDateTime]]", String.Format("{0} {1}", expirationdateTime.ToShortDateString(), expirationdateTime.ToString("hh:mm tt")));
                        inviteHtml = inviteHtml.Replace("[[CompleteRSVPLink]]", "https://www.cellarpass.com");
                        inviteSection = inviteHtml;
                    }

                    string notesSection = "";

                    //Preview sample messages
                    string personalMSg = model.perMsg;
                    string guestNote = reservationData.Notes;

                    //NOTES

                    string notesSectionHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\">[[Notes]]</p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string notesCombined = "";
                    string personlMsgHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Personal Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[Personal_Message]]</span><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"></span></p>";
                    string guestNoteHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Guest Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[RsvpNote]]</span></p>";

                    //If either personal Message or guest note is available we need to render this section
                    bool showNoteSection = false;

                    if (!string.IsNullOrEmpty(personalMSg))
                    {
                        showNoteSection = true;
                        //Replace message tags
                        personlMsgHtml = personlMsgHtml.Replace("[[Personal_Message]]", personalMSg);
                        //Add to notes
                        notesCombined += personlMsgHtml;
                    }

                    if (!string.IsNullOrEmpty(guestNote))
                    {
                        showNoteSection = true;

                        //Replace message tags
                        guestNoteHtml = guestNoteHtml.Replace("[[RsvpNote]]", guestNote);

                        //If personl msg is not empty add this space html first before adding the guest note to create some separation
                        if (notesCombined.Length > 0)
                        {
                            notesCombined += "<p style=\"font-size: 14px; line-height: 120%;\"> </p>";
                        }

                        //Add to notes
                        notesCombined += guestNoteHtml;
                    }

                    //If either personal or guest note was provided this should be true and we combine it all.
                    if (showNoteSection)
                    {
                        //replace tag in notes section html with notes combined
                        notesSectionHtml = notesSectionHtml.Replace("[[Notes]]", notesCombined);
                        //Set noteSection to Html
                        notesSection = notesSectionHtml;
                    }

                    EmailTemplates MailTemplate = EmailTemplates.RsvpReminder;

                    string PhoneNumber = "";


                    string RsvpGuestContent = "";
                    string GuestsAttending = "";
                    DateTime StartDate = EventDate.Add(StartTime);
                    DateTime EndDate = EventDate.Add(EndTime);
                    TimeSpan Duration = EndDate.Subtract(StartDate);
                    string RsvpGuestSubjectContent = "";

                    //Get Affiliate Information
                    string AffiliateEmail = "";
                    string AffiliateName = "";
                    string AffiliateCompany = "";

                    int AffID = AffiliateID;

                    if (AffID > 0)
                    {
                        try
                        {
                            var affiliate = eventDAL.GetUser(reservationData.AffiliateID);
                            if (affiliate != null)
                            {
                                AffiliateEmail = affiliate.AffiliateEmail;
                                AffiliateName = affiliate.AffiliateName;
                                AffiliateCompany = affiliate.AffiliateCompany;
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                    }

                    //Format Fee

                    string FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US"));
                    string DepositPolicy = GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;

                    //Phone Number - check and use home first then work if phone is empty
                    if ((GuestPhone != null))
                    {
                        if (!string.IsNullOrEmpty(GuestPhone))
                        {
                            PhoneNumber = GuestPhone;
                        }
                        else
                        {
                            if ((GuestWPhone != null))
                            {
                                if (!string.IsNullOrEmpty(GuestWPhone))
                                {
                                    PhoneNumber = GuestWPhone;
                                }
                            }
                        }
                    }

                    EmailContent ew = default(EmailContent);
                    //get business message
                    //EmailContent ew1 = emailDAL.GetEmailContent((int)MailTemplate, WineryID);
                    ew = emailDAL.GetEmailContent((int)MailTemplate, 0);
                    int isfaq = emailDAL.CheckFAQExistsForWinery(WineryID);

                    if ((ew != null))
                    {

                        if (ew.Active == true)
                        {
                            //Need to get the guests attending each reservation in the booking
                            if (ReservationId > 0)
                            {
                                //Get Guests Detail
                                GuestsAttending = eventDAL.GetGuestAttending(ReservationId, reservationData.GuestName);

                            }

                            //Configure/Format CancellationPolicy
                            string CancellationPolicy = "";
                            string CancelByDate = reservationData.cancel_message;

                            if (!string.IsNullOrEmpty(CancelByDate))
                                CancelByDate = "Cancel by " + CancelByDate;

                            if (Content != null)
                            {
                                if (Content.Trim().Length > 0)
                                {
                                    CancellationPolicy = Content;

                                    if (reservationData.CancelLeadTime > 50000)
                                    {
                                        CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                    }
                                    else
                                    {
                                        CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                                    }
                                }
                            }

                            //CancellationPolicy = CancellationPolicy + "<br><br>" + reservationData.cancel_message;
                            //Use Location based address instead of winery address if provided for location
                            if ((locAddress1 != null))
                            {
                                if (locAddress1.Trim().Length > 0)
                                {
                                    lAddress1 = locAddress1;
                                    lAddress2 = locAddress2;
                                    lCity = locCity;
                                    lState = locState;
                                    lZip = locZip;
                                }
                            }


                            //Replace Content Tags
                            RsvpGuestSubjectContent = ew.EmailSubject;
                            RsvpGuestContent = ew.EmailBody;

                            //Subject
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingCode]]", BookingCode);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DestinationName]]", DestinationName);

                            RsvpGuestSubjectContent = ReservationTagReplace(RsvpGuestSubjectContent, bookingGUID);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestName]]", GuestName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestEmail]]", GuestEmail);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestPhone]]", PhoneNumber);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestCount]]", Convert.ToString(GuestCount));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestsAttending]]", GuestsAttending);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FormatFee);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Gratuity]]", GratuityContent);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[PaymentStatus]]", PaymentDetails);

                            string bannerImageHtml = Utility.GetBannerImageHtmlByWineryId(reservationData.WineryID);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberName]]", MemberName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(MemberPhone.ToString(), "US"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberEmail]]", MemberEmail);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress1]]", lAddress1);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress2]]", lAddress2);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressCity]]", lCity);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressState]]", lState);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressZipCode]]", lZip);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[LocationAddress]]", lFullAddress);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDuration]]", Duration.ToString());
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpEvent]]", EventName);
                            //RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpHost]]", Host)
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDestination]]", DestinationName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpLocation]]", EventLocation);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[NotesSection]]", notesSection);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InviteSection]]", inviteSection);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(Notes) ? "None" : Notes));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpStatus]]", GetReservationStatus(Status));

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancellationPolicy]]", CancellationPolicy);

                            string DirectionsURL2 = "";
                            if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
                            {
                                //DirectionsURL = "<a href=\"" + reservationData.MapAndDirectionsURL.Trim() + "\" target=\"_blank\">Get Directions</a>";
                                DirectionsURL2 = Utility.GenerateEmailButton("Get Directions", reservationData.MapAndDirectionsURL.Trim(), "#47bf12", "#47bf12", "15px", "15px", "#ffffff");
                            }

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DirectionsURL]]", DirectionsURL2);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[ZoomMeeting]]", zoomContent);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));

                            //Replace double ,, or , , with , (usually because of address)
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace(",,", ",");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace(", ,", ",");

                            //Body
                            RsvpGuestContent = RsvpGuestContent.Replace("[[BookingCode]]", BookingCode);

                            RsvpGuestContent = ReservationTagReplace(RsvpGuestContent, bookingGUID);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[DestinationName]]", reservationData.DestinationName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestName]]", GuestName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", GuestEmail);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestPhone]]", PhoneNumber);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestCount]]", Convert.ToString(GuestCount));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestsAttending]]", GuestsAttending);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FormatFee);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Gratuity]]", GratuityContent);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[PaymentStatus]]", PaymentDetails);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", MemberName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(MemberPhone.ToString(), "US"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", MemberEmail);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress1]]", lAddress1);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress2]]", lAddress2);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressCity]]", lCity);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressState]]", lState);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressZipCode]]", lZip);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[LocationAddress]]", lFullAddress);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDuration]]", Duration.ToString());
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpEvent]]", EventName);
                            //RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpHost]]", Host)
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDestination]]", DestinationName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpLocation]]", EventLocation);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[NotesSection]]", notesSection);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[InviteSection]]", inviteSection);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(Notes) ? "None" : Notes));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpStatus]]", GetReservationStatus(Status));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateName]]", AffiliateName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateCompany]]", AffiliateCompany);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationPolicy]]", CancellationPolicy);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancelByDate]]", CancelByDate);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[ZoomMeeting]]", zoomContent);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));
                            string strpath = "https://typhoon.cellarpass.com/";
                            if (ConnectionString.IndexOf("live") > -1)
                                strpath = "https://www.cellarpass.com/";

                            string profileUrl = string.Format("{1}business/{0}", reservationData.ProfileUrl, strpath);

                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[MemberLinkButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[MemberLinkButton]]", profileUrl);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberLinkButton]]", profileUrl);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[DirectionsURL]]", DirectionsURL);

                            if (isfaq == 1)
                            {
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");

                                RsvpGuestContent = RsvpGuestContent.Replace("[[FAQButton]]", profileUrl + "?f=1");
                            }
                            else
                            {
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl);
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl);

                                RsvpGuestContent = RsvpGuestContent.Replace("[[FAQButton]]", profileUrl);
                            }

                            RsvpGuestContent = RsvpGuestContent.Replace(",,", ",");
                            RsvpGuestContent = RsvpGuestContent.Replace(", ,", ",");

                            SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);

                            List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)SettingGroup.member);
                            string member_rsvp_contact_email = Settings.GetStrValue(settingsGroup, SettingKey.member_rsvp_contact_email);

                            string businessMessage = Settings.GetStrValue(settingsGroup, SettingKey.member_rsvp_review_business_message);
                            //if (ew1 != null)
                            //{
                            //    businessMessage = ew1.BusinessMessage;
                            //}

                            if (string.IsNullOrEmpty(businessMessage))
                            {
                                businessMessage = "Thank you for booking one of our experiences. We would be more than happy to host you again and look forward to your next visit.";
                            }

                            RsvpGuestContent = RsvpGuestContent.Replace("[[BusinessMessage]]", businessMessage);
                            //RsvpGuestContent = RsvpGuestContent.Replace("[[OrganizerMessage]]", OrganizerMsg);
                            string viewItineraryHtml = "";

                            if (!string.IsNullOrWhiteSpace(reservationData.ItineraryGUID))
                            {
                                viewItineraryHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[ViewItineraryURL]]\" style=\"height:47px; v-text-anchor:middle; width:516px;\" arcsize=\"8.5%\" strokecolor=\"#e03e2d\" strokeweight=\"1px\" fillcolor=\"#ffffff\"><w:anchorlock/><center style=\"color:#e03e2d;font-family:'Poppins', sans-serif;\"><![endif]--> <a href=\"[[ViewItineraryURL]]\" target=\"_blank\" class=\"v-button\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #e03e2d; background-color: #ffffff; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:96%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;border-top-width: 1px; border-top-style: solid; border-top-color: #e03e2d; border-left-width: 1px; border-left-style: solid; border-left-color: #e03e2d; border-right-width: 1px; border-right-style: solid; border-right-color: #e03e2d; border-bottom-width: 1px; border-bottom-style: solid; border-bottom-color: #e03e2d;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"font-size: 14px; line-height: 16.8px; font-family: Poppins, sans-serif;\">VIEW ITINERARY</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                                viewItineraryHtml = viewItineraryHtml.Replace("[[ViewItineraryURL]]", string.Format("{1}itinerary/{0}?v=agenda", reservationData.ItineraryGUID, strpath));
                            }

                            RsvpGuestContent = RsvpGuestContent.Replace("[[ViewItinerary]]", viewItineraryHtml);

                            //string AdditionalTicketInfo = "";
                            //StringBuilder TicketLevelMessages = new StringBuilder();
                            //string additionalInfoHeading = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 25px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 25px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\"><strong>Additional Ticket Information</strong></span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                            //string ticketlevelMsg = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\"><strong>{{TicketLevelName}}</strong></span></p> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketLevelMessage}}</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

                            //foreach (var item in ticketDAL.GetAdditionalTicketInfoByID(order.id))
                            //{
                            //    string example1 = ticketlevelMsg;
                            //    example1 = example1.Replace("{{TicketLevelName}}", item.TicketName);
                            //    example1 = example1.Replace("{{TicketLevelMessage}}", item.ConfirmationMessage);
                            //    TicketLevelMessages.Append(example1);
                            //}

                            //AdditionalTicketInfo = additionalInfoHeading + TicketLevelMessages.ToString();

                            //RsvpGuestContent = RsvpGuestContent.Replace("[[AdditionalTicketInformation]]", AdditionalTicketInfo);

                            

                            RsvpGuestContent = RsvpGuestContent.Replace("[[Add-Ons]]", addOnItems);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AddOnItems]]", AddOnDetails);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[DepositPolicy]]", DepositPolicy);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Personal_Message]]", "None");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[LocationSection]]", locationSection);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[ConfirmationMessage]]", reservationData.EventConfirmationMessage);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationMessage]]", reservationData.EventCancellationMessage);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MapURl]]", MapURL);
                            //Directions URL
                            //string DirectionsURL = "";
                            //Send Mail

                            if (!string.IsNullOrEmpty(model.CCGuestEmail))
                                GuestEmail = model.CCGuestEmail;

                            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.RsvpReminder, model.RsvpId, ew.EmailFrom, GuestEmail, RsvpGuestSubjectContent, RsvpGuestContent, reservationData.WineryID, null, ew.Id, member_rsvp_contact_email, DestinationName));

                            //if (response.emailSent)
                            //{
                            //    eventDAL.UpdateReminderSentForReservation(reservationData.ReservationId);
                            //}
                            if (string.IsNullOrEmpty(model.CCGuestEmail))
                                eventDAL.UpdateReminderSentForReservation(reservationData.ReservationId);
                            //Send with new mail system
                            //Email.SendEmail(Email.EmailType.RsvpReminder, Email.EmailProvider.Mailgun, ReservationId, ew.EmailFrom, GuestEmail, RsvpGuestSubjectContent, RsvpGuestContent, 0, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "TaskRsvpReminder", "SendCpRsvpReminderEmail: " + ex.ToString);
            }
            //#### RSVP Guest Email Reminder - END ####
            return response;
        }

        /// <summary>
        /// This method is used to send no show email
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<EmailResponse> SendCpRsvpNoShowEmailV2(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            if (model.data.RsvpId == 0)
            {
                response.message = InternalServerError;
                return response;
            }

            //Get Data for Email
            EventDAL eventDAL = new EventDAL(ConnectionString);
            EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
            //Get Data for Email
            var reservationData = await Task.Run(() => eventDAL.GetReservationEmailDataByReservationId(model.data.RsvpId, 0, ""));

            string GratuityContent = "";
            if (reservationData.GratuityAmount > 0)
            {
                string GratuityHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"419\" style=\"background-color: #ffffff;width: 419px;padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-69p84\" style=\"max-width: 320px;min-width: 419.04px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\">Gratuity:      </span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"180\" style=\"background-color: #ffffff;width: 180px;padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-30p16\" style=\"max-width: 320px;min-width: 180.96px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\">[[GratuityAmt]]</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

                GratuityHtml = GratuityHtml.Replace("[[GratuityAmt]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.GratuityAmount));

                GratuityContent = GratuityHtml;

            }
            int WineryID = reservationData.WineryID;
            int ReservationId = model.data.RsvpId;
            string BookingCode = reservationData.BookingCode;
            string bookingGUID = reservationData.BookingGUID;
            DateTime LocalBookingDate = Times.ToTimeZoneTime(reservationData.BookingDate, (Times.TimeZone)reservationData.TimeZoneId);
            string GuestName = reservationData.GuestName;
            string GuestEmail = reservationData.GuestEmail;
            string GuestPhone = reservationData.GuestPhone;
            string GuestWPhone = reservationData.GuestWPhone;
            string GuestAddress1 = reservationData.GuestAddress1;
            string GuestAddress2 = reservationData.GuestAddress2;
            string GuestCity = reservationData.GuestCity;
            string GuestState = reservationData.GuestState;
            string GuestZipCode = reservationData.GuestZipCode;
            decimal Fee = reservationData.Fee;
            int ChargeFee = reservationData.ChargeFee;
            decimal FeePaid = reservationData.FeePaid;
            short GuestCount = reservationData.GuestCount;
            string MemberName = reservationData.MemberName;
            string MemberPhone = reservationData.MemberPhone;
            string MemberEmail = reservationData.MemberEmail;
            string MemberAddress1 = reservationData.MemberAddress1;
            string MemberAddress2 = reservationData.MemberAddress2;
            string MemberCity = reservationData.MemberCity;
            string MemberState = reservationData.MemberState;
            string MemberZipCode = reservationData.MemberZipCode;
            DateTime EventDate = reservationData.EventDate;
            TimeSpan StartTime = reservationData.StartTime;
            TimeSpan EndTime = reservationData.EndTime;
            string EventName = reservationData.EventName;
            string EventLocation = reservationData.EventLocation;
            string Notes = reservationData.Notes;
            int Status = reservationData.Status;
            string InternalNote = reservationData.InternalNote;
            int BookedById = reservationData.BookedById;
            int CancelLeadTime = reservationData.CancelLeadTime;
            int EmailContentID = reservationData.EmailContentID;
            int EmailTemplateID = reservationData.EmailTemplateID;
            string Host = reservationData.Host;
            int AffiliateID = reservationData.AffiliateID;
            string Content = reservationData.Content;
            string DestinationName = reservationData.DestinationName;
            string locAddress1 = reservationData.locAddress1;
            string locAddress2 = reservationData.locAddress2;
            string locCity = reservationData.locCity;
            string locState = reservationData.locState;
            string locZip = reservationData.locZip;
            DateTime StartDate = EventDate.Add(StartTime);
            DateTime EndDate = EventDate.Add(EndTime);
            TimeSpan Duration = EndDate.Subtract(StartDate);

            string timezoneName = reservationData.timezone_name;

            if (!string.IsNullOrEmpty(timezoneName))
            {
                timezoneName = " (" + timezoneName + ")";
            }
            else
            {
                timezoneName = "";
            }

            string inviteSection = "";
            bool hasInvite = reservationData.HasInvite;

            if (hasInvite)
            {
                string inviteHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 20px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"line-height: 140%;\"><strong>IMPORTANT!</strong></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\"><strong>Your reservation is currently pending and requires additional action to be confirmed.</strong><br /></span></p> <p style=\"line-height: 140%;\"> </p> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\">You must take immediate action by [[ExpirationDateTime]] in order to confirm your appointment or your reservation will be automatically cancelled and released to others.</span></p> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\"><br />Click on the 'Complete Reservation' button to complete your reservation.</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[CompeteRsvpLink]]\" style=\"height:47px; v-text-anchor:middle; width:540px;\" arcsize=\"8.5%\" stroke=\"f\" fillcolor=\"#1069b0\"><w:anchorlock/><center style=\"color:#FFFFFF;font-family:'Poppins', sans-serif;;\"><![endif]--> <a href=\"[[CompleteRSVPLink]]\" target=\"_blank\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #FFFFFF; background-color: #1069b0; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:100%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;font-size: 14px;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"line-height: 16.8px;\">COMPLETE RESERVATION</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                DateTime expirationdateTime = reservationData.ReservationInviteExpirationDateTime;

                inviteHtml = inviteHtml.Replace("[[ExpirationDateTime]]", String.Format("{0} {1}", expirationdateTime.ToShortDateString(), expirationdateTime.ToString("hh:mm tt")));
                inviteHtml = inviteHtml.Replace("[[CompleteRSVPLink]]", "https://www.cellarpass.com");
                inviteSection = inviteHtml;
            }

            string notesSection = "";

            //Preview sample messages
            string personalMSg = model.perMsg;
            string guestNote = reservationData.Notes;

            //NOTES

            string notesSectionHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\">[[Notes]]</p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
            string notesCombined = "";
            string personlMsgHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Personal Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[Personal_Message]]</span><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"></span></p>";
            string guestNoteHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Guest Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[RsvpNote]]</span></p>";

            //If either personal Message or guest note is available we need to render this section
            bool showNoteSection = false;

            if (!string.IsNullOrEmpty(personalMSg))
            {
                showNoteSection = true;
                //Replace message tags
                personlMsgHtml = personlMsgHtml.Replace("[[Personal_Message]]", personalMSg);
                //Add to notes
                notesCombined += personlMsgHtml;
            }

            if (!string.IsNullOrEmpty(guestNote))
            {
                showNoteSection = true;

                //Replace message tags
                guestNoteHtml = guestNoteHtml.Replace("[[RsvpNote]]", guestNote);

                //If personl msg is not empty add this space html first before adding the guest note to create some separation
                if (notesCombined.Length > 0)
                {
                    notesCombined += "<p style=\"font-size: 14px; line-height: 120%;\"> </p>";
                }

                //Add to notes
                notesCombined += guestNoteHtml;
            }

            //If either personal or guest note was provided this should be true and we combine it all.
            if (showNoteSection)
            {
                //replace tag in notes section html with notes combined
                notesSectionHtml = notesSectionHtml.Replace("[[Notes]]", notesCombined);
                //Set noteSection to Html
                notesSection = notesSectionHtml;
            }

            string zoomContent = string.Empty;

            ZoomMeetingInfo zoomMeetingInfo = eventDAL.GetZoomMeetingInfo(reservationData.slotid, reservationData.slottype, reservationData.EventDate, reservationData.ReservationId);

            if (zoomMeetingInfo != null && zoomMeetingInfo.Id > 0)
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)Common.Common.SettingGroup.member);


                bool requirePassword = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_zoom_require_password);
                string zoomMeetingPassword = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_zoom_password);

                if (requirePassword && !string.IsNullOrEmpty(zoomMeetingPassword))
                {
                    zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}<br>Password: {3}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()), zoomMeetingPassword);
                }
                else
                {
                    zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()));
                }
            }

            string PaymentDetails = "";
            string paymentItem = "<p style=\" font-size:14px; line-height: 100%;\"><span style=\"font-family:Poppins, sans-serif; font-size: 14px; line-height: 14px;\">{{PaymentDesc}}</span></p>";

            if (reservationData.Fee == 0)
            {
                string PaymentStatus = paymentItem;
                PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", "Complimentary");

                PaymentDetails = PaymentDetails + PaymentStatus;
            }
            else
            {
                foreach (var item in eventDAL.GetPaymentStatusV2byReservationId(reservationData.ReservationId))
                {
                    string PaymentStatus = paymentItem;
                    PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", item);

                    PaymentDetails = PaymentDetails + PaymentStatus;
                }
            }


            if (model.data.ActionSource == (int)ActionSource.Consumer)
            {
                //#### CP RSVP Email - START ####
                try
                {
                    //Get Email Template Content for CP RSVP Confirmation
                    EmailTemplates MailTemplate = EmailTemplates.NoShowNotification;

                    EmailContent ec = default(EmailContent);
                    ec = emailDAL.GetEmailContent((int)MailTemplate, 0);

                    if (ec == null || ec.Active == false)
                    {
                        //break; // TODO: might not be correct. Was : Exit Try
                    }

                    string PhoneNumber = "";

                    //Parse Content
                    ArrayList BccEmailList = new ArrayList();
                    string StartToken = "{Repeat}";
                    string EndToken = "{/Repeat}";
                    string EmailContent = ec.EmailBody;
                    string BeforeRepeat = "";
                    string AfterRepeat = "";
                    string RepeatSection = "";
                    string RepeatReplaced = "";
                    string RepeatCompleted = "";
                    int RepeatStartInt = 0;
                    int RepeatEndInt = 0;
                    int LoopCount = 0;
                    string EmailSubject = ec.EmailSubject;

                    //See if there is a repeating section
                    RepeatStartInt = EmailContent.IndexOf(StartToken);
                    RepeatEndInt = EmailContent.IndexOf(EndToken);

                    if (RepeatStartInt > 0 && RepeatEndInt > 0)
                    {
                        BeforeRepeat = Common.Common.Left(EmailContent, RepeatStartInt - 1);
                        AfterRepeat = Common.Common.Right(EmailContent, EmailContent.Length - ((RepeatEndInt + EndToken.Length) - 1));
                        RepeatSection = Common.StringHelpers.ParseBetweenTags(StartToken, EndToken, EmailContent);
                    }
                    else
                    {
                        BeforeRepeat = EmailContent;
                        RepeatSection = "";
                        AfterRepeat = "";
                    }



                    //Format Fee
                    string FormatFee = Fee.ToString();
                    if (Fee > 0)
                    {
                        FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")) + ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                    }
                    else
                    {
                        FormatFee = "Complimentary";
                    }

                    string GuestsAttending = "";

                    //Phone Number - check and use home first then work if phone is empty
                    if (!string.IsNullOrEmpty(GuestPhone))
                    {
                        PhoneNumber = GuestPhone;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(GuestWPhone))
                        {
                            PhoneNumber = Utility.FormatTelephoneNumber(GuestWPhone.ToString(), "US");
                        }
                    }

                    //Configure/Format CancellationPolicy
                    string CancellationPolicy = "";
                    string CancelByDate = "";
                    if ((Content != null))
                    {
                        if (Content.Trim().Length > 0)
                        {
                            CancellationPolicy = Content;

                            if (reservationData.CancelLeadTime > 50000)
                            {
                                CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                            }
                            else
                            {
                                CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(RepeatSection))
                    {

                        RepeatReplaced = RepeatSection;
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_Fee]]", Fee.ToString());
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_GuestCount]]", GuestCount.ToString());
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_MemberName]]", MemberName);
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_MemberPhone]]", Utility.FormatTelephoneNumber(MemberPhone.ToString(), "US"));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_MemberEmail]]", MemberEmail);
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpDuration]]", Duration.ToString());
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpEvent]]", EventName);
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpDestination]]", DestinationName);
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpLocation]]", EventLocation);
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_GuestsAttending]]", GuestsAttending);
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpNote]]", (Notes == null ? "" : Notes));
                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_RsvpStatus]]", Status.ToString());

                        RepeatReplaced = RepeatReplaced.Replace("[[rpt_CancellationPolicy]]", CancellationPolicy);

                        RepeatCompleted += RepeatReplaced;
                    }

                    //This is the same for each returned item so we only set it on the first loop.
                    if (LoopCount == 0)
                    {
                        if (EmailSubject.Trim().Length > 0)
                        {
                            EmailSubject = EmailSubject.Replace("[[BookingCode]]", BookingCode);
                            EmailSubject = EmailSubject.Replace("[[DestinationName]]", DestinationName);

                            EmailSubject = ReservationTagReplace(EmailSubject, bookingGUID);

                            EmailSubject = EmailSubject.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            EmailSubject = EmailSubject.Replace("[[GuestName]]", GuestName);
                            EmailSubject = EmailSubject.Replace("[[GuestEmail]]", GuestEmail);
                            EmailSubject = EmailSubject.Replace("[[GuestPhone]]", PhoneNumber);
                        }

                        //GuestEmail = GuestEmail;
                        if (!string.IsNullOrEmpty(BeforeRepeat))
                        {
                            BeforeRepeat = BeforeRepeat.Replace("[[BookingCode]]", BookingCode);

                            BeforeRepeat = ReservationTagReplace(BeforeRepeat, bookingGUID);

                            BeforeRepeat = BeforeRepeat.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            BeforeRepeat = BeforeRepeat.Replace("[[GuestName]]", GuestName);
                            BeforeRepeat = BeforeRepeat.Replace("[[GuestEmail]]", GuestEmail);
                            BeforeRepeat = BeforeRepeat.Replace("[[GuestPhone]]", PhoneNumber);

                            BeforeRepeat = BeforeRepeat.Replace("[[MemberName]]", MemberName);
                            BeforeRepeat = BeforeRepeat.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(MemberPhone.ToString(), "US"));
                            BeforeRepeat = BeforeRepeat.Replace("[[MemberEmail]]", MemberEmail);
                            BeforeRepeat = BeforeRepeat.Replace("[[MemberAddress1]]", locAddress1);
                            BeforeRepeat = BeforeRepeat.Replace("[[MemberAddress2]]", locAddress2);
                            BeforeRepeat = BeforeRepeat.Replace("[[MemberAddressCity]]", locCity);
                            BeforeRepeat = BeforeRepeat.Replace("[[MemberAddressState]]", locState);
                            BeforeRepeat = BeforeRepeat.Replace("[[MemberAddressZipCode]]", locZip);
                            BeforeRepeat = BeforeRepeat.Replace("[[DestinationName]]", reservationData.DestinationName);
                            string lFullAddress = string.Empty;
                            lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", locAddress1, string.IsNullOrEmpty(locAddress2) ? "" : locAddress2 + "<br>", locCity, locState, locZip);

                            BeforeRepeat = BeforeRepeat.Replace("[[LocationAddress]]", lFullAddress);
                            BeforeRepeat = BeforeRepeat.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                            BeforeRepeat = BeforeRepeat.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}", StartDate.Add(StartTime)));
                            BeforeRepeat = BeforeRepeat.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                            BeforeRepeat = BeforeRepeat.Replace("[[RsvpDuration]]", EndDate.Subtract(StartDate).ToString());
                            BeforeRepeat = BeforeRepeat.Replace("[[RsvpEvent]]", EventName);
                            BeforeRepeat = BeforeRepeat.Replace("[[rpt_RsvpEvent]]", EventName);
                            BeforeRepeat = BeforeRepeat.Replace("[[RsvpDestination]]", DestinationName);
                            BeforeRepeat = BeforeRepeat.Replace("[[RsvpLocation]]", EventLocation);

                            BeforeRepeat = BeforeRepeat.Replace("[[NotesSection]]", notesSection);
                            BeforeRepeat = BeforeRepeat.Replace("[[InviteSection]]", inviteSection);

                            BeforeRepeat = BeforeRepeat.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(Notes) ? "None" : Notes));
                            BeforeRepeat = BeforeRepeat.Replace("[[RsvpStatus]]", Status.ToString());

                            BeforeRepeat = BeforeRepeat.Replace("[[CancellationPolicy]]", CancellationPolicy);

                            BeforeRepeat = BeforeRepeat.Replace("[[GuestsAttending]]", GuestsAttending);
                            BeforeRepeat = BeforeRepeat.Replace("[[Fee]]", FormatFee);
                            BeforeRepeat = BeforeRepeat.Replace("[[GuestCount]]", GuestCount.ToString());
                        }

                        if (!string.IsNullOrEmpty(AfterRepeat))
                        {
                            AfterRepeat = AfterRepeat.Replace("[[BookingCode]]", BookingCode);

                            AfterRepeat = ReservationTagReplace(AfterRepeat, bookingGUID);

                            AfterRepeat = AfterRepeat.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            AfterRepeat = AfterRepeat.Replace("[[GuestName]]", GuestName);
                            AfterRepeat = AfterRepeat.Replace("[[GuestEmail]]", GuestEmail);
                            AfterRepeat = AfterRepeat.Replace("[[GuestPhone]]", PhoneNumber);
                            AfterRepeat = AfterRepeat.Replace("[[DestinationName]]", reservationData.DestinationName);
                        }
                    }
                    LoopCount += 1;

                    //Remove Trailing <br>
                    if (RepeatCompleted.Length > 5)
                    {
                        RepeatCompleted = Common.Common.Left(RepeatCompleted, RepeatCompleted.Length - 6);
                    }

                    string FinalContent = BeforeRepeat + RepeatCompleted + AfterRepeat;

                    string emailTo = "";

                    //Email To?
                    if (!string.IsNullOrEmpty(model.SendTo))
                    {
                        emailTo = model.SendTo;
                    }
                    else
                    {
                        emailTo = GuestEmail;
                    }

                    //Additional emails to send
                    if ((ec.EmailTo != null))
                    {
                        if (ec.EmailTo.Trim().Length > 0 && ec.EmailTo.IndexOf("no-reply") == -1 && ec.EmailTo.IndexOf("noreply") == -1)
                        {
                            emailTo = emailTo + "," + ec.EmailTo.Trim();
                        }
                    }

                    response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.NoShowNotice, ReservationId, ec.EmailFrom, emailTo, EmailSubject, FinalContent, WineryID, null, ec.Id));

                }
                catch (Exception ex)
                {
                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpCancelEmail", "RsvpSysCancel: " + ex.ToString());
                }
                //#### CP RSVP Email - END ####
            }

            if (model.data.ActionSource == (int)ActionSource.BackOffice)
            {
                //#### RSVP Guest Email - START ####
                try
                {
                    EmailTemplates MailTemplate = EmailTemplates.NoShowNotification;

                    string PhoneNumber = "";


                    if (GuestEmail.IndexOf("@noemail") == -1 || !string.IsNullOrEmpty(model.SendTo))
                    {

                        string RsvpGuestContent = "";
                        string GuestsAttending = "";
                        ArrayList BccEmailList = new ArrayList();

                        string RsvpGuestSubjectContent = "";

                        //Format Fee
                        string FormatFee = Fee.ToString();
                        if (Fee > 0)
                        {
                            FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")) + ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                        }
                        else
                        {
                            FormatFee = "Complimentary";
                        }

                        //Phone Number - check and use home first then work if phone is empty
                        if (!string.IsNullOrEmpty(GuestPhone))
                        {
                            PhoneNumber = GuestPhone;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(GuestWPhone))
                            {
                                PhoneNumber = Utility.FormatTelephoneNumber(GuestWPhone.ToString(), "US");
                            }
                        }

                        EmailContent ew = default(EmailContent);
                        ew = emailDAL.GetEmailContent((int)MailTemplate, 0);


                        if ((ew != null) && ew.Active == true)
                        {
                            //Configure/Format CancellationPolicy
                            string CancellationPolicy = "";
                            string CancelByDate = "";
                            if ((Content != null))
                            {
                                if (Content.Trim().Length > 0)
                                {
                                    CancellationPolicy = Content;

                                    if (reservationData.CancelLeadTime > 50000)
                                    {
                                        CancellationPolicy = CancellationPolicy.Replace("[[CancellationPolicy]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                    }
                                    else
                                    {
                                        CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                                    }
                                }
                            }

                            //LOCATION ADDRESS
                            string lAddress1 = MemberAddress1;
                            string lAddress2 = MemberAddress2;
                            string lCity = MemberCity;
                            string lState = MemberState;
                            string lZip = MemberZipCode;

                            //Use Location based address instead of winery address if provided for location
                            if ((locAddress1 != null))
                            {
                                if (locAddress1.Trim().Length > 0)
                                {
                                    lAddress1 = locAddress1;
                                    lAddress2 = locAddress2;
                                    lCity = locCity;
                                    lState = locState;
                                    lZip = locZip;
                                }
                            }

                            //Replace Content Tags

                            //Subject
                            RsvpGuestSubjectContent = ew.EmailSubject;
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingCode]]", BookingCode);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DestinationName]]", DestinationName);

                            RsvpGuestSubjectContent = ReservationTagReplace(RsvpGuestSubjectContent, bookingGUID);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestName]]", GuestName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestEmail]]", GuestEmail);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestPhone]]", PhoneNumber);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestCount]]", GuestCount.ToString());
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestsAttending]]", GuestsAttending);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FormatFee);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Gratuity]]", GratuityContent);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[PaymentStatus]]", PaymentDetails);

                            string bannerImageHtml = Utility.GetBannerImageHtmlByWineryId(reservationData.WineryID);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberName]]", MemberName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(MemberPhone.ToString(), "US"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberEmail]]", MemberEmail);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress1]]", lAddress1);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress2]]", lAddress2);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressCity]]", lCity);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressState]]", lState);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressZipCode]]", lZip);

                            string lFullAddress = string.Empty;
                            lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[LocationAddress]]", lFullAddress);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDuration]]", Duration.ToString());
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpEvent]]", EventName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[rpt_RsvpEvent]]", EventName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDestination]]", DestinationName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpLocation]]", EventLocation);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[NotesSection]]", notesSection);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InviteSection]]", inviteSection);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(Notes) ? "None" : Notes));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpStatus]]", Status.ToString());
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancellationPolicy]]", "");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[ZoomMeeting]]", zoomContent);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));
                            //body
                            RsvpGuestContent = ew.EmailBody;
                            RsvpGuestContent = RsvpGuestContent.Replace("[[BookingCode]]", BookingCode);

                            RsvpGuestContent = ReservationTagReplace(RsvpGuestContent, bookingGUID);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[DestinationName]]", reservationData.DestinationName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestName]]", GuestName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", GuestEmail);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestPhone]]", PhoneNumber);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestCount]]", GuestCount.ToString());
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestsAttending]]", GuestsAttending);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FormatFee);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Gratuity]]", GratuityContent);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[PaymentStatus]]", PaymentDetails);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", MemberName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(MemberPhone.ToString(), "US"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", MemberEmail);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress1]]", lAddress1);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress2]]", lAddress2);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressCity]]", lCity);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressState]]", lState);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressZipCode]]", lZip);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[LocationAddress]]", lFullAddress);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpTime]]", string.Format("{0:t}", StartDate));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDuration]]", Duration.ToString());
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpEvent]]", EventName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[rpt_RsvpEvent]]", EventName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDestination]]", DestinationName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpLocation]]", EventLocation);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[NotesSection]]", notesSection);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[InviteSection]]", inviteSection);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(Notes) ? "None" : Notes));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpStatus]]", Status.ToString());

                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationPolicy]]", CancellationPolicy);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));

                            string emailTo = "";

                            //Email To?
                            if (!string.IsNullOrEmpty(model.SendTo))
                            {
                                emailTo = model.SendTo;
                            }
                            else
                            {
                                emailTo = GuestEmail;
                            }

                            //Additional emails to send
                            if ((ew.EmailTo != null))
                            {
                                if (ew.EmailTo.Trim().Length > 0 && ew.EmailTo.IndexOf("no-reply") == -1 && ew.EmailTo.IndexOf("noreply") == -1)
                                {
                                    emailTo = emailTo + "," + ew.EmailTo.Trim();
                                }
                            }

                            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.NoShowNotice, ReservationId, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent, WineryID, null, ew.Id));

                        }
                    }
                }
                catch (Exception ex)
                {
                    //Log.InsertLog(Log.LogType.AppError, "SendCpRsvpNoShowEmail", "RsvpGuestNoShow: " + ex.ToString);
                }
                //#### RSVP Guest Email - END ####
            }
            return response;

        }

        public async Task<EmailResponse> ProcessAbandonedCartTicketsEmail(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                string BodyContent = "";
                string SubjectContent = "";
                EventDAL eventDAL = new EventDAL(ConnectionString);
                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew;
                ew = emailDAL.GetEmailContent((int)EmailTemplates.AbandonedCartTickets, 0);
                if (ew != null && ew.Active == true)
                {
                    int RsvpId = model.RsvpId;

                    if (RsvpId == 0 && model.data != null)
                    {
                        RsvpId = model.data.RsvpId;
                    }

                    var acart = await Task.Run(() => eventDAL.GetTicketAbandonedCart(RsvpId));
                    if (acart == null)
                    {
                        response.message = InternalServerError;
                        return response;
                    }

                    string event_url = Utility.GetFriendlyURL(acart.EventTitle, acart.Event_Id, acart.EventURL);

                    string memberLink = acart.PurchaseURL;
                    //string memberLink = Utility.GetWineryFriendlyURL(acart.member_id);
                    SubjectContent = ew.EmailSubject;
                    BodyContent = ew.EmailBody;
                    SubjectContent = SubjectContent.Replace("[[EventName]]", acart.EventTitle);
                    SubjectContent = SubjectContent.Replace("[[EventNameLink]]", "");
                    SubjectContent = SubjectContent.Replace("[[EventDate]]", acart.StartDateTime.ToString("dddd, MMMM dd, yyyy"));
                    SubjectContent = SubjectContent.Replace("[[EventStarts]]", string.Format("{0:hh:mm tt}", acart.StartDateTime));
                    SubjectContent = SubjectContent.Replace("[[EventEnds]]", string.Format("{0:hh:mm tt}", acart.EndDateTime));
                    SubjectContent = SubjectContent.Replace("[[EventOrganizer]]", acart.EventOrganizerName);
                    SubjectContent = SubjectContent.Replace("[[EventOrganizerLink]]", "");
                    SubjectContent = SubjectContent.Replace("[[VenueName]]", acart.VenueName);
                    SubjectContent = SubjectContent.Replace("[[VenueAddress]]", acart.VenueAddress);
                    SubjectContent = SubjectContent.Replace("[[GuestFirstName]]", acart.FirstName);
                    SubjectContent = SubjectContent.Replace("[[GuestLastName]]", acart.LastName);
                    SubjectContent = SubjectContent.Replace("[[GuestEmail]]", acart.Email);
                    SubjectContent = SubjectContent.Replace("[[EventURL]]", "");
                    SubjectContent = SubjectContent.Replace("[[AccessCode]]", acart.AcccessCode);
                    SubjectContent = SubjectContent.Replace("[[PromoCode]]", acart.Promo);
                    SubjectContent = SubjectContent.Replace("[[Button]]", "");
                    SubjectContent = SubjectContent.Replace("[[MemberName]]", acart.MemberName);
                    SubjectContent = SubjectContent.Replace("[[MemberNameLink]]", "");
                    SubjectContent = SubjectContent.Replace("[[MemberRegionLink]]", "");

                    BodyContent = BodyContent.Replace("[[MemberNameLink]]", string.Format("<a href=\"https://www.cellarpass.com/business/{0}\">{1}</a>", acart.PurchaseURL, acart.MemberName));
                    BodyContent = BodyContent.Replace("[[MemberRegionLink]]", string.Format("<a href=\"https://www.cellarpass.com/region/{0}\">{1}</a>", acart.RegionUrl, acart.RegionName));
                    BodyContent = BodyContent.Replace("[[EventName]]", acart.EventTitle);
                    BodyContent = BodyContent.Replace("[[EventNameLink]]", string.Format("<a href=\"https://www.cellarpass.com/events/{0}\">{1}</a>", event_url, acart.EventTitle));
                    BodyContent = BodyContent.Replace("[[EventDate]]", acart.StartDateTime.ToString("dddd, MMMM dd, yyyy"));
                    BodyContent = BodyContent.Replace("[[EventStarts]]", string.Format("{0:hh:mm tt}", acart.StartDateTime));
                    BodyContent = BodyContent.Replace("[[EventEnds]]", string.Format("{0:hh:mm tt}", acart.EndDateTime));
                    BodyContent = BodyContent.Replace("[[EventOrganizer]]", acart.EventOrganizerName);
                    BodyContent = BodyContent.Replace("[[EventOrganizerLink]]", string.Format("<a href=\"https://www.cellarpass.com/business/{0}\">{1}</a>", memberLink, acart.EventOrganizerName));
                    BodyContent = BodyContent.Replace("[[VenueName]]", acart.VenueName);
                    BodyContent = BodyContent.Replace("[[VenueAddress]]", acart.VenueAddress);
                    BodyContent = BodyContent.Replace("[[GuestFirstName]]", acart.FirstName);
                    BodyContent = BodyContent.Replace("[[GuestLastName]]", acart.LastName);
                    BodyContent = BodyContent.Replace("[[GuestEmail]]", acart.Email);
                    BodyContent = BodyContent.Replace("[[EventURL]]", String.Format("https://www.cellarpass.com/{0}", event_url));
                    BodyContent = BodyContent.Replace("[[AccessCode]]", acart.AcccessCode);
                    BodyContent = BodyContent.Replace("[[PromoCode]]", acart.Promo);
                    BodyContent = BodyContent.Replace("[[MemberName]]", acart.MemberName);
                    BodyContent = BodyContent.Replace("[[MemberLinkButton]]", acart.MemberName);

                    string promo = "";
                    string access = "";
                    if (!string.IsNullOrEmpty(acart.AcccessCode))
                    {
                        access = "?access=" + HttpUtility.UrlEncode(acart.AcccessCode);
                    }

                    if (!string.IsNullOrEmpty(acart.Promo))
                    {
                        promo = HttpUtility.UrlEncode(acart.Promo);
                        if (access.Contains("?"))
                        {
                            promo = "&promo=" + promo;
                        }
                        else
                        {
                            promo = "?promo=" + promo;
                        }
                    }

                    BodyContent = BodyContent.Replace("[[Button]]", Utility.GenerateEmailButton("Register", string.Format("https://www.cellarpass.com/{0}{1}{2}", event_url, access, promo), "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));
                    response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.AbanondedCartTickets, 0, ew.EmailFrom, acart.Email, SubjectContent, BodyContent, 0, null, ew.Id, "", acart.EventOrganizerName));
                    eventDAL.UpdateTicketAbandonedSent(model.RsvpId);
                }
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "AbandonedCartEmail", "ProcessAbandonedCartTicketsEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }

        public async Task<EmailResponse> ProcessAbandonedCartRsvpEmail(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                if (model.RsvpId == 0)
                {
                    response.message = InternalServerError;
                    return response;
                }

                string RsvpGuestContent = "";
                string RsvpSubjectContent = "";
                EventDAL eventDAL = new EventDAL(ConnectionString);
                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew;
                ew = emailDAL.GetEmailContent((int)EmailTemplates.AbandonedCartRsvp, 0);
                if (ew != null && ew.Active == true)
                {
                    int RsvpId = model.RsvpId;

                    if (RsvpId == 0 && model.data != null)
                    {
                        RsvpId = model.data.RsvpId;
                    }

                    var acart = await Task.Run(() => eventDAL.GetRSVPAbandonedCart(RsvpId));
                    if (acart == null)
                    {
                        response.message = InternalServerError;
                        return response;
                    }

                    var summary = await Task.Run(() => eventDAL.GetReservationSummary(acart.Slot_Id, acart.Slot_Type, acart.DateRequested));
                    if (summary == null)
                    {
                        response.message = InternalServerError;
                        return response;
                    }

                    RsvpSubjectContent = ew.EmailSubject;
                    RsvpGuestContent = ew.EmailBody;
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[GuestFirstName]]", acart.FirstName);
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[GuestLastName]]", acart.LastName);
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[GuestCount]]", acart.GuestCount.ToString());
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[GuestEmail]]", acart.Email);
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[MemberName]]", summary.MemberName);
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[MemberNameLink]]", "");
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[MemberRegion]]", summary.RegionName);
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[MemberRegionLink]]", "");
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[RsvpDate]]", summary.EventDate.ToString("dddd, MMMM dd, yyyy"));
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}", summary.EventDate));
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[RsvpDuration]]", EventDuration(summary.EventDate, summary.EventDateEnd));
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[RsvpEvent]]", summary.EventName);
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[RsvpLocation]]", summary.LocationName);

                    //if (summary.CancelPolicy > 50000)
                    //{
                    //    RsvpSubjectContent = RsvpSubjectContent.Replace("[[CancellationPolicy]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                    //}
                    //else
                    //{
                    //    RsvpSubjectContent = RsvpSubjectContent.Replace("[[CancellationPolicy]]", summary.CancelPolicy);
                    //}

                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[CancellationPolicy]]", summary.CancelPolicy);

                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[Button]]", "");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[GuestFirstName]]", acart.FirstName);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[GuestLastName]]", acart.LastName);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[GuestCount]]", acart.GuestCount.ToString());
                    RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", acart.Email);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", summary.MemberName);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[MemberNameLink]]", string.Format("<a href=\"https://www.cellarpass.com/business/{0}\">{1}</a>", summary.PurchaseURL, summary.MemberName));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[MemberRegion]]", summary.RegionName);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[MemberRegionLink]]", string.Format("<a href=\"https://www.cellarpass.com/region/{0}\">{1}</a>", summary.RegionUrl, summary.RegionName));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDate]]", summary.EventDate.ToString("dddd, MMMM dd, yyyy"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}", summary.EventDate));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDuration]]", EventDuration(summary.EventDate, summary.EventDateEnd));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpEvent]]", summary.EventName);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpLocation]]", summary.LocationName);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationPolicy]]", summary.CancelPolicy);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[Button]]", Utility.GenerateEmailButton("Reserve", string.Format("https://www.cellarpass.com/rsvp-checkout?sid={0}&stype={1}&wid={2}&guest={3}&date={4}", acart.Slot_Id, acart.Slot_Type, acart.Member_Id, acart.GuestCount, HttpUtility.UrlEncode(acart.DateRequested.ToString("dddd, MMMM dd, yyyy"))), "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));

                    response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.AbanondedCartRsvp, 0, ew.EmailFrom, acart.Email, RsvpSubjectContent, RsvpGuestContent, 0, null, ew.Id, "", summary.DestinationName));
                    eventDAL.UpdateReservationAbandonedSent(model.RsvpId);
                }
            }
            catch (Exception ex)
            {
                response.message = InternalServerError;
            }
            return response;
        }

        public async Task<EmailResponse> ProcessPrivateBookingRequest(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                if (model.RsvpId == 0)
                {
                    response.message = InternalServerError;
                    return response;
                }

                string RsvpGuestContent = "";
                string RsvpSubjectContent = "";
                string DisplayName = "";
                string ConfirmationMessage = "";
                string NotificationsReceiverEmail = "";
                EventDAL eventDAL = new EventDAL(ConnectionString);
                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew;
                ew = emailDAL.GetEmailContent((int)EmailTemplates.PrivateBookingRequest, 0);
                if (ew != null && ew.Active == true)
                {
                    int Id = model.RsvpId;

                    if (Id == 0 && model.data != null)
                    {
                        Id = model.data.RsvpId;
                    }

                    var acart = await Task.Run(() => eventDAL.GetPrivateEventRequestDetails(Id));
                    if (acart == null)
                    {
                        response.message = InternalServerError;
                        return response;
                    }

                    SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(acart.member_id, (int)Common.Common.SettingGroup.member).ToList();
                    if (settingsGroup != null && settingsGroup.Count > 0)
                    {
                        NotificationsReceiverEmail = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_private_booking_notifications_receiver);
                        ConfirmationMessage = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message);
                    }

                    DisplayName = acart.member_name;

                    RsvpSubjectContent = ew.EmailSubject;
                    RsvpGuestContent = ew.EmailBody;
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[PreferredDate]]", acart.preferred_date.ToString("dddd, MMMM dd, yyyy"));
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[PreferredTime]]", string.Format("{0:hh:mm tt}", Convert.ToDateTime("1/1/1900 " + acart.preferred_start_time.ToString())));
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[PreferredDuration]]", Utility.GetPreferredVisitDuration().Where(a => a.id == acart.preferred_visit_duration).FirstOrDefault().name);
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[PartySize]]", acart.guest.ToString());
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[GuestPhone]]", acart.phone_number);
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[GuestName]]", acart.first_name + " " + acart.last_name);
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[GuestEmail]]", acart.email);
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[ContactReason]]", Utility.GetReasonforVisit().Where(a => a.id == acart.reason_for_visit).FirstOrDefault().name);
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[ContactMessage]]", acart.details);
                    RsvpSubjectContent = RsvpSubjectContent.Replace("[[DestinationName]]", DisplayName);

                    RsvpGuestContent = RsvpGuestContent.Replace("[[PreferredDate]]", acart.preferred_date.ToString("dddd, MMMM dd, yyyy"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[PreferredTime]]", string.Format("{0:hh:mm tt}", Convert.ToDateTime("1/1/1900 " + acart.preferred_start_time.ToString())));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[PreferredDuration]]", Utility.GetPreferredVisitDuration().Where(a => a.id == acart.preferred_visit_duration).FirstOrDefault().name);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[PartySize]]", acart.guest.ToString());
                    RsvpGuestContent = RsvpGuestContent.Replace("[[GuestPhone]]", acart.phone_number);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[GuestName]]", acart.first_name + " " + acart.last_name);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", acart.email);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ContactReason]]", Utility.GetReasonforVisit().Where(a => a.id == acart.reason_for_visit).FirstOrDefault().name);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ContactMessage]]", acart.details);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ConfirmationMessage]]", ConfirmationMessage);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[DestinationName]]", DisplayName);

                    response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.PrivateEventRequest, 0, ew.EmailFrom, acart.email, RsvpSubjectContent, RsvpGuestContent, 0, null, ew.Id, NotificationsReceiverEmail, DisplayName));

                    if (settingsGroup != null && settingsGroup.Count > 0)
                    {
                        string private_booking_request_email = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_private_booking_request_email);
                        bool member_enable_private_booking_requests = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_enable_private_booking_requests);

                        if (member_enable_private_booking_requests)
                        {
                            if (!string.IsNullOrEmpty(private_booking_request_email))
                            {
                                await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.PrivateEventRequest, 0, ew.EmailFrom, private_booking_request_email, RsvpSubjectContent, RsvpGuestContent, 0, null, ew.Id, NotificationsReceiverEmail, DisplayName));
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(NotificationsReceiverEmail))
                    {
                        await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.PrivateEventRequest, 0, ew.EmailFrom, NotificationsReceiverEmail, RsvpSubjectContent, RsvpGuestContent, 0, null, ew.Id, NotificationsReceiverEmail, DisplayName));
                    }
                }
            }
            catch (Exception ex)
            {
                response.message = InternalServerError;
            }
            return response;
        }

        public async Task<EmailResponse> ProcessWaitlistNotificationEmail(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(ConnectionString);

                int RsvpId = model.RsvpId;

                if (RsvpId == 0 && model.data != null)
                {
                    RsvpId = model.data.RsvpId;
                }

                string WaitListGuId = eventDAL.GetWaitListGuIdById(RsvpId);

                if (WaitListGuId.Length == 0)
                {
                    response.message = InternalServerError;
                    return response;
                }

                string WaitlistOfferContent = "";
                string SubjectContent = "";

                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew;

                //get waitlist data for the RSVPId passed
                Waitlist waitlist = eventDAL.GetReservationV2WaitlistbyId(WaitListGuId);

                if (waitlist != null && waitlist.member_id > 0)
                {

                    ew = emailDAL.GetEmailContent((int)EmailTemplates.WaitListNotification, 0);
                    if (ew != null && ew.Active == true)
                    {

                        SubjectContent = ew.EmailSubject;
                        WaitlistOfferContent = ew.EmailBody;

                        // Replace Subject Tags
                        SubjectContent = SubjectContent.Replace("[[GuestName]]", waitlist.first_name + " " + waitlist.last_name);
                        SubjectContent = SubjectContent.Replace("[[GuestEmail]]", waitlist.email);
                        SubjectContent = SubjectContent.Replace("[[GuestCount]]", waitlist.guest_count.ToString());
                        SubjectContent = SubjectContent.Replace("[[WaitListStatus]]", waitlist.waitlist_status_desc);
                        SubjectContent = SubjectContent.Replace("[[DestinationName]]", waitlist.member_name);

                        string address = waitlist.address.address_1;
                        if (waitlist.address.address_2.Length > 0)
                            address = address + ", " + waitlist.address.address_2;

                        address = address + " " + waitlist.address.city;

                        if (waitlist.address.city.Length > 0)
                            address = address + ", ";

                        address = address + waitlist.address.state;
                        address = address + " " + waitlist.address.zip_code;

                        string work_phone = Utility.FormatTelephoneNumber(waitlist.work_phone, waitlist.address.country);



                        SubjectContent = SubjectContent.Replace("[[MemberAddress]]", address);
                        SubjectContent = SubjectContent.Replace("[[MemberPhone]]", work_phone);
                        SubjectContent = SubjectContent.Replace("[[EventName]]", waitlist.event_name);
                        SubjectContent = SubjectContent.Replace("[[EventStartDate]]", waitlist.event_date_time.ToShortDateString());
                        SubjectContent = SubjectContent.Replace("[[EventStartTime]]", waitlist.event_date_time.ToString("hh:mm tt"));
                        SubjectContent = SubjectContent.Replace("[[DestinationName]]", waitlist.member_name);
                        try
                        {
                            if (waitlist.waitlist_status == Waitlist_Status.approved)
                            {
                                DateTime invited_date_time = Convert.ToDateTime(waitlist.invited_date_time);

                                DateTime expDate = Times.ToTimeZoneTime(invited_date_time.AddMinutes(waitlist.valid_minutes), (Times.TimeZone)waitlist.location_timezone);
                                SubjectContent = SubjectContent.Replace("[[WaitListTimeout]]", expDate.ToShortDateString() + " " + expDate.ToString("hh:mm tt"));
                                WaitlistOfferContent = WaitlistOfferContent.Replace("[[WaitListTimeout]]", expDate.ToShortDateString() + " " + expDate.ToString("hh:mm tt"));
                            }
                            else
                            {
                                SubjectContent = SubjectContent.Replace("[[WaitListTimeout]]", "");
                                WaitlistOfferContent = WaitlistOfferContent.Replace("[[WaitListTimeout]]", "");
                            }
                        }
                        catch
                        {
                            SubjectContent = SubjectContent.Replace("[[WaitListTimeout]]", "");
                            WaitlistOfferContent = WaitlistOfferContent.Replace("[[WaitListTimeout]]", "");
                        }

                        SubjectContent = SubjectContent.Replace("[[ApproveButton]]", "");
                        SubjectContent = SubjectContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(waitlist.member_id));

                        // Body
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[GuestName]]", waitlist.first_name + " " + waitlist.last_name);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[GuestEmail]]", waitlist.email);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[GuestCount]]", waitlist.guest_count.ToString());
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[WaitListStatus]]", waitlist.waitlist_status_desc);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[DestinationName]]", waitlist.member_name);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[MemberAddress]]", address);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[MemberPhone]]", work_phone);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventName]]", waitlist.event_name);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventStartDate]]", waitlist.event_date_time.ToShortDateString());
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventStartTime]]", waitlist.event_date_time.ToString("hh:mm tt"));
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(waitlist.member_id));

                        //Conditional tags
                        if (waitlist.waitlist_status == Waitlist_Status.approved)
                        {
                            //Remove tags
                            WaitlistOfferContent = WaitlistOfferContent.Replace("[[Conditional_Approved]]", "");
                            WaitlistOfferContent = WaitlistOfferContent.Replace("[[/Conditional_Approved]]", "");

                            if (WaitlistOfferContent.Contains("[[Conditional_Pending]]") && WaitlistOfferContent.Contains("[[/Conditional_Pending]]"))
                            {
                                //    'Remove pending content and tags
                                int tag1 = WaitlistOfferContent.IndexOf("[[Conditional_Pending]]");
                                int tag2 = WaitlistOfferContent.IndexOf("[[/Conditional_Pending]]");
                                string beforeContent = WaitlistOfferContent.Substring(0, tag1);
                                string afterContent = WaitlistOfferContent.Substring(tag2 + "[[/Conditional_Pending]]".Length);
                                //Dim beforeContent As String = WaitlistOfferContent.Substring(0, tag1 - 1)
                                //Dim afterContent As String = WaitlistOfferContent.Substring(tag2 + "[[/Conditional_Pending]]".Length - 1)
                                //'Put it together
                                WaitlistOfferContent = beforeContent + afterContent;
                            }
                        }
                        else if (waitlist.waitlist_status == Waitlist_Status.pending)
                        {
                            //Remove tags
                            WaitlistOfferContent = WaitlistOfferContent.Replace("[[Conditional_Pending]]", "");
                            WaitlistOfferContent = WaitlistOfferContent.Replace("[[/Conditional_Pending]]", "");

                            if (WaitlistOfferContent.Contains("[[Conditional_Approved]]") && WaitlistOfferContent.Contains("[[/Conditional_Approved]]"))
                            {
                                //    'Remove approved content
                                int tag1 = WaitlistOfferContent.IndexOf("[[Conditional_Approved]]");
                                int tag2 = WaitlistOfferContent.IndexOf("[[/Conditional_Approved]]");
                                string beforeContent = WaitlistOfferContent.Substring(0, tag1);
                                string afterContent = WaitlistOfferContent.Substring(tag2 + "[[/Conditional_Approved]]".Length);
                                //Dim beforeContent As String = WaitlistOfferContent.Substring(0, tag1 - 1)
                                //Dim afterContent As String = WaitlistOfferContent.Substring(tag2 + "[[/Conditional_Approved]]".Length - 1)
                                //'Put it together
                                WaitlistOfferContent = beforeContent + afterContent;
                            }
                        }
                        else
                        {
                            if (WaitlistOfferContent.Contains("[[Conditional_Pending]]") && WaitlistOfferContent.Contains("[[/Conditional_Pending]]"))
                            {
                                //    'Remove pending content and tags
                                int tag1 = WaitlistOfferContent.IndexOf("[[Conditional_Pending]]");
                                int tag2 = WaitlistOfferContent.IndexOf("[[/Conditional_Pending]]");
                                string beforeContent = WaitlistOfferContent.Substring(0, tag1);
                                string afterContent = WaitlistOfferContent.Substring(tag2 + "[[/Conditional_Pending]]".Length);
                                //Dim beforeContent As String = WaitlistOfferContent.Substring(0, tag1 - 1)
                                //Dim afterContent As String = WaitlistOfferContent.Substring(tag2 + "[[/Conditional_Pending]]".Length - 1)
                                //'Put it together
                                WaitlistOfferContent = beforeContent + afterContent;
                            }

                            if (WaitlistOfferContent.Contains("[[Conditional_Approved]]") && WaitlistOfferContent.Contains("[[/Conditional_Approved]]"))
                            {
                                //    'Remove approved content
                                int tag1 = WaitlistOfferContent.IndexOf("[[Conditional_Approved]]");
                                int tag2 = WaitlistOfferContent.IndexOf("[[/Conditional_Approved]]");
                                string beforeContent = WaitlistOfferContent.Substring(0, tag1);
                                string afterContent = WaitlistOfferContent.Substring(tag2 + "[[/Conditional_Approved]]".Length);
                                //Dim beforeContent As String = WaitlistOfferContent.Substring(0, tag1 - 1)
                                //Dim afterContent As String = WaitlistOfferContent.Substring(tag2 + "[[/Conditional_Approved]]".Length - 1)
                                //'Put it together
                                WaitlistOfferContent = beforeContent + afterContent;
                            }
                        }

                        if (waitlist.waitlist_status == Waitlist_Status.approved)
                        {
                            WaitlistOfferContent = WaitlistOfferContent.Replace("[[ApproveButton]]", Utility.GenerateEmailButton("Confirm Reservation", string.Format("https://www.cellarpass.com/rsvp_member.aspx?sid={0}&type={1}&id={2}&Guests={3}&date={4}&wl={5}", waitlist.slot_id, waitlist.slot_type, waitlist.member_id, waitlist.guest_count, HttpUtility.UrlEncode(waitlist.event_date_time.ToString("yyyy-MM-dd") + "T00:00:00Z"), model.RsvpId), "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));
                        }
                        else
                        {
                            WaitlistOfferContent = WaitlistOfferContent.Replace("[[ApproveButton]]", "");
                        }

                        response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.WaitListNotification, 0, ew.EmailFrom, waitlist.email, SubjectContent, WaitlistOfferContent, 0, null, ew.Id));

                        if (!string.IsNullOrEmpty(waitlist.location_notification_email))
                        {
                            await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.WaitListNotification, 0, ew.EmailFrom, waitlist.location_notification_email, SubjectContent, WaitlistOfferContent, 0, null, ew.Id));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.message = InternalServerError;
            }
            return response;
        }

        public async Task<EmailResponse> ProcessWaitListCancellationEmail(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(ConnectionString);

                int RsvpId = model.RsvpId;

                if (RsvpId == 0 && model.data != null)
                {
                    RsvpId = model.data.RsvpId;
                }

                string WaitListGuId = eventDAL.GetWaitListGuIdById(RsvpId);

                if (WaitListGuId.Length == 0)
                {
                    response.message = InternalServerError;
                    return response;
                }

                string WaitlistOfferContent = "";
                string SubjectContent = "";

                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew;

                //get waitlist data for the RSVPId passed
                Waitlist waitlist = eventDAL.GetReservationV2WaitlistbyId(WaitListGuId);

                if (waitlist != null && waitlist.member_id > 0)
                {

                    ew = emailDAL.GetEmailContent((int)EmailTemplates.WaitListCancellation, 0);
                    if (ew != null && ew.Active == true)
                    {

                        SubjectContent = ew.EmailSubject;
                        WaitlistOfferContent = ew.EmailBody;

                        // Replace Subject Tags
                        SubjectContent = SubjectContent.Replace("[[GuestName]]", waitlist.first_name + " " + waitlist.last_name);
                        SubjectContent = SubjectContent.Replace("[[GuestEmail]]", waitlist.email);
                        SubjectContent = SubjectContent.Replace("[[GuestCount]]", waitlist.guest_count.ToString());
                        SubjectContent = SubjectContent.Replace("[[WaitListStatus]]", waitlist.waitlist_status_desc);
                        SubjectContent = SubjectContent.Replace("[[DestinationName]]", waitlist.member_name);

                        string address = waitlist.address.address_1;
                        if (waitlist.address.address_2.Length > 0)
                            address = address + ", " + waitlist.address.address_2;

                        address = address + " " + waitlist.address.city;

                        if (waitlist.address.city.Length > 0)
                            address = address + ", ";

                        address = address + waitlist.address.state;
                        address = address + " " + waitlist.address.zip_code;

                        string work_phone = Utility.FormatTelephoneNumber(waitlist.work_phone, waitlist.address.country);



                        SubjectContent = SubjectContent.Replace("[[MemberAddress]]", address);
                        SubjectContent = SubjectContent.Replace("[[MemberPhone]]", work_phone);
                        SubjectContent = SubjectContent.Replace("[[EventName]]", waitlist.event_name);
                        SubjectContent = SubjectContent.Replace("[[EventStartDate]]", waitlist.event_date_time.ToShortDateString());
                        SubjectContent = SubjectContent.Replace("[[EventStartTime]]", waitlist.event_date_time.ToString("hh:mm tt"));

                        try
                        {
                            DateTime invited_date_time = Convert.ToDateTime(waitlist.invited_date_time);

                            DateTime expDate = Times.ToTimeZoneTime(invited_date_time.AddMinutes(waitlist.valid_minutes), (Times.TimeZone)waitlist.location_timezone);
                            SubjectContent = SubjectContent.Replace("[[WaitListTimeout]]", expDate.ToShortDateString() + " " + expDate.ToString("hh:mm tt"));
                            WaitlistOfferContent = WaitlistOfferContent.Replace("[[WaitListTimeout]]", expDate.ToShortDateString() + " " + expDate.ToString("hh:mm tt"));
                        }
                        catch
                        {
                            SubjectContent = SubjectContent.Replace("[[WaitListTimeout]]", "");
                            WaitlistOfferContent = WaitlistOfferContent.Replace("[[WaitListTimeout]]", "");
                        }

                        SubjectContent = SubjectContent.Replace("[[ApproveButton]]", "");
                        SubjectContent = SubjectContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(waitlist.member_id));

                        // Body
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[GuestName]]", waitlist.first_name + " " + waitlist.last_name);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[GuestEmail]]", waitlist.email);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[GuestCount]]", waitlist.guest_count.ToString());
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[WaitListStatus]]", waitlist.waitlist_status_desc);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[DestinationName]]", waitlist.member_name);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[MemberAddress]]", address);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[MemberPhone]]", work_phone);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventName]]", waitlist.event_name);
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventStartDate]]", waitlist.event_date_time.ToShortDateString());
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventStartTime]]", waitlist.event_date_time.ToString("hh:mm tt"));

                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[ApproveButton]]", "");
                        WaitlistOfferContent = WaitlistOfferContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(waitlist.member_id));

                        response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.WaitListCancellation, 0, ew.EmailFrom, waitlist.email, SubjectContent, WaitlistOfferContent, 0, null, ew.Id));

                        if (!string.IsNullOrEmpty(waitlist.location_notification_email))
                        {
                            await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.WaitListCancellation, 0, ew.EmailFrom, waitlist.location_notification_email, SubjectContent, WaitlistOfferContent, 0, null, ew.Id));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.message = InternalServerError;
            }
            return response;
        }

        public string EventDuration(DateTime EventDate, DateTime EventDateEnd)
        {
            string duration = "NA";
            TimeSpan TS;

            if ((EventDateEnd > EventDate))
            {
                TS = EventDateEnd - EventDate;
            }
            else
            {
                TS = DateTime.Now.Date.AddDays(1).Add(new TimeSpan(EventDateEnd.TimeOfDay.Ticks)) - System.DateTime.Now.Date.Add(new TimeSpan(EventDate.TimeOfDay.Ticks));
            }

            double totalHours = TS.TotalHours;
            totalHours = Math.Round(totalHours * 4, MidpointRounding.ToEven) / 4;
            int minutes = TS.Minutes;
            decimal min = minutes;
            min = Math.Round(min * 4, MidpointRounding.ToEven) / 4;
            minutes = Convert.ToInt32(min);
            if (minutes > 0 && totalHours < 1)
            {
                duration = minutes + " Mins";
            }
            else
            {
                if (totalHours > 1)
                {
                    duration = totalHours + " Hours";
                }
                else
                {
                    duration = totalHours + " Hour";
                }
            }
            return duration;
        }
        public bool ProcessCheckInPromoEmail(PromoEmail emailPromo, int rsvpId)
        {
            EventDAL eventDAL = new EventDAL(ConnectionString);
            EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
            bool success = false;

            if ((emailPromo != null))
            {
                if (emailPromo.Promo.Id == 0)
                {
                    return false;
                }
            }

            try
            {
                //Get Email Template Content for CP RSVP Confirmation
                EmailTemplates MailTemplate = EmailTemplates.CheckInPromoOffer;

                EmailContent ec = default(EmailContent);
                ec = emailDAL.GetEmailContent((int)MailTemplate, 0);

                if (ec != null)
                {
                    if (ec.Active == false)
                    {
                        return false;
                    }

                    string emailSubject = ec.EmailSubject;
                    string emailBody = ec.EmailBody;

                    //Subject
                    emailSubject = emailSubject.Replace("[[MemberName]]", emailPromo.Promo.MemberName);
                    emailSubject = emailSubject.Replace("[[PromoName]]", emailPromo.Promo.PromoName);
                    emailSubject = emailSubject.Replace("[[PromoSchema]]", emailPromo.Promo.PromoValueDesc(emailPromo.Promo.PromoValue.ToString()));
                    emailSubject = emailSubject.Replace("[[FinePrint]]", emailPromo.Promo.PromoFinePrint);
                    emailSubject = emailSubject.Replace("[[ReferralCode]]", emailPromo.Promo.ReferralCode);
                    emailSubject = emailSubject.Replace("[[RedeemInstructions]]", emailPromo.Promo.RedemptionInstructions);
                    emailSubject = emailSubject.Replace("[[Event]]", emailPromo.Promo.EventName);
                    emailSubject = emailSubject.Replace("[[PromoExpires]]", emailPromo.Promo.EndDate.ToString("MM/dd/yyyy hh:mm tt"));
                    emailSubject = emailSubject.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(emailPromo.Promo.MemberId));
                    emailSubject = emailSubject.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(emailPromo.Promo.MemberPhone.ToString(), "US"));

                    //Body
                    emailBody = emailBody.Replace("[[MemberName]]", emailPromo.Promo.MemberName);
                    emailBody = emailBody.Replace("[[PromoName]]", emailPromo.Promo.PromoName);
                    emailBody = emailBody.Replace("[[PromoSchema]]", emailPromo.Promo.PromoValueDesc(emailPromo.Promo.PromoValue.ToString()));
                    emailBody = emailBody.Replace("[[FinePrint]]", emailPromo.Promo.PromoFinePrint);
                    emailBody = emailBody.Replace("[[ReferralCode]]", emailPromo.Promo.ReferralCode);
                    emailBody = emailBody.Replace("[[RedeemInstructions]]", emailPromo.Promo.RedemptionInstructions);
                    emailBody = emailBody.Replace("[[Event]]", emailPromo.Promo.EventName);
                    emailBody = emailBody.Replace("[[PromoExpires]]", emailPromo.Promo.EndDate.ToString("MM/dd/yyyy hh:mm tt"));
                    emailBody = emailBody.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(emailPromo.Promo.MemberId));
                    emailBody = emailBody.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(emailPromo.Promo.MemberPhone.ToString(), "US"));

                    //Send Mail
                    SendEmailAndSaveEmailLog(emailPromo.MailConfig, Email.EmailType.CheckInPromoOffer, rsvpId, ec.EmailFrom, emailPromo.ToEmail, emailSubject, emailBody, emailPromo.Promo.MemberId, null, ec.Id);
                }
            }
            catch (Exception ex)
            {
            }
            return success;
        }

        private byte[] GeneratePDFBytes(string strTicketpath)
        {
            ChromePdfRenderer renderer = new ChromePdfRenderer();
            License.LicenseKey = _IronPDFLicenseKey;
            // Create a PDF from a URL or local file path

            renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

            renderer.RenderingOptions.FitToPaperMode = FitToPaperModes.Automatic;
            renderer.RenderingOptions.CreatePdfFormsFromHtml = true;

            // Supports margin customization!
            renderer.RenderingOptions.MarginTop = 40; //millimeters
            renderer.RenderingOptions.MarginLeft = 20; //millimeters
            renderer.RenderingOptions.MarginRight = 20; //millimeters
            renderer.RenderingOptions.MarginBottom = 40; //millimeters
            var newPDF = renderer.RenderUrlAsPdf(strTicketpath);

            byte[] pdfBuffer = newPDF.BinaryData;

            return pdfBuffer;
        }

        public byte[] GenerateOrderPDFTest()
        {
             string strTicketpath = "https://dev.cellarpass.com/TicketTemplate.aspx?Oid=89121ef7-5ef7-44cb-801d-64f3de3d0139&spo=1";


            byte[] pdfBuffer = GeneratePDFBytes(strTicketpath);
            return pdfBuffer;

        }
        public byte[] GenerateOrderPDF(string orderGUID, int ticketOrderTicketId = 0, bool SelfPrintOnly = true)
        {
            LogDAL logDAL = new LogDAL(ConnectionString);
            try
            {
                string strTicketpath = "https://dev.cellarpass.com/";
                if (ConnectionString.IndexOf("live") > -1)
                    strTicketpath = "https://admin.cellarpass.com/";

                strTicketpath = strTicketpath + "TicketTemplate.aspx?Oid=" + orderGUID;

                if (ticketOrderTicketId > 0)
                    strTicketpath = strTicketpath + "&tid=" + ticketOrderTicketId.ToString();

                if (SelfPrintOnly)
                    strTicketpath = strTicketpath + "&spo=1";


                byte[] pdfBuffer = GeneratePDFBytes(strTicketpath);
                return pdfBuffer;
            }
            catch (Exception e)
            {
                logDAL.InsertLog("WebApi", "GenerateOrderPDF:  " + e.Message.ToString(), "", 1, 0);
            }
            return null;
        }

        public string GenerateOrderPDFStr(string orderGUID, int ticketOrderTicketId = 0, bool SelfPrintOnly = true)
        {
            LogDAL logDAL = new LogDAL(ConnectionString);
            try
            {
                 string strTicketpath = "https://dev.cellarpass.com/";
                if (ConnectionString.IndexOf("live") > -1)
                    strTicketpath = "https://admin.cellarpass.com/";

                strTicketpath = strTicketpath + "TicketTemplate.aspx?Oid=" + orderGUID;

                if (ticketOrderTicketId > 0)
                    strTicketpath = strTicketpath + "&tid=" + ticketOrderTicketId.ToString();

                if (SelfPrintOnly)
                    strTicketpath = strTicketpath + "&spo=1";

                byte[] pdfBuffer = GeneratePDFBytes(strTicketpath);

                string base64 = Convert.ToBase64String(pdfBuffer);

                return base64;
            }
            catch (Exception e)
            {
                logDAL.InsertLog("WebApi", "GenerateOrderPDFStr:  " + e.Message.ToString(), "", 1, 0);
            }
            return null;
        }

        public string GenerateInvoicePDFStr(int InvoiceId, int WineryId, DateTime Invoicedate)
        {
            LogDAL logDAL = new LogDAL(ConnectionString);
            try
            {
                string strTicketpath = "https://dev.cellarpass.com/";
                if (ConnectionString.IndexOf("live") > -1)
                    strTicketpath = "https://admin.cellarpass.com/";

                strTicketpath = strTicketpath + "Admin/InvoiceTemplate.aspx?id=" + InvoiceId.ToString() + "&wid=" + WineryId.ToString() + "&date=" + Invoicedate.ToString();


                byte[] pdfBuffer = GeneratePDFBytes(strTicketpath);

                string base64 = Convert.ToBase64String(pdfBuffer);

                return base64;
            }
            catch (Exception e)
            {
                logDAL.InsertLog("WebApi", "GenerateInvoicePDFStr:  " + e.Message.ToString(), "", 1, WineryId);
            }
            return null;
        }

        public string GenerateGutPrintBadgePDFStr(string TicketOrderTicketId, int EventID, int BadgeLayoutId, int WineryId)
        {
            LogDAL logDAL = new LogDAL(ConnectionString);
            try
            {
                ChromePdfRenderer renderer = new ChromePdfRenderer();
                License.LicenseKey = _IronPDFLicenseKey;

                renderer.RenderingOptions.SetCustomPaperSizeInInches(2.625, 3.75);
                renderer.RenderingOptions.ViewPortWidth = 252;
                renderer.RenderingOptions.MarginLeft = 0;
                renderer.RenderingOptions.MarginRight = 0;
                renderer.RenderingOptions.MarginTop = 0;
                renderer.RenderingOptions.MarginBottom = 0;

                if (TicketOrderTicketId.IndexOf(",") == -1)
                {
                    renderer.RenderingOptions.ViewPortHeight = 480;
                }

                 string strTicketpath = "https://dev.cellarpass.com/";
                if (ConnectionString.IndexOf("live") > -1)
                    strTicketpath = "https://admin.cellarpass.com/";

                strTicketpath = strTicketpath + "Admin/BadgeTemplate.aspx?totIds=" + TicketOrderTicketId + "&EventId=" + EventID.ToString() + "&BadgeLayoutId=" + BadgeLayoutId.ToString() + "&WineryId=" + WineryId.ToString();

                var newPDF = renderer.RenderUrlAsPdf(strTicketpath);
                byte[] pdfBuffer = newPDF.BinaryData;

                string base64 = Convert.ToBase64String(pdfBuffer);

                return base64;
            }
            catch (Exception e)
            {
                logDAL.InsertLog("WebApi", "GenerateGutPrintBadgePDFStr:  " + e.Message.ToString(), "", 1, WineryId);
            }
            return null;
        }

        public string GenerateAddressLabelBadgePDFStr(string TicketOrderTicketId, int EventID, int BadgeLayoutId, int WineryId)
        {
            LogDAL logDAL = new LogDAL(ConnectionString);
            try
            {
                ChromePdfRenderer renderer = new ChromePdfRenderer();
                License.LicenseKey = _IronPDFLicenseKey;

                renderer.RenderingOptions.SetCustomPaperSizeInInches(8.5, 11.04165354);
                renderer.RenderingOptions.ViewPortWidth = 612;
                renderer.RenderingOptions.MarginLeft = 0;
                renderer.RenderingOptions.MarginRight = 0;
                renderer.RenderingOptions.MarginTop = 12.7;
                renderer.RenderingOptions.MarginBottom = 12.7;

                string strTicketpath = "https://dev.cellarpass.com/";
                if (ConnectionString.IndexOf("live") > -1)
                    strTicketpath = "https://admin.cellarpass.com/";

                strTicketpath = strTicketpath + "Admin/BadgeTemplateAvery5160.aspx?totIds=" + TicketOrderTicketId + "&EventId=" + EventID.ToString() + "&BadgeLayoutId=" + BadgeLayoutId.ToString() + "&WineryId=" + WineryId.ToString();

                var newPDF = renderer.RenderUrlAsPdf(strTicketpath);
                byte[] pdfBuffer = newPDF.BinaryData;

                string base64 = Convert.ToBase64String(pdfBuffer);

                return base64;
            }
            catch (Exception e)
            {
                logDAL.InsertLog("WebApi", "GenerateAddressLabelBadgePDFStr:  " + e.Message.ToString(), "", 1, WineryId);
            }
            return null;
        }

        public string GenerateNameBadgePDFStr(string TicketOrderTicketId, int EventID, int BadgeLayoutId, int WineryId)
        {
            LogDAL logDAL = new LogDAL(ConnectionString);
            try
            {
                ChromePdfRenderer renderer = new ChromePdfRenderer();
                License.LicenseKey = _IronPDFLicenseKey;

               renderer.RenderingOptions.SetCustomPaperSizeInInches(8.5, 11.04165354);
               renderer.RenderingOptions.ViewPortWidth = 612;
               renderer.RenderingOptions.MarginLeft = 0;
               renderer.RenderingOptions.MarginRight = 0;
               renderer.RenderingOptions.MarginTop = 11.2889;
               renderer.RenderingOptions.MarginBottom = 7.05556;

               string strTicketpath = "https://dev.cellarpass.com/";
                if (ConnectionString.IndexOf("live") > -1)
                    strTicketpath = "https://admin.cellarpass.com/";

                strTicketpath = strTicketpath + "Admin/BadgeTemplateAvery5395.aspx?totIds=" + TicketOrderTicketId + "&EventId=" + EventID.ToString() + "&BadgeLayoutId=" + BadgeLayoutId.ToString() + "&WineryId=" + WineryId.ToString();

                var newPDF = renderer.RenderUrlAsPdf(strTicketpath);
                byte[] pdfBuffer = newPDF.BinaryData;

                string base64 = Convert.ToBase64String(pdfBuffer);

                return base64;
            }
            catch (Exception e)
            {
                logDAL.InsertLog("WebApi", "GenerateNameBadgePDFStr:  " + e.Message.ToString(), "", 1, WineryId);
            }
            return null;
        }

        public string GetDeliveryOptionFormatted(TicketDelivery deliveryOption, string fulfillment_lead_time_desc = "")
        {
            string delivery = string.Empty;

            if (deliveryOption == TicketDelivery.WillCall)
                delivery = "Will Call- {0}";
            else if (deliveryOption == TicketDelivery.SelfPrint)
                delivery = "SELF-PRINT - Print your tickets before the event";
            else if (deliveryOption == TicketDelivery.Shipped)
            {
                delivery = "Ships in 1-2 business days";

                if (fulfillment_lead_time_desc.Length > 0)
                    delivery = fulfillment_lead_time_desc;
            }

            return delivery;
        }


        public async Task<EmailResponse> ProcessSendCpRsvpTicketSaleEmail(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            LogDAL logDAL = new LogDAL(ConnectionString);

            //logDAL.InsertLog("WebApi", JsonConvert.SerializeObject(model), "", 1, -1);

            int member_id = 0;
            int OrderId = model.RsvpId;

            if (OrderId == 0 && model.data != null)
            {
                OrderId = model.data.RsvpId;
            }

            bool AdminEmail = false;

            if (model.AdminEmail || model.data.AdminEmail)
                AdminEmail = true;

            string AlternativeEmail = model.CCGuestEmail;

            try
            {
                TicketDAL ticketDAL = new TicketDAL(ConnectionString);
                EventDAL eventDAL = new EventDAL(ConnectionString);

                var order = ticketDAL.GetTicketOrderById(OrderId);

                member_id = order.Winery_Id;
                string RsvpGuestContent = "";
                string SubjectContent = "";

                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew;
                ew = emailDAL.GetEmailContent((int)EmailTemplates.RsvpTicketSalesConfirmation, -9999);
                // ew = emailDAL.GetEmailContentByID(order.EmailReceiptTemplate);
                if (ew != null && ew.Active == true)
                {
                    SubjectContent = ew.EmailSubject;
                    RsvpGuestContent = ew.EmailBody;

                    SubjectContent = SubjectContent.Replace("[[EventName]]", order.EventTitle);
                    SubjectContent = SubjectContent.Replace("[[EventOrganizer]]", order.wineryName);

                    //StringBuilder ticketTbl = new StringBuilder();

                    //ticketTbl.Append("<table>");
                    //ticketTbl.Append("<tr><th style=\"text-align:left;padding:0 5px 0 5px;\">Ticket Level</th><th style=\"text-align:right;padding:0 5px 0 5px;\">Qty</th><th style=\"text-align:right;padding:0 5px 0 5px;\">Delivery Type</th><th style=\"text-align:center;padding:0 5px 0 5px;\">Ticket Holder</th><th style=\"text-align:right;padding:0 5px 0 5px;\">Total</th></tr>");

                    string EventPolicy = "";
                    string EventPolicyHtml = "<div class=\"u-row-container row-payment\" style=\"padding: 0px;background-color: transparent\">                                                    <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\">                                                        <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">                                                            <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">   <tr>      <td style=\"padding: 0px;background-color: transparent;\" align=\"center\">         <table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\">            <tr style=\"background-color: transparent;\">               <![endif]-->                                                            <!--[if (mso)|(IE)]><td align=\"center\" width=\"49\" style=\"background-color: #ffffff;width: 49px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\">   <![endif]-->                                                            <div class=\"u-col u-col-8p33\" style=\"max-width: 320px;min-width: 49.98px;display: table-cell;vertical-align: top;\">                                                                <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                                                                    <!--[if (!mso)&(!IE)]><!-->                                                                    <div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                                                                        <!--<![endif]-->                                                                        <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                                                                            <tbody>                                                                                <tr>                                                                                    <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\">                                                                                        <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">                                                                                            <tbody>                                                                                                <tr style=\"vertical-align: top\">                                                                                                    <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">            <span>&nbsp;</span>        </td>                                                                                                </tr>                                                                                            </tbody>                                                                                        </table>                                                                                    </td>                                                                                </tr>                                                                            </tbody>                                                                        </table>                                                                        <!--[if (!mso)&(!IE)]><!-->                                                                    </div>                                                                    <!--<![endif]-->                                                                </div>                                                            </div>                                                            <!--[if (mso)|(IE)]></td><![endif]-->                                                            <!--[if (mso)|(IE)]><td align=\"center\" width=\"500\" style=\"background-color: #ecf0f1;width: 500px;padding: 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\">   <![endif]-->                                                            <div class=\"u-col u-col-83p34\" style=\"max-width: 320px;min-width: 500.04px;display: table-cell;vertical-align: top;\">                                                                <div style=\"background-color: #ecf0f1;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                                                                    <!--[if (!mso)&(!IE)]><!-->                                                                    <div style=\"box-sizing: border-box; height: 100%; padding: 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                                                                        <!--<![endif]-->                                                                        <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                                                                            <tbody>                                                                                <tr>                                                                                    <td style=\"overflow-wrap:break-word;word-break:break-word;padding:15px;font-family:'Poppins', sans-serif;;\" align=\"left\">                                                                                        <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\">                                                                                            <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Event Policy</strong></span></p>                                                                                            <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[EventPolicy]]</span></p>                                                                                        </div>                                                                                    </td>                                                                                </tr>                                                                            </tbody>                                                                        </table>                                                                        <!--[if (!mso)&(!IE)]><!-->                                                                    </div>                                                                    <!--<![endif]-->                                                                </div>                                                            </div>                                                            <!--[if (mso)|(IE)]></td><![endif]-->                                                            <!--[if (mso)|(IE)]><td align=\"center\" width=\"49\" style=\"background-color: #ffffff;width: 49px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\">   <![endif]-->                                                            <div class=\"u-col u-col-8p33\" style=\"max-width: 320px;min-width: 49.98px;display: table-cell;vertical-align: top;\">                                                                <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                                                                    <!--[if (!mso)&(!IE)]><!-->                                                                    <div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                                                                        <!--<![endif]-->                                                                        <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                                                                            <tbody>                                                                                <tr>                                                                                    <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\">                                                                                        <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">                                                                                            <tbody>                                                                                                <tr style=\"vertical-align: top\">                                                                                                    <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">            <span>&nbsp;</span>        </td>                                                                                                </tr>                                                                                            </tbody>                                                                                        </table>                                                                                    </td>                                                                                </tr>                                                                            </tbody>                                                                        </table>                                                                        <!--[if (!mso)&(!IE)]><!-->                                                                    </div>                                                                    <!--<![endif]-->                                                                </div>                                                            </div>                                                            <!--[if (mso)|(IE)]></td><![endif]-->                                                            <!--[if (mso)|(IE)]>            </tr>         </table>      </td>   </tr></table><![endif]-->                                                        </div>                                                    </div>                                                </div>";
                    string EventPolicycontent = order.ticket_event_policy;

                    if (!string.IsNullOrEmpty(EventPolicycontent))
                    {
                        EventPolicy = EventPolicyHtml;
                        EventPolicy = EventPolicy.Replace("[[EventPolicy]]", EventPolicycontent);
                    }

                    string OrganizerMsg = "";
                    string organizerMsgHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"><div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\">    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">        <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]-->        <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->        <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">            <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--<![endif]--><table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">    <tbody>        <tr>            <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"><table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"><tbody>    <tr style=\"vertical-align: top\">        <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">            <span>&nbsp;</span>        </td>    </tr></tbody></table></td>        </tr>    </tbody></table><!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->            </div>        </div>        <!--[if (mso)|(IE)]></td><![endif]-->        <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->    </div></div></div><div class=\"u-row-container row-payment\" style=\"padding: 0px;background-color: transparent\"><div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\">    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">        <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]-->        <!--[if (mso)|(IE)]><td align=\"center\" width=\"49\" style=\"background-color: #ffffff;width: 49px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->        <div class=\"u-col u-col-8p33\" style=\"max-width: 320px;min-width: 49.98px;display: table-cell;vertical-align: top;\">            <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--<![endif]--><table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">    <tbody>        <tr>            <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"><table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"><tbody>    <tr style=\"vertical-align: top\">        <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">            <span>&nbsp;</span>        </td>    </tr></tbody></table></td>        </tr>    </tbody></table><!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->            </div>        </div>        <!--[if (mso)|(IE)]></td><![endif]-->        <!--[if (mso)|(IE)]><td align=\"center\" width=\"500\" style=\"background-color: #ecf0f1;width: 500px;padding: 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->        <div class=\"u-col u-col-83p34\" style=\"max-width: 320px;min-width: 500.04px;display: table-cell;vertical-align: top;\">            <div style=\"background-color: #ecf0f1;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--<![endif]--><table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">    <tbody>        <tr>            <td style=\"overflow-wrap:break-word;word-break:break-word;padding:15px;font-family:'Poppins', sans-serif;;\" align=\"left\"><div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"><p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>A message from [[EventOrganizer]]</strong></span></p><p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[BusinessMessage]]</span></p></div></td>        </tr>    </tbody></table><!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->            </div>        </div>        <!--[if (mso)|(IE)]></td><![endif]-->        <!--[if (mso)|(IE)]><td align=\"center\" width=\"49\" style=\"background-color: #ffffff;width: 49px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->        <div class=\"u-col u-col-8p33\" style=\"max-width: 320px;min-width: 49.98px;display: table-cell;vertical-align: top;\">            <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--<![endif]--><table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">    <tbody>        <tr>            <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"><table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"><tbody>    <tr style=\"vertical-align: top\">        <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">            <span>&nbsp;</span>        </td>    </tr></tbody></table></td>        </tr>    </tbody></table><!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->            </div>        </div>        <!--[if (mso)|(IE)]></td><![endif]-->        <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->    </div></div></div>";
                    //string organizerMsgHtml = "<div class=\"u-row-container row-payment\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"49\" style=\"background-color: #ffffff;width: 49px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-8p33\" style=\"max-width: 320px;min-width: 49.98px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"500\" style=\"background-color: #ecf0f1;width: 500px;padding: 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-83p34\" style=\"max-width: 320px;min-width: 500.04px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ecf0f1;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:15px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>A message from [[EventOrganizer]]</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[BusinessMessage]]</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"49\" style=\"background-color: #ffffff;width: 49px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-8p33\" style=\"max-width: 320px;min-width: 49.98px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string businessMessage = order.business_message;

                    if (!string.IsNullOrEmpty(businessMessage))
                    {
                        OrganizerMsg = organizerMsgHtml;
                        OrganizerMsg = OrganizerMsg.Replace("[[BusinessMessage]]", businessMessage);
                        OrganizerMsg = OrganizerMsg.Replace("[[EventOrganizer]]", order.EventOrganizerName);
                    }

                    string strpath = "https://typhoon.cellarpass.com/";
                    if (ConnectionString.IndexOf("live") > -1)
                        strpath = "https://www.cellarpass.com/";

                    StringBuilder ticketLines = new StringBuilder();

                    var tictorderticket = ticketDAL.Tickets_GetTicketOrderTicketsByOrder(OrderId);
                    bool hasSelfPrintTickets = false;

                    foreach (var t in tictorderticket)
                    {
                        string validDates = "<strong>Ticket Valid: </strong>" + t.ValidStartDate.Date.ToString("MM/dd/yy");

                        if (t.ValidStartDate.Date != t.ValidEndDate.Date)
                        {
                            validDates = string.Format("<strong>Ticket Valid: </strong>{0} - {1}", t.ValidStartDate.Date.ToString("MM/dd/yy"), t.ValidEndDate.ToString("MM/dd/yy"));
                        }

                        string DeliveryType = string.Empty;
                        if (t.DeliveryType == TicketDelivery.SelfPrint)
                        {
                            hasSelfPrintTickets = true;
                            DeliveryType = GetDeliveryOptionFormatted(TicketDelivery.SelfPrint);
                        }
                        else if (t.DeliveryType == TicketDelivery.WillCall)
                        {
                            hasSelfPrintTickets = true;
                            DeliveryType = "Will Call"; //GetDeliveryOptionFormatted(TicketDelivery.WillCall);
                            //DeliveryType = string.Format(DeliveryType.Replace("Will Call: ", ""), t.WillCallLocationName);
                            if (t.WillCallLcationId > 0)
                            {
                                string wcLocation = ticketDAL.GetTicketWillCallLocationsByLocationId(t.WillCallLcationId);
                                if (wcLocation != string.Empty && wcLocation != null)
                                {
                                    DeliveryType += "<br /><p>" + wcLocation + "</p>";
                                }
                            }
                        }

                        else if (t.DeliveryType == TicketDelivery.Shipped)
                            DeliveryType = GetDeliveryOptionFormatted(TicketDelivery.Shipped, t.fulfillment_lead_time_desc);

                        //ticketTbl.Append(string.Format("<tr><td style=\"text-align:left;padding:0 5px 0 5px;\">{0}<br /><br /><small>{1}</small><br><br></td><td style=\"text-align:right;padding:0 5px 0 5px;\">{2}</td><td style=\"text-align:right;padding:0 5px 0 5px;\">{3}</td><td style=\"text-align:center;padding:0 5px 0 5px;\">{4}</td><td style=\"text-align:right;padding:0 5px 0 5px;\">{5}</td><br /></tr>", t.TicketName, validDates, t.TicketQty, DeliveryType, string.Format("{0} {1}", t.FirstName, t.LastName), string.Format(new CultureInfo("en-US"), "{0:C}", t.TicketPrice)));
                        //if (t.include_confirmation_message == true)
                        //    ticketTbl.Append(string.Format("<tr><td colspan='5'>{0}<br /><br /></td></tr>", t.confirmation_message));

                        //string ticketLine = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"><div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"><div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"><!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"199\" style=\"background-color: #ffffff;width: 199px;padding: 0px 0px 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--><div class=\"u-col u-col-42\" style=\"max-width: 320px;min-width: 252px;display: table-cell;vertical-align: top;\"><div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--<![endif]--><table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"><tbody><tr><td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"><div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\"><p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketQty}} X <strong>{{TicketLevelName}}</strong></span></p> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 16.8px;\">{{DeliveryMethod}}</span></p><p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 12px; line-height: 14.4px;\">{{TicketValidDate}}</span></p> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 12px; line-height: 14.4px;\">&nbsp;  &nbsp;</span></p></div></td></tr></tbody></table> <!--[if (!mso)&(!IE)]><!--></div> <!--<![endif]--></div></div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"148\" style=\"background-color: #ffffff;width: 148px;padding: 0px 20px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--><div class=\"u-col u-col-33p33\" style=\"max-width: 320px;min-width: 199.98px;display: table-cell;vertical-align: top;\"><div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 0px 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"><tbody><tr><td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"><div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\"><p style=\"font-size: 14px; line-height: 120%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketHolderName}}</span></p></div></td></tr></tbody></table> <!--[if (!mso)&(!IE)]><!--></div> <!--<![endif]--></div></div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"252\" style=\"background-color: #ffffff;width: 252px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--><div class=\"u-col u-col-24p67\" style=\"max-width: 320px;min-width: 148.02px;display: table-cell;vertical-align: top;\"><div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"><!--<![endif]--><table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"><tbody><tr><td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"><div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\"><p style=\"font-size: 14px; line-height: 120%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketPrice}}</span></p></div></td></tr></tbody></table> <!--[if (!mso)&(!IE)]><!--></div> <!--<![endif]--></div></div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--></div></div></div>";
                        //string ticketLine = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"><div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\">    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">        <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"199\" style=\"background-color: #ffffff;width: 199px;padding: 0px 0px 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->        <div class=\"u-col u-col-33p33\" style=\"max-width: 320px;min-width: 199.98px;display: table-cell;vertical-align: top;\">            <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 0px 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                    <!--<![endif]-->                    <table style=\"font-family:'Poppins', sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                        <tbody>                            <tr>                                <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;\" align=\"left\">                                    <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\">                                        <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketQty}} X <strong>{{TicketLevelName}}</strong></span></p></div>                                </td>                            </tr>                        </tbody>                    </table> <!--[if (!mso)&(!IE)]><!-->                </div> <!--<![endif]-->            </div>        </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"252\" style=\"background-color: #ffffff;width: 252px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--><div class=\"u-col u-col-42\" style=\"max-width: 320px;min-width: 252px;display: table-cell;vertical-align: top;\">            <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                <!--[if (!mso)&(!IE)]><!-->                <div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                    <!--<![endif]-->                    <table style=\"font-family:'Poppins', sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                        <tbody>                            <tr>                                <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;\" align=\"left\">                                    <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\">                                        <p style=\"font-size: 14px; line-height: 120%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketHolderName}}</span></p>                                        <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 16.8px;\">{{DeliveryMethod}}</span></p>                                        <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 12px; line-height: 14.4px;\">{{TicketValidDate}}</span></p>                                    </div>                                </td>                            </tr>                        </tbody>                    </table> <!--[if (!mso)&(!IE)]><!-->                </div> <!--<![endif]-->            </div>        </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"148\" style=\"background-color: #ffffff;width: 148px;padding: 0px 20px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->        <div class=\"u-col u-col-24p67\" style=\"max-width: 320px;min-width: 148.02px;display: table-cell;vertical-align: top;\">            <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                <!--[if (!mso)&(!IE)]><!-->                <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                    <!--<![endif]-->                    <table style=\"font-family:'Poppins', sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                        <tbody>                            <tr>                                <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;\" align=\"left\">                                    <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\">                                        <p style=\"font-size: 14px; line-height: 120%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketPrice}}</span></p>                                    </div>                                </td>                            </tr>                        </tbody>                    </table> <!--[if (!mso)&(!IE)]><!-->                </div> <!--<![endif]-->            </div>        </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->    </div></div></div>";
                        //string ticketLine = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">    <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\">        <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">            <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"199\" style=\"background-color: #ffffff;width: 199px;padding: 0px 0px 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->            <div class=\"u-col u-col-33p33\" style=\"max-width: 320px; min-width: 451.98px; display: table-cell; vertical-align: top;\">                <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                    <!--[if (!mso)&(!IE)]><!-->                    <div style=\"box-sizing: border-box; height: 100%; padding: 0px 0px 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                        <!--<![endif]-->                        <table style=\"font-family:'Poppins', sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                            <tbody>                                <tr>                                    <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;\" align=\"left\">                                        <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\">                                            <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketQty}} X <strong>{{TicketLevelName}}</strong></span></p>                                            <p style=\"font-size: 14px; line-height: 120%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketHolderName}}</span></p>                                            <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 16.8px;\">{{DeliveryMethod}}</span></p>                                            <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 12px; line-height: 14.4px;\">{{TicketValidDate}}</span></p>                                        </div>                                    </td>                                </tr>                            </tbody>                        </table> <!--[if (!mso)&(!IE)]><!-->                    </div> <!--<![endif]-->                </div>            </div>            <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"252\" style=\"background-color: #ffffff;width: 252px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->                                    <!--<div class=\"u-col u-col-42\" style=\"max-width: 320px;min-width: 252px;display: table-cell;vertical-align: top;\">                <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">-->                    <!--[if (!mso)&(!IE)]><!-->                    <!--<div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">-->                        <!--<![endif]-->                        <!--<table style=\"font-family:'Poppins', sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                            <tbody>                                <tr>                                    <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;\" align=\"left\">                                        <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\">                                            <p style=\"font-size: 14px; line-height: 120%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketHolderName}}</span></p>                                            <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 16.8px;\">{{DeliveryMethod}}</span></p>                                            <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 12px; line-height: 14.4px;\">{{TicketValidDate}}</span></p>                                        </div>                                    </td>                                </tr>                            </tbody>                        </table>--> <!--[if (!mso)&(!IE)]><!-->                    <!--</div>--> <!--<![endif]-->                <!--</div>            </div>-->            <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"148\" style=\"background-color: #ffffff;width: 148px;padding: 0px 20px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->            <div class=\"u-col u-col-24p67\" style=\"max-width: 320px;min-width: 148.02px;display: table-cell;vertical-align: top;\">                <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                    <!--[if (!mso)&(!IE)]><!-->                    <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                        <!--<![endif]-->                        <table style=\"font-family:'Poppins', sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                            <tbody>                                <tr>                                    <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;\" align=\"left\">                                        <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\">                                            <p style=\"font-size: 14px; line-height: 120%; text-align: right;\">                                                <span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketPrice}}</span>                                            </p>                                        </div>                                    </td>                                </tr>                            </tbody>                        </table>                        <!--[if (!mso)&(!IE)]><!-->                    </div> <!--<![endif]-->                </div>            </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->        </div>    </div></div>";

                        string ticketLine = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">    <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;background-color: transparent;\">        <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">            <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"199\" style=\"background-color: #ffffff;width: 199px;padding: 0px 0px 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->            <div class=\"u-col u-col-33p33\" style=\"max-width: 320px; min-width: 451.98px; display: table-cell; vertical-align: top;\">                <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                    <!--[if (!mso)&(!IE)]><!-->                    <div style=\"box-sizing: border-box; height: 100%; padding: 0px 0px 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                        <!--<![endif]-->                        <table style=\"font-family:'Poppins', sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                            <tbody>                                <tr>                                    <td style=\"padding:10px;font-family:'Poppins', sans-serif;\" align=\"left\" nowrap>                                        <div style=\"line-height: 120%; text-align: left;\">                                            <p style=\"font-size: 14px; line-height: 120%;\"><pre><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketQty}} X <strong>{{TicketLevelName}}</strong></span></pre></p>                                            <p style=\"font-size: 14px; line-height: 120%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketHolderName}}</span></p>                                            <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 16.8px;\">{{DeliveryMethod}}</span></p>                                            <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 12px; line-height: 14.4px;\">{{TicketValidDate}}</span></p>                                        </div>                                    </td>                                </tr>                            </tbody>                        </table> <!--[if (!mso)&(!IE)]><!-->                    </div> <!--<![endif]-->                </div>            </div>            <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"252\" style=\"background-color: #ffffff;width: 252px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->            <!--<div class=\"u-col u-col-42\" style=\"max-width: 320px;min-width: 252px;display: table-cell;vertical-align: top;\">                <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">-->            <!--[if (!mso)&(!IE)]><!-->            <!--<div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">-->            <!--<![endif]-->            <!--<table style=\"font-family:'Poppins', sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                <tbody>                    <tr>                        <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;\" align=\"left\">                            <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\">                                <p style=\"font-size: 14px; line-height: 120%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketHolderName}}</span></p>                                <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 16.8px;\">{{DeliveryMethod}}</span></p>                                <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 12px; line-height: 14.4px;\">{{TicketValidDate}}</span></p>                            </div>                        </td>                    </tr>                </tbody>            </table>-->            <!--[if (!mso)&(!IE)]><!-->            <!--</div>--> <!--<![endif]-->            <!--</div>            </div>-->            <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"148\" style=\"background-color: #ffffff;width: 148px;padding: 0px 20px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]-->            <div class=\"u-col u-col-24p67\" style=\"max-width: 320px;min-width: 148.02px;display: table-cell;vertical-align: top;\">                <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                    <!--[if (!mso)&(!IE)]><!-->                    <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                        <!--<![endif]-->                        <table style=\"font-family:'Poppins', sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                            <tbody>                                <tr>                                    <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;\" align=\"left\">                                        <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\">                                            <p style=\"font-size: 14px; line-height: 120%; text-align: right;\">                                                <span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketPrice}}</span>                                            </p>                                        </div>                                    </td>                                </tr>                            </tbody>                        </table>                        <!--[if (!mso)&(!IE)]><!-->                    </div> <!--<![endif]-->                </div>            </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->        </div>    </div></div>";
                        ticketLine = ticketLine.Replace("{{TicketHolderName}}", string.Format("{0} {1}", t.FirstName, t.LastName));
                        ticketLine = ticketLine.Replace("{{TicketQty}}", Convert.ToString(t.TicketQty));
                        ticketLine = ticketLine.Replace("{{TicketLevelName}}", t.TicketName);

                        if (t.DeliveryType == TicketDelivery.SelfPrint)
                        {
                            ticketLine = ticketLine.Replace("{{DeliveryMethod}}", String.Format("Self Print- <a href=\"{0}\" target=\"_blank\">Print Ticket</a>", string.Format("{1}tix-confirmation?orderNumber={0}", order.OrderGUID, strpath)));
                        }
                        else if (t.DeliveryType == TicketDelivery.Shipped)
                        {
                            ticketLine = ticketLine.Replace("{{DeliveryMethod}}", GetDeliveryOptionFormatted(TicketDelivery.Shipped, t.fulfillment_lead_time_desc));
                        }
                        else if (t.DeliveryType == TicketDelivery.WillCall)
                        {
                            DeliveryType = "Will Call";
                            if (t.WillCallLocationName.Length > 0)
                            {
                                DeliveryType = "<strong>Will Call: </strong>" + t.WillCallLocationName + "<br />" + t.WillCallLocationAddress;
                            }

                            ticketLine = ticketLine.Replace("{{DeliveryMethod}}", DeliveryType);
                        }

                        ticketLine = ticketLine.Replace("{{TicketValidDate}}", validDates);
                        ticketLine = ticketLine.Replace("{{TicketPrice}}", string.Format(new CultureInfo("en-US"), "{0:C}", t.TicketPrice));
                        ticketLines.Append(ticketLine);
                    }

                    //ticketTbl.Append("</table>");                    

                    string eventDate = order.EventStartDateTime.ToString("MM/dd/yy");

                    if (order.EventEndDateTime.Date != order.EventStartDateTime.Date)
                    {
                        eventDate = string.Format("{0} - {1}", order.EventStartDateTime.ToString("MM/dd/yy"), order.EventEndDateTime.ToString("MM/dd/yy"));
                    }

                    string OrderTotalMsg = string.Empty;

                    if (order.PaymentType == PaymentType.CreditCard)
                    {
                        OrderTotalMsg = string.Format("You will see a charge in the amount of {0} from '{1}' on your next credit card statement.", string.Format(new CultureInfo("en-US"), "{0:C}", order.OrderTotal), Utility.GenerateChargeDescription(order.wineryName, order.DynamicPaymentDesc, order.Id.ToString(), Configuration.Gateway.Braintree));
                    }

                    string eventCancelPolicy = "No Refunds, No Transfers";

                    if (order.refund_policy != TicketRefundPolicy.Oncasbycasebasis)
                    {
                        eventCancelPolicy = order.refund_policy_text;
                    }

                    if (order.PrimaryCategory == TicketCategory.Passport)
                    {
                        RsvpGuestContent = RsvpGuestContent.Replace("[[EventStarts]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[EventEnds]]", "");
                    }

                    RsvpGuestContent = RsvpGuestContent.Replace("[[OrderNumber]]", order.Id.ToString());
                    RsvpGuestContent = RsvpGuestContent.Replace("[[PurchaseDate]]", order.OrderDate.ToString("MM/dd/yy"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[Tickets]]", ticketLines.ToString());



                    RsvpGuestContent = RsvpGuestContent.Replace("[[ConfirmationURL]]", string.Format("{1}tix-confirmation?orderNumber={0}", order.OrderGUID, strpath));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[FollowURL]]", string.Format("{1}{0}?fav=1", Utility.GetFriendlyURL(order.EventTitle, order.Tickets_Event_Id,order.event_url), strpath));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ContactOrganizer]]", string.Format("{1}{0}?co=1", Utility.GetFriendlyURL(order.EventTitle, order.Tickets_Event_Id,order.event_url), strpath));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventDetailsURL]]", string.Format("{1}{0}", Utility.GetFriendlyURL(order.EventTitle, order.Tickets_Event_Id,order.event_url), strpath));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[PrivacyPolicyURL]]", string.Format("{0}business/legal/cellarpass-privacy-policy", strpath));

                    RsvpGuestContent = RsvpGuestContent.Replace("[[PaymentMessage]]", OrderTotalMsg);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[OrderTotal]]", string.Format(new CultureInfo("en-US"), "{0:C}", order.OrderTotal));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[TotalPaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", order.PaidAmt));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[TicketHolder]]", string.Format("{0} {1}", order.BillFirstName, order.BillLastName));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[TicketBuyerEmail]]", order.BillEmailAddress);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[TicketBuyerPhone]]", StringHelpers.FormatTelephoneNumber(order.BillHomePhone.ToString(), order.BillCountry));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", order.wineryName);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventName]]", order.EventTitle);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[VenueName]]", "");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[VenueAddress]]", order.EventVenueAddress);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventDate]]", eventDate);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventStarts]]", order.EventStartDateTime.ToString("hh:mm tt"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventEnds]]", order.EventEndDateTime.ToString("hh:mm tt"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventOrganizer]]", order.EventOrganizerName);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventOrganizerPhone]]", StringHelpers.FormatTelephoneNumber(order.EventOrganizerPhone.ToString(), order.BillCountry));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventURL]]", Utility.GenerateEmailButton("View Event Details", string.Format("https://www.cellarpass.com/{0}", Utility.GetFriendlyURL(order.EventTitle, order.Tickets_Event_Id,order.event_url)), "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", StringHelpers.FormatTelephoneNumber(order.EventOrganizerPhone.ToString(), order.BillCountry));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", order.EventOrganizerEmail);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[DeliveryOption]]", "");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[OrderNotes]]", order.OrderNote);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[PostCaptureMessage]]", "");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventCancelPolicy]]", eventCancelPolicy);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[BusinessMessage]]", order.business_message);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[OrganizerMessage]]", OrganizerMsg);

                    RsvpGuestContent = RsvpGuestContent.Replace("[[TixEventPolicy]]", EventPolicy);

                    string bannerImageHtml = "";
                    bannerImageHtml = Utility.GetBannerImageHtmlForTicketEvent(order.EventBannerImage);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventBannerImage]]", bannerImageHtml);

                    //Broadcast button
                    string ButtonURL = "";

                    if (order.PrimaryCategory == TicketCategory.LiveBroadcast)
                    {
                        ButtonURL = Utility.GenerateEmailButton("View Broadcast", string.Format("https://www.cellarpass.com/{0}", Utility.GetFriendlyURL(order.EventTitle, order.Tickets_Event_Id,order.event_url)), "#dc3545", "#dc3545", "15px", "15px", "#ffffff");
                    }
                    else if (order.PrimaryCategory == TicketCategory.Webinar)
                    {
                        ButtonURL = Utility.GenerateEmailButton("Join Meeting", string.Format("https://www.cellarpass.com/{0}", Utility.GetFriendlyURL(order.EventTitle, order.Tickets_Event_Id,order.event_url)), "#dc3545", "#dc3545", "15px", "15px", "#ffffff");
                    }

                    RsvpGuestContent = RsvpGuestContent.Replace("[[BroadcastURL]]", ButtonURL);

                    //if (!string.IsNullOrWhiteSpace(order.ItineraryGUID))
                    //    RsvpGuestContent = RsvpGuestContent.Replace("[[ViewItinerary]]", Utility.GenerateEmailButton("View Itinerary", string.Format("{1}itinerary/{0}?v=agenda", order.ItineraryGUID, strpath), "#dc3545", "#dc3545", "15px", "15px", "#ffffff"));
                    //else
                    //    RsvpGuestContent = RsvpGuestContent.Replace("[[ViewItinerary]]", "");

                    string viewItineraryHtml = "";

                    if (!string.IsNullOrWhiteSpace(order.ItineraryGUID))
                    {
                        viewItineraryHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[ViewItineraryURL]]\" style=\"height:47px; v-text-anchor:middle; width:516px;\" arcsize=\"8.5%\" strokecolor=\"#e03e2d\" strokeweight=\"1px\" fillcolor=\"#ffffff\"><w:anchorlock/><center style=\"color:#e03e2d;font-family:'Poppins', sans-serif;\"><![endif]--> <a href=\"[[ViewItineraryURL]]\" target=\"_blank\" class=\"v-button\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #e03e2d; background-color: #ffffff; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:96%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;border-top-width: 1px; border-top-style: solid; border-top-color: #e03e2d; border-left-width: 1px; border-left-style: solid; border-left-color: #e03e2d; border-right-width: 1px; border-right-style: solid; border-right-color: #e03e2d; border-bottom-width: 1px; border-bottom-style: solid; border-bottom-color: #e03e2d;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"font-size: 14px; line-height: 16.8px; font-family: Poppins, sans-serif;\">VIEW ITINERARY</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                        viewItineraryHtml = viewItineraryHtml.Replace("[[ViewItineraryURL]]", string.Format("{1}itinerary/{0}?v=agenda", order.ItineraryGUID, strpath));
                    }

                    RsvpGuestContent = RsvpGuestContent.Replace("[[ViewItinerary]]", viewItineraryHtml);

                    string AdditionalTicketInfo = "";
                    StringBuilder TicketLevelMessages = new StringBuilder();
                    string additionalInfoHeading = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 25px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 25px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\"><strong>Additional Ticket Information</strong></span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string ticketlevelMsg = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\"><strong>{{TicketLevelName}}</strong></span></p> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketLevelMessage}}</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

                    foreach (var item in ticketDAL.GetAdditionalTicketInfoByID(order.Id))
                    {
                        string example1 = ticketlevelMsg;
                        example1 = example1.Replace("{{TicketLevelName}}", item.TicketName);
                        example1 = example1.Replace("{{TicketLevelMessage}}", item.ConfirmationMessage);
                        TicketLevelMessages.Append(example1);
                    }

                    if (TicketLevelMessages.ToString().Length > 0)
                        AdditionalTicketInfo = additionalInfoHeading + TicketLevelMessages.ToString();

                    RsvpGuestContent = RsvpGuestContent.Replace("[[AdditionalTicketInformation]]", AdditionalTicketInfo);

                    string MapURL = "";

                    if (order.PrimaryCategory == TicketCategory.Webinar || order.PrimaryCategory == TicketCategory.Membership || order.PrimaryCategory == TicketCategory.LiveBroadcast)
                    {
                        MapURL = "";
                    }
                    else
                    {
                        if (order.geo_latitude != "" && order.geo_longitude != "" && order.venue_full_address != "")
                        {
                            string googleAPIKey = "AIzaSyDKGd8SyV0GhpzD-onvM0ItLpzq5GhCFfA";
                            if (_appSetting != null && _appSetting.Value != null && !string.IsNullOrEmpty(_appSetting.Value.GoogleAPIKey))
                            {
                                googleAPIKey = _appSetting.Value.GoogleAPIKey;
                            }

                            //Fulladdress + ' ' + $('#txtCity').val() + ' ' + $('#ddlStates :selected').text() + ' ' + $('#txtPostalCode').val() + ' ' + $('#ddlCountry').val()

                            string googleurl = "https://www.google.com/maps/place/" + HttpUtility.UrlEncode(order.venue_full_address);

                            MapURL = await Utility.GetMapImageHtmlByLocation(googleAPIKey, order.geo_latitude, order.geo_longitude, order.Tickets_Event_Id, googleurl);
                        }
                    }

                    RsvpGuestContent = RsvpGuestContent.Replace("[[MapURL]]", MapURL);

                    var adminContent = "";
                    string TicketHolderOrBuyer = string.Format("{0} {1}", order.BillFirstName, order.BillLastName);
                    adminContent += "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\"><strong>Good News!</strong></span></p> <p style=\"font-size: 14px; line-height: 120%;\"><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">You've received an order for [[EventName]] happening [[EventDate]].</span></p> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">Below is a copy of the order confirmation that was sent to:</span></p> <p style=\"font-size: 14px; line-height: 120%;\"><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">[[TicketBuyerName]]</span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">[[TicketBuyerEmail]]</span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">Order [[OrderNumber]]</span></p> <p style=\"font-size: 14px; line-height: 120%;\"><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">Cheers!</span></p> <p style=\"font-size: 14px; line-height: 120%;\"><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">CellarPass Team</span></p> </div> </td> </tr> </tbody> </table> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:20px 10px 30px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    adminContent = adminContent.Replace("[[EventName]]", order.EventTitle);
                    adminContent = adminContent.Replace("[[EventDate]]", eventDate);
                    adminContent = adminContent.Replace("[[TicketBuyerName]]", TicketHolderOrBuyer);
                    adminContent = adminContent.Replace("[[TicketBuyerEmail]]", order.BillEmailAddress);
                    adminContent = adminContent.Replace("[[OrderNumber]]", order.Id.ToString());


                    if (order.BillEmailAddress.IndexOf("@noemail") == -1 || AdminEmail)
                    {
                        EmailAttachment attachment = new EmailAttachment();
                        if (hasSelfPrintTickets)
                        {
                            attachment.Name = string.Format("Ticket_{0}.pdf", order.Id);
                            attachment.ContentBytes = GenerateOrderPDF(order.OrderGUID);
                            attachment.Contents = "";
                        }

                        string EmailTo = order.BillEmailAddress;
                        string EventOrganizerName = order.EventOrganizerName;
                        string EventOrganizerEmail = order.EventOrganizerEmail;

                        if (string.IsNullOrEmpty(EventOrganizerEmail))
                        {
                            EventOrganizerEmail = "";
                        }

                        if (!string.IsNullOrEmpty(AlternativeEmail) || !string.IsNullOrEmpty(EmailTo))
                        {
                            if (!string.IsNullOrEmpty(AlternativeEmail))
                                EmailTo = AlternativeEmail;
                            if (hasSelfPrintTickets)
                            {
                                response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.TicketSale, order.Id, ew.EmailFrom, EmailTo, SubjectContent, RsvpGuestContent.Replace("[[AdminMessage]]", ""), 0, attachment, ew.Id, EventOrganizerEmail, EventOrganizerName));
                            }
                            else
                            {
                                response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.TicketSale, order.Id, ew.EmailFrom, EmailTo, SubjectContent, RsvpGuestContent.Replace("[[AdminMessage]]", ""), 0, null, ew.Id, EventOrganizerEmail, EventOrganizerName));
                            }
                        }

                        if (AdminEmail)
                        {
                            EmailTo = "";
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                            if (order.internal_notification_recipient.Count > 0)
                            {
                                foreach (var item in order.internal_notification_recipient)
                                {
                                    if (!string.IsNullOrEmpty(EmailTo))
                                        EmailTo = EmailTo + ",";

                                    EmailTo = EmailTo + userDAL.GetUserNameById(item);
                                }
                            }
                            else if (ew.EmailTo != null)
                            {
                                if (!string.IsNullOrEmpty(ew.EmailTo))
                                {
                                    EmailTo = ew.EmailTo;
                                }
                            }

                            if (!string.IsNullOrEmpty(EmailTo))
                            {
                                SubjectContent = ew.EmailSubjectAdmin;

                                SubjectContent = SubjectContent.Replace("[[EventName]]", order.EventTitle);
                                SubjectContent = SubjectContent.Replace("[[EventOrganizer]]", order.wineryName);

                                if (hasSelfPrintTickets)
                                {
                                    response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.TicketSale, order.Id, ew.EmailFrom, EmailTo, SubjectContent, RsvpGuestContent.Replace("[[AdminMessage]]", adminContent), 0, attachment, ew.Id, EventOrganizerEmail, EventOrganizerName));
                                }
                                else
                                {
                                    response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.TicketSale, order.Id, ew.EmailFrom, EmailTo, SubjectContent, RsvpGuestContent.Replace("[[AdminMessage]]", adminContent), 0, null, ew.Id, EventOrganizerEmail, EventOrganizerName));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "ProcessSendCpRsvpTicketSaleEmail:  OrderId-" + OrderId.ToString() + ",Message-" + ex.Message.ToString(), "", 1, member_id);
            }
            return response;
        }

        public async Task<EmailResponse> ProcessSendCpTicketPostCaptureEmail(ReservationEmailModel model)
        {
            int member_id = 0;
            var response = new EmailResponse();
            LogDAL logDAL = new LogDAL(ConnectionString);
            try
            {
                TicketDAL ticketDAL = new TicketDAL(ConnectionString);
                EventDAL eventDAL = new EventDAL(ConnectionString);
                int OrderId = model.RsvpId;
                int TicketOrderTicketId = model.UId;

                if (TicketOrderTicketId == 0 && model.data != null)
                {
                    TicketOrderTicketId = model.data.UId;
                }

                if (OrderId == 0 && model.data != null)
                {
                    OrderId = model.data.RsvpId;
                }

                //logDAL.InsertLog("Email", "ProcessSendCPTicketPostCaptureEmail: orderId=" + model.RsvpId.ToString() + ", TicketId = " + model.UId.ToString(), "");

                var order = ticketDAL.GetTicketOrder(OrderId, "");
                member_id = order.member_id;
                string RsvpGuestContent = "";
                string SubjectContent = "";

                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew;
                ew = emailDAL.GetEmailContent((int)EmailType.TicketPostCapture,0);
                if (ew != null && ew.Active == true)
                {
                    //logDAL.InsertLog("Email", "ProcessSendCPTicketPostCaptureEmail: Got email content data", "");

                    SubjectContent = ew.EmailSubject;
                    RsvpGuestContent = ew.EmailBody;

                    string TicketHolderOrBuyer = string.Format("{0} {1}", order.bill_first_name, order.bill_last_name);

                    string totalTblHeading = string.Empty;

                    var tictorderticket = order.ticket_order_ticket;
                    string ticketLevelName = "";
                    string ticketHolderName = "";
                    string postCaptureKey = "";
                    string inviteToEmailAddress = "";
                    foreach (var t in tictorderticket)
                    {
                        if (t.id == model.UId)
                        {
                            ticketLevelName = t.ticket_name;
                            ticketHolderName = String.Format("{0} {1}", t.first_name, t.last_name);
                            postCaptureKey = t.post_capture_key;
                            inviteToEmailAddress = t.email;
                            break;
                        }

                    }

                    string eventDate = order.event_start_date_time.ToString("MM/dd/yyyy");

                    if (order.event_end_date_time.Date != order.event_start_date_time.Date)
                    {
                        eventDate = string.Format("{0} - {1}", order.event_start_date_time.ToString("MM/dd/yyyy"), order.event_end_date_time.ToString("MM/dd/yyyy"));
                    }

                    SubjectContent = SubjectContent.Replace("[[EventName]]", order.event_title);
                    SubjectContent = SubjectContent.Replace("[[EventDate]]", eventDate);
                    SubjectContent = SubjectContent.Replace("[[EventStarts]]", order.event_start_date_time.ToString("hh:mm tt"));
                    SubjectContent = SubjectContent.Replace("[[EventEnds]]", order.event_end_date_time.ToString("hh:mm tt"));
                    SubjectContent = SubjectContent.Replace("[[EventOrganizer]]", order.event_organizer_name);
                    SubjectContent = SubjectContent.Replace("[[EventOrganizerPhone]]", StringHelpers.FormatTelephoneNumber(order.event_organizer_phone.ToString(), order.bill_country));
                    SubjectContent = SubjectContent.Replace("[[EventURL]]", "");
                    SubjectContent = SubjectContent.Replace("[[TicketLevelName]]", ticketLevelName);
                    SubjectContent = SubjectContent.Replace("[[TicketHolder]]", ticketHolderName);
                    SubjectContent = SubjectContent.Replace("[[TicketBuyerName]]", TicketHolderOrBuyer);
                    SubjectContent = SubjectContent.Replace("[[InviteLink]]", "");
                    SubjectContent = SubjectContent.Replace("[[InviteLinkTest]]", "");
                    SubjectContent = SubjectContent.Replace("[[PostCaptureMessage]]", "");



                    string bannerImageHtml = "";
                    bannerImageHtml = Utility.GetBannerImageHtmlForTicketEvent(order.event_image_big);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventBannerImage]]", bannerImageHtml);

                    string ButtonURL = "";

                    if (order.primary_category == TicketCategory.LiveBroadcast)
                    {
                        ButtonURL = Utility.GenerateEmailButton("View Broadcast", string.Format("https://www.cellarpass.com/{0}", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id,order.event_url)), "#dc3545", "#dc3545", "15px", "15px", "#ffffff");
                    }
                    else if (order.primary_category == TicketCategory.Webinar)
                    {
                        ButtonURL = Utility.GenerateEmailButton("Join Meeting", string.Format("https://www.cellarpass.com/{0}", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id, order.event_url)), "#dc3545", "#dc3545", "15px", "15px", "#ffffff");
                    }

                    RsvpGuestContent = RsvpGuestContent.Replace("[[BroadcastURL]]", ButtonURL);

                    string strpath = "https://typhoon.cellarpass.com/";
                    if (ConnectionString.IndexOf("live") > -1)
                        strpath = "https://www.cellarpass.com/";

                    //if (!string.IsNullOrWhiteSpace(order.itinerary_guid))
                    //    RsvpGuestContent = RsvpGuestContent.Replace("[[ViewItinerary]]", Utility.GenerateEmailButton("View Itinerary", string.Format("{1}itinerary/{0}?v=agenda", order.itinerary_guid, strpath), "#dc3545", "#dc3545", "15px", "15px", "#ffffff"));
                    //else
                    //    RsvpGuestContent = RsvpGuestContent.Replace("[[ViewItinerary]]", "");

                    string viewItineraryHtml = "";

                    if (!string.IsNullOrWhiteSpace(order.itinerary_guid))
                    {
                        viewItineraryHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[ViewItineraryURL]]\" style=\"height:47px; v-text-anchor:middle; width:516px;\" arcsize=\"8.5%\" strokecolor=\"#e03e2d\" strokeweight=\"1px\" fillcolor=\"#ffffff\"><w:anchorlock/><center style=\"color:#e03e2d;font-family:'Poppins', sans-serif;\"><![endif]--> <a href=\"[[ViewItineraryURL]]\" target=\"_blank\" class=\"v-button\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #e03e2d; background-color: #ffffff; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:96%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;border-top-width: 1px; border-top-style: solid; border-top-color: #e03e2d; border-left-width: 1px; border-left-style: solid; border-left-color: #e03e2d; border-right-width: 1px; border-right-style: solid; border-right-color: #e03e2d; border-bottom-width: 1px; border-bottom-style: solid; border-bottom-color: #e03e2d;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"font-size: 14px; line-height: 16.8px; font-family: Poppins, sans-serif;\">VIEW ITINERARY</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                        viewItineraryHtml = viewItineraryHtml.Replace("[[ViewItineraryURL]]", string.Format("{1}itinerary/{0}?v=agenda", order.itinerary_guid, strpath));
                    }

                    RsvpGuestContent = RsvpGuestContent.Replace("[[ViewItinerary]]", viewItineraryHtml);

                    string AdditionalTicketInfo = "";
                    StringBuilder TicketLevelMessages = new StringBuilder();
                    string additionalInfoHeading = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 25px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 25px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\"><strong>Additional Ticket Information</strong></span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string ticketlevelMsg = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\"><strong>{{TicketLevelName}}</strong></span></p> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketLevelMessage}}</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

                    foreach (var item in ticketDAL.GetAdditionalTicketInfoByID(order.id))
                    {
                        string example1 = ticketlevelMsg;
                        example1 = example1.Replace("{{TicketLevelName}}", item.TicketName);
                        example1 = example1.Replace("{{TicketLevelMessage}}", item.ConfirmationMessage);
                        TicketLevelMessages.Append(example1);
                    }

                    if (TicketLevelMessages.ToString().Length > 0)
                        AdditionalTicketInfo = additionalInfoHeading + TicketLevelMessages.ToString();

                    RsvpGuestContent = RsvpGuestContent.Replace("[[AdditionalTicketInformation]]", AdditionalTicketInfo);

                    string MapURL = "";

                    if (order.geo_latitude != "" && order.geo_longitude != "")
                    {
                        string googleAPIKey = "AIzaSyDKGd8SyV0GhpzD-onvM0ItLpzq5GhCFfA";
                        if (_appSetting != null && _appSetting.Value != null && !string.IsNullOrEmpty(_appSetting.Value.GoogleAPIKey))
                        {
                            googleAPIKey = _appSetting.Value.GoogleAPIKey;
                        }

                        string googleurl = "https://www.google.com/maps/place/" + HttpUtility.UrlEncode(order.venue_full_address);

                        MapURL = await Utility.GetMapImageHtmlByLocation(googleAPIKey, order.geo_latitude, order.geo_longitude, order.tickets_event_id, googleurl);
                    }

                    RsvpGuestContent = RsvpGuestContent.Replace("[[MapURL]]", MapURL);

                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventName]]", order.event_title);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventDate]]", eventDate);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventStarts]]", order.event_start_date_time.ToString("hh:mm tt"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventEnds]]", order.event_end_date_time.ToString("hh:mm tt"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventOrganizer]]", order.event_organizer_name);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventOrganizerPhone]]", StringHelpers.FormatTelephoneNumber(order.event_organizer_phone.ToString(), order.bill_country));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventURL]]", Utility.GenerateEmailButton("View Event Details", string.Format("https://www.cellarpass.com/{0}", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id,order.event_url)), "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[TicketLevelName]]", ticketLevelName);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[TicketHolder]]", ticketHolderName);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[TicketBuyerName]]", TicketHolderOrBuyer);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[InviteLink]]", Utility.GenerateEmailButton("Register", String.Format("https://www.cellarpass.com/tickets-claim?ticketId={0}", postCaptureKey), "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[InviteLinkTest]]", Utility.GenerateEmailButton("Register", String.Format("https://www.cellarpass.com/tickets-claim?ticketId={0}", postCaptureKey), "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[PostCaptureMessage]]", "");

                    //RsvpGuestContent = RsvpGuestContent.Replace("[[PostCaptureMessage]]", postCaptureContent);
                    //logDAL.InsertLog("Email", "ProcessSendCPTicketPostCaptureEmail: email Id:" + inviteToEmailAddress, "");
                    if (inviteToEmailAddress.IndexOf("@noemail") == -1 || model.AdminEmail)
                    {
                        string EmailTo = inviteToEmailAddress;
                        if (model.AdminEmail)
                        {
                            EmailTo = "";
                            if (ew.EmailTo != null)
                            {
                                if (!string.IsNullOrEmpty(ew.EmailTo))
                                {
                                    EmailTo = ew.EmailTo;
                                }
                            }
                            SubjectContent = ew.EmailSubjectAdmin;
                        }

                        if (!string.IsNullOrEmpty(EmailTo))
                        {
                            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.TicketPostCapture, order.id, ew.EmailFrom, EmailTo, SubjectContent, RsvpGuestContent, 0, null, ew.Id));
                            //logDAL.InsertLog("Email", "ProcessSendCPTicketPostCaptureEmail: email sent, response:" + response.emailSent.ToString(), "");
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("Email", "ProcessSendCPTicketPostCaptureEmail: orderId=" + model.RsvpId.ToString() + "TicketId = " + model.UId.ToString() + ", error: " + ex.ToString(), "", 1, member_id);
            }
            return response;
        }

        public async Task<EmailResponse> ProcessTicketSalesConfirmationEmail(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(ConnectionString);
                EventDAL eventDAL = new EventDAL(ConnectionString);
                int OrderId = model.RsvpId;
                int TicketOrderTicketId = model.UId;
                string AlternativeEmail = model.CCGuestEmail;

                if (OrderId == 0 && model.data != null)
                {
                    OrderId = model.data.RsvpId;
                }

                if (TicketOrderTicketId == 0 && model.data != null)
                {
                    TicketOrderTicketId = model.data.UId;
                }

                if (string.IsNullOrEmpty(AlternativeEmail) && model.data != null)
                {
                    AlternativeEmail = model.data.CCGuestEmail;
                }

                var order = ticketDAL.GetTicketOrder(OrderId, "");


                string RsvpGuestContent = "";
                string SubjectContent = "";

                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew = new EmailContent();

                //int EmailReceiptTemplate = order.email_receipt_template;

                //if (EmailReceiptTemplate > 0)
                //    ew = emailDAL.GetEmailContentByID(EmailReceiptTemplate);

                if (ew == null)
                    ew = emailDAL.GetEmailContent((int)EmailTemplates.RsvpTicketSalesConfirmation, -9999);
                else if (ew.Active == false)
                    ew = emailDAL.GetEmailContent((int)EmailTemplates.RsvpTicketSalesConfirmation, -9999);

                if (ew != null && ew.Active == true)
                {
                    SubjectContent = ew.EmailSubject;
                    RsvpGuestContent = ew.EmailBody;

                    SubjectContent = SubjectContent.Replace("[[EventOrganizer]]", order.member_name);
                    SubjectContent = SubjectContent.Replace("[[EventName]]", order.event_title);

                    string EventPolicy = "";
                    string EventPolicyHtml = "<div class=\"u-row-container row-payment\" style=\"padding: 0px;background-color: transparent\">                                                    <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\">                                                        <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">                                                            <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">   <tr>      <td style=\"padding: 0px;background-color: transparent;\" align=\"center\">         <table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\">            <tr style=\"background-color: transparent;\">               <![endif]-->                                                            <!--[if (mso)|(IE)]><td align=\"center\" width=\"49\" style=\"background-color: #ffffff;width: 49px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\">   <![endif]-->                                                            <div class=\"u-col u-col-8p33\" style=\"max-width: 320px;min-width: 49.98px;display: table-cell;vertical-align: top;\">                                                                <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                                                                    <!--[if (!mso)&(!IE)]><!-->                                                                    <div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                                                                        <!--<![endif]-->                                                                        <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                                                                            <tbody>                                                                                <tr>                                                                                    <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\">                                                                                        <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">                                                                                            <tbody>                                                                                                <tr style=\"vertical-align: top\">                                                                                                    <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">            <span>&nbsp;</span>        </td>                                                                                                </tr>                                                                                            </tbody>                                                                                        </table>                                                                                    </td>                                                                                </tr>                                                                            </tbody>                                                                        </table>                                                                        <!--[if (!mso)&(!IE)]><!-->                                                                    </div>                                                                    <!--<![endif]-->                                                                </div>                                                            </div>                                                            <!--[if (mso)|(IE)]></td><![endif]-->                                                            <!--[if (mso)|(IE)]><td align=\"center\" width=\"500\" style=\"background-color: #ecf0f1;width: 500px;padding: 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\">   <![endif]-->                                                            <div class=\"u-col u-col-83p34\" style=\"max-width: 320px;min-width: 500.04px;display: table-cell;vertical-align: top;\">                                                                <div style=\"background-color: #ecf0f1;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                                                                    <!--[if (!mso)&(!IE)]><!-->                                                                    <div style=\"box-sizing: border-box; height: 100%; padding: 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                                                                        <!--<![endif]-->                                                                        <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                                                                            <tbody>                                                                                <tr>                                                                                    <td style=\"overflow-wrap:break-word;word-break:break-word;padding:15px;font-family:'Poppins', sans-serif;;\" align=\"left\">                                                                                        <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\">                                                                                            <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Event Policy</strong></span></p>                                                                                            <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[EventPolicy]]</span></p>                                                                                        </div>                                                                                    </td>                                                                                </tr>                                                                            </tbody>                                                                        </table>                                                                        <!--[if (!mso)&(!IE)]><!-->                                                                    </div>                                                                    <!--<![endif]-->                                                                </div>                                                            </div>                                                            <!--[if (mso)|(IE)]></td><![endif]-->                                                            <!--[if (mso)|(IE)]><td align=\"center\" width=\"49\" style=\"background-color: #ffffff;width: 49px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\">   <![endif]-->                                                            <div class=\"u-col u-col-8p33\" style=\"max-width: 320px;min-width: 49.98px;display: table-cell;vertical-align: top;\">                                                                <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                                                                    <!--[if (!mso)&(!IE)]><!-->                                                                    <div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\">                                                                        <!--<![endif]-->                                                                        <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">                                                                            <tbody>                                                                                <tr>                                                                                    <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\">                                                                                        <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">                                                                                            <tbody>                                                                                                <tr style=\"vertical-align: top\">                                                                                                    <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">            <span>&nbsp;</span>        </td>                                                                                                </tr>                                                                                            </tbody>                                                                                        </table>                                                                                    </td>                                                                                </tr>                                                                            </tbody>                                                                        </table>                                                                        <!--[if (!mso)&(!IE)]><!-->                                                                    </div>                                                                    <!--<![endif]-->                                                                </div>                                                            </div>                                                            <!--[if (mso)|(IE)]></td><![endif]-->                                                            <!--[if (mso)|(IE)]>            </tr>         </table>      </td>   </tr></table><![endif]-->                                                        </div>                                                    </div>                                                </div>";
                    string EventPolicycontent = order.ticket_event_policy;

                    if (!string.IsNullOrEmpty(EventPolicycontent))
                    {
                        EventPolicy = EventPolicyHtml;
                        EventPolicy = EventPolicy.Replace("[[EventPolicy]]", EventPolicycontent);
                    }

                    string OrganizerMsg = "";
                    string organizerMsgHtml = "<div class=\"u-row-container row-payment\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"49\" style=\"background-color: #ffffff;width: 49px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-8p33\" style=\"max-width: 320px;min-width: 49.98px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"500\" style=\"background-color: #ecf0f1;width: 500px;padding: 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-83p34\" style=\"max-width: 320px;min-width: 500.04px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ecf0f1;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:15px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>A message from [[EventOrganizer]]</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[BusinessMessage]]</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"49\" style=\"background-color: #ffffff;width: 49px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-8p33\" style=\"max-width: 320px;min-width: 49.98px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string businessMessage = order.business_message;

                    if (!string.IsNullOrEmpty(businessMessage))
                    {
                        OrganizerMsg = organizerMsgHtml;
                        OrganizerMsg = OrganizerMsg.Replace("[[BusinessMessage]]", businessMessage);
                        OrganizerMsg = OrganizerMsg.Replace("[[EventOrganizer]]", order.event_organizer_name);
                    }

                    string TicketHolderOrBuyer = string.Format("{0} {1}", order.bill_first_name, order.bill_last_name);

                    string totalTblHeading = "Total";

                    StringBuilder ticketTbl = new StringBuilder();

                    ticketTbl.Append("<table>");
                    ticketTbl.Append("<tr><th style=\"text-align:left;padding:0 5px 0 5px;\">Ticket Level</th><th style=\"text-align:right;padding:0 5px 0 5px;\">Qty</th><th style=\"text-align:right;padding:0 5px 0 5px;\">Delivery Type</th><th style=\"text-align:center;padding:0 5px 0 5px;\">Ticket Holder</th><th style=\"text-align:right;padding:0 5px 0 5px;\">" + totalTblHeading + "</th></tr>");

                    var tictorderticket = order.ticket_order_ticket;
                    //string ticketLevelName = "";
                    //string ticketHolderName = "";
                    //string postCaptureKey = "";
                    //string inviteToEmailAddress = "";
                    bool hasSelfPrintTickets = false;
                    bool hasPostCaptureSelfPrint = false;

                    foreach (var t in tictorderticket)
                    {
                        //if (t.id == TicketOrderTicketId)
                        //{
                        //    ticketLevelName = t.ticket_name;
                        //    ticketHolderName = String.Format("{0} {1}", t.first_name, t.last_name);
                        //    postCaptureKey = t.post_capture_key;
                        //    inviteToEmailAddress = t.email;
                        //    //break;
                        //}
                        string validDates = t.valid_start_date.Date.ToString("MM/dd/yy");

                        if (t.valid_start_date.Date != t.validend_date.Date)
                        {
                            validDates = string.Format("Valid: {0} - {1}", t.valid_start_date.Date.ToString("MM/dd/yy"), t.validend_date.ToString("MM/dd/yy"));
                        }

                        string DeliveryType = string.Empty;
                        if (t.delivery_type == TicketDelivery.SelfPrint)
                        {
                            hasSelfPrintTickets = true;
                            DeliveryType = GetDeliveryOptionFormatted(TicketDelivery.SelfPrint);
                        }
                        else if (t.delivery_type == TicketDelivery.WillCall)
                        {
                            hasSelfPrintTickets = true;
                            DeliveryType = "Will Call"; //GetDeliveryOptionFormatted(TicketDelivery.WillCall);
                            //DeliveryType = string.Format(DeliveryType.Replace("Will Call: ", ""), t.WillCallLocationName);
                            if (t.will_call_location_id > 0)
                            {
                                string wcLocation = ticketDAL.GetTicketWillCallLocationsByLocationId(t.will_call_location_id);
                                if (wcLocation != string.Empty && wcLocation != null)
                                {
                                    DeliveryType += "<br /><p>" + wcLocation + "</p>";
                                }
                            }
                        }
                        else if (t.delivery_type == TicketDelivery.Shipped)
                            DeliveryType = GetDeliveryOptionFormatted(TicketDelivery.Shipped, t.fulfillment_lead_time_desc);

                        if (t.post_capture_status == TicketPostCaptureStatus.Claimed && t.id == TicketOrderTicketId)
                        {
                            if (t.delivery_type == TicketDelivery.SelfPrint || t.delivery_type == TicketDelivery.WillCall)
                                hasPostCaptureSelfPrint = true;

                            TicketHolderOrBuyer = string.Format("{0} {1}", t.first_name, t.last_name);

                            ticketTbl.Append(string.Format("<tr><td style=\"text-align:left;padding:0 5px 0 5px;\">{0}<br /><small>{1}</small></td><td style=\"text-align:right;padding:0 5px 0 5px;\">{2}</td><td style=\"text-align:right;padding:0 5px 0 5px;\">{3}</td><td style=\"text-align:center;padding:0 5px 0 5px;\">{4}</td><td style=\"text-align:right;padding:0 5px 0 5px;\">{5}</td></tr>", t.ticket_name, validDates, t.ticket_qty, DeliveryType, string.Format("{0} {1}", t.first_name, t.last_name), string.Format(new CultureInfo("en-US"), "{0:C}", t.ticket_price)));
                            if (t.include_confirmation_message == true)
                                ticketTbl.Append(string.Format("<tr><td colspan='5'>{0}<br /><br /></td></tr>", t.confirmation_message));
                        }
                    }

                    ticketTbl.Append("</table>");



                    string eventDate = order.event_start_date_time.ToString("MM/dd/yy");

                    if (order.event_end_date_time.Date != order.event_start_date_time.Date)
                    {
                        eventDate = string.Format("{0} - {1}", order.event_start_date_time.ToString("MM/dd/yy"), order.event_end_date_time.ToString("MM/dd/yy"));
                    }

                    string OrderTotalMsg = string.Empty;
                    string orderNum = "Hidden";
                    string orderTotal = "Hidden";
                    string eventCancelPolicy = "No Refunds, No Transfers";

                    if (order.payment_type == PaymentType.CreditCard)
                    {
                        OrderTotalMsg = string.Format("You will see a charge in the amount of {0} from '{1}' on your next credit card statement.", string.Format(new CultureInfo("en-US"), "{0:C}", order.order_total), Utility.GenerateChargeDescription(order.member_name, order.dynamic_payment_desc, order.id.ToString(), Configuration.Gateway.Braintree));
                    }

                    if (order.refund_policy != TicketRefundPolicy.Oncasbycasebasis)
                    {
                        eventCancelPolicy = order.refund_policy_text;
                    }

                    RsvpGuestContent = RsvpGuestContent.Replace("[[OrderNumber]]", orderNum);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[PurchaseDate]]", order.order_date.ToString("MM/dd/yy"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[Tickets]]", ticketTbl.ToString());

                    string strpath = "https://typhoon.cellarpass.com/";
                    if (ConnectionString.IndexOf("live") > -1)
                        strpath = "https://www.cellarpass.com/";

                    RsvpGuestContent = RsvpGuestContent.Replace("[[ConfirmationURL]]", string.Format("{1}tix-confirmation?orderNumber={0}", order.order_guid, strpath));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[FollowURL]]", string.Format("{1}{0}fav=1", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id,order.event_url), strpath));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ContactOrganizer]]", string.Format("{1}{0}co=1", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id,order.event_url), strpath));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventDetailsURL]]", string.Format("{1}{0}?", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id,order.event_url), strpath));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[PrivacyPolicyURL]]", string.Format("{0}business/legal/cellarpass-privacy-policy", strpath));

                    RsvpGuestContent = RsvpGuestContent.Replace("[[OrderTotal]]", orderTotal);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[TotalPaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", order.paid_total));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[TicketHolder]]", TicketHolderOrBuyer);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[TicketBuyerEmail]]", order.bill_email_address);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[TicketBuyerPhone]]", StringHelpers.FormatTelephoneNumber(order.bill_home_phone.ToString(), order.bill_country)); //order.BillHomePhone);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", order.member_name);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[BusinessMessage]]", order.business_message);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[OrganizerMessage]]", OrganizerMsg);

                    RsvpGuestContent = RsvpGuestContent.Replace("[[TixEventPolicy]]", EventPolicy);

                    RsvpGuestContent = RsvpGuestContent.Replace("[[VenueName]]", "");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[VenueAddress]]", order.event_venue_address);

                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventName]]", order.event_title);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventOrganizer]]", order.event_organizer_name);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventOrganizerPhone]]", StringHelpers.FormatTelephoneNumber(order.event_organizer_phone.ToString(), order.bill_country));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventURL]]", Utility.GenerateEmailButton("View Event Details", string.Format("https://www.cellarpass.com/{0}", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id,order.event_url)), "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));

                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventDate]]", eventDate);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventStarts]]", order.event_start_date_time.ToString("hh:mm tt"));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventEnds]]", order.event_end_date_time.ToString("hh:mm tt"));

                    RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", StringHelpers.FormatTelephoneNumber(order.event_organizer_phone.ToString(), order.bill_country));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", order.event_organizer_email);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[DeliveryOption]]", "");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[OrderNotes]]", order.order_note);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventCancelPolicy]]", eventCancelPolicy);

                    RsvpGuestContent = RsvpGuestContent.Replace("[[PaymentMessage]]", OrderTotalMsg);

                    string postCaptureContent = ew.EmailBodyAdmin;

                    postCaptureContent = postCaptureContent.Replace("[[PaymentMessage]]", OrderTotalMsg);
                    postCaptureContent = postCaptureContent.Replace("[[OrderNumber]]", orderNum);
                    postCaptureContent = postCaptureContent.Replace("[[PurchaseDate]]", order.order_date.ToString("MM/dd/yy"));
                    postCaptureContent = postCaptureContent.Replace("[[Tickets]]", ticketTbl.ToString());

                    RsvpGuestContent = RsvpGuestContent.Replace("[[ConfirmationURL]]", string.Format("{1}tix-confirmation?orderNumber={0}", order.order_guid, strpath));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[FollowURL]]", string.Format("{1}{0}fav=1", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id,order.event_url), strpath));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ContactOrganizer]]", string.Format("{1}{0}co=1", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id,order.event_url), strpath));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventDetailsURL]]", string.Format("{1}{0}?", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id,order.event_url), strpath));
                    RsvpGuestContent = RsvpGuestContent.Replace("[[PrivacyPolicyURL]]", string.Format("{0}business/legal/cellarpass-privacy-policy", strpath));

                    postCaptureContent = postCaptureContent.Replace("[[OrderTotal]]", orderTotal);
                    postCaptureContent = postCaptureContent.Replace("[[TotalPaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", order.paid_total));
                    postCaptureContent = postCaptureContent.Replace("[[TicketHolder]]", TicketHolderOrBuyer);
                    postCaptureContent = postCaptureContent.Replace("[[TicketBuyerEmail]]", order.bill_email_address);
                    postCaptureContent = postCaptureContent.Replace("[[MemberName]]", order.member_name);
                    postCaptureContent = postCaptureContent.Replace("[[EventName]]", order.event_title);
                    postCaptureContent = postCaptureContent.Replace("[[VenueName]]", "");
                    postCaptureContent = postCaptureContent.Replace("[[VenueAddress]]", order.event_venue_address);
                    postCaptureContent = postCaptureContent.Replace("[[EventDate]]", eventDate);
                    postCaptureContent = postCaptureContent.Replace("[[EventStarts]]", order.event_start_date_time.ToString("hh:mm tt"));
                    postCaptureContent = postCaptureContent.Replace("[[EventEnds]]", order.event_end_date_time.ToString("hh:mm tt"));
                    postCaptureContent = postCaptureContent.Replace("[[EventOrganizer]]", order.event_organizer_name);
                    postCaptureContent = postCaptureContent.Replace("[[EventOrganizerPhone]]", StringHelpers.FormatTelephoneNumber(order.event_organizer_phone.ToString(), order.bill_country));
                    postCaptureContent = postCaptureContent.Replace("[[EventURL]]", Utility.GenerateEmailButton("View Event Details", string.Format("https://www.cellarpass.com/{0}", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id,order.event_url)), "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));

                    postCaptureContent = postCaptureContent.Replace("[[MemberPhone]]", StringHelpers.FormatTelephoneNumber(order.event_organizer_phone.ToString(), order.bill_country));
                    postCaptureContent = postCaptureContent.Replace("[[MemberEmail]]", order.event_organizer_email);

                    postCaptureContent = postCaptureContent.Replace("[[DeliveryOption]]", "");
                    postCaptureContent = postCaptureContent.Replace("[[OrderNotes]]", order.order_note);
                    postCaptureContent = postCaptureContent.Replace("[[PostCaptureMessage]]", "");
                    postCaptureContent = postCaptureContent.Replace("[[BusinessMessage]]", order.business_message);
                    postCaptureContent = postCaptureContent.Replace("[[OrganizerMessage]]", OrganizerMsg);
                    postCaptureContent = postCaptureContent.Replace("[[EventCancelPolicy]]", eventCancelPolicy);

                    postCaptureContent = postCaptureContent.Replace("[[TixEventPolicy]]", EventPolicy);
                    string bannerImageHtml = "";
                    bannerImageHtml = Utility.GetBannerImageHtmlForTicketEvent(order.event_image_big);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[EventBannerImage]]", bannerImageHtml);

                    string ButtonURL = "";

                    if (order.primary_category == TicketCategory.LiveBroadcast)
                    {
                        ButtonURL = Utility.GenerateEmailButton("View Broadcast", string.Format("https://www.cellarpass.com/{0}", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id,order.event_url)), "#dc3545", "#dc3545", "15px", "15px", "#ffffff");
                    }
                    else if (order.primary_category == TicketCategory.Webinar)
                    {
                        ButtonURL = Utility.GenerateEmailButton("Join Meeting", string.Format("https://www.cellarpass.com/{0}", Utility.GetFriendlyURL(order.event_title, order.tickets_event_id,order.event_url)), "#dc3545", "#dc3545", "15px", "15px", "#ffffff");
                    }

                    postCaptureContent = postCaptureContent.Replace("[[BroadcastURL]]", ButtonURL);


                    RsvpGuestContent = RsvpGuestContent.Replace("[[PostCaptureMessage]]", postCaptureContent);

                    //if (!string.IsNullOrWhiteSpace(order.itinerary_guid))
                    //    RsvpGuestContent = RsvpGuestContent.Replace("[[ViewItinerary]]", Utility.GenerateEmailButton("View Itinerary", string.Format("{1}itinerary/{0}?v=agenda", order.itinerary_guid, strpath), "#dc3545", "#dc3545", "15px", "15px", "#ffffff"));
                    //else
                    //    RsvpGuestContent = RsvpGuestContent.Replace("[[ViewItinerary]]", "");

                    string viewItineraryHtml = "";

                    if (!string.IsNullOrWhiteSpace(order.itinerary_guid))
                    {
                        viewItineraryHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[ViewItineraryURL]]\" style=\"height:47px; v-text-anchor:middle; width:516px;\" arcsize=\"8.5%\" strokecolor=\"#e03e2d\" strokeweight=\"1px\" fillcolor=\"#ffffff\"><w:anchorlock/><center style=\"color:#e03e2d;font-family:'Poppins', sans-serif;\"><![endif]--> <a href=\"[[ViewItineraryURL]]\" target=\"_blank\" class=\"v-button\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #e03e2d; background-color: #ffffff; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:96%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;border-top-width: 1px; border-top-style: solid; border-top-color: #e03e2d; border-left-width: 1px; border-left-style: solid; border-left-color: #e03e2d; border-right-width: 1px; border-right-style: solid; border-right-color: #e03e2d; border-bottom-width: 1px; border-bottom-style: solid; border-bottom-color: #e03e2d;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"font-size: 14px; line-height: 16.8px; font-family: Poppins, sans-serif;\">VIEW ITINERARY</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                        viewItineraryHtml = viewItineraryHtml.Replace("[[ViewItineraryURL]]", string.Format("{1}itinerary/{0}?v=agenda", order.itinerary_guid, strpath));
                    }

                    RsvpGuestContent = RsvpGuestContent.Replace("[[ViewItinerary]]", viewItineraryHtml);

                    string AdditionalTicketInfo = "";
                    StringBuilder TicketLevelMessages = new StringBuilder();
                    string additionalInfoHeading = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 25px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 25px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\"><strong>Additional Ticket Information</strong></span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string ticketlevelMsg = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 120%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\"><strong>{{TicketLevelName}}</strong></span></p> <p style=\"font-size: 14px; line-height: 120%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 16.8px;\">{{TicketLevelMessage}}</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

                    foreach (var item in ticketDAL.GetAdditionalTicketInfoByID(order.id))
                    {
                        string example1 = ticketlevelMsg;
                        example1 = example1.Replace("{{TicketLevelName}}", item.TicketName);
                        example1 = example1.Replace("{{TicketLevelMessage}}", item.ConfirmationMessage);
                        TicketLevelMessages.Append(example1);
                    }

                    if (TicketLevelMessages.ToString().Length > 0)
                        AdditionalTicketInfo = additionalInfoHeading + TicketLevelMessages.ToString();

                    RsvpGuestContent = RsvpGuestContent.Replace("[[AdditionalTicketInformation]]", AdditionalTicketInfo);

                    string MapURL = "";

                    if (order.geo_latitude != "" && order.geo_longitude != "")
                    {
                        string googleAPIKey = "AIzaSyDKGd8SyV0GhpzD-onvM0ItLpzq5GhCFfA";
                        if (_appSetting != null && _appSetting.Value != null && !string.IsNullOrEmpty(_appSetting.Value.GoogleAPIKey))
                        {
                            googleAPIKey = _appSetting.Value.GoogleAPIKey;
                        }

                        string googleurl = "https://www.google.com/maps/place/" + HttpUtility.UrlEncode(order.venue_full_address);

                        MapURL = await Utility.GetMapImageHtmlByLocation(googleAPIKey, order.geo_latitude, order.geo_longitude, order.tickets_event_id, googleurl);
                    }

                    RsvpGuestContent = RsvpGuestContent.Replace("[[MapURL]]", MapURL);

                    if (order.bill_email_address.IndexOf("@noemail") == -1 || model.AdminEmail)
                    {
                        string EmailTo = order.bill_email_address;
                        if (model.AdminEmail)
                        {
                            EmailTo = "";
                            if (ew.EmailTo != null)
                            {
                                if (!string.IsNullOrEmpty(ew.EmailTo))
                                {
                                    EmailTo = ew.EmailTo;
                                }
                            }
                            SubjectContent = ew.EmailSubjectAdmin;

                            SubjectContent = SubjectContent.Replace("[[EventOrganizer]]", order.member_name);
                            SubjectContent = SubjectContent.Replace("[[EventName]]", order.event_title);
                        }
                        else if (!string.IsNullOrEmpty(AlternativeEmail))
                        {
                            EmailTo = AlternativeEmail;
                        }

                        if (!string.IsNullOrEmpty(EmailTo))
                        {
                            if (hasSelfPrintTickets || hasPostCaptureSelfPrint)
                            {
                                EmailAttachment attachment = new EmailAttachment();
                                attachment.Name = string.Format("Ticket_{0}.pdf", order.id);
                                attachment.ContentBytes = GenerateOrderPDF(order.order_guid);
                                attachment.Contents = "";
                                response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.RsvpTicketSalesConfirmation, order.id, ew.EmailFrom, EmailTo, SubjectContent, RsvpGuestContent, 0, attachment, ew.Id));
                            }
                            else
                                response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.RsvpTicketSalesConfirmation, order.id, ew.EmailFrom, EmailTo, SubjectContent, RsvpGuestContent, 0, null, ew.Id));
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "Email", "ProcessSendCPTicketPostCaptureEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }

        public async Task<EmailResponse> ProcessSendExportReservations(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(ConnectionString);
                int WineryId = model.RsvpId;
                
                int uId = model.UId;
                string ipaddress = model.CCGuestEmail;
                int exportType = model.ActionSource;
                string wineryname = model.perMsg;

                UserDAL userDAL = new UserDAL(ConnectionString);
                var userDetailModel = new UserDetailModel();
                userDetailModel=userDAL.GetUserById(uId);

                string EmailTo = userDetailModel.email;
                string username = userDetailModel.first_name + " " + userDetailModel.last_name;

                string RsvpGuestContent = "";
                string SubjectContent = "";

                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew = new EmailContent();

                ew = emailDAL.GetEmailContent((int)EmailTemplates.RsvpExport, 0);

                SubjectContent = ew.EmailSubject;
                RsvpGuestContent = ew.EmailBody;

                RsvpGuestContent = RsvpGuestContent.Replace("[[UserEmail]]", EmailTo);
                RsvpGuestContent = RsvpGuestContent.Replace("[[UserName]]", username);
                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", wineryname);
                RsvpGuestContent = RsvpGuestContent.Replace("[[DateTime]]", string.Format("{0} PST", Times.ToTimeZoneTime(DateTime.UtcNow).ToString("MM/dd/yyyy hh:mm tt")));

                string url = string.Empty;

                if (exportType == 1)
                {
                    DateTime StartDate = Convert.ToDateTime(model.BCode);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ExportType]]", "Detailed Export");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[DateSelected]]", StartDate.ToString("MM/dd/yyyy"));
                    url = Utility.ReservationDetailedExport(WineryId, StartDate);
                }
                else if (exportType == 2)
                {
                    DateTime StartDate = Convert.ToDateTime(model.BCode);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ExportType]]", "Simplified Export");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[DateSelected]]", StartDate.ToString("MM/dd/yyyy"));
                    url = Utility.ReservationSimplifiedExport(WineryId, StartDate);
                }
                else if (exportType == 3)
                {
                    DateTime StartDate = Convert.ToDateTime(model.BCode);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ExportType]]", "By Location");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[DateSelected]]", StartDate.ToString("MM/dd/yyyy"));
                    url = Utility.ReservationByLocationExport(WineryId, StartDate);
                }
                else if (exportType == 4)
                {
                    DateTime StartDate = Convert.ToDateTime(model.BCode);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ExportType]]", "Credit Card Transactions Report");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[DateSelected]]", StartDate.ToString("MM/dd/yyyy"));
                    url = Utility.TransactionReportByMonth(WineryId, StartDate);
                }
                else if (exportType == 5)
                {
                    DateTime StartDate = Convert.ToDateTime(model.BCode);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ExportType]]", "Credit Card Sales Detail Report");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[DateSelected]]", StartDate.ToString("MM/dd/yyyy"));
                    url = Utility.FinancialV2ReportByMonth(WineryId, StartDate);
                }
                else if (exportType == 6)
                {
                    int EventId = Convert.ToInt32(model.BCode);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ExportType]]", "Export Ticketholders");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[DateSelected]]", DateTime.Now.ToString("MM/dd/yyyy"));
                    url = Utility.TicketHoldersExport(WineryId, EventId,false);
                }
                else if (exportType == 7)
                {
                    int EventId = Convert.ToInt32(model.BCode);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ExportType]]", "Export Ticketholders");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[DateSelected]]", DateTime.Now.ToString("MM/dd/yyyy"));
                    //url = Utility.PassportTicketholdersExport(WineryId, EventId);
                    url = Utility.TicketHoldersExport(WineryId, EventId, true);
                }
                else if (exportType == 8)
                {
                    int EventId = Convert.ToInt32(model.BCode);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ExportType]]", "Export Attendees");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[DateSelected]]", DateTime.Now.ToString("MM/dd/yyyy"));
                    url = Utility.CheckInAttendeesExport(WineryId, EventId, false);
                }
                else if (exportType == 9)
                {
                    int EventId = Convert.ToInt32(model.BCode);
                    RsvpGuestContent = RsvpGuestContent.Replace("[[ExportType]]", "Export Attendees");
                    RsvpGuestContent = RsvpGuestContent.Replace("[[DateSelected]]", DateTime.Now.ToString("MM/dd/yyyy"));
                    url = Utility.CheckInAttendeesExport(WineryId, EventId, true);
                }
                //1 = Detailed Export
                //2 = Simplified Export
                //3 = By Location
                //4 = Credit Card Transactions Report
                //5 = Credit Card Sales Detail Report
                //6 = Export Ticketholders
                //7 = Export Passport Ticketholders
                //8 = Export Attendees
                //9 = Export Passport Attendees

                RsvpGuestContent = RsvpGuestContent.Replace("[[DownloadFile]]", url);
                RsvpGuestContent = RsvpGuestContent.Replace("[[IPAddress]]", ipaddress);

                response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.RsvpExport, WineryId, ew.EmailFrom, EmailTo, SubjectContent, RsvpGuestContent, 0, null, ew.Id));

            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "Email", "ProcessSendCPTicketPostCaptureEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }

        public async Task<EmailResponse> ProcessSendTicketedEventEndedNotification(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(ConnectionString);
                int WineryId = model.RsvpId;
                int event_id = Convert.ToInt32(model.BCode);
                int uId = model.UId;
                string AlternativeEmail = model.CCGuestEmail;
                string wineryname = model.perMsg;

                UserDAL userDAL = new UserDAL(ConnectionString);
                var userDetailModel = new UserDetailModel();
                userDetailModel = userDAL.GetUserById(uId);

                string EmailTo = userDetailModel.email;

                string username = userDetailModel.first_name + " " + userDetailModel.last_name;

                string RsvpGuestContent = "";
                string SubjectContent = "";

                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew = new EmailContent();

                ew = emailDAL.GetEmailContent((int)EmailTemplates.TicketedEventEndedNotification, 0);

                SubjectContent = ew.EmailSubject;
                RsvpGuestContent = ew.EmailBody;

                string eventPassword = string.Empty;
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                TicketEventDetailModel ticketEvent = ticketDAL.GetTicketEventDetailsById(event_id, ref eventPassword);

                string lFullAddress = string.Empty;
                lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", ticketEvent.venue_address_1, string.IsNullOrEmpty(ticketEvent.venue_address_2) ? "" : ticketEvent.venue_address_2 + "<br>", ticketEvent.venue_city, ticketEvent.venue_state, ticketEvent.venue_zip);

                SubjectContent = SubjectContent.Replace("[[EventName]]", ticketEvent.name);
                SubjectContent = SubjectContent.Replace("[[VenueName]]", "");
                SubjectContent = SubjectContent.Replace("[[VenueAddress]]", lFullAddress);
                SubjectContent = SubjectContent.Replace("[[EventDate]]", ticketEvent.start_date.ToShortDateString());
                SubjectContent = SubjectContent.Replace("[[EventDayOfWeek]]", ticketEvent.start_date.DayOfWeek.ToString());
                SubjectContent = SubjectContent.Replace("[[EventStarts]]", ticketEvent.start_date.ToString("hh:mm tt"));
                SubjectContent = SubjectContent.Replace("[[EventEnds]]", ticketEvent.end_date.ToString("hh:mm tt"));
                SubjectContent = SubjectContent.Replace("[[EventOrganizer]]", ticketEvent.event_organizer_name);
                SubjectContent = SubjectContent.Replace("[[EventOrganizerPhone]]", "");
                SubjectContent = SubjectContent.Replace("[[EventOrganizerEmail]]", ticketEvent.event_organizer_email);
                SubjectContent = SubjectContent.Replace("[[EventURL]]", "");

                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", ticketEvent.event_organizer_name);
                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", StringHelpers.FormatTelephoneNumber(ticketEvent.event_organizer_phone.ToString(), ticketEvent.venue_country));
                RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", ticketEvent.event_organizer_email);
                RsvpGuestContent = RsvpGuestContent.Replace("[[EventName]]", ticketEvent.name);
                RsvpGuestContent = RsvpGuestContent.Replace("[[VenueName]]", "");
                RsvpGuestContent = RsvpGuestContent.Replace("[[VenueAddress]]", lFullAddress);
                RsvpGuestContent = RsvpGuestContent.Replace("[[VenueMap]]", "");
                RsvpGuestContent = RsvpGuestContent.Replace("[[EventDate]]", ticketEvent.start_date.ToShortDateString());
                RsvpGuestContent = RsvpGuestContent.Replace("[[EventDayOfWeek]]", ticketEvent.start_date.DayOfWeek.ToString());
                RsvpGuestContent = RsvpGuestContent.Replace("[[EventStarts]]", ticketEvent.start_date.ToString("hh:mm tt"));
                RsvpGuestContent = RsvpGuestContent.Replace("[[EventEnds]]", ticketEvent.end_date.ToString("hh:mm tt"));
                RsvpGuestContent = RsvpGuestContent.Replace("[[EventOrganizer]]", ticketEvent.event_organizer_name);
                RsvpGuestContent = RsvpGuestContent.Replace("[[EventOrganizerPhone]]", StringHelpers.FormatTelephoneNumber(ticketEvent.event_organizer_phone.ToString(), ticketEvent.venue_country));
                RsvpGuestContent = RsvpGuestContent.Replace("[[EventOrganizerEmail]]", ticketEvent.event_organizer_email);
                RsvpGuestContent = RsvpGuestContent.Replace("[[EventURLPath]]", String.Format("https://www.cellarpass.com/{0}", ticketEvent.event_url));
                RsvpGuestContent = RsvpGuestContent.Replace("[[EventURL]]", Utility.GenerateEmailButton("View Event Details", String.Format("https://www.cellarpass.com/{0}", ticketEvent.event_url), "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));
                //string imageURL = "";
                //if (string.IsNullOrEmpty(ticketEvent.EventImageBig)) {
                //    imageURL = ImageManager.GetImagePath(ImageManager.ImageType.ticketEventImage, ImageManager.ImagePathType.azure) & "/" & ticketEvent.EventImageBig & "?clearcache=" & Date.UtcNow.Millisecond.ToString;
                //}
                 
                //RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[EventBannerImage]]", imageURL);
                //RsvpGuestContent = RsvpGuestContent.Replace("https://admin.cellarpass.com/admin/[[EventBannerImage]]", imageURL);
                //RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[EventBannerImage]]", imageURL);

                string bannerImageHtml = "";
                bannerImageHtml = Utility.GetBannerImageForTicketEvent(ticketEvent.event_image_big);

                RsvpGuestContent = RsvpGuestContent.Replace("[[EventBannerImage]]", bannerImageHtml);
                RsvpGuestContent = RsvpGuestContent.Replace("[[BusinessMessage]]", "BUSINESS MESSAGE CONTENT HERE");

                if (!string.IsNullOrWhiteSpace(AlternativeEmail))
                    EmailTo += ", " + AlternativeEmail;

                if (!string.IsNullOrWhiteSpace(ew.EmailTo))
                    EmailTo += ", " + ew.EmailTo;

                response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.TicketedEventEndedNotification, WineryId, ew.EmailFrom, EmailTo, SubjectContent, RsvpGuestContent, 0, null, ew.Id));

            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "Email", "ProcessSendCPTicketPostCaptureEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }

        public async Task<EmailResponse> ProcessReservationChangesUpdate(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(ConnectionString);
                int event_id = model.RsvpId;

                EventReservationChangesModel m = eventDAL.GetEventDetailForReservationChanges(event_id);
                
                decimal ReservationChanges = await GetReservationChangesByEventId(m);

                m.item_id = m.min_item_id;
                m.group_id = m.min_group_id;
                m.price = m.min_price;
                m.Taxable = m.min_Taxable;
                m.calculate_gratuity = m.min_calculate_gratuity;

                decimal MinReservationChanges = await GetReservationChangesByEventId(m);

                m.item_id = m.max_item_id;
                m.group_id = m.max_group_id;
                m.price = m.max_price;
                m.Taxable = m.max_Taxable;
                m.calculate_gratuity = m.max_calculate_gratuity;

                decimal MaxReservationChanges = await GetReservationChangesByEventId(m);

                //eventDAL.UpdateReservationChangesByEventId(event_id, Convert.ToDecimal(val));
                eventDAL.UpdateReservationChangesByEventId(event_id, ReservationChanges, MinReservationChanges, MaxReservationChanges);

            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "Email", "ProcessSendCPTicketPostCaptureEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }

        public async Task<decimal> GetReservationChangesByEventId(EventReservationChangesModel m)
        {
            EventDAL eventDAL = new EventDAL(ConnectionString);

            decimal EventFee = 0;
            decimal gratuityTotal = 0;

            decimal addOnTotal = 0;
            decimal taxAmount = 0;
            decimal rsvpTaxTotal = 0;
            //decimal ServiceFees = 0;
            decimal SalesTaxpercentage = 0;
            decimal addOnTaxableTotal = 0;
            decimal taxableGratuityAmt = 0;

            EventFee = m.FeePerPerson;

            if (m.Taxable)
                addOnTaxableTotal = m.price;

            addOnTotal = m.price;

            if ((m.GratuityPercentage > 0) && (m.FeePerPerson + addOnTotal) > 0)
            {
                if (m.calculate_gratuity && m.GratuityPercentage > 0)
                {
                    decimal itemGratuity = Utility.CalculateGratuity(m.price, m.GratuityPercentage);
                    gratuityTotal += itemGratuity;

                    if (itemGratuity > 0 && m.TaxGratuity)
                    {
                        taxableGratuityAmt += itemGratuity;
                    }
                }
            }

            if (m.GratuityPercentage > 0)
            {
                gratuityTotal += Utility.CalculateGratuity(m.FeePerPerson, m.GratuityPercentage);

                if (m.TaxGratuity)
                    taxableGratuityAmt += Utility.CalculateGratuity(m.FeePerPerson, m.GratuityPercentage);
            }

            rsvpTaxTotal = addOnTaxableTotal + taxableGratuityAmt;

            decimal OrderTotalWithoutTax = m.FeePerPerson + gratuityTotal + addOnTotal;

            decimal svcFee = eventDAL.GetServiceFeePaidByGuest(m.MemberID, 1, OrderTotalWithoutTax);

            Utility objUtility = new Utility();
            SalesTaxpercentage = await objUtility.GetTaxByEventId(m.MemberID, 100, m.Zip, m.Address1, m.city, m.state);

            if (m.ChargeSalesTax)
                rsvpTaxTotal = rsvpTaxTotal + m.FeePerPerson;

            if (rsvpTaxTotal > 0)
                taxAmount = (rsvpTaxTotal * SalesTaxpercentage) / 100;

            string val = String.Format("{0:F2}", EventFee + addOnTotal + gratuityTotal + taxAmount + svcFee);
            return Convert.ToDecimal(val);
        }

        public async Task<EmailResponse> ProcessEventReviewInviteEmail(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(ConnectionString);

                List<EventTicketHolder> list = ticketDAL.GetEventTicketHoldersForReview(48, DateTime.Now, model.RsvpId);

                string ReviewInviteSubject = "";
                string ReviewInviteContent = "";

                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew;
                ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.EventReviewInvitation, 0);
                if (ew != null && ew.Active == true)
                {
                    foreach (var h in list)
                    {
                        ReviewInviteSubject = ew.EmailSubject;
                        ReviewInviteContent = ew.EmailBody;

                        if (h.TicketHolderEmail.IndexOf("@noemail") == -1 && !string.IsNullOrEmpty(h.TicketHolderEmail))
                        {
                            //string qsEncrypted = string.Empty;

                            //qsEncrypted = StringHelpers.Encryption(h.Id.ToString());
                            //qsEncrypted = qsEncrypted.Replace("+", "-").Replace("/", ".");

                            //qsEncrypted = HttpUtility.UrlPathEncode(qsEncrypted);

                            string ReviewLink = "https://typhoon.cellarpass.com/ticket-event-review" + string.Format("?q={0}", h.TicketsOrderTicketGUID);
                            if (ConnectionString.IndexOf("live") > -1)
                                ReviewLink = "https://www.cellarpass.com/ticket-event-review" + string.Format("?q={0}", h.TicketsOrderTicketGUID);

                            //string ReviewLink = "https://www.cellarpass.com/ticket-event-review" + string.Format("?q={0}", h.TicketsOrderTicketGUID);

                            ReviewInviteSubject = ReviewInviteSubject.Replace("[[GuestFirstName]]", h.TicketHolderFirstName);
                            ReviewInviteSubject = ReviewInviteSubject.Replace("[[GuestLastName]]", h.TicketHolderLastName);
                            ReviewInviteSubject = ReviewInviteSubject.Replace("[[GuestEmail]]", h.TicketHolderEmail);
                            ReviewInviteSubject = ReviewInviteSubject.Replace("[[EventName]]", h.EventTitle);
                            ReviewInviteSubject = ReviewInviteSubject.Replace("[[VenueName]]", h.EventVenueName);
                            ReviewInviteSubject = ReviewInviteSubject.Replace("[[EventDate]]", h.ValidStartDate.ToLongDateString());
                            ReviewInviteSubject = ReviewInviteSubject.Replace("[[CheckInDate]]", h.ClaimDate.ToLongDateString());
                            ReviewInviteSubject = ReviewInviteSubject.Replace("[[ReviewLink]]", "");

                            ReviewInviteContent = ReviewInviteContent.Replace("[[GuestFirstName]]", h.TicketHolderFirstName);
                            ReviewInviteContent = ReviewInviteContent.Replace("[[GuestLastName]]", h.TicketHolderLastName);
                            ReviewInviteContent = ReviewInviteContent.Replace("[[GuestEmail]]", h.TicketHolderEmail);
                            ReviewInviteContent = ReviewInviteContent.Replace("[[EventName]]", h.EventTitle);
                            ReviewInviteContent = ReviewInviteContent.Replace("[[VenueName]]", h.EventVenueName);
                            ReviewInviteContent = ReviewInviteContent.Replace("[[EventDate]]", h.ValidStartDate.ToLongDateString());
                            ReviewInviteContent = ReviewInviteContent.Replace("[[CheckInDate]]", h.ClaimDate.ToLongDateString());
                            ReviewInviteContent = ReviewInviteContent.Replace("[[ReviewLink]]", Utility.GenerateEmailButton("Write Review", ReviewLink, "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));

                            string EmailTo = h.TicketHolderEmail;
                            if (model.RsvpId > 0 && !string.IsNullOrEmpty(model.CCGuestEmail))
                            {
                                EmailTo = model.CCGuestEmail;
                            }

                            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.EventReviewInvitation, h.Id, ew.EmailFrom, EmailTo, ReviewInviteSubject, ReviewInviteContent, 0, null, ew.Id));

                            ticketDAL.UpdateReviewInviteSentForEvent(h.Id);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
            }
            return response;
        }

        public async Task<EmailResponse> SendCpTicketWaitlistOfferEmail(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(ConnectionString);
                EventDAL eventDAL = new EventDAL(ConnectionString);
                int OrderId = model.RsvpId;

                var tEvent = ticketDAL.GetTicketsEventByWaitlistId(OrderId);


                string WaitlistOfferContent = "";
                string SubjectContent = "";

                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew;
                ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.TicketWaitlistOffer, 0);
                if (ew != null && ew.Active == true)
                {
                    SubjectContent = ew.EmailSubject;
                    WaitlistOfferContent = ew.EmailBody;

                    string eventDate = tEvent.start_date_time.ToString("MM/dd/yyyy");

                    if (tEvent.end_date_time.Date != tEvent.start_date_time.Date)
                    {
                        eventDate = string.Format("{0} - {1}", tEvent.start_date_time.ToString("MM/dd/yyyy"), tEvent.end_date_time.ToString("MM/dd/yyyy"));
                    }

                    SubjectContent = SubjectContent.Replace("[[EventOrganizer]]", tEvent.organizer_name);
                    SubjectContent = SubjectContent.Replace("[[EventName]]", tEvent.organizer_name);

                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventName]]", tEvent.event_title);
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[VenueName]]", "");
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventDate]]", eventDate);
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventStarts]]", tEvent.start_date_time.ToString("hh:mm tt"));
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventEnds]]", tEvent.end_date_time.ToString("hh:mm tt"));
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventOrganizer]]", tEvent.organizer_name);

                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventOrganizerPhone]]", StringHelpers.FormatTelephoneNumber(tEvent.home_phone.ToString(), "US"));
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[EventURL]]", string.Format("https://www.cellarpass.com/{0}?wl={1}", Utility.GetFriendlyURL(tEvent.event_title, tEvent.event_id, tEvent.event_url), tEvent.waitlist_guid));
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[TicketLevel]]", tEvent.ticket_name);
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[TicketPrice]]", string.Format(new CultureInfo("en-US"), "{0:C}", tEvent.price));
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[TicketQtyRequested]]", tEvent.qty_desired.ToString());
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[TicketQtyOffered]]", tEvent.qty_offered.ToString());
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[TicketHolder]]", string.Format("{0} {1}", tEvent.first_name, tEvent.last_name));
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[TicketHolderEmail]]", tEvent.email);
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[WaitlistTimeout]]", tEvent.waitlist_expiration.ToString() + " Hours");
                    WaitlistOfferContent = WaitlistOfferContent.Replace("[[WaitListNotes]]", tEvent.note);

                    await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.TicketWaitlistOffer, OrderId, ew.EmailFrom, tEvent.email, SubjectContent, WaitlistOfferContent, tEvent.member_id, null, ew.Id));
                }
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "AbandonedCartEmail", "ProcessAbandonedCartTicketsEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }

        public async Task<EmailResponse> SendContactEventOrganizerEmail(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                int Id = model.RsvpId;

                if (Id == 0 && model.data != null)
                {
                    Id = model.data.RsvpId;
                }

                var tEvent = emailDAL.GetTempQueueDataByID(Id);

                emailDAL.DeleteTempQueueData(Id);

                string BodyContent = "";
                string SubjectContent = "";


                EmailContent ew;
                ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.SysContactEventOrganizer, 0);
                if (ew != null && ew.Active == true)
                {
                    SubjectContent = ew.EmailSubject;
                    BodyContent = ew.EmailBody;

                    SubjectContent = SubjectContent.Replace("[[EventName]]", tEvent.EventTitle);
                    SubjectContent = SubjectContent.Replace("[[EventStartDate]]", tEvent.StartDateTime.ToShortDateString());
                    SubjectContent = SubjectContent.Replace("[[EventOrganizer]]", tEvent.EventOrganizerName);
                    SubjectContent = SubjectContent.Replace("[[EventOrganizerPhone]]", StringHelpers.FormatTelephoneNumber(tEvent.EventOrganizerPhone, "US"));
                    SubjectContent = SubjectContent.Replace("[[GuestName]]", tEvent.GuestName);
                    SubjectContent = SubjectContent.Replace("[[GuestEmailAddress]]", tEvent.GuestEmailAddress);
                    SubjectContent = SubjectContent.Replace("[[ContactReason]]", tEvent.ContactReason);
                    SubjectContent = SubjectContent.Replace("[[ContactMessage]]", tEvent.ContactMessage);

                    BodyContent = BodyContent.Replace("[[EventName]]", tEvent.EventTitle);
                    BodyContent = BodyContent.Replace("[[EventStartDate]]", tEvent.StartDateTime.ToShortDateString());
                    BodyContent = BodyContent.Replace("[[EventOrganizer]]", tEvent.EventOrganizerName);
                    BodyContent = BodyContent.Replace("[[EventOrganizerPhone]]", StringHelpers.FormatTelephoneNumber(tEvent.EventOrganizerPhone, "US"));
                    BodyContent = BodyContent.Replace("[[GuestName]]", tEvent.GuestName);
                    BodyContent = BodyContent.Replace("[[GuestEmailAddress]]", tEvent.GuestEmailAddress);
                    BodyContent = BodyContent.Replace("[[ContactReason]]", tEvent.ContactReason);
                    BodyContent = BodyContent.Replace("[[ContactMessage]]", tEvent.ContactMessage);


                    await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.SysContactEventOrganizer, 0, ew.EmailFrom, tEvent.EventOrganizerEmail, SubjectContent, BodyContent, tEvent.member_id, null, ew.Id));
                }
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "AbandonedCartEmail", "ProcessAbandonedCartTicketsEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }

        public async Task<EmailResponse> SendPasswordResetCodeEmail(string resetCode, UserDetailModel user, UserType userType, MailConfig mailConfig)
        {
            var response = new EmailResponse();
            try
            {
                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);

                string EmailBody = "";
                string SubjectContent = "";

                EmailContent ew;
                ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.ForgotPasswordGuestSys, 0);
                if (ew != null && ew.Active == true)
                {
                    SubjectContent = ew.EmailSubject;
                    EmailBody = ew.EmailBody;

                    string LoginLink = string.Format("https://www.cellarpass.com/auth-forgot-password?code={0}", resetCode);

                    EmailBody = EmailBody.Replace("[[GuestName]]", user.first_name + " " + user.last_name);
                    EmailBody = EmailBody.Replace("[[GuestEmail]]", user.email);
                    EmailBody = EmailBody.Replace("[[UserName]]", user.first_name + " " + user.last_name);
                    EmailBody = EmailBody.Replace("[[UserEmail]]", user.email);
                    EmailBody = EmailBody.Replace("[[ResetDateTime]]", string.Format("{0} PST", Times.ToTimeZoneTime(DateTime.UtcNow).ToString("MM/dd/yyyy hh:mm tt")));
                    EmailBody = EmailBody.Replace("[[NewPassword]]", "");
                    EmailBody = EmailBody.Replace("[[LoginLink]]", Utility.GenerateEmailButton("Reset Password", LoginLink, "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));
                    EmailBody = EmailBody.Replace("[[LoginLinkTest]]", Utility.GenerateEmailButton("Reset Password", LoginLink.Replace("www.", "dev."), "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));


                    await Task.Run(() => SendEmailAndSaveEmailLog(mailConfig, Email.EmailType.NA, 0, ew.EmailFrom, user.email, ew.EmailSubject, EmailBody, 0, null, ew.Id));
                }
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "AbandonedCartEmail", "ProcessAbandonedCartTicketsEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }

        public async Task<EmailResponse> ProcessSendEventFollowingReminder(ReservationEmailModel model)
        {
            int member_id = 0;
            var response = new EmailResponse();
            LogDAL logDAL = new LogDAL(ConnectionString);
            try
            {
                TicketDAL ticketDAL = new TicketDAL(ConnectionString);
                EventDAL eventDAL = new EventDAL(ConnectionString);
                int eventId = model.RsvpId;
                if (eventId == 0 && model.data != null)
                {
                    eventId = model.data.RsvpId;
                }
                int queueId = model.UId;
                if (queueId == 0 && model.data != null)
                {
                    queueId = model.data.UId;
                }
                string refPasswd = "";
                var eventDetails = ticketDAL.GetTicketEventDetailsById(eventId, ref refPasswd);
                member_id = eventDetails.member_id;
                string RsvpGuestContent = "";
                string SubjectContent = "";


                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew;
                ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.TicketEventFollowingReminder, 0);
                if (ew != null && ew.Active == true)
                {

                    var usersList = ticketDAL.GetFollowingUsersForEventNotification(eventId);
                    string currentYear = DateTime.UtcNow.Year.ToString();
                    string copyrightMsg = string.Format("&copy;2009 - {0} CellarPass, Inc. All Rights Reserved", currentYear);
                    string errMsg = "";

                    if (usersList != null && usersList.Count > 0)
                    {
                        foreach (TicketEventUserNotificationMOdel user in usersList)
                        {
                            SubjectContent = ew.EmailSubject;
                            RsvpGuestContent = ew.EmailBody;

                            string eventDt = eventDetails.start_date.ToString("MM/dd/yyyy");
                            string eventDateMonthDay = eventDetails.start_date.ToString("MMMM dd");
                            string venueLocation = eventDetails.venue_address_1;

                            if (!string.IsNullOrWhiteSpace(eventDetails.venue_address_2))
                                venueLocation += ", " + eventDetails.venue_address_2;
                            if (!string.IsNullOrWhiteSpace(eventDetails.venue_city))
                                venueLocation += ", " + eventDetails.venue_city;
                            if (!string.IsNullOrWhiteSpace(eventDetails.venue_state))
                                venueLocation += ", " + eventDetails.venue_state;
                            if (!string.IsNullOrWhiteSpace(eventDetails.venue_zip))
                                venueLocation += " " + eventDetails.venue_zip;

                            SubjectContent = SubjectContent.Replace("[[UserFirstName]]", user.first_name.Trim());
                            SubjectContent = SubjectContent.Replace("[[UserEmail]]", user.email);
                            SubjectContent = SubjectContent.Replace("[[EventName]]", eventDetails.name);
                            SubjectContent = SubjectContent.Replace("[[EventOrganizer]]", eventDetails.event_organizer_name);
                            SubjectContent = SubjectContent.Replace("[[EventOrganizerEmail]]", eventDetails.event_organizer_email);
                            SubjectContent = SubjectContent.Replace("[[EventDate]]", eventDt);
                            SubjectContent = SubjectContent.Replace("[[EventDateMonthAndDay]]", eventDateMonthDay);
                            SubjectContent = SubjectContent.Replace("[[Location]]", venueLocation);
                            SubjectContent = SubjectContent.Replace("[[EventURL]]", "");
                            SubjectContent = SubjectContent.Replace("[[ProfileURL]]", "");
                            SubjectContent = SubjectContent.Replace("[[Copyright]]", copyrightMsg);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[UserFirstName]]", user.first_name.Trim());
                            RsvpGuestContent = RsvpGuestContent.Replace("[[UserEmail]]", user.email);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[EventName]]", eventDetails.name);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[EventOrganizer]]", eventDetails.event_organizer_name);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[EventOrganizerEmail]]", eventDetails.event_organizer_email);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[EventDate]]", eventDt);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[EventDateMonthAndDay]]", eventDateMonthDay);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Location]]", venueLocation);
                            string eventURL = String.Format("https://www.cellarpass.com/events/{0}", eventDetails.event_url);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[EventURL]]", eventURL);

                            string profileURL = String.Format("https://www.cellarpass.com/business/{0}", eventDetails.member_url);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[ProfileURL]]", profileURL);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Copyright]]", copyrightMsg);

                            string bannerImageHtml = "";
                            bannerImageHtml = Utility.GetBannerImageForTicketEvent(eventDetails.event_image_big);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[EventBanner]]", bannerImageHtml);

                            //string unsubscribeeventURL = string.Format("https://www.cellarpass.com/unsubscribe-event?user_id={0}&event_id={1}", StringHelpers.Encryption(user.user_id.ToString()), StringHelpers.Encryption(eventDetails.id.ToString()));

                            string strpath = "https://typhoon.cellarpass.com/";
                            if (ConnectionString.IndexOf("live") > -1)
                                strpath = "https://www.cellarpass.com/";

                            NameValueCollection queryCol = new NameValueCollection();

                            queryCol.Add("user_id", user.user_id.ToString());
                            queryCol.Add("event_id", eventDetails.id.ToString());

                            string encrypted = StringHelpers.EncryptQueryString(queryCol);

                            string unsubscribeeventURL = string.Format("{1}unsubscribe-event?q={0}", encrypted, strpath);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[Unsubscribe]]", Utility.GenerateEmailButton("Unsubscribe", unsubscribeeventURL, "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));

                            try
                            {
                                response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.TicketPostCapture, eventId, ew.EmailFrom, user.email, SubjectContent, RsvpGuestContent, 0, null, ew.Id, "", eventDetails.event_organizer_name));
                                errMsg = "";

                            }
                            catch (Exception ex)
                            {
                                logDAL.InsertLog("Email", "ProcessSendEventFollowingReminder: eventId=" + model.RsvpId.ToString() + "QueueId = " + model.UId.ToString() + ", error: " + ex.ToString(), "", 1, member_id);
                                errMsg = ex.Message;
                            }
                            ticketDAL.InsertTicketFollowEventReminderLog(eventId, user.user_id, errMsg);

                        }
                    }

                    //update log status
                    ticketDAL.UpdateReminderQueueStatus(queueId, errMsg);

                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("Email", "ProcessSendEventFollowingReminder: eventId=" + model.RsvpId.ToString() + "QueueId = " + model.UId.ToString() + ", error: " + ex.ToString(), "", 1, member_id);
            }
            return response;
        }

        public async Task<EmailResponse> ProcessCreateMailChimpOrder(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                int Id = model.RsvpId;

                int userId = model.UId;

                if (userId == 0 && model.data != null)
                {
                    userId = model.data.UId;
                }

                if (Id == 0 && model.data != null)
                {
                    Id = model.data.RsvpId;
                }

                SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);
                EventDAL eventDAL = new EventDAL(ConnectionString);

                var rsvp = eventDAL.GetReservationDetailsbyReservationId(Id);

                if (userId > 0)
                {
                    logDAL.InsertLog("WebApi", "AddMessageIntoQueue data 47:" + JsonConvert.SerializeObject(model), "", 3, 0);

                    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(rsvp.member_id, (int)SettingGroup.mailchimp);
                    string mcAPIKey = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_key);
                    string mcStore = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_store);
                    string mcCampaign = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_listname);
                    string mcreservationstag = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_reservationstag);
                    string mcticketingstag = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_ticketingstag);
                    string mcrsvpListId = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_rsvplistid);
                    string mcticketListId = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_ticketlistid);
                    string mclastSync = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_lastSync);

                    //call routine and pass data
                    MailChimpAPI mailChimpAPI = new MailChimpAPI(mcAPIKey, mcStore, mcCampaign, mcreservationstag, mcticketingstag, mcrsvpListId, mcticketListId);
                    bool isSuccess = mailChimpAPI.CreateRSVPOrder(rsvp);

                    Settings.Setting setting = new Settings.Setting();

                    setting.MemberId = rsvp.member_id;
                    setting.Group = SettingGroup.mailchimp;
                    setting.Key = SettingKey.mailchimp_lastSync;
                    setting.Value = DateTime.UtcNow.ToString();

                    settingsDAL.InsertSetting(setting);
                }
                else
                {
                    logDAL.InsertLog("WebApi", "AddMessageIntoQueue data 61:" + JsonConvert.SerializeObject(model), "", 3, 0);

                    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)SettingGroup.mailchimp_cp);
                    string mcAPIKey = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_key);
                    string mcStore = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_store);
                    string mcCampaign = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_newsletterListName);
                    string mcreservationstag = "";
                    string mcticketingstag = "";
                    string mcrsvpListId = "";
                    string mcticketListId = "";

                    //call routine and pass data
                    MailChimpAPI mailChimpAPI = new MailChimpAPI(mcAPIKey, mcStore, mcCampaign, mcreservationstag, mcticketingstag, mcrsvpListId, mcticketListId);
                    bool isSuccess = mailChimpAPI.CreateRSVPOrder(rsvp);
                }

                response.emailStatus = EmailStatus.na;
                response.emailSent = true;
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "ProcessCreateMailChimpOrder:  request data:" + JsonConvert.SerializeObject(model) + ", error: " + ex.Message, "", 3, 0);
            }
            return response;
        }

        public async Task<EmailResponse> ProcessCreateMailChimpTicketOrder(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "AddMessageIntoQueue data 58:" + JsonConvert.SerializeObject(model), "", 3, 0);

                int Id = model.RsvpId;

                if (Id == 0 && model.data != null)
                {
                    Id = model.data.RsvpId;
                }

                int userId = model.UId;

                if (userId == 0 && model.data != null)
                {
                    userId = model.data.UId;
                }

                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                TicketOrderModel ticketOrder = ticketDAL.GetTicketOrder(Id, "");

                SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);
                EventDAL eventDAL = new EventDAL(ConnectionString);

                if (userId > 0)
                {
                    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(ticketOrder.member_id, (int)SettingGroup.mailchimp);
                    string mcAPIKey = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_key);
                    string mcStore = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_store);
                    string mcCampaign = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_listname);
                    string mcreservationstag = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_reservationstag);
                    string mcticketingstag = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_ticketingstag);
                    string mcrsvpListId = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_rsvplistid);
                    string mcticketListId = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_ticketlistid);
                    string mclastSync = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_lastSync);

                    //call routine and pass data
                    MailChimpAPI mailChimpAPI = new MailChimpAPI(mcAPIKey, mcStore, mcCampaign, mcreservationstag, mcticketingstag, mcrsvpListId, mcticketListId);
                    bool isSuccess = mailChimpAPI.CreateTicketOrder(ticketOrder);

                    Settings.Setting setting = new Settings.Setting();

                    setting.MemberId = ticketOrder.member_id;
                    setting.Group = SettingGroup.mailchimp;
                    setting.Key = SettingKey.mailchimp_lastSync;
                    setting.Value = DateTime.UtcNow.ToString();

                    settingsDAL.InsertSetting(setting);
                }
                else
                {
                    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)SettingGroup.mailchimp_cp);
                    string mcAPIKey = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_key);
                    string mcStore = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_store);
                    string mcCampaign = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_newsletterListName);
                    string mcreservationstag = "";
                    string mcticketingstag = "";
                    string mcrsvpListId = "";
                    string mcticketListId = "";

                    //call routine and pass data
                    MailChimpAPI mailChimpAPI = new MailChimpAPI(mcAPIKey, mcStore, mcCampaign, mcreservationstag, mcticketingstag, mcrsvpListId, mcticketListId);
                    bool isSuccess = mailChimpAPI.CreateTicketOrder(ticketOrder);
                }

                response.emailStatus = EmailStatus.na;
                response.emailSent = true;
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "ProcessCreateMailChimpTicketOrder:  request data:" + JsonConvert.SerializeObject(model) + ", error: " + ex.Message, "", 3, 0);
            }
            return response;
        }

        public async Task<EmailResponse> ProcessCreateThirdPartyContact(ReservationEmailModel model, string ShopifyUrl, string ShopifyAuthToken)
        {
            var response = new EmailResponse();
            try
            {
                UserDAL userDAL = new UserDAL(ConnectionString);
                int userId = model.UId;
                int memberId = model.RsvpId;
                int rsvpId = 0;

                try
                {
                    if (!string.IsNullOrEmpty(model.BCode))
                        rsvpId = Convert.ToInt32(model.BCode);

                    if (rsvpId == 0 && model.data != null)
                    {
                        if (!string.IsNullOrEmpty(model.data.BCode))
                            rsvpId = Convert.ToInt32(model.data.BCode);
                    }
                }
                catch { }

                if (userId == 0 && model.data != null)
                {
                    userId = model.data.UId;
                }

                if (memberId == 0 && model.data != null)
                {
                    memberId = model.data.RsvpId;
                }

                Common.ReferralType refType = (Common.ReferralType)model.ActionSource;
                model.CCGuestEmail = userDAL.GetUserEmailById(userId);

                if (model.CCGuestEmail.ToLower().IndexOf("@noemail") > -1)
                    return response;

                UserDetailModel user = userDAL.GetUserDetailsbyemail(model.CCGuestEmail, memberId);

                bool optin = false;
                if (!string.IsNullOrEmpty(model.perMsg))
                    optin = true;

                await Utility.SaveOrUpdateContactThirdParty(memberId, user, (ReferralType)refType, rsvpId, ShopifyUrl, ShopifyAuthToken, optin);
                response.emailStatus = EmailStatus.na;
                response.emailSent = true;
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "AbandonedCartEmail", "ProcessAbandonedCartTicketsEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }

        public async Task<EmailResponse> ProcessGoogleCalendar(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {

                int rsvpId = model.data.RsvpId;
                if (rsvpId == 0)
                {
                    rsvpId = model.RsvpId;
                }

                if (rsvpId > 0)
                {
                    EventDAL eventDAL = new EventDAL(ConnectionString);
                    LogDAL logDAL = new LogDAL(ConnectionString);

                    var reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(rsvpId);

                    logDAL.InsertLog("Queservice::ProcessGoogleCalendar", "data:" + JsonConvert.SerializeObject(model), "", 3, reservationDetailModel.member_id);

                    GoogleCalendar.CalendarAddEventV2(reservationDetailModel);
                    response.emailStatus = EmailStatus.na;
                    response.emailSent = true;
                }
                else
                {
                    response.emailSent = false;
                    response.message = "No Reservation data found";
                }

            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                //Log.InsertLog(Log.LogType.AppError, "AbandonedCartEmail", "ProcessAbandonedCartTicketsEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }


        public async Task<EmailResponse> ProcessUploadOrderTobLoyal(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                //LogDAL logDAL = new LogDAL(ConnectionString);
                //logDAL.InsertLog("test step 3 bLoyal", "SendReservation", "");

                int reservationId = model.RsvpId;

                if (reservationId == 0 && model.data != null)
                {
                    reservationId = model.data.RsvpId;
                }

                //logDAL.InsertLog("test step 4 bLoyal", "SendReservation: reservationId-" + reservationId.ToString(), "");
                SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);
                EventDAL eventDAL = new EventDAL(ConnectionString);

                var rsvp = eventDAL.GetReservationDetailsbyReservationId(reservationId);

                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(rsvp.member_id, (int)SettingGroup.bLoyal);

                string extorderId = await Utility.bLoyalSendOrder(settingsGroup, reservationId);

                response.emailStatus = EmailStatus.na;
                response.emailSent = true;
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "AbandonedCartEmail", "ProcessAbandonedCartTicketsEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }

        public async Task<EmailResponse> SendContactSalesEmail(ContactSalesRequest model, MailConfig mailConfig)
        {
            var response = new EmailResponse();
            try
            {
                string toEmail = "sales@cellarpass.com";

                if (model.message_type == MessageType.PassTheBuck)
                    toEmail = "passthebuck@cellarpass.com";

                string emailMessage = string.Format("Name: {0}<br />Email: {1}<br />Phone: {2}<br />Company Name: {3}<br /><br />{4}", model.name, model.email, model.phone_number, model.company_name, model.message);
                await Task.Run(() => SendEmailAndSaveEmailLog(mailConfig, Email.EmailType.NA, 0, model.email, toEmail, "CellarPass Contact Page: " + model.subject, emailMessage, 0, null, 0));
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "AbandonedCartEmail", "ProcessAbandonedCartTicketsEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }

        public async Task<EmailResponse> ProcessVerificationEmail(VerificationEmailRequest model, MailConfig mailConfig)
        {
            var response = new EmailResponse();
            try
            {
                EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
                EmailContent ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.EmailVerification, 0);

                if (ew != null)
                {
                    string emailSubject = ew.EmailSubject;
                    string emailBody = ew.EmailBody;

                    string verificationURL = GenerateVerificationEmailURL(model.business_email, model.member_id, VerificationType.Subscription);

                    emailBody = emailBody.Replace("[[VerifyLink]]", Utility.GenerateEmailButton("Verify Email Address", verificationURL, "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));

                    await Task.Run(() => SendEmailAndSaveEmailLog(mailConfig, EmailType.VerifySubscription, model.member_id, "no-reply@cellarpass.com", model.business_email, emailSubject, emailBody, 0, null, ew.Id));
                }
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "AbandonedCartEmail", "ProcessAbandonedCartTicketsEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }

        public string GenerateVerificationEmailURL(string emailAddress, int idToEnable, VerificationType verificationType)
        {
            string link = string.Empty;

            NameValueCollection queryCol = new NameValueCollection();

            queryCol.Add(VerificationFields.email.ToString(), emailAddress);
            queryCol.Add(VerificationFields.id.ToString(), idToEnable.ToString());
            queryCol.Add(VerificationFields.type.ToString(), ((int)verificationType).ToString());

            string encrypted = StringHelpers.EncryptQueryString(queryCol);

            link = string.Format("https://www.cellarpass.com/verify-account?verification_code={0}", encrypted);

            return link;
        }
        public string GetSignupPlanName(int plan)
        {
            string planName = "";
            switch (plan)
            {
                case 1:
                    {
                        planName = "Basic";
                        break;
                    }

                case 2:
                    {
                        planName = "Professional";
                        break;
                    }

                case 3:
                    {
                        planName = "Ticketing Only";
                        break;
                    }

                case 4:
                    {
                        planName = "Table Pro";
                        break;
                    }

                case 5:
                    {
                        planName = "Starter";
                        break;
                    }

                case 6:
                    {
                        planName = "Plus";
                        break;
                    }

                case 7:
                    {
                        planName = "Enterprise";
                        break;
                    }

                default:
                    {
                        planName = "NA";
                        break;
                    }
            }

            return planName;
        }

        public async Task<EmailResponse> ProcessBusinessSubscriptionEmail(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            try
            {
                //LogDAL logDAL = new LogDAL(ConnectionString);
                //logDAL.InsertLog("test step 3 bLoyal", "SendReservation", "");

                int memberId = model.RsvpId;

                if (memberId == 0 && model.data != null)
                {
                    memberId = model.data.RsvpId;
                }

                string passCode = model.BCode;
                if (string.IsNullOrWhiteSpace(passCode) && model.data != null)
                {
                    passCode = model.data.BCode;
                }

                //logDAL.InsertLog("test step 4 bLoyal", "SendReservation: reservationId-" + reservationId.ToString(), "");
                SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);
                EventDAL eventDAL = new EventDAL(ConnectionString);
                var winery = eventDAL.GetWineryById(memberId);


                if (winery != null)
                {
                    EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);

                    EmailContent ec;
                    ec = emailDAL.GetEmailContent((int)Email.EmailTemplates.SysSubscriptionSignup, 0);
                    if (ec == null | ec.Active == false)
                    {
                        response.emailStatus = EmailStatus.na;
                        response.emailSent = false;
                    }
                    else
                    {
                        string subjectContent = ec.EmailSubject;
                        string bodyContent = ec.EmailBody;
                        string signupPlan = GetSignupPlanName(winery.SubscriptionPlan);
                        // Replace Subject Tags
                        subjectContent = subjectContent.Replace("[[FirstName]]", winery.BillingFirstName);
                        subjectContent = subjectContent.Replace("[[LastName]]", winery.BillingLastName);
                        subjectContent = subjectContent.Replace("[[BusinessName]]", winery.DisplayName);
                        subjectContent = subjectContent.Replace("[[BusinessPhone]]", winery.BusinessPhone.ToString());
                        subjectContent = subjectContent.Replace("[[BusinessEmail]]", winery.BillingEmailAddress);
                        subjectContent = subjectContent.Replace("[[BusinessURL]]", winery.WebSiteUrl);
                        subjectContent = subjectContent.Replace("[[BusinessZipCode]]", winery.WineryAddress.zip_code);
                        subjectContent = subjectContent.Replace("[[CPFriendlyURL]]", "https://www.cellarpass.com/business/" + winery.MemberProfileUrl);
                        subjectContent = subjectContent.Replace("[[ServicePlan]]", signupPlan);
                        subjectContent = subjectContent.Replace("[[RegionURL]]", "https://www.cellarpass.com/region/" + winery.AppellationName);

                        // Body replacement
                        bodyContent = bodyContent.Replace("[[FirstName]]", winery.BillingFirstName);
                        bodyContent = bodyContent.Replace("[[LastName]]", winery.BillingLastName);
                        bodyContent = bodyContent.Replace("[[BusinessName]]", winery.DisplayName);
                        bodyContent = bodyContent.Replace("[[BusinessPhone]]", winery.BusinessPhone.ToString());
                        bodyContent = bodyContent.Replace("[[BusinessEmail]]", winery.BillingEmailAddress);
                        bodyContent = bodyContent.Replace("[[BusinessURL]]", winery.WebSiteUrl);
                        bodyContent = bodyContent.Replace("[[BusinessZipCode]]", winery.WineryAddress.zip_code);
                        bodyContent = bodyContent.Replace("[[CPFriendlyURL]]", "https://www.cellarpass.com/business/" + winery.MemberProfileUrl);
                        bodyContent = bodyContent.Replace("[[ServicePlan]]", signupPlan);
                        bodyContent = bodyContent.Replace("[[RegionURL]]", "https://www.cellarpass.com/region/" + winery.AppellationName);
                        if (!string.IsNullOrEmpty(passCode))
                        {
                            bodyContent = bodyContent.Replace("[[NewUserLogin]]", "https://admin.cellarpass.com/admin/login_newuser.aspx?k=" + passCode);
                        }
                        else
                        {
                            bodyContent = bodyContent.Replace("[[NewUserLogin]]", "https://www.cellarpass.com/manage=" + passCode);
                        }

                        // Add additional email to copy sys admin
                        string toEmails = string.Format("{0},{1}", winery.BillingEmailAddress.TrimEnd(','), ec.EmailTo);

                        response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.SysSubscriptionSignup, 0, ec.EmailFrom, toEmails, subjectContent, bodyContent, 0, null, ec.Id));

                    }


                }
                else
                {
                    response.emailStatus = EmailStatus.na;
                    response.emailSent = false;
                }



            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "AbandonedCartEmail", "ProcessAbandonedCartTicketsEmail: acId=" + acId + " : " + ex.ToString);
            }
            return response;
        }


        /// <summary>
        /// This method is used for sending email on create reservation
        /// </summary>
        /// <returns></returns>
        public async Task<EmailResponse> SendWaverInviteEmail(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            if (model.RsvpId == 0)
            {
                response.message = InternalServerError;
                return response;
            }
            EventDAL eventDAL = new EventDAL(ConnectionString);
            EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
            string bookingCode = model.data.BCode;
            int userID = Convert.ToInt32(model.BCode);
            int guestID = model.UId;
            int iReservationId = model.RsvpId;
            bool isMobile = model.isMobile;
            InviteType invite_type = (InviteType)model.data.isRsvpType;
            bool SendToFriendMode = model.SendToFriendMode;
            string ShareMessage = model.ShareMessage;
            List<ShareFriends> ShareEmails = model.share_friends;
            string CCGuestEmail = model.CCGuestEmail;
            string inviteEmail = model.InviteEmail;

            bool SendCCOnly = CCGuestEmail.Length > 0;
            string perMsg = model.perMsg;

            if (string.IsNullOrEmpty(perMsg))
            {
                perMsg = model.data.perMsg;
            }

            int alternativeEmailTemplate = model.alternativeEmailTemplate;

            string bookingGUID = "";
            //Note: In the rare chance that our auto generated booking codes should ever duplicate we pass the user id
            //to make sure what we get is unique.

            //Get Data for Email
            var reservationData = eventDAL.GetReservationEmailDataByReservationId(iReservationId, userID, bookingCode);

            DateTime LocalBookingDate = DateTime.UtcNow;
            if (reservationData != null)
            {
                if (string.IsNullOrWhiteSpace(bookingCode))
                    bookingCode = reservationData.BookingCode;

                bookingGUID = reservationData.BookingGUID;
                LocalBookingDate = Times.ToTimeZoneTime(reservationData.BookingDate, (Times.TimeZone)reservationData.TimeZoneId);
            }

            string inviteSection = "";
            bool hasInvite = reservationData.HasInvite;

            if (hasInvite)
            {
                string inviteHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 20px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"line-height: 140%;\"><strong>IMPORTANT!</strong></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\"><strong>Your reservation is currently pending and requires additional action to be confirmed.</strong><br /></span></p> <p style=\"line-height: 140%;\"> </p> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\">You must take immediate action by [[ExpirationDateTime]] in order to confirm your appointment or your reservation will be automatically cancelled and released to others.</span></p> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\"><br />Click on the 'Complete Reservation' button to complete your reservation.</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[CompeteRsvpLink]]\" style=\"height:47px; v-text-anchor:middle; width:540px;\" arcsize=\"8.5%\" stroke=\"f\" fillcolor=\"#1069b0\"><w:anchorlock/><center style=\"color:#FFFFFF;font-family:'Poppins', sans-serif;;\"><![endif]--> <a href=\"[[CompleteRSVPLink]]\" target=\"_blank\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #FFFFFF; background-color: #1069b0; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:100%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;font-size: 14px;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"line-height: 16.8px;\">COMPLETE RESERVATION</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                DateTime expirationdateTime = reservationData.ReservationInviteExpirationDateTime;

                inviteHtml = inviteHtml.Replace("[[ExpirationDateTime]]", String.Format("{0} {1}", expirationdateTime.ToShortDateString(), expirationdateTime.ToString("hh:mm tt")));
                inviteHtml = inviteHtml.Replace("[[CompleteRSVPLink]]", "https://www.cellarpass.com");
                inviteSection = inviteHtml;
            }

            string notesSection = "";

            //Preview sample messages
            string personalMSg = model.perMsg;
            string guestNote = reservationData.Notes;

            //NOTES

            string notesSectionHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\">[[Notes]]</p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
            string notesCombined = "";
            string personlMsgHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Personal Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[Personal_Message]]</span><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"></span></p>";
            string guestNoteHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Guest Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[RsvpNote]]</span></p>";

            //If either personal Message or guest note is available we need to render this section
            bool showNoteSection = false;

            if (!string.IsNullOrEmpty(personalMSg))
            {
                showNoteSection = true;
                //Replace message tags
                personlMsgHtml = personlMsgHtml.Replace("[[Personal_Message]]", personalMSg);
                //Add to notes
                notesCombined += personlMsgHtml;
            }

            if (!string.IsNullOrEmpty(guestNote))
            {
                showNoteSection = true;

                //Replace message tags
                guestNoteHtml = guestNoteHtml.Replace("[[RsvpNote]]", guestNote);

                //If personl msg is not empty add this space html first before adding the guest note to create some separation
                if (notesCombined.Length > 0)
                {
                    notesCombined += "<p style=\"font-size: 14px; line-height: 120%;\"> </p>";
                }

                //Add to notes
                notesCombined += guestNoteHtml;
            }

            //If either personal or guest note was provided this should be true and we combine it all.
            if (showNoteSection)
            {
                //replace tag in notes section html with notes combined
                notesSectionHtml = notesSectionHtml.Replace("[[Notes]]", notesCombined);
                //Set noteSection to Html
                notesSection = notesSectionHtml;
            }

            //Add-Ons
            string addOnItems = "";
            dynamic addOnItemswithoutpayment = "";

            if (iReservationId > 0)
            {

                var listAddOns = eventDAL.GetReservationAddOnItems(iReservationId);
                if ((listAddOns != null))
                {
                    int idx = 0;
                    foreach (var addon in listAddOns)
                    {
                        idx += 1;
                        string numberedList = "";
                        if (addon.ItemType == (int)Common.AddOnGroupType.menu)
                        {
                            numberedList = string.Format("{0}. ", idx);
                        }
                        addOnItems += string.Format("{0}{1} ({2}) {3}<br />", numberedList, addon.Name, addon.Qty, (string.IsNullOrWhiteSpace(Convert.ToString(addon.Price)) ? "0.00" : addon.Price.ToString("N2")));
                        addOnItemswithoutpayment += string.Format("{0}{1} ({2})<br />", numberedList, addon.Name, addon.Qty);
                    }
                }

            }

            //Guests Attending

            //Need to get the guests attending each reservation in the booking
            //Get Guests Detail
            string GuestsAttending = eventDAL.GetGuestAttending(reservationData.ReservationId, reservationData.GuestName);


            string zoomContent = string.Empty;

            ZoomMeetingInfo zoomMeetingInfo = eventDAL.GetZoomMeetingInfo(reservationData.slotid, reservationData.slottype, reservationData.EventDate, reservationData.ReservationId);

            if (zoomMeetingInfo != null && zoomMeetingInfo.Id > 0)
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)Common.Common.SettingGroup.member);


                bool requirePassword = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_zoom_require_password);
                string zoomMeetingPassword = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_zoom_password);

                if (requirePassword && !string.IsNullOrEmpty(zoomMeetingPassword))
                {
                    zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}<br>Password: {3}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()), zoomMeetingPassword);
                }
                else
                {
                    zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()));
                }
            }


            string PaymentDetails = "";
            string paymentItem = "<p style=\" font-size:14px; line-height: 100%;\"><span style=\"font-family:Poppins, sans-serif; font-size: 14px; line-height: 14px;\">{{PaymentDesc}}</span></p>";

            if (reservationData.Fee == 0)
            {
                string PaymentStatus = paymentItem;
                PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", "Complimentary");

                PaymentDetails = PaymentDetails + PaymentStatus;
            }
            else
            {
                foreach (var item in eventDAL.GetPaymentStatusV2byReservationId(reservationData.ReservationId))
                {
                    string PaymentStatus = paymentItem;
                    PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", item);

                    PaymentDetails = PaymentDetails + PaymentStatus;
                }
            }

            //#### RSVP Guest Email - START ####

            try
            {
                string PhoneNumber = "";

                if (model.InviteEmail.IndexOf("@noemail") == -1)
                {

                    string RsvpGuestContent = "";
                    //Dim GuestsAttending As String = ""
                    ArrayList alGuestsAttendingEmail = new ArrayList();
                    DateTime StartDate = reservationData.EventDate.Add(reservationData.StartTime);
                    DateTime EndDate = reservationData.EventDate.Add(reservationData.EndTime);
                    TimeSpan Duration = EndDate.Subtract(StartDate);
                    string RsvpGuestSubjectContent = "";

                    string timezoneName = reservationData.timezone_name;

                    if (!string.IsNullOrEmpty(timezoneName))
                    {
                        timezoneName = " (" + timezoneName + ")";
                    }
                    else
                    {
                        timezoneName = "";
                    }

                    //LOCATION ADDRESS
                    string lAddress1 = reservationData.MemberAddress1;
                    string lAddress2 = reservationData.MemberAddress2;
                    string lCity = reservationData.MemberCity;
                    string lState = reservationData.MemberState;
                    string lZip = reservationData.MemberZipCode;
                    string calendarAddress = "";
                    //Use Location based address instead of winery address if provided for location
                    if ((reservationData.locAddress1 != null))
                    {
                        if (reservationData.locAddress1.Trim().Length > 0)
                        {
                            lAddress1 = (reservationData.locAddress1 == null ? "" : reservationData.locAddress1);
                            lAddress2 = (reservationData.locAddress2 == null ? "" : reservationData.locAddress2);
                            lCity = (reservationData.locCity == null ? "" : reservationData.locCity);
                            lState = (reservationData.locState == null ? "" : reservationData.locState);
                            lZip = (reservationData.locZip == null ? "" : reservationData.locZip);
                        }
                    }

                    //Calendar address
                    calendarAddress += lAddress1 + "\\n ";
                    if (lAddress2.Trim().Length > 0)
                    {
                        calendarAddress += lAddress2 + "\\n ";
                    }
                    calendarAddress += lCity + ", " + lState + " " + lZip + "\\n ";

                    string FormatFee = reservationData.Fee.ToString();

                    //Format Fee
                    if (reservationData.Fee > 0)
                    {
                        FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US")) + ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                        //FormatFee = string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.Fee) + ", " + GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;
                    }
                    else
                    {
                        FormatFee = "Complimentary";
                    }

                    //Phone Number - check and use home first then work if phone is empty
                    if (!string.IsNullOrEmpty(reservationData.GuestPhone))
                    {
                        PhoneNumber = reservationData.GuestPhone;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(reservationData.GuestWPhone))
                        {
                            PhoneNumber = Utility.FormatTelephoneNumber(reservationData.GuestWPhone.ToString(), "US");
                        }
                    }



                    EmailContent ew = new EmailContent();
                    ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.SysCovidWaiver, 0);
                    //If Not ew Is Nothing And ew.Active = True Then

                    if ((ew != null))
                    {

                        //Configure/Format CancellationPolicy
                        string CancellationPolicy = "";
                        string CancelByDate = "";
                        if ((reservationData.Content != null))
                        {
                            if (reservationData.Content.Trim().Length > 0)
                            {
                                CancellationPolicy = reservationData.Content;

                                if (reservationData.CancelLeadTime > 50000)
                                {
                                    CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                }
                                else
                                {
                                    CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                                }

                            }
                        }

                        //Replace Content Tags
                        RsvpGuestSubjectContent = ew.EmailSubject;
                        RsvpGuestContent = ew.EmailBody;

                        //Subject
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingCode]]", bookingCode);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DestinationName]]", reservationData.DestinationName);

                        RsvpGuestSubjectContent = ReservationTagReplace(RsvpGuestSubjectContent, bookingGUID);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpGuid]]", bookingGUID);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestName]]", reservationData.GuestName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestPhone]]", PhoneNumber);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestsAttending]]", GuestsAttending);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FormatFee);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Gratuity]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.GratuityAmount));

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[PaymentStatus]]", PaymentDetails);

                        string bannerImageHtml = Utility.GetBannerImageHtmlByWineryId(reservationData.WineryID);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberName]]", reservationData.MemberName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress1]]", lAddress1);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress2]]", lAddress2);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressCity]]", lCity);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressState]]", lState);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressZipCode]]", lZip);

                        string lFullAddress = string.Empty;
                        lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[LocationAddress]]", lFullAddress);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName)); //{0:t}
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDuration]]", Duration.ToString());
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                        //RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpHost]]", Host)
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[NotesSection]]", notesSection);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InviteSection]]", inviteSection);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancellationPolicy]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Personal_Message]]", perMsg);
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[ZoomMeeting]]", zoomContent);
                        //Remove Admin Only Tags
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddress1]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[[GuestAddress2]]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressCity]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressState]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestAddressZipCode]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InternalNotes]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[[AffiliateNote]]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookedBy]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[AdminConfirmStatus]]", "");
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));
                        RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[SurveyWaiverLink]]", "");

                        //Body
                        RsvpGuestContent = RsvpGuestContent.Replace("[[BookingCode]]", bookingCode);

                        RsvpGuestContent = ReservationTagReplace(RsvpGuestContent, bookingGUID);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpGuid]]", bookingGUID);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestName]]", reservationData.GuestName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", reservationData.GuestEmail);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestPhone]]", PhoneNumber);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestCount]]", Convert.ToString(reservationData.GuestCount));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestsAttending]]", GuestsAttending);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FormatFee);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[Gratuity]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.GratuityAmount));

                        RsvpGuestContent = RsvpGuestContent.Replace("[[PaymentStatus]]", PaymentDetails);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", reservationData.MemberName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", reservationData.MemberEmail);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress1]]", lAddress1);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress2]]", lAddress2);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressCity]]", lCity);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressState]]", lState);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressZipCode]]", lZip);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[LocationAddress]]", lFullAddress);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(reservationData.CancelLeadTime));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDuration]]", Duration.ToString());
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpEvent]]", reservationData.EventName);
                        //RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpHost]]", Host)
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDestination]]", reservationData.DestinationName);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpLocation]]", reservationData.EventLocation);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[NotesSection]]", notesSection);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[InviteSection]]", inviteSection);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(reservationData.Notes) ? "None" : reservationData.Notes));
                        RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpStatus]]", Email.GetReservationStatus(reservationData.Status));

                        RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationPolicy]]", CancellationPolicy);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[Personal_Message]]", perMsg);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[ZoomMeeting]]", zoomContent);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));

                        string strpath = "https://typhoon.cellarpass.com/";
                        if (ConnectionString.IndexOf("live") > -1)
                            strpath = "https://www.cellarpass.com/";

                        string profileUrl = string.Format("{1}business/{0}", reservationData.ProfileUrl, strpath);

                        RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                        RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[MemberLinkButton]]", profileUrl);
                        RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                        RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[MemberLinkButton]]", profileUrl);

                        RsvpGuestContent = RsvpGuestContent.Replace("[[MemberLinkButton]]", profileUrl);

                        //Add-Ons:
                        RsvpGuestContent = RsvpGuestContent.Replace("[[Add-Ons]]", addOnItems);
                        RsvpGuestContent = RsvpGuestContent.Replace("[[AddOnItems]]", addOnItemswithoutpayment);

                        //Remove Admin Only Tags

                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddress1]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddress2]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressCity]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressState]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[GuestAddressZipCode]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[InternalNotes]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateNote]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[BookedBy]]", "");
                        RsvpGuestContent = RsvpGuestContent.Replace("[[AdminConfirmStatus]]", "");

                        string surveyWaverLink = "";
                        //get Survey waiver link
                        if (reservationData.WineryID > 0)
                        {
                            SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                            List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)Common.Common.SettingGroup.member);
                            bool covidSurveyEnabled = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_enable_covid_survey);
                            bool covidWaiverEnabled = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_enable_covid_waiver);

                            //bookingGUID
                            string surveyURL = "";
                            if (covidSurveyEnabled && invite_type == InviteType.Survey)
                            {
                                surveyURL = string.Format("https://www.cellarpass.com/survey/covid19-symptom-questionaire?mid={0}&bcode={1}", reservationData.WineryID, reservationData.BookingGUID);
                                surveyWaverLink = Utility.GenerateEmailButton("Complete Survey", surveyURL, "#47bf12", "#47bf12", "15px", "15px", "#ffffff");
                            }

                            if (covidWaiverEnabled && invite_type == InviteType.Waiver)
                            {
                                surveyURL = string.Format("https://www.cellarpass.com/member-survey/encore-winery-limits-of-liability-waiver?mid={0}&bcode={1}", reservationData.WineryID, reservationData.BookingGUID);
                                surveyWaverLink = Utility.GenerateEmailButton("Complete Waiver", surveyURL, "#47bf12", "#47bf12", "15px", "15px", "#ffffff");

                            }

                        }
                        RsvpGuestContent = RsvpGuestContent.Replace("[[SurveyWaiverLink]]", surveyWaverLink);
                        //Attach iCal File
                        //Dim attach As New Mail.Attachment
                        EmailAttachment emailAttachment = new EmailAttachment();

                        //Directions URL
                        //string DirectionsURL = "";


                        try
                        {
                            if (RsvpGuestContent.IndexOf("[[?Attach_iCal]]") > 0)
                            {
                                if ((reservationData != null))
                                {
                                    if (RsvpGuestContent.IndexOf("[[?Attach_iCal]]") > 0)
                                    {
                                        var winery = new WineryModel();
                                        winery = eventDAL.GetWineryById(reservationData.WineryID);
                                        //We replace this with nothing as it's just used as a flag to attach
                                        //attach.Name = "CellarPass_Confirm.ics"
                                        //attach.Contents = Events.ExportRsvpV2ToICalendarGuest(rsvpResult, winery, calendarAddress, DestinationName)
                                        emailAttachment.Name = "CellarPass_Confirm.ics";
                                        var reservation = eventDAL.GetReservationDetailsbyReservationId(iReservationId);
                                        emailAttachment.Contents = ExportRsvpV2ToICalendarGuest(reservation, winery, calendarAddress, reservationData.DestinationName);
                                    }
                                    else
                                    {
                                        //attach = Nothing
                                        emailAttachment = null;
                                    }

                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            //attach = Nothing
                            emailAttachment = null;
                        }

                        //Replace here so that we remove it if it's blank
                        string DirectionsURL = "";
                        if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
                        {
                            DirectionsURL = "<a href=\"" + reservationData.MapAndDirectionsURL.Trim() + "\" target=\"_blank\">Get Directions</a>";
                        }
                        RsvpGuestContent = RsvpGuestContent.Replace("[[DirectionsURL]]", DirectionsURL);

                        //We replace this with nothing as it's just used as a flag to attach
                        RsvpGuestContent = RsvpGuestContent.Replace("[[?Attach_iCal]]", "");

                        string emailTo = inviteEmail;
                        //Send with New Mail (Mailgun)
                        response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.Rsvp, iReservationId, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent, reservationData.WineryID, emailAttachment, ew.Id));

                        //eventDAL.UpdateRsvpSurveyWaiverStatus(reservationData.WineryID, iReservationId, inviteEmail, (int)invite_type, RSVPPostCaptureStatus.Invited);
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "SendWaverInviteEmail", "InviteEmail: " + ex.ToString());
            }
            //#### RSVP Guest Email - END ####

            return response;
        }

        public async Task<EmailResponse> SendSystemEmailAddressConfirmed(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            if (string.IsNullOrEmpty(model.CCGuestEmail))
            {
                response.message = InternalServerError;
                return response;
            }
            EventDAL eventDAL = new EventDAL(ConnectionString);
            EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);

            EmailContent ew = new EmailContent();
            ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.SysEmailAddressConfirmed, 0);

            string RsvpGuestSubjectContent = "";
            string RsvpGuestContent = "";

            RsvpGuestSubjectContent = ew.EmailSubject;
            RsvpGuestContent = ew.EmailBody;

            string emailTo = model.CCGuestEmail;
            //Send with New Mail (Mailgun)
            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.SysUserInvite, 0, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent, 0, null, ew.Id));

            return response;
        }

        public async Task<EmailResponse> SendSystemNewUser(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            if (string.IsNullOrEmpty(model.CCGuestEmail))
            {
                response.message = InternalServerError;
                return response;
            }
            UserDAL userDAL = new UserDAL(ConnectionString);
            EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);

            string emailTo = model.CCGuestEmail;

            UserDetailModel userDetailModel = userDAL.GetUserDetailsbyemail(emailTo);

            EmailContent ew = new EmailContent();
            ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.NewGuestSys, 0);

            string RsvpGuestSubjectContent = "";
            string RsvpGuestContent = "";

            RsvpGuestSubjectContent = ew.EmailSubject;
            RsvpGuestContent = ew.EmailBody;

            string verificationURL = string.Format("https://www.cellarpass.com/verify-account?verification_code={0}", CPReservationApi.Common.StringHelpers.Encryption(emailTo));

            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DateAndTime]]", userDetailModel.date_created.ToShortDateString());
            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestFirstName]]", userDetailModel.first_name);
            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestLastName]]", userDetailModel.last_name);
            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestEmail]]", userDetailModel.email);
            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[LoginLink]]", "<a href=\"https://www.cellarpass.com/account\">Click Here</a>");

            RsvpGuestContent = RsvpGuestContent.Replace("[[DateAndTime]]", userDetailModel.date_created.ToShortDateString());
            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestFirstName]]", userDetailModel.first_name);
            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestLastName]]", userDetailModel.last_name);
            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", userDetailModel.email);
            RsvpGuestContent = RsvpGuestContent.Replace("[[VerifyLink]]", Utility.GenerateEmailButton("Verify Email Address", verificationURL, "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));

            RsvpGuestContent = RsvpGuestContent.Replace("[[LoginLink]]", "<a href=\"https://www.cellarpass.com/account\">Click Here</a>");

            //Send with New Mail (Mailgun)
            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.NA, 0, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent, 0, null, ew.Id));

            return response;
        }

        public async Task<EmailResponse> ProcessSendSysNewAdminUser(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            if (string.IsNullOrEmpty(model.CCGuestEmail))
            {
                response.message = InternalServerError;
                return response;
            }
            UserDAL userDAL = new UserDAL(ConnectionString);
            EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);

            string emailTo = model.CCGuestEmail;

            UserDetailModel userDetailModel = userDAL.GetUserDetailsbyemail(emailTo);

            EmailContent ew = new EmailContent();
            ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.SysNewAdminUser, 0);

            string RsvpGuestSubjectContent = "";
            string RsvpGuestContent = "";

            RsvpGuestSubjectContent = ew.EmailSubject;
            RsvpGuestContent = ew.EmailBody;

            string currentYear = DateTime.UtcNow.Year.ToString();
            string copyrightMsg = string.Format("&copy;2009 - {0} CellarPass, Inc. All Rights Reserved", currentYear);

            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[UserFirstName]]", userDetailModel.first_name);
            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Copyright]]", copyrightMsg);
            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[UserEmail]]", userDetailModel.email);
            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[LoginLink]]", "<a href=\"https://admin.cellarpass.com/admin/login.aspx\">Click Here</a>");

            RsvpGuestContent = RsvpGuestContent.Replace("[[UserFirstName]]", userDetailModel.first_name);
            RsvpGuestContent = RsvpGuestContent.Replace("[[Copyright]]", copyrightMsg);
            RsvpGuestContent = RsvpGuestContent.Replace("[[UserEmail]]", userDetailModel.email);
            RsvpGuestContent = RsvpGuestContent.Replace("[[LoginLink]]", "<a href=\"https://admin.cellarpass.com/admin/login.aspx\">Click Here</a>");

            //Send with New Mail (Mailgun)
            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.SysNewAdminUser, 0, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent, 0, null, ew.Id));

            return response;
        }

        public async Task<EmailResponse> SendInvitetoGuestLinkProApp(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            if (string.IsNullOrEmpty(model.CCGuestEmail))
            {
                response.message = InternalServerError;
                return response;
            }
            EventDAL eventDAL = new EventDAL(ConnectionString);
            EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);

            EmailContent ew = new EmailContent();
            ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.SysInviteToGuestLinkProApp, 0);

            string RsvpGuestSubjectContent = "";
            string RsvpGuestContent = "";

            RsvpGuestSubjectContent = ew.EmailSubject;
            RsvpGuestContent = ew.EmailBody;

            string emailTo = model.CCGuestEmail;
            //Send with New Mail (Mailgun)
            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, EmailType.SysUserInvite, 0, ew.EmailFrom, emailTo, RsvpGuestSubjectContent, RsvpGuestContent, 0, null, ew.Id));

            return response;
        }

        /// <summary>
        /// This method is used to send cprsvp reminder emailv2
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<EmailResponse> SendReservationInviteReminder(ReservationEmailModel model)
        {
            var response = new EmailResponse();
            if ((!string.IsNullOrEmpty(model.data.BCode) || !(model.data.UId > 0)) && model.data.RsvpId == 0)
            {
                response.message = InternalServerError;
                return response;
            }
            EventDAL eventDAL = new EventDAL(ConnectionString);
            EmailServiceDAL emailDAL = new EmailServiceDAL(ConnectionString);
            //Get Data for Email
            //#### RSVP Guest Email Reminder - START ####
            try
            {
                var reservationData = await Task.Run(() => eventDAL.GetReservationEmailDataByReservationId(model.data.RsvpId, model.data.UId, model.data.BCode));
                if (reservationData != null)
                {
                    int ReservationId = model.data.RsvpId;
                    int WineryID = reservationData.WineryID;
                    string BookingCode = reservationData.BookingCode;
                    string bookingGUID = reservationData.BookingGUID;
                    DateTime LocalBookingDate = Times.ToTimeZoneTime(reservationData.BookingDate, (Times.TimeZone)reservationData.TimeZoneId);
                    string GuestName = reservationData.GuestName;
                    string GuestEmail = reservationData.GuestEmail;
                    string GuestPhone = reservationData.GuestPhone;
                    string GuestWPhone = reservationData.GuestWPhone;
                    string GuestAddress1 = reservationData.GuestAddress1;
                    string GuestAddress2 = reservationData.GuestAddress2;
                    string GuestCity = reservationData.GuestCity;
                    string GuestState = reservationData.GuestState;
                    string GuestZipCode = reservationData.GuestZipCode;
                    decimal Fee = reservationData.Fee;
                    int ChargeFee = reservationData.ChargeFee;
                    decimal FeePaid = reservationData.FeePaid;
                    short GuestCount = reservationData.GuestCount;
                    string MemberName = reservationData.MemberName;
                    string MemberPhone = reservationData.MemberPhone;
                    string MemberEmail = reservationData.MemberEmail;
                    string MemberAddress1 = reservationData.MemberAddress1;
                    string MemberAddress2 = reservationData.MemberAddress2;
                    string MemberCity = reservationData.MemberCity;
                    string MemberState = reservationData.MemberState;
                    string MemberZipCode = reservationData.MemberZipCode;
                    DateTime EventDate = reservationData.EventDate;
                    TimeSpan StartTime = reservationData.StartTime;
                    TimeSpan EndTime = reservationData.EndTime;
                    string EventName = reservationData.EventName;
                    string EventLocation = reservationData.EventLocation;
                    string Notes = reservationData.Notes;
                    int Status = reservationData.Status;
                    string InternalNote = reservationData.InternalNote;
                    int BookedById = reservationData.BookedById;
                    int CancelLeadTime = reservationData.CancelLeadTime;
                    int EmailContentID = reservationData.EmailContentID;
                    int EmailTemplateID = reservationData.EmailTemplateID;
                    string Host = reservationData.Host;
                    int AffiliateID = reservationData.AffiliateID;
                    string Content = reservationData.Content;
                    string DestinationName = reservationData.DestinationName;
                    string locAddress1 = reservationData.locAddress1;
                    string locAddress2 = reservationData.locAddress2;
                    string locCity = reservationData.locCity;
                    string locState = reservationData.locState;
                    string locZip = reservationData.locZip;

                    string addOnItems = "";
                    string AddOnDetails = "";
                    StringBuilder AddOnItemDetails = new StringBuilder();
                    string addOnHeading = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 10px 20px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\"><strong><span style=\"font-size: 14px; line-height: 14px;\">ADDITIONAL SELECTIONS</span></strong> </span> </p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string addOnItem = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"439\" style=\"background-color: #ffffff;width: 439px;padding: 0px 0px 0px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-73p33\" style=\"max-width: 320px;min-width: 439.98px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 0px 0px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">{{AddOnQty}} X {{AddOnItem}}</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"160\" style=\"background-color: #ffffff;width: 160px;padding: 0px 30px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-26p67\" style=\"max-width: 320px;min-width: 160.02px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 30px 0px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">{{AddOnPrice}}</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

                    string map_and_directions_url = "";
                    string mapInageURl = "";
                    string MapURL = "";

                    string googleAPIKey = "AIzaSyDKGd8SyV0GhpzD-onvM0ItLpzq5GhCFfA";
                    if (_appSetting != null && _appSetting.Value != null && !string.IsNullOrEmpty(_appSetting.Value.GoogleAPIKey))
                    {
                        googleAPIKey = _appSetting.Value.GoogleAPIKey;
                    }

                    if (reservationData.LocationId > 0)
                    {
                        MapURL = await Utility.GetMapImageHtmlByLocation(reservationData.LocationId, googleAPIKey);

                        var location = eventDAL.GetLocationMapDataByID(reservationData.LocationId);

                        if (location != null && location.location_id > 0)
                        {
                            map_and_directions_url = location.map_and_directions_url;
                            mapInageURl = "https://cdncellarpass.blob.core.windows.net/photos/location_maps/" + reservationData.LocationId.ToString() + "_dot.jpg";
                        }
                    }

                    var listAddOns = eventDAL.GetReservationAddOnItems(reservationData.ReservationId);
                    if (listAddOns != null)
                    {
                        int idx = 0;
                        foreach (var addon in listAddOns)
                        {
                            string item1 = addOnItem;
                            item1 = item1.Replace("{{AddOnQty}}", addon.Qty.ToString());
                            item1 = item1.Replace("{{AddOnItem}}", addon.Name);
                            item1 = item1.Replace("{{AddOnPrice}}", addon.Price.ToString("C", new CultureInfo("en-US")));
                            AddOnItemDetails.Append(item1);

                            idx += 1;
                            string numberedList = "";
                            if (addon.ItemType == (int)Common.AddOnGroupType.menu)
                            {
                                numberedList = string.Format("{0}. ", idx);
                            }
                            addOnItems += string.Format("{0}{1} ({2}) {3}<br />", numberedList, addon.Name, addon.Qty, (string.IsNullOrWhiteSpace(Convert.ToString(addon.Price)) ? "0.00" : addon.Price.ToString("N2")));
                        }
                    }

                    AddOnDetails = addOnHeading + AddOnItemDetails.ToString();

                    if (string.IsNullOrEmpty(addOnItems))
                        AddOnDetails = "";

                    string PaymentDetails = "";
                    string paymentItem = "<p style=\" font-size:14px; line-height: 100%;\"><span style=\"font-family:Poppins, sans-serif; font-size: 14px; line-height: 14px;\">{{PaymentDesc}}</span></p>";

                    if (reservationData.Fee == 0)
                    {
                        string PaymentStatus = paymentItem;
                        PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", "Complimentary");

                        PaymentDetails = PaymentDetails + PaymentStatus;
                    }
                    else
                    {
                        foreach (var item in eventDAL.GetPaymentStatusV2byReservationId(reservationData.ReservationId))
                        {
                            string PaymentStatus = paymentItem;
                            PaymentStatus = PaymentStatus.Replace("{{PaymentDesc}}", item);

                            PaymentDetails = PaymentDetails + PaymentStatus;
                        }
                    }

                    //'LOCATION / ZOOM 'NOTE: We do not show the zoom in the preview unless you swich this manually here. 
                    bool sendPreviewWithZoom = false;

                    if (reservationData.EventId > 0)
                    {
                        EventModel eventModelValidate = eventDAL.GetEventById(reservationData.EventId);

                        if (eventModelValidate != null && eventModelValidate.EventID > 0)
                        {
                            if (eventModelValidate.EventTypeId == 34 && eventModelValidate.MeetingBehavior == 2)
                            {
                                sendPreviewWithZoom = true;
                            }
                        }
                    }

                    string GratuityContent = "";
                    if (reservationData.GratuityAmount > 0)
                    {
                        string GratuityHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"419\" style=\"background-color: #ffffff;width: 419px;padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-69p84\" style=\"max-width: 320px;min-width: 419.04px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 10px 10px 30px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\">Gratuity:      </span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"180\" style=\"background-color: #ffffff;width: 180px;padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-30p16\" style=\"max-width: 320px;min-width: 180.96px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 30px 10px 10px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"font-family: 'Poppins', sans-serif;; line-height: 100%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 100%; text-align: right;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 14px;\">[[GratuityAmt]]</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";

                        GratuityHtml = GratuityHtml.Replace("[[GratuityAmt]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.GratuityAmount));

                        GratuityContent = GratuityHtml;

                    }

                    //LOCATION ADDRESS
                    string lAddress1 = reservationData.MemberAddress1;
                    string lAddress2 = reservationData.MemberAddress2;
                    string lCity = reservationData.MemberCity;
                    string lState = reservationData.MemberState;
                    string lZip = reservationData.MemberZipCode;
                    string calendarAddress = "";
                    //Use Location based address instead of winery address if provided for location
                    if ((reservationData.locAddress1 != null))
                    {
                        if (reservationData.locAddress1.Trim().Length > 0)
                        {
                            lAddress1 = (reservationData.locAddress1 == null ? "" : reservationData.locAddress1);
                            lAddress2 = (reservationData.locAddress2 == null ? "" : reservationData.locAddress2);
                            lCity = (reservationData.locCity == null ? "" : reservationData.locCity);
                            lState = (reservationData.locState == null ? "" : reservationData.locState);
                            lZip = (reservationData.locZip == null ? "" : reservationData.locZip);
                        }
                    }

                    //Calendar address
                    calendarAddress += lAddress1 + "\\n ";
                    if (lAddress2.Trim().Length > 0)
                    {
                        calendarAddress += lAddress2 + "\\n ";
                    }
                    calendarAddress += lCity + ", " + lState + " " + lZip + "\\n ";

                    string lFullAddress = string.Empty;
                    lFullAddress = string.Format("{0}<br>{1}{2}, {3} {4}", lAddress1, string.IsNullOrEmpty(lAddress2) ? "" : lAddress2 + "<br>", lCity, lState, lZip);

                    //Replace here so that we remove it if it's blank
                    string DirectionsURL = "";
                    if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
                    {
                        DirectionsURL = reservationData.MapAndDirectionsURL;
                    }
                    string locationSection = "";
                    string locationHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #ffffff;width: 300px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>[[DestinationName]]</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[LocationAddress]]</span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[MemberPhone]]</span></p> <p style=\"font-size: 14px; line-height: 140%;\"> </p> </div> </td> </tr> </tbody> </table> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[DirectionsURL]]\" style=\"height:48px; v-text-anchor:middle; width:238px;\" arcsize=\"8.5%\" strokecolor=\"#236fa1\" strokeweight=\"1px\" fillcolor=\"#ffffff\"><w:anchorlock/><center style=\"color:#236fa1;font-family:'Poppins', sans-serif;;\"><![endif]--> <a href=\"[[DirectionsURL]]\" target=\"_blank\" class=\"v-button\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #236fa1; background-color: #ffffff; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:100%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;border-top-color: #236fa1; border-top-style: solid; border-top-width: 1px; border-left-color: #236fa1; border-left-style: solid; border-left-width: 1px; border-right-color: #236fa1; border-right-style: solid; border-right-width: 1px; border-bottom-color: #236fa1; border-bottom-style: solid; border-bottom-width: 1px;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"font-size: 14px; line-height: 16.8px; font-family: Poppins, sans-serif;\">GET DIRECTIONS</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #ffffff;width: 300px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;bordear-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div> [[MapURL]] </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string zoomHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Zoom Meeting Information</strong></span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">We are using Zoom to host our virtual tastings which will require you to take some additional steps to ensure you can connect to our virtual tasting without any delays. This will require you to enter the Zoom MeetingID and Zoom passport assigned to you.</span></p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[ZoomMeeting]] </span></p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>[[MemberName]] Technical Support</strong></span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">Should you require any assistance with connecting to our virtual tasting, please contact us immediately at [[MemberPhone]]</span></p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p> <p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Zoom Technical Support</strong></span><br /><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">If you have questions or need additional assistance with using Zoom, please contact their <a rel=\"noopener\" href=\"https://support.zoom.us/hc/en-us\" target=\"_blank\">Technical Support</a>.</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string mapHtml = "<table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"> <tr> <td style=\"padding-right: 0px;padding-left: 0px;\" align=\"center\"> <a href=\"[[map_and_directions_url]]\" target=\"_blank\"> <img align=\"center\" border=\"0\" src=\"[[mapInageURl]]\" alt=\"Map Image\" title=\"Map Image\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 100%;max-width: 280px;\" width=\"280\" /> </a> </td> </tr> </table> </td> </tr> </tbody> </table>";

                    //'Replace tags in location html
                    locationHtml = locationHtml.Replace("[[DestinationName]]", DestinationName);
                    locationHtml = locationHtml.Replace("[[LocationAddress]]", lFullAddress);
                    locationHtml = locationHtml.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));
                    locationHtml = locationHtml.Replace("[[DirectionsURL]]", DirectionsURL);

                    mapHtml = mapHtml.Replace("[[map_and_directions_url]]", map_and_directions_url);
                    mapHtml = mapHtml.Replace("[[mapInageURl]]", mapInageURl);

                    locationHtml = locationHtml.Replace("[[MapURL]]", mapHtml);

                    //'by default it's set to location
                    locationSection = locationHtml;

                    string zoomContent = string.Empty;

                    ZoomMeetingInfo zoomMeetingInfo = eventDAL.GetZoomMeetingInfo(reservationData.slotid, reservationData.slottype, reservationData.EventDate, reservationData.ReservationId);

                    if (zoomMeetingInfo != null && zoomMeetingInfo.Id > 0)
                    {
                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)Common.Common.SettingGroup.member);


                        bool requirePassword = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_zoom_require_password);
                        string zoomMeetingPassword = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_zoom_password);

                        if (requirePassword && !string.IsNullOrEmpty(zoomMeetingPassword))
                        {
                            zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}<br>Password: {3}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()), zoomMeetingPassword);
                        }
                        else
                        {
                            zoomContent = string.Format("<p>Join our {0} on Zoom.</p><p>Click or copy + paste the following link<br>{1}</p><p> Meeting ID:  {2}</p>", reservationData.EventName, zoomMeetingInfo.MeetingURL, Utility.FormatZoomMeetingId(zoomMeetingInfo.MeetingId.ToString()));
                        }
                    }

                    //'if set to show zoom we use it instead
                    if (sendPreviewWithZoom)
                    {
                        //'Replace tags in Zoom html
                        zoomHtml = zoomHtml.Replace("[[ZoomMeeting]]", zoomContent);
                        zoomHtml = zoomHtml.Replace("[[MemberName]]", DestinationName);
                        zoomHtml = zoomHtml.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(reservationData.MemberPhone.ToString(), "US"));

                        locationSection = zoomHtml;
                    }

                    string notesSection = "";

                    //Preview sample messages
                    string personalMSg = model.perMsg;
                    string guestNote = reservationData.Notes;

                    //NOTES

                    string notesSectionHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"font-size: 14px; line-height: 140%;\">[[Notes]]</p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                    string notesCombined = "";
                    string personlMsgHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Personal Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[Personal_Message]]</span><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"></span></p>";
                    string guestNoteHtml = "<p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\"><strong>Guest Note</strong></span></p> <p style=\"font-size: 14px; line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; font-size: 14px; line-height: 19.6px;\">[[RsvpNote]]</span></p>";

                    //If either personal Message or guest note is available we need to render this section
                    bool showNoteSection = false;

                    if (!string.IsNullOrEmpty(personalMSg))
                    {
                        showNoteSection = true;
                        //Replace message tags
                        personlMsgHtml = personlMsgHtml.Replace("[[Personal_Message]]", personalMSg);
                        //Add to notes
                        notesCombined += personlMsgHtml;
                    }

                    if (!string.IsNullOrEmpty(guestNote))
                    {
                        showNoteSection = true;

                        //Replace message tags
                        guestNoteHtml = guestNoteHtml.Replace("[[RsvpNote]]", guestNote);

                        //If personl msg is not empty add this space html first before adding the guest note to create some separation
                        if (notesCombined.Length > 0)
                        {
                            notesCombined += "<p style=\"font-size: 14px; line-height: 120%;\"> </p>";
                        }

                        //Add to notes
                        notesCombined += guestNoteHtml;
                    }

                    //If either personal or guest note was provided this should be true and we combine it all.
                    if (showNoteSection)
                    {
                        //replace tag in notes section html with notes combined
                        notesSectionHtml = notesSectionHtml.Replace("[[Notes]]", notesCombined);
                        //Set noteSection to Html
                        notesSection = notesSectionHtml;
                    }

                    string inviteSection = "";
                    bool hasInvite = reservationData.HasInvite;

                    if (hasInvite)
                    {
                        string inviteHtml = "<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 20px 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"line-height: 140%;\"><strong>IMPORTANT!</strong></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 0px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table class=\"c-personal-note\" style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <div style=\"line-height: 140%; text-align: left; word-wrap: break-word;\"> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\"><strong>Your reservation is currently pending and requires additional action to be confirmed.</strong><br /></span></p> <p style=\"line-height: 140%;\"> </p> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\">You must take immediate action by [[ExpirationDateTime]] in order to confirm your appointment or your reservation will be automatically cancelled and released to others.</span></p> <p style=\"line-height: 140%;\"><span style=\"font-family: Poppins, sans-serif; line-height: 19.6px;\"><br />Click on the 'Complete Reservation' button to complete your reservation.</span></p> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[CompeteRsvpLink]]\" style=\"height:47px; v-text-anchor:middle; width:540px;\" arcsize=\"8.5%\" stroke=\"f\" fillcolor=\"#1069b0\"><w:anchorlock/><center style=\"color:#FFFFFF;font-family:'Poppins', sans-serif;;\"><![endif]--> <a href=\"[[CompleteRSVPLink]]\" target=\"_blank\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #FFFFFF; background-color: #1069b0; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:100%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;font-size: 14px;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"line-height: 16.8px;\">COMPLETE RESERVATION</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div> <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 1px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <tbody> <tr style=\"vertical-align: top\"> <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\"> <span>&#160;</span> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                        DateTime expirationdateTime = reservationData.ReservationInviteExpirationDateTime;

                        inviteHtml = inviteHtml.Replace("[[ExpirationDateTime]]", String.Format("{0} {1}", expirationdateTime.ToShortDateString(), expirationdateTime.ToString("hh:mm tt")));
                        inviteHtml = inviteHtml.Replace("[[CompleteRSVPLink]]", "https://www.cellarpass.com");
                        inviteSection = inviteHtml;
                    }

                    EmailTemplates MailTemplate = EmailTemplates.InviteReminder;

                    string PhoneNumber = "";


                    string RsvpGuestContent = "";
                    string GuestsAttending = "";
                    DateTime StartDate = EventDate.Add(StartTime);
                    DateTime EndDate = EventDate.Add(EndTime);
                    TimeSpan Duration = EndDate.Subtract(StartDate);
                    string RsvpGuestSubjectContent = "";

                    string timezoneName = reservationData.timezone_name;

                    if (!string.IsNullOrEmpty(timezoneName))
                    {
                        timezoneName = " (" + timezoneName + ")";
                    }
                    else
                    {
                        timezoneName = "";
                    }

                    //Get Affiliate Information
                    string AffiliateEmail = "";
                    string AffiliateName = "";
                    string AffiliateCompany = "";

                    int AffID = AffiliateID;

                    if (AffID > 0)
                    {
                        try
                        {
                            var affiliate = eventDAL.GetUser(reservationData.AffiliateID);
                            if (affiliate != null)
                            {
                                AffiliateEmail = affiliate.AffiliateEmail;
                                AffiliateName = affiliate.AffiliateName;
                                AffiliateCompany = affiliate.AffiliateCompany;
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                    }

                    //Format Fee

                    string FormatFee = reservationData.Fee.ToString("C", new CultureInfo("en-US"));
                    string DepositPolicy = GetDepositPoliciesEmail().Where(a => a.ID == reservationData.ChargeFee).FirstOrDefault().Name;

                    //Phone Number - check and use home first then work if phone is empty
                    if ((GuestPhone != null))
                    {
                        if (!string.IsNullOrEmpty(GuestPhone))
                        {
                            PhoneNumber = GuestPhone;
                        }
                        else
                        {
                            if ((GuestWPhone != null))
                            {
                                if (!string.IsNullOrEmpty(GuestWPhone))
                                {
                                    PhoneNumber = GuestWPhone;
                                }
                            }
                        }
                    }

                    EmailContent ew = default(EmailContent);
                    //get business message
                    //EmailContent ew1 = emailDAL.GetEmailContent((int)MailTemplate, WineryID);
                    ew = emailDAL.GetEmailContent((int)MailTemplate, 0);
                    int isfaq = emailDAL.CheckFAQExistsForWinery(WineryID);

                    if ((ew != null))
                    {

                        if (ew.Active == true)
                        {
                            //Need to get the guests attending each reservation in the booking
                            if (ReservationId > 0)
                            {
                                //Get Guests Detail
                                GuestsAttending = eventDAL.GetGuestAttending(ReservationId, reservationData.GuestName);

                            }

                            //Configure/Format CancellationPolicy
                            string CancellationPolicy = "";
                            string CancelByDate = "";
                            if (Content != null)
                            {
                                if (Content.Trim().Length > 0)
                                {
                                    CancellationPolicy = Content;

                                    if (reservationData.CancelLeadTime > 50000)
                                    {
                                        CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", "We’re sorry, but this reservation cannot be cancelled or scheduled.");
                                    }
                                    else
                                    {
                                        CancellationPolicy = CancellationPolicy.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                                    }
                                }
                            }

                            //Use Location based address instead of winery address if provided for location
                            if ((locAddress1 != null))
                            {
                                if (locAddress1.Trim().Length > 0)
                                {
                                    lAddress1 = locAddress1;
                                    lAddress2 = locAddress2;
                                    lCity = locCity;
                                    lState = locState;
                                    lZip = locZip;
                                }
                            }


                            //Replace Content Tags
                            RsvpGuestSubjectContent = ew.EmailSubject;
                            RsvpGuestContent = ew.EmailBody;

                            //Subject
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingCode]]", BookingCode);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DestinationName]]", DestinationName);

                            RsvpGuestSubjectContent = ReservationTagReplace(RsvpGuestSubjectContent, bookingGUID);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestName]]", GuestName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestEmail]]", GuestEmail);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestPhone]]", PhoneNumber);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestCount]]", Convert.ToString(GuestCount));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[GuestsAttending]]", GuestsAttending);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Fee]]", FormatFee);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[Gratuity]]", GratuityContent);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[PaymentStatus]]", PaymentDetails);

                            string bannerImageHtml = Utility.GetBannerImageHtmlByWineryId(reservationData.WineryID);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberName]]", MemberName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(MemberPhone.ToString(), "US"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberEmail]]", MemberEmail);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress1]]", lAddress1);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddress2]]", lAddress2);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressCity]]", lCity);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressState]]", lState);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[MemberAddressZipCode]]", lZip);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[LocationAddress]]", lFullAddress);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDuration]]", Duration.ToString());
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpEvent]]", EventName);
                            //RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpHost]]", Host)
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpDestination]]", DestinationName);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpLocation]]", EventLocation);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[NotesSection]]", notesSection);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[InviteSection]]", inviteSection);

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(Notes) ? "None" : Notes));
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[RsvpStatus]]", GetReservationStatus(Status));

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CancellationPolicy]]", CancellationPolicy);

                            string DirectionsURL2 = "";
                            if (reservationData.MapAndDirectionsURL.Trim().Length > 0)
                            {
                                //DirectionsURL = "<a href=\"" + reservationData.MapAndDirectionsURL.Trim() + "\" target=\"_blank\">Get Directions</a>";
                                DirectionsURL2 = Utility.GenerateEmailButton("Get Directions", reservationData.MapAndDirectionsURL.Trim(), "#47bf12", "#47bf12", "15px", "15px", "#ffffff");
                            }

                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[DirectionsURL]]", DirectionsURL2);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[ZoomMeeting]]", zoomContent);
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));

                            //Replace double ,, or , , with , (usually because of address)
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace(",,", ",");
                            RsvpGuestSubjectContent = RsvpGuestSubjectContent.Replace(", ,", ",");

                            //Body
                            RsvpGuestContent = RsvpGuestContent.Replace("[[BookingCode]]", BookingCode);

                            RsvpGuestContent = ReservationTagReplace(RsvpGuestContent, bookingGUID);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[DestinationName]]", reservationData.DestinationName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[BookingDate]]", LocalBookingDate.ToString("MM/dd/yyyy hh:mm tt"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestName]]", GuestName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestEmail]]", GuestEmail);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestPhone]]", PhoneNumber);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestCount]]", Convert.ToString(GuestCount));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[GuestsAttending]]", GuestsAttending);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Fee]]", FormatFee);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[FeePaid]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePaid));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[FeePerPerson]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.FeePerPerson));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[SalesTax]]", string.Format(new CultureInfo("en-US"), "{0:C}", reservationData.SalesTax));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Gratuity]]", GratuityContent);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[PaymentStatus]]", PaymentDetails);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberBannerImage]]", bannerImageHtml);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberName]]", MemberName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberPhone]]", Utility.FormatTelephoneNumber(MemberPhone.ToString(), "US"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberEmail]]", MemberEmail);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress1]]", lAddress1);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddress2]]", lAddress2);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressCity]]", lCity);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressState]]", lState);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberAddressZipCode]]", lZip);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[LocationAddress]]", lFullAddress);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDate]]", StartDate.ToString("dddd, MMMM dd, yyyy"));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpTime]]", string.Format("{0:hh:mm tt}{1}", StartDate, timezoneName));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancelLeadTime]]", Email.GetLeadTimeTextByValue(CancelLeadTime));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDuration]]", Duration.ToString());
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpEvent]]", EventName);
                            //RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpHost]]", Host)
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpDestination]]", DestinationName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpLocation]]", EventLocation);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[NotesSection]]", notesSection);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[InviteSection]]", inviteSection);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpNote]]", (string.IsNullOrEmpty(Notes) ? "None" : Notes));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[RsvpStatus]]", GetReservationStatus(Status));
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateName]]", AffiliateName);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AffiliateCompany]]", AffiliateCompany);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationPolicy]]", CancellationPolicy);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[ZoomMeeting]]", zoomContent);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CompanyLogo]]", Utility.GenerateLogoImageTag(reservationData.WineryID));
                            string strpath = "https://typhoon.cellarpass.com/";
                            if (ConnectionString.IndexOf("live") > -1)
                                strpath = "https://www.cellarpass.com/";

                            string profileUrl = string.Format("{1}business/{0}", reservationData.ProfileUrl, strpath);

                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[MemberLinkButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[MemberLinkButton]]", profileUrl);
                            RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[MemberLinkButton]]", profileUrl);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[MemberLinkButton]]", profileUrl);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[DirectionsURL]]", DirectionsURL);

                            if (isfaq == 1)
                            {
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl + "?f=1");
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl + "?f=1");

                                RsvpGuestContent = RsvpGuestContent.Replace("[[FAQButton]]", profileUrl + "?f=1");
                            }
                            else
                            {
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                                RsvpGuestContent = RsvpGuestContent.Replace("https://www.cellarpass.com/[[FAQButton]]", profileUrl);
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/admin/[[FAQButton]]", profileUrl);
                                RsvpGuestContent = RsvpGuestContent.Replace("https://dev.cellarpass.com/[[FAQButton]]", profileUrl);

                                RsvpGuestContent = RsvpGuestContent.Replace("[[FAQButton]]", profileUrl);
                            }

                            RsvpGuestContent = RsvpGuestContent.Replace(",,", ",");
                            RsvpGuestContent = RsvpGuestContent.Replace(", ,", ",");

                            SettingsDAL settingsDAL = new SettingsDAL(ConnectionString);

                            List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reservationData.WineryID, (int)SettingGroup.member);
                            string member_rsvp_contact_email = Settings.GetStrValue(settingsGroup, SettingKey.member_rsvp_contact_email);
                            string businessMessage = Settings.GetStrValue(settingsGroup, SettingKey.member_rsvp_review_business_message);

                            if (string.IsNullOrEmpty(businessMessage))
                            {
                                businessMessage = "Thank you for booking one of our experiences. We would be more than happy to host you again and look forward to your next visit.";
                            }

                            RsvpGuestContent = RsvpGuestContent.Replace("[[BusinessMessage]]", businessMessage);
                            //RsvpGuestContent = RsvpGuestContent.Replace("[[OrganizerMessage]]", OrganizerMsg);
                            string viewItineraryHtml = "";

                            if (!string.IsNullOrWhiteSpace(reservationData.ItineraryGUID))
                            {
                                viewItineraryHtml = " <div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\"> <div class=\"u-row\" style=\"Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\"> <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\"> <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]--> <!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"background-color: #ffffff;width: 600px;padding: 10px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\" valign=\"top\"><![endif]--> <div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\"> <div style=\"background-color: #ffffff;height: 100%;width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--[if (!mso)&(!IE)]><!--> <div style=\"box-sizing: border-box; height: 100%; padding: 10px 20px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;\"> <!--<![endif]--> <table style=\"font-family:'Poppins', sans-serif;;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\"> <tbody> <tr> <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:'Poppins', sans-serif;;\" align=\"left\"> <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]--> <div align=\"center\"> <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"[[ViewItineraryURL]]\" style=\"height:47px; v-text-anchor:middle; width:516px;\" arcsize=\"8.5%\" strokecolor=\"#e03e2d\" strokeweight=\"1px\" fillcolor=\"#ffffff\"><w:anchorlock/><center style=\"color:#e03e2d;font-family:'Poppins', sans-serif;\"><![endif]--> <a href=\"[[ViewItineraryURL]]\" target=\"_blank\" class=\"v-button\" style=\"box-sizing: border-box;display: inline-block;font-family:'Poppins', sans-serif;;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #e03e2d; background-color: #ffffff; border-radius: 4px;-webkit-border-radius: 4px; -moz-border-radius: 4px; width:96%; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;border-top-width: 1px; border-top-style: solid; border-top-color: #e03e2d; border-left-width: 1px; border-left-style: solid; border-left-color: #e03e2d; border-right-width: 1px; border-right-style: solid; border-right-color: #e03e2d; border-bottom-width: 1px; border-bottom-style: solid; border-bottom-color: #e03e2d;\"> <span style=\"display:block;padding:15px 20px;line-height:120%;\"><span style=\"font-size: 14px; line-height: 16.8px; font-family: Poppins, sans-serif;\">VIEW ITINERARY</span></span> </a> <!--[if mso]></center></v:roundrect><![endif]--> </div> </td> </tr> </tbody> </table> <!--[if (!mso)&(!IE)]><!--> </div> <!--<![endif]--> </div> </div> <!--[if (mso)|(IE)]></td><![endif]--> <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]--> </div> </div> </div>";
                                viewItineraryHtml = viewItineraryHtml.Replace("[[ViewItineraryURL]]", string.Format("{1}itinerary/{0}?v=agenda", reservationData.ItineraryGUID, strpath));
                            }

                            RsvpGuestContent = RsvpGuestContent.Replace("[[ViewItinerary]]", viewItineraryHtml);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[Add-Ons]]", addOnItems);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[AddOnItems]]", AddOnDetails);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[DepositPolicy]]", DepositPolicy);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[Personal_Message]]", "None");
                            RsvpGuestContent = RsvpGuestContent.Replace("[[LocationSection]]", locationSection);

                            RsvpGuestContent = RsvpGuestContent.Replace("[[ConfirmationMessage]]", reservationData.EventConfirmationMessage);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[CancellationMessage]]", reservationData.EventCancellationMessage);
                            RsvpGuestContent = RsvpGuestContent.Replace("[[MapURl]]", MapURL);
                            //Directions URL
                            //string DirectionsURL = "";
                            //Send Mail

                            if (!string.IsNullOrEmpty(model.CCGuestEmail))
                                GuestEmail = model.CCGuestEmail;

                            response = await Task.Run(() => SendEmailAndSaveEmailLog(model.MailConfig, Email.EmailType.RsvpReminder, model.RsvpId, ew.EmailFrom, GuestEmail, RsvpGuestSubjectContent, RsvpGuestContent, reservationData.WineryID, null, ew.Id, member_rsvp_contact_email, DestinationName));

                            eventDAL.SetReservationInviteReminderStatus(ReservationId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.AppError, "TaskRsvpReminder", "SendCpRsvpReminderEmail: " + ex.ToString);
            }
            //#### RSVP Guest Email Reminder - END ####
            return response;
        }

        public string ReservationTagReplace(string RsvpContent, string BookingGUID)
        {
            string strpath = "https://typhoon.cellarpass.com/";
            if (ConnectionString.IndexOf("live") > -1)
                strpath = "https://www.cellarpass.com/";

            string CancelRSVPLink = string.Format("{0}rsvp-cancel?bookingCode={1}", strpath, BookingGUID);
            string ModifyRSVPLink = string.Format("{0}rsvp-confirmation/{1}", strpath, BookingGUID);
            string ViewRSVP = string.Format("{0}rsvp-complete?bookingcode={1}", strpath, BookingGUID);

            RsvpContent = RsvpContent.Replace("https://www.cellarpass.com/admin/[[CancelRSVPLink]]", CancelRSVPLink);
            RsvpContent = RsvpContent.Replace("https://www.cellarpass.com/admin/[[ModifyRSVPLink]]", ModifyRSVPLink);
            RsvpContent = RsvpContent.Replace("https://www.cellarpass.com/admin/[[ViewRSVP]]", ViewRSVP);

            RsvpContent = RsvpContent.Replace("https://www.cellarpass.com/[[CancelRSVPLink]]", CancelRSVPLink);
            RsvpContent = RsvpContent.Replace("https://www.cellarpass.com/[[ModifyRSVPLink]]", ModifyRSVPLink);
            RsvpContent = RsvpContent.Replace("https://www.cellarpass.com/[[ViewRSVP]]", ViewRSVP);

            RsvpContent = RsvpContent.Replace("https://dev.cellarpass.com/admin/[[CancelRSVPLink]]", CancelRSVPLink);
            RsvpContent = RsvpContent.Replace("https://dev.cellarpass.com/admin/[[ModifyRSVPLink]]", ModifyRSVPLink);
            RsvpContent = RsvpContent.Replace("https://dev.cellarpass.com/admin/[[ViewRSVP]]", ViewRSVP);

            RsvpContent = RsvpContent.Replace("https://dev.cellarpass.com/[[CancelRSVPLink]]", CancelRSVPLink);
            RsvpContent = RsvpContent.Replace("https://dev.cellarpass.com/[[ModifyRSVPLink]]", ModifyRSVPLink);
            RsvpContent = RsvpContent.Replace("https://dev.cellarpass.com/[[ViewRSVP]]", ViewRSVP);

            RsvpContent = RsvpContent.Replace("[[CancelRSVPLink]]", CancelRSVPLink);
            RsvpContent = RsvpContent.Replace("[[ModifyRSVPLink]]", ModifyRSVPLink);
            RsvpContent = RsvpContent.Replace("[[ViewRSVP]]", ViewRSVP);

            return RsvpContent;
        }
    }
}
