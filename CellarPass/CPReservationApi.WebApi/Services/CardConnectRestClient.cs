using CPReservationApi.DAL;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using static CPReservationApi.Common.Payments;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;

namespace CPReservationApi.WebApi.Services
{
    public class CardConnectRestClient
    {
        private String url;
        private String userpass;
        private String username;
        private String password;
        //fts-uat.cardconnect.com/cardconnect/rest/
        private static string _baseURL = "https://boltgw-uat.cardconnect.com/cardconnect/rest/";


        // Endpoint names
        private static String ENDPOINT;
        private static String ENDPOINT_TOKENIZE;
        //private static String USERNAME = "testing";
        //private static String PASSWORD = "testing123";
        private static String USERNAME;
        private static String PASSWORD;
        private static String ENDPOINT_AUTH = "auth";
        private static String ENDPOINT_TOKEN = "tokenize";
        private static String ENDPOINT_CAPTURE = "capture";
        private static String ENDPOINT_VOID = "void";
        private static String ENDPOINT_REFUND = "refund";
        private static String ENDPOINT_INQUIRE = "inquire";
        private static String ENDPOINT_SETTLESTAT = "settlestat";
        private static String ENDPOINT_DEPOSIT = "deposit";
        private static String ENDPOINT_PROFILE = "profile";

        private enum OPERATIONS { GET, PUT, POST, DELETE };

        private static String USER_AGENT = "CardConnectRestClient-Csharp";
        private static String CLIENT_VERSION = "1.0";

        static private ViewModels.AppSettings _appSettings;
        public CardConnectRestClient(IOptions<ViewModels.AppSettings> settings)
        {
            _appSettings = settings.Value;
        }

        public CardConnectRestClient(String url, String username, String password)
        {
            if (isEmpty(url)) throw new ArgumentException("url parameter is required");
            if (isEmpty(username)) throw new ArgumentException("username parameter is required");
            if (isEmpty(password)) throw new ArgumentException("password parameter is required");

            if (!url.EndsWith("/")) url = url + "/";
            this.url = url;
            this.username = username;
            this.password = password;
            this.userpass = username + ":" + password;
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
                JObject requestData = new JObject();

                requestData.Add("account", request.number.ToString());
                // Card Expiry
                requestData.Add("expiry", request.exp_year.ToString() + request.exp_month.ToString());
                // Card CCV2
                if(!string.IsNullOrWhiteSpace(request.cvv2))
                    requestData.Add("cvv", request.cvv2.ToString());
                if (!testMode)
                    ENDPOINT_TOKENIZE = "https://" + pcfg.UserConfig2 + ".cardconnect.com/cardsecure/api/v1/ccn/";
                else
                    ENDPOINT_TOKENIZE = "https://boltgw-uat.cardconnect.com/cardsecure/api/v1/ccn/";

                USERNAME = pcfg.MerchantLogin;
                PASSWORD = pcfg.MerchantPassword;
                CardConnectRestClient client = new CardConnectRestClient(ENDPOINT_TOKENIZE, USERNAME, PASSWORD);
                // Send an AuthTransaction request
                if(testMode)
                    logDAL.InsertLog("CardConnect::TokenizedCard", "RequestURL:" + ENDPOINT_TOKENIZE + ", data:" + requestData.ToString(), "", 3,request.member_id);
                JObject resultModel = client.tokenize(requestData);
                logDAL.InsertLog("CardConnect::TokenizedCard", "Response:" + resultModel.ToString(), "", 3,request.member_id);
                CardConnectModel result = JsonConvert.DeserializeObject<CardConnectModel>(resultModel.ToString());
                resp = new TokenizedCard
                {
                    card_token = result.token,
                    last_four_digits = Common.Common.Right(request.number, 4),
                    first_four_digits = Common.Common.Left(request.number, 4),
                    is_expired = false,
                    customer_name = request.cust_name + "",
                    card_type = Payments.GetCardType(request.number)
                };
            }
            catch (Exception ex)
            {

                logDAL.InsertLog("WebApi", "CardConnect::Tokenized Card:  " + ex.Message.ToString(), "",1,request.member_id);
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
                    _baseURL = "https://" + pcfg.UserConfig2 + ".cardconnect.com/cardconnect/rest/";


                //Transaction Sale Type
                switch (payment.Type)
                {
                    case Payments.Transaction1.ChargeType.Sale:
                    case Payments.Transaction1.ChargeType.Capture:
                        if (!string.IsNullOrWhiteSpace(payment.TransactionID))
                        {
                            ProcessCaptureAuthCard(invoiceId, pcfg, payment, ref pr);
                        }
                        else
                        {
                            ProcessSaleAuthCard(invoiceId, pcfg, payment, ref pr, true);
                        }
                        break;
                    case Payments.Transaction1.ChargeType.AuthOnly:
                        ProcessSaleAuthCard(invoiceId, pcfg, payment, ref pr, false);
                        break;
                    case Payments.Transaction1.ChargeType.Credit:
                        ProcessRefund(invoiceId, payment, pcfg, "refunds", ref pr);
                        break;
                    case Payments.Transaction1.ChargeType.Void:
                        ProcessVoid(invoiceId, payment, pcfg, "voids", ref pr);
                        break;
                }


                pr.PaymentGateway = Configuration.Gateway.CardConnect;
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
                logDAL.InsertLog("CardConnect.ProcessCreditCard", "MemberId:" + wineryID.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "",1,wineryID);
            }

            return pr;
        }


        public static void ProcessSaleAuthCard(int invoiceid, Configuration pcfg, Payments.Transaction1 payment, ref TransactionResult pr, bool captureSale = false)
        {
            JObject request = new JObject();
            // Merchant ID
            //request.Add("merchid", "496160873888");
            request.Add("merchid", pcfg.UserConfig1.ToString());
            // Card Number

            //Profile field should always be a 20 - digit numeric sequence while account will be a 16 - digit sequence.CellarPass should review to confirm correct values are being passed in these fields for future auth requests.

            if (payment.Card.CardToken.Length == 20)
                request.Add("profile", payment.Card.CardToken.ToString());
            else
                request.Add("account", payment.Card.CardToken.ToString());

            // Card Expiry
            request.Add("expiry", payment.Card.ExpMonth.ToString() + payment.Card.ExpYear.ToString().Substring(2, 2));
            // Card CCV2
            if (!string.IsNullOrWhiteSpace(payment.Card.CVV))
                request.Add("cvv2", payment.Card.CVV.ToString());
            // Transaction amount
            request.Add("amount", payment.Amount.ToString());

            request.Add("capture", captureSale?"Y": "N");
            request.Add("ecomind", "E");

            //if (!string.IsNullOrWhiteSpace(payment.TransactionID))
            //{
            //    request.Add("authcode", payment.TransactionID);
            //}


            string userName = "";
            if (!string.IsNullOrWhiteSpace(payment.Card.CustName))
            {
                userName = payment.Card.CustName;
            }
            else if (payment.User != null && !string.IsNullOrWhiteSpace(payment.User.first_name))
            {
                userName = payment.User.first_name + " " + Convert.ToString(payment.User.last_name);
            }
            if (!string.IsNullOrWhiteSpace(userName))
            {
                request.Add("name", userName);
            }
            if (payment.User != null )
            {
                if (payment.User.address != null)
                {
                    var address = payment.User.address;
                    if (!string.IsNullOrWhiteSpace(address.address_1))
                    {
                        request.Add("address", address.address_1.Trim());
                    }

                    if (!string.IsNullOrWhiteSpace(address.address_2))
                    {
                        request.Add("address2", address.address_2.Trim());
                    }


                    if (!string.IsNullOrWhiteSpace(address.zip_code.Trim()))
                    {
                        string zipCode = address.zip_code.Trim();

                        request.Add("postal", zipCode.Length > 9 ? zipCode.Substring(0, 5) : zipCode);

                        if (string.IsNullOrEmpty(address.city) || string.IsNullOrEmpty(address.state))
                        {
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            Model.UserAddress addr = userDAL.GetUserAddressByZipCode(zipCode);

                            address.city = addr.city;
                            address.state = addr.state;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(address.city))
                    {
                        request.Add("city", address.city);
                    }

                    if (!string.IsNullOrWhiteSpace(address.state))
                    {
                        request.Add("region", address.state.Trim());
                    }


                    if (!string.IsNullOrWhiteSpace(address.country))
                    {
                        string country = address.country.Trim();
                        if (country.Length > 2)
                        {
                            UserDAL dal = new UserDAL(Common.Common.ConnectionString);
                            country = dal.GetCountryISOByCountryName(country);
                        }
                        request.Add("country", country);
                    }
                }
                if (!string.IsNullOrWhiteSpace(payment.User.phone_number))
                {
                    request.Add("phone", payment.User.phone_number.Trim());
                }                
                if (!string.IsNullOrWhiteSpace(payment.User.email))
                {
                    request.Add("email", payment.User.email.Trim());
                }

            }

            // Create the REST client
            ENDPOINT = _baseURL;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            CardConnectRestClient client = new CardConnectRestClient(ENDPOINT, pcfg.MerchantLogin, pcfg.MerchantPassword);
            // Send an AuthTransaction request
            JObject resultModel = client.authorizeTransaction(request);
            
            logDAL.InsertLog("CardConnect::ProcessSale", "Request:" + JsonConvert.SerializeObject(request), "", 3, payment.WineryId);
            CardConnectModel result = JsonConvert.DeserializeObject<CardConnectModel>(resultModel.ToString());
            logDAL.InsertLog("CardConnect::ProcessSale", "Response:" + JsonConvert.SerializeObject(result), "", 3, payment.WineryId);
            if (result != null)
            {
                int respcode = -1;
                int.TryParse(result.respcode, out respcode);

                if (result != null && (respcode == 0 || result.respstat.Trim().ToUpper().Equals("A")))
                {
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = result.retref;
                    pr.ResponseCode = result.respcode;
                    pr.ApprovalCode = result.authcode;
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = result.resptext;
                    pr.ResponseCode = result.respcode;
                }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";

                logDAL.InsertLog("WebApi", "CardConnect.ProcessCreditCard: Request:" + JsonConvert.SerializeObject(request) + ",Unknown error from gateway", "", 3,payment.WineryId);
            }
        }

        public static void ProcessSaleAuthCard_old(int invoiceid, Configuration pcfg, Payments.Transaction1 payment, ref TransactionResult pr)
        {
            JObject request = new JObject();
            // Merchant ID
            //request.Add("merchid", "496160873888");
            request.Add("merchid", pcfg.UserConfig1.ToString());
            // Card Number
            request.Add("account", payment.Card.CardToken.ToString());
            // Card Expiry
            request.Add("expiry", payment.Card.ExpMonth.ToString() + payment.Card.ExpYear.ToString().Substring(2, 2));
            // Card CCV2

            if (!string.IsNullOrWhiteSpace(payment.Card.CVV))
                request.Add("cvv2", payment.Card.CVV.ToString());

            // Transaction amount
            request.Add("amount", payment.Amount.ToString());

            request.Add("capture", "Y");

            // Create the REST client
            ENDPOINT = _baseURL;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            CardConnectRestClient client = new CardConnectRestClient(ENDPOINT, pcfg.MerchantLogin, pcfg.MerchantPassword);
            // Send an AuthTransaction request
            JObject resultModel = client.authorizeTransaction(request);
            logDAL.InsertLog("CardConnect::ProcessSale", "Request:" + JsonConvert.SerializeObject(request), "", 3,payment.WineryId);
            CardConnectModel result = JsonConvert.DeserializeObject<CardConnectModel>(resultModel.ToString());
            logDAL.InsertLog("CardConnect::ProcessSale", "Response:" + JsonConvert.SerializeObject(result), "", 3,payment.WineryId);

            if (result != null)
            {
                int respcode = -1;
                int.TryParse(result.respcode, out respcode);

                if (result != null && (respcode == 0 || result.respstat.Trim().ToUpper().Equals("A")))
                {
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = result.retref;
                    pr.ResponseCode = result.respcode;
                    pr.ApprovalCode = result.authcode;
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = result.resptext;
                    pr.ResponseCode = result.respcode;
                }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";

                logDAL.InsertLog("WebApi", "CardConnect.ProcessCreditCard: Request:" + JsonConvert.SerializeObject(request) + ",Unknown error from gateway", "", 3,payment.WineryId);
            }
        }

        public static void ProcessCaptureAuthCard(int invoiceid, Configuration pcfg, Payments.Transaction1 payment, ref TransactionResult pr)
        {
            JObject request = new JObject();
            // Merchant ID
            //request.Add("merchid", "496160873888");
            request.Add("merchid", pcfg.UserConfig1.ToString());
            request.Add("retref", payment.TransactionID.ToString());
            // Transaction amount
            request.Add("amount", payment.Amount.ToString());

            // Create the REST client
            ENDPOINT = _baseURL;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            CardConnectRestClient client = new CardConnectRestClient(ENDPOINT, pcfg.MerchantLogin, pcfg.MerchantPassword);
            // Send an AuthTransaction request
            JObject resultModel = client.captureTransaction(request);
            logDAL.InsertLog("CardConnect::ProcessCaptureAuthCard", "Request:" + JsonConvert.SerializeObject(request), "", 3, payment.WineryId);
            CardConnectModel result = JsonConvert.DeserializeObject<CardConnectModel>(resultModel.ToString());
            logDAL.InsertLog("CardConnect::ProcessCaptureAuthCard", "Response:" + JsonConvert.SerializeObject(result), "", 3, payment.WineryId);

            if (result != null)
            {
                int respcode = -1;
                int.TryParse(result.respcode, out respcode);

                if (result != null && (respcode == 0 || result.respstat.Trim().ToUpper().Equals("A")))
                {
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = result.retref;
                    pr.ResponseCode = result.respcode;
                    pr.ApprovalCode = result.authcode;
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = result.resptext;
                    pr.ResponseCode = result.respcode;
                }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";

                logDAL.InsertLog("WebApi", "CardConnect.ProcessCreditCard: Request:" + JsonConvert.SerializeObject(request) + ",Unknown error from gateway", "", 3, payment.WineryId);
            }
        }

        private static void ProcessRefund(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, string action, ref TransactionResult pr)
        {
            JObject request = new JObject();
            // Merchant ID
            request.Add("merchid", pcfg.UserConfig1);
            // Transaction amount
            request.Add("amount", payment.Amount.ToString());
            // Return Reference code from authorization request
            request.Add("retref", payment.TransactionID.ToString());

            // Create the CardConnect REST client
            ENDPOINT = _baseURL;
            CardConnectRestClient client = new CardConnectRestClient(ENDPOINT, pcfg.MerchantLogin, pcfg.MerchantPassword);

            // Send an refundTransaction request
            JObject resultModel = client.refundTransaction(request);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            logDAL.InsertLog("CardConnect::ProcessRefund", "Request:" + JsonConvert.SerializeObject(request), "", 3,payment.WineryId);
            CardConnectModel result = JsonConvert.DeserializeObject<CardConnectModel>(resultModel.ToString());
            logDAL.InsertLog("CardConnect::ProcessRefund", "Response:" + JsonConvert.SerializeObject(result), "", 3,payment.WineryId);

            if (result != null)
            {
                int respcode = -1;
                int.TryParse(result.respcode, out respcode);

                if (result != null && (respcode == 0 || result.respstat.Trim().ToUpper().Equals("A")))
                {
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = result.retref;
                    pr.Detail = result.resptext;
                    pr.ResponseCode = result.respcode;
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = result.resptext;
                    pr.ResponseCode = result.respcode;

               }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";

                logDAL.InsertLog("WebApi", "CardConnect.ProcessRefund: Request:" + JsonConvert.SerializeObject(request) + ",Unknown error from gateway", "", 3,payment.WineryId);
            }
        }

        private static void ProcessVoid(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, string action, ref TransactionResult pr)
        {
            JObject request = new JObject();
            // Merchant ID
            request.Add("merchid", pcfg.UserConfig1);
            // Return Reference code from authorization request
            request.Add("retref", payment.TransactionID.ToString());

            // Create the CardConnect REST client
            ENDPOINT = _baseURL;
            CardConnectRestClient client = new CardConnectRestClient(ENDPOINT, pcfg.MerchantLogin, pcfg.MerchantPassword);

            // Send an refundTransaction request
            JObject resultModel = client.voidTransaction(request);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            logDAL.InsertLog("CardConnect::ProcessVoid:", "Request:" + JsonConvert.SerializeObject(request), "", 3,payment.WineryId);
            CardConnectModel result = JsonConvert.DeserializeObject<CardConnectModel>(resultModel.ToString());
            logDAL.InsertLog("CardConnect::ProcessVoid", "Response:" + JsonConvert.SerializeObject(result), "", 3,payment.WineryId);

            if (result != null)
            {
                int respcode = -1;
                int.TryParse(result.respcode, out respcode);

                if (result != null && (respcode == 0 || result.respstat.Trim().ToUpper().Equals("A")))
                {
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = result.retref;
                    pr.Detail = result.resptext;
                    pr.ResponseCode = result.respcode;
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = result.resptext;
                    pr.ResponseCode = result.respcode;

                }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";

                logDAL.InsertLog("WebApi", "CardConnect.ProcessRefund: ProcessVoid:" + JsonConvert.SerializeObject(request) + ",Unknown error from gateway", "", 3,payment.WineryId);
            }
        }

        /**
        * Authorize trasaction
        * @param request JObject representing an Authorization transaction request
        * @return JObject representing an Authorization transaction response
        */
        public JObject authorizeTransaction(JObject request)
        {
            return (JObject)send(ENDPOINT_AUTH, OPERATIONS.PUT, request);
        }

        public JObject tokenize(JObject request)
        {
            return (JObject)send(ENDPOINT_TOKEN, OPERATIONS.PUT, request);
        }


        /**
* Capture transaction
* @param request JObject representing a Capture transaction request
* @return JObject representing a Capture transaction response
*/
        public JObject captureTransaction(JObject request)
        {
            return (JObject)send(ENDPOINT_CAPTURE, OPERATIONS.PUT, request);
        }


        /**
         * Void transaction
         * @param request JObject representing a Void transaction request
         * @return JObject representing a Void transaction response
         */
        public JObject voidTransaction(JObject request)
        {
            return (JObject)send(ENDPOINT_VOID, OPERATIONS.PUT, request);
        }


        /**
         * Refund Transaction
         * @param request JObject representing a Refund transaction request
         * @return JObject represeting a Refund transactino response
         */
        public JObject refundTransaction(JObject request)
        {
            return (JObject)send(ENDPOINT_REFUND, OPERATIONS.PUT, request);
        }


        /**
         * Inquire Transaction
         * @param merchid Merchant ID
         * @param retref RetRef to inquire
         * @return JObject representing the request transaction
         * @throws IllegalArgumentException
         */
        public JObject inquireTransaction(String merchid, String retref)
        {
            if (isEmpty(merchid)) throw new ArgumentException("Missing required parameter: merchid");
            if (isEmpty(retref)) throw new ArgumentException("Missing required parameter: retref");

            String url = ENDPOINT_INQUIRE + "/" + retref + "/" + merchid;
            return (JObject)send(url, OPERATIONS.GET, null);
        }


        /**
         * Gets the settlement status for transactions
         * @param merchid Mechant ID
         * @param date Date in MMDD format
         * @return JArray of JObjects representing Settlement batches, each batch containing a JArray of 
         * JObjects representing the settlement status of each transaction
         * @throws IllegalArgumentException
         */
        public JArray settlementStatus(String merchid, String date)
        {
            if ((!isEmpty(merchid) && isEmpty(date)) || (isEmpty(merchid) && !isEmpty(date)))
                throw new ArgumentException("Both merchid and date parameters are required, or neither");

            String url = null;
            if (isEmpty(merchid) || isEmpty(date))
            {
                url = ENDPOINT_SETTLESTAT;
            }
            else
            {
                url = ENDPOINT_SETTLESTAT + "?merchid=" + merchid + "&date=" + date;
            }

            return (JArray)send(url, OPERATIONS.GET, null);
        }


        /**
         * Retrieves deposit status information for the given merchant and date
         * @param merchid Merchant ID
         * @param date in MMDD format
         * @return
         * @throws IllegalArgumentException
         */
        public JObject depositStatus(String merchid, String date)
        {
            if ((!isEmpty(merchid) && isEmpty(date)) || (isEmpty(merchid) && !isEmpty(date)))
                throw new ArgumentException("Both merchid and date parameters are required, or neither");

            String url = null;
            if (isEmpty(merchid) || isEmpty(date))
            {
                url = ENDPOINT_DEPOSIT;
            }
            else
            {
                url = ENDPOINT_DEPOSIT + "?merchid=" + merchid + "&date=" + date;
            }
            return (JObject)send(url, OPERATIONS.GET, null);
        }


        /**
         * Retrieves a profile
         * @param profileid ProfileID to retrieve
         * @param accountid Optional account id within profile
         * @param merchid Merchant ID
         * @return JArray of JObjects each represeting a profile
         * @throws IllegalArgumentException
         */
        public JArray profileGet(String profileid, String accountid, String merchid)
        {
            if (isEmpty(profileid)) throw new ArgumentException("Missing required parameter: profileid");
            if (isEmpty(merchid)) throw new ArgumentException("Missing required parameter: merchid");
            if (accountid == null) accountid = "";

            String url = ENDPOINT_PROFILE + "/" + profileid + "/" + accountid + "/" + merchid;
            return (JArray)send(url, OPERATIONS.GET, null);
        }


        /**
         * Deletes a profile
         * @param profileid ProfileID to delete
         * @param accountid Optional accountID within the profile
         * @param merchid Merchant ID
         * @return
         * @throws IllegalArgumentException
         */
        public JObject profileDelete(String profileid, String accountid, String merchid)
        {
            if (isEmpty(profileid)) throw new ArgumentException("Missing required parameter: profileid");
            if (isEmpty(merchid)) throw new ArgumentException("Missing required parameter: merchid");
            if (accountid == null) accountid = "";

            String url = ENDPOINT_PROFILE + "/" + profileid + "/" + accountid + "/" + merchid;
            return (JObject)send(url, OPERATIONS.DELETE, null);
        }



        /**
         * Creates a new profile
         * @param request JObject representing the Profile creation request
         * @return JSONObejct representing the newly created profile
         * @throws IllegalArgumentException
         */
        public JObject profileCreate(JObject request)
        {
            return (JObject)send(ENDPOINT_PROFILE, OPERATIONS.PUT, request);
        }


        /**
         * Updates an existing profile
         * @param request JObject representing the Profile Update request
         * @return JObject representing the updated Profile
         */
        public JObject profileUpdate(JObject request)
        {
            return profileCreate(request);
        }


        private Boolean isEmpty(String s)
        {
            if (s == null) return true;
            if (s.Length <= 0) return true;
            if ("".Equals(s)) return true;
            return false;
        }

        private Object send(String endpoint, OPERATIONS operation, JObject request)
        {
            Object objResponse = null;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            string content = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            using (var response = client.PostAsync(endpoint, stringContent).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                JsonTextReader jsreader = new JsonTextReader(new StringReader(varResponse));
                try
                {
                    objResponse= new JsonSerializer().Deserialize(jsreader);
                }
                catch (JsonReaderException jx)
                {
                    return null;
                }
            }

            return objResponse;
            // Create REST client
            //RestClient client = new RestClient(url);

            //// Set authentication credentials
            //client.Authenticator = new HttpBasicAuthenticator(username, password);

            //// Create REST request
            //RestRequest rest = null;
            //switch (operation)
            //{
            //    case OPERATIONS.PUT: rest = new RestRequest(endpoint, Method.PUT); break;
            //    case OPERATIONS.GET: rest = new RestRequest(endpoint, Method.GET); break;
            //    case OPERATIONS.POST: rest = new RestRequest(endpoint, Method.POST); break;
            //    case OPERATIONS.DELETE: rest = new RestRequest(endpoint, Method.DELETE); break;
            //}

            //rest.RequestFormat = DataFormat.Json;
            //rest.AddHeader("Content-Type", "application/json");

            //String data = (request != null) ? request.ToString() : "";
            //rest.AddParameter("application/json", data, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(rest);
            //JsonTextReader jsreader = new JsonTextReader(new StringReader(response.Content));

            //try
            //{
            //    return new JsonSerializer().Deserialize(jsreader);
            //}
            //catch (JsonReaderException jx)
            //{
            //    return null;
            //}
        }



    }
}
