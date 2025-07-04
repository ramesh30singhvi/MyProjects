using System;

namespace CPReservationApi.Model
{
    public class Tickets_AbandonedModel
    {
        public int Id { get; set; }
        public int User_Id { get; set; }
        public string Email { get; set; }
        public int Event_Id { get; set; }
        public DateTime DateAdded { get; set; }
        public int Member_Id { get; set; }
        public bool SendEmailNotice { get; set; }
        public bool EmailSent { get; set; }
        public int ConvertedOrderId { get; set; }
        public string AcccessCode { get; set; }
        public string MemberName { get; set; }
        public string Promo { get; set; }
        public int ReferralId { get; set; }
        public bool isWidget { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EventTitle { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string EventOrganizerName { get; set; }
        public string VenueName { get; set; }
        public string VenueAddress { get; set; }
        public int TicketsEventId { get; set; }
        public int TimeZoneId { get; set; }
        public string RegionName { get; set; }
        public string PurchaseURL { get; set; }
        public string RegionUrl { get; set; }
        public string EventURL { get; set; }
    }
}
