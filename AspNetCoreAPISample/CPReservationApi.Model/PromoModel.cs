using CPReservationApi.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CPReservationApi.Model
{
    public class PromoModel
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public string PromoName { get; set; }
        public string PromoContent { get; set; }
        public string PromoFinePrint { get; set; }
        public PromoZone PromoZone { get; set; }
        public PromoValue PromoValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public string ReferralCode { get; set; }
        public string RedemptionInstructions { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string MemberPhone { get; set; }

        public List<ClsStaticValues> GetPromoValues()
        {
            List<ClsStaticValues> list = new List<ClsStaticValues>();

            list.Add(new ClsStaticValues(PromoValue.Promo_2For1_unlimited.ToString(), "2 for 1 Tasting(s)", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_2For1_limit2.ToString(), "2 for 1 Tasting - limit 2", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_2For1_limit4.ToString(), "2 for 1 Tasting - limit 4", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_2For1_limit6.ToString(), "2 for 1 Tasting - limit 6", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_2For1_limit8.ToString(), "2 for 1 Tasting - limit 8", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_5PercentOffExperiences.ToString(), "5% Discount", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_10PercentOffExperiences.ToString(), "10% Discount", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_15PercentOffExperiences.ToString(), "15% Discount", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_20PercentOffExperiences.ToString(), "20% Discount", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_25PercentOffExperiences.ToString(), "25% Discount", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_30PercentOffExperiences.ToString(), "30% Discount", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_45PercentOffExperiences.ToString(), "45% Discount", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_50PercentOffExperiences.ToString(), "50% Discount", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_75PercentOffExperiences.ToString(), "75% Discount", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_100PercentOffExperiences.ToString(), "100% Discount", PromoType.Promo_2For1.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_FreeCheesePlate.ToString(), "Cheese Plate included with Reservation", PromoType.Promo_FreeUpgrade.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_FreeCharcuterie.ToString(), "Charcuterie included with Reservation", PromoType.Promo_FreeUpgrade.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_UpgradeToReserveTasting.ToString(), "Upgraded Tasting", PromoType.Promo_FreeUpgrade.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_5PercentOff.ToString(), "5% Off Purchase", PromoType.Promo_PurchaseDiscount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_10PercentOff.ToString(), "10% Off Purchase", PromoType.Promo_PurchaseDiscount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_15PercentOff.ToString(), "15% Off Purchase", PromoType.Promo_PurchaseDiscount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_20PercentOff.ToString(), "20% Off Purchase", PromoType.Promo_PurchaseDiscount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_25PercentOff.ToString(), "25% Off Purchase", PromoType.Promo_PurchaseDiscount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_30PercentOff.ToString(), "30% Off Purchase", PromoType.Promo_PurchaseDiscount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_45PercentOff.ToString(), "45% Off Purchase", PromoType.Promo_PurchaseDiscount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_50PercentOff.ToString(), "50% Off Purchase", PromoType.Promo_PurchaseDiscount.ToString()));

            list.Add(new ClsStaticValues(PromoValue.Promo_5DollarsOffRetail.ToString(), "$5 Off Retail Purchase", PromoType.Promo_PurchaseDiscount_Amount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_10DollarsOffRetail.ToString(), "$10 Off Retail Purchase", PromoType.Promo_PurchaseDiscount_Amount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_15DollarsOffRetail.ToString(), "$15 Off Retail Purchase", PromoType.Promo_PurchaseDiscount_Amount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_20DollarsOffRetail.ToString(), "$20 Off Retail Purchase", PromoType.Promo_PurchaseDiscount_Amount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_25DollarsOffRetail.ToString(), "$25 Off Retail Purchase", PromoType.Promo_PurchaseDiscount_Amount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_30DollarsOffRetail.ToString(), "$30 Off Retail Purchase", PromoType.Promo_PurchaseDiscount_Amount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_50DollarsOffRetail.ToString(), "$50 Off Retail Purchase", PromoType.Promo_PurchaseDiscount_Amount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_75DollarsOffRetail.ToString(), "$75 Off Retail Purchase", PromoType.Promo_PurchaseDiscount_Amount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_100DollarsOffRetail.ToString(), "$100 Off Retail Purchase", PromoType.Promo_PurchaseDiscount_Amount.ToString()));


            list.Add(new ClsStaticValues(PromoValue.Promo_5DollarsOff.ToString(), "$5 Off Tastings", PromoType.Promo_DiscountTasting.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_10DollarsOff.ToString(), "$10 Off Tastings", PromoType.Promo_DiscountTasting.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_15DollarsOff.ToString(), "$15 Off Tastings", PromoType.Promo_DiscountTasting.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_20DollarsOff.ToString(), "$20 Off Tastings", PromoType.Promo_DiscountTasting.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_25DollarsOff.ToString(), "$25 Off Tastings", PromoType.Promo_DiscountTasting.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_30DollarsOff.ToString(), "$30 Off Tastings", PromoType.Promo_DiscountTasting.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_40DollarsOff.ToString(), "$40 Off Tastings", PromoType.Promo_DiscountTasting.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_50DollarsOff.ToString(), "$50 Off Tastings", PromoType.Promo_DiscountTasting.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_60DollarsOff.ToString(), "$60 Off Tastings", PromoType.Promo_DiscountTasting.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_75DollarsOff.ToString(), "$75 Off Tastings", PromoType.Promo_DiscountTasting.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_100DollarsOff.ToString(), "$100 Off Tastings", PromoType.Promo_DiscountTasting.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_25PercentOffShipping.ToString(), "25% Discount Shipping", PromoType.Promo_ShippingDiscount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_50PercentOffShipping.ToString(), "50% Discount Shipping", PromoType.Promo_ShippingDiscount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_CompShipping.ToString(), "Shipping included with Purchase", PromoType.Promo_ShippingDiscount.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_CompGiftWithRsvp.ToString(), "Gift included with Reservation", PromoType.Promo_ComplimentaryGift.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_CompGiftWithTasting.ToString(), "Gift included with Tasting", PromoType.Promo_ComplimentaryGift.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_CompGiftWithPurchase.ToString(), "Gift included with Purchase", PromoType.Promo_ComplimentaryGift.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_CompGiftWithCheckin.ToString(), "Gift included with Check-in", PromoType.Promo_ComplimentaryGift.ToString()));
            list.Add(new ClsStaticValues(PromoValue.Promo_ClubMemberForaday.ToString(), "Club Member for a Day", PromoType.Promo_ClubMemberForDay.ToString()));

            return list;
        }

        public string PromoValueDesc(string proValue)
        {
            string _promoValueDesc = string.Empty;
            string valueDesc = GetPromoValues().Where(f => f.ID == proValue).FirstOrDefault().Name;
            if ((valueDesc != null))
            {
                _promoValueDesc = valueDesc;
            }
            return _promoValueDesc;
        }
    }

    public class PromoEmail
    {
        public PromoEmail()
        {
            Promo = new PromoModel();
            MailConfig = new MailConfig();
        }
        public string ToEmail { get; set; }
        public PromoModel Promo { get; set; }
        public MailConfig MailConfig { get; set; }
    }
    //private string _promoValueDesc = "";
    //public string PromoValueDesc
    //{
    //    get
    //    {
    //        string valueDesc = StaticItems.GetPromoValues().Where(f => f.ID == PromoValue).SingleOrDefault.Name;
    //        if ((valueDesc != null))
    //        {
    //            _promoValueDesc = valueDesc;
    //        }
    //        return _promoValueDesc;
    //    }
    //}
    public enum PromoZone
    {
        profilePage = 1,
        surveyInvite = 2,
        checkInRsvp = 3,
        checkInTicket = 4
    }
    public enum PromoType
    {
        Promo_2For1 = 1,
        Promo_FreeUpgrade = 2,
        Promo_PurchaseDiscount = 3,
        Promo_DiscountTasting = 4,
        Promo_ShippingDiscount = 5,
        Promo_ComplimentaryGift = 6,
        Promo_PurchaseDiscount_Amount = 7,
        Promo_ClubMemberForDay = 8
    }
    public enum PromoValue
    {
        Promo_2For1_limit2 = 1,
        Promo_2For1_limit4 = 2,
        Promo_2For1_limit6 = 3,
        Promo_2For1_limit8 = 4,
        Promo_2For1_unlimited = 5,
        Promo_5PercentOff = 6,
        Promo_10PercentOff = 7,
        Promo_15PercentOff = 8,
        Promo_20PercentOff = 9,
        Promo_25PercentOff = 10,
        Promo_30PercentOff = 11,
        Promo_45PercentOff = 12,
        Promo_50PercentOff = 13,
        Promo_FreeCheesePlate = 20,
        Promo_UpgradeToReserveTasting = 21,
        Promo_FreeCharcuterie = 22,
        Promo_5DollarsOff = 30,
        Promo_10DollarsOff = 31,
        Promo_15DollarsOff = 32,
        Promo_20DollarsOff = 33,
        Promo_25DollarsOff = 34,
        Promo_30DollarsOff = 35,
        Promo_40DollarsOff = 36,
        Promo_50DollarsOff = 37,
        Promo_60DollarsOff = 38,
        Promo_75DollarsOff = 39,
        Promo_100DollarsOff = 40,
        Promo_5DollarsOffRetail = 41,
        Promo_10DollarsOffRetail = 42,
        Promo_15DollarsOffRetail = 43,
        Promo_20DollarsOffRetail = 44,
        Promo_25DollarsOffRetail = 45,
        Promo_30DollarsOffRetail = 46,
        Promo_50DollarsOffRetail = 47,
        Promo_75DollarsOffRetail = 48,
        Promo_100DollarsOffRetail = 49,
        Promo_25PercentOffShipping = 50,
        Promo_50PercentOffShipping = 51,
        Promo_CompShipping = 52,
        Promo_CompGiftWithRsvp = 60,
        Promo_CompGiftWithTasting = 61,
        Promo_CompGiftWithPurchase = 62,
        Promo_CompGiftWithCheckin = 63,
        Promo_5PercentOffExperiences = 70,
        Promo_10PercentOffExperiences = 71,
        Promo_15PercentOffExperiences = 72,
        Promo_20PercentOffExperiences = 73,
        Promo_25PercentOffExperiences = 74,
        Promo_30PercentOffExperiences = 75,
        Promo_45PercentOffExperiences = 76,
        Promo_50PercentOffExperiences = 77,
        Promo_75PercentOffExperiences = 78,
        Promo_100PercentOffExperiences = 79,
        Promo_ClubMemberForaday = 90,
    }


}
