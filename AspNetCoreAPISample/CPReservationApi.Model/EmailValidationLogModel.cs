using CPReservationApi.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class EmailValidationLogModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public int UserId { get; set; }
        public EmailValidStatus EmailStatus { get; set; }
        public EmailStatusType StatusType { get; set; }
        public EmailWebhookEvent WebhookEvent { get; set; }
        public DateTime LogDate { get; set; }
        public bool IsValid { get; set; }
    }
}
