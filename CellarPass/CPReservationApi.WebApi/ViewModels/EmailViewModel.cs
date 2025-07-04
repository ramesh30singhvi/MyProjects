using CPReservationApi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CPReservationApi.Common.Email;

namespace CPReservationApi.WebApi.ViewModels
{
    public class EmailViewModel
    {
        public EmailType EmailType { get; set; }
        public int TemplateId { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string FromEmail { get; set; }
        public string EmailBody { get; set; }
        public bool IsOnline { get; set; }
        public string DomainURL { get; set; }
        public string Comments { get; set; }
        public string PreviewEmail { get; set; }
        public int OffsetHours { get; set; }
        public string InternalNotficationEmail { get; set; }
        public string AdminEmailSubject { get; set; }
        public string CurrentUser { get; set; }
        public int ReferenceId { get; set; }
        public int MemberId { get; set; }
        public EmailAttachment EmailAttachment { get; set; }
        public string MessageContent { get; set; }
    }
}
