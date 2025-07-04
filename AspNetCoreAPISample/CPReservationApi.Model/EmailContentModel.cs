using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class EmailContentModel
    {
        public int Id { set; get; }

        public int TemplateID { get; set; }

        public int EmailFormat { get; set; }

        public string EmailFrom { get; set; }

        public string EmailSubject { get; set; }

        public string EmailBody { get; set; }

        public int WineryID { get; set; }

        public bool Active { get; set; }

        public DateTime dateCreated { get; set; }

        public string createdByUser { get; set; }

        public DateTime? dateModified { get; set; }

        public string modifiedByUser { get; set; }

        public string EmailTo { get; set; }

        public string EmailName { get; set; }

        public bool SystemDefault { get; set; }

        public string EmailSubjectAdmin { get; set; }

        public string EmailBodyAdmin { get; set; }
    }

    public class FailedMailModel
    {
        public int ReservationId { get; set; }

        public string BookingCode { get; set; }

        public int UserId { get; set; }

        public int AffiliateID { get; set; }

        public int ReferralType { get; set; }
    }
}
