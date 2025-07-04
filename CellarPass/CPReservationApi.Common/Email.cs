using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.Common
{
    public class Email
    {
        public enum EmailTemplates
        {
            SystemTest = -1,
            Generic = -2,
            //Used for sending one off emails
            NewUserSys = 0,
            //This goes out to the user when a new user is created for the first time - from CP
            NewGuestAmin = 1,
            //This goes to the Winery when a guest books with them for the first time or is attached - from Winery or CP
            NewGuestSys = 2,
            //This email goes out to the Guest when a guest creates an account for the first time - from CP
            RsvpGuest = 3,
            //This email goes out to Guest when they book a rsvp - from each Winery
            RsvpAdmin = 4,
            //This email goes out to Winery when a guest books a rsvp - from Winery or CP
            RsvpSys = 5,
            //This email goes out to Guest when a guest books a rsvp  - from CP
            ForgotPasswordGuestSys = 6,
            //This email goes out to Guest  - from CP
            ForgotPasswordUserSys = 7,
            //This email goes out to CP Users (admins) - from CP
            RsvpGuestCancel = 8,
            //This email goes out to Guest when they cancel a rsvp - from each Winery
            RsvpSysCancel = 9,
            //This email goes out to Guest when a they cancel a rsvp  - from CP
            ShareWithFriend = 10,
            //This email goes out to Friends from the Member on receipt page  - from CP
            RsvpReminder = 11,
            //This email will remind guests of their rsvp
            RsvpFollowUp = 12,
            //This email will follow up after the rsvp
            RsvpSys_Mobile = 13,
            //This email goes out to Guest when a guest books a rsvp through mobile  - from CP
            ReviewInvitation = 14,
            //This email goes out to CP Users to invite them to leave a review - from CP
            RsvpGuestMember = 15,
            //This email goes out to Guest when they book a rsvp - from each winery - Multiple templates allowed and created by Member
            RsvpGuestWineReview = 16,
            //This email goes out to guest/Winery Admin when guest submits the review
            RsvpTicketSalesConfirmation = 17,
            //This email goes out to Ticket Sales Confirmation
            TicketWaitlistOffer = 18,
            //This email will offer waitlist attendee to purchase tickets
            AccountVerification = 19,
            //This email will go out to verify email address
            TicketEventReminder = 20,
            //This email will remind guests of the ticket event they have tickets for. System only
            NoShowNotification = 21,
            SysClaimPageRequest = 22,
            //This email goes out when a someone wants to claim a business page
            InvitePreview = 23,
            CheckInPromoOffer = 24,
            TicketPostCapture = 25,
            SysSubscriptionSignup = 26,
            TicketBuyerInviteReminder = 27,
            //Reminder email to ticket buyer to send out invites - Can be cloned
            BillingInvoice = 28,
            //For sendign billing invoices to clients
            TicketRemindMe = 29,
            //For sending to users you have asked to be reminded about an event.
            EventReviewInvitation = 30,
            //System sends this to invite someone to leave an event review.
            EmailVerification = 31,
            //Sent when we need to verify the email address of the user
            TicketPayoutNotice = 32,
            //Sent when a payout is made
            TicketChargebackNotice = 33,
            //Sent when marked as chargeback
            TicketEventOrganizerReminder = 34,
            //The email reminds organizer of the event
            TicketEventOrganizerPost = 35,
            //The email is sent after the event to the organizer
            SysInactivityNotice = 36,
            //Used to send email to users who are inactive
            SysUserChangeRequest = 37,
            //User when a member wants to change user information that needs approval (concierge users for example)
            RsvpSalesReceipt = 38,
            //Used to send an rsvp sales receipt after payment
            AbandonedCartTickets = 39,
            AbandonedCartRsvp = 40,
            WaitListNotification = 41,
            WaitListCancellation = 42,
            SysUserInvite = 43,
            SysContactEventOrganizer = 44,
            SysCovidWaiver = 45,
            PrivateBookingRequest = 46,
            SMSReservationConfirmation = 47,
            SysInviteToGuestLinkProApp = 48,
            SysEmailAddressConfirmed = 49,
            TicketEventPublishedNotification = 50,
            TicketEventFollowingReminder = 51,
            SysNewAdminUser = 52,
            SysDailyReport = 53,
            SysTransferTicket = 54,
            InviteReminder = 55,
            RsvpExport = 56,
            TicketedEventEndedNotification = 57
        }

        public enum ReservationStatus
        {
            Pending = 0,
            Completed = 1,
            Cancelled = 2,
            NoShow = 3,
            Rescheduled = 4,
            GuestDelayed = 5,
            Updated = 6,
            YelpInitiated = 7,
            Initiated = 8,
            ReviewReceived = 10
        }

        public enum EmailType
        {
            NA = 0,
            Subscription = 1,
            Invoice = 2,
            VerifySubscription = 3,
            Rsvp = 4,
            RsvpAdmin = 5,
            RsvpAffiliate = 6,
            RsvpReminder = 7,
            NoShowNotice = 8,
            RsvpCancel = 9,
            PasswordReset = 10,
            TicketSale = 11,
            TicketReminder = 12,
            TicketBuyerReminder = 13,
            TicketRemindMe = 14,
            TicketPostOrganizer = 15,
            TicketWaitlistOffer = 16,
            RsvpReviewInvite = 17,
            Generic = 18,
            EventReviewInvite = 19,
            TicketPayoutNotice = 20,
            TicketEventInvite = 21,
            InvitePreview = 22,
            TicketEventChargeback = 23,
            TicketReminderOrganizer = 24,
            TicketPostCapture = 25,
            InactivityNotice = 26,
            RsvpUpdate = 27,
            AbanondedCartTickets = 28,
            AbanondedCartRsvp = 29,
            EventReviewInvitation = 30,
            WaitListNotification = 41,
            WaitListCancellation = 42,
            SysUserInvite = 43,
            SysContactEventOrganizer = 44,
            RsvpTicketSalesConfirmation = 45,
            CreateThirdPartyContact = 46,
            MailChimpOrder = 47,
            UploadOrderTobLoyal = 48,
            SysSubscriptionSignup = 49,
            SysCovidWaiver = 50,
            GoogleCalendar=99,
            PrivateEventRequest=51,
            CheckInPromoOffer=52,
            TicketEventFollowingReminder = 53,
            SysNewAdminUser = 54,
            EventsUpdateRequest = 55,
            InviteReminder = 56,
            RsvpExport = 57,
            MailChimpTicketOrder = 58,
            TicketedEventEndedNotification = 59,
            ReservationChangesUpdate = 60,
            CPMailChimpOrder = 61,
            Erroroccuredonadminv2 = 99
        }

        public enum EmailProvider
        {
            NA = 0,
            Mailgun = 1
        }
        public enum EmailStatus
        {
            na = 0,
            accepted = 1,
            delivered = 2,
            failed = 3,
            opened = 4,
            clicked = 5,
            unsubscribed = 6,
            complained = 7,
            stored = 8,
            dropped = 9,
            hardbounce = 10,
            cpInvalid = 11,
            added = 12,
            SmsSent=13
        }


        public static string GetLeadTimeTextByValue(int LeadTime)
        {

            string strRet = "";

            if (LeadTime > 4320)
            {
                int hours = (LeadTime / 60);
                strRet = (hours / 24).ToString() + " Days";
            }
            else if (LeadTime > 60)
            {
                strRet = (LeadTime / 60).ToString() + " Hours";
            }
            else if (LeadTime > 0)
            {
                strRet = LeadTime.ToString() + " Minutes";
            }

            return strRet;
        }

        public static string GetReservationStatus(int Status)
        {
            string sRetValue = "";
            switch (Status)
            {
                case (int)ReservationStatus.Pending:
                    sRetValue = "Confirmed";
                    break;
                case (int)ReservationStatus.Completed:
                    sRetValue = "Completed";
                    break;
                case (int)ReservationStatus.Cancelled:
                    sRetValue = "Cancelled";
                    break;
                case (int)ReservationStatus.NoShow:
                    sRetValue = "No Show";
                    break;
                default:
                    break;
            }
            return sRetValue;
        }

        public static bool EmailIsValid(string Email)
        {

            string pattern = "\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z";
            Match match = Regex.Match(Email.ToLower().Trim(), pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public class EmailLog
        {
            public int logId { get; set; }
            public int referenceId { get; set; }
            public EmailType emailType { get; set; }
            public EmailProvider emailProvider { get; set; }
            public EmailStatus emailStatus { get; set; }
            public string emailSender { get; set; }
            public string emailRecipient { get; set; }
            public string logNote { get; set; }
            public DateTime logDate { get; set; }
            public int memberId { get; set; }
            public int userId { get; set; }
            public Common.TimeZone TimeZone { get; set; }

            private DateTime _logDateLocal;
        }

        public class EmailResponse
        {
            public bool emailSent { get; set; }
            public EmailStatus emailStatus { get; set; }
            public string emailRecipient { get; set; }
            public string message { get; set; }
        }

        public class MailGunMessagesResponse
        {
            public string id { get; set; }
            public string message { get; set; }
        }

        public class MailGunEmailValidationResponse
        {
            public string address { get; set; }
            public bool is_disposable_address { get; set; }
            public bool is_role_address { get; set; }
            public List<string> reason { get; set; }
            public string result { get; set; }
            public string risk { get; set; }
        }
        public static string GetEmailTypeDesc(EmailType emailType)
        {
            string desc = "";
            switch (emailType)
            {
                case Email.EmailType.Invoice:
                    desc = "Invoice";
                    break;
                case Email.EmailType.Subscription:
                    desc = "Subscription";
                    break;
                case Email.EmailType.VerifySubscription:
                    desc = "Verify Subscription Email";
                    break;
                case Email.EmailType.Rsvp:
                    desc = "RSVP Confirmation";
                    break;
                case Email.EmailType.RsvpAdmin:
                    desc = "RSVP Admin Confirmation";
                    break;
                case Email.EmailType.RsvpAffiliate:
                    desc = "RSVP Affiliate Confirmation";
                    break;
                case Email.EmailType.RsvpReminder:
                    desc = "RSVP Reminder";
                    break;
                case Email.EmailType.NoShowNotice:
                    desc = "No Show Notice";
                    break;
                case Email.EmailType.RsvpCancel:
                    desc = "RSVP Cancel";
                    break;
                case Email.EmailType.PasswordReset:
                    desc = "Password Reset";
                    break;
                case Email.EmailType.TicketSale:
                    desc = "Ticket Sale Confirmation";
                    break;
                case Email.EmailType.TicketReminder:
                    desc = "Ticket Reminder";
                    break;
                case Email.EmailType.TicketBuyerReminder:
                    desc = "Ticket Buyer Reminder";
                    break;
                case Email.EmailType.TicketRemindMe:
                    desc = "Ticket Remind Me";
                    break;
                case Email.EmailType.TicketPostCapture:
                    desc = "Ticket Post Capture";
                    break;
                case Email.EmailType.TicketWaitlistOffer:
                    desc = "Ticket Waitlist Offer";
                    break;
                case Email.EmailType.RsvpReviewInvite:
                    desc = "RSVP Review Invite";
                    break;
                case Email.EmailType.EventReviewInvite:
                    desc = "Event Review Invite";
                    break;
                case Email.EmailType.TicketPayoutNotice:
                    desc = "Ticket Payout Notice";
                    break;
                case Email.EmailType.InvitePreview:
                    desc = "Invite Preview";
                    break;
                case Email.EmailType.TicketEventChargeback:
                    desc = "Ticket Event Chargeback";
                    break;
                case Email.EmailType.TicketPostOrganizer:
                    desc = "Ticket Post Organizer";
                    break;
                case Email.EmailType.TicketEventInvite:
                    desc = "Ticket Event Invite";
                    break;
                case Email.EmailType.TicketReminderOrganizer:
                    desc = "Ticket Organizer Reminder";
                    break;
                case Email.EmailType.InactivityNotice:
                    desc = "System Inactivity Notice";
                    break;
                case Email.EmailType.WaitListNotification:
                    desc = "Waitlist Confirmation";
                    break;
                case Email.EmailType.WaitListCancellation:
                    desc = "Waitlist Cancellation Notice";
                    break;
                case Email.EmailType.CheckInPromoOffer:
                    desc = "Check In Promo Offer";
                    break;
            }
            return desc;
        }

        public static string GetEmailStatusDesc(EmailStatus emailStatus)
        {
            string desc = "";

            switch (emailStatus)
            {
                case Email.EmailStatus.accepted:
                    desc = "Accepted";
                    break;
                case Email.EmailStatus.clicked:
                    desc = "Clicked";
                    break;
                case Email.EmailStatus.complained:
                    desc = "Complaint";
                    break;
                case Email.EmailStatus.delivered:
                    desc = "Delivered";
                    break;
                case Email.EmailStatus.dropped:
                    desc = "Dropped";
                    break;
                case Email.EmailStatus.failed:
                    desc = "Failed";
                    break;
                case Email.EmailStatus.hardbounce:
                    desc = "Bounced";
                    break;
                case Email.EmailStatus.na:
                    desc = "NA";
                    break;
                case Email.EmailStatus.opened:
                    desc = "Opened";
                    break;
                case Email.EmailStatus.stored:
                    desc = "Stored";
                    break;
                case Email.EmailStatus.unsubscribed:
                    desc = "Unsubscribed";
                    break;
                case Email.EmailStatus.cpInvalid:
                    desc = "Email Invalid";
                    break;
                case Email.EmailStatus.added:
                    desc = "Queued";
                    break;
            }
            return desc;
        }
    }
    public class Reservations
    {
        public Reservations()
        {
            data = new ReservationCancelled();
        }
        public int EmailType { get; set; }
        public ReservationCancelled data { get; set; }
    }
    public class ReservationCancelled
    {
        public int ReservationId { get; set; }
        public int RsvpType { get; set; }
        public decimal RefundAmount { get; set; }
    }

    public class ShareFriends
    {
        public string share_friend_email { get; set; }
        public string share_friend_first_name { get; set; }
        public string share_friend_last_name { get; set; }
    }

    public class ReservationEmailModel
    {
        public ReservationEmailModel()
        {
            MailConfig = new MailConfig();
            RsvpId = 0;
            isMobile = false;
            SendCPEmail = true;
            SendToFriendMode = false;
            ShareMessage = "";
            share_friends = null;
            CCGuestEmail = "";
            SendAffiliateEmail = false;
            SendCCOnly = false;
            perMsg = "";
            alternativeEmailTemplate = 0;
            isRsvpType = 0;
        }
        public string BCode { get; set; }
        public int RsvpId { get; set; }
        public bool isMobile { get; set; }
        public bool SendCPEmail { get; set; }
        public bool GuestEmail { get; set; }
        public bool AdminEmail { get; set; }
        public bool SendToFriendMode { get; set; }
        public string ShareMessage { get; set; }
        public List<ShareFriends> share_friends { get; set; }
        public string CCGuestEmail { get; set; }
        public bool SendAffiliateEmail { get; set; }
        public bool SendCCOnly { get; set; }
        public string perMsg { get; set; }
        public int alternativeEmailTemplate { get; set; }
        public int isRsvpType { get; set; }
        public int EType { get; set; }
        public int UId { get; set; }
        public bool isRsvpUpdate { get; set; }
        public ReservationEmailModel data { get; set; }
        public MailConfig MailConfig { get; set; }
        public decimal RefundAmount { get; set; }
        public int ActionSource { get; set; }
        public string SendTo { get; set; }

        public bool SendCopyToGuest { get; set; } = false;

        public string InviteEmail { get; set; } = "";
    }
    public class Queue
    {
        public string QueueData { get; set; }
    }

    public class EmailQueue
    {
        public int UId { get; set; } = 0;
        public string BCode { get; set; } = "";
        public int RsvpId { get; set; }
        public bool GuestEmail { get; set; } = true;
        public bool AdminEmail { get; set; } = true;
        public int EType { get; set; }
        public string PerMsg { get; set; } = "";
        public int Src { get; set; }
        public decimal Ramt { get; set; } = 0;
        public int ActionType { get; set; } = 0;
        public bool AffiliateEmail { get; set; } = false;
        public string AlternativeEmail { get; set; } = "";
    }
    public class ClsStaticValues
    {
        private string _ID;
        private string _Name;

        private string _Attribute;
        public ClsStaticValues(string ID, string Name, string Attribute = "")
        {
            this._ID = ID;
            this._Name = Name;
            this._Attribute = Attribute;
        }

        public string ID
        {
            get { return this._ID; }
            set { this._ID = value; }
        }

        public string Name
        {
            get { return this._Name; }
            set { this._Name = value; }
        }

        public string Attribute
        {
            get { return this._Attribute; }
            set { this._Attribute = value; }
        }
    }

    public class SelectListItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class MailConfig
    {
        public string ApiKey { get; set; }
        public string Domain { get; set; }
        public string StorageConnectionString { get; set; }
    }
    public class EmailContent
    {
        public int Id { set; get; }

        //public int TemplateID { get; set; }

        //public int EmailFormat { get; set; }

        public string EmailFrom { get; set; }

        public string EmailSubject { get; set; }

        public string EmailBody { get; set; }

        //public int WineryID { get; set; }

        public bool Active { get; set; }

        //public DateTime dateCreated { get; set; }

        //public string createdByUser { get; set; }

        //public DateTime? dateModified { get; set; }

        //public string modifiedByUser { get; set; }

        public string EmailTo { get; set; }

        //public string EmailName { get; set; }

        //public bool SystemDefault { get; set; }

        public string EmailSubjectAdmin { get; set; }

        public string EmailBodyAdmin { get; set; }

        public string BusinessMessage { get; set; }
    }

    public class EmailAttachment
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _Contents;
        public string Contents
        {
            get { return _Contents; }
            set { _Contents = value; }
        }

        private byte[] _ContentBytes;
        public byte[] ContentBytes
        {
            get { return _ContentBytes; }
            set { _ContentBytes = value; }
        }
    }
}
