using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class EmailLogModel
    {
        public int MyProperty { get; set; }
        public int Id { get; set; }
        public int RefId { get; set; }
        public int EmailType { get; set; }
        public int EmailProvider { get; set; }
        public int EmailStatus { get; set; }
        public string EmailSender { get; set; }
        public string EmailRecipient { get; set; }
        public string LogNote { get; set; }
        public DateTime LogDate { get; set; }
        public int MemberId { get; set; }
        public int EmailContentId { get; set; }
    }
}
