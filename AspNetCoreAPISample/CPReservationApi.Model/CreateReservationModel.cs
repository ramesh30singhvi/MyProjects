using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class CreateReservationModel
    {
        public int ReservationId { get; set; }
        public int EventId { get; set; }
        public int WineryId { get; set; }
        public string EventName { get; set; }
        public string EventLocation { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal FeePerPerson { get; set; }
        public int ChargeFee { get; set; }
        public bool RequireCreditCard { get; set; } = false;
        public int UserId { get; set; } = 0;
        public int TotalGuests { get; set; }
        public int FeeType { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public string Note { get; set; }
        public int ReferralType { get; set; }
        public string BookingCode { get; set; }
        public int PayType { get; set; }
        public string PayCardType { get; set; }
        public string PayCardNumber { get; set; }
        public string PayCardCustName { get; set; }
        public string PayCardExpMonth { get; set; }
        public string PayCardExpYear { get; set; }
        public string PayCardToken { get; set; }
        public string PayCardLastFourDigits { get; set; }
        public string PayCardFirstFourDigits { get; set; }
        public int Status { get; set; }
        public int CompletedGuestCount { get; set; }
        public int BookedById { get; set; }
        public string ConciergeNote { get; set; }
        public bool WaiveFee { get; set; }
        public int AffiliateID { get; set; }
        public int ReferralID { get; set; }
        public bool ReturningGuest { get; set; }
        public int EmailContentID { get; set; }
        public int WineryReferralId { get; set; }
        public decimal FeeDue { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal PurchaseTotal { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Zip { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int SlotId { get; set; }
        public int SlotType { get; set; }
        public decimal DiscountAmt { get; set; }
        public string DiscountDesc { get; set; }
        public string InternalNote { get; set; }
        public string Country { get; set; }
        public int CustomerType { get; set; }
               
        public string BookedByName { get; set; }
        public string CreditCardReferenceNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int LocationId { get; set; }
        public decimal SalesTax { get; set; }
        public int DiscountId { get; set; }
        public string DiscountCode { get; set; }
        public decimal DiscountCodeAmount { get; set; }
        public decimal SalesTaxPercentage { get; set; }
        public string HDYH { get; set; }
        public string MobilePhone { get; set; }
        public int MobilePhoneStatus { get; set; }
        public string Tags { get; set; } = "";
        public int PreAssign_Server_Id { get; set; }
        public string PreAssign_Table_Id { get; set; }
        public int privateaddongroup { get; set; }
        public int CancelLeadTime { get; set; }

        public int TransportationId { get; set; }

        public string TransportationName { get; set; }

        public decimal GratuityAmount { get; set; }

        public List<AttendeeQuestion> AttendeeQuestions { get; set; }
        public List<ActivationCodesModel> activation_codes { get; set; }

        public string WaitListGuid { get; set; }
        public string ContactTypes { get; set; }

        public string AccessCode { get; set; } = "";

        public int FloorPlanId { get; set; } = 0;
        public bool IgnoreDiscount { get; set; } = false;
        public string PersonalMessage { get; set; } = "";
    }
}
