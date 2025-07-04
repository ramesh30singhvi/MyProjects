using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using System;
using static CPReservationApi.Common.Payments;
using Stripe;
using CPReservationApi.Model;
using CPReservationApi.DAL;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CPReservationApi.WebApi.Services
{
    public class Stripe
    {
        static private AppSettings _appSettings;
        public Stripe(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
        }

        public TokenizedCard GetTokenId(TokenizedCardRequest request, string stripeSecretKey)
        {
            string token = "";

            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                string stripeUserId = ticketDAL.GetStripeUserId(request.member_id);

                StripeConfiguration.ApiKey = stripeSecretKey;

                var requestOptions = new RequestOptions
                {
                    ApiKey = stripeSecretKey,
                    StripeAccount = stripeUserId
                };

                var myToken = new TokenCreateOptions
                {
                    Card = new TokenCardOptions
                    {
                        Number = request.number, // "4242424242424242",
                        ExpYear = request.exp_year, // "2022",
                        ExpMonth = request.exp_month, // "10",
                        Name = request.cust_name, // optional
                    }
                };

                var tokenService = new TokenService();
                var stripeToken = tokenService.Create(myToken, requestOptions);

                if (request.source_module == Common.ModuleType.Reservation)
                {
                    //attach this token to the customer 
                    string custEmail = "";
                    if (request.user_info != null && !string.IsNullOrWhiteSpace(request.user_info.email))
                    {
                        custEmail = request.user_info.email.Trim();
                    }
                    if (custEmail.Length == 0 && request.user_id > 0)
                    {
                        UserDAL userDal = new UserDAL(Common.Common.ConnectionString);
                        custEmail = userDal.GetUserEmailById(request.user_id);

                    }
                    if (custEmail.Length == 0)
                    {
                        custEmail = request.cust_name.Replace(" ", "_") + "@noemail.com";
                    }
                    var options = new CustomerCreateOptions
                    {
                        Source = stripeToken.Id,
                        Email = custEmail,
                        Description = request.cust_name + "- CellarPass"
                    };
                    var service = new CustomerService();
                    var customer = service.Create(options, requestOptions);


                    //token = stripeToken.Id;
                    token = customer.Id;
                }
                else
                {
                    token = stripeToken.Id;
                }
                var resp = new TokenizedCard
                {
                    card_token = token,
                    last_four_digits = Common.Common.Right(request.number, 4),
                    first_four_digits = Common.Common.Left(request.number, 4),
                    card_type = Payments.GetCardType(request.number),
                    is_expired = false
                };
                return resp;
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "WebApi-Stripe::Tokenized Card:  request-" + JsonConvert.SerializeObject(request) + ",Error-" + ex.Message.ToString(), "",1,request.member_id);

                var resp = new TokenizedCard();
                resp.card_token = "";
                resp.ErrorMessage = ex.Message;
                return resp;
            }
        }

        public static TransactionResult ProcessCreditCard(int wineryID, int invoiceId, Configuration pcfg, Payments.Transaction1 payment, string stripeSecretKey)
        {
            TransactionResult pr = new TransactionResult();
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                string stripeUserId = ticketDAL.GetStripeUserId(wineryID);
                if (!string.IsNullOrWhiteSpace(stripeUserId))
                {
                    StripeConfiguration.ApiKey = stripeSecretKey;

                    var requestOptions = new RequestOptions
                    {
                        ApiKey = stripeSecretKey,
                        StripeAccount = stripeUserId
                    };

                   

                    if (payment.Type == Payments.Transaction1.ChargeType.Sale || payment.Type == Payments.Transaction1.ChargeType.AuthOnly || payment.Type == Payments.Transaction1.ChargeType.Capture)
                    {
                        string chargeDesc = string.Empty;
                        //TODO
                        //chargeDesc = Utility.GenerateChargeDescription(reqModel.wineryName, reqModel.DynamicPaymentDesc, reqModel.Id.ToString(), Configuration.Gateway.Stripe);

                        if (string.IsNullOrEmpty(chargeDesc))
                            chargeDesc = "CellarPass Rsvp";

                        //Create Invoice Id String
                        string InvoiceRev = "";
                        string bookingCode = "";
                        switch (payment.Transaction)
                        {
                            case Payments.Transaction1.TransactionType.Billing:
                                InvoiceRev = "CP-" + invoiceId;
                                break;
                            case Payments.Transaction1.TransactionType.Rsvp:
                                bookingCode = eventDAL.GetBookingCodeByReservationID(invoiceId);
                                if (!object.ReferenceEquals(bookingCode, string.Empty))
                                {
                                    InvoiceRev = string.Format("CP-{0}-{1}", wineryID, bookingCode);
                                }
                                else
                                {
                                    InvoiceRev = string.Format("CP-{0}-{1}", wineryID, payment.CheckOrRefNumber);
                                }
                                break;
                            case Payments.Transaction1.TransactionType.TicketSale:
                                InvoiceRev = string.Format("CP-Tickets-{0}-{1}", wineryID.ToString().PadLeft(4, '0'), invoiceId.ToString().PadLeft(8, '0'));
                                break;
                        }

                        ReservationTransactionDetailModel reservationTransactionDetailModel = eventDAL.GetReservationTransactionDetail(invoiceId);

                        decimal TransactionFee = 0;
                        if (reservationTransactionDetailModel != null && reservationTransactionDetailModel.TransactionFee > 0)
                            TransactionFee = reservationTransactionDetailModel.TransactionFee;

                        var chargeCreateOptions = new ChargeCreateOptions();
                        var chargeService = new ChargeService();
                        var charge = new Charge();
                        if ((payment.Type == Payments.Transaction1.ChargeType.Sale || payment.Type == Payments.Transaction1.ChargeType.Capture) && !string.IsNullOrWhiteSpace(payment.TransactionID))
                        {

                            charge = chargeService.Capture(payment.TransactionID);
                        }
                        else
                        {
                            chargeCreateOptions.Amount = (long)(Math.Round(payment.Amount, 2) * 100);

                            if (TransactionFee > 0)
                                chargeCreateOptions.ApplicationFeeAmount = (long)(TransactionFee * 100);

                            chargeCreateOptions.Currency = "usd";
                            chargeCreateOptions.Customer = payment.Card.CardToken;
                            chargeCreateOptions.Description = InvoiceRev;
                            chargeCreateOptions.StatementDescriptor = chargeDesc;
                            chargeCreateOptions.Metadata = new Dictionary<string, string>() { { "CP OrderId", invoiceId.ToString() } };
                            chargeCreateOptions.Capture = (payment.Type != Payments.Transaction1.ChargeType.AuthOnly);
                            pr.PaymentGateway = Configuration.Gateway.Stripe;
                            pr.Card = payment.Card;
                            pr.ProcessedBy = payment.ProcessedBy;

                            charge = chargeService.Create(chargeCreateOptions, requestOptions);
                        }
                        pr.PayType = Common.Common.PaymentType.CreditCard;
                        pr.TransactionType = payment.Type == Payments.Transaction1.ChargeType.AuthOnly? Transaction.ChargeType.AuthOnly: Transaction.ChargeType.Sale;

                        if (charge != null)
                        {
                            if (charge.Status == "failed")
                            {
                                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;

                                string moreDetails = string.Empty;

                                if (charge.Outcome != null)
                                {
                                    if (!string.IsNullOrEmpty(charge.Outcome.SellerMessage))
                                    {
                                        moreDetails = " | " + charge.Outcome.SellerMessage;
                                    }
                                }
                                pr.Detail = charge.Status + moreDetails;
                            }
                            else if (charge.Status == "succeeded")
                            {
                                if (charge.Paid)
                                {
                                    pr.Status = Common.Payments.TransactionResult.StatusType.Success;
                                    pr.ApprovalCode = charge.Id;
                                    pr.Detail = charge.Status;
                                    pr.Amount = Math.Round(payment.Amount, 2);
                                    pr.AvsResponse = "";
                                    pr.TransactionID = charge.Id;

                                }
                                else
                                {
                                    pr.Status = Common.Payments.TransactionResult.StatusType.Success;
                                    pr.ApprovalCode = "";
                                    pr.Detail = "Success but Stripe shows Paid as False";
                                    pr.Amount = 0;
                                    pr.AvsResponse = "";
                                    pr.TransactionID = charge.Id;
                                }
                            }
                            else
                            {
                                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                                logDAL.InsertLog("WebApi", "WebApi-Stripe Charge:  responce-" + JsonConvert.SerializeObject(charge) + ",request-" + JsonConvert.SerializeObject(chargeCreateOptions), "", 3,wineryID);
                            }
                        }
                        else
                        {
                            pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                            logDAL.InsertLog("WebApi", "WebApi-Stripe Charge:  request-" + JsonConvert.SerializeObject(chargeCreateOptions), "", 3,wineryID);
                        }
                    }
                    else if (payment.Type == Payments.Transaction1.ChargeType.Void || payment.Type == Payments.Transaction1.ChargeType.Credit)
                    {
                        ReservationTransactionDetailModel reservationTransactionDetailModel = eventDAL.GetReservationTransactionDetail(invoiceId);

                        bool RefundFees = false;
                        if (reservationTransactionDetailModel != null && payment.Amount > 0)
                            RefundFees = reservationTransactionDetailModel.RefundFees;

                        pr.PayType = Common.Common.PaymentType.CreditCard;
                        pr.PaymentGateway = Configuration.Gateway.Stripe;
                        pr.Card = payment.Card;
                        pr.ProcessedBy = payment.ProcessedBy;
                        if (payment.Type == Payments.Transaction1.ChargeType.Void)
                            pr.TransactionType = Transaction.ChargeType.Void;

                        if (payment.Type == Payments.Transaction1.ChargeType.Credit)
                            pr.TransactionType = Transaction.ChargeType.Credit;

                        var options = new RefundCreateOptions();
                        options.Amount = (long)(Math.Round(payment.Amount, 2) * 100);

                        if (RefundFees)
                            options.RefundApplicationFee = false;

                        options.Reason = RefundReasons.RequestedByCustomer;
                        options.Charge = payment.TransactionID;
                        options.Metadata = new Dictionary<string, string>() { { "CP OrderId", invoiceId.ToString() } };

                        var service = new RefundService();
                        var refund = new Refund();
                        refund = service.Create(options, requestOptions);

                        if (refund != null)
                        {
                            if (refund.Status == "failed")
                            {
                                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                                pr.Detail = refund.Status;
                                pr.AvsResponse = "";
                                pr.TransactionID = "";
                            }
                            else if (refund.Status == "succeeded")
                            {
                                pr.Status = Common.Payments.TransactionResult.StatusType.Success;
                                pr.ApprovalCode = refund.Id;
                                pr.Detail = refund.Status + " | " + refund.ChargeId;
                                pr.Amount = Math.Round(payment.Amount, 2);
                                pr.AvsResponse = "";
                                pr.TransactionID = refund.Id;
                            }
                            else
                            {
                                pr.Status = Common.Payments.TransactionResult.StatusType.Success;
                                pr.ApprovalCode = refund.Id;
                                pr.Detail = refund.Status + " | " + refund.ChargeId;
                                pr.Amount = Math.Round(payment.Amount, 2);
                                pr.AvsResponse = "";
                                pr.TransactionID = refund.Id;

                                logDAL.InsertLog("WebApi", "WebApi-Stripe Charge:  responce-" + JsonConvert.SerializeObject(refund) + ",request-" + JsonConvert.SerializeObject(options), "", 3,wineryID);
                            }
                        }
                        else
                        {
                            pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                            logDAL.InsertLog("WebApi", "WebApi-Stripe Charge:  request-" + JsonConvert.SerializeObject(options), "", 3,wineryID);
                        }
                    }
                }
                else
                {
                    pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                    pr.Detail = "Result from Processing returned Nothing";
                }
            }
            catch (Exception ex)
            {
                pr.Detail = "Result from Processing returned Nothing";
                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                
                logDAL.InsertLog("Stripe.ProcessCreditCard", "MemberId:" + wineryID.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "",1,wineryID);
            }

            return pr;
        }

        public static Common.Payments.TransactionResult ChargeStripe(TicketOrder reqModel, TixOrderCalculationModel taxCalculationModel, string stripeSecretKey, string token, Common.Payments.Transaction payment)
        {
            Common.Payments.TransactionResult pr = new Common.Payments.TransactionResult();
            pr.Card = new CreditCard();

            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                pr.Card.Type = payment.Card.Type;
                pr.Card.Number = payment.Card.Number;
                pr.Card.CustName = payment.Card.CustName;
                pr.Card.ExpMonth = payment.Card.ExpMonth;
                pr.Card.ExpYear = payment.Card.ExpYear;
                pr.Card.CardLastFourDigits = payment.Card.CardLastFourDigits;
                pr.Card.CardFirstFourDigits = payment.Card.CardFirstFourDigits;

                string stripeUserId = ticketDAL.GetStripeUserId(reqModel.Winery_Id);
                if (!string.IsNullOrWhiteSpace(stripeUserId))
                {
                    StripeConfiguration.ApiKey = stripeSecretKey;

                    var requestOptions = new RequestOptions
                    {
                        ApiKey = stripeSecretKey,
                        StripeAccount = stripeUserId
                    };

                    if (payment.Type == Common.Payments.Transaction.ChargeType.Sale)
                    {
                        string chargeDesc = string.Empty;
                        chargeDesc = Utility.GenerateChargeDescription(reqModel.wineryName, reqModel.DynamicPaymentDesc, reqModel.Id.ToString(), Configuration.Gateway.Stripe);

                        if (string.IsNullOrEmpty(chargeDesc))
                            chargeDesc = "CellarPass TIX";

                        string description = String.Format("CP-Tickets-{0}-{1}", reqModel.Winery_Id.ToString().PadLeft(4, '0'), reqModel.Id.ToString().PadLeft(8, '0'));
                        var chargeCreateOptions = new ChargeCreateOptions
                        {
                            Amount = (long)(taxCalculationModel.grand_total * 100),
                            ApplicationFeeAmount = (long)(taxCalculationModel.service_fees * 100),
                            Currency = "usd",
                            Source = token,
                            Description = description,
                            StatementDescriptor = chargeDesc,
                            Metadata = new Dictionary<string, string>() { { "CP OrderId", reqModel.Id.ToString() } }
                        };
                        pr.PaymentGateway = Configuration.Gateway.Stripe;
                        pr.Card.CardToken = token;
                        var chargeService = new ChargeService();
                        var charge = new Charge();
                        charge = chargeService.Create(chargeCreateOptions, requestOptions);

                        pr.PayType = Common.Common.PaymentType.CreditCard;
                        pr.TransactionType = Transaction.ChargeType.Sale;

                        if (charge != null)
                        {
                            if (charge.Status == "failed")
                            {
                                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;

                                string moreDetails = string.Empty;

                                if (charge.Outcome != null)
                                {
                                    if (!string.IsNullOrEmpty(charge.Outcome.SellerMessage))
                                    {
                                        moreDetails = " | " + charge.Outcome.SellerMessage;
                                    }
                                }
                                pr.Detail = charge.Status + moreDetails;
                            }
                            else if (charge.Status == "succeeded")
                            {
                                if (charge.Paid)
                                {
                                    pr.Status = Common.Payments.TransactionResult.StatusType.Success;
                                    pr.ApprovalCode = charge.Id;
                                    pr.Detail = charge.Status;
                                    pr.Amount = taxCalculationModel.grand_total;
                                    pr.AvsResponse = "";
                                    pr.TransactionID = charge.Id;

                                }
                                else
                                {
                                    pr.Status = Common.Payments.TransactionResult.StatusType.Success;
                                    pr.ApprovalCode = "";
                                    pr.Detail = "Success but Stripe shows Paid as False";
                                    pr.Amount = 0;
                                    pr.AvsResponse = "";
                                    pr.TransactionID = charge.Id;
                                }
                            }
                            else
                            {
                                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                                logDAL.InsertLog("WebApi", "WebApi-Stripe Charge:  responce-" + JsonConvert.SerializeObject(charge) + ",request-" + JsonConvert.SerializeObject(chargeCreateOptions), "", 3,reqModel.Winery_Id);
                            }
                        }
                        else
                        {
                            pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                            logDAL.InsertLog("WebApi", "WebApi-Stripe Charge:  request-" + JsonConvert.SerializeObject(chargeCreateOptions), "", 3,reqModel.Winery_Id);
                        }
                    }
                    else if (payment.Type == Common.Payments.Transaction.ChargeType.Void || payment.Type == Common.Payments.Transaction.ChargeType.Credit)
                    {
                        var options = new RefundCreateOptions
                        {
                            Amount = (long)(taxCalculationModel.grand_total * 100),
                            RefundApplicationFee = false,
                            Reason = RefundReasons.RequestedByCustomer,
                            Charge = payment.TransactionID,
                            Metadata = new Dictionary<string, string>() { { "CP OrderId", reqModel.Id.ToString() } }
                        };

                        var service = new RefundService();
                        var refund = new Refund();
                        refund = service.Create(options, requestOptions);

                        pr.PayType = Common.Common.PaymentType.CreditCard;
                        pr.PaymentGateway = Configuration.Gateway.Stripe;

                        if (payment.Type == Common.Payments.Transaction.ChargeType.Void)
                            pr.TransactionType = Transaction.ChargeType.Void;

                        if (payment.Type == Common.Payments.Transaction.ChargeType.Credit)
                            pr.TransactionType = Transaction.ChargeType.Credit;

                        if (refund != null)
                        {
                            if (refund.Status == "failed")
                            {
                                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                                pr.Detail = refund.Status;
                                pr.AvsResponse = "";
                                pr.TransactionID = "";
                            }
                            else if (refund.Status == "succeeded")
                            {
                                pr.Status = Common.Payments.TransactionResult.StatusType.Success;
                                pr.ApprovalCode = refund.Id;
                                pr.Detail = refund.Status + " | " + refund.ChargeId;
                                pr.Amount = taxCalculationModel.grand_total;
                                pr.AvsResponse = "";
                                pr.TransactionID = refund.Id;
                            }
                            else
                            {
                                pr.Status = Common.Payments.TransactionResult.StatusType.Success;
                                pr.ApprovalCode = refund.Id;
                                pr.Detail = refund.Status + " | " + refund.ChargeId;
                                pr.Amount = taxCalculationModel.grand_total;
                                pr.AvsResponse = "";
                                pr.TransactionID = refund.Id;

                                logDAL.InsertLog("WebApi", "WebApi-Stripe Charge:  responce-" + JsonConvert.SerializeObject(refund) + ",request-" + JsonConvert.SerializeObject(options), "", 3,reqModel.Winery_Id);
                            }
                        }
                        else
                        {
                            pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                            logDAL.InsertLog("WebApi", "WebApi-Stripe Charge:  request-" + JsonConvert.SerializeObject(options), "", 3,reqModel.Winery_Id);
                        }
                    }
                }
                else
                {
                    pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                    pr.Detail = "Result from Processing returned Nothing";
                }

                if (pr.Status == TransactionResult.StatusType.Success)
                {
                    ticketDAL.TicketsOrderPaymentInsert(reqModel.Id, pr);
                }
            }
            catch(Exception ex)
            {
                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                pr.Detail = "Result from Processing returned Nothing";

                logDAL.InsertLog("Stripe.ProcessCreditCard", "MemberId:" + reqModel.Winery_Id.ToString() + ",IncoiceId:" + reqModel.Id.ToString() + ",Error:" + ex.Message, "",1,reqModel.Winery_Id);
            }
            
            return pr;
        }
    }
}
