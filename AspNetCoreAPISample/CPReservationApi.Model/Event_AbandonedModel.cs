using System;

namespace CPReservationApi.Model
{
    public class Event_AbandonedModel
    {
        public int Id { get; set; }
        public int User_Id { get; set; }
        public string Email { get; set; }
        public int Member_Id { get; set; }
        public int Slot_Id { get; set; }
        public int Slot_Type { get; set; }
        public DateTime DateAdded { get; set; }
        public bool SendEmailNotice { get; set; }
        public bool EmailSent { get; set; }
        public int ConvertedReservationId { get; set; }
        public int GuestCount { get; set; }
        public int ReferralId { get; set; }
        public bool isWidget { get; set; }
        public DateTime DateRequested { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class ReservationSummaryModel
    {
        public string MemberName { get; set; }
        public string RegionName { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime EventDateEnd { get; set; }
        public string EventName { get; set; }
        public string LocationName { get; set; }
        public string PurchaseURL { get; set; }
        public string RegionUrl { get; set; }
        public string CancelPolicy { get; set; }
        public string DestinationName { get; set; }
    }
}
