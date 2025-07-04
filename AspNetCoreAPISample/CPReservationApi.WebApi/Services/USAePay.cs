using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using static CPReservationApi.Common.Payments;
using CPReservationApi.DAL;

namespace CPReservationApi.WebApi.Services
{
    public class USAePay
    {
        static private AppSettings _appSettings;
        public USAePay(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
        }

        const string LiveUrl = "https://usaepay.com";
        const string TestUrl = "https://sandbox.usaepay.com";

        public class SaleAmountDetail
        {
            public string tax { get; set; }
            public string tip { get; set; }
        }

        public class SaleRootObjectRequest
        {
            public string command { get; set; }
            public string amount { get; set; }
            public SaleAmountDetail amount_detail { get; set; }
            public creditcardModel creditcard { get; set; }
            public string invoice { get; set; }
        }

        public class RefundRootObjectRequest
        {
            public string command { get; set; }
            public string amount { get; set; }
            public string refnum { get; set; }
        }

        public class SaleCreditcard
        {
            public string number { get; set; }
            public string category_code { get; set; }
        }

        public class Avs
        {
            public string result_code { get; set; }
            public string result { get; set; }
        }

        public class Cvc
        {
            public string result_code { get; set; }
            public string result { get; set; }
        }

        public class Batch
        {
            public string type { get; set; }
            public string key { get; set; }
            public string sequence { get; set; }
        }

        public class SaleRootObjectResponce
        {
            public string type { get; set; }
            public string key { get; set; }
            public string refnum { get; set; }
            public string is_duplicate { get; set; }
            public string result_code { get; set; }
            public string result { get; set; }
            public string authcode { get; set; }
            public SaleCreditcard creditcard { get; set; }
            public Avs avs { get; set; }
            public Cvc cvc { get; set; }
            public Batch batch { get; set; }
            public string auth_amount { get; set; }
        }

        public class RefundRootObjectResponce
        {
            public string type { get; set; }
            public string key { get; set; }
            public string refnum { get; set; }
            public string is_duplicate { get; set; }
            public string result_code { get; set; }
            public string result { get; set; }
            public string authcode { get; set; }
            public Avs avs { get; set; }
            public Cvc cvc { get; set; }
        }

        public class VoidRootObjectResponce
        {
            public string type { get; set; }
            public string key { get; set; }
            public string refnum { get; set; }
            public string is_duplicate { get; set; }
            public string result_code { get; set; }
            public string result { get; set; }
            public string authcode { get; set; }
        }

        public class AmountDetail
        {
            public string tax { get; set; }
            public string subtotal { get; set; }
        }

        public class BillingAddress
        {
            public string firstname { get; set; }
            public string lastname { get; set; }
            public string street { get; set; }
            public string street2 { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string postalcode { get; set; }
            public string country { get; set; }
            public string phone { get; set; }
            public string email { get; set; }
        }

        public class SaleRootObject
        {
            public string command { get; set; }
            public string payment_key { get; set; }
            public string amount { get; set; }
            public AmountDetail amount_detail { get; set; }
            public BillingAddress billing_address { get; set; }
            public string custemailaddr { get; set; }
        }

        public class ccsaveModel
        {
            public string command { get; set; }
            public creditcardModel creditcard { get; set; }
        }

        public class creditcardModel
        {
            public string cardholder { get; set; }
            public string number { get; set; }
            public string expiration { get; set; }
            public string cvc { get; set; }
            public string avs_street { get; set; }
            public string avs_zip { get; set; }
        }

        public class Creditcard
        {
            public string number { get; set; }
            public string cardholder { get; set; }
        }

        public class Savedcard
        {
            public string type { get; set; }
            public string key { get; set; }
            public string cardnumber { get; set; }
        }

        public class RootObject
        {
            public string type { get; set; }
            public string key { get; set; }
            public string refnum { get; set; }
            public string is_duplicate { get; set; }
            public string result_code { get; set; }
            public string result { get; set; }
            public string authcode { get; set; }
            public Creditcard creditcard { get; set; }
            public Savedcard savedcard { get; set; }
        }

        private static string GenerateHeaderValue(string SourceKey)
        {
            string HeaderValue = string.Empty;
            string pin = "";
            string Seed= Guid.NewGuid().ToString();
            string prehashvalue = string.Concat(SourceKey, Seed, pin);
            string HashValue = GenerateHash(prehashvalue);

            var byteArray = Encoding.ASCII.GetBytes(prehashvalue + ":" + HashValue);
            HeaderValue = Convert.ToBase64String(byteArray);

            return HeaderValue;
        }

        private static string GenerateHash(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            //MD5 md5Hasher = MD5.Create();
            SHA256 sha256Hash = SHA256.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = sha256Hash.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static TokenizedCard TokenziedCard(TokenizedCardRequest request, Configuration pcfg)
        {
            TokenizedCard resp = null;
            bool testMode = false;
            if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }

            //USAePayAPI.USAePay usaepay = new USAePayAPI.USAePay();

            ccsaveModel ccModel = new ccsaveModel();
            creditcardModel creditcardModel = new creditcardModel();

            ccModel.command = "cc:save";

            creditcardModel.number = request.number;
            creditcardModel.cardholder = request.cust_name;
            creditcardModel.expiration = request.exp_month + Common.Common.Right(request.exp_year, 2);
            creditcardModel.avs_zip = "";
            creditcardModel.avs_street = "";
            creditcardModel.cvc = request.cvv2;
            ccModel.creditcard = creditcardModel;

            string payLoadContent = JsonConvert.SerializeObject(ccModel);

            string postURL = LiveUrl;

            //If test mode, use test url
            if (testMode)
            {
                postURL = TestUrl;
            }

            HttpClient httpClient = GetUSAePayHttpClient(postURL,pcfg.MerchantLogin, pcfg.MerchantPassword);
            var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");
            using (var response = httpClient.PostAsync(string.Format("{0}/api/v2/transactions",postURL), stringContent).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                RootObject ccDetails = JsonConvert.DeserializeObject<RootObject>(varResponse);

                if (ccDetails != null && ccDetails.result != null && ccDetails.result != "Error")
                {
                    resp = new TokenizedCard
                    {
                        card_token = ccDetails.savedcard.key,
                        customer_name = ccDetails.creditcard.cardholder,
                        last_four_digits = Common.Common.Right(ccDetails.creditcard.number, 4),
                        first_four_digits = Common.Common.Left(ccDetails.creditcard.number, 4),
                        is_expired = false,
                        card_type = Payments.GetCardType(request.number)
                    };
                }
                else
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi-USAePay::TokenziedCard", "Member Id:" + request.member_id.ToString() + ", Card Number:" + request.number + ", Error Response:" + varResponse,"",1,request.member_id);

                    try
                    {
                        USAePayErrorRoot usaePayErrorRoot = JsonConvert.DeserializeObject<USAePayErrorRoot>(varResponse);

                        resp = new TokenizedCard
                        {
                            ErrorMessage = usaePayErrorRoot.error,
                            ErrorCode = usaePayErrorRoot.errorcode.ToString()
                        };
                    }
                    catch { }
                    
                }
            }
            httpClient.Dispose();

            return resp;
        }

        public static HttpClient GetUSAePayHttpClient(string postURL,string apikey,string pin)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(postURL);
            httpClient.DefaultRequestHeaders.Accept.Clear();

            string seed = Guid.NewGuid().ToString();
            string apipin = pin;
            string prehash = apikey + seed + apipin;
            var apihash = "s2/" + seed + '/' + GenerateHash(prehash);
            var byteArray = Encoding.ASCII.GetBytes(apikey + ':' + apihash);
            var authKey = Convert.ToBase64String(byteArray);

            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "X1Y4N1F0YjUxM0NkM3ZhYk03UkMwVGJ0SldlU284cDc6czIvYWJjZGVmZ2hpamtsbW5vcC9iNzRjMmZhOTFmYjBhMDk3NTVlMzc3ZWU4ZTIwYWE4NmQyYjkyYzNkMmYyNzcyODBkYjU5NWY2MzZiYjE5MGU2");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authKey);
            return httpClient;
        }

        public static TransactionResult ProcessCreditCard(int wineryID, int invoiceId, Configuration pcfg, Payments.Transaction1 payment)
        {
            TransactionResult pr = new TransactionResult();

            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                int memberId = wineryID;

                if (memberId == -1)
                {
                    memberId = payment.WineryId;
                }

                //Get Test Mode Value
                bool testMode = false;
                if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
                {
                    if (_appSettings.PaymentTestMode == true)
                    {
                        testMode = true;
                    }
                }

                //If Global Test Mode is False then see if Gateway Test Mode is On
                if (testMode == false)
                {
                    if (pcfg.GatewayMode == Configuration.Mode.test)
                    {
                        testMode = true;
                    }
                }

                bool ValidRefund = false;
                if (payment.Type == Payments.Transaction1.ChargeType.Credit || payment.Type == Payments.Transaction1.ChargeType.Void)
                {
                    decimal PreviousDeposit = 0;
                    if (payment.IsPreAuthCredit)
                    {
                        var authPayment = eventDAL.GetReservationPreAuths(invoiceId);
                        if (authPayment != null && authPayment.Count > 0)
                        {
                            PreviousDeposit = authPayment[0].amount;
                        }

                    }
                    else
                    {
                        PreviousDeposit = eventDAL.GetPreviousDepositByReservationID(invoiceId);
                    }

                    if (PreviousDeposit >= payment.Amount)
                        ValidRefund = true;
                }

                if (payment.Type == Payments.Transaction1.ChargeType.Sale)
                {
                    pr = Sale(wineryID, invoiceId, pcfg, payment);
                }
                else if (payment.Type == Payments.Transaction1.ChargeType.AuthOnly)
                {
                    pr = Sale(wineryID, invoiceId, pcfg, payment, true);
                }
                else if (payment.Type == Payments.Transaction1.ChargeType.Capture)
                {
                    pr = Capture(wineryID, invoiceId, pcfg, payment);
                }
                else if (payment.Type == Payments.Transaction1.ChargeType.Credit && ValidRefund)
                {
                    pr = Refund(wineryID, invoiceId, pcfg, payment);
                }
                else if (payment.Type == Payments.Transaction1.ChargeType.Void && ValidRefund)
                {
                    pr = Void(wineryID, invoiceId, pcfg, payment);
                }
            }
            catch (Exception ex)
            {
                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("USAePay.ProcessCreditCard", "MemberId:" + wineryID.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "",1,wineryID);
            }
            
            return pr;
        }

        public static TransactionResult Sale(int wineryID, int invoiceId, Configuration pcfg, Payments.Transaction1 payment, bool isAuth = false)
        {
            TransactionResult pr = new TransactionResult();
            bool testMode = false;
            if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }

            SaleRootObjectRequest saleRootObject = new SaleRootObjectRequest();
            SaleAmountDetail amountDetail = new SaleAmountDetail();
            creditcardModel creditcardModel = new creditcardModel();

            string InvoiceRev = "";
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            string bookingCode = eventDAL.GetBookingCodeByReservationID(invoiceId);
            if ((!object.ReferenceEquals(bookingCode, string.Empty)))
            {
                InvoiceRev = string.Format("CP-{0}-{1}", wineryID, bookingCode);
            }
            else
            {
                InvoiceRev = string.Format("CP-{0}-{1}", wineryID, payment.CheckOrRefNumber);
            }

            saleRootObject.command = "cc:sale";
            if(isAuth)
                saleRootObject.command = "cc:authonly"; 
            saleRootObject.invoice = InvoiceRev;
            saleRootObject.amount = Math.Round(payment.Amount, 2).ToString();

            amountDetail.tip = "0";
            amountDetail.tax = "0";

            saleRootObject.amount_detail = amountDetail;

            if (!string.IsNullOrEmpty(payment.Card.CardToken))
            {
                creditcardModel.number = payment.Card.CardToken;
            }
            else
            {
                creditcardModel.number = payment.Card.Number;
            }
            
            creditcardModel.cardholder = payment.Card.CustName;
            creditcardModel.expiration = payment.Card.ExpMonth + Common.Common.Right(payment.Card.ExpYear, 2);
            creditcardModel.avs_zip = payment.User.address.zip_code;
            creditcardModel.avs_street = payment.User.address.address_1;
            creditcardModel.cvc = payment.Card.CVV;

            saleRootObject.creditcard = creditcardModel;

            string payLoadContent = JsonConvert.SerializeObject(saleRootObject);

            string postURL = LiveUrl;

            //If test mode, use test url
            if (testMode)
            {
                postURL = TestUrl;
            }

            pr.PaymentGateway = Configuration.Gateway.USAePay;
            pr.PayType = Common.Common.PaymentType.CreditCard;
            pr.Amount = payment.Amount;
            pr.Card = payment.Card;
            pr.ProcessedBy = payment.ProcessedBy;
            pr.TransactionType = (Common.Payments.Transaction.ChargeType)payment.Type;

            HttpClient httpClient = GetUSAePayHttpClient(postURL, pcfg.MerchantLogin, pcfg.MerchantPassword);
            var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");
            using (var response = httpClient.PostAsync(string.Format("{0}/api/v2/transactions", postURL), stringContent).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                try
                {
                    SaleRootObjectResponce saleRootObjectResponce = JsonConvert.DeserializeObject<SaleRootObjectResponce>(varResponse);

                    if (saleRootObjectResponce != null && saleRootObjectResponce.result != null && saleRootObjectResponce.result.ToLower() != "error" && saleRootObjectResponce.result.ToLower() != "declined")
                    {
                        pr.ResponseCode = saleRootObjectResponce.key;
                        pr.TransactionID = saleRootObjectResponce.refnum;
                        pr.Status = TransactionResult.StatusType.Success;
                        pr.ApprovalCode = saleRootObjectResponce.authcode;
                        pr.Detail = saleRootObjectResponce.result;
                    }
                    else
                    {
                        pr.Status = TransactionResult.StatusType.Failed;
                        pr.Detail = saleRootObjectResponce.result;

                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                        logDAL.InsertLog("WebApi", "USAePay.ProcessCreditCard: Request:" + payLoadContent, "", 3,wineryID);
                        logDAL.InsertLog("WebApi", "USAePay.ProcessCreditCard: Response:" + varResponse, "", 3,wineryID);
                    }
                }
                catch
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Null Response from usaepay";

                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi", "USAePay.ProcessCreditCard: Request:" + payLoadContent + "", "", 3,wineryID);
                    logDAL.InsertLog("WebApi", "USAePay.ProcessCreditCard: Response:" + varResponse, "", 3,wineryID);
                }

            }
            httpClient.Dispose();

            return pr;
        }


        public static TransactionResult Capture(int wineryID, int invoiceId, Configuration pcfg, Payments.Transaction1 payment)
        {
            TransactionResult pr = new TransactionResult();
            bool testMode = false;
            if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }


            string payLoadContent = string.Format(@"{
                                                    ""command"": ""cc:capture"",
                                                    ""trankey"": ""{0}""
                                                }", payment.TransactionID);

            string postURL = LiveUrl;

            //If test mode, use test url
            if (testMode)
            {
                postURL = TestUrl;
            }

            pr.PaymentGateway = Configuration.Gateway.USAePay;
            pr.PayType = Common.Common.PaymentType.CreditCard;
            pr.Amount = payment.Amount;
            pr.Card = payment.Card;
            pr.ProcessedBy = payment.ProcessedBy;
            pr.TransactionType = (Common.Payments.Transaction.ChargeType)payment.Type;

            HttpClient httpClient = GetUSAePayHttpClient(postURL, pcfg.MerchantLogin, pcfg.MerchantPassword);
            var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");
            using (var response = httpClient.PostAsync(string.Format("{0}/api/v2/transactions", postURL), stringContent).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                try
                {
                    SaleRootObjectResponce saleRootObjectResponce = JsonConvert.DeserializeObject<SaleRootObjectResponce>(varResponse);

                    if (saleRootObjectResponce != null && saleRootObjectResponce.result != null && saleRootObjectResponce.result.ToLower() != "error" && saleRootObjectResponce.result.ToLower() != "declined")
                    {
                        pr.ResponseCode = saleRootObjectResponce.key;
                        pr.TransactionID = saleRootObjectResponce.refnum;
                        pr.Status = TransactionResult.StatusType.Success;
                        pr.ApprovalCode = saleRootObjectResponce.authcode;
                        pr.Detail = saleRootObjectResponce.result;
                    }
                    else
                    {
                        pr.Status = TransactionResult.StatusType.Failed;
                        pr.Detail = saleRootObjectResponce.result;

                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                        logDAL.InsertLog("WebApi", "USAePay.Capture: Request:" + payLoadContent, "", 3, wineryID);
                        logDAL.InsertLog("WebApi", "USAePay.Capture: Response:" + varResponse, "", 3, wineryID);
                    }
                }
                catch
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Null Response from usaepay";

                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi", "USAePay.ProcessCreditCard: Request:" + payLoadContent + "", "", 3, wineryID);
                    logDAL.InsertLog("WebApi", "USAePay.ProcessCreditCard: Response:" + varResponse, "", 3, wineryID);
                }

            }
            httpClient.Dispose();

            return pr;
        }

        public static TransactionResult Void(int wineryID, int invoiceId, Configuration pcfg, Payments.Transaction1 payment)
        {
            TransactionResult pr = new TransactionResult();
            bool testMode = false;
            if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }

            RefundRootObjectRequest refundRootObjectRequest = new RefundRootObjectRequest();

            refundRootObjectRequest.command = "cc:void";
            refundRootObjectRequest.refnum = payment.TransactionID;
            refundRootObjectRequest.amount = Math.Round(payment.Amount,2).ToString();

            string payLoadContent = JsonConvert.SerializeObject(refundRootObjectRequest);

            string postURL = LiveUrl;

            //If test mode, use test url
            if (testMode)
            {
                postURL = TestUrl;
            }

            pr.PaymentGateway = Configuration.Gateway.USAePay;
            pr.PayType = Common.Common.PaymentType.CreditCard;
            pr.Amount = payment.Amount;
            pr.Card = payment.Card;
            pr.ProcessedBy = payment.ProcessedBy;
            pr.TransactionType = (Common.Payments.Transaction.ChargeType)payment.Type;

            HttpClient httpClient = GetUSAePayHttpClient(postURL, pcfg.MerchantLogin, pcfg.MerchantPassword);
            var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");
            using (var response = httpClient.PostAsync(string.Format("{0}/api/v2/transactions", postURL), stringContent).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                try
                {
                    VoidRootObjectResponce refundRootObjectResponce = JsonConvert.DeserializeObject<VoidRootObjectResponce>(varResponse);

                    if (refundRootObjectResponce != null && refundRootObjectResponce.result != null && refundRootObjectResponce.result != "Error")
                    {
                        pr.ResponseCode = refundRootObjectResponce.key;
                        pr.TransactionID = refundRootObjectResponce.refnum;
                        pr.Status = TransactionResult.StatusType.Success;
                        pr.ApprovalCode = refundRootObjectResponce.authcode;
                        pr.Detail = refundRootObjectResponce.result;
                    }
                    else
                    {
                        pr.Status = TransactionResult.StatusType.Failed;
                        pr.Detail = refundRootObjectResponce.result;
                    }
                }
                catch
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Null Response from usaepay";
                }

            }
            httpClient.Dispose();

            return pr;
        }

        public static TransactionResult Refund(int wineryID, int invoiceId, Configuration pcfg, Payments.Transaction1 payment)
        {
            TransactionResult pr = new TransactionResult();
            bool testMode = false;
            if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }

            RefundRootObjectRequest refundRootObjectRequest = new RefundRootObjectRequest();

            refundRootObjectRequest.command = "cc:refund";
            refundRootObjectRequest.refnum = payment.TransactionID;
            refundRootObjectRequest.amount = Math.Round(payment.Amount, 2).ToString();

            string payLoadContent = JsonConvert.SerializeObject(refundRootObjectRequest);

            string postURL = LiveUrl;

            //If test mode, use test url
            if (testMode)
            {
                postURL = TestUrl;
            }

            pr.PaymentGateway = Configuration.Gateway.USAePay;
            pr.PayType = Common.Common.PaymentType.CreditCard;
            pr.Amount = payment.Amount;
            pr.Card = payment.Card;
            pr.ProcessedBy = payment.ProcessedBy;
            pr.TransactionType = (Common.Payments.Transaction.ChargeType)payment.Type;

            HttpClient httpClient = GetUSAePayHttpClient(postURL, pcfg.MerchantLogin, pcfg.MerchantPassword);
            var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");
            using (var response = httpClient.PostAsync(string.Format("{0}/api/v2/transactions", postURL), stringContent).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                try
                {
                    RefundRootObjectResponce refundRootObjectResponce = JsonConvert.DeserializeObject<RefundRootObjectResponce>(varResponse);

                    if (refundRootObjectResponce != null && refundRootObjectResponce.result != null && refundRootObjectResponce.result != "Error")
                    {
                        pr.ResponseCode = refundRootObjectResponce.key;
                        pr.TransactionID = refundRootObjectResponce.refnum;
                        pr.Status = TransactionResult.StatusType.Success;
                        pr.ApprovalCode = refundRootObjectResponce.authcode;
                        pr.Detail = refundRootObjectResponce.result;
                    }
                    else
                    {
                        pr.Status = TransactionResult.StatusType.Failed;
                        pr.Detail = refundRootObjectResponce.result;
                    }
                }
                catch
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Null Response from usaepay";
                }

            }
            httpClient.Dispose();

            return pr;
        }
    }
}
