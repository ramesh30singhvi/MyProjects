using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.WebApi.Services
{
    public class Discount
    {
        public class EventDiscountResult
        {
            public int DiscountId { get; set; }
            public decimal DiscountTotal { get; set; }
            public decimal DiscountGuestCount { get; set; }
            public string DiscountMsg { get; set; }
            public bool DiscountApplied { get; set; }
            public bool DiscountValid { get; set; } = false;
            public string DiscountDesc { get; set; }
            public List<ActivationCodesResult> ActivationCodes { get; set; }
            public bool AccessOnly { get; set; } = false;
            public bool ClubMember { get; set; } = false;
            public List<DiscountDetailsModel> club_discount_details { get; set; }
        }

        public class ActivationCodesResult
        {
            public string ActivationCode { get; set; }
            public string DiscountDesc { get; set; }
            public bool IsValid { get; set; } = false;
            public int TicketId { get; set; }
        }

        //public static EventDiscountResult DiscountCodeApply(int event_id, int quantity, decimal fee_per_person, string discount_code)
        //{
        //    EventDiscountResult result = new EventDiscountResult();

        //    result.DiscountMsg = "Promo Not Applicable";

        //    DiscountDAL discountDAL = new DiscountDAL(ConnectionString);
        //    var eventDiscount = new EventDiscount();

        //    eventDiscount = discountDAL.GetDiscountDetail(event_id, discount_code);

        //    if (eventDiscount.Id > 0)
        //    {
        //        EventDiscountResult eventDiscountResult = VerifyDiscount(eventDiscount, event_id, quantity);

        //        if (eventDiscountResult.DiscountValid)
        //        {
        //            decimal eventPrice = quantity * fee_per_person;
        //            result = CalculateDiscount(eventDiscount, eventPrice, quantity,event_id);
        //            result.DiscountId = eventDiscount.Id;
        //        }
        //    }

        //    return result;
        //}

        public static async Task<EventDiscountResult> CheckAndApplyEventDiscount(EventModel eventModel, int quantity, decimal fee_per_person, string discount_code, int MemberID, string email, List<string> passport_code, DateTime event_date,int fee_type, string ShopifyUrl, string ShopifyAuthToken)
        {
            EventDiscountResult result = new EventDiscountResult();
            EventDAL eventDAL = new EventDAL(ConnectionString);
            UserDAL userDAL = new UserDAL(ConnectionString);
            DiscountDAL discountDAL = new DiscountDAL(ConnectionString);

            if (email.ToLower().IndexOf("@noemail") > -1)
                email = "";

            var eventDiscount = new EventDiscount();
            bool discountCodeValid = false;
            WineryModel wineryModel = eventDAL.GetWineryById(MemberID);

            bool bLoyalClubLookupEnabled = Utility.CheckIfBloyalEnabledForMember(MemberID);
            bool AccessOnly = false;
            int applyclaimCode = 0;
            List<ActivationCodesResult> ActivationCodes = new List<ActivationCodesResult>();
            if (passport_code != null && applyclaimCode < quantity && passport_code.Count > 0)
            {
                foreach (var item in passport_code)
                {
                    string claimCode = Common.Common.Right(item.Replace("-", ""), 16);
                    PassportAvailableStatus pStatus = eventDAL.isPassportClaimAvailable(claimCode, MemberID, wineryModel.DisplayName, eventModel.EventID, event_date);

                    if (pStatus != null)
                    {
                        ActivationCodesResult code = new ActivationCodesResult();
                        if (pStatus.isAvailable)
                        {
                            applyclaimCode += 1;
                            code.IsValid = true;
                        }
                        code.ActivationCode = item;
                        code.DiscountDesc = pStatus.Message;
                        code.TicketId = pStatus.ticketId;
                        ActivationCodes.Add(code);
                    }
                }
            }

            if (applyclaimCode > 0)
            {
                result.DiscountApplied = true;
                result.DiscountDesc = "Passport Code Applied";
                result.DiscountId = 0;
                result.DiscountMsg = "Passport Code Applied";
                result.DiscountTotal = fee_per_person * applyclaimCode;
                result.DiscountValid = true;
                result.ActivationCodes = ActivationCodes;
            }
            else
            {
                if (!string.IsNullOrEmpty(discount_code))
                {
                    eventDiscount = discountDAL.GetDiscountDetail(eventModel.EventID, discount_code);

                    if (eventDiscount != null && eventDiscount.Id > 0)
                    {
                        EventDiscountResult eventDiscountResult = VerifyDiscount(eventDiscount, eventModel.EventID, quantity, event_date);

                        if (eventDiscountResult != null)
                        {
                            if (eventDiscountResult.DiscountValid)
                                discountCodeValid = eventDiscountResult.DiscountValid;
                            else
                            {
                                result.DiscountDesc = eventDiscountResult.DiscountMsg;
                                result.DiscountMsg = eventDiscountResult.DiscountMsg;
                            }
                        }
                    }
                }

                if (discountCodeValid == true && eventDiscount != null && !string.IsNullOrEmpty(discount_code))
                {
                    decimal eventPrice = quantity * fee_per_person;

                    if (fee_type == 1)
                        eventPrice = fee_per_person;

                    result = CalculateDiscount(eventDiscount, eventPrice, quantity, eventModel.EventID, event_date);
                    result.DiscountId = eventDiscount.Id;
                }

                if (result.DiscountTotal == 0 && string.IsNullOrEmpty(email) == false && string.IsNullOrEmpty(discount_code))
                {
                    decimal discount = 0;
                    bool ClubMember = false;
                    string discountmsg = "";
                    string discountdesc = "";
                    string MemberBenefitdiscountmsg = "";
                    string MemberBenefitdiscountdesc = "";
                    string MemberBenefitdesc = "";

                    var userDetailModel = new List<UserDetailModel>();
                    userDetailModel = await Utility.GetUsersByEmail(email, MemberID,false,ShopifyUrl,ShopifyAuthToken);

                    if (userDetailModel != null && userDetailModel.Count > 0 && userDetailModel[0].customer_type == 1)
                    {
                        AccessOnly = (eventModel.MemberBenefit == DiscountType.AccessOnly);
                        ClubMember = userDetailModel[0].customer_type == 1;
                    }

                    if (eventModel.MemberBenefit != DiscountType.None && eventModel.MemberBenefit != DiscountType.AccessOnly)
                    {
                        if (!string.IsNullOrEmpty(email))
                        {
                            if (wineryModel.EnableClubAMS == false && wineryModel.EnableClubMicroworks == false && wineryModel.EnableClubCoresense == false && wineryModel.EnableClubeCellar == false && wineryModel.EnableClubemember == false && wineryModel.EnableClubShopify == false)
                            {
                                if (userDetailModel != null && userDetailModel.Count > 0 && userDetailModel[0].customer_type == 1)
                                {
                                    ClubMember = true;
                                    if (fee_per_person > 0)
                                    {
                                        decimal MemberBenefitCustomValue = 0;
                                        if (eventModel.MemberBenefit == DiscountType.CustomDiscountAmt || eventModel.MemberBenefit == DiscountType.CustomDiscountPercent)
                                            MemberBenefitCustomValue = userDAL.GetEventMemberBenefitCustomValue(eventModel.EventID);

                                        decimal tempDisc = GetMemberBenefitDiscount(eventModel.MemberBenefit, quantity, fee_per_person, fee_type, MemberBenefitCustomValue);
                                        discount = tempDisc;
                                        MemberBenefitdiscountmsg = wineryModel.CurrencySymbol + string.Format("{0:0.00}", discount) + " discount applied due to Member benefits " + GetMemberBenefitDescByValue(eventModel.MemberBenefit);
                                        MemberBenefitdiscountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetMemberBenefitDescByValue(eventModel.MemberBenefit));
                                        MemberBenefitdesc = GetMemberBenefitDescByValue(eventModel.MemberBenefit);
                                    }
                                }
                            }
                        }
                    }

                    decimal discountAmount = 0;

                    List<AccountTypeDiscountModel> atDiscounts = new List<AccountTypeDiscountModel>();
                    atDiscounts = userDAL.LoadAccountTypeDiscounts(eventModel.EventID);

                    if (wineryModel.EnableClubeCellar || wineryModel.EnableClubAMS || wineryModel.EnableClubMicroworks || wineryModel.EnableClubCoresense)
                    {
                        if (userDetailModel != null && userDetailModel.Count > 0)
                        {
                            if (userDetailModel[0].contact_types.Count > 0)
                            {
                                string Type = string.Empty;
                                string Tier = string.Empty;

                                if (userDetailModel[0].contact_types.Count > 0)
                                    Type = userDetailModel[0].contact_types[0];

                                if (userDetailModel[0].contact_types.Count > 1)
                                    Tier = userDetailModel[0].contact_types[1];


                                if (atDiscounts != null && atDiscounts.Count > 0)
                                {
                                    var accountTypeDiscount = atDiscounts.Where(f => f.ContactType == Type).FirstOrDefault();
                                    if (accountTypeDiscount != null)
                                    {
                                        if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                                            AccessOnly = true;

                                        ClubMember = true;

                                        if (fee_per_person > 0)
                                        {
                                            decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, quantity, fee_per_person, fee_type, accountTypeDiscount.MemberBenefitCustomValue);
                                            if (tempDisc > discountAmount)
                                            {
                                                discountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                discountAmount = tempDisc;
                                                discountmsg = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Contact Type", GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));

                                                MemberBenefitdesc = GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                            }
                                        }
                                    }

                                    accountTypeDiscount = atDiscounts.Where(f => f.ContactType == Tier).FirstOrDefault();
                                    if (accountTypeDiscount != null)
                                    {
                                        if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                                            AccessOnly = true;

                                        ClubMember = true;

                                        if (fee_per_person > 0)
                                        {
                                            decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, quantity, fee_per_person, fee_type, accountTypeDiscount.MemberBenefitCustomValue);
                                            if (tempDisc > discountAmount)
                                            {
                                                discountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                discountAmount = tempDisc;
                                                discountmsg = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Contact Type", GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));

                                                MemberBenefitdesc = GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                    else if (wineryModel.EnableClubVin65 && !string.IsNullOrWhiteSpace(wineryModel.Vin65UserName)
                    && !string.IsNullOrWhiteSpace(wineryModel.Vin65Password))
                    {
                        List<Vin65Model> modelList = new List<Vin65Model>();
                        modelList = await Task.Run(() => Utility.Vin65GetContacts(email, wineryModel.Vin65Password, wineryModel.Vin65UserName));

                        if (modelList != null && modelList.Count > 0)
                        {
                            Vin65Model vin65Model = new Vin65Model();
                            vin65Model = modelList[0];
                            if (vin65Model != null && vin65Model.contact_types != null && vin65Model.contact_types.Count > 0 && vin65Model.member_status == true)
                            {
                                if (atDiscounts != null && atDiscounts.Count > 0)
                                {
                                    foreach (var ct in vin65Model.contact_types)
                                    {
                                        var accountTypeDiscount = atDiscounts.Where(f => f.ContactType == ct).FirstOrDefault();
                                        if (accountTypeDiscount != null)
                                        {
                                            if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                                                AccessOnly = true;

                                            ClubMember = true;

                                            if (fee_per_person > 0)
                                            {
                                                decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, quantity, fee_per_person, fee_type, accountTypeDiscount.MemberBenefitCustomValue);
                                                if (tempDisc > discountAmount)
                                                {
                                                    discountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                    discountAmount = tempDisc;
                                                    discountmsg = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Contact Type", GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));

                                                    MemberBenefitdesc = GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (
                       (wineryModel.EnableClubOrderPort && !string.IsNullOrWhiteSpace(wineryModel.OrderPortClientId)
                    && !string.IsNullOrWhiteSpace(wineryModel.OrderPortApiKey) && !string.IsNullOrWhiteSpace(wineryModel.OrderPortApiToken))
                    || (wineryModel.EnableClubCommerce7 && !string.IsNullOrWhiteSpace(wineryModel.Commerce7Username)
                    && !string.IsNullOrWhiteSpace(wineryModel.Commerce7Password) && !string.IsNullOrWhiteSpace(wineryModel.Commerce7Tenant))
                    || (wineryModel.EnableClubShopify && !string.IsNullOrWhiteSpace(wineryModel.ShopifyAppPassword)
                    && !string.IsNullOrWhiteSpace(wineryModel.ShopifyAppStoreName) && !string.IsNullOrWhiteSpace(wineryModel.ShopifyPublishKey))
                    || bLoyalClubLookupEnabled
                    )
                    {
                        //userDetailModel = new List<UserDetailModel>();
                        //userDetailModel = await Task.Run(() => Utility.GetCustomersByNameOrEmail(email, wineryModel.OrderPortApiKey, wineryModel.OrderPortApiToken, wineryModel.OrderPortClientId));

                        if (userDetailModel != null && userDetailModel.Count > 0)
                        {
                            if (userDetailModel[0].contact_types.Count > 0)
                            {
                                if (atDiscounts != null && atDiscounts.Count > 0)
                                {
                                    if (wineryModel.EnableClubCommerce7 && !string.IsNullOrWhiteSpace(wineryModel.Commerce7Username)
                                        && !string.IsNullOrWhiteSpace(wineryModel.Commerce7Password) && !string.IsNullOrWhiteSpace(wineryModel.Commerce7Tenant))
                                    {
                                        foreach (var ct in userDetailModel[0].contact_types_id)
                                        {
                                            var accountTypeDiscount = atDiscounts.Where(f => f.ContactTypeId.Trim().ToLower() == ct.Trim().ToLower()).FirstOrDefault();
                                            if (accountTypeDiscount != null)
                                            {
                                                if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                                                    AccessOnly = true;

                                                ClubMember = true;

                                                if (fee_per_person > 0)
                                                {
                                                    decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, quantity, fee_per_person, fee_type, accountTypeDiscount.MemberBenefitCustomValue);
                                                    if (tempDisc > discountAmount)
                                                    {
                                                        discountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                        discountAmount = tempDisc;
                                                        discountmsg = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Club Membership", GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));

                                                        MemberBenefitdesc = GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (var ct in userDetailModel[0].contact_types)
                                        {
                                            var accountTypeDiscount = atDiscounts.Where(f => f.ContactType.Trim().ToLower() == ct.Trim().ToLower()).FirstOrDefault();
                                            if (accountTypeDiscount != null)
                                            {
                                                if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                                                    AccessOnly = true;

                                                ClubMember = true;

                                                if (fee_per_person > 0)
                                                {
                                                    decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, quantity, fee_per_person, fee_type, accountTypeDiscount.MemberBenefitCustomValue);
                                                    if (tempDisc > discountAmount)
                                                    {
                                                        discountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                        discountAmount = tempDisc;
                                                        discountmsg = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Club Membership", GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));

                                                        MemberBenefitdesc = GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (wineryModel.EnableClubemember && !string.IsNullOrWhiteSpace(wineryModel.eMemberUserNAme)
                        && !string.IsNullOrWhiteSpace(wineryModel.eMemberPAssword))
                    {
                        List<ViewModels.eWineryCustomerViewModel> modelList = new List<ViewModels.eWineryCustomerViewModel>();
                        modelList = await Task.Run(() => Utility.ResolveCustomer(wineryModel.eMemberUserNAme, wineryModel.eMemberPAssword, email,MemberID));

                        if (modelList != null && modelList.Count > 0)
                        {
                            ViewModels.eWineryCustomerViewModel eWineryCustomerViewModel = new ViewModels.eWineryCustomerViewModel();
                            eWineryCustomerViewModel = modelList[0];
                            if (eWineryCustomerViewModel != null && eWineryCustomerViewModel.memberships != null && eWineryCustomerViewModel.memberships.Count > 0)
                            {
                                if (atDiscounts != null && atDiscounts.Count > 0)
                                {
                                    foreach (var ct in eWineryCustomerViewModel.memberships)
                                    {
                                        var accountTypeDiscount = atDiscounts.Where(f => f.ContactTypeId == ct.club_id).FirstOrDefault();
                                        if (accountTypeDiscount != null)
                                        {
                                            if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                                                AccessOnly = true;

                                            ClubMember = true;

                                            if (fee_per_person > 0)
                                            {
                                                decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, quantity, fee_per_person, fee_type, accountTypeDiscount.MemberBenefitCustomValue);
                                                if (tempDisc > discountAmount)
                                                {
                                                    discountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                    discountAmount = tempDisc;
                                                    discountmsg = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Contact Type", GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));

                                                    MemberBenefitdesc = GetMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (discountAmount > discount)
                    {
                        discount = discountAmount;
                    }
                    else
                    {
                        discountmsg = MemberBenefitdiscountmsg;
                        discountdesc = MemberBenefitdiscountdesc;
                    }

                    if (discount > 0)
                    {
                        result.DiscountApplied = true;
                        result.DiscountDesc = discountdesc;
                        //result.DiscountId = 0;
                        result.DiscountMsg = discountmsg;
                        result.DiscountTotal = discount;
                        result.DiscountValid = true;
                    }
                    result.AccessOnly = AccessOnly;
                    result.ClubMember = ClubMember;

                    if (ClubMember && !string.IsNullOrEmpty(MemberBenefitdesc))
                    {
                        List<DiscountDetailsModel> discount_details = new List<DiscountDetailsModel>();
                        DiscountDetailsModel discountdetail = new DiscountDetailsModel();

                        discountdetail.discount_amount = discount;
                        discountdetail.name = "Event Fees";
                        discountdetail.member_benefit_desc = MemberBenefitdesc;

                        discount_details.Add(discountdetail);

                        result.club_discount_details = discount_details;
                    }
                }
                //else if (eventDiscount != null)
                //{
                //    decimal eventPrice = quantity * fee_per_person;

                //    if (fee_type == 1)
                //        eventPrice = fee_per_person;

                //    result = CalculateDiscount(eventDiscount, eventPrice, quantity, eventModel.EventID);
                //    result.DiscountId = eventDiscount.Id;
                //}
            }

            return result;
        }

        public static async Task<EventDiscountResult> CheckAndApplyEventAddOnsDiscount(List<Addon_info> addon_info, int MemberID, string email, string ShopifyUrl, string ShopifyAuthToken)
        {
            EventDiscountResult result = new EventDiscountResult();
            EventDAL eventDAL = new EventDAL(ConnectionString);
            UserDAL userDAL = new UserDAL(ConnectionString);
            DiscountDAL discountDAL = new DiscountDAL(ConnectionString);

            if (email.ToLower().IndexOf("@noemail") > -1)
                email = "";

            var eventDiscount = new EventDiscount();
            bool discountCodeValid = false;
            WineryModel wineryModel = eventDAL.GetWineryById(MemberID);

            bool bLoyalClubLookupEnabled = Utility.CheckIfBloyalEnabledForMember(MemberID);
            bool AccessOnly = false;

            if (discountCodeValid == false && string.IsNullOrEmpty(email) == false)
            {
                bool ClubMember = false;
                
                
                //string MemberBenefitdiscountmsg = "";
                //string MemberBenefitdiscountdesc = "";
                List<DiscountDetailsModel> discount_details = new List<DiscountDetailsModel>();

                var userDetailModel = new List<UserDetailModel>();
                userDetailModel = await Utility.GetUsersByEmail(email, MemberID, false, ShopifyUrl, ShopifyAuthToken);

                foreach (var item in addon_info)
                {
                    decimal discountAmount = 0;
                    
                    string MemberBenefitdesc = "";
                    string discountmsg = "";
                    string discountdesc = "";

                    decimal discount = 0;
                    string MemberBenefitdesc2 = "";
                    string discountmsg2 = "";
                    string discountdesc2 = "";

                    List<AccountTypeDiscountModel> atDiscounts = new List<AccountTypeDiscountModel>();
                    atDiscounts = userDAL.LoadAddOnsAccountTypeDiscounts(item.group_id,item.item_id);

                    if (atDiscounts != null && atDiscounts.Count > 0)
                    {
                        var accountTypeDiscount = atDiscounts.Where(f => f.Id == 0).FirstOrDefault();
                        if (accountTypeDiscount != null)
                        {
                            //if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                            //    AccessOnly = true;

                            //ClubMember = true;

                            AccessOnly = (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly);
                            ClubMember = userDetailModel[0].customer_type == 1;

                            if (item.price > 0 && ClubMember)
                            {
                                decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, item.qty, item.price, 0, accountTypeDiscount.MemberBenefitCustomValue);
                                if (tempDisc > discount)
                                {
                                    discountdesc2 = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                    discount = tempDisc;
                                    discountmsg2 = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Contact Type", GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                    MemberBenefitdesc2 = GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                }
                            }
                        }
                    }

                    if (wineryModel.EnableClubeCellar || wineryModel.EnableClubAMS || wineryModel.EnableClubMicroworks || wineryModel.EnableClubCoresense)
                    {
                        if (userDetailModel != null && userDetailModel.Count > 0)
                        {
                            if (userDetailModel[0].contact_types.Count > 0)
                            {
                                string Type = string.Empty;
                                string Tier = string.Empty;

                                if (userDetailModel[0].contact_types.Count > 0)
                                    Type = userDetailModel[0].contact_types[0];

                                if (userDetailModel[0].contact_types.Count > 1)
                                    Tier = userDetailModel[0].contact_types[1];


                                if (atDiscounts != null && atDiscounts.Count > 0)
                                {
                                    var accountTypeDiscount = atDiscounts.Where(f => f.ContactType == Type).FirstOrDefault();
                                    if (accountTypeDiscount != null)
                                    {
                                        if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                                            AccessOnly = true;

                                        ClubMember = true;

                                        if (item.price > 0)
                                        {
                                            decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, item.qty, item.price, 0, accountTypeDiscount.MemberBenefitCustomValue);
                                            if (tempDisc > discountAmount)
                                            {
                                                discountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                discountAmount = tempDisc;
                                                discountmsg = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Contact Type", GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                MemberBenefitdesc = GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                            }
                                        }
                                    }

                                    accountTypeDiscount = atDiscounts.Where(f => f.ContactType == Tier).FirstOrDefault();
                                    if (accountTypeDiscount != null)
                                    {
                                        if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                                            AccessOnly = true;

                                        ClubMember = true;

                                        if (item.price > 0)
                                        {
                                            decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, item.qty, item.price, 0, accountTypeDiscount.MemberBenefitCustomValue);
                                            if (tempDisc > discountAmount)
                                            {
                                                discountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                discountAmount = tempDisc;
                                                discountmsg = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Contact Type", GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));

                                                MemberBenefitdesc = GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                    else if (wineryModel.EnableClubVin65 && !string.IsNullOrWhiteSpace(wineryModel.Vin65UserName)
                    && !string.IsNullOrWhiteSpace(wineryModel.Vin65Password))
                    {
                        List<Vin65Model> modelList = new List<Vin65Model>();
                        modelList = await Task.Run(() => Utility.Vin65GetContacts(email, wineryModel.Vin65Password, wineryModel.Vin65UserName));

                        if (modelList != null && modelList.Count > 0)
                        {
                            Vin65Model vin65Model = new Vin65Model();
                            vin65Model = modelList[0];
                            if (vin65Model != null && vin65Model.contact_types != null && vin65Model.contact_types.Count > 0 && vin65Model.member_status == true)
                            {
                                if (atDiscounts != null && atDiscounts.Count > 0)
                                {
                                    foreach (var ct in vin65Model.contact_types)
                                    {
                                        var accountTypeDiscount = atDiscounts.Where(f => f.ContactType == ct).FirstOrDefault();
                                        if (accountTypeDiscount != null)
                                        {
                                            if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                                                AccessOnly = true;

                                            ClubMember = true;

                                            if (item.price > 0)
                                            {
                                                decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, item.qty, item.price, 0, accountTypeDiscount.MemberBenefitCustomValue);
                                                if (tempDisc > discountAmount)
                                                {
                                                    discountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                    discountAmount = tempDisc;
                                                    discountmsg = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Contact Type", GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));

                                                    MemberBenefitdesc = GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (
                       (wineryModel.EnableClubOrderPort && !string.IsNullOrWhiteSpace(wineryModel.OrderPortClientId)
                    && !string.IsNullOrWhiteSpace(wineryModel.OrderPortApiKey) && !string.IsNullOrWhiteSpace(wineryModel.OrderPortApiToken))
                    || (wineryModel.EnableClubCommerce7 && !string.IsNullOrWhiteSpace(wineryModel.Commerce7Username)
                    && !string.IsNullOrWhiteSpace(wineryModel.Commerce7Password) && !string.IsNullOrWhiteSpace(wineryModel.Commerce7Tenant))
                    || (wineryModel.EnableClubShopify && !string.IsNullOrWhiteSpace(wineryModel.ShopifyAppPassword)
                    && !string.IsNullOrWhiteSpace(wineryModel.ShopifyAppStoreName) && !string.IsNullOrWhiteSpace(wineryModel.ShopifyPublishKey))
                    || bLoyalClubLookupEnabled
                    )
                    {
                        if (userDetailModel != null && userDetailModel.Count > 0)
                        {
                            if (userDetailModel[0].contact_types.Count > 0)
                            {
                                if (atDiscounts != null && atDiscounts.Count > 0)
                                {
                                    if (wineryModel.EnableClubCommerce7 && !string.IsNullOrWhiteSpace(wineryModel.Commerce7Username)
                                        && !string.IsNullOrWhiteSpace(wineryModel.Commerce7Password) && !string.IsNullOrWhiteSpace(wineryModel.Commerce7Tenant))
                                    {
                                        foreach (var ct in userDetailModel[0].contact_types_id)
                                        {
                                            var accountTypeDiscount = atDiscounts.Where(f => f.ContactTypeId.Trim().ToLower() == ct.Trim().ToLower()).FirstOrDefault();
                                            if (accountTypeDiscount != null)
                                            {
                                                if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                                                    AccessOnly = true;

                                                ClubMember = true;

                                                if (item.price > 0)
                                                {
                                                    decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, item.qty, item.price, 0, accountTypeDiscount.MemberBenefitCustomValue);
                                                    if (tempDisc > discountAmount)
                                                    {
                                                        discountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                        discountAmount = tempDisc;
                                                        discountmsg = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Club Membership", GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));

                                                        MemberBenefitdesc = GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (var ct in userDetailModel[0].contact_types)
                                        {
                                            var accountTypeDiscount = atDiscounts.Where(f => f.ContactType.Trim().ToLower() == ct.Trim().ToLower()).FirstOrDefault();
                                            if (accountTypeDiscount != null)
                                            {
                                                if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                                                    AccessOnly = true;

                                                ClubMember = true;

                                                if (item.price > 0)
                                                {
                                                    decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, item.qty, item.price, 0, accountTypeDiscount.MemberBenefitCustomValue);
                                                    if (tempDisc > discountAmount)
                                                    {
                                                        discountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                        discountAmount = tempDisc;
                                                        discountmsg = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Club Membership", GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));

                                                        MemberBenefitdesc = GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                                    }
                                                }
                                            }
                                        }
                                    }   
                                }
                            }
                        }
                    }
                    else if (wineryModel.EnableClubemember && !string.IsNullOrWhiteSpace(wineryModel.eMemberUserNAme)
                        && !string.IsNullOrWhiteSpace(wineryModel.eMemberPAssword))
                    {
                        List<ViewModels.eWineryCustomerViewModel> modelList = new List<ViewModels.eWineryCustomerViewModel>();
                        modelList = await Task.Run(() => Utility.ResolveCustomer(wineryModel.eMemberUserNAme, wineryModel.eMemberPAssword, email, MemberID));

                        if (modelList != null && modelList.Count > 0)
                        {
                            ViewModels.eWineryCustomerViewModel eWineryCustomerViewModel = new ViewModels.eWineryCustomerViewModel();
                            eWineryCustomerViewModel = modelList[0];
                            if (eWineryCustomerViewModel != null && eWineryCustomerViewModel.memberships != null && eWineryCustomerViewModel.memberships.Count > 0)
                            {
                                if (atDiscounts != null && atDiscounts.Count > 0)
                                {
                                    foreach (var ct in eWineryCustomerViewModel.memberships)
                                    {
                                        var accountTypeDiscount = atDiscounts.Where(f => f.ContactTypeId == ct.club_id).FirstOrDefault();
                                        if (accountTypeDiscount != null)
                                        {
                                            if (accountTypeDiscount.MemberBenefit == DiscountType.AccessOnly)
                                                AccessOnly = true;

                                            ClubMember = true;

                                            if (item.price > 0)
                                            {
                                                decimal tempDisc = GetMemberBenefitDiscount(accountTypeDiscount.MemberBenefit, item.qty, item.price, 0, accountTypeDiscount.MemberBenefitCustomValue);
                                                if (tempDisc > discountAmount)
                                                {
                                                    discountdesc = string.Format("As being an active member at {0}, you will receive {1}", wineryModel.DisplayName, GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));
                                                    discountAmount = tempDisc;
                                                    discountmsg = string.Format("{0} discount applied {1} - {2}", wineryModel.CurrencySymbol + string.Format("{0:0.00}", discountAmount), "due to Contact Type", GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit));

                                                    MemberBenefitdesc = GetAddOnsMemberBenefitDescByValue(accountTypeDiscount.MemberBenefit);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //If discount from account/contact types > club member then use it instead
                    if (discountAmount > discount) {
                        discount = discountAmount;
                    }
                    else
                    {
                        MemberBenefitdesc = MemberBenefitdesc2;
                        discountmsg = discountmsg2;
                        discountdesc = discountdesc2;
                    }

                    //if (discountAmount > 0)
                    //{
                    //    discount = discountAmount;
                    //    discountAmount = 0;
                    //}
                    //else
                    //{
                    //    discountmsg = MemberBenefitdiscountmsg;
                    //    discountdesc = MemberBenefitdiscountdesc;
                    //}
                    if (discount > 0)
                    {
                        result.DiscountApplied = true;

                        if (!string.IsNullOrEmpty(result.DiscountDesc))
                            result.DiscountDesc = result.DiscountDesc + ", ";

                        result.DiscountDesc = result.DiscountDesc + discountdesc;

                        result.DiscountId = 0;

                        if (!string.IsNullOrEmpty(result.DiscountMsg))
                            result.DiscountMsg = result.DiscountMsg + ", ";

                        result.DiscountMsg = result.DiscountMsg + discountmsg;

                        result.DiscountTotal = result.DiscountTotal + discount;

                        result.DiscountValid = true;

                        if (!string.IsNullOrEmpty(MemberBenefitdesc))
                        {
                            DiscountDetailsModel discountdetail = new DiscountDetailsModel();

                            discountdetail.discount_amount = discount;
                            discountdetail.name = userDAL.GetAddOnsGroupNameById(item.group_id);
                            discountdetail.member_benefit_desc = MemberBenefitdesc;

                            discount_details.Add(discountdetail);
                        }
                    }
                    result.AccessOnly = AccessOnly;
                    result.ClubMember = ClubMember;

                    result.club_discount_details = discount_details;
                }
            }

            return result;
        }

        public static decimal GetMemberBenefitDiscount(DiscountType memberBenefitId, int guestCount, decimal feePerPerson,int feetype,decimal CustomValue)
        {
            decimal discountAmount = 0;

            if (feePerPerson > 0 && guestCount > 0 && memberBenefitId != DiscountType.None)
            {
                if (feetype == 1)
                {
                    guestCount = 1;

                    if (memberBenefitId == DiscountType.Comp1 || memberBenefitId == DiscountType.Comp10 || memberBenefitId == DiscountType.Comp12
                         || memberBenefitId == DiscountType.Comp14 || memberBenefitId == DiscountType.Comp16 || memberBenefitId == DiscountType.Comp18
                          || memberBenefitId == DiscountType.Comp2 || memberBenefitId == DiscountType.Comp20 || memberBenefitId == DiscountType.Comp3
                           || memberBenefitId == DiscountType.Comp4 || memberBenefitId == DiscountType.Comp6 || memberBenefitId == DiscountType.Comp5 || memberBenefitId == DiscountType.Comp8
                            || memberBenefitId == DiscountType.CompUnlimited || memberBenefitId == DiscountType.PP100Off || memberBenefitId == DiscountType.PP10Off
                             || memberBenefitId == DiscountType.PP15Off || memberBenefitId == DiscountType.PP20Off || memberBenefitId == DiscountType.PP25Off || memberBenefitId == DiscountType.PP30Off
                              || memberBenefitId == DiscountType.PP35Off || memberBenefitId == DiscountType.PP40Off || memberBenefitId == DiscountType.PP45Off || memberBenefitId == DiscountType.PP50Off
                               || memberBenefitId == DiscountType.PP55Off || memberBenefitId == DiscountType.PP55Off || memberBenefitId == DiscountType.PP5Off || memberBenefitId == DiscountType.PP60Off
                                || memberBenefitId == DiscountType.PP65Off || memberBenefitId == DiscountType.PP70Off || memberBenefitId == DiscountType.PP75Off || memberBenefitId == DiscountType.PP80Off
                                 || memberBenefitId == DiscountType.PP85Off || memberBenefitId == DiscountType.PP90Off || memberBenefitId == DiscountType.PP95Off)
                    {
                        memberBenefitId = DiscountType.None;
                    }

                }

                switch (memberBenefitId)
                {
                    case DiscountType.Comp1: //Comp up to 1
                        discountAmount = feePerPerson;
                        break;
                    case DiscountType.Comp2: //Comp up to 2
                        if (guestCount > 2)
                            discountAmount = (feePerPerson * 2);
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.Comp3: //Comp up to 3
                        if (guestCount > 3)
                            discountAmount = (feePerPerson * 3);
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.Comp4: //Comp up to 4
                        if (guestCount > 4)
                            discountAmount = (feePerPerson * 4);
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.Comp5: //Comp up to 5
                        if (guestCount > 5)
                            discountAmount = (feePerPerson * 5);
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.Comp6: //Comp up to 6
                        if (guestCount > 6)
                            discountAmount = (feePerPerson * 6);
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.Comp8: //Comp up to 8
                        if (guestCount > 8)
                            discountAmount = (feePerPerson * 8);
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.Comp10: //Comp up to 10
                        if (guestCount > 10)
                            discountAmount = (feePerPerson * 10);
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.Comp12: //Comp up to 12
                        if (guestCount > 12)
                            discountAmount = (feePerPerson * 12);
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.Comp14: //Comp up to 14
                        if (guestCount > 14)
                            discountAmount = (feePerPerson * 14);
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.Comp16: //Comp up to 16
                        if (guestCount > 16)
                            discountAmount = (feePerPerson * 16);
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.Comp18: //Comp up to 18
                        if (guestCount > 18)
                            discountAmount = (feePerPerson * 18);
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.Comp20: //Comp up to 20
                        if (guestCount > 20)
                            discountAmount = (feePerPerson * 20);
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.CompUnlimited: //Comp unlimited
                        discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.Percent5:
                        discountAmount = (feePerPerson * guestCount * 5) / 100;
                        break;
                    case DiscountType.Percent50: //50% discount
                        discountAmount = (feePerPerson * guestCount * 50) / 100;
                        break;
                    case DiscountType.Percent25: //25% discount
                        discountAmount = (feePerPerson * guestCount * 25) / 100;
                        break;
                    case DiscountType.Percent15: //15% discount
                        discountAmount = (feePerPerson * guestCount * 15) / 100;
                        break;
                    case DiscountType.Percent30: //30% discount
                        discountAmount = (feePerPerson * guestCount * 30) / 100;
                        break;
                    case DiscountType.Percent10: //10% discount
                        discountAmount = (feePerPerson * guestCount * 10) / 100;
                        break;
                    case DiscountType.Percent20: //20% discount
                        discountAmount = (feePerPerson * guestCount * 20) / 100;
                        break;
                    case DiscountType.Percent35: //35% discount
                        discountAmount = (feePerPerson * guestCount * 35) / 100;
                        break;
                    case DiscountType.CustomDiscountPercent:
                        discountAmount = (feePerPerson * guestCount * CustomValue) / 100;
                        break;
                    case DiscountType.CustomDiscountAmt:
                        if ((feePerPerson * guestCount) > CustomValue)
                            discountAmount = CustomValue;
                        else
                            discountAmount = feePerPerson * guestCount;
                        break;
                    case DiscountType.PP5Off: //$5 Off Per Person
                        if (feePerPerson >= 5)
                            discountAmount = 5 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP10Off: //$10 Off Per Person
                        if (feePerPerson >= 10)
                            discountAmount = 10 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP15Off: //$15 Off Per Person
                        if (feePerPerson >= 15)
                            discountAmount = 15 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP20Off: //$20 Off Per Person
                        if (feePerPerson >= 20)
                            discountAmount = 20 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP25Off: //$25 Off Per Person
                        if (feePerPerson >= 25)
                            discountAmount = 25 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP30Off: //$30 Off Per Person
                        if (feePerPerson >= 30)
                            discountAmount = 30 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP35Off: //$35 Off Per Person
                        if (feePerPerson >= 35)
                            discountAmount = 35 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP40Off: //$40 Off Per Person
                        if (feePerPerson >= 40)
                            discountAmount = 40 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP45Off: //$45 Off Per Person
                        if (feePerPerson >= 45)
                            discountAmount = 45 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP50Off: //$50 Off Per Person
                        if (feePerPerson >= 50)
                            discountAmount = 50 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP55Off: //$55 Off Per Person
                        if (feePerPerson >= 55)
                            discountAmount = 55 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP60Off: //$60 Off Per Person
                        if (feePerPerson >= 60)
                            discountAmount = 60 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP65Off: //$65 Off Per Person
                        if (feePerPerson >= 65)
                            discountAmount = 65 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP70Off: //$70 Off Per Person
                        if (feePerPerson >= 70)
                            discountAmount = 70 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP75Off: //$75 Off Per Person
                        if (feePerPerson >= 75)
                            discountAmount = 75 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP80Off: //$80 Off Per Person
                        if (feePerPerson >= 80)
                            discountAmount = 80 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP85Off: //$85 Off Per Person
                        if (feePerPerson >= 85)
                            discountAmount = 85 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP90Off: //$90 Off Per Person
                        if (feePerPerson >= 90)
                            discountAmount = 90 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP95Off: //$95 Off Per Person
                        if (feePerPerson >= 95)
                            discountAmount = 95 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                    case DiscountType.PP100Off: //$100 Off Per Person
                        if (feePerPerson >= 100)
                            discountAmount = 100 * guestCount;
                        else
                            discountAmount = (feePerPerson * guestCount);
                        break;
                }
            }
            return discountAmount;
        }

        public static EventDiscountResult VerifyDiscount(EventDiscount discount, int eventId, int numberOfGuests,DateTime event_date)
        {
            EventDiscountResult result = new EventDiscountResult();

            result.DiscountMsg = "Promo Not Applicable";

            if ((discount != null))
            {
                if (discount.Id > 0)
                {
                    //Less expensive checks
                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                    int TimeZoneId = eventDAL.GetTimeZonebyEventId(eventId);
                    DateTime localDateTime = Common.Times.ToTimeZoneTime(DateTime.UtcNow, (Common.Times.TimeZone)TimeZoneId);

                    if (discount.DateType == DateType.ByEventDate)
                        localDateTime = event_date;

                    if (discount.Active == true && discount.StartDateTime <= localDateTime && discount.EndDateTime > localDateTime && numberOfGuests >= discount.RequiredMinimum)
                    {
                        //If passes these checks then check use count for Total Reservations
                        if (discount.NumberOfUses == 0)
                        {
                            //Unlimited
                            result.DiscountValid = true;
                            result.DiscountMsg = "";
                        }
                        else
                        {
                            //Check Qty used (more expensive)
                            DiscountDAL discountDAL = new DiscountDAL(ConnectionString);

                            int useCount = discountDAL.GetDiscountUseCountByCode(eventId, discount.DiscountCode);
                            if (useCount < discount.NumberOfUses)
                            {
                                result.DiscountValid = true;
                                result.DiscountMsg = "";
                            }
                        }
                    }
                    else
                    {
                        if (discount.Active == true)
                        {
                            if (numberOfGuests < discount.RequiredMinimum)
                            {
                                result.DiscountMsg = string.Format("Promo requires a minimum of {0} guests", discount.RequiredMinimum);
                            }
                            else
                            {
                                result.DiscountMsg = string.Format("Discount code good for events from {0} to {1}, only.", discount.StartDateTime.ToString("MM/dd/yyyy"), discount.EndDateTime.ToString("MM/dd/yyyy"));
                            }
                                
                            //if (discount.EndDateTime < localDateTime)
                            //{
                            //    result.DiscountMsg = string.Format("Promo Expired on {0}", discount.EndDateTime.ToString("MM/dd/yyyy"));
                            //}
                            //else if (discount.StartDateTime > localDateTime && discount.EndDateTime >= localDateTime)
                            //{
                            //    result.DiscountMsg = string.Format("Promo begins on {0}", discount.StartDateTime.ToString("MM/dd/yyyy"));
                            //}
                            //else if (numberOfGuests < discount.RequiredMinimum)
                            //{
                            //    result.DiscountMsg = string.Format("Promo requires a minimum of {0} guests", discount.RequiredMinimum);
                            //}
                        }
                    }
                }
            }
            return result;
        }

        public static EventDiscountResult CalculateDiscount(EventDiscount discount, decimal rsvpTotal, int numberOfGuests,int eventId,DateTime event_date)
        {
            EventDiscountResult result = new EventDiscountResult();

            if (discount != null && discount.Id > 0)
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                int TimeZoneId = eventDAL.GetTimeZonebyEventId(eventId);
                DateTime localDateTime = Common.Times.ToTimeZoneTime(DateTime.UtcNow, (Common.Times.TimeZone)TimeZoneId);

                if (discount.DateType == DateType.ByEventDate)
                    localDateTime = event_date;

                if (discount.Active == true && discount.StartDateTime <= localDateTime && discount.EndDateTime > localDateTime && numberOfGuests >= discount.RequiredMinimum)
                {
                    if (rsvpTotal > 0)
                    {
                        //Make sure qty meets Minimum
                        if (numberOfGuests >= discount.RequiredMinimum)
                        {
                            int validGuestCount = 0;

                            //If they are booking more guests then the discount allows we use the max as the new guest count
                            if (numberOfGuests > discount.RequiredMaximum)
                            {
                                validGuestCount = discount.RequiredMaximum;
                            }
                            else
                            {
                                validGuestCount = numberOfGuests;
                            }

                            decimal discountValue = 0;
                            //Get Per Person Amount
                            decimal ppAmount = (rsvpTotal / numberOfGuests);

                            if (discount.DiscountType == DiscountOption.FeePerPerson)
                            {
                                //Get Amount vs Percent
                                if (discount.DiscountAmount > 0)
                                {
                                    discountValue = (discount.DiscountAmount * validGuestCount);
                                }
                                else if (discount.DiscountPercent > 0)
                                {
                                    //Get total based on allowed max guest count
                                    decimal ppTotal = (ppAmount * validGuestCount);
                                    //Base percentage of this total
                                    discountValue = (ppTotal * discount.DiscountPercent);
                                }
                            }
                            else
                            {
                                if (discount.DiscountAmount > 0)
                                {
                                    discountValue = discount.DiscountAmount;
                                }
                                else if (discount.DiscountPercent > 0)
                                {
                                    discountValue = ppAmount * validGuestCount * discount.DiscountPercent;
                                }
                            }

                            //Safety Check - If discount > orig sub total then set discount to that amount to prevent a discount from being larger than the total
                            if (discountValue > rsvpTotal)
                            {
                                discountValue = rsvpTotal;
                            }

                            //Discount Result
                            result.DiscountGuestCount = validGuestCount;
                            result.DiscountTotal = discountValue;

                            if (discountValue > 0)
                            {
                                result.DiscountValid = true;
                                result.DiscountApplied = true;
                                if (validGuestCount < numberOfGuests)
                                {
                                    result.DiscountMsg = string.Format("'{0}' is valid (limited to {1} guests).", discount.DiscountCode, validGuestCount);
                                }
                                else
                                {
                                    result.DiscountMsg = string.Format("'{0}' is valid (applied to {1} guests).", discount.DiscountCode, validGuestCount);
                                }
                            }
                            else
                            {
                                result.DiscountMsg = "Promo Not Applicable";
                            }
                        }
                        else
                        {
                            result.DiscountMsg = string.Format("'{0}' requires a minimum of {1} guests.", discount.DiscountCode, discount.RequiredMinimum);
                        }
                    }
                }
                else
                {
                    if (discount.EndDateTime < localDateTime)
                    {
                        result.DiscountMsg = string.Format("Promo Expired on {0}", discount.EndDateTime.ToString("MM/dd/yyyy"));
                    }
                    else if (discount.StartDateTime > localDateTime && discount.EndDateTime >= localDateTime)
                    {
                        result.DiscountMsg = string.Format("Promo begins on {0}", discount.StartDateTime.ToString("MM/dd/yyyy"));
                    }
                    else if (numberOfGuests < discount.RequiredMinimum)
                    {
                        result.DiscountMsg = string.Format("Promo requires a minimum of {0} guests", discount.RequiredMinimum);
                    }
                }
            }
            return result;
        }
    }
}
