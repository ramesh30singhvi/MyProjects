using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static CPReservationApi.Common.Payments;
using pbt = Braintree;

namespace CPReservationApi.WebApi.Services
{
    public class Braintree
    {
        static private AppSettings _appSettings;

        public Braintree(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
        }
        private static pbt.BraintreeGateway SetupGateway(Configuration pcfg, bool testMode)
        {
            pbt.BraintreeGateway bt = new pbt.BraintreeGateway();
            bt.MerchantId = pcfg.MerchantLogin;
            //"kn6nh9pc96ndvczf"
            bt.PublicKey = pcfg.MerchantPassword;
            //"934whrksrccx69hy"
            bt.PrivateKey = pcfg.UserConfig1;
            //"a77d3dc3b1a3a53f66f68811c838982d"

            if (testMode)
            {
                bt.Environment = pbt.Environment.SANDBOX;
            }
            else
            {
                bt.Environment = pbt.Environment.PRODUCTION;
            }
            return bt;
        }
        public static TransactionResult ProcessCreditCard(int wineryID, int invoiceId, Configuration pcfg, Payments.Transaction1 payment, bool saveCard = false)
        {
            TransactionResult pr = new TransactionResult();
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            try
            {
                int memberId = wineryID;

                //Get Test Mode Value
                bool TestMode = false;
                if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
                {
                    if (_appSettings.PaymentTestMode == true)
                    {
                        TestMode = true;
                    }
                }

                if (memberId == -1)
                {
                    memberId = payment.WineryId;
                    if (TestMode)
                    {
                        if (payment.Transaction == Payments.Transaction1.TransactionType.TicketSale)
                        {
                            memberId = -1;
                        }

                        var conf = eventDAL.GetWineryPaymentConfiguration(memberId);
                        pcfg.MerchantLogin = conf.MerchantLogin;
                        pcfg.MerchantPassword = conf.MerchantPassword;
                        pcfg.UserConfig1 = conf.UserConfig1;
                        pcfg.GatewayMode = (Configuration.Mode)conf.GatewayMode;
                    }
                }

                //If Global Test Mode is False then see if Gateway Test Mode is On
                //if (TestMode == false)
                //{
                //    if (pcfg.GatewayMode == Configuration.Mode.test)
                //    {
                //        TestMode = true;
                //    }
                //}


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
                        if ((!object.ReferenceEquals(bookingCode, string.Empty)))
                        {
                            InvoiceRev = string.Format("CP-{0}-{1}", memberId, bookingCode);
                        }
                        else
                        {
                            InvoiceRev = string.Format("CP-{0}-{1}", memberId, payment.CheckOrRefNumber);
                        }
                        break;
                    case Payments.Transaction1.TransactionType.TicketSale:
                        InvoiceRev = string.Format("CP-Tickets-{0}-{1}", memberId.ToString().PadLeft(4, '0'), invoiceId.ToString().PadLeft(8, '0'));
                        break;
                }



                //Setup Gateway
                pbt.BraintreeGateway bt = SetupGateway(pcfg, TestMode);

                //Result Object
                pbt.Result<pbt.Transaction> gatewayResult = null;

                bool submitSaleAfterVoid = false;

                //Transaction Sale Type
                switch (payment.Type)
                {
                    case Payments.Transaction1.ChargeType.Sale:
                    case Payments.Transaction1.ChargeType.AuthOnly:
                    case Payments.Transaction1.ChargeType.Capture:

                        pbt.TransactionRequest req = new pbt.TransactionRequest();
                        req.OrderId = InvoiceRev;
                        req.Channel = "CellarPass";
                        req.Amount = Math.Round(payment.Amount, 2);

                        //Customer
                        pbt.CustomerRequest cust = new pbt.CustomerRequest();
                        cust.FirstName = payment.User.first_name;
                        cust.LastName = payment.User.last_name;
                        cust.Phone = "";
                        //This can be promlematic for BrainTree, leave blank for now 'payment.User.Phone
                        cust.Email = payment.User.email;
                        req.Customer = cust;

                        //Billing
                        pbt.AddressRequest bill = new pbt.AddressRequest();

                        if (payment.User != null)
                        {
                            bill.FirstName = payment.User.first_name + "";
                            bill.LastName = payment.User.last_name + "";
                            bill.StreetAddress = payment.User.address.address_1 + "";
                            bill.ExtendedAddress = payment.User.address.address_2 + "";
                            bill.Locality = payment.User.address.city + "";
                            bill.Region = payment.User.address.state + "";
                            bill.PostalCode = payment.User.address.zip_code + "";
                            bill.CountryCodeAlpha2 = payment.User.address.country + "";
                            req.BillingAddress = bill;
                        }
                        

                        //Credit Card
                        pbt.TransactionCreditCardRequest card = new pbt.TransactionCreditCardRequest();
                        if (!string.IsNullOrWhiteSpace(payment.Card.Number) && payment.Card.Number.Length > 10)
                        {
                            card.CardholderName = payment.Card.CustName;
                            card.Number = payment.Card.Number;
                            card.ExpirationMonth = payment.Card.ExpMonth;
                            card.ExpirationYear = payment.Card.ExpYear;
                            card.CVV = (payment.Card.CVV == null ? "" : payment.Card.CVV);
                        }
                        else
                        {
                            //card.Token = payment.Card.CardToken;
                            req.PaymentMethodToken = payment.Card.CardToken;
                        }
                        req.CreditCard = card;

                        //Dynamic Descriptions
                        if (payment.Transaction == Payments.Transaction1.TransactionType.TicketSale)
                        {

                            var winery = new WineryModel();

                            winery = eventDAL.GetWineryById(payment.WineryId);
                            pbt.DescriptorRequest desc = new pbt.DescriptorRequest();
                            if ((winery != null))
                            {
                                //Dim wineryName As String = Wineries.GetWineryNameById(memberId)
                                string phone = Utility.ExtractPhone(winery.BusinessPhone.ToString().Trim().TrimStart('1')).ToString().Substring(0, 10);

                                if ((phone != null))
                                {
                                    if (phone.Length != 10)
                                    {
                                        phone = "";
                                    }
                                }

                                desc.Name = Utility.GenerateChargeDescription(winery.DisplayName, winery.DynamicPaymentDesc, invoiceId.ToString(), Configuration.Gateway.Braintree);

                                desc.Phone = phone;
                                //desc.Url = IIf(winery.WebsiteURL Is Nothing, "", Left(winery.WebsiteURL.ToLower.Trim.Replace("http://", "").Replace("www.", ""), 13))
                                req.Descriptor = desc;
                            }
                        }

                        bool doSaleTransaction = true;
                        //If sale then submit for settlement.
                        if (payment.Type == Payments.Transaction1.ChargeType.Sale || payment.Type == Payments.Transaction1.ChargeType.Capture)
                        {
                            if (!string.IsNullOrWhiteSpace(payment.TransactionID))
                            {
                                gatewayResult = bt.Transaction.SubmitForSettlement(payment.TransactionID);
                                doSaleTransaction = false;
                            }
                            else
                            {
                                pbt.TransactionOptionsRequest options = new pbt.TransactionOptionsRequest();
                                options.SubmitForSettlement = true;
                                req.Options = options;
                            }

                        }
                        else
                        {
                            pbt.TransactionOptionsRequest options = new pbt.TransactionOptionsRequest();
                            options.SubmitForSettlement = false;
                            req.Options = options;
                        }

                        if (doSaleTransaction)
                            //Submit for Processing
                            gatewayResult = bt.Transaction.Sale(req);

                        logDAL.InsertLog("WebApi", "ThirdPartyLib.Braintree.ProcessCreditCard.Request: " + req.ToQueryString(), "", 3, wineryID);
                        if (gatewayResult != null)
                        {
                            if (gatewayResult.Message != null)
                            {
                                logDAL.InsertLog("WebApi", "ThirdPartyLib.Braintree.ProcessCreditCard.Result: " + gatewayResult.Message, "", 3, wineryID);
                            }
                            //else if (gatewayResult.Transaction != null)
                            //{
                            //    logDAL.InsertLog("WebApi", "ThirdPartyLib.Braintree.ProcessCreditCard.Result: " + gatewayResult.Transaction.Status.ToString(), "", 3, wineryID);
                            //}
                            else
                            {
                                logDAL.InsertLog("WebApi", "ThirdPartyLib.Braintree.ProcessCreditCard.Result: " + JsonConvert.SerializeObject(gatewayResult), "", 3, wineryID);
                            }
                        }
                        break;
                    case Payments.Transaction1.ChargeType.Credit:
                    case Payments.Transaction1.ChargeType.Void:

                        //Search for existing transaction
                        pbt.Transaction prevTransaction = bt.Transaction.Find(payment.TransactionID);

                        //void or refund?
                        if (prevTransaction.Status.Equals(pbt.TransactionStatus.SETTLED) || prevTransaction.Status.Equals(pbt.TransactionStatus.SETTLING))
                        {
                            //Submit for Refund
                            gatewayResult = bt.Transaction.Refund(payment.TransactionID, Math.Round(payment.Amount, 2));
                            payment.Type = Payments.Transaction1.ChargeType.Credit;
                        }
                        else if (prevTransaction.Status.Equals(pbt.TransactionStatus.SUBMITTED_FOR_SETTLEMENT))
                        {
                            //Submit for Void
                            gatewayResult = bt.Transaction.Void(payment.TransactionID);
                            payment.Type = Payments.Transaction1.ChargeType.Void;
                            if (payment.Amount < payment.OrigAmount)
                            {
                                submitSaleAfterVoid = true;
                            }
                        }
                        else
                        {
                            //Submit for Void
                            gatewayResult = bt.Transaction.Void(payment.TransactionID);
                            payment.Type = Payments.Transaction1.ChargeType.Void;
                            if (payment.Amount < payment.OrigAmount)
                            {
                                submitSaleAfterVoid = true;
                            }
                        }
                        break;
                }

                //Set some properties of the transaction result
                pr.PaymentGateway = Configuration.Gateway.Braintree;
                pr.PayType = Common.Common.PaymentType.CreditCard;
                pr.Amount = payment.Amount;
                pr.Card = payment.Card;
                pr.ProcessedBy = payment.ProcessedBy;
                pr.TransactionType = (Common.Payments.Transaction.ChargeType)payment.Type;

                //Check Result

                if (gatewayResult != null)
                {
                    pbt.Transaction transaction = gatewayResult.Target;

                    if (transaction != null)
                    {
                        //General 
                        pr.ResponseCode = transaction.ProcessorResponseCode;
                        pr.TransactionID = transaction.Id;
                        pr.AvsResponse = transaction.AvsStreetAddressResponseCode;
                    }

                    //Check for success or failure
                    if (gatewayResult.IsSuccess())
                    {
                        //Success
                        pbt.TransactionStatus status = transaction.Status;

                        pr.Status = TransactionResult.StatusType.Success;
                        pr.ApprovalCode = transaction.ProcessorAuthorizationCode;
                        pr.Detail = transaction.ProcessorResponseText;

                        if (submitSaleAfterVoid == true)
                        {
                            pr.DoFollowUpSale = true;
                            //Set amount to full amount since we had to do a void and can't void for less
                            pr.Amount = payment.OrigAmount;
                            //pr.TransactionType = Transaction.ChargeType.Void;
                        }

                        //try
                        //{
                        //    if (!saveCard && !string.IsNullOrWhiteSpace(payment.Card.CardToken))
                        //    {
                        //        bt.CreditCard.Delete(payment.Card.CardToken);
                        //    }
                        //}
                        //catch { }

                    }
                    else if (gatewayResult.Transaction != null)
                    {
                        pr.Status = TransactionResult.StatusType.Failed;
                        pr.TransactionID = gatewayResult.Transaction.Id;
                        if (gatewayResult.Transaction.ProcessorResponseCode != null)
                        {
                            pr.ResponseCode = gatewayResult.Transaction.ProcessorResponseCode;
                        }
                        if (gatewayResult.Transaction.ProcessorResponseText != null)
                        {
                            if (gatewayResult.Message != null)
                            {
                                pr.Detail = gatewayResult.Message;
                            }
                            else if (gatewayResult.Transaction != null)
                            {
                                pr.Detail = gatewayResult.Transaction.ProcessorResponseText;
                            }
                            //pr.Detail = gatewayResult.Transaction.Status.ToString() + ", " + gatewayResult.Transaction.ProcessorResponseText;
                        }
                    }
                    else
                    {
                        //Error
                        pr.Status = TransactionResult.StatusType.Failed;

                        if (gatewayResult.Message != null)
                        {
                            pr.Detail = gatewayResult.Message;
                        }
                    }
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Result from Processing returned Nothing";

                }
            }
            catch (Exception ex)
            {
                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                logDAL.InsertLog("Braintree.ProcessCreditCard", "MemberId:" + wineryID.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "", 1, wineryID);
            }

            return pr;
        }

        private static void ParseCustomerName(string custName, ref string firstName, ref string lastName)
        {
            //char splitChar = ' ';
            if (custName.Contains(","))
            {
                string[] arr = custName.Split(',');
                lastName = arr[0];
                if (arr.Length > 1)
                    firstName = arr[1];
            }
            else
            {
                string[] arr = custName.Split(' ');
                firstName = arr[0];
                if (arr.Length > 1)
                    lastName = arr[1];

            }
        }

        public static List<TokenizedCard> GetTokenizedCardsByCustomer(string custId, Configuration pcfg)
        {
            List<TokenizedCard> resp = new List<TokenizedCard>();
            bool testMode = false;
            if (!string.IsNullOrEmpty(_appSettings.PaymentDebug))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }
            pbt.BraintreeGateway bt = SetupGateway(pcfg, testMode);
            var searchRequest = new pbt.CustomerSearchRequest().Id.Is(custId);
            var results = bt.Customer.Search(searchRequest);
            if (results != null && results.FirstItem != null && results.FirstItem.CreditCards != null && results.FirstItem.CreditCards.Length > 0)
            {
                foreach (var card in results.FirstItem.CreditCards)
                {
                    resp.Add(new TokenizedCard
                    {
                        card_token = card.Token,
                        card_type = card.CardType.ToString(),
                        customer_name = card.CardholderName,
                        cust_id = custId,
                        is_expired = card.IsExpired.HasValue ? card.IsExpired.Value : false,
                        last_four_digits = card.LastFour,
                        first_four_digits = "",
                        card_expiration = card.ExpirationMonth + "/" + card.ExpirationYear
                    });
                }
            }
            return resp;
        }

        public static List<TokenizedCard> GetTokenizedCardsByCustomer(string firstname,string lastname, Configuration pcfg)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            List<string> custIds = new List<string>();
            List<TokenizedCard> resp = new List<TokenizedCard>();
            bool testMode = false;
            if (!string.IsNullOrEmpty(_appSettings.PaymentDebug))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }
            pbt.BraintreeGateway bt = SetupGateway(pcfg, testMode);
            var searchRequest = new pbt.CustomerSearchRequest().FirstName.Is(firstname).LastName.Is(lastname).Company.Is("CellarPass");

            try
            {
                var results = bt.Customer.Search(searchRequest);
                if (results != null && results.Ids != null && results.Ids.Count > 0 && results.FirstItem != null)
                {
                    custIds = results.Ids;
                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "BrainTree::Customer.Search:  " + ex.Message.ToString(), "", 1, 0);
            }

            if (custIds.Count > 0)
            {
                foreach (var custId in custIds)
                {
                    //check if the card already exists
                    searchRequest = new pbt.CustomerSearchRequest().Id.Is(custId);

                    var ccSearchResults = bt.Customer.Search(searchRequest);
                    if (ccSearchResults != null && ccSearchResults.Ids != null && ccSearchResults.Ids.Count > 0 && ccSearchResults.FirstItem != null && ccSearchResults.FirstItem.CreditCards != null && ccSearchResults.FirstItem.CreditCards.Length > 0)
                    {
                        foreach (var card in ccSearchResults.FirstItem.CreditCards)
                        {
                            resp.Add(new TokenizedCard
                            {
                                card_token = card.Token,
                                card_type = card.CardType.ToString(),
                                customer_name = firstname + " " + lastname,
                                cust_id = custId,
                                is_expired = card.IsExpired.HasValue ? card.IsExpired.Value : false,
                                last_four_digits = card.LastFour,
                                first_four_digits = "",
                                card_expiration = card.ExpirationMonth + "/" + card.ExpirationYear,
                                card_exp_month = card.ExpirationMonth,
                                card_exp_year = card.ExpirationYear
                            });
                        }
                    }
                }
            }

            return resp;
        }

        public static TokenizedCard TokenziedCard(TokenizedCardRequest request, Configuration pcfg)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            TokenizedCard resp = null;
            bool testMode = false;
            if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }
            pbt.BraintreeGateway bt = SetupGateway(pcfg, testMode);
            try
            {
                string firstName = "";
                string lastName = "";
                List<string> custIds = new List<string>();
                //string custId = "";
                string Id = "";
                ParseCustomerName(request.cust_name, ref firstName, ref lastName);

                //check if customer exists
                var searchRequest = new pbt.CustomerSearchRequest().FirstName.Is(firstName).LastName.Is(lastName).Company.Is("CellarPass");

                try
                {
                    var results = bt.Customer.Search(searchRequest);
                    if (results != null && results.Ids != null && results.Ids.Count > 0 && results.FirstItem != null)
                    {
                        custIds = results.Ids;
                    }
                }
                catch (Exception ex)
                {
                    logDAL.InsertLog("WebApi", "BrainTree::Customer.Search:  " + ex.Message.ToString(), "", 1, request.member_id);

                    if (ex.Message.ToString().IndexOf("AuthenticationException") > -1)
                    {
                        resp = new TokenizedCard
                        {
                            ErrorMessage = "Authentication Exception"
                        };
                    }
                    else
                    {
                        resp = new TokenizedCard
                        {
                            ErrorMessage = ex.Message.ToString()
                        };
                    }
                }

                if (custIds.Count == 0)
                {
                    //create customer
                    var custResult = bt.Customer.Create(new pbt.CustomerRequest
                    {
                        Company = "CellarPass",
                        FirstName = firstName,
                        LastName = lastName
                    });
                    if (custResult.IsSuccess())
                    {
                        custIds.Add(custResult.Target.Id);
                    }
                    else
                    {
                        if (custResult != null)
                            logDAL.InsertLog("WebApi", "BrainTree::Customer.Create:  " + JsonConvert.SerializeObject(custResult) + ",Request:" + JsonConvert.SerializeObject(request), "", 1, request.member_id);
                        else
                            logDAL.InsertLog("WebApi", "BrainTree::Customer.Create: Request:" + JsonConvert.SerializeObject(request), "", 1, request.member_id);
                    }
                }


                if (custIds.Count > 0)
                {
                    foreach (var custId in custIds)
                    {
                        //check if the card already exists
                        bool createCreditCard = false;
                        if (!string.IsNullOrEmpty(request.number))
                            searchRequest = new pbt.CustomerSearchRequest().Id.Is(custId).CreditCardNumber.StartsWith(request.number.Substring(0, 6));
                        else
                            searchRequest = new pbt.CustomerSearchRequest().Id.Is(custId).CreditCardNumber.EndsWith(request.card_last_four_digits + "");

                        var ccSearchResults = bt.Customer.Search(searchRequest);
                        if (ccSearchResults != null && ccSearchResults.Ids != null && ccSearchResults.Ids.Count > 0 && ccSearchResults.FirstItem != null && ccSearchResults.FirstItem.CreditCards != null && ccSearchResults.FirstItem.CreditCards.Length > 0)
                        {
                            var cc = ccSearchResults.FirstItem.CreditCards[0];
                            bool isCCGood = true;
                            if (!string.IsNullOrEmpty(request.number))
                            {
                                //if entire CC number is passed then check if the number returned in result is same
                                string reqLast4Digits = request.number.Trim().Substring(request.number.Trim().Length - 4);
                                if (reqLast4Digits != cc.LastFour)
                                {
                                    isCCGood = false;
                                }
                                //&& request.number.Replace(" ", "") != cc.LastFour
                            }
                            else if (!string.IsNullOrEmpty(request.card_last_four_digits) && request.card_last_four_digits != cc.LastFour)
                            {
                                isCCGood = false;
                            }
                            if (isCCGood)
                            {
                                string expiryDate = request.exp_month.PadLeft(2, '0') + "/" + request.exp_year;
                                try
                                {
                                    if (!string.IsNullOrWhiteSpace(cc.ExpirationDate) && !cc.ExpirationDate.Equals(expiryDate))
                                    {
                                        logDAL.InsertLog("WebApi", "BrainTree::CreditCard.Update:  CC Token: " + cc.Token + ", original expiry date:" + cc.ExpirationDate + ", new expiry date:" + expiryDate, "", 3, request.member_id);
                                        //update card
                                        pbt.CreditCardAddressRequest billingAddress = new pbt.CreditCardAddressRequest();

                                        if (request.user_info != null)
                                        {
                                            billingAddress.FirstName = request.user_info.first_name + "";
                                            billingAddress.LastName = request.user_info.last_name + "";
                                            billingAddress.PhoneNumber = request.user_info.phone_number + "";
                                            billingAddress.Locality = request.user_info.address.city + "";
                                            billingAddress.Region = request.user_info.address.state + "";
                                            billingAddress.PostalCode = request.user_info.address.zip_code + "";
                                            billingAddress.CountryCodeAlpha2 = request.user_info.address.country + "";
                                            billingAddress.StreetAddress = request.user_info.address.address_1 + "";
                                        }

                                        var result = bt.CreditCard.Update(cc.Token, new pbt.CreditCardRequest
                                        {
                                            CustomerId = custId,
                                            Number = request.number,
                                            CVV = request.cvv2,
                                            ExpirationDate = request.exp_month.PadLeft(2, '0') + "/" + request.exp_year,
                                            BillingAddress = billingAddress
                                        });

                                        if (result != null && result.Target != null)
                                        {
                                            resp = new TokenizedCard
                                            {
                                                card_token = result.Target.Token,
                                                customer_name = result.Target.CardholderName,
                                                last_four_digits = result.Target.LastFour,
                                                first_four_digits = Common.Common.Left(request.number, 4),
                                                is_expired = result.Target.IsExpired.HasValue ? result.Target.IsExpired.Value : false,
                                                card_type = Payments.GetCardType(request.number),
                                                cust_id = custId
                                            };
                                        }
                                        else
                                        {
                                            if (result != null)
                                            {
                                                resp = new TokenizedCard
                                                {
                                                    ErrorMessage = result.Message
                                                };
                                            }

                                            if (result != null)
                                                logDAL.InsertLog("WebApi", "BrainTree::CreditCard.Update:  " + JsonConvert.SerializeObject(result) + ",Request:" + JsonConvert.SerializeObject(request), "", 1, request.member_id);
                                            else
                                                logDAL.InsertLog("WebApi", "BrainTree::CreditCard.Update: Request:" + JsonConvert.SerializeObject(request), "", 1, request.member_id);
                                        }
                                        return resp;
                                    }

                                }
                                catch (Exception ex){
                                    logDAL.InsertLog("WebApi", "BrainTree::CreditCard.Update:  Error updating card info. CC Token:" + cc.Token + ", error:" + ex.Message, "", 1, request.member_id);
                                }
                                resp = new TokenizedCard
                                {
                                    card_token = cc.Token,
                                    customer_name = cc.CardholderName,
                                    last_four_digits = cc.LastFour,
                                    first_four_digits = Common.Common.Left(request.number, 4),
                                    is_expired = cc.IsExpired.HasValue ? cc.IsExpired.Value : false,
                                    card_type = Payments.GetCardType(request.number),
                                    cust_id = custId
                                };

                                break;
                            }
                            else
                            {
                                createCreditCard = true;
                            }
                        }
                        else if (!string.IsNullOrEmpty(request.number))
                        {
                            createCreditCard = true;
                        }

                        try
                        {
                            if (resp != null && resp.is_expired && !string.IsNullOrEmpty(request.number))
                            {
                                createCreditCard = true;
                            }
                        }
                        catch { }


                        if (createCreditCard)
                        {
                            pbt.CreditCardAddressRequest billingAddress = new pbt.CreditCardAddressRequest();

                            if (request.user_info != null)
                            {
                                billingAddress.FirstName = request.user_info.first_name + "";
                                billingAddress.LastName = request.user_info.last_name + "";
                                billingAddress.PhoneNumber = request.user_info.phone_number + "";
                                billingAddress.Locality = request.user_info.address.city + "";
                                billingAddress.Region = request.user_info.address.state + "";
                                billingAddress.PostalCode = request.user_info.address.zip_code + "";
                                billingAddress.CountryCodeAlpha2 = request.user_info.address.country + "";
                                billingAddress.StreetAddress = request.user_info.address.address_1 + "";
                            }

                            var result = bt.CreditCard.Create(new pbt.CreditCardRequest
                            {
                                CustomerId = custId,
                                Number = request.number,
                                CVV = request.cvv2,
                                ExpirationDate = request.exp_month.PadLeft(2, '0') + "/" + request.exp_year,
                                BillingAddress = billingAddress
                            });

                            if (result != null && result.Target != null)
                            {
                                resp = new TokenizedCard
                                {
                                    card_token = result.Target.Token,
                                    customer_name = result.Target.CardholderName,
                                    last_four_digits = result.Target.LastFour,
                                    first_four_digits = Common.Common.Left(request.number, 4),
                                    is_expired = result.Target.IsExpired.HasValue ? result.Target.IsExpired.Value : false,
                                    card_type = Payments.GetCardType(request.number),
                                    cust_id = custId
                                };

                                break;
                            }
                            else
                            {
                                if (result != null)
                                {
                                    resp = new TokenizedCard
                                    {
                                        ErrorMessage = result.Message
                                    };
                                }
                                
                                if (result != null)
                                    logDAL.InsertLog("WebApi", "BrainTree::CreditCard.Create:  " + JsonConvert.SerializeObject(result) + ",Request:" + JsonConvert.SerializeObject(request), "", 3, request.member_id);
                                else
                                    logDAL.InsertLog("WebApi", "BrainTree::CreditCard.Create: Request:" + JsonConvert.SerializeObject(request), "", 3, request.member_id);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "BrainTree::Tokenized Card:  Request:" + JsonConvert.SerializeObject(request) + ex.Message.ToString(), "", 1, request.member_id);

                if (ex.Message.ToString().IndexOf("reference not set to an instance") == -1)
                {
                    if (ex.Message.ToString().IndexOf("AuthenticationException") > -1)
                    {
                        resp = new TokenizedCard
                        {
                            ErrorMessage = "Authentication Exception"
                        };
                    }
                    else
                    {
                        resp = new TokenizedCard
                        {
                            ErrorMessage = ex.Message.ToString()
                        };
                    }
                }
            }
            return resp;
        }
    }
}
