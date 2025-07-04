using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPReservationApi.WebApi.ViewModels;
using CPReservationApi.DAL;
using CPReservationApi.WebApi.Services;
using Microsoft.Extensions.Options;
using static CPReservationApi.Common.Payments;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
//using CPReservationApi.Model;
//using static CPReservationApi.Common.Email;
//using CPReservationApi.Common;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/payment")]
    public class PaymentController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        public PaymentController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        [Route("tokenizecardsbycustomer")]
        [HttpGet]
        public async Task<IActionResult> GetTokenizedCardsByCustomer(int member_id, string cust_id, int module_type = 1)
        {
            TokenziedCardListResponse response = new TokenziedCardListResponse();
            if (string.IsNullOrWhiteSpace(cust_id))
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "";
                response.error_info.description = "Invalid customer Id.";
                return new ObjectResult(response);
            }

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            try
            {
                if (module_type == (int)Common.ModuleType.Ticketing)
                {
                    member_id = -1;
                }
                var paymentConfig = eventDAL.GetPaymentConfigByWineryId(member_id);

                if (paymentConfig != null)
                {
                    Configuration config = new Configuration();
                    config.MerchantLogin = paymentConfig.MerchantLogin;
                    config.MerchantPassword = paymentConfig.MerchantPassword;
                    config.UserConfig1 = paymentConfig.UserConfig1;
                    config.UserConfig2 = paymentConfig.UserConfig2;
                    config.GatewayMode = (Configuration.Mode)paymentConfig.GatewayMode;

                    switch (paymentConfig.PaymentGateway)
                    {
                        case Configuration.Gateway.AuthorizeNet:
                            break;
                        case Configuration.Gateway.Braintree:
                            //Services.Braintree objBraintree = new Services.Braintree(_appSetting);
                            //Services.Braintree.GetTokenizedCardsByCustomer("Jonathan", "Elliman", config);
                            response.data = Services.Braintree.GetTokenizedCardsByCustomer(cust_id, config);
                            break;
                        case Configuration.Gateway.CardConnect:
                            break;
                        case Configuration.Gateway.OpenEdge:
                            break;
                        case Configuration.Gateway.Shift4:
                            break;
                        case Configuration.Gateway.Stripe:
                            break;
                        case Configuration.Gateway.USAePay:
                            break;
                        case Configuration.Gateway.WorldPayXML:
                            break;
                        case Configuration.Gateway.Zeamster:
                            Services.Zeamster objZeamster = new Services.Zeamster(_appSetting);
                            response.data = Services.Zeamster.GetTokenizedCardsByCustomer(cust_id, config);
                            break;
                    }

                    if (response.data != null)
                    {
                        response.success = true;
                    }
                    else
                    {
                        response.success = true;
                        response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        response.error_info.extra_info = "no record found";
                    }
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetTokenizedCardsByCustomer:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);

            }


            return new ObjectResult(response);
        }

        [Route("tickettokenizecardsbycustomername")]
        [HttpGet]
        public async Task<IActionResult> GetTicketTokenizedCardsByCustomerName(int member_id, string first_name, string last_name)
        {
            TokenziedCardListResponse response = new TokenziedCardListResponse();
            if (string.IsNullOrWhiteSpace(first_name))
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "";
                response.error_info.description = "Invalid First Name.";
                return new ObjectResult(response);
            }

            if (string.IsNullOrWhiteSpace(last_name))
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "";
                response.error_info.description = "Invalid Last Name.";
                return new ObjectResult(response);
            }

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            try
            {
                var paymentProcessor = eventDAL.GetTicketPaymentProcessorByWinery(member_id);
                if (paymentProcessor == Common.Common.TicketsPaymentProcessor.CellarPassProcessor)
                {
                    var paymentConfig = eventDAL.GetPaymentConfigByWineryId(-1);

                    if (paymentConfig != null)
                    {
                        Configuration config = new Configuration();
                        config.MerchantLogin = paymentConfig.MerchantLogin;
                        config.MerchantPassword = paymentConfig.MerchantPassword;
                        config.UserConfig1 = paymentConfig.UserConfig1;
                        config.UserConfig2 = paymentConfig.UserConfig2;
                        config.GatewayMode = (Configuration.Mode)paymentConfig.GatewayMode;

                        Services.Braintree objBraintree = new Services.Braintree(_appSetting);
                        response.data = Services.Braintree.GetTokenizedCardsByCustomer(first_name, last_name, config);

                        if (response.data != null)
                        {
                            response.success = true;
                        }
                        else
                        {
                            response.success = true;
                            response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                            response.error_info.extra_info = "no record found";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetTicketTokenizedCardsByCustomerName:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }

            return new ObjectResult(response);
        }

        [Route("commerce7tokenizecardsbycustomer")]
        [HttpGet]
        public async Task<IActionResult> GetCommerce7TokenizedCardsByCustomer(int member_id, string cust_id, string email)
        {
            TokenziedCardListResponse response = new TokenziedCardListResponse();

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            try
            {
                Model.WineryModel memberModel = eventDAL.GetWineryById(member_id);
                List<Model.CustomerCreditCard> customerCreditCards = new List<Model.CustomerCreditCard>();

                if (memberModel.EnableClubCommerce7 || memberModel.EnableCommerce7)
                {
                    if (!string.IsNullOrEmpty(email) && string.IsNullOrEmpty(cust_id))
                    {
                        var membership_number = await Task.Run(() => Utility.GetCommerce7CustomerIdByNameOrEmail(email, memberModel.Commerce7Username, memberModel.Commerce7Password, memberModel.Commerce7Tenant, member_id));

                        if (!string.IsNullOrEmpty(membership_number))
                            cust_id = membership_number;

                        if (string.IsNullOrWhiteSpace(cust_id))
                        {
                            response.success = false;
                            response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                            response.error_info.extra_info = "";
                            response.error_info.description = "Invalid customer Id.";
                            return new ObjectResult(response);
                        }
                    }

                    if (!string.IsNullOrEmpty(cust_id))
                        customerCreditCards = await Utility.GetCommerce7CreditCardByCustId(memberModel.Commerce7Username, memberModel.Commerce7Password, memberModel.Commerce7Tenant, cust_id, member_id);

                }

                if (customerCreditCards != null && customerCreditCards.Count > 0)
                {
                    Model.PaymentConfigModel paymentConfig = new Model.PaymentConfigModel();
                    paymentConfig = eventDAL.GetPaymentConfigByWineryId(member_id);
                    customerCreditCards = customerCreditCards.OrderBy(c => c.maskedCardNumber).ThenByDescending(c => c.expiryYr).ThenByDescending(c => c.expiryMo).ThenByDescending(c => c.createdAt).ToList();
                    string gatewayName = Utility.GetCommerce7PaymentGatewayName(paymentConfig.PaymentGateway);
                    if (paymentConfig.PaymentGateway == Configuration.Gateway.Commrece7Payments)
                    {
                        customerCreditCards = customerCreditCards.Where(c => string.IsNullOrWhiteSpace(c.gateway) || c.gateway == gatewayName).ToList();
                    }
                    else
                    {
                        customerCreditCards = customerCreditCards.Where(c => c.gateway == gatewayName).ToList();
                    }

                    List<TokenizedCard> resp = new List<TokenizedCard>();
                    string prevCardNum = "";
                    string prevExpiryDate = "";
                    foreach (var item in customerCreditCards)
                    {
                        string card_expiration = string.Empty;
                        if (item.expiryMo.ToString().Length == 1)
                            card_expiration = "0" + item.expiryMo.ToString() + "/" + item.expiryYr.ToString();
                        else
                            card_expiration = item.expiryMo.ToString() + "/" + item.expiryYr.ToString();
                        string cardNum = item.maskedCardNumber;

                        if (!prevCardNum.Equals(cardNum) || !prevExpiryDate.Equals(card_expiration))
                        {
                            prevCardNum = cardNum;
                            prevExpiryDate = card_expiration;

                            TokenizedCard cc = new TokenizedCard();

                            cc.card_token = item.tokenOnFile;
                            cc.card_type = item.cardBrand;
                            cc.customer_name = item.cardHolderName;
                            cc.cust_id = item.customerId;

                            bool is_expired = true;

                            if (item.expiryYr > DateTime.Now.Year)
                            {
                                is_expired = false;
                            }
                            else if (item.expiryYr == DateTime.Now.Year && item.expiryMo >= DateTime.Now.Month)
                            {
                                is_expired = false;
                            }

                            cc.is_expired = is_expired;

                            cc.last_four_digits = Common.Common.Right(item.maskedCardNumber, 4);
                            cc.card_expiration = card_expiration;
                            cc.card_exp_month = item.expiryMo.ToString();
                            cc.card_exp_year = item.expiryYr.ToString();

                            if (is_expired == false)
                            {
                                string expiryMo = item.expiryMo.ToString();

                                if (item.expiryMo.ToString().Length == 1)
                                    expiryMo = "0" + item.expiryMo.ToString();

                                bool IsValid = true;

                                //if (gatewayName == "No Gateway")
                                IsValid = eventDAL.IsValidPayCardToken((int)paymentConfig.PaymentGateway, cc.card_token, member_id, item.cardHolderName, expiryMo, item.expiryYr.ToString());

                                //bool IsValid = eventDAL.IsValidPayCardToken((int)paymentConfig.PaymentGateway, cc.card_token);

                                //if (IsValid == false)
                                //    IsValid = eventDAL.IsValidPayCardToken2((int)paymentConfig.PaymentGateway, cc.card_token, member_id);

                                //if (IsValid == false)
                                //{
                                //    string expiryMo = item.expiryMo.ToString();

                                //    if (item.expiryMo.ToString().Length == 1)
                                //        expiryMo = "0" + item.expiryMo.ToString();

                                //    IsValid = eventDAL.IsValidPayCardToken3(item.cardHolderName, expiryMo, item.expiryYr.ToString(),member_id);

                                //}

                                if (IsValid)
                                    resp.Add(cc);
                            }

                            //if (eventDAL.IsValidPayCardToken((int)paymentConfig.PaymentGateway, cc.card_token) && is_expired == false)
                            //    resp.Add(cc);
                        }
                        //if (paymentConfig.PaymentGateway == Configuration.Gateway.CardConnect)
                        //{
                        //    if (item.gateway.ToLower() == "cardconnect" && item.tokenOnFile.Length == 16)
                        //        resp.Add(cc);
                        //}
                        //else
                        //    resp.Add(cc);
                    }

                    response.data = resp;
                    response.success = true;
                }
                else
                {
                    response.success = true;
                    response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    response.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetCommerce7TokenizedCardsByCustomer:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);

            }

            return new ObjectResult(response);
        }

        [Route("tokenizecard")]
        [HttpPost]
        public async Task<IActionResult> TokenizeCard([FromBody]TokenizedCardRequest request)
        {
            TokenziedCardResponse response = new TokenziedCardResponse();
            TokenizedCard card = null;
            TokenizedCard secondarycard = null;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            //CPReservationApi.WebApi.Services.Payments.Braintree

            if (request.member_id <= 0 || string.IsNullOrWhiteSpace(request.number) || (request.number + "").Length < 12)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                response.error_info.extra_info = "";
                response.error_info.description = "Invalid credit card information";
                return new ObjectResult(response);
            }
            if (string.IsNullOrWhiteSpace(request.exp_year) || request.exp_year.Length > 4)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                response.error_info.extra_info = "";
                response.error_info.description = "Invalid credit card Year";
                return new ObjectResult(response);
            }
            if (string.IsNullOrWhiteSpace(request.exp_month) || request.exp_month.Length > 2)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                response.error_info.extra_info = "";
                response.error_info.description = "Invalid credit card month";
                return new ObjectResult(response);
            }

            if (string.IsNullOrEmpty(request.cvv2))
            {
                logDAL.InsertLog("WebApi", "TokenizeCard:  " + JsonConvert.SerializeObject(request), HttpContext.Request.Headers["AuthenticateKey"], 3, request.member_id);
            }

            //get payment gateway of the member
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);


            if (request.user_info == null)
            {
                request.user_info = new UserDetailViewModel();

                request.user_info.address = new UserAddress();
            }
            var winery = new Model.WineryModel();
            winery = eventDAL.GetWineryById(request.member_id);

            request.number = request.number.Replace(" ", "").Replace("-", "");

            string cardtype = Payments.GetCardType(request.number);

            string EnabledCreditCards = ',' + winery.EnabledCreditCards + ',';

            if (!(((EnabledCreditCards.IndexOf(",2,") > -1) && cardtype == "Visa") || ((EnabledCreditCards.IndexOf(",4,") > -1) && cardtype == "American Express") || ((EnabledCreditCards.IndexOf(",1,") > -1) && cardtype == "Master Card") || ((EnabledCreditCards.IndexOf(",8,") > -1) && cardtype == "Diners Club") || ((EnabledCreditCards.IndexOf(",32,") > -1 || EnabledCreditCards.IndexOf(",20,") > -1) && cardtype == "Discover")))
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                response.error_info.extra_info = "";
                response.error_info.description = "Sorry, " + winery.DisplayName + " does not accept " + cardtype + ".";
                return new ObjectResult(response);
            }

            try
            {
                int member_id = request.member_id;
                bool isStripe = false;
                Model.PaymentConfigModel paymentConfig = new Model.PaymentConfigModel();
                Model.PaymentConfigModel secondaryPaymentConfig = new Model.PaymentConfigModel();

                if (request.source_module == Common.ModuleType.Ticketing)
                {
                    var tktPayProcessor = eventDAL.GetTicketPaymentProcessorByWinery(member_id);
                    if (tktPayProcessor == Common.Common.TicketsPaymentProcessor.CellarPassProcessor)
                        member_id = -1;
                    else if (tktPayProcessor == Common.Common.TicketsPaymentProcessor.Stripe)
                        isStripe = true;
                }

                if (isStripe)
                {
                    Services.Stripe objStripe = new Services.Stripe(_appSetting);
                    card = objStripe.GetTokenId(request, _appSetting.Value.StripeSecretKey);
                }
                else
                {
                    int setting_type = 1;
                    paymentConfig = eventDAL.GetPaymentConfigByWineryId(member_id, setting_type);

                    if (paymentConfig != null && paymentConfig.PaymentGateway != Configuration.Gateway.Offline)
                    {
                        Payments objPayments = new Payments(_appSetting);
                        card = Payments.TokenziedCard(request, paymentConfig);
                    }
                    else
                    {
                        string description = string.Empty;

                        response.success = false;
                        response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                        response.error_info.extra_info = "";

                        if (request.source_module == Common.ModuleType.Ticketing)
                        {
                            if (request.ticket_event_id > 0)
                            {
                                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                                string eventPassword = string.Empty;
                                Model.TicketEventDetailModel ticketEvent = ticketDAL.GetTicketEventDetailsById(request.ticket_event_id, ref eventPassword);

                                if (ticketEvent != null)
                                {
                                    description = "We're sorry, but we have encountered an error. You will need to contact " + ticketEvent.event_organizer_name + " at " + Utility.FormatTelephoneNumber(ticketEvent.event_organizer_phone, "US") + " to complete your request.";
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(description))
                        {
                            description = "We're sorry, but we have encountered an error. You will need to contact " + winery.DisplayName + " at " + Utility.FormatTelephoneNumber(winery.BusinessPhone.ToString(), winery.WineryAddress.country) + " to complete your request.";
                        }

                        response.error_info.description = description;
                        return new ObjectResult(response);
                    }

                    // secondary payment gateway 

                    setting_type = 2;
                    secondaryPaymentConfig = eventDAL.GetPaymentConfigByWineryId(member_id, setting_type);

                    if (secondaryPaymentConfig != null && secondaryPaymentConfig.PaymentGateway != Configuration.Gateway.Offline && request.source_module == Common.ModuleType.Reservation)
                    {
                        Payments objPayments = new Payments(_appSetting);
                        secondarycard = Payments.TokenziedCard(request, secondaryPaymentConfig);
                    }
                }

                if (card == null || string.IsNullOrWhiteSpace(card.card_token))
                {
                    string description = string.Empty;
                    string extra_info = string.Empty;

                    if (card != null && !string.IsNullOrWhiteSpace(card.ErrorMessage))
                        description = card.ErrorMessage;

                    if (card != null && !string.IsNullOrWhiteSpace(card.ErrorMessage))
                        extra_info = card.ErrorCode;

                    if (request.source_module == Common.ModuleType.Ticketing)
                    {
                        bool IsAdmin = Convert.ToBoolean(Request.Headers["IsAdmin"]);

                        if (request.ticket_event_id > 0)
                        {
                            TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                            string eventPassword = string.Empty;
                            Model.TicketEventDetailModel ticketEvent = ticketDAL.GetTicketEventDetailsById(request.ticket_event_id, ref eventPassword);

                            if (ticketEvent != null && string.IsNullOrWhiteSpace(description))
                            {
                                description = "We're sorry, but we have encountered an error. You will need to contact " + ticketEvent.event_organizer_name + " at " + Utility.FormatTelephoneNumber(ticketEvent.event_organizer_phone, "US") + " to complete your request.";
                            }

                            if (ticketEvent != null && IsAdmin == false)
                            {
                                if (request.attempts > 2)
                                {
                                    description = "Your credit card was declined. For further assistance, please contact " + ticketEvent.event_organizer_name + " at " + Utility.FormatTelephoneNumber(ticketEvent.event_organizer_phone, "US");
                                }
                                else
                                    description = "Your credit card was declined";
                            }
                        }
                        else
                        {
                            if (IsAdmin == false)
                                description = "Your credit card was declined";
                        }
                    }

                    if (string.IsNullOrEmpty(description))
                    {
                        description = "We're sorry, but we have encountered an error. You will need to contact " + winery.DisplayName + " at " + Utility.FormatTelephoneNumber(winery.BusinessPhone.ToString(), winery.WineryAddress.country) + " to complete your request.";
                    }

                    response.error_info.description = description;
                    response.error_info.extra_info = extra_info;

                    response.success = false;
                    response.error_info = new ErrorInfo
                    {
                        error_type = (int)Common.Common.ErrorType.CreditCard,
                        description = description,
                        extra_info = extra_info
                    };
                }
                else
                {
                    response.success = true;
                    response.data = card;

                    if (request.user_info == null)
                    {
                        request.user_info = new UserDetailViewModel();

                        request.user_info.email = "";
                        request.user_info.first_name = "";
                        request.user_info.last_name = "";
                        request.user_id = 0;
                    }

                    if (isStripe)
                        paymentConfig.PaymentGateway = Configuration.Gateway.Stripe;

                    if (card != null && !string.IsNullOrEmpty(card.card_token) && request.user_info != null)
                        eventDAL.InsertCreditCardDetail(Payments.GetCardType(request.number), request.cust_name, request.exp_month, request.exp_year, card.card_token, member_id, Common.Common.Right(request.number, 4), Common.Common.Left(request.number, 4), (int)request.source_module, request.user_info.email, request.user_info.first_name, request.user_info.last_name, request.user_id, (int)paymentConfig.PaymentGateway);

                    if (request.source_module == Common.ModuleType.Reservation)
                    {
                        if ((winery.EnableVin65 || winery.EnableClubVin65) && !string.IsNullOrEmpty(card.card_token) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(winery.SALT) && !string.IsNullOrEmpty(winery.DecryptKey))
                        {
                            string cardtype2 = Payments.GetCardType(request.number, "vin65");
                            string cardnumber = Common.StringHelpers.EncryptedCardNumber(request.number, winery.SALT, winery.DecryptKey);
                            string cardToken = card.card_token;
                            eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, cardToken, member_id, winery.Vin65UserName, winery.Vin65Password,request.cvv2);

                            if (secondarycard != null && !string.IsNullOrWhiteSpace(secondarycard.card_token))
                            {
                                cardToken = secondarycard.card_token;
                                eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, cardToken, member_id, winery.Vin65UserName, winery.Vin65Password, request.cvv2);

                                if (request.user_info == null)
                                {
                                    request.user_info = new UserDetailViewModel();

                                    request.user_info.email = "";
                                    request.user_info.first_name = "";
                                    request.user_info.last_name = "";
                                    request.user_id = 0;
                                }

                                if (card != null && !string.IsNullOrEmpty(card.card_token) && request.user_info != null)
                                    eventDAL.InsertCreditCardDetail(Payments.GetCardType(request.number), request.cust_name, request.exp_month, request.exp_year, cardToken, member_id, Common.Common.Right(request.number, 4), Common.Common.Left(request.number, 4), (int)request.source_module, request.user_info.email, request.user_info.first_name, request.user_info.last_name, request.user_id, (int)secondaryPaymentConfig.PaymentGateway);
                            }

                        }
                        else if ((winery.EnableCommerce7 || winery.EnableClubCommerce7) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(card.card_token))
                        {
                            string cardtype2 = Payments.GetCardType(request.number, "commerce7");
                            string cardnumber = Common.Common.Right(request.number, 4).PadLeft(request.number.Length, '*');

                            string gateway = "No Gateway";
                            string cardToken = card.card_token;
                            var paymentGateway = paymentConfig.PaymentGateway;
                            gateway = Utility.GetCommerce7PaymentGatewayName(paymentGateway);

                            eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, cardToken, member_id, gateway, "",request.cvv2);

                            if (secondarycard != null && !string.IsNullOrWhiteSpace(secondarycard.card_token))
                            {
                                cardToken = secondarycard.card_token;
                                paymentGateway = secondaryPaymentConfig.PaymentGateway;
                                gateway = Utility.GetCommerce7PaymentGatewayName(paymentGateway);
                                eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, cardToken, member_id, gateway, "", request.cvv2);

                                if (request.user_info == null)
                                {
                                    request.user_info = new UserDetailViewModel();

                                    request.user_info.email = "";
                                    request.user_info.first_name = "";
                                    request.user_info.last_name = "";
                                    request.user_id = 0;
                                }

                                if (card != null && !string.IsNullOrEmpty(card.card_token) && request.user_info != null)
                                    eventDAL.InsertCreditCardDetail(Payments.GetCardType(request.number), request.cust_name, request.exp_month, request.exp_year, cardToken, member_id, Common.Common.Right(request.number, 4), Common.Common.Left(request.number, 4), (int)request.source_module, request.user_info.email, request.user_info.first_name, request.user_info.last_name, request.user_id, (int)secondaryPaymentConfig.PaymentGateway);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                
                logDAL.InsertLog("WebApi", "TokenizeCard:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, request.member_id);
            }

            return new ObjectResult(response);
        }

        [Route("tokenizecardandauthorization")]
        [HttpPost]
        public async Task<IActionResult> TokenizeCardAndAuthorization([FromBody]TokenizedCardAuthorizationRequest request)
        {
            TokenizeCardAndAuthorizationResponse response = new TokenizeCardAndAuthorizationResponse();
            TokenizedCard card = null;

            if (string.IsNullOrWhiteSpace(request.card_token))
            {
                if (string.IsNullOrWhiteSpace(request.number) || (request.number + "").Length < 12)
                {
                    response.success = false;
                    response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                    response.error_info.extra_info = "";
                    response.error_info.description = "Invalid credit card information";
                    return new ObjectResult(response);
                }
                if (string.IsNullOrWhiteSpace(request.exp_year) || request.exp_year.Length > 4)
                {
                    response.success = false;
                    response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                    response.error_info.extra_info = "";
                    response.error_info.description = "Invalid credit card Year";
                    return new ObjectResult(response);
                }
                if (string.IsNullOrWhiteSpace(request.exp_month) || request.exp_month.Length > 2)
                {
                    response.success = false;
                    response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                    response.error_info.extra_info = "";
                    response.error_info.description = "Invalid credit card month";
                    return new ObjectResult(response);
                }
            }

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            var reservationDetailModel = new Model.ReservationDetailModel();
            reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(request.reservation_id);

            var winery = new Model.WineryModel();
            winery = eventDAL.GetWineryById(reservationDetailModel.member_id);

            string card_token = string.Empty;

            //get payment gateway of the member
            if (string.IsNullOrWhiteSpace(request.card_token))
            {
                request.number = request.number.Replace(" ", "").Replace("-", "");

                string cardtype = Payments.GetCardType(request.number);

                string EnabledCreditCards = ',' + winery.EnabledCreditCards + ',';

                if (!(((EnabledCreditCards.IndexOf(",2,") > -1) && cardtype == "Visa") || ((EnabledCreditCards.IndexOf(",4,") > -1) && cardtype == "American Express") || ((EnabledCreditCards.IndexOf(",1,") > -1) && cardtype == "Master Card") || ((EnabledCreditCards.IndexOf(",8,") > -1) && cardtype == "Diners Club") || ((EnabledCreditCards.IndexOf(",32,") > -1 || EnabledCreditCards.IndexOf(",20,") > -1) && cardtype == "Discover")))
                {
                    response.success = false;
                    response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                    response.error_info.extra_info = "";
                    response.error_info.description = "Sorry, " + winery.DisplayName + " does not accept " + cardtype + ".";
                    return new ObjectResult(response);
                }

                try
                {
                    int member_id = reservationDetailModel.member_id;
                    Model.PaymentConfigModel paymentConfig = new Model.PaymentConfigModel();

                    paymentConfig = eventDAL.GetPaymentConfigByWineryId(member_id);

                    if (paymentConfig != null && paymentConfig.PaymentGateway != Configuration.Gateway.Offline)
                    {
                        Payments objPayments = new Payments(_appSetting);

                        TokenizedCardRequest tokenizedCardRequest = new TokenizedCardRequest();

                        tokenizedCardRequest.card_type = request.card_type;
                        tokenizedCardRequest.cust_name = request.cust_name;
                        tokenizedCardRequest.exp_month = request.exp_month;
                        tokenizedCardRequest.exp_year = request.exp_year;
                        tokenizedCardRequest.member_id = member_id;
                        tokenizedCardRequest.number = request.number;
                        tokenizedCardRequest.card_last_four_digits = Common.Common.Right(request.number, 4);
                        tokenizedCardRequest.card_first_four_digits = Common.Common.Left(request.number, 4);

                        tokenizedCardRequest.user_info = new UserDetailViewModel();
                        tokenizedCardRequest.user_info.address = new ViewModels.UserAddress
                        {
                            zip_code = reservationDetailModel.user_detail.address.zip_code,
                            address_1 = reservationDetailModel.user_detail.address.address_1
                        };

                        tokenizedCardRequest.ignore_avs_error = true;

                        card = Payments.TokenziedCard(tokenizedCardRequest, paymentConfig);
                    }

                    if (card == null || string.IsNullOrWhiteSpace(card.card_token))
                    {
                        string description = string.Empty;
                        string extra_info = string.Empty;

                        if (card != null && !string.IsNullOrWhiteSpace(card.ErrorMessage))
                            description = card.ErrorMessage;

                        if (card != null && !string.IsNullOrWhiteSpace(card.ErrorMessage))
                            extra_info = card.ErrorCode;

                        if (string.IsNullOrEmpty(description))
                        {
                            description = "We're sorry, but we have encountered an error. You will need to contact " + winery.DisplayName + " at " + Utility.FormatTelephoneNumber(winery.BusinessPhone.ToString(), winery.WineryAddress.country) + " to complete your request.";
                        }

                        response.error_info.description = description;
                        response.error_info.extra_info = extra_info;

                        response.success = false;
                        response.error_info = new ErrorInfo
                        {
                            error_type = (int)Common.Common.ErrorType.CreditCard,
                            description = description,
                            extra_info = extra_info
                        };
                    }
                    else
                    {
                        card_token = card.card_token;
                        eventDAL.UpdateReservationPaycardDetails(reservationDetailModel.reservation_id, card.card_token, card.card_type, CPReservationApi.Common.StringHelpers.Encryption(Common.Common.Right(request.number, 4)), request.cust_name, request.exp_month, request.exp_year);

                        if (card != null && !string.IsNullOrEmpty(card.card_token) && reservationDetailModel.user_detail != null)
                            eventDAL.InsertCreditCardDetail(Payments.GetCardType(request.number), request.cust_name, request.exp_month, request.exp_year, card_token, reservationDetailModel.member_id, Common.Common.Right(request.number, 4), Common.Common.Left(request.number, 4), (int)Common.ModuleType.Reservation, reservationDetailModel.user_detail.email, reservationDetailModel.user_detail.first_name, reservationDetailModel.user_detail.last_name, reservationDetailModel.user_detail.user_id, (int)paymentConfig.PaymentGateway);

                        if ((winery.EnableVin65 || winery.EnableClubVin65) && !string.IsNullOrEmpty(card.card_token) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(winery.SALT) && !string.IsNullOrEmpty(winery.DecryptKey))
                        {
                            string cardtype2 = Payments.GetCardType(request.number, "vin65");
                            string cardnumber = Common.StringHelpers.EncryptedCardNumber(request.number, winery.SALT, winery.DecryptKey);
                            eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, card.card_token, reservationDetailModel.member_id, winery.Vin65UserName, winery.Vin65Password, request.cvv2);
                        }
                        else if ((winery.EnableCommerce7 || winery.EnableClubCommerce7) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(card.card_token))
                        {
                            string cardtype2 = Payments.GetCardType(request.number, "commerce7");
                            string cardnumber = Common.Common.Right(request.number, 4).PadLeft(request.number.Length, '*');

                            string gateway = "No Gateway";
                            gateway = Utility.GetCommerce7PaymentGatewayName(paymentConfig.PaymentGateway);

                            eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, card.card_token, reservationDetailModel.member_id, gateway, "", request.cvv2);
                        }

                        QueueService getStarted = new QueueService();

                        var queueModel = new Common.EmailQueue();
                        queueModel.EType = (int)Common.Email.EmailType.CreateThirdPartyContact;
                        queueModel.BCode = request.reservation_id.ToString();
                        queueModel.UId = reservationDetailModel.user_detail.user_id;
                        queueModel.RsvpId = reservationDetailModel.member_id;
                        queueModel.PerMsg = "";
                        queueModel.Src = reservationDetailModel.referral_type;
                        var qData = Newtonsoft.Json.JsonConvert.SerializeObject(queueModel);

                        AppSettings _appsettings = _appSetting.Value;
                        getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                    }
                }
                catch (Exception ex)
                {
                    response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                    response.error_info.extra_info = Common.Common.InternalServerError;
                    response.error_info.description = ex.Message.ToString();
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi", "TokenizeCardAndAuthorization:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, reservationDetailModel.member_id);
                }
            }
            else
            {
                //request.card_type = reservationDetailModel.pay_card.card_type;
                //request.cust_name = reservationDetailModel.pay_card.cust_name;
                //request.exp_month = reservationDetailModel.pay_card.exp_month;
                //request.exp_year = reservationDetailModel.pay_card.exp_year;
                card_token = request.card_token;
                //request.number = CPReservationApi.Common.StringHelpers.Decryption(reservationDetailModel.pay_card.number);
                //request.card_entry = reservationDetailModel.pay_card.card_entry;
            }

            if (!string.IsNullOrWhiteSpace(card_token))
            {
                try
                {
                    double amountToCharge = Convert.ToDouble(reservationDetailModel.fee_due) + (Convert.ToDouble(reservationDetailModel.fee_due) * .25);

                    CreateReservationRequest rsvp = new CreateReservationRequest();

                    ViewModels.UserDetailViewModel user_detail = new ViewModels.UserDetailViewModel();
                    ViewModels.UserAddress address = new ViewModels.UserAddress();

                    user_detail.first_name = reservationDetailModel.user_detail.first_name;
                    user_detail.last_name = reservationDetailModel.user_detail.last_name;
                    user_detail.phone_number = reservationDetailModel.user_detail.phone_number;
                    user_detail.email = reservationDetailModel.user_detail.email;
                    address.zip_code = reservationDetailModel.user_detail.address.zip_code;
                    address.city = reservationDetailModel.user_detail.address.city;
                    address.state = reservationDetailModel.user_detail.address.state;
                    address.country = reservationDetailModel.user_detail.address.country;
                    address.address_1 = reservationDetailModel.user_detail.address.address_1;
                    address.address_2 = reservationDetailModel.user_detail.address.address_2;

                    user_detail.address = address;
                    rsvp.user_detail = user_detail;

                    Model.PayCard pay_card = new Model.PayCard();

                    pay_card.card_type = request.card_type;
                    pay_card.cust_name = request.cust_name;
                    pay_card.exp_month = request.exp_month;
                    pay_card.exp_year = request.exp_year;
                    pay_card.card_token = card_token;
                    pay_card.number = request.number;
                    pay_card.card_last_four_digits = Common.Common.Right(request.number, 4);
                    pay_card.card_first_four_digits = Common.Common.Left(request.number, 4);
                    pay_card.card_entry = request.card_entry;
                    pay_card.application_type = request.application_type;
                    pay_card.application_version = request.application_version;
                    pay_card.terminal_id = request.terminal_id;
                    pay_card.card_reader = request.card_reader;

                    rsvp.pay_card = pay_card;

                    rsvp.member_id = reservationDetailModel.member_id;

                    rsvp.reservation_id = reservationDetailModel.reservation_id;

                    int booked_by_id = reservationDetailModel.booked_by_id ?? 0;

                    Services.Payments objPayments = new Services.Payments(_appSetting);

                    var paymentresult = await objPayments.ChargeReservation(rsvp, Convert.ToDecimal(amountToCharge), reservationDetailModel.booking_code, booked_by_id, Transaction.ChargeType.AuthOnly);

                    if (paymentresult.Status == TransactionResult.StatusType.Failed)
                    {
                        response.success = false;
                        response.error_info.error_type = (int)Common.Common.ErrorType.ReservationPaymentError;
                        response.error_info.extra_info = "Payment Authorization Error " + paymentresult.Detail;
                        response.error_info.description = "Problem authorizing your payment. Please review the credit card and try again.";

                        if (string.IsNullOrEmpty(paymentresult.Detail))
                        {
                            paymentresult.Detail = "Problem authorizing your payment.";
                        }

                        eventDAL.SaveReservationPaymentV2(reservationDetailModel.member_id, request.reservation_id, paymentresult);
                        return new ObjectResult(response);
                    }
                    else
                    {
                        response.success = true;
                        TokenizeCardAndAuthorization resp = new TokenizeCardAndAuthorization();
                        resp.approval_code = paymentresult.ApprovalCode;
                        resp.authorized_amount = Convert.ToDecimal(amountToCharge);

                        response.data = resp;

                        if (reservationDetailModel.status == (int)Common.Email.ReservationStatus.Initiated)
                        {
                            reservationDetailModel.status = (int)Common.Email.ReservationStatus.Pending;
                            reservationDetailModel.reservation_id = request.reservation_id;
                            Services.Payments.UpdateReservation(reservationDetailModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                    response.error_info.extra_info = Common.Common.InternalServerError;
                    response.error_info.description = ex.Message.ToString();
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi", "TokenizeCardAndAuthorization:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, reservationDetailModel.member_id);
                }
            }

            return new ObjectResult(response);
        }

        [Route("tokenizecardsbycustomerandmember")]
        [HttpGet]
        public async Task<IActionResult> GetTokenizedCardsByCustomerAndMember(int member_id, Common.ModuleType source_module,int user_id=0, string email="")
        {
            CreditCardListResponse response = new CreditCardListResponse();

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            try
            {
                Configuration.Gateway gateway = Configuration.Gateway.Offline;

                if (source_module == Common.ModuleType.Ticketing)
                {
                    var tktPayProcessor = eventDAL.GetTicketPaymentProcessorByWinery(member_id);
                    if (tktPayProcessor == Common.Common.TicketsPaymentProcessor.CellarPassProcessor)
                    {
                        gateway = Configuration.Gateway.Braintree;
                        member_id = -1;
                    }
                    else if (tktPayProcessor == Common.Common.TicketsPaymentProcessor.Stripe)
                        gateway = Configuration.Gateway.Stripe;
                }
                else
                {
                    int setting_type = 1;
                    Model.PaymentConfigModel paymentConfig = eventDAL.GetPaymentConfigByWineryId(member_id, setting_type);

                    if (paymentConfig != null)
                        gateway = paymentConfig.PaymentGateway;
                }

                List<Model.CreditCardDetail> list = eventDAL.GetCreditCardDetail(member_id, source_module, user_id, email, gateway);

                if (list.Count > 0)
                {
                    response.data = list;
                    response.success = true;
                }
                else
                {
                    response.success = true;
                    response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    response.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetTokenizedCardsByCustomerAndMember:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);

            }

            return new ObjectResult(response);
        }
    }
}