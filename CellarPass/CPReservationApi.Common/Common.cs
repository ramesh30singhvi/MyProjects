using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.Common
{
    public static class Common
    {
        public const string InternalServerError = "Internal server error please try again";
        public static string ConnectionString
        {
            get;
            set;
        }

        public static string ConnectionString_tablepro
        {
            get;
            set;
        }
        public static string ConnectionString_readonly
        {
            get;
            set;
        }
        public enum ThirdPartyType
        {
            none = -1,
            usps = 0,
            vin65 = 1,
            eWinery = 2,
            sendGrid = 3,
            google = 4,
            bLoyal = 5,
            eCellar = 6,
            salesforce = 7,
            vintegrate = 8,
            OrderPort = 9,
            Commerce7 = 10,
            AMS = 11,
            Microworks = 12,
            ActiveClubSolutions = 13,
            Shopify = 14,
            BigCommerce=15,
            Coresense = 16
        }

        public enum InviteType
        {
            Survey = 1,
            Waiver = 2
        }

        public enum MessageType
        {
            Sales = 0,
            PassTheBuck = 1
        }

        public enum ChargeFee
        {
            NoFee = 0,
            UponArrival = 1,
            ComplimentaryWithPurchase = 3,
            ComplimentaryWithPurchaseCCRequired = 8,
            UponBooking = 4,
            TwentyFourtHoursPrior = 5,
            FortyEightHoursPrior = 6,
            ChargeUponArrival = 7,
            AutoChargeUponArrivalDateAMCCRequired = 9,
            AutoChargeUponBooking = 10,
            Charge25PercentDepositUponBooking = 11,
            Charge50PercentDepositUponBooking = 12
        }

        public enum VerificationType
        {
            NA = 0,
            User = 1,
            Subscription = 2
        }

        public enum VerificationFields
        {
            id,
            email,
            type
        }

        public enum BillingPlanTransactionType
        {
            PerPerson = 0,
            PerRsvp = 1,
            PerPersonPaidByGuest = 2,
            PerRsvpPaidByGuest = 3,
            PercentByHost = 4,
            PercentByGuest = 5
        }

        public enum BillingPlanType
        {
            Basic = 0,
            Professional = 1,
            Enterprise = 2,
            TablePro = 3

        }

        public enum Notification_Preference
        {
            email = 0,
            mobile = 1
        }

        public enum TicketWaitlistStatus
        {
            Pending = 0,
            Purchased = 1,
            Approved = 2,
            Cancelled = 3,
            Expired = 4
        }

        public enum Waitlist_Status
        {
            all = -1,
            pending = 0,
            approved = 1,
            converted = 2,
            canceled = 3,
            noresponse = 4,
            expired = 5
        }

        public enum Itinerary_Status
        {
            Initial = 0,
            Completed = 1,
            Onhold = 2,
            InActive = 3

        }

        public enum ItineraryBookingType
        {
            Hotel = 1,
            CarRental = 2,
            Restaurant = 3,
            TicketOrder = 4,
            EventRsvp = 5


        }

        public enum SettingType
        {
            NA = 0,
            EJGalloAPI = 1
        }

        public enum SiteContentType
        {
            MyAccountPromos = 0,
            RSVPPage = 1,
            RSVPPageFromWidget = 2,
            RSVPPageConcierge = 3,
            ReviewThankYouPage = 4,
            ReviewThankYouPageAd = 5,
            AuthLoginBenefits = 6,
            CancelPolicy = 7,
            SiteMessage = 8,
            DefaultTicketCancelPolicy = 9,
            TrainingResources = 10,
            ClaimPage = 11,
            BusinessLeftColumn = 12,
            SSLSeal = 13,
            PCICompliance = 14,
            SubscriptionStep1 = 15,
            SubscriptionStep2 = 16,
            SubscriptionStep3 = 17,
            SubscriptionPrivacyPolicy = 18,
            SubscriptionUsePolicy = 19,
            SubscriptionAuthorize = 20,
            SubscriptionSignatureTerms = 21,
            NewsletterSignupContent = 22,
            PointsProgram = 23,
            BoxOfficeAppAbout = 24,
            WaitlistAbout = 25,
            TicketCancelPolicy = 26,
            TermsAndConditions = 27,
            HelpCenterPublic = 28,
            EditTicketsCheckIn = 29,
            DestinationAwardsSurvey = 30,
            PassportGiveaway = 31,
            SiteWideMessage = 32,
            CovidWaiver = 33,
            StarterPlanOverview = 34,
            PlusPlanOverview = 35,
            ProfessionalPlanOverview = 36,
            EnterprisePlanOverview = 37,
            TicketingPlanOverview = 38,
            CellarPassMarketOverview = 39,
            CellarPassSafetyPledge = 40,
            GoogleFontList = 41,
            PassportItineraryInstructions = 42,
            LPWhatisCellarScout = 43,
            LPCellarScoutforTRs = 44,
            FrequentlyAskedQuestions = 45,
            BusinessFrequentlyAskedQuestions = 46,
            AboutCellarScout = 47,
            MeettheTeam = 48,
            TastingRoomSignup = 49,
            LPBlogArticles = 50,
            MembersTermsConditions = 51,
            TRsAboutCellarScout = 52,
            TRsHowDoesItWork = 53,
            LPMostPopularOffers = 54,
            CellarScoutMembershipEvent = 55,
            SupportPlanMessageBasic = 56,
            SupportPlanMessagePlus = 57,
            SupportPlanMessageProfessional = 58,
            SupportPlanMessageEnterprise = 59,
            SupportPlanMessageTicketingOnly = 60,
            CPHomepagePromo = 61,
            BusinessPagePromoModal = 62
        }


        public static List<SelectListItem> GetPassportVisitationRulesConsumer()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem[] prams = {
                new SelectListItem { ID =(int)Visitation_Rule.walkin, Name ="No RSVP Required"},
                new SelectListItem { ID =(int)Visitation_Rule.reservations_recommended, Name ="Reservations Recommended"},
                new SelectListItem { ID =(int)Visitation_Rule.reservations_required, Name ="Reservations Required"},
                new SelectListItem { ID =(int)Visitation_Rule.reservations_required_6, Name ="Reservations 6+"},
                new SelectListItem { ID =(int)Visitation_Rule.reservations_required_8, Name ="Reservations 8+"},
            };
            list.AddRange(prams);
            return list;
        }

        public static List<SelectListItem> GetDepositPoliciesEmail()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem[] prams = {
                new SelectListItem { ID =0, Name ="Complimentary"},
                new SelectListItem { ID =7, Name ="Fee(s) Charged Upon Arrival"},
                new SelectListItem { ID =1, Name ="Fee(s) Charged Upon Arrival"},
                new SelectListItem { ID =3, Name ="Complimentary with Min. Purchase"},
                new SelectListItem { ID =8, Name ="Complimentary with Min. Purchase"},
                new SelectListItem { ID =10, Name ="Fee(s) Charged Upon Booking"},
                new SelectListItem { ID =4, Name ="Fee(s) Charged Upon Booking"},
                new SelectListItem { ID =5, Name ="Fee(s) Charged 24 Hours Prior to Arrival"},
                new SelectListItem { ID =6, Name ="Fee(s) Charged 48 Hours Prior to Arrival"},
                new SelectListItem { ID =9, Name ="Fee(s) Charged on Morning of Arrival"},
                new SelectListItem { ID =11, Name ="Charge 25% Deposit Upon Booking"},
                new SelectListItem { ID =12, Name ="Charge 50% Deposit Upon Booking"},
                //new SelectListItem { ID =13, Name ="Fee(s) Charged Upon Booking (Offline)"},
            };
            list.AddRange(prams);
            return list;
        }
        public static List<ClsStaticValues> GetDepositPolicies(bool IsAdmin)
        {
            List<ClsStaticValues> list = new List<ClsStaticValues>();
            list.Add(new ClsStaticValues("0", "Complimentary"));
            list.Add(new ClsStaticValues("7", "Fee(s) Charged Upon Arrival"));
            list.Add(new ClsStaticValues("1", "Fee(s) Charged Upon Arrival- CC Required"));
            list.Add(new ClsStaticValues("3", "Complimentary with Min. Purchase"));
            list.Add(new ClsStaticValues("8", "Complimentary with Min. Purchase- CC Required"));
            list.Add(new ClsStaticValues("10", "Fee(s) Charged Upon Booking"));
            if (IsAdmin)
                list.Add(new ClsStaticValues("4", "Fee(s) Charged Immediately- Accept Declined"));
            else
                list.Add(new ClsStaticValues("4", "Fee(s) Charged Upon Booking"));
            list.Add(new ClsStaticValues("5", "Fee(s) Charged 24 Hours Prior to Arrival"));
            list.Add(new ClsStaticValues("6", "Fee(s) Charged 48 Hours Prior to Arrival"));
            list.Add(new ClsStaticValues("9", "Fee(s) Charged on Morning of Arrival"));
            list.Add(new ClsStaticValues("11", "Fee(s) 25% Charged Upon Booking"));
            list.Add(new ClsStaticValues("12", "Fee(s) 50% Charged Upon Booking"));
            //if (IsAdmin)
            //    list.Add(new ClsStaticValues("13", "Fee(s) Charged Upon Booking (Offline)- CC Required"));
            //else
            //    list.Add(new ClsStaticValues("13", "Fee(s) Charged Upon Booking- CC Required"));
            return list;
        }

        public static List<ClsStaticValues> GetChargeFee()
        {
            List<ClsStaticValues> list = new List<ClsStaticValues>();
            list.Add(new ClsStaticValues("0", "No Fee"));
            list.Add(new ClsStaticValues("7", "Collect Upon Arrival"));
            list.Add(new ClsStaticValues("1", "Collect Upon Arrival - Require CC"));
            list.Add(new ClsStaticValues("3", "Comp’d with Min. Purchase"));
            list.Add(new ClsStaticValues("8", "Comp’d with Min. Purchase- Require CC"));
            list.Add(new ClsStaticValues("10", "Charge Immediately- Reject Declined"));
            list.Add(new ClsStaticValues("4", "Charge Immediately- Accept Declined"));
            list.Add(new ClsStaticValues("5", "Auto-charge 24 Hours Prior- Require CC"));
            list.Add(new ClsStaticValues("6", "Auto-charge 48 Hours Prior- Require CC"));
            list.Add(new ClsStaticValues("9", "Auto-charge Upon Arrival Date (AM)- Require CC"));
            list.Add(new ClsStaticValues("11", "Charge 25% Deposit Upon Booking"));
            list.Add(new ClsStaticValues("12", "Charge 50% Deposit Upon Booking"));
            //list.Add(new ClsStaticValues("13", "Charged Upon Booking (Offline)- CC Required"));
            return list;
        }

        public static List<ClsStaticValues> GetWineCategory()
        {
            List<ClsStaticValues> list = new List<ClsStaticValues>();
            list.Add(new ClsStaticValues("5", "Beer"));
            list.Add(new ClsStaticValues("6", "Champagne"));
            list.Add(new ClsStaticValues("7", "Food"));
            list.Add(new ClsStaticValues("8", "Gift Card"));
            list.Add(new ClsStaticValues("9", "Liquor"));
            list.Add(new ClsStaticValues("10", "Merchandise"));
            list.Add(new ClsStaticValues("11", "Spirits"));
            list.Add(new ClsStaticValues("1", "Wine- Red"));
            list.Add(new ClsStaticValues("2", "Wine- White"));
            list.Add(new ClsStaticValues("3", "Wine- Sparkling"));
            list.Add(new ClsStaticValues("4", "Wine- Dessert"));
            return list;
        }

        public static List<ClsStaticValues> GetMemberBenefit()
        {
            List<ClsStaticValues> list = new List<ClsStaticValues>();
            list.Add(new ClsStaticValues("0", "None"));
            list.Add(new ClsStaticValues("1", "Complimentary (up to 2 attendees)"));
            list.Add(new ClsStaticValues("30", "Complimentary (up to 3 attendees)"));
            list.Add(new ClsStaticValues("2", "Complimentary (up to 4 attendees)"));
            list.Add(new ClsStaticValues("6", "Complimentary (up to 6 attendees)"));
            list.Add(new ClsStaticValues("3", "Complimentary (unlimited attendees)"));
            list.Add(new ClsStaticValues("7", "10% Discount"));
            list.Add(new ClsStaticValues("31", "15% Discount"));
            list.Add(new ClsStaticValues("8", "20% Discount"));
            list.Add(new ClsStaticValues("5", "25% Discount"));
            list.Add(new ClsStaticValues("32", "30% Discount"));
            list.Add(new ClsStaticValues("9", "35% Discount"));
            list.Add(new ClsStaticValues("4", "50% Discount"));
            list.Add(new ClsStaticValues("10", "$5 Off Per Person"));
            list.Add(new ClsStaticValues("11", "$10 Off Per Person"));
            list.Add(new ClsStaticValues("12", "$15 Off Per Person"));
            list.Add(new ClsStaticValues("13", "$20 Off Per Person"));
            list.Add(new ClsStaticValues("14", "$25 Off Per Person"));
            list.Add(new ClsStaticValues("15", "$30 Off Per Person"));
            list.Add(new ClsStaticValues("16", "$35 Off Per Person"));
            list.Add(new ClsStaticValues("17", "$40 Off Per Person"));
            list.Add(new ClsStaticValues("18", "$45 Off Per Person"));
            list.Add(new ClsStaticValues("19", "$50 Off Per Person"));
            list.Add(new ClsStaticValues("20", "$55 Off Per Person"));
            list.Add(new ClsStaticValues("21", "$60 Off Per Person"));
            list.Add(new ClsStaticValues("22", "$65 Off Per Person"));
            list.Add(new ClsStaticValues("23", "$70 Off Per Person"));
            list.Add(new ClsStaticValues("24", "$75 Off Per Person"));
            list.Add(new ClsStaticValues("25", "$80 Off Per Person"));
            list.Add(new ClsStaticValues("26", "$85 Off Per Person"));
            list.Add(new ClsStaticValues("27", "$90 Off Per Person"));
            list.Add(new ClsStaticValues("28", "$95 Off Per Person"));
            list.Add(new ClsStaticValues("29", "$100 Off Per Person"));
            return list;
        }

        public enum DiscountOption
        {
            FeePerPerson = 1,
            OrderTotal = 2
        }

        public enum DateType
        {
            ByDateBooked = 1,
            ByEventDate = 2
        }

        public enum DiscountType
        {
            None = 0,
            Comp2 = 1,
            Comp4 = 2,
            CompUnlimited = 3,
            Percent50 = 4,
            Percent25 = 5,
            Comp6 = 6,
            Percent10 = 7,
            Percent20 = 8,
            Percent35 = 9,
            PP5Off = 10,
            PP10Off = 11,
            PP15Off = 12,
            PP20Off = 13,
            PP25Off = 14,
            PP30Off = 15,
            PP35Off = 16,
            PP40Off = 17,
            PP45Off = 18,
            PP50Off = 19,
            PP55Off = 20,
            PP60Off = 21,
            PP65Off = 22,
            PP70Off = 23,
            PP75Off = 24,
            PP80Off = 25,
            PP85Off = 26,
            PP90Off = 27,
            PP95Off = 28,
            PP100Off = 29,
            Comp3 = 30,
            Percent15 = 31,
            Percent30 = 32,
            Comp8 = 33,
            Comp10 = 34,
            Comp12 = 35,
            Comp14 = 36,
            Comp16 = 37,
            Comp18 = 38,
            Comp20 = 39,
            Percent5 = 40,
            AccessOnly = 99,
            Comp1 = 41,
            CustomDiscountAmt = 42,
            CustomDiscountPercent = 43,
            Comp5 = 44
        }

        public enum RsvpClaimStatus
        {
            booked = 0,
            canceled = 1,
            rescheduled = 2
        }

        public enum Discount
        {
            discount = 1,
            activationCode = 2,
            manualDiscount = 3
        }

        public enum ClubMembershipApi
        {
            all = 0,
            eCellar = 1,
            cellarpass = 2,
            ams = 3,
            microworks = 4,
            coresense = 6
        }

        public static string GetMemberBenefitDescByValue(DiscountType value)
        {
            string retDesc = "NA";

            switch (value)
            {
                case DiscountType.Comp1:
                    retDesc = "complimentary fees (up to 1 attendees)";
                    break;
                case DiscountType.Comp2:
                    retDesc = "complimentary fees (up to 2 attendees)";
                    break;
                case DiscountType.Comp3:
                    retDesc = "complimentary fees (up to 3 attendees)";
                    break;
                case DiscountType.Comp4:
                    retDesc = "complimentary fees (up to 4 attendees)";
                    break;
                case DiscountType.Comp5:
                    retDesc = "complimentary fees (up to 5 attendees)";
                    break;
                case DiscountType.Comp6:
                    retDesc = "complimentary fees (up to 6 attendees)";
                    break;
                case DiscountType.Comp8:
                    retDesc = "complimentary fees (up to 8 attendees)";
                    break;
                case DiscountType.Comp10:
                    retDesc = "complimentary fees (up to 10 attendees)";
                    break;
                case DiscountType.Comp12:
                    retDesc = "complimentary fees (up to 12 attendees)";
                    break;
                case DiscountType.Comp14:
                    retDesc = "complimentary fees (up to 14 attendees)";
                    break;
                case DiscountType.Comp16:
                    retDesc = "complimentary fees (up to 16 attendees)";
                    break;
                case DiscountType.Comp18:
                    retDesc = "complimentary fees (up to 18 attendees)";
                    break;
                case DiscountType.Comp20:
                    retDesc = "complimentary fees (up to 20 attendees)";
                    break;
                case DiscountType.CompUnlimited:
                    retDesc = "complimentary fees";
                    break;
                case DiscountType.Percent5:
                    retDesc = "5% discount on tasting fees";
                    break;
                case DiscountType.Percent50:
                    retDesc = "50% discount on tasting fees";
                    break;
                case DiscountType.Percent25:
                    retDesc = "25% discount on tasting fees";
                    break;
                case DiscountType.Percent10:
                    retDesc = "10% discount on tasting fees";
                    break;
                case DiscountType.Percent15:
                    retDesc = "15% discount on tasting fees";
                    break;
                case DiscountType.Percent20:
                    retDesc = "20% discount on tasting fees";
                    break;
                case DiscountType.Percent30:
                    retDesc = "30% discount on tasting fees";
                    break;
                case DiscountType.Percent35:
                    retDesc = "35% discount on tasting fees";
                    break;
                case DiscountType.PP5Off:
                    retDesc = "$5 Off Per Person";
                    break;
                case DiscountType.PP10Off:
                    retDesc = "$10 Off Per Person";
                    break;
                case DiscountType.PP15Off:
                    retDesc = "$15 Off Per Person";
                    break;
                case DiscountType.PP20Off:
                    retDesc = "$20 Off Per Person";
                    break;
                case DiscountType.PP25Off:
                    retDesc = "$25 Off Per Person";
                    break;
                case DiscountType.PP30Off:
                    retDesc = "$30 Off Per Person";
                    break;
                case DiscountType.PP35Off:
                    retDesc = "$35 Off Per Person";
                    break;
                case DiscountType.PP40Off:
                    retDesc = "$40 Off Per Person";
                    break;
                case DiscountType.PP45Off:
                    retDesc = "$45 Off Per Person";
                    break;
                case DiscountType.PP50Off:
                    retDesc = "$50 Off Per Person";
                    break;
                case DiscountType.PP55Off:
                    retDesc = "$55 Off Per Person";
                    break;
                case DiscountType.PP60Off:
                    retDesc = "$60 Off Per Person";
                    break;
                case DiscountType.PP65Off:
                    retDesc = "$65 Off Per Person";
                    break;
                case DiscountType.PP70Off:
                    retDesc = "$70 Off Per Person";
                    break;
                case DiscountType.PP75Off:
                    retDesc = "$75 Off Per Person";
                    break;
                case DiscountType.PP80Off:
                    retDesc = "$80 Off Per Person";
                    break;
                case DiscountType.PP85Off:
                    retDesc = "$85 Off Per Person";
                    break;
                case DiscountType.PP90Off:
                    retDesc = "$90 Off Per Person";
                    break;
                case DiscountType.PP95Off:
                    retDesc = "$95 Off Per Person";
                    break;
                case DiscountType.PP100Off:
                    retDesc = "$100 Off Per Person";
                    break;
                case DiscountType.AccessOnly:
                    retDesc = "Members Only";
                    break;
                case DiscountType.CustomDiscountAmt:
                    retDesc = "Custom Discount Amount";
                    break;
                case DiscountType.CustomDiscountPercent:
                    retDesc = "Custom Discount Percent";
                    break;
            }

            return retDesc;
        }

        public static string GetAddOnsMemberBenefitDescByValue(DiscountType value)
        {
            string retDesc = "NA";

            switch (value)
            {
                case DiscountType.Comp2:
                    retDesc = "complimentary fees (up to 2 attendees)";
                    break;
                case DiscountType.Comp3:
                    retDesc = "complimentary fees (up to 3 attendees)";
                    break;
                case DiscountType.Comp4:
                    retDesc = "complimentary fees (up to 4 attendees)";
                    break;
                case DiscountType.Comp5:
                    retDesc = "complimentary fees (up to 5 attendees)";
                    break;
                case DiscountType.Comp6:
                    retDesc = "complimentary fees (up to 6 attendees)";
                    break;
                case DiscountType.Comp8:
                    retDesc = "complimentary fees (up to 8 attendees)";
                    break;
                case DiscountType.Comp10:
                    retDesc = "complimentary fees (up to 10 attendees)";
                    break;
                case DiscountType.Comp12:
                    retDesc = "complimentary fees (up to 12 attendees)";
                    break;
                case DiscountType.Comp14:
                    retDesc = "complimentary fees (up to 14 attendees)";
                    break;
                case DiscountType.Comp16:
                    retDesc = "complimentary fees (up to 16 attendees)";
                    break;
                case DiscountType.Comp18:
                    retDesc = "complimentary fees (up to 18 attendees)";
                    break;
                case DiscountType.Comp20:
                    retDesc = "complimentary fees (up to 20 attendees)";
                    break;
                case DiscountType.CompUnlimited:
                    retDesc = "complimentary fees";
                    break;
                case DiscountType.Percent5:
                    retDesc = "5% discount on tasting fee add-ons";
                    break;
                case DiscountType.Percent50:
                    retDesc = "50% discount on tasting fee add-ons";
                    break;
                case DiscountType.Percent25:
                    retDesc = "25% discount on tasting fee add-ons";
                    break;
                case DiscountType.Percent10:
                    retDesc = "10% discount on tasting fee add-ons";
                    break;
                case DiscountType.Percent15:
                    retDesc = "15% discount on tasting fee add-ons";
                    break;
                case DiscountType.Percent20:
                    retDesc = "20% discount on tasting fee add-ons";
                    break;
                case DiscountType.Percent30:
                    retDesc = "30% discount on tasting fee add-ons";
                    break;
                case DiscountType.Percent35:
                    retDesc = "35% discount on tasting fee add-ons";
                    break;
                case DiscountType.PP5Off:
                    retDesc = "$5 Off Per Person";
                    break;
                case DiscountType.PP10Off:
                    retDesc = "$10 Off Per Person";
                    break;
                case DiscountType.PP15Off:
                    retDesc = "$15 Off Per Person";
                    break;
                case DiscountType.PP20Off:
                    retDesc = "$20 Off Per Person";
                    break;
                case DiscountType.PP25Off:
                    retDesc = "$25 Off Per Person";
                    break;
                case DiscountType.PP30Off:
                    retDesc = "$30 Off Per Person";
                    break;
                case DiscountType.PP35Off:
                    retDesc = "$35 Off Per Person";
                    break;
                case DiscountType.PP40Off:
                    retDesc = "$40 Off Per Person";
                    break;
                case DiscountType.PP45Off:
                    retDesc = "$45 Off Per Person";
                    break;
                case DiscountType.PP50Off:
                    retDesc = "$50 Off Per Person";
                    break;
                case DiscountType.PP55Off:
                    retDesc = "$55 Off Per Person";
                    break;
                case DiscountType.PP60Off:
                    retDesc = "$60 Off Per Person";
                    break;
                case DiscountType.PP65Off:
                    retDesc = "$65 Off Per Person";
                    break;
                case DiscountType.PP70Off:
                    retDesc = "$70 Off Per Person";
                    break;
                case DiscountType.PP75Off:
                    retDesc = "$75 Off Per Person";
                    break;
                case DiscountType.PP80Off:
                    retDesc = "$80 Off Per Person";
                    break;
                case DiscountType.PP85Off:
                    retDesc = "$85 Off Per Person";
                    break;
                case DiscountType.PP90Off:
                    retDesc = "$90 Off Per Person";
                    break;
                case DiscountType.PP95Off:
                    retDesc = "$95 Off Per Person";
                    break;
                case DiscountType.PP100Off:
                    retDesc = "$100 Off Per Person";
                    break;
                case DiscountType.AccessOnly:
                    retDesc = "Members Only";
                    break;
                case DiscountType.CustomDiscountAmt:
                    retDesc = "Custom Discount Amount";
                    break;
                case DiscountType.CustomDiscountPercent:
                    retDesc = "Custom Discount Percent";
                    break;
            }

            return retDesc;
        }

        public static List<ClsStaticValues> GetRsvpStatusColor()
        {
            List<ClsStaticValues> list = new List<ClsStaticValues>();
            list.Add(new ClsStaticValues("0", "bg-color-yellow"));
            //Pending
            list.Add(new ClsStaticValues("1", "bg-color-greenLight"));
            //Completed
            list.Add(new ClsStaticValues("2", "bg-color-red"));
            //Cancelled
            list.Add(new ClsStaticValues("3", "bg-color-blueDark"));
            //No Show
            list.Add(new ClsStaticValues("4", "bg-color-orange"));
            //Rescheduled
            list.Add(new ClsStaticValues("5", "bg-color-blueLight"));
            //GuestDelayed
            list.Add(new ClsStaticValues("6", "bg-color-orangeDark"));
            //Updated
            list.Add(new ClsStaticValues("7", "bg-color-yellow"));
            //Yelp Initiated
            list.Add(new ClsStaticValues("8", "bg-color-grayDark"));
            //Review Received
            list.Add(new ClsStaticValues("8", "bg-color-shockingpink"));
            return list;
        }



        ///// <summary>
        ///// Validate the connection string information in app.config and throws an exception if it looks like 
        ///// the user hasn't updated this to valid values. 
        ///// </summary>
        ///// <param name="storageConnectionString">The storage connection string</param>
        ///// <returns>CloudStorageAccount object</returns>
        //public static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        //{
        //    CloudStorageAccount storageAccount;
        //    try
        //    {
        //        storageAccount = CloudStorageAccount.Parse(storageConnectionString);
        //    }
        //    catch (FormatException)
        //    {
        //        Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
        //        Console.ReadLine();
        //        throw;
        //    }
        //    catch (ArgumentException)
        //    {
        //        Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
        //        Console.ReadLine();
        //        throw;
        //    }

        //    return storageAccount;
        //}



        public enum SettingGroup
        {
            Google = 10,
            bLoyal = 20,
            directory = 30,
            member = 40,
            twilio = 50,
            taxify = 60,
            mailgun = 70,
            mailchimp = 80,
            mailchimp_cp = 800,
            facebook = 90,
            eWinery = 110,
            TripAdvisor = 130,
            OpenTableAPI = 600,
            FareharborAPI = 700,
            system = 900,
            lastmodified = 2000,
            MailGunEmailValidation = 4000,
            usersync = 300
        }

        public enum SettingKey
        {

            GoogleClientID = 10,
            GoogleClientSecret = 11,
            GoogleRefreshToken = 12,
            GoogleCalendarAuth = 13,

            bLoyalApiEnabled = 20,
            bLoyalApiCompany = 21,
            bLoyalApiUsername = 22,
            bLoyalApiPassword = 23,
            bLoyalApiDeviceKey = 24,
            bLoyalApiBaseURL = 25,
            bLoyalApiClubLookup = 26,
            bLoyalApiSyncOrderCriteria = 27,

            directory_byAppoinmentOnly = 30,

            member_byAppoinmentOnly = 30,

            member_defaultAccount = 41,
            member_table_pro_username = 42,
            member_table_pro_password = 43,
            member_zoom_meeting_id_setting = 44,
            member_zoom_require_password = 45,
            member_zoom_password = 46,
            member_enable_covid_survey = 47,
            member_enable_covid_waiver = 48,

            member_enable_private_booking_requests = 400,
            member_private_booking_request_email = 401,
            member_Marketing_Opt_in = 402,
            member_private_rsvp_cancellation_policy = 403,
            member_rsvp_contact_email = 404,
            member_rsvp_contact_phone = 405,
            member_Pvt_rsvp_confirmation_message = 406,
            member_Pvt_rsvp_cancellation_message = 407,
            member_private_booking_notifications_receiver = 408,
            member_rsvp_review_business_message = 409,

            member_Pvt_rsvp_confirmation_message1 = 411,
            member_Pvt_rsvp_confirmation_message2 = 412,
            member_Pvt_rsvp_confirmation_message3 = 413,
            member_Pvt_rsvp_confirmation_message4 = 414,
            member_Pvt_rsvp_confirmation_message5 = 415,
            member_Pvt_rsvp_confirmation_message6 = 416,
            member_Pvt_rsvp_confirmation_message7 = 417,
            member_Pvt_rsvp_confirmation_message8 = 418,

            member_Pvt_rsvp_confirmation_message_title1 = 421,
            member_Pvt_rsvp_confirmation_message_title2 = 422,
            member_Pvt_rsvp_confirmation_message_title3 = 423,
            member_Pvt_rsvp_confirmation_message_title4 = 424,
            member_Pvt_rsvp_confirmation_message_title5 = 425,
            member_Pvt_rsvp_confirmation_message_title6 = 426,
            member_Pvt_rsvp_confirmation_message_title7 = 427,
            member_Pvt_rsvp_confirmation_message_title8 = 428,

            twilio_ACCOUNTSID = 51,
            twilio_AUTHTOKEN = 52,
            twilio_DisableVerificationService = 53,
            taxify_ApiKey = 61,

            mailgun_01_key = 71,
            mailgun_01_domain = 72,
            mailgun_02_key = 73,
            mailgun_02_domain = 74,
            mailgun_03_key = 75,
            mailgun_03_domain = 76,
            mailgun_04_key = 77,
            mailgun_04_domain = 78,
            mailgun_05_key = 79,
            mailgun_05_domain = 80,
            mailgun_06_key = 101,
            mailgun_06_domain = 102,
            mailgun_07_key = 103,
            mailgun_07_domain = 104,
            mailgun_08_key = 105,
            mailgun_08_domain = 106,

            mailchimp_key = 81,
            mailchimp_list = 82,
            mailchimp_listname = 83,
            mailchimp_store = 84,
            mailchimp_reservationstag = 86,
            mailchimp_ticketingstag = 87,
            mailchimp_rsvplistid = 88,
            mailchimp_ticketlistid = 89,
            mailchimp_rsvplistName = 90,
            mailchimp_ticketlistName = 91,
            mailchimp_lastSync = 92,

            mailchimp_cp_key = 801,
            mailchimp_cp_adminList = 802,
            mailchimp_cp_adminListName = 803,
            mailchimp_cp_guestList = 804,
            mailchimp_cp_guestListName = 805,
            mailchimp_cp_newsletterList = 806,
            mailchimp_cp_newsletterListName = 807,
            mailchimp_cp_specialOffersList = 808,
            mailchimp_cp_specialOffersListName = 809,
            mailchimp_cp_billingList = 810,
            mailchimp_cp_billingListName = 811,
            mailchimp_cp_conciergeList = 812,
            mailchimp_cp_conciergeListName = 813,
            mailchimp_cp_store = 814,

            facebook_pixel = 91,

            ewinery_affiliate = 111,
            ewinery_back_office = 112,
            ewinery_concierge = 113,
            ewinery_referral = 114,
            ewinery_table_pro = 115,
            ewinery_widget = 116,
            ewinery_sourcecode = 117,

            tripadvisor_content_api = 131,

            OpenTableAPI_UserName = 601,
            OpenTableAPI_Password = 602,

            FareharborAPI_UserName = 701,
            FareharborAPI_Password = 702,
            system_site_gated = 901,
            system_copyright_msg = 902,
            system_zoom_basic_auth = 904,
            system_zoom_redirect_url = 905,
            system_zoom_client_id = 906,
            system_zoom_verification_token = 907,
            system_sms_master_number = 908,

            lastmodified_page_member_network = 2001,
            lastmodified_page_member_network_user = 2002,
            EnableMailGunEmailValidation = 4001,
            usersync_schedule=301
        }
        public enum ExportType
        {
            None = 0,
            Vin65 = 1,
            eWinery = 2,
            exportError = 3,
            bLoyal = 4,
            OrderPort = 5,
            Commerce7 = 6,
            Shopify = 7,
            BigCommerce = 8
        }

        public enum MailgunDomain
        {
            NA = 0,
            SystemMessages = 1,
            Invitations = 2,
            Reservations = 3,
            Ticketing = 4,
            Reminder = 5,
            Notification = 6,
            Reviews = 7,
            Billing = 8
        }

        public enum ErrorType
        {
            None = 0,
            EventFutureDate = 1,
            IsHoliday = 2,
            MinSeats = 3,
            MaxSeats = 4,
            MaxEndDate = 5,
            Exception = 6,
            ReservationConflict = 7,
            EventInactive = 8,
            EventLeadTime = 9,
            AvailableSeats = 10,
            Guest = 11,
            CreditCard = 12,
            CreatingUser = 13,
            EventError = 14,
            ReservationUpdateError = 15,
            ReservationSavingError = 16,
            ReservationPaymentError = 17,
            RsvpBackToBack = 18,
            ReservationAlreadySeated = 19,
            ReservationAlreadyTerminated = 20,
            NoRecordFound = 21,
            TableAlreadyInUse = 22,
            NoTicketingPlan = 23,
            TableCoflict = 24,
            DuplicateTicket = 25, //!!important!! always add new enum value at the bottom
            CancelLeadTimeError = 26,
            HiddenMember = 27,
            PostCaptureTicketInvalid = 28,
            WaitListExists = 29,
            QuantityNotAvailable = 30,
            InvalidData = 31,
            InvalidClubMember = 32,
            TicketAcessCodeInvalid = 33,
            OpenTableError = 34,
            WillCallUnavailable = 35,
            AlreadyInvited = 36,
            InvalidAccessCode=37,
            NoTablesAvailable = 38,
            TableInventory = 39,
            TableBlocked = 40,
            InvalidPromoCode = 41
        }

        public enum UserRole
        {
            SysAdmin = 1,
            Administrator = 2,
            Host = 3,
            Guest = 4,
            ViewSecureInfo = 5,
            Affiliate = 6,
            Hospitality = 7,
            BusinessOwner = 8,
            AccountManager = 9
        }
        public enum UserType
        {
            User = 0,
            Guest = 1,
            Affiliate = 2
        }

        public enum TicketsEventStatus
        {
            DRAFT = 0,
            LIVE = 1,
            HIDDEN = 2,
            ENDED = 3,
            SOLDOUT = 4,
            CANCELLED = 5
        }

        public enum TicketStatus
        {
            ACTIVE = 0,
            CLAIMED = 1,
            INVALID = 2
        }

        public enum TicketType
        {
            Ticket = 0,
            Donation = 1
        }

        public enum TicketsSaleStatus
        {
            Any = -1,
            NotStarted = 0,
            OnSale = 1,
            Hidden = 2,
            Ended = 3,
            SoldOut = 4
        }

        public enum TicketOrderStatus
        {
            ALL = -1,
            Draft = 0,
            Completed = 1,
            Problem = 2,
            Void = 3,
        }

        public enum TicketOrderType
        {
            Online = 0,
            Backoffice = 1,
            Exception = 2,
            BOXOFFICEAPP = 3,
        }

        public enum TicketDelivery
        {
            None = -1,
            WillCall = 0,
            SelfPrint = 1,
            Shipped = 2
        }

        public enum CheckInStatus
        {
            NA = 0,
            SUCCESS = 1,
            NOT_ALLOWED = 2,
            FAILED = 3,
            NOT_ALLOWED_BAD_EVENT = 4,
            NOT_ALLOWED_BAD_DATE = 5,
            TEST = 6,
            NOT_ALLOWED_INVITED_TICKET = 7
        }

        public enum TicketPostCaptureStatus
        {
            NA = 0,
            Available = 1,
            Invited = 2,
            Claimed = 3,
            Printed = 4,
            Issued = 5,
            CheckedIn = 6
        }

        public enum RSVPPostCaptureStatus
        {
            NA = -1,
            Available = 0,
            Invited = 1,
            Registered = 2,
            Expired = 3
        }

        public enum TicketsServiceFeesOption
        {
            Ticketholder = 0,
            Organizer = 1,
            TicketHolderPlusCCProcessing = 2
        }
        public enum TicketsPaymentProcessor
        {
            CellarPassProcessor = 0,
            ClientProcessor = 1,
            Stripe = 2
        }

        public enum AttendanceModeStatus
        {
            PhysicalEvent = 0,
            OnlineEvent = 1,
            MixedAttendanceEvent = 2
        }

        public enum AttendeeAppCheckInMode
        {
            standardSingle = 0,
            standardMulti = 1, //Not Surported as of 9/30/2015
            barcodeSingle = 2,
            barcodeMulti = 3,
            searchSingle = 4,
            searchMulti = 5,
            multiEvent = 6,
        }

        public enum TimeZone
        {
            None = 0,
            ArizonaTimeZone = 1,
            EasternTimeZone = 2,
            CentralTimeZone = 3,
            MountainTimeZone = 4,
            PacificTimeZone = 5,
            HawaiianStandardTime = 6,
            AlaskanStandardTime = 7
            //NOTE - THESE VALUES CORRESPOND TO THE VALUES IN THE TIMES CLASS FOR THE TIMEZONEIDs - ENABLE AS NEEDED
            //UTC11 = 8
            //PacificStandardTimeMexico = 9
            //USMountainStandardTime = 10
            //MountainStandardTimeMexico = 11
            //CentralAmericaStandardTime = 12
            //CentralStandardTimeMexico = 13
            //CanadaCentralStandardTime = 14
            //SAPacificStandardTime = 15
            //EasternStandardTimeMexico = 16
            //USEasternStandardTime = 17
            //VenezuelaStandardTime = 18
            //ParaguayStandardTime = 19
            //AtlanticStandardTime = 20
            //CentralBrazilianStandardTime = 21
            //SAWesternStandardTime = 22
            //NewfoundlandStandardTime = 23
            //ESouthAmericaStandardTime = 24
            //ArgentinaStandardTime = 25
            //SAEasternStandardTime = 26
            //GreenlandStandardTime = 27
            //MontevideoStandardTime = 28
            //BahiaStandardTime = 29
            //PacificSAStandardTime = 30
            //UTC02 = 31
            //MidAtlanticStandardTime = 32
            //AzoresStandardTime = 33
            //CapeVerdeStandardTime = 34
            //MoroccoStandardTime = 35
            //UTC = 36
            //GMTStandardTime = 37
            //GreenwichStandardTime = 38
            //WEuropeStandardTime = 39
            //CentralEuropeStandardTime = 40
            //RomanceStandardTime = 41
            //CentralEuropeanStandardTime = 42
            //WCentralAfricaStandardTime = 43
            //NamibiaStandardTime = 44
            //JordanStandardTime = 45
            //GTBStandardTime = 46
            //MiddleEastStandardTime = 47
            //EgyptStandardTime = 48
            //SyriaStandardTime = 49
            //EEuropeStandardTime = 50
            //SouthAfricaStandardTime = 51
            //FLEStandardTime = 52
            //TurkeyStandardTime = 53
            //IsraelStandardTime = 54
            //KaliningradStandardTime = 55
            //LibyaStandardTime = 56
            //ArabicStandardTime = 57
            //ArabStandardTime = 58
            //BelarusStandardTime = 59
            //RussianStandardTime = 60
            //EAfricaStandardTime = 61
            //IranStandardTime = 62
            //ArabianStandardTime = 63
            //AzerbaijanStandardTime = 64
            //RussiaTimeZone3 = 65
            //MauritiusStandardTime = 66
            //GeorgianStandardTime = 67
            //CaucasusStandardTime = 68
            //AfghanistanStandardTime = 69
            //WestAsiaStandardTime = 70
            //EkaterinburgStandardTime = 71
            //PakistanStandardTime = 72
            //IndiaStandardTime = 73
            //SriLankaStandardTime = 74
            //NepalStandardTime = 75
            //CentralAsiaStandardTime = 76
            //BangladeshStandardTime = 77
            //NCentralAsiaStandardTime = 78
            //MyanmarStandardTime = 79
            //SEAsiaStandardTime = 80
            //NorthAsiaStandardTime = 81
            //ChinaStandardTime = 82
            //NorthAsiaEastStandardTime = 83
            //SingaporeStandardTime = 84
            //WAustraliaStandardTime = 85
            //TaipeiStandardTime = 86
            //UlaanbaatarStandardTime = 87
            //TokyoStandardTime = 88
            //KoreaStandardTime = 89
            //YakutskStandardTime = 90
            //CenAustraliaStandardTime = 91
            //AUSCentralStandardTime = 92
            //EAustraliaStandardTime = 93
            //AUSEasternStandardTime = 94
            //WestPacificStandardTime = 95
            //TasmaniaStandardTime = 96
            //MagadanStandardTime = 97
            //VladivostokStandardTime = 98
            //RussiaTimeZone10 = 99
            //CentralPacificStandardTime = 100
            //RussiaTimeZone11 = 101
            //NewZealandStandardTime = 102
            //UTC12 = 103
            //FijiStandardTime = 104
            //KamchatkaStandardTime = 105
            //TongaStandardTime = 106
            //SamoaStandardTime = 107
            //LineIslandsStandardTime = 108
            //DatelineStandardTime = 109
        }
        public enum PaymentType
        {
            None = 0,
            CreditCard = 1,
            PayPal = 2,
            Cash = 3,
            Check = 4,
            Offline = 5,
            ACH = 6
        }

        public enum TicketCategory
        {
            None = 0,
            ArtExhibit = 13,
            BeerFestival = 16,
            BeerTasting = 15,
            Concert = 2,
            Education = 3,
            Festival = 4,
            FilmFestival = 17,
            FoodWine = 5,
            Fundraiser = 6,
            GuestLecture = 18,
            GuidedTour = 19,
            LiveMusic = 14,
            LivePerformance = 20,
            Other = 7,
            Passport = 1,
            SpecialEvent = 8,
            LiveBroadcast = 21,
            Theater = 12,
            TourTasting = 9,
            WineCompetition = 11,
            WineTasting = 10,
            Workshop = 22,
            Webinar = 23,
            Membership = 24
        }

        public enum CustomerType
        {
            general = 0,
            club_member = 1,
            vip_customer = 2,
            trade_1 = 3,
            employee = 4,
            trade_2 = 5,
            club_member_guest = 6,
            owner_friends_family = 7,
            employee_friends_family = 8,
            outreach = 9,
            press_media = 10,
            sales_internal = 11,
            sales_external = 12,
            vendor = 13,
            winery_partner = 14,
            platinum_vip = 15,
            walk_in = 16,
        }

        public enum AurthorizedUserType
        {
            [Display(Name = "Backoffice User", Description = "BackOffice User")]
            BackOffice = 1,
            [Display(Name = "Guest Link App User", Description = "Guest Link App User")]
            BoxOffice = 2,
            [Display(Name = "TablePro User", Description = "TablePro User")]
            TablePro = 3,
            [Display(Name = "CheckIn User", Description = "CheckIn User")]
            CheckIn = 4,
            [Display(Name = "CoreApi User", Description = "CoreApi User")]
            CoreApi = 5,
            [Display(Name = "Guest Link App User", Description = "Guest Link App User")]
            BoxOfficeV2 = 6,
            [Display(Name = "CellarPass Account", Description = "CellarPass Account")]
            CellarpassAccount = 7
        }

        public static Int32 GetSource(string AuthenticateKey)
        {
            AurthorizedUserType src = AurthorizedUserType.BackOffice;

            if (AuthenticateKey == GetEnumDescription(AurthorizedUserType.BackOffice))
                src = AurthorizedUserType.BackOffice;
            else if (AuthenticateKey == GetEnumDescription(AurthorizedUserType.BoxOffice))
                src = AurthorizedUserType.BoxOffice;
            else if (AuthenticateKey == GetEnumDescription(AurthorizedUserType.BoxOfficeV2))
                src = AurthorizedUserType.BoxOfficeV2;
            else if (AuthenticateKey == GetEnumDescription(AurthorizedUserType.CellarpassAccount))
                src = AurthorizedUserType.CellarpassAccount;
            else if (AuthenticateKey == GetEnumDescription(AurthorizedUserType.CheckIn))
                src = AurthorizedUserType.CheckIn;
            else if (AuthenticateKey == GetEnumDescription(AurthorizedUserType.CoreApi))
                src = AurthorizedUserType.CoreApi;
            else if (AuthenticateKey == GetEnumDescription(AurthorizedUserType.TablePro))
                src = AurthorizedUserType.TablePro;

            return (int)src;
        }

        public enum FulfillmentLeadTime
        {
            [Display(Name = "Ships in 1-2 business days", Description = "Ships in 1-2 business days")]
            Shippedin1_2 = 1,
            [Display(Name = "Ships in 2-3 business days", Description = "Ships in 2-3 business days")]
            Shippedin2_3 = 2,
            [Display(Name = "Ships in 3-5 business days", Description = "Ships in 3-5 business days")]
            Shippedin3_5 = 3,
            [Display(Name = "Ships in 5-7 business days", Description = "Ships in 5-7 business days")]
            Shippedin5_7 = 4
        }

        public enum TransactionCategory
        {
            waitlists = 1,
            rsvp = 2
        }
        public enum Color
        {
            [Display(Name = "seated", Description = "#98459B")]
            seated = 2,
            [Display(Name = "partially_seated", Description = "#66986E")]
            partially_seated = 3,
            [Display(Name = "first_course", Description = "#3A709B")]
            first_course = 4,
            [Display(Name = "check", Description = "#CE6D04")]
            check = 5,
            [Display(Name = "bus", Description = "#A90318")]
            bus = 6,
            [Display(Name = "terminated", Description = "#0D0D0D")]
            terminated = 9
        }

        public enum Visitation_Rule
        {
            walkin = 0,
            reservations_recommended = 1,
            reservations_required = 2,
            reservations_required_6 = 3,
            reservations_required_8 = 4
        }

        public enum TicketRefundPolicy
        {
            [Display(Description = "No Refunds")]
            NoRefunds = 0,
            [Display(Description = "Up to 24 hours before event starts")]
            Upto24hoursbeforeeventstarts = 1,
            [Display(Description = "Up to 7 days before the event starts")]
            Upto7daysbeforetheeventstarts = 2,
            [Display(Description = "Up to 30 days before the event starts")]
            Upto30daysbeforetheeventstarts = 3,
            [Display(Description = "On a case-by-case basis")]
            Oncasbycasebasis = 4,
        }

        public enum RefundServiceFeesOption
        {
            [Display(Description = "Host Pays Fees")]
            HostPaysFees = 0,
            [Display(Description = "Ticketbuyer Pays Fees")]
            TicketbuyerPaysFees = 1,          
        }

        public enum ReferralTypeText
        {
            [Display(Description = "B/O")]
            BackOffice = 0,
            [Display(Description = "REF")]
            CellarPass = 1,
            [Display(Description = "WGT")]
            Widget = 2,
            [Display(Description = "CON")]
            Affiliate = 3,           
            [Display(Description = "MOB")]
            Mobile = 4,
            [Display(Description = "AFF")]
            Referrer = 5,           
            [Display(Description = "WREF")]
            WineryReferral = 6,
            [Display(Description = "TBL")]
            TablePro = 7,
            [Display(Description = "GL")]
            GuestLink = 8,
        }

        public enum ReferralTypeFullText
        {
            [Display(Description = "Back Office")]
            BackOffice = 0,
            [Display(Description = "CellarPass")]
            CellarPass = 1,
            [Display(Description = "Widget")]
            Widget = 2,
            [Display(Description = "Affiliate")]
            Affiliate = 3,
            [Display(Description = "Referrer")]
            Mobile = 4,
            [Display(Description = "Referrer")]
            Referrer = 5,
            [Display(Description = "Winery Referral")]
            WineryReferral = 6,
            [Display(Description = "TablePro")]
            TablePro = 7,
            [Display(Description = "Guest Link App User")]
            GuestLink = 8,
        }


        #region "Get Enum Desciption"
        public static string GetEnumDescription(this Enum value)
        {
            // get attributes  
            FieldInfo field = value.GetType().GetField(value.ToString());
            var attributes = field.GetCustomAttributes(false);

            // Description is in a hidden Attribute class called DisplayAttribute
            // Not to be confused with DisplayNameAttribute
            dynamic displayAttribute = null;

            if (attributes.Any())
            {
                displayAttribute = attributes.ElementAt(0);
            }

            // return description
            return displayAttribute?.Description ?? "Description Not Found";
        }

        #endregion "Get Enum Desciption"
        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : value.Substring(0, maxLength)
                   );
        }

        public static string Right(this string str, int length)
        {
            str = (str ?? string.Empty);
            return (str.Length >= length)
                ? str.Substring(str.Length - length, length)
                : str;
        }

        public static string ExtractNumbers(string expr)
        {
            return string.Join(null, System.Text.RegularExpressions.Regex.Split(expr, "[^\\d]"));
        }

    }

    public class Payments
    {
        public class Configuration
        {
            public string MerchantLogin { get; set; }
            public string MerchantPassword { get; set; }
            public string UserConfig1 { get; set; }
            public string UserConfig2 { get; set; }
            public int PaymentGateway { get; set; }
            public Mode GatewayMode { get; set; }
            public enum Gateway
            {
                Offline = 0,
                AuthorizeNet = 1,
                PayFlowPro = 2,
                CenPos = 3,
                USAePay = 4,
                Braintree = 5,
                WorldPayXML = 6,
                OpenEdge = 7,
                Stripe = 8,
                Cybersource = 9,
                Shift4 = 10,
                CardConnect = 11,
                Zeamster=12,
                Commrece7Payments=13
            }
            public enum Mode
            {
                live = 0,
                test = 1
            }
        }

        public class TransactionResult
        {

            public enum StatusType
            {
                Failed = 0,
                Success = 1
            }
            private StatusType _Status = StatusType.Failed;
            public StatusType Status
            {
                get { return _Status; }
                set { _Status = value; }
            }

            private bool _doFollowUpSale;
            public bool DoFollowUpSale
            {
                get { return _doFollowUpSale; }
                set { _doFollowUpSale = value; }
            }

            private Transaction.ChargeType _TransactionType = Transaction.ChargeType.Sale;
            public Transaction.ChargeType TransactionType
            {
                get { return _TransactionType; }
                set { _TransactionType = value; }
            }

            private Configuration.Gateway _paymentGateway = Configuration.Gateway.Offline;
            public Configuration.Gateway PaymentGateway
            {
                get { return _paymentGateway; }
                set { _paymentGateway = value; }
            }

            private PaymentType _PayType = PaymentType.CreditCard;
            public PaymentType PayType
            {
                get { return _PayType; }
                set { _PayType = value; }
            }

            private string _ResponseCode;
            public string ResponseCode
            {
                get { return _ResponseCode; }
                set { _ResponseCode = value; }
            }

            private string _Detail;
            public string Detail
            {
                get { return _Detail; }
                set { _Detail = value; }
            }

            private string _ApprovalCode;
            public string ApprovalCode
            {
                get { return _ApprovalCode; }
                set { _ApprovalCode = value; }
            }

            private string _TransactionID;
            public string TransactionID
            {
                get { return _TransactionID; }
                set { _TransactionID = value; }
            }

            private string _AvsResponse;
            public string AvsResponse
            {
                get { return _AvsResponse; }
                set { _AvsResponse = value; }
            }

            private string _CheckOrRefNumber;
            public string CheckOrRefNumber
            {
                get { return _CheckOrRefNumber; }
                set { _CheckOrRefNumber = value; }
            }

            //private Payments.CreditCard _Card;
            //public Payments.CreditCard Card
            //{
            //    get { return _Card; }
            //    set { _Card = value; }
            //}

            private decimal _Amount;
            public decimal Amount
            {
                get { return _Amount; }
                set { _Amount = value; }
            }

            //Private _ResponseAmount As Decimal
            //Public Property ResponseAmount() As Decimal
            //    Get
            //        Return _ResponseAmount
            //    End Get
            //    Set(ByVal value As Decimal)
            //        _ResponseAmount = value
            //    End Set
            //End Property

            private int _ProcessedBy;
            public int ProcessedBy
            {
                get { return _ProcessedBy; }
                set { _ProcessedBy = value; }
            }

            private int _PaymentID;
            public int PaymentID
            {
                get { return _PaymentID; }
                set { _PaymentID = value; }
            }
            public CreditCard Card { get; set; }

            private decimal _Change;
            public decimal Change
            {
                get { return _Change; }
                set { _Change = value; }
            }
        }
        public class CreditCard
        {
            public string Number { get; set; }
            public string CustName { get; set; }
            public string ExpMonth { get; set; }
            public string ExpYear { get; set; }
            public string CVV { get; set; }
            public string Type { get; set; }
            public CardType CardTypes { get; set; }
            public string CardToken { get; set; }
            public string CardLastFourDigits { get; set; }
            public string CardFirstFourDigits { get; set; }
            public CardEntry CardEntry { get; set; }
            public ApplicationType ApplicationType { get; set; }
            public string ApplicationVersion { get; set; }
            public string TerminalId { get; set; }
            public string CardReader { get; set; }
            public enum CardType
            {
                MasterCard = 0x1,
                VISA = 0x2,
                Amex = 0x4,
                DinersClub = 0x8,
                enRoute = 0x10,
                Discover = 0x20,
                JCB = 0x40,
                Unknown = 0x80,
                All = CardType.Amex | CardType.DinersClub | CardType.Discover | CardType.Discover | CardType.enRoute | CardType.JCB | CardType.MasterCard | CardType.VISA
            }
        }
        public class UserDetails
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
            public string Country { get; set; }
            public string HomePhoneStr { get; set; }
            public decimal Phone { get; set; }

        }
        public class Transaction
        {
            public UserDetails User { get; set; }
            public CreditCard Card { get; set; }
            public enum ChargeType
            {
                None = 0,
                AuthOnly = 1,
                Sale = 2,
                Credit = 3,
                Void = 4,
                Capture = 5,
                VoidAuth = 6
            }

            public enum TransactionType
            {
                Rsvp = 0,
                Billing = 1,
                TicketSale = 2
            }

            private ChargeType _Type;
            public ChargeType Type
            {
                get { return _Type; }
                set { _Type = value; }
            }

            private TransactionType _transaction;
            public TransactionType Transactions
            {
                get { return _transaction; }
                set { _transaction = value; }
            }

            private Configuration.Gateway _gateway;
            public Configuration.Gateway Gateway
            {
                get { return _gateway; }
                set { _gateway = value; }
            }

            //private CreditCard _Card;
            //public CreditCard Card
            //{
            //    get { return _Card; }
            //    set { _Card = value; }
            //}

            private decimal _Amount;
            public decimal Amount
            {
                get { return _Amount; }
                set { _Amount = value; }
            }

            private decimal _origAmount;
            public decimal OrigAmount
            {
                get { return _origAmount; }
                set { _origAmount = value; }
            }

            private string _CheckOrRefNumber;
            public string CheckOrRefNumber
            {
                get { return _CheckOrRefNumber; }
                set { _CheckOrRefNumber = value; }
            }

            private string _TransactionID;
            public string TransactionID
            {
                get { return _TransactionID; }
                set { _TransactionID = value; }
            }

            //private Users.UserDetail _User;
            //public Users.UserDetail User
            //{
            //    get { return _User; }
            //    set { _User = value; }
            //}

            private int _WineryId;
            public int WineryId
            {
                get { return _WineryId; }
                set { _WineryId = value; }
            }

            private int _ProcessedBy;
            public int ProcessedBy
            {
                get { return _ProcessedBy; }
                set { _ProcessedBy = value; }
            }

            //public static Hashtable GetChargeTypes()
            //{
            //    string ActiveTypes = "2,3,4";
            //    Type enumeration = typeof(ChargeType);
            //    string[] names = Enum.GetNames(enumeration);
            //    Array values = Enum.GetValues(enumeration);
            //    Hashtable ht = new Hashtable();
            //    for (int i = 0; i <= names.Length - 1; i++)
            //    {
            //        if (Strings.InStr(ActiveTypes, Convert.ToInt32(values.GetValue(i)).ToString()) > 0)
            //        {
            //            ht.Add(Convert.ToInt32(values.GetValue(i)).ToString(), names(i));
            //        }
            //    }
            //    return ht;
            //}

        }
    }

    public class AddOns
    {
        public enum EventType
        {
            none = 0,
            rsvpEvent = 1,
            ticketEvent = 2
        }
    }

    public class Times
    {
        public enum TimeZone
        {
            None = 0,
            ArizonaTimeZone = 1,
            EasternTimeZone = 2,
            CentralTimeZone = 3,
            MountainTimeZone = 4,
            PacificTimeZone = 5
        }
        public static double GetOffsetMinutes(TimeZone timeZone)
        {

            Double minutes = 0;

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(GetTimeZoneId(timeZone));
            TimeSpan offset = tzi.GetUtcOffset(System.DateTime.UtcNow);

            minutes = offset.TotalMinutes;

            return minutes;

        }
        public static string GetTimeZoneId(TimeZone timeZone)
        {
            string value = "";
            if (TimeZoneIds.TryGetValue(Convert.ToInt32(timeZone), out value))
            {
                return value;
            }
            else
            {
                return "Pacific Standard Time";
            }
        }

        private static readonly Dictionary<int, string> TimeZoneIds = new Dictionary<int, string> {
        {
          1,
          "Mountain Standard Time"
         },
         {
          2,
          "Eastern Standard Time"
         },
         {
          3,
          "Central Standard Time"
         },
         {
          4,
          "Mountain Standard Time"
         },
         {
          5,
          "Pacific Standard Time"
         },
         {
          6,
          "UTC-11"
         },
         {
          7,
          "Hawaiian Standard Time"
         },
         {
          8,
          "Alaskan Standard Time"
         },
         {
          9,
          "Pacific Standard Time (Mexico)"
         },
         {
          10,
          "US Mountain Standard Time"
         },
         {
          11,
          "Mountain Standard Time (Mexico)"
         },
         {
          12,
          "Central America Standard Time"
         },
         {
          13,
          "Central Standard Time (Mexico)"
         },
         {
          14,
          "Canada Central Standard Time"
         },
         {
          15,
          "SA Pacific Standard Time"
         },
         {
          16,
          "Eastern Standard Time (Mexico)"
         },
         {
          17,
          "US Eastern Standard Time"
         },
         {
          18,
          "Venezuela Standard Time"
         },
         {
          19,
          "Paraguay Standard Time"
         },
         {
          20,
          "Atlantic Standard Time"
         },
         {
          21,
          "Central Brazilian Standard Time"
         },
         {
          22,
          "SA Western Standard Time"
         },
         {
          23,
          "Newfoundland Standard Time"
         },
         {
          24,
          "E. South America Standard Time"
         },
         {
          25,
          "Argentina Standard Time"
         },
         {
          26,
          "SA Eastern Standard Time"
         },
         {
          27,
          "Greenland Standard Time"
         },
         {
          28,
          "Montevideo Standard Time"
         },
         {
          29,
          "Bahia Standard Time"
         },
         {
          30,
          "Pacific SA Standard Time"
         },
         {
          31,
          "UTC-02"
         },
         {
          32,
          "Mid-Atlantic Standard Time"
         },
         {
          33,
          "Azores Standard Time"
         },
         {
          34,
          "Cape Verde Standard Time"
         },
         {
          35,
          "Morocco Standard Time"
         },
         {
          36,
          "UTC"
         },
         {
          37,
          "GMT Standard Time"
         },
         {
          38,
          "Greenwich Standard Time"
         },
         {
          39,
          "W. Europe Standard Time"
         },
         {
          40,
          "Central Europe Standard Time"
         },
         {
          41,
          "Romance Standard Time"
         },
         {
          42,
          "Central European Standard Time"
         },
         {
          43,
          "W. Central Africa Standard Time"
         },
         {
          44,
          "Namibia Standard Time"
         },
         {
          45,
          "Jordan Standard Time"
         },
         {
          46,
          "GTB Standard Time"
         },
         {
          47,
          "Middle East Standard Time"
         },
         {
          48,
          "Egypt Standard Time"
         },
         {
          49,
          "Syria Standard Time"
         },
         {
          50,
          "E. Europe Standard Time"
         },
         {
          51,
          "South Africa Standard Time"
         },
         {
          52,
          "FLE Standard Time"
         },
         {
          53,
          "Turkey Standard Time"
         },
         {
          54,
          "Israel Standard Time"
         },
         {
          55,
          "Kaliningrad Standard Time"
         },
         {
          56,
          "Libya Standard Time"
         },
         {
          57,
          "Arabic Standard Time"
         },
         {
          58,
          "Arab Standard Time"
         },
         {
          59,
          "Belarus Standard Time"
         },
         {
          60,
          "Russian Standard Time"
         },
         {
          61,
          "E. Africa Standard Time"
         },
         {
          62,
          "Iran Standard Time"
         },
         {
          63,
          "Arabian Standard Time"
         },
         {
          64,
          "Azerbaijan Standard Time"
         },
         {
          65,
          "Russia Time Zone 3"
         },
         {
          66,
          "Mauritius Standard Time"
         },
         {
          67,
          "Georgian Standard Time"
         },
         {
          68,
          "Caucasus Standard Time"
         },
         {
          69,
          "Afghanistan Standard Time"
         },
         {
          70,
          "West Asia Standard Time"
         },
         {
          71,
          "Ekaterinburg Standard Time"
         },
         {
          72,
          "Pakistan Standard Time"
         },
         {
          73,
          "India Standard Time"
         },
         {
          74,
          "Sri Lanka Standard Time"
         },
         {
          75,
          "Nepal Standard Time"
         },
         {
          76,
          "Central Asia Standard Time"
         },
         {
          77,
          "Bangladesh Standard Time"
         },
         {
          78,
          "N. Central Asia Standard Time"
         },
         {
          79,
          "Myanmar Standard Time"
         },
         {
          80,
          "SE Asia Standard Time"
         },
         {
          81,
          "North Asia Standard Time"
         },
         {
          82,
          "China Standard Time"
         },
         {
          83,
          "North Asia East Standard Time"
         },
         {
          84,
          "Singapore Standard Time"
         },
         {
          85,
          "W. Australia Standard Time"
         },
         {
          86,
          "Taipei Standard Time"
         },
         {
          87,
          "Ulaanbaatar Standard Time"
         },
         {
          88,
          "Tokyo Standard Time"
         },
         {
          89,
          "Korea Standard Time"
         },
         {
          90,
          "Yakutsk Standard Time"
         },
         {
          91,
          "Cen. Australia Standard Time"
         },
         {
          92,
          "AUS Central Standard Time"
         },
         {
          93,
          "E. Australia Standard Time"
         },
         {
          94,
          "AUS Eastern Standard Time"
         },
         {
          95,
          "West Pacific Standard Time"
         },
         {
          96,
          "Tasmania Standard Time"
         },
         {
          97,
          "Magadan Standard Time"
         },
         {
          98,
          "Vladivostok Standard Time"
         },
         {
          99,
          "Russia Time Zone 10"
         },
         {
          100,
          "Central Pacific Standard Time"
         },
         {
          101,
          "Russia Time Zone 11"
         },
         {
          102,
          "New Zealand Standard Time"
         },
         {
          103,
          "UTC+12"
         },
         {
          104,
          "Fiji Standard Time"
         },
         {
          105,
          "Kamchatka Standard Time"
         },
         {
          106,
          "Tonga Standard Time"
         },
         {
          107,
          "Samoa Standard Time"
         },
         {
          108,
          "Line Islands Standard Time"
         },
         {
          109,
          "Dateline Standard Time"
         }
        };

        public static DateTime ToTimeZoneTime(DateTime dateTime, TimeZone TimeZone = TimeZone.PacificTimeZone)
        {
            string timezoneId = "Pacific Standard Time";

            string value = "";
            if (TimeZoneIds.TryGetValue(System.Convert.ToInt32(TimeZone), out value))
                timezoneId = value;

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);

            return TimeZoneInfo.ConvertTime(dateTime, tzi);
        }

        public static DateTime ToUniversalTime(DateTime dateTime, TimeZone TimeZone = TimeZone.PacificTimeZone)
        {
            if (dateTime == default(DateTime))
                dateTime = DateTime.UtcNow;

            // If the date is already in UTC then return it, otherwise we will have a kind mismatch exception.
            if (dateTime.Kind == DateTimeKind.Utc)
                return dateTime;

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(GetTimeZoneId(TimeZone));
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, tzi);
        }

    }

    public enum MobileNumberStatus
    {
        unverified = 0,
        verified = 1,
        failed = 2
    }

    public enum CardEntry
    {
        Swiped = 0,
        Keyed = 1,
        CardonFile = 2,
        EMVChip = 3,
        EMVTap = 4
    }

    public enum ApplicationType
    {
        GuestLink = 0,
        TablePro = 1,
        web = 2
    }

    public enum emailMarketingStatus
    {
        NA = -1,
        Subscribed = 0,
        Unsubscribed = 1
    }

    public enum ReservationSeatedStatus
    {
        UNSEATED = 0,
        SEATED = 1,
        CLOSED = 2
    }

    public enum ReservationPaymentStatus
    {
        NOPAYMENT = 0,
        PAIDFULL = 1,
        PAIDPARTIAL = 2,
        UNPAID = 3,
        OVERPAID = 4,
        REFUND = 5,
        SCHEDULED = 6
    }

    public enum AddOnGroupType
    {
        none = 0,
        menu = 1,
        experience = 2,
        upsell = 3,
        flexmenu = 4
    }

    public enum ActionSource
    {
        BackOffice = 0,
        Consumer = 1
    }

    public enum ReferralType
    {
        BackOffice = 0,
        CellarPass = 1,
        Widget = 2,
        Affiliate = 3,
        Mobile = 4,
        Referrer = 5,
        WineryReferral = 6,
        TablePro = 7,
        GuestLink = 8
    }

    public enum ResponseStatus
    {
        Failed = 0,
        Success = 1
    }

    public enum SaveType
    {
        Saved = 0,
        Updated = 1
    }

    public enum SlotType
    {
        Rule = 0,
        Exception = 1,
        isPrivate = 2
    }
    public enum EventStatus
    {
        Closed = 0,
        AutoClosed = 2,
        Active = 1
    }

    public enum AppType
    {
        BOXOFFICEAPP = 0,
        TABLEMANAGEMENTAPP = 1
    }

    public enum ItemType
    {
        Waitlists = 1,
        Reservations = 2,
        TableStatus = 3,
        FloorPlan = 4,
        Servers = 5,
        Serversessions = 6,
        TableBlockingIntervals = 7,
        SMSMessageRSVP = 8,
        SMSMessageWaitlist = 9,
        ReceiptSetting = 10
    }

    public enum enumShippingService
    {
        Standard = 1,
        Expedited,
        NextDay,
        Priority,
        SecondDay,
        Next,
        Second,
        Scheduled
    }

    public enum enumPaymentMethod
    {
        Cash = 1,
        CreditCard,
        Check,
        PurchaseOrder,
        PaymentPending
    }

    public enum enumCreditCardType
    {
        Visa = 1,
        MasterCard,
        AmericanExpress,
        Discover,
        Debit
    }

    public enum ModuleType
    {
        Reservation = 0,
        Ticketing = 1
    }


    public enum MailChimpOrderType
    {
        NA = 0,
        Ticket = 1,
        Reservation = 2
    }

    public enum EmailValidStatus
    {
        na = 0,
        valid = 1,
        invalid = 2
    }

    public enum EmailStatusType
    {
        validation = 0,
        webhook = 1,
        import = 2
    }

    public enum EmailWebhookEvent
    {
        na = 0,
        accepted = 1,
        rejected = 2,
        delivered = 3,
        failed = 4,
        opened = 5,
        clicked = 6,
        unsubscribed = 7,
        complained = 8
    }

    public class Weekdays
    {

        private enum weekday
        {
            //Mon = 1
            //Tue = 2
            //Wed = 4
            //Thu = 8
            //Fri = 16
            //Sat = 32
            //Sun = 64

            //Mon = 2
            //Tue = 4
            //Wed = 8
            //Thu = 16
            //Fri = 32
            //Sat = 64
            //Sun = 128

            Sun = 2,
            Mon = 4,
            Tue = 8,
            Wed = 16,
            Thu = 32,
            Fri = 64,
            Sat = 128

        }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }

        public static Weekdays GetWeekdays(int value)
        {
            Weekdays weekdays = new Weekdays();
            weekdays.AllDays = Convert.ToByte(value);

            return weekdays;
        }

        public Int32 AllDays
        {
            get
            {
                Int32 Value__1 = 0;
                // from LSB to MSB -  Monday, Tuesday, 
                // Wednesday, Thursday, Friday, Saturday, Sunday                       
                if (Monday)
                    Value__1 = Value__1 | Convert.ToByte(weekday.Mon);
                if (Tuesday)
                    Value__1 = Value__1 | Convert.ToByte(weekday.Tue);
                if (Wednesday)
                    Value__1 = Value__1 | Convert.ToByte(weekday.Wed);
                if (Thursday)
                    Value__1 = Value__1 | Convert.ToByte(weekday.Thu);
                if (Friday)
                    Value__1 = Value__1 | Convert.ToByte(weekday.Fri);
                if (Saturday)
                    Value__1 = Value__1 | Convert.ToByte(weekday.Sat);
                if (Sunday)
                    Value__1 = Value__1 | Convert.ToByte(weekday.Sun);
                return Value__1;
            }
            set
            {
                // Extract the corresponding bits for each weekday
                // into bool properties of the class
                Monday = ((value & Convert.ToByte(weekday.Mon)) != 0);
                Tuesday = ((value & Convert.ToByte(weekday.Tue)) != 0);
                Wednesday = ((value & Convert.ToByte(weekday.Wed)) != 0);
                Thursday = ((value & Convert.ToByte(weekday.Thu)) != 0);
                Friday = ((value & Convert.ToByte(weekday.Fri)) != 0);
                Saturday = ((value & Convert.ToByte(weekday.Sat)) != 0);
                Sunday = ((value & Convert.ToByte(weekday.Sun)) != 0);
            }
        }



    }

    public enum PreAssignServerTransactionType
    {
        WaitList = 1,
        Reservation = 2,

    }

    public enum CPImageType
    {
        MemberGallery = 3,
        MemberLogo = 7
    }

    public enum ImageType
    {
        cpImage = 0,
        memberImage = 1,
        rsvpEventImage = 2,
        ticketEventImage = 3,
        adImage = 4,
        user = 5,
        ProductImage = 6,
        addOnImage = 7,
        blogImage = 8,
        ReceiptLogo = 9,
        OrderSignature = 10,
        LocationMaps = 11,
        RsvpExport = 12
    }

    public enum ImagePathType
    {
        cdn = 0,
        azure = 1
    }

    public enum AddonItemCategory
    {
        None = 0,
        Accessories = 1,
        Clothing = 2,
        Experience = 3,
        Food = 4,
        Merchandise = 5,
        Upgrade = 6,
        Other = 7,
        Amenity = 8
    }

    public enum SignatureCaptureType
    {
        None = 0,
        OnDevice = 1,
        OnPrintedReceipt = 2
    }

    public enum SignatureType
    {
        CustomerPickup = 0,
        CreditCard = 1,
        Package = 2
    }

    public enum TagType
    {
        na = 0,
        special_event = 1,
        special_guests = 2
    }

    public enum LinkContentType
    {
        None = 0,
        CPNews = 1,
        TipsAndTricks = 2,
        HelpfulTips = 3
    }

    public enum CancellationReasonSetting
    {
        [Display(Description = "Disabled")]
        Disabled = 0,
        [Display(Description = "Required")]
        Required =1,
        [Display(Description = "Optional")]
        Optional =2
  
    }

    public enum ReservationInviteStatus
    {
        pending = 0,
        Complete = 1,
        CompletedbyAdmin = 2,
        Invalid =3
    }

    public enum RegionDirectorytemType
    {
        GettingHere = 0,
        Getaway = 1,
        WhereToDrink = 2,
        WhereToEat = 3,
        WhereToStay = 4,
        ThingsToDo = 5
    }

    public enum RegionTravelItemType
    {
        Air = 0,
        Personal = 1,
        PublicTravel = 2
    }

    public enum RegionSeasonsItemType
    {
        Winter = 0,
        Spring = 1,
        Summer = 2,
        Fall = 3
    }

    public enum LoginStatus
    {
        Failed,
        Locked,
        Success,
        Blocked
    }

    public enum AffiliateType
    {
        [Description("N/A")]
        NA = 0,
        [Description("Hotel")]
        Hotel = 1,
        [Description("Bed & Breakfast")]
        BedAndBreakfast = 2,
        [Description("Bus Driver")]
        BusDriver = 3,
        [Description("Concierge- Hotel")]
        ConciergeHotel = 4,
        [Description("Limo Driver")]
        LimoDriver = 5,
        [Description("Tour Operator")]
        TourOperator = 6,
        [Description("Transportation Company")]
        TransportationCompany = 7,
        [Description("Concierge- Private")]
        ConciergePrivate = 8,
        [Description("Concierge- Winery")]
        ConciergeWinery = 9,
        [Description("Distributor")]
        Distributor = 10,
        [Description("Press/Media")]
        PressMedia = 11,
        [Description("Other")]
        Other = 99
    }

    public enum CMSPageEntityType
    {
        Region = 1,
        SubRegion
    }

    public enum CmsPageSectionType
    {
        Html = 0,
        RegionList = 1,
        RegionSlider = 2,
        DestinationList = 3,
        DestinationSlider = 4,
        UserFavoritesList = 5,
        UserFavoritesSlider = 6,
        EventsList = 7,
        EventsSlider = 8,
        SubRegionGroupList = 9,
        SubRegionGroupSlider = 10,
        EventTypeList = 11,
        EventTypeSlider = 12,
        RecentlyViewedList = 13,
        RecentlyViewedSlider = 14,
        StateList = 15,
        SubRegionList = 16,
        SubRegionSlider = 17,
        EventExperienceList = 18,
        EventExperienceSlider = 19,
        Faq = 20,
        PointsProgram = 21,
        PrivacyPolicy = 22,
        RegionSeason = 23,
        RegionDirectory = 24,
        RegionTravel = 25,
        RegionAbout = 26,
        CookiePolicy = 27,
        TermsConditions = 28,
        JoinCellarpass = 29,
        TicketEventCatogoriesList = 30,
        TicketEventCatogoriesSlider = 31,
        PassportPrograms = 32,
        ChecnInOffers = 33,
        RsvpCheckInOffers = 34,
        ProfileOffers = 35,
        MemberGetawaysList = 36,
        MemberGetawaysSlider = 37,
        MemberImagesList = 38,
        MemberImagesSlider = 39,
        FeatureHighlights = 40,
        WhoWeCanHelp = 41,
        AdditionalBenefits = 42,
        ReservationStatistics = 43,
        CardLayout = 1001
    }

    public enum BusinessSearchResultType
    {
        Event = 0,
        Property = 1,
        Location = 2,
        Region = 3
    }
}
