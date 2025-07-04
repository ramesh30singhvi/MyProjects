using CPReservationApi.DAL;
using CPReservationApi.WebApi.ViewModels;
using CyberSource.Api;
using CyberSource.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using static CPReservationApi.Common.Payments;
using System.Linq;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json.Linq;
using System.IO;

namespace CPReservationApi.WebApi.Services
{

    public class Shift4PaymentService
    {
        private static string _baseURL = "https://utgapi.shift4test.com/api/rest/v1/";
        private static String ENDPOINT_ACCESSTOKEN = "credentials/accesstoken";
        private static String ENDPOINT_TOKEN = "tokens/add";
        private static String ENDPOINT_SALE = "transactions/sale";
        private static String ENDPOINT_REFUND = "transactions/refund";
        private static String ENDPOINT_VOID = "transactions/invoice";
        private static String ENDPOINT_AUTH = "transactions/authorization";
        private static String ENDPOINT_CAPTURE = "transactions/capture";


        static private ViewModels.AppSettings _appSettings;

        public Shift4PaymentService(IOptions<ViewModels.AppSettings> settings)
        {
            _appSettings = settings.Value;
        }
        public Shift4PaymentService() { }

        public static string CheckAndGenerateAccessToken(int member_id, Configuration pcfg)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            Shift4PaymentService shift4PaymentService = new Shift4PaymentService();
            Shift4Model Shift4Model = new Shift4Model();
            Shift4Model.dateTime = DateTime.Now.ToString("o");
            var ob = new Shift4Credential();
            ob.authToken = pcfg.MerchantPassword.ToString();
            ob.clientGuid = pcfg.MerchantLogin.ToString();
            Shift4Model.credential = ob;
            logDAL.InsertLog("shift4::GenerateAccesstoken", "Request send:" + JsonConvert.SerializeObject(Shift4Model), "", 3,member_id);
            JObject response = shift4PaymentService.AccessTokenExchange(Shift4Model);
            logDAL.InsertLog("shift4::GenerateAccesstoken", "Response Rcvd:" + response.ToString(), "", 3,member_id);
            AccessResult AccessResult = JsonConvert.DeserializeObject<AccessResult>(response.ToString());
            var Accesstoken = AccessResult.result[0].credential.accessToken;

            //Store access token in DB
            SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
            settingsDAL.UpdateUserConfig1(pcfg.PaymentGateway, Accesstoken.ToString(), member_id);

            return Accesstoken;
        }

        public static TokenizedCard TokenziedCard(TokenizedCardRequest request, Configuration pcfg)
        {
            TokenizedCard resp = null;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            bool testMode = false;
            if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }
            try
            {
                if (!testMode)
                    _baseURL = " https://utg.shift4api.net/api/rest/v1/";
                else
                    _baseURL = "https://utgapi.shift4test.com/api/rest/v1/";

                Shift4PaymentService shift4PaymentService = new Shift4PaymentService();
                if (pcfg.UserConfig1 == "")
                {
                    pcfg.UserConfig1 = CheckAndGenerateAccessToken(request.member_id, pcfg);
                }
                TokenAddReq authTransaction = new TokenAddReq();
                authTransaction.dateTime = DateTime.Now.ToString("o");
                var carddata = new CardAuth();
                carddata.number = request.number.ToString();
                //carddata.expirationDate = Int32.Parse(request.exp_month + request.exp_year.Substring(2, 2));
                carddata.expirationDate = request.exp_month.PadLeft(2, '0') + request.exp_year.Substring(2, 2);
                if (!string.IsNullOrWhiteSpace(request.cvv2))
                {
                    carddata.securityCode = new SecurityCode
                    {
                        value = request.cvv2
                    };
                }
                authTransaction.card = carddata;
                if(testMode)
                    logDAL.InsertLog("shift4::Tokenized Card:", "Access token:" + pcfg.UserConfig1 + ", Request send:" + JsonConvert.SerializeObject(authTransaction), "", 3,request.member_id);
                JObject responseToken = shift4PaymentService.TokenizeCardRequest(authTransaction, pcfg.UserConfig1.ToString());
                logDAL.InsertLog("shift4::Tokenized Card:", "Response Rcvd:" + responseToken.ToString(), "", 3,request.member_id);
                AccessResult tokenResult = JsonConvert.DeserializeObject<AccessResult>(responseToken.ToString());
                string cardToken = "";
                if (tokenResult != null && tokenResult.result != null && tokenResult.result.Count > 0)
                {
                    cardToken = tokenResult.result[0].card.token.value;
                    resp = new TokenizedCard
                    {
                        card_token = cardToken,
                        last_four_digits = Common.Common.Right(request.number, 4),
                        first_four_digits = Common.Common.Left(request.number, 4),
                        is_expired = false,
                        customer_name = request.cust_name + "",
                        card_type = Payments.GetCardType(request.number)
                    };
                }
           
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "shift4::Tokenized Card:  " + ex.Message.ToString(), "",1,request.member_id);
            }
            return resp;
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
                if (!testMode)
                    _baseURL = "https://utg.shift4api.net/api/rest/v1/";
                else
                    _baseURL = "https://utgapi.shift4test.com/api/rest/v1/";


                //Transaction Sale Type
                switch (payment.Type)
                {
                    case Payments.Transaction1.ChargeType.Sale:
                    case Payments.Transaction1.ChargeType.Capture:
                    case Payments.Transaction1.ChargeType.AuthOnly:
                        ProcessSaleAuthCard(invoiceId, pcfg, payment, ref pr, payment.Type);
                        break;
                    case Payments.Transaction1.ChargeType.Credit:
                        ProcessRefund(invoiceId, payment, pcfg, "refunds", ref pr);
                        break;
                    case Payments.Transaction1.ChargeType.Void:
                        ProcessVoid(invoiceId, payment, pcfg, "voids", ref pr);
                        break;
                }


                pr.PaymentGateway = Configuration.Gateway.Shift4;
                pr.PayType = Common.Common.PaymentType.CreditCard;
                pr.Amount = payment.Amount;
                pr.Card = payment.Card;
                pr.ProcessedBy = payment.ProcessedBy;
                pr.TransactionType = (Common.Payments.Transaction.ChargeType)payment.Type;
            }
            catch (Exception ex)
            {
                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("Shift4::ProcessCreditCard", "MemberId:" + wineryID.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "",1,wineryID);
            }

            return pr;
        }

        public static void ProcessSaleAuthCard(int invoiceid, Configuration pcfg, Payments.Transaction1 payment, ref TransactionResult pr, Payments.Transaction1.ChargeType chargeType)
        {
            SaleClass saleClass = new SaleClass();
            EventDAL eventdal = new EventDAL(Common.Common.ConnectionString);
            var rsvpDetails = eventdal.GetReservationDetailsbyReservationId(invoiceid);
            saleClass.dateTime = DateTime.Now.ToString("o");
            var amount = new Shift4Amount();
            amount.tax = rsvpDetails.sales_tax.ToString();
            amount.total = payment.Amount.ToString();
            saleClass.amount = amount;

            var clerk = new Clerk();
            clerk.numericId = 17;
            saleClass.clerk = clerk;

            var transaction = new ViewModels.Shift4Transaction();
            transaction.invoice = invoiceid.ToString();
            var purchasecard = new Shift4PurchaseCard();
            purchasecard.customerReference = rsvpDetails.booking_code;
            if (rsvpDetails.user_detail != null)
            {
                
               
                if (rsvpDetails.user_detail.address != null)
                {
                    purchasecard.destinationPostalCode = rsvpDetails.user_detail.address.zip_code + "";
                    saleClass.customer = new Shift4Customer
                    {
                        firstName = rsvpDetails.user_detail.first_name,
                        lastName = rsvpDetails.user_detail.last_name,
                        postalCode = rsvpDetails.user_detail.address.zip_code + ""
                    };
                }
                else
                {
                    purchasecard.destinationPostalCode = "";
                }


                //purchasecard.destinationPostalCode = payment.User.address.zip_code;
            }
            else if (payment.User != null)
            {
                //purchasecard.customerReference = payment.User.user_id.ToString();
                if (payment.User.address != null)
                {
                    purchasecard.destinationPostalCode = payment.User.address.zip_code + "";
                    saleClass.customer = new Shift4Customer
                    {
                        firstName = payment.User.first_name,
                        lastName = payment.User.last_name,
                        addressLine1 = payment.User.address.address_1 + "",
                        postalCode = payment.User.address.zip_code + ""
                    };
                }
                else
                {
                    purchasecard.destinationPostalCode = "";
                }
                //purchasecard.destinationPostalCode = payment.User.address.zip_code;
            }
            purchasecard.productDescriptors = new List<string>();
            purchasecard.productDescriptors.Add("CellarPass - RSVP");
            transaction.purchaseCard = purchasecard;
            saleClass.transaction = transaction;

            var salecard = new SaleCard();
            var saltoken = new Token();
            saltoken.value = payment.Card.CardToken.ToString();
            if (!string.IsNullOrWhiteSpace(payment.Card.CVV))
            {
                salecard.securityCode = new SecurityCode
                {
                    value = payment.Card.CVV
                };
            }
            salecard.token = saltoken;
            saleClass.card = salecard;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            Shift4PaymentService shift4PaymentService = new Shift4PaymentService();
            logDAL.InsertLog("Shift4::ProcessSale", "Request Sent:" + JsonConvert.SerializeObject(saleClass), "", 3,payment.WineryId);
            JObject saleToken = null;
            try
            {
                if (chargeType == Payments.Transaction1.ChargeType.Sale)
                {
                    saleToken = shift4PaymentService.SalePurchaseTransaction(saleClass, pcfg.UserConfig1.ToString());
                }
                else if (chargeType == Payments.Transaction1.ChargeType.AuthOnly)
                {
                    saleToken = shift4PaymentService.AuthOnlyTransaction(saleClass, pcfg.UserConfig1.ToString());
                }
                else
                {
                    saleToken = shift4PaymentService.CaptureAuthTransaction(saleClass, pcfg.UserConfig1.ToString());
                }
                
                logDAL.InsertLog("Shift4::ProcessSale", "Response Rcvd:" + saleToken.ToString(), "", 3,payment.WineryId);
            }
            catch(Exception ex)
            {
                if (ex.Message.Contains("TIMEOUT"))
                {
                    logDAL.InsertLog("Shift4::ProcessSale", "Gateway timeout error,  Response Rcvd:" + ex.Message, "", 1,payment.WineryId);
                    //do get invoice call
                    Shift4PaymentService.GetInvoiceInfo(invoiceid, payment, pcfg, ref pr);

                }
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";
                return;
            }
            

            AccesssaleResult saleResult = JsonConvert.DeserializeObject<AccesssaleResult>(saleToken.ToString());
            
            
            if (saleResult != null && saleResult.result!= null && saleResult.result.Count > 0)
            {
                
                if (saleResult.result[0].transaction.responseCode == "A")
                {
                    var transactID = saleResult.result[0].transaction.invoice;
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = transactID;
                    pr.ResponseCode = saleResult.result[0].transaction.responseCode;
                    pr.ApprovalCode = saleResult.result[0].transaction.authorizationCode;
                }
                else
                {
                    

                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Error";
                    pr.ResponseCode = saleResult.result[0].transaction.responseCode;
                    pr.TransactionID = saleResult.result[0].transaction.invoice;
                }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";
            }
        }

        private static void ProcessRefund(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, string action, ref TransactionResult pr)
        {
            SaleClass saleClass = new SaleClass();
            EventDAL eventdal = new EventDAL(Common.Common.ConnectionString);
            var rsvpDetails = eventdal.GetReservationDetailsbyReservationId(invoiceId);
            saleClass.dateTime = DateTime.Now.ToString("o");
            var amount = new Shift4Amount();
            amount.tax = rsvpDetails.sales_tax.ToString();
            amount.total = payment.Amount.ToString();
            saleClass.amount = amount;

            var clerk = new Clerk();
            clerk.numericId = 17;
            saleClass.clerk = clerk;

            var transaction = new ViewModels.Shift4Transaction();
            transaction.invoice = payment.TransactionID;
            var purchasecard = new Shift4PurchaseCard();
            purchasecard.customerReference = rsvpDetails.booking_code;
            if (payment.User.address != null)
            {
                purchasecard.destinationPostalCode = payment.User.address.zip_code + "";
            }
            else
            {
                purchasecard.destinationPostalCode = "";
            }
            //purchasecard.destinationPostalCode = payment.User.address.zip_code;
            purchasecard.productDescriptors = new List<string>();
            purchasecard.productDescriptors.Add("CellarPass - RSVP");
            transaction.purchaseCard = purchasecard;
            saleClass.transaction = transaction;

            var salecard = new SaleCard();
            var saltoken = new Token();
            saltoken.value = payment.Card.CardToken.ToString();
            salecard.token = saltoken;
            saleClass.card = salecard;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            Shift4PaymentService shift4PaymentService = new Shift4PaymentService();
            logDAL.InsertLog("Shift4::ProcessRefund", "Request Sent:" + JsonConvert.SerializeObject(saleClass), "", 3,payment.WineryId);
            JObject saleToken = shift4PaymentService.RefundTransaction(saleClass, pcfg.UserConfig1.ToString());
            logDAL.InsertLog("Shift4::ProcessRefund", "Response Rcvd:" + saleToken.ToString(), "", 3,payment.WineryId);

            AccesssaleResult saleResult = JsonConvert.DeserializeObject<AccesssaleResult>(saleToken.ToString());
            

            if (saleResult != null && saleResult.result != null && saleResult.result.Count > 0)
            {
                if (saleResult.result[0].transaction.responseCode == "A")
                {
                    var transactID = saleResult.result[0].transaction.invoice;
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = transactID;
                    pr.ResponseCode = saleResult.result[0].transaction.responseCode;
                    pr.ApprovalCode = saleResult.result[0].transaction.authorizationCode;
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Error";
                    pr.ResponseCode = saleResult.result[0].transaction.responseCode;
                }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";
            }
        }

        private static void ProcessVoid(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, string action, ref TransactionResult pr)
        {
            Shift4PaymentService shift4PaymentService = new Shift4PaymentService();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            logDAL.InsertLog("Shift4::ProcessVoid", "Request Sent:" + payment.TransactionID.ToString(), "", 3,payment.WineryId);
            JObject saleToken = shift4PaymentService.VoidTransaction(pcfg.UserConfig1.ToString(), payment.TransactionID.ToString());
            logDAL.InsertLog("Shift4::ProcessVoid", "Response Rcvd:" + saleToken.ToString(), "", 3,payment.WineryId);

            AccesssaleResult saleResult = JsonConvert.DeserializeObject<AccesssaleResult>(saleToken.ToString());
            
            

            if (saleResult != null && saleResult.result != null && saleResult.result.Count > 0)
            {
                if (saleResult.result[0].transaction.responseCode == "A")
                {
                    var transactID = saleResult.result[0].transaction.invoice;
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = transactID;
                    pr.ResponseCode = saleResult.result[0].transaction.responseCode;
                    pr.ApprovalCode = saleResult.result[0].transaction.authorizationCode;
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Error";
                    pr.ResponseCode = saleResult.result[0].transaction.responseCode;
                }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";
            }
        }

        private static void GetInvoiceInfo(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, ref TransactionResult pr)
        {
            Shift4PaymentService shift4PaymentService = new Shift4PaymentService();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            logDAL.InsertLog("Shift4::GetInvoiceInfo", "Request Sent:" + pr.TransactionID.ToString(), "", 3,payment.WineryId);
            JObject saleToken = shift4PaymentService.GetInvoiceInfo(pcfg.UserConfig1.ToString(), pr.TransactionID.ToString());
            logDAL.InsertLog("Shift4::GetInvoiceInfo", "Response Rcvd:" + saleToken.ToString(), "", 3,payment.WineryId);

            AccesssaleResult saleResult = JsonConvert.DeserializeObject<AccesssaleResult>(saleToken.ToString());



            if (saleResult != null && saleResult.result != null && saleResult.result.Count > 0)
            {
                if (saleResult.result[0].transaction.responseCode == "A")
                {
                    var transactID = saleResult.result[0].transaction.invoice;
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = transactID;
                    pr.ResponseCode = saleResult.result[0].transaction.responseCode;
                    pr.ApprovalCode = saleResult.result[0].transaction.authorizationCode;
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Error";
                    pr.ResponseCode = saleResult.result[0].transaction.responseCode;
                }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";
            }
        }

        public JObject AccessTokenExchange(Shift4Model request)
        {
            return (JObject)send(ENDPOINT_ACCESSTOKEN, request);
        }

        public JObject RefundTransaction(SaleClass request, string token)
        {
            Object objResponse = null;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("InterfaceVersion", "3.0");
            client.DefaultRequestHeaders.Add("InterfaceName", "CellarPass");
            client.DefaultRequestHeaders.Add("CompanyName", "CELLARPASS");
            client.DefaultRequestHeaders.Add("AccessToken", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string content = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            using (var response = client.PostAsync(ENDPOINT_REFUND, stringContent).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                JsonTextReader jsreader = new JsonTextReader(new StringReader(varResponse));
                try
                {
                    objResponse = new JsonSerializer().Deserialize(jsreader);
                }
                catch (JsonReaderException jx)
                {
                    return null;
                }
            }
            return (JObject)objResponse;
        }

        public JObject VoidTransaction(string token, string Invoice)
        {
            Object objResponse = null;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Invoice", Invoice);
            client.DefaultRequestHeaders.Add("InterfaceVersion", "3.0");
            client.DefaultRequestHeaders.Add("InterfaceName", "CellarPass");
            client.DefaultRequestHeaders.Add("CompanyName", "CELLARPASS");
            client.DefaultRequestHeaders.Add("AccessToken", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using (var response = client.DeleteAsync(ENDPOINT_VOID))
            {
                var varResponse = response.Result;
                var DelResponse = varResponse.Content.ReadAsStringAsync().Result;
                JsonTextReader jsreader = new JsonTextReader(new StringReader(DelResponse));
                try
                {
                    objResponse = new JsonSerializer().Deserialize(jsreader);
                }
                catch (JsonReaderException jx)
                {
                    return null;
                }
            }
            return (JObject)objResponse;
        }

        public JObject GetInvoiceInfo(string token, string Invoice)
        {
            Object objResponse = null;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Invoice", Invoice);
            client.DefaultRequestHeaders.Add("InterfaceVersion", "3.0");
            client.DefaultRequestHeaders.Add("InterfaceName", "CellarPass");
            client.DefaultRequestHeaders.Add("CompanyName", "CELLARPASS");
            client.DefaultRequestHeaders.Add("AccessToken", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using (var response = client.GetAsync(ENDPOINT_VOID))
            {
                var varResponse = response.Result;
                var DelResponse = varResponse.Content.ReadAsStringAsync().Result;
                JsonTextReader jsreader = new JsonTextReader(new StringReader(DelResponse));
                try
                {
                    objResponse = new JsonSerializer().Deserialize(jsreader);
                }
                catch (JsonReaderException jx)
                {
                    return null;
                }
            }
            return (JObject)objResponse;
        }


        public JObject SalePurchaseTransaction(SaleClass request, string token)
        {
            Object objResponse = null;
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 0, 65, 0);
            client.BaseAddress = new Uri(_baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("InterfaceVersion", "3.0");
            client.DefaultRequestHeaders.Add("InterfaceName", "CellarPass");
            client.DefaultRequestHeaders.Add("CompanyName", "CELLARPASS");
            client.DefaultRequestHeaders.Add("AccessToken", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string content = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            using (var response = client.PostAsync(ENDPOINT_SALE, stringContent).Result)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                {
                    throw new Exception("TIMEOUT:" + response.Content.ReadAsStringAsync().Result);
                }
                var varResponse = response.Content.ReadAsStringAsync().Result;
                JsonTextReader jsreader = new JsonTextReader(new StringReader(varResponse));
                try
                {
                    objResponse = new JsonSerializer().Deserialize(jsreader);
                }
                catch (JsonReaderException jx)
                {
                    return null;
                }
            }
            return (JObject)objResponse;
        }

        public JObject CaptureAuthTransaction(SaleClass request, string token)
        {
            Object objResponse = null;
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 0, 65, 0);
            client.BaseAddress = new Uri(_baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("InterfaceVersion", "3.0");
            client.DefaultRequestHeaders.Add("InterfaceName", "CellarPass");
            client.DefaultRequestHeaders.Add("CompanyName", "CELLARPASS");
            client.DefaultRequestHeaders.Add("AccessToken", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string content = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            using (var response = client.PostAsync(ENDPOINT_CAPTURE, stringContent).Result)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                {
                    throw new Exception("TIMEOUT:" + response.Content.ReadAsStringAsync().Result);
                }
                var varResponse = response.Content.ReadAsStringAsync().Result;
                JsonTextReader jsreader = new JsonTextReader(new StringReader(varResponse));
                try
                {
                    objResponse = new JsonSerializer().Deserialize(jsreader);
                }
                catch (JsonReaderException jx)
                {
                    return null;
                }
            }
            return (JObject)objResponse;
        }


        public JObject AuthOnlyTransaction(SaleClass request, string token)
        {
            Object objResponse = null;
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 0, 65, 0);
            client.BaseAddress = new Uri(_baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("InterfaceVersion", "3.0");
            client.DefaultRequestHeaders.Add("InterfaceName", "CellarPass");
            client.DefaultRequestHeaders.Add("CompanyName", "CELLARPASS");
            client.DefaultRequestHeaders.Add("AccessToken", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string content = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            using (var response = client.PostAsync(ENDPOINT_AUTH, stringContent).Result)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                {
                    throw new Exception("TIMEOUT:" + response.Content.ReadAsStringAsync().Result);
                }
                var varResponse = response.Content.ReadAsStringAsync().Result;
                JsonTextReader jsreader = new JsonTextReader(new StringReader(varResponse));
                try
                {
                    objResponse = new JsonSerializer().Deserialize(jsreader);
                }
                catch (JsonReaderException jx)
                {
                    return null;
                }
            }
            return (JObject)objResponse;
        }

        public JObject TokenizeCardRequest(TokenAddReq request, string token)
        {
            Object objResponse = null;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("InterfaceVersion", "3.0");
            client.DefaultRequestHeaders.Add("InterfaceName", "CellarPass");
            client.DefaultRequestHeaders.Add("CompanyName", "CELLARPASS");
            client.DefaultRequestHeaders.Add("AccessToken", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string content = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            using (var response = client.PostAsync(ENDPOINT_TOKEN, stringContent).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                JsonTextReader jsreader = new JsonTextReader(new StringReader(varResponse));
                try
                {
                    objResponse = new JsonSerializer().Deserialize(jsreader);
                }
                catch (JsonReaderException jx)
                {
                    return null;
                }
            }
            return (JObject)objResponse;
        }


        private Object send(String endpoint, Shift4Model request, string token = "")
        {
            Object objResponse = null;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("InterfaceVersion", "3.0");
            client.DefaultRequestHeaders.Add("InterfaceName", "CellarPass");
            client.DefaultRequestHeaders.Add("CompanyName", "CELLARPASS");
            if (token != "")
            {
                client.DefaultRequestHeaders.Add("AccessToken", token);
            }
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string content = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            using (var response = client.PostAsync(endpoint, stringContent).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                JsonTextReader jsreader = new JsonTextReader(new StringReader(varResponse));
                try
                {
                    objResponse = new JsonSerializer().Deserialize(jsreader);
                }
                catch (JsonReaderException jx)
                {
                    return null;
                }
            }
            return objResponse;
        }

    }
}
