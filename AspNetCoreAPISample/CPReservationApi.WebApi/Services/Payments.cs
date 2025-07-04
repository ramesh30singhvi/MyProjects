using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
//using nsoftware.InPay;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CPReservationApi.Common.Payments;
using pbt = Braintree;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using eWineryWebServices;
using Microsoft.AspNetCore.Http;

namespace CPReservationApi.WebApi.Services
{
    public class Payments
    {
        static private AppSettings _appSettings;
        static private IOptions<AppSettings> _appSetting;
        public Payments(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
            _appSetting = settings;
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
        //public class CreditCard
        //{
        //    public string Number { get; set; }
        //    public string CustName { get; set; }
        //    public string ExpMonth { get; set; }
        //    public string ExpYear { get; set; }
        //    public string CVV { get; set; }
        //}
        public class TransactionResult
        {
            public enum StatusType
            {
                Failed = 0,
                Success = 1
            }
            public StatusType Status { get; set; }
            public bool DoFollowUpSale { get; set; }
            public Transaction1.ChargeType TransactionType { get; set; }
            public Configuration.Gateway PaymentGateway { get; set; }
            public PaymentType PayType { get; set; }
            public string ResponseCode { get; set; }
            public string Detail { get; set; }
            public string ApprovalCode { get; set; }
            public string TransactionID { get; set; }
            public string AvsResponse { get; set; }
            public string CheckOrRefNumber { get; set; }
            public CreditCard Card { get; set; }
            public decimal Amount { get; set; }
            public int ProcessedBy { get; set; }
            public int PaymentID { get; set; }
        }

        //public class Configuration
        //{
        //    public enum Gateway
        //    {
        //        Offline = 0,
        //        AuthorizeNet = 1,
        //        PayFlowPro = 2,
        //        CenPos = 3,
        //        USAePay = 4,
        //        Braintree = 5,
        //        WorldPayXML = 6,
        //        OpenEdge = 7,
        //        Stripe = 8,
        //        Cybersource = 9
        //    }

        //    public enum Mode
        //    {
        //        live = 0,
        //        test = 1
        //    }

        //    public Gateway PaymentGateway { get; set; }
        //    public string MerchantLogin { get; set; }
        //    public string MerchantPassword { get; set; }
        //    public string UserConfig1 { get; set; }
        //    public string UserConfig2 { get; set; }
        //    public Mode GatewayMode { get; set; }
        //    public string CardTypes { get; set; }
        //}

        public class Transaction1
        {
            public Transaction1()
            {
                User = new UserDetailModel();
                Card = new CreditCard();
            }
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
            public ChargeType Type { get; set; }
            public TransactionType Transaction { get; set; }
            public Configuration.Gateway Gateway { get; set; }
            public CreditCard Card { get; set; }
            public decimal Amount { get; set; }
            public decimal OrigAmount { get; set; }
            public string CheckOrRefNumber { get; set; }
            public string TransactionID { get; set; }
            public UserDetailModel User { get; set; }
            public int WineryId { get; set; }
            public int ProcessedBy { get; set; }
            public bool IsPreAuthCredit { get; set; } = false;
        }

        public class Transaction
        {
            public UserDetailModel User { get; set; }
            public enum ChargeType
            {
                None = 0,
                AuthOnly = 1,
                Sale = 2,
                Credit = 3,
                Void = 4,
                Capture = 5
            }

            public enum TransactionType
            {
                Rsvp = 0,
                Billing = 1,
                TicketSale = 2
            }
            public ChargeType Type { get; set; }
            public Configuration.Gateway Gateway { get; set; }
            public CreditCard Card { get; set; }
            public decimal Amount { get; set; }
            public decimal OrigAmount { get; set; }
            public string CheckOrRefNumber { get; set; }
            public string TransactionID { get; set; }

            public int WineryId { get; set; }

            public int ProcessedBy { get; set; }
            public TransactionType TransactionsType { get; set; }

            public static Hashtable GetChargeTypes()
            {
                string ActiveTypes = "2,3,4";
                Type enumeration = typeof(ChargeType);
                string[] names = Enum.GetNames(enumeration);
                Array values = Enum.GetValues(enumeration);
                Hashtable ht = new Hashtable();
                for (int i = 0; i <= names.Length - 1; i++)
                {
                    if (ActiveTypes.IndexOf(Convert.ToInt32(values.GetValue(i)).ToString()) > 0)
                    {
                        ht.Add(Convert.ToInt32(values.GetValue(i)).ToString(), names[i]);
                    }
                }
                return ht;
            }

        }

        public async Task<Common.Payments.TransactionResult> RefundReservation(ReservationDetailModel rsvp, decimal amountToRefund, int BookedById)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            //PAYMENT/REFUND  
            Common.Payments.TransactionResult result = new Common.Payments.TransactionResult();
            decimal RefundAmt = 0;
            //Get Latest Payment to refund against
            List<PaymentModel> lstpayments = eventDAL.GetReservationPayments(rsvp.reservation_id);

            if ((lstpayments != null))
            {
                foreach (PaymentModel p in lstpayments)
                {
                    //Make sure we have a payment and that the amount is >= amount to refund
                    //And p.PaymentAmount <= amountToRefund
                    if (p.id > 0 && amountToRefund > 0)
                    {

                        Common.Payments.Transaction trans = new Common.Payments.Transaction();
                        Common.Payments.Transaction.ChargeType chargeType = default(Common.Payments.Transaction.ChargeType);

                        trans.Transactions = Common.Payments.Transaction.TransactionType.Rsvp;
                        trans.CheckOrRefNumber = rsvp.booking_code;

                        UserDetails user = new UserDetails();
                        user.FirstName = rsvp.user_detail.first_name;
                        user.LastName = rsvp.user_detail.last_name;
                        user.HomePhoneStr = rsvp.user_detail.phone_number;
                        user.Email = rsvp.user_detail.email;
                        user.ZipCode = rsvp.user_detail.address.zip_code;

                        trans.User = user;
                        trans.WineryId = rsvp.member_id;

                        if (BookedById > 0)
                            trans.ProcessedBy = BookedById;
                        else
                            trans.ProcessedBy = rsvp.user_detail.user_id;


                        //Get Card used on Payment
                        Common.Payments.CreditCard card = new Common.Payments.CreditCard();
                        card.Number = p.payment_card_number;
                        card.CustName = p.payment_card_customer_name;
                        card.ExpMonth = p.payment_card_exp_month;
                        card.ExpYear = p.payment_card_exp_year;
                        card.Type = p.payment_card_type;
                        card.CardToken = p.payment_card_token;
                        card.CardLastFourDigits = p.card_last_four_digits;
                        card.CardFirstFourDigits = p.card_first_four_digits;
                        //Set Card
                        trans.Card = card;

                        //trans.OrigAmount = p.amount;
                        //Set Amount
                        if (p.amount == amountToRefund)
                        {
                            trans.Amount = amountToRefund;
                        }
                        else if (p.amount > amountToRefund)
                        {
                            trans.Amount = amountToRefund;
                        }
                        else
                        {
                            trans.Amount = p.amount;
                        }

                        //Issue Credit if Payment Date is more than 24 hours old, else try void first.
                        if (p.payment_date < DateTime.UtcNow.AddHours(-24) || trans.Amount < p.original_amount)
                        {
                            chargeType = Common.Payments.Transaction.ChargeType.Credit;
                        }
                        else
                        {
                            chargeType = Common.Payments.Transaction.ChargeType.Void;
                        }

                        //Set Trans Payment Type
                        trans.Type = chargeType;

                        //Original Transaction ID
                        trans.TransactionID = p.transaction_id;

                        trans.Gateway = p.payment_gateway;

                        //Process Payment
                        result = await Payments.ProcessPaymentV2(rsvp.member_id, rsvp.reservation_id, trans);

                        //If Result Fails and we did a Void then we try one more time as a Credit
                        if (result.Status == Common.Payments.TransactionResult.StatusType.Failed && chargeType == Common.Payments.Transaction.ChargeType.Void)
                        {
                            trans.Type = Common.Payments.Transaction.ChargeType.Credit;
                            result = await Payments.ProcessPaymentV2(rsvp.member_id, rsvp.reservation_id, trans);
                        }
                        else if (result.Status == Common.Payments.TransactionResult.StatusType.Failed && chargeType == Common.Payments.Transaction.ChargeType.Credit)
                        {
                            trans.Type = Common.Payments.Transaction.ChargeType.Void;
                            trans.Amount = p.amount;
                            result = await Payments.ProcessPaymentV2(rsvp.member_id, rsvp.reservation_id, trans);
                        }

                        if (result.Status == Common.Payments.TransactionResult.StatusType.Success)
                        {
                            amountToRefund = amountToRefund - p.amount;
                            RefundAmt = RefundAmt + result.Amount;
                            eventDAL.UpdateReservationPaymentV2(p.id, result.Amount);
                        }
                    }
                }
            }
            result.Amount = RefundAmt;
            return result;
        }

        public async Task<Common.Payments.TransactionResult> VoidReservationPreAuth(PaymentModel p, ReservationDetailModel rsvp, decimal amountToRefund, int BookedById)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            //PAYMENT/REFUND  
            Common.Payments.TransactionResult result = new Common.Payments.TransactionResult();
            decimal RefundAmt = 0;

            //if ((lstpayments != null))
            //{
            //foreach (PaymentModel p in lstpayments)
            //{
            //Make sure we have a payment and that the amount is >= amount to refund
            //And p.PaymentAmount <= amountToRefund
            if (p.id > 0 && amountToRefund > 0)
            {

                Common.Payments.Transaction trans = new Common.Payments.Transaction();
                Common.Payments.Transaction.ChargeType chargeType = default(Common.Payments.Transaction.ChargeType);

                trans.Transactions = Common.Payments.Transaction.TransactionType.Rsvp;
                trans.CheckOrRefNumber = rsvp.booking_code;

                UserDetails user = new UserDetails();
                user.FirstName = rsvp.user_detail.first_name;
                user.LastName = rsvp.user_detail.last_name;
                user.HomePhoneStr = rsvp.user_detail.phone_number;
                user.Email = rsvp.user_detail.email;
                user.ZipCode = rsvp.user_detail.address.zip_code;

                trans.User = user;
                trans.WineryId = rsvp.member_id;

                if (BookedById > 0)
                    trans.ProcessedBy = BookedById;
                else
                    trans.ProcessedBy = rsvp.user_detail.user_id;


                //Get Card used on Payment
                Common.Payments.CreditCard card = new Common.Payments.CreditCard();
                card.Number = p.payment_card_number;
                card.CustName = p.payment_card_customer_name;
                card.ExpMonth = p.payment_card_exp_month;
                card.ExpYear = p.payment_card_exp_year;
                card.Type = p.payment_card_type;
                card.CardToken = p.payment_card_token;
                card.CardLastFourDigits = p.card_last_four_digits;
                card.CardFirstFourDigits = p.card_first_four_digits;
                //Set Card
                trans.Card = card;

                //trans.OrigAmount = p.amount;
                //Set Amount
                if (p.amount == amountToRefund)
                {
                    trans.Amount = amountToRefund;
                }
                else if (p.amount > amountToRefund)
                {
                    trans.Amount = amountToRefund;
                }
                else
                {
                    trans.Amount = p.amount;
                }

                //Issue Credit if Payment Date is more than 24 hours old, else try void first.
                if (p.payment_date < DateTime.UtcNow.AddHours(-24) || trans.Amount < p.original_amount)
                {
                    chargeType = Common.Payments.Transaction.ChargeType.Credit;
                }
                else
                {
                    chargeType = Common.Payments.Transaction.ChargeType.Void;
                }

                //Set Trans Payment Type
                trans.Type = chargeType;

                //Original Transaction ID
                trans.TransactionID = p.transaction_id;


                //Process Payment
                result = await Payments.ProcessPaymentV2(rsvp.member_id, rsvp.reservation_id, trans, false, true);

                //If Result Fails and we did a Void then we try one more time as a Credit
                if (result.Status == Common.Payments.TransactionResult.StatusType.Failed && chargeType == Common.Payments.Transaction.ChargeType.Void)
                {
                    trans.Type = Common.Payments.Transaction.ChargeType.Credit;
                    result = await Payments.ProcessPaymentV2(rsvp.member_id, rsvp.reservation_id, trans, false, true);
                }
                else if (result.Status == Common.Payments.TransactionResult.StatusType.Failed && chargeType == Common.Payments.Transaction.ChargeType.Credit)
                {
                    trans.Type = Common.Payments.Transaction.ChargeType.Void;
                    trans.Amount = p.amount;
                    result = await Payments.ProcessPaymentV2(rsvp.member_id, rsvp.reservation_id, trans, false, true);
                }

                if (result.Status == Common.Payments.TransactionResult.StatusType.Success)
                {
                    amountToRefund = amountToRefund - p.amount;
                    RefundAmt = RefundAmt + result.Amount;
                    eventDAL.UpdateReservationPaymentV2(p.id, result.Amount);
                }
            }
            //}
            //}
            result.Amount = RefundAmt;
            return result;
        }

        public async Task<Common.Payments.TransactionResult> ChargeReservation(CreateReservationRequest rsvp, decimal amountToCharge, string booking_code, int user_id, Common.Payments.Transaction.ChargeType chargeType = Common.Payments.Transaction.ChargeType.Sale, string prevTranId = "")
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            Common.Payments.TransactionResult result = new Common.Payments.TransactionResult();
            Common.Payments.Transaction trans = new Common.Payments.Transaction();
            Common.Payments.CreditCard card = new Common.Payments.CreditCard();

            try
            {
                trans.Amount = amountToCharge;
                trans.Type = chargeType;
                if (chargeType == Common.Payments.Transaction.ChargeType.Capture && !string.IsNullOrWhiteSpace(prevTranId))
                {
                    trans.TransactionID = prevTranId;
                }
                trans.Transactions = Common.Payments.Transaction.TransactionType.Rsvp;
                trans.CheckOrRefNumber = booking_code;
                trans.ProcessedBy = user_id;

                UserDetails user = new UserDetails();
                user.FirstName = rsvp.user_detail.first_name;
                user.LastName = rsvp.user_detail.last_name;
                user.HomePhoneStr = rsvp.user_detail.phone_number;
                user.Email = rsvp.user_detail.email;
                user.ZipCode = rsvp.user_detail.address.zip_code;
                user.Country = rsvp.user_detail.address.country;
                user.City = rsvp.user_detail.address.city;
                user.State = rsvp.user_detail.address.state;
                user.Address1 = rsvp.user_detail.address.address_1;
                user.Address2 = rsvp.user_detail.address.address_2;
                if (!string.IsNullOrWhiteSpace(prevTranId))
                {
                    trans.TransactionID = prevTranId;
                }

                trans.User = user;

                card.Number = rsvp.pay_card.number;
                card.CustName = rsvp.pay_card.cust_name;
                card.ExpMonth = rsvp.pay_card.exp_month;
                card.ExpYear = rsvp.pay_card.exp_year;
                card.CVV = rsvp.pay_card.cvv2;
                card.CardToken = rsvp.pay_card.card_token;
                card.CardLastFourDigits = rsvp.pay_card.card_last_four_digits;
                card.CardFirstFourDigits = rsvp.pay_card.card_first_four_digits;
                card.Type = rsvp.pay_card.card_type;
                card.CardEntry = rsvp.pay_card.card_entry;
                card.ApplicationType = rsvp.pay_card.application_type;
                card.ApplicationVersion = rsvp.pay_card.application_version;
                card.TerminalId = rsvp.pay_card.terminal_id;
                card.CardReader = rsvp.pay_card.card_reader;
                //Set Card
                trans.Card = card;

                trans.WineryId = rsvp.member_id;

                //Process Payment
                result = await Payments.ProcessPaymentV2(rsvp.member_id, rsvp.reservation_id, trans);
            }
            catch (Exception ex)
            {
                result.Status = Common.Payments.TransactionResult.StatusType.Failed;
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("ChargeReservation", "ReservationId-" + rsvp.reservation_id.ToString() + ",Message-" + ex.Message, "", 1, rsvp.member_id);
            }

            return result;
        }
        //private static bool cardIsValid(Common.Payments.CreditCard CC)
        //{
        //    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
        //    //Lets validate card first:
        //    Icharge iCharge = new Icharge();
        //    //iCharge.RuntimeLicense = "42504E35414131535542524131535542374A4348343834350000000000000000000000000000000048554A35463837560000445A42354B484457475546550000";
        //    iCharge.RuntimeLicense = "42504E4241413153554252413153554241554743303834330000000000000000000000000000000054305A4632574D5400004A34374641374238483138450000";
        //    iCharge.Reset();

        //    Cardvalidator CV = new Cardvalidator();
        //    //CV.RuntimeLicense = "42504E35414131535542524131535542374A4348343834350000000000000000000000000000000048554A35463837560000445A42354B484457475546550000";
        //    CV.RuntimeLicense = "42504E4241413153554252413153554241554743303834330000000000000000000000000000000054305A4632574D5400004A34374641374238483138450000";
        //    iCharge.Card.Number = CC.Number;
        //    iCharge.Card.ExpMonth = Int32.Parse((string.IsNullOrEmpty(CC.ExpMonth) ? "1" : CC.ExpMonth));
        //    iCharge.Card.ExpYear = Int32.Parse((string.IsNullOrEmpty(CC.ExpYear) ? "1" : CC.ExpYear));

        //    try
        //    {
        //        CV.CardNumber = iCharge.Card.Number;
        //        CV.CardExpMonth = iCharge.Card.ExpMonth;
        //        CV.CardExpYear = iCharge.Card.ExpYear;
        //        CV.ValidateCard();
        //        return true;
        //    }
        //    catch (InPayException ex)
        //    {
        //        logDAL.InsertLog("Payments.cardIsValid", ex.Message, "", 1, 0);
        //        //ex.message
        //    }
        //    return false;
        //}

        public static TokenizedCard TokenziedCard(TokenizedCardRequest request, PaymentConfigModel paymentConfig)
        {
            TokenizedCard card = null;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            if (string.IsNullOrEmpty(request.cvv2))
            {
                logDAL.InsertLog("WebApi", "TokenizeCard:  " + JsonConvert.SerializeObject(request), "", 3, request.member_id);
            }

            request.number = request.number.Replace(" ", "");

            Configuration config = new Configuration();
            config.MerchantLogin = paymentConfig.MerchantLogin;
            config.MerchantPassword = paymentConfig.MerchantPassword;
            config.UserConfig1 = paymentConfig.UserConfig1;
            config.UserConfig2 = paymentConfig.UserConfig2;
            config.GatewayMode = paymentConfig.GatewayMode;
            config.PaymentGateway = (int)paymentConfig.PaymentGateway;
            switch (paymentConfig.PaymentGateway)
            {
                case Configuration.Gateway.Braintree:
                    Braintree objBraintree = new Braintree(_appSetting);
                    card = Braintree.TokenziedCard(request, config);
                    break;
                case Configuration.Gateway.OpenEdge:
                    OpenEdge objOpenEdge = new OpenEdge(_appSetting);
                    card = OpenEdge.TokenziedCard(request, config);
                    break;
                case Configuration.Gateway.USAePay:
                    USAePay objUSAePay = new USAePay(_appSetting);
                    card = USAePay.TokenziedCard(request, config);
                    break;
                case Configuration.Gateway.AuthorizeNet:
                    AuthNet objAuthNet = new AuthNet(_appSetting);
                    card = AuthNet.TokenziedCard(request, config);
                    break;
                case Configuration.Gateway.WorldPayXML:
                    WorldPayXML objWorldPayXML = new WorldPayXML(_appSetting);
                    card = WorldPayXML.TokenziedCard(request, config);
                    break;
                case Configuration.Gateway.Cybersource:
                    Cybersource objCyberSource = new Cybersource(_appSetting);
                    card = Cybersource.TokenziedCard(request, config);
                    break;
                case Configuration.Gateway.Stripe:
                    Stripe objStripe = new Stripe(_appSetting);
                    card = objStripe.GetTokenId(request, _appSetting.Value.StripeSecretKey);
                    break;
                case Configuration.Gateway.CardConnect:
                    CardConnectRestClient cardConnectRestClient = new CardConnectRestClient(_appSetting);
                    card = CardConnectRestClient.TokenziedCard(request, config);
                    break;
                case Configuration.Gateway.Shift4:
                    config.MerchantLogin = _appSetting.Value.Shift4ClientGUID;
                    Shift4PaymentService Shift4PaymentService = new Shift4PaymentService(_appSetting);
                    card = Shift4PaymentService.TokenziedCard(request, config);
                    break;
                case Configuration.Gateway.Zeamster:
                    Zeamster zeamsterSvc = new Zeamster(_appSetting);
                    config.UserConfig1 = _appSetting.Value.ZeamsterDeveloperId;
                    card = Zeamster.TokenziedCard(request, config);
                    break;
                case Configuration.Gateway.Commrece7Payments:

                    card = Task.Run(() => Utility.TokenizeAndAddCommerce7Card(_appSetting, request, config)).Result;
                    break;
            }

            if (card != null && !string.IsNullOrEmpty(card.card_token) && request.user_info != null)
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                int member_id = request.member_id;

                if (request.source_module == Common.ModuleType.Ticketing)
                {
                    var tktPayProcessor = eventDAL.GetTicketPaymentProcessorByWinery(member_id);
                    if (tktPayProcessor == Common.Common.TicketsPaymentProcessor.CellarPassProcessor)
                        member_id = -1;
                }

                eventDAL.InsertCreditCardDetail(Payments.GetCardType(request.number), request.cust_name, request.exp_month, request.exp_year, card.card_token, member_id, Common.Common.Right(request.number, 4), Common.Common.Left(request.number, 4), (int)request.source_module, request.user_info.email, request.user_info.first_name, request.user_info.last_name, request.user_id, (int)paymentConfig.PaymentGateway);
            }
            

            return card;
        }

        public async static Task<Common.Payments.TransactionResult> RefundTicket(TicketRefundRequest request)
        {
            Common.Payments.TransactionResult response = new Common.Payments.TransactionResult();
            TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
            TicketOrder ticketOrderModel = ticketDAL.GetTicketOrderById(request.ticket_order_id);

            if (request.amount > 0)
            {
                int member_id = ticketOrderModel.Winery_Id;

                Common.Payments.Transaction trans = new Common.Payments.Transaction();
                Common.Payments.CreditCard card = new Common.Payments.CreditCard();

                trans.ProcessedBy = ticketOrderModel.User_Id;

                //if (ticketOrderModel.refund_service_fees_option == Common.Common.RefundServiceFeesOption.TicketbuyerPaysFees && request.payment_gateway != Configuration.Gateway.Stripe)
                //{
                //    trans.Amount = request.amount - ticketOrderModel.ServiceFee;
                //}
                //else
                //{
                //    trans.Amount = request.amount;
                //}

                trans.Amount = request.amount;
                trans.OrigAmount = ticketDAL.GetAmountBytransactionId(request.transaction_id,request.ticket_order_id);
                trans.Type = request.charge_type;
                trans.Transactions = Common.Payments.Transaction.TransactionType.TicketSale;

                Common.Payments.UserDetails user = new Common.Payments.UserDetails();
                user.FirstName = ticketOrderModel.BillFirstName;
                user.LastName = ticketOrderModel.BillLastName;
                user.HomePhoneStr = ticketOrderModel.BillHomePhone;
                user.Email = ticketOrderModel.BillEmailAddress;
                user.ZipCode = ticketOrderModel.BillZip;

                trans.User = user;

                card.Number = request.pay_card_number;
                card.CustName = request.pay_card_custName;
                card.ExpMonth = request.pay_card_exp_month;
                card.ExpYear = request.pay_card_exp_year;
                card.CardToken = request.card_token;
                card.CardLastFourDigits = request.pay_card_last_four_digits;
                card.CardFirstFourDigits = request.pay_card_first_four_digits;
                card.Type = request.pay_card_type;
                //Set Card
                trans.Card = card;
                trans.WineryId = ticketOrderModel.Winery_Id;
                trans.TransactionID = request.transaction_id;

                var PaymentGateway = request.payment_gateway;
                int wid = -1;
                if (PaymentGateway != Configuration.Gateway.Braintree)
                    wid = ticketOrderModel.Winery_Id;

                if (PaymentGateway == Configuration.Gateway.Stripe)
                {
                    TixOrderCalculationModel taxCalculationModel = new TixOrderCalculationModel();

                    taxCalculationModel.grand_total = request.amount;
                    taxCalculationModel.service_fees = ticketOrderModel.ServiceFee;
                    Services.Payments objPayments = new Services.Payments(_appSetting);
                    Services.Stripe obj = new Services.Stripe(_appSetting);
                    response = Services.Stripe.ChargeStripe(ticketOrderModel, taxCalculationModel, _appSetting.Value.StripeSecretKey, request.card_token, trans);
                }
                else
                {
                    Services.Payments objPayments = new Services.Payments(_appSetting);
                    response = await Services.Payments.ProcessPaymentV2(wid, request.ticket_order_id, trans);

                    if (response.DoFollowUpSale && response.Amount > trans.Amount)
                    {
                        trans.Amount = response.Amount - trans.Amount;
                        trans.Type = Common.Payments.Transaction.ChargeType.Sale;
                        trans.TransactionID = "";

                        await Services.Payments.ProcessPaymentV2(wid, request.ticket_order_id, trans);
                    }
                }
            }
            return response;
        }

        public async static Task<Common.Payments.TransactionResult> ProcessPaymentV2(int wineryID, int invoiceID, Common.Payments.Transaction transaction, bool saveCard = false, bool isPreAuth = false)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

            transaction.Card.Number = transaction.Card.Number.Replace(" ", "").Replace("-", "");

            wineryID = transaction.WineryId;
            if (transaction.Transactions == Common.Payments.Transaction.TransactionType.TicketSale)
            {
                wineryID = -1;
            }

            Common.Payments.Configuration config = new Common.Payments.Configuration();
            if (transaction.Gateway != Configuration.Gateway.Offline && (transaction.Type == Common.Payments.Transaction.ChargeType.Credit || transaction.Type == Common.Payments.Transaction.ChargeType.Void))
            {
                config = eventDAL.GetWineryPaymentConfiguration(wineryID, (int)transaction.Gateway, 1);
            }
            else
            {
                config = eventDAL.GetWineryPaymentConfiguration(wineryID);
            }

            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            Common.Payments.TransactionResult result = new Common.Payments.TransactionResult();

            //Get Payment Config
            bool isLive = false;

            //if (string.IsNullOrEmpty(transaction.Card.CardToken))
            if (!string.IsNullOrEmpty(transaction.Card.Number) && transaction.Card.Number.Length> 10)
            {
                if (transaction.Type == Common.Payments.Transaction.ChargeType.Sale)
                {
                    if (transaction.Card != null && !string.IsNullOrWhiteSpace(transaction.Card.Number) && !string.IsNullOrWhiteSpace(transaction.Card.ExpMonth) && !string.IsNullOrWhiteSpace(transaction.Card.ExpYear))
                    {
                        TokenizedCard card = null;
                        var paymentConfig = eventDAL.GetPaymentConfigByWineryId(wineryID, 1);

                        TokenizedCardRequest request = new TokenizedCardRequest();

                        request.card_type = transaction.Card.Type;
                        request.cust_name = transaction.Card.CustName;
                        request.exp_month = transaction.Card.ExpMonth;
                        request.exp_year = transaction.Card.ExpYear;
                        request.member_id = wineryID;
                        request.number = transaction.Card.Number;
                        request.cvv2 = transaction.Card.CVV;

                        UserDetailViewModel user_info = new UserDetailViewModel();
                        ViewModels.UserAddress address = new ViewModels.UserAddress();

                        user_info.email = transaction.User.Email;
                        user_info.first_name = transaction.User.FirstName;
                        user_info.last_name = transaction.User.LastName;
                        user_info.phone_number = transaction.User.HomePhoneStr;
                        address.city = transaction.User.City;
                        address.state = transaction.User.State;
                        address.zip_code = transaction.User.ZipCode;
                        address.country = transaction.User.Country;
                        address.address_1 = transaction.User.Address1;

                        if (!string.IsNullOrWhiteSpace(transaction.User.HomePhoneStr))
                        {
                            user_info.phone_number = transaction.User.HomePhoneStr;

                        }
                        else
                        {
                            user_info.phone_number = Utility.FormatTelephoneNumber(transaction.User.Phone.ToString(), transaction.User.Country);
                        }

                        if (string.IsNullOrEmpty(address.city) || string.IsNullOrEmpty(address.state) || string.IsNullOrEmpty(address.country) || string.IsNullOrEmpty(address.address_1) || string.IsNullOrEmpty(address.zip_code))
                        {
                            var userDetailModel = new List<UserDetailModel>();
                            userDetailModel = await Utility.GetUsersByEmail(transaction.User.Email, transaction.WineryId, false, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);

                            if (userDetailModel != null && userDetailModel.Count > 0)
                            {
                                if (string.IsNullOrEmpty(address.city) || string.IsNullOrEmpty(address.state) || string.IsNullOrEmpty(address.zip_code))
                                {
                                    address.city = userDetailModel[0].address.city;
                                    address.state = userDetailModel[0].address.state;
                                    address.zip_code = userDetailModel[0].address.zip_code;
                                }

                                if (string.IsNullOrEmpty(address.country))
                                    address.country = userDetailModel[0].address.country;

                                if (string.IsNullOrEmpty(address.address_1))
                                    address.address_1 = userDetailModel[0].address.address_1;

                                if (string.IsNullOrEmpty(address.address_2))
                                    address.address_2 = userDetailModel[0].address.address_2;

                                if (string.IsNullOrEmpty(user_info.phone_number))
                                    user_info.phone_number = userDetailModel[0].phone_number;
                            }
                        }

                        if (!string.IsNullOrEmpty(address.zip_code))
                        {
                            if (string.IsNullOrEmpty(address.city) || string.IsNullOrEmpty(address.state))
                            {
                                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                                Model.UserAddress addr = userDAL.GetUserAddressByZipCode(address.zip_code);

                                address.city = addr.city;
                                address.state = addr.state;
                            }
                        }

                        if (string.IsNullOrEmpty(address.city) || string.IsNullOrEmpty(address.state) || string.IsNullOrEmpty(address.country) || string.IsNullOrEmpty(address.address_1) || string.IsNullOrEmpty(address.zip_code))
                        {
                            LocationModel locationModel = eventDAL.GetLocationAddressByReservationId(invoiceID);

                            if (string.IsNullOrEmpty(address.zip_code) || string.IsNullOrEmpty(address.city) || string.IsNullOrEmpty(address.state))
                            {
                                address.city = locationModel.address.city;
                                address.state = locationModel.address.state;
                                address.zip_code = locationModel.address.zip_code;
                            }

                            if (string.IsNullOrEmpty(address.country))
                                address.country = locationModel.address.country;

                            if (string.IsNullOrEmpty(address.address_1))
                                address.address_1 = locationModel.address.address_1;

                            if (string.IsNullOrEmpty(address.address_2))
                                address.address_2 = locationModel.address.address_2;
                        }

                        user_info.address = address;
                        request.user_info = user_info;

                        request.card_last_four_digits = transaction.Card.CardLastFourDigits;
                        request.card_first_four_digits = transaction.Card.CardFirstFourDigits;

                        card = TokenziedCard(request, paymentConfig);
                        TokenizedCard secondarycard = null;
                        if (card == null)
                        {
                            result.Status = Common.Payments.TransactionResult.StatusType.Failed;

                            switch (transaction.Transactions)
                            {
                                case Common.Payments.Transaction.TransactionType.Billing:
                                    break;
                                case Common.Payments.Transaction.TransactionType.Rsvp:
                                    eventDAL.SaveReservationPaymentV2(wineryID, invoiceID, result);
                                    break;
                                case Common.Payments.Transaction.TransactionType.TicketSale:
                                    result.Card.CardLastFourDigits = Common.Common.Right(transaction.Card.Number, 4);
                                    result.Card.CardFirstFourDigits = Common.Common.Left(transaction.Card.Number, 4);
                                    ticketDAL.TicketsOrderPaymentInsert(invoiceID, result);
                                    break;
                            }

                            return result;
                        }
                        else if (card != null && string.IsNullOrEmpty(card.card_token))
                        {
                            result.Status = Common.Payments.TransactionResult.StatusType.Failed;
                            result.Detail = card.ErrorMessage;

                            if (result.Card == null)
                            {
                                result.Card = transaction.Card;
                                result.Amount = transaction.Amount;
                            }

                            switch (transaction.Transactions)
                            {
                                case Common.Payments.Transaction.TransactionType.Billing:
                                    break;
                                case Common.Payments.Transaction.TransactionType.Rsvp:
                                    eventDAL.SaveReservationPaymentV2(wineryID, invoiceID, result);
                                    break;
                                case Common.Payments.Transaction.TransactionType.TicketSale:
                                    result.Card.CardLastFourDigits = Common.Common.Right(transaction.Card.Number, 4);
                                    result.Card.CardFirstFourDigits = Common.Common.Left(transaction.Card.Number, 4);
                                    ticketDAL.TicketsOrderPaymentInsert(invoiceID, result);
                                    break;
                            }

                            return result;
                        }
                        else
                        {
                            transaction.Card.CardToken = card.card_token;
                            eventDAL.UpdateReservationPaycardtoken(invoiceID, card.card_token);
                            var secondaryPaymentConfig = eventDAL.GetPaymentConfigByWineryId(wineryID, 2);

                            if (secondaryPaymentConfig != null && secondaryPaymentConfig.PaymentGateway != Configuration.Gateway.Offline)
                            {
                                secondarycard = TokenziedCard(request, secondaryPaymentConfig);
                            }
                            try
                            {
                                var winery = new Model.WineryModel();
                                winery = eventDAL.GetWineryById(wineryID);

                                if ((winery.EnableVin65 || winery.EnableClubVin65) && !string.IsNullOrEmpty(card.card_token) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(winery.SALT) && !string.IsNullOrEmpty(winery.DecryptKey))
                                {
                                    string cardtype2 = Payments.GetCardType(request.number, "vin65");
                                    string cardnumber = Common.StringHelpers.EncryptedCardNumber(request.number, winery.SALT, winery.DecryptKey);
                                    eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, card.card_token, wineryID, winery.Vin65UserName, winery.Vin65Password, request.cvv2);
                                    try
                                    {
                                        if (secondarycard != null && !string.IsNullOrWhiteSpace(secondarycard.card_token))
                                        {
                                            eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, secondarycard.card_token, request.member_id, winery.Vin65UserName, winery.Vin65Password, request.cvv2);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        logDAL.InsertLog("Payments.ProcessPaymentV2", ex.Message, transaction.User.Email, 1, wineryID);
                                    }
                                }
                                else if ((winery.EnableCommerce7 || winery.EnableClubCommerce7) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(card.card_token))
                                {
                                    string cardtype2 = Payments.GetCardType(request.number, "commerce7");
                                    string cardnumber = Common.Common.Left(request.number, 4) + Common.Common.Right(request.number, 4).PadLeft(request.number.Length - 4, '*');

                                    string gateway = "No Gateway";
                                    gateway = Utility.GetCommerce7PaymentGatewayName(paymentConfig.PaymentGateway);

                                    eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, card.card_token, wineryID, gateway, "",request.cvv2);
                                    try
                                    {
                                        if (secondarycard != null && !string.IsNullOrWhiteSpace(secondarycard.card_token))
                                        {
                                            gateway = Utility.GetCommerce7PaymentGatewayName(secondaryPaymentConfig.PaymentGateway);
                                            eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, secondarycard.card_token, request.member_id, gateway, "", request.cvv2);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        logDAL.InsertLog("Payments.ProcessPaymentV2", ex.Message, transaction.User.Email, 1, wineryID);
                                    }
                                    var reservationDetailModel = new ReservationDetail2Model();
                                    reservationDetailModel = eventDAL.GetReservationDetails2byReservationId(invoiceID);

                                    QueueService getStarted = new QueueService();

                                    var queueModel = new Common.EmailQueue();
                                    queueModel.EType = (int)Common.Email.EmailType.CreateThirdPartyContact;
                                    queueModel.BCode = invoiceID.ToString();
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
                            }
                        }
                    }
                    else
                    {
                        result.Status = Common.Payments.TransactionResult.StatusType.Failed;
                        return result;
                    }
                }
                else
                    isLive = true;
            }

            //if (_appSetting.Value.QueueName == "emailqueue")
            //    isLive = true;

            if (config.MerchantLogin == null || config.MerchantPassword == null)
            {
                logDAL.InsertLog("Payments.ProcessPaymentV2", "Fn Payment.ProcessPayment Payment Configuration Missing. Winery ID:" + wineryID, transaction.User.Email, 1, wineryID);
                result.Status = (int)Common.Payments.TransactionResult.StatusType.Failed;
                result.Detail = "Payment Configuration Missing";
                return result;
            }

            bool ProcessTransaction = false;

            try
            {
                if ((transaction.Card != null) &&
                    (!string.IsNullOrWhiteSpace(transaction.Card.CardToken) || (
                    !string.IsNullOrWhiteSpace(transaction.Card.Number) && !string.IsNullOrWhiteSpace(transaction.Card.ExpMonth) && !string.IsNullOrWhiteSpace(transaction.Card.ExpYear))))
                {
                    ProcessTransaction = true;
                }
            }
            catch (Exception ex)
            {
                ProcessTransaction = false;
            }

            if (ProcessTransaction)
            {
                //Process Transaction
                try
                {
                    Common.Payments.TransactionResult transResult = null;
                    if (!isLive)
                    {
                        //Tickets - If CenPos gateway was used on a previous transaction with this ticket order then overide config with these old values
                        if (transaction.Gateway == Common.Payments.Configuration.Gateway.CenPos)
                        {
                            config.PaymentGateway = (int)Configuration.Gateway.CenPos;
                            var config2 = eventDAL.GetWineryPaymentConfiguration(-9999);
                            if ((config2 != null))
                            {
                                config.MerchantLogin = config2.MerchantLogin;
                                config.MerchantPassword = config2.MerchantPassword;
                            }
                        }


                        if (config.PaymentGateway == (int)Configuration.Gateway.Braintree ||
                            config.PaymentGateway == (int)Configuration.Gateway.AuthorizeNet ||
                            config.PaymentGateway == (int)Configuration.Gateway.WorldPayXML ||
                            config.PaymentGateway == (int)Configuration.Gateway.USAePay ||
                            config.PaymentGateway == (int)Configuration.Gateway.Stripe ||
                            config.PaymentGateway == (int)Configuration.Gateway.Cybersource ||
                            config.PaymentGateway == (int)Configuration.Gateway.CardConnect ||
                            config.PaymentGateway == (int)Configuration.Gateway.Shift4 ||
                            config.PaymentGateway == (int)Configuration.Gateway.Zeamster ||
                            config.PaymentGateway == (int)Configuration.Gateway.Commrece7Payments
                            )
                        {
                            //USPS_Address addr = await USPS_LocationByZipCode(transaction.User.ZipCode);
                            var conf = eventDAL.GetWineryPaymentConfiguration(wineryID);
                            Configuration config1 = new Configuration();
                            config1.MerchantLogin = conf.MerchantLogin;
                            config1.MerchantPassword = conf.MerchantPassword;
                            config1.UserConfig1 = conf.UserConfig1;
                            config1.UserConfig2 = conf.UserConfig2;
                            config1.GatewayMode = (Configuration.Mode)conf.GatewayMode;
                            Transaction1 trans = new Transaction1();
                            trans.WineryId = transaction.WineryId;
                            trans.Transaction = (Transaction1.TransactionType)transaction.Transactions;
                            trans.TransactionID = transaction.TransactionID;
                            trans.OrigAmount = transaction.OrigAmount;
                            trans.ProcessedBy = transaction.ProcessedBy;
                            trans.CheckOrRefNumber = transaction.CheckOrRefNumber;
                            trans.Type = (Transaction1.ChargeType)transaction.Type;
                            trans.Amount = transaction.Amount;
                            trans.User.first_name = transaction.User.FirstName;
                            trans.User.last_name = transaction.User.LastName;
                            trans.User.email = transaction.User.Email;
                            trans.User.address.address_1 = transaction.User.Address1;
                            trans.User.address.address_2 = transaction.User.Address2;
                            trans.User.address.city = transaction.User.City;
                            trans.User.address.state = transaction.User.State;
                            trans.User.address.zip_code = transaction.User.ZipCode;
                            trans.User.address.country = transaction.User.Country;
                            trans.IsPreAuthCredit = isPreAuth;
                            if (!string.IsNullOrWhiteSpace(transaction.User.HomePhoneStr))
                            {
                                trans.User.phone_number = transaction.User.HomePhoneStr;

                            }
                            else
                            {
                                trans.User.phone_number = Utility.FormatTelephoneNumber(transaction.User.Phone.ToString(), transaction.User.Country);
                            }

                            if (config.PaymentGateway == (int)Configuration.Gateway.Cybersource || config.PaymentGateway == (int)Configuration.Gateway.CardConnect || (config.PaymentGateway == (int)Configuration.Gateway.Braintree && transaction.Type == Common.Payments.Transaction.ChargeType.Sale))
                            {
                                if (string.IsNullOrEmpty(trans.User.address.city) || string.IsNullOrEmpty(trans.User.address.state) || string.IsNullOrEmpty(trans.User.address.country) || string.IsNullOrEmpty(trans.User.address.address_1) || string.IsNullOrEmpty(trans.User.address.zip_code))
                                {
                                    var userDetailModel = new List<UserDetailModel>();
                                    userDetailModel = await Utility.GetUsersByEmail(trans.User.email, trans.WineryId, false, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);

                                    if (userDetailModel != null && userDetailModel.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(trans.User.address.city) || string.IsNullOrEmpty(trans.User.address.state) || string.IsNullOrEmpty(trans.User.address.zip_code))
                                        {
                                            trans.User.address.city = userDetailModel[0].address.city;
                                            trans.User.address.state = userDetailModel[0].address.state;
                                            trans.User.address.zip_code = userDetailModel[0].address.zip_code;
                                        }

                                        if (string.IsNullOrEmpty(trans.User.address.country))
                                            trans.User.address.country = userDetailModel[0].address.country;

                                        if (string.IsNullOrEmpty(trans.User.address.address_1))
                                            trans.User.address.address_1 = userDetailModel[0].address.address_1;

                                        if (string.IsNullOrEmpty(trans.User.address.address_2))
                                            trans.User.address.address_2 = userDetailModel[0].address.address_2;

                                        if (string.IsNullOrEmpty(trans.User.phone_number))
                                            trans.User.phone_number = userDetailModel[0].phone_number;
                                    }
                                }

                                if (!string.IsNullOrEmpty(trans.User.address.zip_code))
                                {
                                    if (string.IsNullOrEmpty(trans.User.address.city) || string.IsNullOrEmpty(trans.User.address.state))
                                    {
                                        UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                                        Model.UserAddress addr = userDAL.GetUserAddressByZipCode(trans.User.address.zip_code);

                                        trans.User.address.city = addr.city;
                                        trans.User.address.state = addr.state;
                                    }
                                }

                                if (string.IsNullOrEmpty(trans.User.address.city) || string.IsNullOrEmpty(trans.User.address.state) || string.IsNullOrEmpty(trans.User.address.country) || string.IsNullOrEmpty(trans.User.address.address_1) || string.IsNullOrEmpty(trans.User.address.zip_code))
                                {
                                    LocationModel locationModel = eventDAL.GetLocationAddressByReservationId(invoiceID);

                                    if (string.IsNullOrEmpty(trans.User.address.zip_code) || string.IsNullOrEmpty(trans.User.address.city) || string.IsNullOrEmpty(trans.User.address.state))
                                    {
                                        trans.User.address.city = locationModel.address.city;
                                        trans.User.address.state = locationModel.address.state;
                                        trans.User.address.zip_code = locationModel.address.zip_code;
                                    }

                                    if (string.IsNullOrEmpty(trans.User.address.country))
                                        trans.User.address.country = locationModel.address.country;

                                    if (string.IsNullOrEmpty(trans.User.address.address_1))
                                        trans.User.address.address_1 = locationModel.address.address_1;

                                    if (string.IsNullOrEmpty(trans.User.address.address_2))
                                        trans.User.address.address_2 = locationModel.address.address_2;
                                }
                            }

                            trans.Card = transaction.Card;
                            trans.Card.CustName = transaction.Card.CustName;
                            trans.Card.Number = transaction.Card.Number;
                            trans.Card.ExpMonth = transaction.Card.ExpMonth;
                            trans.Card.ExpYear = transaction.Card.ExpYear;
                            trans.Card.CVV = transaction.Card.CVV;
                            trans.Card.CardToken = transaction.Card.CardToken;
                            trans.Card.CardLastFourDigits = transaction.Card.CardLastFourDigits;
                            trans.Card.CardFirstFourDigits = transaction.Card.CardFirstFourDigits;

                            if (config.PaymentGateway == (int)Configuration.Gateway.Braintree)
                            {
                                Braintree obj = new Braintree(_appSetting);

                                transResult = Braintree.ProcessCreditCard(wineryID, invoiceID, config1, trans, saveCard);

                            }
                            else if (config.PaymentGateway == (int)Configuration.Gateway.AuthorizeNet)
                            {
                                AuthNet obj = new AuthNet(_appSetting);
                                transResult = AuthNet.ProcessCreditCard(wineryID, invoiceID, config, trans);
                            }
                            else if (config.PaymentGateway == (int)Configuration.Gateway.Cybersource)
                            {
                                Cybersource obj = new Cybersource(_appSetting);
                                transResult = Cybersource.ProcessCreditCard(wineryID, invoiceID, config, trans);
                            }
                            else if (config.PaymentGateway == (int)Configuration.Gateway.CardConnect)
                            {
                                CardConnectRestClient obj = new CardConnectRestClient(_appSetting);
                                transResult = CardConnectRestClient.ProcessCreditCard(wineryID, invoiceID, config, trans);
                            }
                            else if (config.PaymentGateway == (int)Configuration.Gateway.Shift4)
                            {
                                config.MerchantLogin = _appSetting.Value.Shift4ClientGUID;
                                Shift4PaymentService obj = new Shift4PaymentService(_appSetting);
                                transResult = Shift4PaymentService.ProcessCreditCard(wineryID, invoiceID, config, trans);
                            }
                            else if (config.PaymentGateway == (int)Configuration.Gateway.USAePay)
                            {
                                USAePay obj = new USAePay(_appSetting);
                                transResult = USAePay.ProcessCreditCard(wineryID, invoiceID, config, trans);
                            }
                            else if (config.PaymentGateway == (int)Configuration.Gateway.WorldPayXML)
                            {
                                WorldPayXML obj = new WorldPayXML(_appSetting);
                                transResult = WorldPayXML.ProcessCreditCard(wineryID, invoiceID, config, trans);
                            }
                            else if (config.PaymentGateway == (int)Configuration.Gateway.Stripe)
                            {
                                Payments objPayments = new Payments(_appSetting);
                                Stripe obj = new Stripe(_appSetting);
                                transResult = Stripe.ProcessCreditCard(wineryID, invoiceID, config, trans, _appSetting.Value.StripeSecretKey);
                            }
                            else if (config.PaymentGateway == (int)Configuration.Gateway.Zeamster)
                            {
                                config.UserConfig1 = _appSetting.Value.ZeamsterDeveloperId;
                                Zeamster obj = new Zeamster(_appSetting);
                                transResult = Zeamster.ProcessCreditCard(wineryID, invoiceID, config, trans);
                            }
                            else if (config.PaymentGateway == (int)Configuration.Gateway.Commrece7Payments)
                            {
                                config.UserConfig1 = _appSetting.Value.ZeamsterDeveloperId;
                                Commerce7Gateway obj = new Commerce7Gateway(_appSetting);
                                transResult = Commerce7Gateway.ProcessCreditCard(wineryID, invoiceID, config, trans);
                            }

                            result.Amount = transResult.Amount;
                            result.ApprovalCode = transResult.ApprovalCode;
                            result.AvsResponse = transResult.AvsResponse;
                            result.Card = (Common.Payments.CreditCard)transResult.Card;
                            result.CheckOrRefNumber = transResult.CheckOrRefNumber;
                            result.Detail = transResult.Detail;
                            result.DoFollowUpSale = transResult.DoFollowUpSale;
                            result.PaymentGateway = (Common.Payments.Configuration.Gateway)transResult.PaymentGateway;
                            result.PaymentID = transResult.PaymentID;
                            result.PayType = (Common.Common.PaymentType)PaymentType.CreditCard;
                            result.ProcessedBy = transResult.ProcessedBy;
                            result.ResponseCode = transResult.ResponseCode;
                            result.Status = (Common.Payments.TransactionResult.StatusType)transResult.Status;
                            result.TransactionID = transResult.TransactionID;
                            result.TransactionType = (Common.Payments.Transaction.ChargeType)transResult.TransactionType;
                        }
                        else if (config.PaymentGateway == (int)Configuration.Gateway.OpenEdge)
                        {
                            OpenEdge obj = new OpenEdge(_appSetting);
                            result = await OpenEdge.ProcessOpenEdgeCreditCard(wineryID, invoiceID, config, transaction);
                        }
                        else if (config.PaymentGateway == (int)Configuration.Gateway.Offline)
                        {
                            result.Status = Common.Payments.TransactionResult.StatusType.Success;
                            result.Card = transaction.Card;
                            result.Detail = "Offline Payment";
                            result.Amount = 0;
                            result.ApprovalCode = "0";
                            result.AvsResponse = "";
                            result.CheckOrRefNumber = "";
                            result.PaymentGateway = Common.Payments.Configuration.Gateway.Offline;
                            result.PaymentID = 0;
                            result.ResponseCode = "0";
                            result.TransactionID = "-";
                            result.TransactionType = Common.Payments.Transaction.ChargeType.None;
                        }
                        //else
                        //{
                        //    result = ProcessCreditCard(wineryID, invoiceID, config, transaction);
                        //}
                    }
                    else
                    {
                        if (config.PaymentGateway == (int)Configuration.Gateway.Braintree)
                        {
                            //USPS_Address addr = await USPS_LocationByZipCode(transaction.User.ZipCode);
                            var conf = eventDAL.GetWineryPaymentConfiguration(wineryID);
                            Configuration config1 = new Configuration();
                            config1.MerchantLogin = conf.MerchantLogin;
                            config1.MerchantPassword = conf.MerchantPassword;
                            config1.UserConfig1 = conf.UserConfig1;
                            config1.GatewayMode = (Configuration.Mode)conf.GatewayMode;
                            Transaction1 trans = new Transaction1();
                            trans.WineryId = transaction.WineryId;
                            trans.Transaction = (Transaction1.TransactionType)transaction.Transactions;
                            trans.TransactionID = transaction.TransactionID;
                            trans.OrigAmount = transaction.OrigAmount;
                            trans.ProcessedBy = transaction.ProcessedBy;
                            trans.CheckOrRefNumber = transaction.CheckOrRefNumber;
                            trans.Type = (Transaction1.ChargeType)transaction.Type;
                            trans.Amount = transaction.Amount;
                            trans.User.first_name = transaction.User.FirstName;
                            trans.User.last_name = transaction.User.LastName;
                            trans.User.email = transaction.User.Email;



                            trans.User.address.address_1 = transaction.User.Address1;
                            trans.User.address.address_2 = transaction.User.Address2;
                            trans.User.address.city = transaction.User.City;
                            trans.User.address.state = transaction.User.State;
                            trans.User.address.zip_code = transaction.User.ZipCode;
                            trans.User.address.country = transaction.User.Country;
                            trans.Card = transaction.Card;
                            trans.Card.CustName = transaction.Card.CustName;
                            trans.Card.Number = transaction.Card.Number;
                            trans.Card.ExpMonth = transaction.Card.ExpMonth;
                            trans.Card.ExpYear = transaction.Card.ExpYear;
                            trans.Card.CVV = transaction.Card.CVV;
                            trans.Card.CardToken = transaction.Card.CardToken;
                            Braintree obj = new Braintree(_appSetting);
                            transResult = Braintree.ProcessCreditCard(wineryID, invoiceID, config1, trans);
                            result.Amount = transResult.Amount;
                            result.ApprovalCode = transResult.ApprovalCode;
                            result.AvsResponse = transResult.AvsResponse;
                            result.Card = (Common.Payments.CreditCard)transResult.Card;
                            result.CheckOrRefNumber = transResult.CheckOrRefNumber;
                            result.Detail = transResult.Detail;
                            result.DoFollowUpSale = transResult.DoFollowUpSale;
                            result.PaymentGateway = (Common.Payments.Configuration.Gateway)transResult.PaymentGateway;
                            result.PaymentID = transResult.PaymentID;
                            result.PayType = (Common.Common.PaymentType)PaymentType.CreditCard;
                            result.ProcessedBy = transResult.ProcessedBy;
                            result.ResponseCode = transResult.ResponseCode;
                            result.Status = (Common.Payments.TransactionResult.StatusType)transResult.Status;
                            result.TransactionID = transResult.TransactionID;
                            result.TransactionType = (Common.Payments.Transaction.ChargeType)transResult.TransactionType;
                        }
                        else if (config.PaymentGateway == (int)Configuration.Gateway.OpenEdge)
                        {
                            OpenEdge obj = new OpenEdge(_appSetting);
                            result = await OpenEdge.ProcessOpenEdgeCreditCard(wineryID, invoiceID, config, transaction);
                        }
                        else if (config.PaymentGateway == (int)Configuration.Gateway.Offline)
                        {
                            result.Status = Common.Payments.TransactionResult.StatusType.Success;
                            result.Card = transaction.Card;
                            result.Detail = "Offline Payment";
                            result.Amount = 0;
                            result.ApprovalCode = "0";
                            result.AvsResponse = "";
                            result.CheckOrRefNumber = "";
                            result.PaymentGateway = Common.Payments.Configuration.Gateway.Offline;
                            result.PaymentID = 0;
                            result.ResponseCode = "0";
                            result.TransactionID = "-";
                            result.TransactionType = Common.Payments.Transaction.ChargeType.None;
                        }
                        //else
                        //{
                        //    result = ProcessCreditCard(wineryID, invoiceID, config, transaction);
                        //}

                    }
                }
                catch (Exception ex)
                {
                    result.Status = Common.Payments.TransactionResult.StatusType.Failed;

                    logDAL.InsertLog("Payments.ProcessPaymentV2", ex.Message, transaction.User.Email, 1, wineryID);
                }
            }
            else
            {
                result.Status = Common.Payments.TransactionResult.StatusType.Failed;
                result.Detail = "Payment Credit Card Detail Missing";

                try
                {
                    if (string.IsNullOrEmpty(transaction.Card.Type))
                        result.Card.Type = GetCardType(transaction.Card.Number);
                    else
                        result.Card.Type = transaction.Card.Type;

                    switch (transaction.Transactions)
                    {
                        case Common.Payments.Transaction.TransactionType.Billing:
                            break;
                        case Common.Payments.Transaction.TransactionType.Rsvp:
                            eventDAL.SaveReservationPaymentV2(wineryID, invoiceID, result);
                            break;
                        case Common.Payments.Transaction.TransactionType.TicketSale:
                            result.Card.CardLastFourDigits = Common.Common.Right(transaction.Card.Number, 4);
                            result.Card.CardFirstFourDigits = Common.Common.Left(transaction.Card.Number, 4);
                            ticketDAL.TicketsOrderPaymentInsert(invoiceID, result);
                            break;
                    }
                }
                catch { }

                return result;
            }

            if (result != null && result.Card != null)
            {
                result.Card.CardEntry = transaction.Card.CardEntry;
                result.Card.ApplicationType = transaction.Card.ApplicationType;
                result.Card.ApplicationVersion = transaction.Card.ApplicationVersion;
                result.Card.CardReader = transaction.Card.CardReader;
                result.Card.TerminalId = transaction.Card.TerminalId;

                if (string.IsNullOrEmpty(transaction.Card.Type))
                    result.Card.Type = GetCardType(transaction.Card.Number);
                else
                    result.Card.Type = transaction.Card.Type;

                //Save Result
                int paymentID = 0;
                switch (transaction.Transactions)
                {
                    case Common.Payments.Transaction.TransactionType.Billing:
                        break;
                    case Common.Payments.Transaction.TransactionType.Rsvp:
                        if (isPreAuth)
                            result.TransactionType = Common.Payments.Transaction.ChargeType.VoidAuth;
                        paymentID = eventDAL.SaveReservationPaymentV2(wineryID, invoiceID, result);
                        break;
                    case Common.Payments.Transaction.TransactionType.TicketSale:
                        result.Card.CardLastFourDigits = Common.Common.Right(transaction.Card.Number, 4);
                        result.Card.CardFirstFourDigits = Common.Common.Left(transaction.Card.Number, 4);
                        ticketDAL.TicketsOrderPaymentInsert(invoiceID, result);
                        break;
                }

                result.PaymentID = paymentID;
            }
            else
            {
                result.Status = Common.Payments.TransactionResult.StatusType.Failed;
                result.Detail = "Invalid Credit Card used for transaction.";

                try
                {
                    if (string.IsNullOrEmpty(transaction.Card.Type))
                        result.Card.Type = GetCardType(transaction.Card.Number);
                    else
                        result.Card.Type = transaction.Card.Type;

                    switch (transaction.Transactions)
                    {
                        case Common.Payments.Transaction.TransactionType.Billing:
                            break;
                        case Common.Payments.Transaction.TransactionType.Rsvp:
                            if (isPreAuth)
                                result.TransactionType = Common.Payments.Transaction.ChargeType.VoidAuth;
                            eventDAL.SaveReservationPaymentV2(wineryID, invoiceID, result);
                            break;
                        case Common.Payments.Transaction.TransactionType.TicketSale:
                            result.Card.CardLastFourDigits = Common.Common.Right(transaction.Card.Number, 4);
                            result.Card.CardFirstFourDigits = Common.Common.Left(transaction.Card.Number, 4);
                            ticketDAL.TicketsOrderPaymentInsert(invoiceID, result);
                            break;
                    }
                }
                catch { }
            }
            return result;

        }

        //public static Common.Payments.TransactionResult ProcessCreditCard(int wineryID, int invoiceId, Common.Payments.Configuration pcfg, Common.Payments.Transaction payment)
        //{
        //    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
        //    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
        //    string MerchantLogin = "";
        //    string MerchantPassword = "";
        //    string UserConfig1 = "";
        //    string UserConfig2 = "";

        //    bool TestMode = false;

        //    //Get Test Mode Value
        //    if ((_appSettings.PaymentDebug != null))
        //    {
        //        if (_appSettings.PaymentDebug == "1")
        //        {
        //            TestMode = true;
        //        }
        //    }

        //    //Return Obj
        //    Common.Payments.TransactionResult pr = new Common.Payments.TransactionResult();

        //    try
        //    {
        //        //Double Check Card
        //        if (!cardIsValid(payment.Card))
        //        {
        //            pr.Detail = "Invalid Card";
        //            pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
        //            pr.PayType = Common.Common.PaymentType.CreditCard;
        //            pr.Amount = payment.Amount;
        //            pr.Card = payment.Card;
        //            pr.ResponseCode = "";
        //            pr.TransactionID = "0";
        //            pr.ApprovalCode = "";
        //            pr.AvsResponse = "";
        //            pr.ProcessedBy = payment.ProcessedBy;
        //            pr.TransactionType = payment.Type;

        //            return pr;
        //        }

        //        //New Nsoftware Charge Obj
        //        Icharge iCharge = new Icharge();
        //        //iCharge.RuntimeLicense = "42504E35414131535542524131535542374A4348343834350000000000000000000000000000000048554A35463837560000445A42354B484457475546550000";
        //        iCharge.RuntimeLicense = "42504E4241413153554252413153554241554743303834330000000000000000000000000000000054305A4632574D5400004A34374641374238483138450000";
        //        iCharge.Reset();


        //        if ((pcfg != null))
        //        {
        //            MerchantLogin = pcfg.MerchantLogin;
        //            MerchantPassword = pcfg.MerchantPassword;
        //            UserConfig1 = pcfg.UserConfig1;
        //            UserConfig2 = pcfg.UserConfig2;

        //            //Get Gateway
        //            switch (pcfg.PaymentGateway)
        //            {
        //                case (int)Configuration.Gateway.Offline:
        //                    return null;
        //                case (int)Configuration.Gateway.AuthorizeNet:
        //                    iCharge.Gateway = IchargeGateways.gwAuthorizeNet;
        //                    break;
        //                case (int)Configuration.Gateway.PayFlowPro:
        //                    iCharge.Gateway = IchargeGateways.gwPayFlowPro;
        //                    break;
        //                case (int)Configuration.Gateway.CenPos:
        //                    iCharge.Gateway = IchargeGateways.gwAuthorizeNet;
        //                    iCharge.GatewayURL = "https://hub.cenpos.net/EAuthProxy/Handler.ashx";
        //                    break;
        //                case (int)Configuration.Gateway.USAePay:
        //                    iCharge.Gateway = IchargeGateways.gwUSAePay;
        //                    break;
        //                case (int)Configuration.Gateway.WorldPayXML:
        //                    iCharge.Gateway = IchargeGateways.gwWorldPayXML;
        //                    break;
        //                case (int)Configuration.Gateway.Cybersource:
        //                    iCharge.Gateway = IchargeGateways.gwCyberSource;
        //                    break;
        //            }

        //            //If Global Test Mode is False then see if Gateway Test Mode is On
        //            if (TestMode == false)
        //            {
        //                if (pcfg.GatewayMode == (Configuration.Mode)Configuration.Mode.test)
        //                {
        //                    TestMode = true;
        //                }
        //            }

        //            //## AMOUNT ##
        //            decimal PaymentAmount = payment.Amount;

        //            //Gateway Specifics
        //            switch (iCharge.Gateway)
        //            {
        //                case IchargeGateways.gwAuthorizeNet:
        //                    //Supports: Sale, AuthOnly, Capture, Credit, VoidTransaction

        //                    //CenPos Emulator
        //                    if (pcfg.PaymentGateway == (int)Configuration.Gateway.AuthorizeNet)
        //                    {
        //                        //Test Mode
        //                        //UserName	Password
        //                        //6KSwN7Hud4e	76FjuTu73r49tP4a
        //                        if (TestMode)
        //                        {
        //                            iCharge.GatewayURL = "https://test.authorize.net/gateway/transact.dll";
        //                        }
        //                        if (TestMode)
        //                        {
        //                            if (payment.Type != Common.Payments.Transaction.ChargeType.Sale)
        //                            {
        //                                iCharge.TestMode = true;
        //                            }

        //                        }
        //                    }
        //                    else if (pcfg.PaymentGateway == (int)Common.Payments.Configuration.Gateway.CenPos)
        //                    {
        //                        if (TestMode)
        //                        {
        //                            MerchantLogin = "test@10000009";
        //                            MerchantPassword = "Test@1234";
        //                            iCharge.TestMode = true;
        //                        }
        //                    }

        //                    break;
        //                //Use Hash Feature?
        //                //iCharge.Config("AIMHashSecret=XXX")
        //                case IchargeGateways.gwPayFlowPro:
        //                    //Supports: Sale, AuthOnly, Capture, Credit, VoidTransaction, Force

        //                    //Test Mode (Do not set TestMode property)
        //                    if (TestMode)
        //                    {
        //                        iCharge.GatewayURL = "https://pilot-payflowpro.paypal.com";
        //                    }

        //                    //When UserID and VendorID are different supply Vendor ID to MerchantLogin and UserID as Special
        //                    string UserID = MerchantLogin;
        //                    if (UserConfig2 != string.Empty)
        //                    {
        //                        if (MerchantLogin != UserConfig2)
        //                        {
        //                            UserID = UserConfig2;
        //                        }
        //                    }
        //                    //Check if Partner is different than default and update default value of special field
        //                    if (UserConfig1.Trim().ToLower() != "paypal")
        //                    {
        //                        foreach (var s in iCharge.SpecialFields)
        //                        {
        //                            if (s.Name == "PARTNER")
        //                            {
        //                                s.Value = UserConfig1.Trim();
        //                            }
        //                        }
        //                    }
        //                    //Add user as special field
        //                    iCharge.AddSpecialField("USER", UserID);
        //                    break;
        //                case IchargeGateways.gwUSAePay:
        //                    if (TestMode)
        //                    {
        //                        //iCharge.TestMode = True
        //                        iCharge.GatewayURL = "https://sandbox.usaepay.com/gate.php";
        //                    }

        //                    if (!string.IsNullOrEmpty(MerchantPassword))
        //                    {
        //                        // If a pin is associated with your source, send it with this config.
        //                        //icharge.Config("HashSecret=yourpinhere");
        //                        iCharge.Config("HashSecret=" + MerchantPassword);

        //                        //MerchantPassword is not used
        //                        MerchantPassword = "";
        //                    }
        //                    break;
        //                case IchargeGateways.gwWorldPayXML:
        //                    if (TestMode)
        //                    {
        //                        iCharge.GatewayURL = "https://secure-test.wp3.rbsworldpay.com/jsp/merchant/xml/paymentService.jsp";
        //                    }
        //                    iCharge.Config(string.Format("TerminalId={0}", UserConfig1));
        //                    //Change Payment Amount to Cents
        //                    PaymentAmount = (payment.Amount * 100);
        //                    break;
        //                case IchargeGateways.gwCyberSource:
        //                    if (TestMode)
        //                    {
        //                        iCharge.GatewayURL = "https://ics2wstest.ic3.com/commerce/1.x/transactionProcessor/";
        //                    }

        //                    break;
        //            }

        //            //Transaction Type
        //            string reference = "";
        //            switch (payment.Transactions)
        //            {
        //                case Common.Payments.Transaction.TransactionType.Billing:
        //                    reference = "CP-" + invoiceId;
        //                    break;
        //                case Common.Payments.Transaction.TransactionType.Rsvp:
        //                    string bookingCode = eventDAL.GetBookingCodeByReservationID(invoiceId);
        //                    if ((!object.ReferenceEquals(bookingCode, string.Empty)))
        //                    {
        //                        reference = string.Format("CP-{0}-{1}", wineryID, bookingCode);
        //                    }
        //                    else
        //                    {
        //                        reference = string.Format("CP-{0}-{1}", wineryID, payment.CheckOrRefNumber);
        //                    }
        //                    break;
        //                case Common.Payments.Transaction.TransactionType.TicketSale:
        //                    reference = string.Format("CP-Tickets-{0}-{1}", wineryID.ToString().PadLeft(4, '0'), invoiceId.ToString().PadLeft(8, '0'));
        //                    break;
        //            }


        //            iCharge.MerchantLogin = MerchantLogin;
        //            iCharge.MerchantPassword = MerchantPassword;

        //            iCharge.TransactionDesc = "Payment through CellarPass";
        //            iCharge.InvoiceNumber = reference;

        //            //Amount
        //            iCharge.TransactionAmount = String.Format("{0:0.00}", PaymentAmount);   //Convert.ToString(PaymentAmount);

        //            //Required
        //            iCharge.Customer.FirstName = payment.User.FirstName;
        //            iCharge.Customer.LastName = payment.User.LastName;
        //            iCharge.Customer.Address = (payment.User.Address1 + " " + payment.User.Address2).Trim();
        //            iCharge.Customer.City = payment.User.City;
        //            iCharge.Customer.State = payment.User.State;
        //            iCharge.Customer.Zip = payment.User.ZipCode;
        //            iCharge.Customer.Country = string.IsNullOrEmpty(payment.User.Country) ? "US" : payment.User.Country;
        //            //iCharge.Customer.Country = "US"

        //            //CYBERSOURCE REQUIRES FULL ADDRESS IF NOT DISABLED IN THE CYBERSOURCE ACCOUNT SETTINGS.

        //            if (iCharge.Gateway == IchargeGateways.gwCyberSource)
        //            {
        //                if (string.IsNullOrEmpty(iCharge.Customer.Address))
        //                {
        //                    iCharge.Customer.Address = "NA";
        //                }
        //                if (string.IsNullOrEmpty(iCharge.Customer.City))
        //                {
        //                    iCharge.Customer.City = "NA";
        //                }
        //                if (string.IsNullOrEmpty(iCharge.Customer.State))
        //                {
        //                    iCharge.Customer.State = "CA";
        //                }
        //            }

        //            iCharge.Customer.Email = payment.User.Email;
        //            if ((payment.User.HomePhoneStr.Length > 0))
        //            {
        //                iCharge.Customer.Phone = payment.User.HomePhoneStr;
        //            }
        //            else
        //            {
        //                string phone = Utility.ExtractPhone(payment.User.Phone.ToString().Trim().TrimStart('1')).ToString().Substring(0, 10);

        //                if ((phone != null))
        //                {
        //                    if (phone.Length != 10)
        //                    {
        //                        phone = "";
        //                    }
        //                }
        //                iCharge.Customer.Phone = phone;
        //            }


        //            iCharge.Customer.FullName = payment.Card.CustName;
        //            iCharge.Card.Number = payment.Card.Number;
        //            iCharge.Card.ExpMonth = Int32.Parse(payment.Card.ExpMonth);
        //            iCharge.Card.ExpYear = Int32.Parse(payment.Card.ExpYear);
        //            iCharge.Card.CVVData = (payment.Card.CVV == null ? "" : payment.Card.CVV);

        //            if (iCharge.Card.CVVData == string.Empty)
        //            {
        //                //iCharge.Card.CVVIndicator = TCVVIndicators.cvNotSet;
        //            }

        //            //Select Charge Type
        //            switch (payment.Type)
        //            {
        //                case Common.Payments.Transaction.ChargeType.Sale:
        //                    iCharge.Sale();
        //                    break;
        //                case Common.Payments.Transaction.ChargeType.AuthOnly:
        //                    iCharge.AuthOnly();
        //                    break;
        //                case Common.Payments.Transaction.ChargeType.Credit:
        //                    //iCharge.Credit(); //payment.TransactionID, PaymentAmount
        //                    iCharge.Refund(payment.TransactionID, String.Format("{0:0.00}", PaymentAmount));
        //                    break;
        //                case Common.Payments.Transaction.ChargeType.Void:
        //                    iCharge.VoidTransaction(payment.TransactionID);
        //                    break;
        //            }

        //            //DEBUG MODE
        //            //If on we write some data to the log.
        //            if ((_appSettings.PaymentDebug != null))
        //            {
        //                if (_appSettings.PaymentDebug == "1")
        //                {
        //                    logDAL.InsertLog("Payments.ProcessCreditCard", "ThirdPartyLib.nSoftware.ProcessCreditCard.RawRequest " + iCharge.Config("RawRequest"), payment.User.Email, 1, wineryID);
        //                    logDAL.InsertLog("Payments.ProcessCreditCard", "ThirdPartyLib.nSoftware.ProcessCreditCard.Response.Data " + iCharge.Response.Data, payment.User.Email, 1, wineryID);
        //                }
        //            }


        //            try
        //            {
        //                pr.PayType = Common.Common.PaymentType.CreditCard;
        //                pr.Amount = payment.Amount;
        //                pr.Card = payment.Card;
        //                pr.Detail = iCharge.Response.Text;
        //                pr.ResponseCode = iCharge.Response.Code;
        //                pr.TransactionID = iCharge.Response.TransactionId;
        //                pr.ApprovalCode = iCharge.Response.ApprovalCode;
        //                pr.AvsResponse = iCharge.Response.AVSResult;
        //                pr.ProcessedBy = payment.ProcessedBy;
        //                pr.TransactionType = payment.Type;
        //                //pr.PaymentGateway = pcfg.PaymentGateway;

        //                if ((iCharge.Response.Approved))
        //                {
        //                    pr.Status = Common.Payments.TransactionResult.StatusType.Success;
        //                }
        //                else
        //                {
        //                    pr.Status = Common.Payments.TransactionResult.StatusType.Failed;

        //                    //## Exceptions - In some cases we want the item to be approved on our end ##

        //                    //PayPal Payflow Pro
        //                    if (iCharge.Gateway == IchargeGateways.gwPayFlowPro)
        //                    {
        //                        if (iCharge.Response.Code == "126" && iCharge.Response.ApprovalCode != string.Empty)
        //                        {
        //                            pr.Status = Common.Payments.TransactionResult.StatusType.Success;
        //                        }
        //                    }
        //                }
        //            }
        //            catch (InPayException Ex)
        //            {
        //                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
        //                logDAL.InsertLog("Payment.ProcessCreditCard", Ex.Message, payment.User.Email, 1, wineryID);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
        //        logDAL.InsertLog("Payment.ProcessCreditCard", "MemberId:" + wineryID.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "", 1, wineryID);
        //    }

        //    return pr;

        //}

        public static ReservationResponse UpdateReservation(ReservationDetailModel reservation)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            ReservationResponse response = new ReservationResponse();
            try
            {
                if (reservation.slot_id > 0)
                {
                    //For Event Rule
                    if (reservation.slot_type == 0)
                    {
                        int eventId = eventDAL.GetEventRulesBySlotId(reservation.slot_id);
                        if (eventId > 0)
                            reservation.event_id = eventId;
                    }
                    else
                    {
                        //For Event Exceptions
                        int eventID = eventDAL.GetExceptionBySlotId(reservation.slot_id);
                        if (eventID > 0)
                            reservation.event_id = eventID;
                    }
                }

                var retval = eventDAL.UpdateReservation(reservation.reservation_id, reservation.amount_paid);


                response.success = true;

            }
            catch (Exception ex)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                logDAL.InsertLog("Reservations.UpdateReservation", ex.Message, reservation.user_detail.email, 1, reservation.member_id);
            }
            return response;
        }

        public static int GetAvailableSeats(int SlotId, int SlotType, System.DateTime EventDate)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            int capacity = 0;
            if (SlotType == 1)
            {
                capacity = eventDAL.GetEventExceptionbyIdAndStatus(SlotId, (int)Common.EventStatus.Active);
            }
            else
            {
                capacity = eventDAL.GetEventRulesbyId(SlotId).eventmodel.TotalSeats;
            }
            var guestSum = eventDAL.GetReservationGuestCount(SlotId, SlotType, EventDate);

            int taken = guestSum;

            int availableSeats = (capacity - taken);
            return availableSeats;
        }

        public class USPS_Address
        {
            public string AptOrSuite { get; set; }
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
        }
        public async static Task<USPS_Address> USPS_LocationByZipCode(string zipcode)
        {
            USPS_Address address = new USPS_Address();
            if (USPS_IsEnabled())
            {
                string strUSPS = null;
                strUSPS = _appSettings.USPS_URL + "?API=CityStateLookup&XML=<CityStateLookupRequest USERID=\"" + _appSettings.USPS_Username + "\">";
                strUSPS = strUSPS + "<ZipCode ID=\"0\">";
                strUSPS = strUSPS + "<Zip5>" + zipcode + "</Zip5>";
                strUSPS = strUSPS + "</ZipCode></CityStateLookupRequest>";
                string response = await GetDataFromSite(strUSPS);
                if ((response != null))
                {
                    if (!object.ReferenceEquals(response.Trim(), string.Empty))
                    {
                        //USPS RESPONSE
                        //<?xml version="1.0"?>
                        //<CityStateLookupResponse>
                        //<ZipCode ID="0">
                        //<Zip5>90210</Zip5>
                        //<City>BEVERLY HILLS</City>
                        //<State>CA</State>
                        //</ZipCode>
                        //</CityStateLookupResponse>
                        var messagesElement = XElement.Parse(response);
                        dynamic zipCodeXML = (from zc in messagesElement.Elements("ZipCode")
                                              select zc).FirstOrDefault();
                        if ((zipCodeXML != null))
                        {
                            try
                            {
                                address.City = zipCodeXML.Element("City").Value;
                                address.State = zipCodeXML.Element("State").Value;
                                address.ZipCode = zipCodeXML.Element("Zip5").Value;
                            }
                            catch (Exception ex)
                            {
                                address.City = "";
                                address.State = "";
                                address.ZipCode = "";
                            }
                        }
                    }
                }
            }
            return address;
        }
        private async static Task<string> GetDataFromSite(string USPS_Request)
        {
            string strResponse = "";

            HttpClient wsClient = new HttpClient();
            HttpResponseMessage response = await wsClient.GetAsync(USPS_Request);
            response.EnsureSuccessStatusCode();
            HttpContent content = response.Content;
            strResponse = await response.Content.ReadAsStringAsync();
            return strResponse;
        }

        private static bool USPS_IsEnabled()
        {
            if (!object.ReferenceEquals(_appSettings.USPS_Username.Trim(), string.Empty) && !object.ReferenceEquals(_appSettings.USPS_Password.Trim(), string.Empty) && !object.ReferenceEquals(_appSettings.USPS_URL.Trim(), string.Empty))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetCardType(string cardNumber, string CallBy = "")
        {

            string cardType = "";

            if ((cardNumber != null) && (cardNumber + "").Length > 4)
            {
                if ((!object.ReferenceEquals(cardNumber.Trim(), string.Empty)))
                {
                    // AMEX -- 34 or 37 -- 15 length
                    if ((Regex.IsMatch(cardNumber, "^(34|37)")))
                    {
                        if (CallBy == "vin65")
                        {
                            cardType = ("AmericanExpress");
                        }
                        else if (CallBy == "cybersource")
                        {
                            cardType = "003";
                        }
                        else if (CallBy == "commerce7")
                        {
                            cardType = "American Express";
                        }
                        else
                        {
                            cardType = ("American Express");
                        }


                        // MasterCard -- 51 through 55 -- 16 length
                    }
                    else if ((Regex.IsMatch(cardNumber, "^(51|52|53|54|55)")))
                    {
                        if (CallBy == "vin65")
                        {
                            cardType = ("MasterCard");
                        }
                        else if (CallBy == "cybersource")
                        {
                            cardType = "002";
                        }
                        else if (CallBy == "commerce7")
                        {
                            cardType = "MasterCard";
                        }
                        else
                        {
                            cardType = ("Master Card");
                        }

                        // VISA -- 4 -- 13 and 16 length
                    }
                    else if ((Regex.IsMatch(cardNumber, "^(4)")))
                    {
                        if (CallBy == "vin65")
                        {
                            cardType = ("Visa");
                        }
                        else if (CallBy == "cybersource")
                        {
                            cardType = "001";
                        }
                        else if (CallBy == "commerce7")
                        {
                            cardType = "Visa";
                        }
                        else
                        {
                            cardType = ("Visa");
                        }

                        // Diners Club -- 300-305, 36 or 38 -- 14 length
                    }
                    else if ((Regex.IsMatch(cardNumber, "^(300|301|302|303|304|305|36|38)")))
                    {
                        if (CallBy == "vin65")
                        {
                            cardType = ("Diners");
                        }
                        else if (CallBy == "cybersource")
                        {
                            cardType = "005";
                        }
                        else if (CallBy == "commerce7")
                        {
                            cardType = "Diners Club";
                        }
                        else
                        {
                            cardType = ("Diners Club");
                        }

                    }
                    else if ((Regex.IsMatch(cardNumber, "6(?:011\\d{12}|5\\d{14}|4[4-9]\\d{13}|22(?:1(?:2[6-9]|[3-9]\\d)|[2-8]\\d{2}|9(?:[01]\\d|2[0-5]))\\d{10})$")))
                    {
                        if (CallBy == "vin65")
                        {
                            cardType = ("Discover");
                        }
                        else if (CallBy == "cybersource")
                        {
                            cardType = "004";
                        }
                        else if (CallBy == "commerce7")
                        {
                            cardType = "Discover";
                        }
                        else
                        {
                            cardType = ("Discover");
                        }
                    }
                    else
                    {
                        cardType = "";
                    }

                    //Last Minute Attempt to get Card type if regex don't match
                    if (object.ReferenceEquals(cardType.Trim(), string.Empty))
                    {
                        //Just match by first digit if no card type was found
                        switch (Common.Common.Left(cardNumber, 1))
                        {
                            case "3":
                                if (CallBy == "vin65")
                                {
                                    cardType = ("AmericanExpress");
                                }
                                else if (CallBy == "cybersource")
                                {
                                    cardType = "003";
                                }
                                else if (CallBy == "commerce7")
                                {
                                    cardType = "American Express";
                                }
                                else
                                {
                                    cardType = ("American Express");
                                }
                                break;
                            case "5":
                                if (CallBy == "vin65")
                                {
                                    cardType = ("MasterCard");
                                }
                                else if (CallBy == "cybersource")
                                {
                                    cardType = "002";
                                }
                                else if (CallBy == "commerce7")
                                {
                                    cardType = "MasterCard";
                                }
                                else
                                {
                                    cardType = ("Master Card");
                                }
                                break;
                            case "4":
                                if (CallBy == "vin65")
                                {
                                    cardType = ("Visa");
                                }
                                else if (CallBy == "cybersource")
                                {
                                    cardType = "001";
                                }
                                else if (CallBy == "commerce7")
                                {
                                    cardType = "Visa";
                                }
                                else
                                {
                                    cardType = ("Visa");
                                }
                                break;
                            case "6":
                                if (CallBy == "vin65")
                                {
                                    cardType = ("Discover");
                                }
                                else if (CallBy == "cybersource")
                                {
                                    cardType = "004";
                                }
                                else if (CallBy == "commerce7")
                                {
                                    cardType = "Discover";
                                }
                                else
                                {
                                    cardType = ("Discover");
                                }

                                break;
                            default:
                                cardType = "Visa";
                                if (CallBy == "cybersource")
                                {
                                    cardType = "001";
                                }
                                break;
                        }
                    }
                }
            }

            return cardType;

        }
    }
}
