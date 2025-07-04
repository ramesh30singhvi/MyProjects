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
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CPReservationApi.WebApi.Services
{
    public class Zeamster
    {
        static private AppSettings _appSettings;
        public Zeamster(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
        }

        const string LiveUrl = "https://api.zeamster.com";
        const string TestUrl = "https://api.sandbox.zeamster.com";
        const string ACCT_VAULT_ENDPOINT = "v2/accountvaults";
        const string TRANS_ENDPOINT = "v2/transactions";

        #region Classes For various Requests

        public class AccountvaultModel
        {
            public string title { get; set; }
            public string payment_method { get; set; }
            public string account_holder_name { get; set; }
            public string account_number { get; set; }
            public string exp_date { get; set; }
            public string location_id { get; set; }
            public string contact_id { get; set; }
            public string billing_zip { get; set; }
            public bool run_avs { get; set; } = true;

        }

        public class AccountValutRequest
        {
            public AccountvaultModel accountvault { get; set; }
        }

        public class Self
        {
            public string href { get; set; }
        }

        public class Links
        {
            public Self self { get; set; }
        }

        public class AccountvaultResponseModel
        {
            public string id { get; set; }
            public string payment_method { get; set; }
            public string title { get; set; }
            public string account_holder_name { get; set; }
            public string first_six { get; set; }
            public string last_four { get; set; }
            public string billing_address { get; set; }
            public string billing_zip { get; set; }
            public string exp_date { get; set; }
            public string routing { get; set; }
            public string account_type { get; set; }
            public int created_ts { get; set; }
            public int modified_ts { get; set; }
            public string account_vault_api_id { get; set; }
            public string contact_id { get; set; }
            public string location_id { get; set; }
            public int expiring_in_months { get; set; }
            public bool has_recurring { get; set; }
            public string accountvault_c1 { get; set; }
            public string accountvault_c2 { get; set; }
            public string accountvault_c3 { get; set; }
            public string active { get; set; }
            public string ach_sec_code { get; set; }
            public string dl_number { get; set; }
            public string dl_state { get; set; }
            public string ssn4 { get; set; }
            public string dob_year { get; set; }
            public string billing_state { get; set; }
            public string billing_city { get; set; }
            public string billing_phone { get; set; }
            public string customer_id { get; set; }
            public int cau_summary_status_id { get; set; }
            public object cau_last_updated_ts { get; set; }
            public Links _links { get; set; }
        }

        public class AccountvaultResponse
        {
            public AccountvaultResponseModel accountvault { get; set; }
        }

        public class ContactModel
        {
            public string location_id { get; set; }
            public string account_number { get; set; }
            public string contact_api_id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string cell_phone { get; set; }
            public string contact_balance { get; set; }
            public string email { get; set; }
        }

        public class AddContactRequest
        {
            public ContactModel contact { get; set; }
        }

        public class ZeamsterTransaction
        {
            public string action { get; set; }
            public string payment_method { get; set; }
            public string account_vault_id { get; set; }
            public string transaction_amount { get; set; }
            public string location_id { get; set; }
            public string account_number { get; set; }

            public string exp_date { get; set; }

            public string account_holder_name { get; set; }

            public string order_num { get; set; }

            public string previous_transaction_id { get; set; }
            public string billing_street { get; set; }
            //public string billing_city { get; set; }
            public string billing_zip { get; set; }


        }

        public class ZeamsterTransRequest
        {
            public ZeamsterTransaction transaction { get; set; }
        }
        #endregion
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
            string postURL = LiveUrl;

            //If test mode, use test url
            if (testMode)
            {
                postURL = TestUrl;
            }
            string firstname = "", lastname = "", email = "", zipcode = "", address1 = "";

            if (request.user_info != null)
            {
                firstname = request.user_info.last_name;
                lastname = request.user_info.first_name;
                email = request.user_info.email;
                if (request.user_info.address != null)
                {
                    zipcode = request.user_info.address.zip_code;
                    address1 = request.user_info.address.address_1;
                }


            }
            else if (request.user_id > 0)
            {
                UserDAL dal = new UserDAL(Common.Common.ConnectionString);
                var userObj = dal.GetUserById(request.user_id);
                firstname = userObj.last_name;
                lastname = userObj.first_name;
                email = userObj.email;
                if (userObj.address != null)
                {
                    zipcode = userObj.address.zip_code;
                    address1 = userObj.address.address_1;
                }
            }
            else
            {
                Common.StringHelpers.ParseCustomerName(request.cust_name, ref firstname, ref lastname);
                email = firstname + "." + lastname + "@noemail.com";
            }

            if (testMode)
            {
                logDAL.InsertLog("Zeamster::TokenizedCard", "Request Rcvd:" + JsonConvert.SerializeObject(request), "WebApi", 3, request.member_id);
            }

            string custId = CheckAndCreateCustomer(firstname, lastname, email, pcfg, request.member_id);

            string expDate = request.exp_month + Common.Common.Right(request.exp_year, 2);
            string last4ccDigits = Common.Common.Right(request.number, 4);
            bool avsCheckRequired = false;
            string accountValutId = CheckAndGetAccountVaultId(custId, last4ccDigits, expDate, zipcode, address1, pcfg, ref avsCheckRequired);

            if (!string.IsNullOrWhiteSpace(accountValutId))
            {
                if (avsCheckRequired)
                {
                    // do AVS check
                    string avsCheck = "GOOD";
                    avsCheck = PerformAVSCheck(accountValutId, address1, zipcode, testMode, pcfg);

                    if (avsCheck.ToUpper().Equals("GOOD"))
                    {
                        //update acct vault data
                        UpdateAccountVaultData(postURL, accountValutId, address1, zipcode, request.member_id, pcfg);
                        resp = new TokenizedCard
                        {
                            card_token = accountValutId,
                            customer_name = request.cust_name,
                            last_four_digits = last4ccDigits,
                            first_four_digits = Common.Common.Left(request.number, 4),
                            is_expired = false,
                            cust_id = custId,
                            card_type = Payments.GetCardType(request.number),

                        };
                    }
                    else
                    {

                        string errMessage = "Both street and zip do not match.";
                        if (!request.ignore_avs_error)
                        {
                            switch (avsCheck.ToUpper())
                            {
                                case "BAD":
                                    errMessage = "Both street and zip do not match.";
                                    break;
                                case "STREET":
                                    errMessage = "Street does not match.";
                                    break;
                                case "ZIP":
                                    errMessage = "Zip does not match.";
                                    break;
                            }
                            resp = new TokenizedCard
                            {
                                ErrorMessage = "Error performing AVS Check. " + errMessage + ". Please enter valid address and zip."

                            };
                        }
                        else
                        {
                            resp = new TokenizedCard
                            {
                                card_token = "AVSFAILED" + avsCheck,
                                customer_name = request.cust_name,
                                last_four_digits = last4ccDigits,
                                first_four_digits = Common.Common.Left(request.number, 4),
                                is_expired = false,
                                cust_id = custId,
                                card_type = Payments.GetCardType(request.number),
                            };

                        }
                    }

                }
                else
                {
                    resp = new TokenizedCard
                    {
                        card_token = accountValutId,
                        customer_name = request.cust_name,
                        last_four_digits = last4ccDigits,
                        first_four_digits = Common.Common.Left(request.number, 4),
                        is_expired = false,
                        cust_id = custId,
                        card_type = Payments.GetCardType(request.number),

                    };
                }
            }
            else
            {

                //USAePayAPI.USAePay usaepay = new USAePayAPI.USAePay();

                AccountValutRequest ccModel = new AccountValutRequest();
                AccountvaultModel creditcardModel = new AccountvaultModel();

                //ccModel.command = "cc:save";
                creditcardModel.payment_method = "cc";
                creditcardModel.account_number = request.number;
                creditcardModel.account_holder_name = request.cust_name;
                creditcardModel.exp_date = request.exp_month + Common.Common.Right(request.exp_year, 2);
                creditcardModel.location_id = pcfg.UserConfig2;
                creditcardModel.title = "CP Customer";
                creditcardModel.contact_id = custId;
                creditcardModel.billing_zip = zipcode;
                creditcardModel.run_avs = true;

                ccModel.accountvault = creditcardModel;

                string payLoadContent = JsonConvert.SerializeObject(ccModel);


                if (testMode)
                {
                    logDAL.InsertLog("WebApi-Zeamster::AccountVaultAdd", "request:" + payLoadContent, "", 3);
                }


                HttpClient httpClient = GetZeamsterHttpClient(postURL, pcfg.MerchantLogin, pcfg.MerchantPassword, pcfg.UserConfig1);
                var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");


                using (var response = httpClient.PostAsync(string.Format("{0}/{1}", postURL, ACCT_VAULT_ENDPOINT), stringContent).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    if (testMode)
                    {
                        logDAL.InsertLog("WebApi-Zeamster::AccountVaultAdd", "response:" + varResponse, "", 3);
                    }
                    AccountvaultResponse ccDetails = JsonConvert.DeserializeObject<AccountvaultResponse>(varResponse);

                    if (ccDetails != null && ccDetails.accountvault != null)
                    {
                        resp = new TokenizedCard
                        {
                            card_token = ccDetails.accountvault.id,
                            customer_name = ccDetails.accountvault.account_holder_name,
                            last_four_digits = ccDetails.accountvault.last_four,
                            first_four_digits = Common.Common.Left(request.number, 4),
                            is_expired = false,
                            cust_id = custId,
                            card_type = Payments.GetCardType(request.number),

                        };
                    }
                    else
                    {
                        if (!request.ignore_avs_error)
                        { }
                        string errMessage = "";

                        if (varResponse.Contains("Account Verification failed"))
                        {
                            if (!request.ignore_avs_error)
                            {
                                errMessage = "Error performing AVS Check. Please enter valid address and zip.";
                                resp = new TokenizedCard
                                {
                                    ErrorMessage = errMessage

                                };
                            }
                            else
                            {
                                resp = new TokenizedCard
                                {
                                    card_token = "AVSFAILED",
                                    customer_name = request.cust_name,
                                    last_four_digits = last4ccDigits,
                                    first_four_digits = Common.Common.Left(request.number, 4),
                                    is_expired = false,
                                    cust_id = custId,
                                    card_type = Payments.GetCardType(request.number),
                                };

                            }
                        }
                        else
                        {
                            errMessage = "Invalid Credit Card.";
                            resp = new TokenizedCard
                            {
                                ErrorMessage = errMessage

                            };
                        }

                        try
                        {
                            zeamsterError zeamstererror = JsonConvert.DeserializeObject<zeamsterError>(varResponse);

                            if (zeamstererror == null || zeamstererror.message == null)
                            {
                                zeamsterError2Root zeamstererror2 = JsonConvert.DeserializeObject<zeamsterError2Root>(varResponse);

                                resp = new TokenizedCard();

                                if (zeamstererror2.errors.account_number != null)
                                {
                                    resp.ErrorMessage = zeamstererror2.errors.account_number[0];
                                }
                                else if (zeamstererror2.errors.billing_zip != null)
                                {
                                    resp.ErrorMessage = zeamstererror2.errors.billing_zip[0];
                                }
                                else if (zeamstererror2.errors.exp_date != null)
                                {
                                    resp.ErrorMessage = zeamstererror2.errors.exp_date[0];
                                }
                                else if (zeamstererror2.errors.location_id != null)
                                {
                                    resp.ErrorMessage = zeamstererror2.errors.location_id[0];
                                }
                            }
                            else
                            {
                                resp = new TokenizedCard
                                {
                                    ErrorMessage = zeamstererror.message,
                                    ErrorCode = zeamstererror.status.ToString()
                                };
                            }
                        }
                        catch { }
                        

                        logDAL.InsertLog("WebApi-zeamster::TokenziedCard", "Member Id:" + request.member_id.ToString() + ", Card Number:" + (testMode ? request.number : Common.Common.Right(request.number, 4)) + ", Error Response:" + varResponse, "", 1, request.member_id);
                    }
                }
                httpClient.Dispose();
            }
            return resp;
        }

        private static void UpdateAccountVaultData(string postURL, string acctValutId, string address1, string zipcode, int memberId, Configuration pcfg)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            string payLoadContent = "{\"accountvault\": {\"billing_address\": \"" + address1 + "\", \"billing_zip\": \"" + zipcode + "\"}}";

            HttpClient httpClient = GetZeamsterHttpClient(postURL, pcfg.MerchantLogin, pcfg.MerchantPassword, pcfg.UserConfig1);
            var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");

            try
            {
                using (var response = httpClient.PutAsync(string.Format("{0}/{1}/{2}", postURL, ACCT_VAULT_ENDPOINT, acctValutId), stringContent).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    AccountvaultResponse ccDetails = JsonConvert.DeserializeObject<AccountvaultResponse>(varResponse);

                    if (ccDetails != null && ccDetails.accountvault != null)
                    {
                        //all good
                    }
                    else
                    {
                        logDAL.InsertLog("WebApi-zeamster::UpdateAccountValutData", "Error updating account vault data. request:" + payLoadContent + ", Error:" + varResponse, "", 1, memberId);
                    }
                }
                httpClient.Dispose();
            }
            catch { }
        }
        private static string ParseExpiryDate(string expDate)
        {
            string ret = "";
            if (ret.Length > 2)
            {
                ret = expDate.Substring(0, expDate.Length - 2) + "/" + Common.Common.Right(expDate, 2);

            }
            else
            {
                ret = expDate;
            }
            return expDate;
        }

        public static List<TokenizedCard> GetTokenizedCardsByCustomer(string custId, Configuration pcfg, string lastFour = "", string expDate = "")
        {
            List<TokenizedCard> resp = new List<TokenizedCard>();
            bool testMode = false;
            if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }
            string postURL = LiveUrl;

            //If test mode, use test url
            if (testMode)
            {
                postURL = TestUrl;
            }
            HttpClient httpClient = GetZeamsterHttpClient(postURL, pcfg.MerchantLogin, pcfg.MerchantPassword, pcfg.UserConfig1);
            string query = "location_id=" + pcfg.UserConfig2.Trim() + "&payment_method=cc&contact_id=" + custId.Trim();
            if (!string.IsNullOrWhiteSpace(lastFour))
            {
                query += "&last_four=" + lastFour.Trim();
            }
            if (!string.IsNullOrWhiteSpace(expDate))
            {
                query += "&exp_date=" + expDate.Trim();
            }
            using (var response = httpClient.GetAsync(string.Format("{0}/{2}?{1}", postURL, query, ACCT_VAULT_ENDPOINT)).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                dynamic ccDetails = JObject.Parse(varResponse);

                if (ccDetails != null && ccDetails["meta"] != null && ccDetails["meta"]["pagination"] != null && (int)ccDetails["meta"]["pagination"]["totalCount"] > 0)
                {
                    JArray jarr = (JArray)ccDetails["accountvaults"];
                    for (int i = 0; i < jarr.Count; i++)
                    {
                        resp.Add(new TokenizedCard
                        {
                            card_token = Convert.ToString(jarr[i]["id"]),
                            card_type = Convert.ToString(jarr[i]["account_type"]),
                            customer_name = Convert.ToString(jarr[i]["account_holder_name"]),
                            cust_id = custId,
                            is_expired = Convert.ToInt32(jarr[i]["expiring_in_months"]) > 0 ? true : false,
                            last_four_digits = Convert.ToString(jarr[i]["last_four"]),
                            first_four_digits = "",
                            card_expiration = ParseExpiryDate(Convert.ToString(jarr[i]["exp_date"])),
                            address1 = Convert.ToString(jarr[i]["billing_address"]),
                            zip_code = Convert.ToString(jarr[i]["billing_zip"]),
                        });

                    }
                }
                //else
                //{

                //    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                //    logDAL.InsertLog("WebApi-zeamster::TokenziedCard", "Member Id:" + request.member_id.ToString() + ", Card Number:" + request.number + ", Error Response:" + varResponse, "", 1, request.member_id);
                //}
            }
            httpClient.Dispose();
            return resp;
        }

        private static string CheckAndCreateCustomer(string firstname, string lastname, string email, Configuration pcfg, int memberId)
        {
            string custId = "";
            bool testMode = false;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }
            string postURL = LiveUrl;

            //If test mode, use test url
            if (testMode)
            {
                postURL = TestUrl;
            }

            HttpClient httpClient = GetZeamsterHttpClient(postURL, pcfg.MerchantLogin, pcfg.MerchantPassword, pcfg.UserConfig1);

            string query = "location_id=" + pcfg.UserConfig2.Trim();

            if (!string.IsNullOrWhiteSpace(email))
            {
                query += "&email=" + email.Trim();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(lastname))
                {
                    query += "&last_name=" + lastname.Trim();
                }
                if (!string.IsNullOrWhiteSpace(firstname))
                {
                    query += "&first_name=" + firstname.Trim();
                }

            }
            using (var response = httpClient.GetAsync(string.Format("{0}/v2/contacts?{1}", postURL, query)).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode)
                {

                    dynamic contactsData = JObject.Parse(varResponse);

                    if (contactsData != null && contactsData["meta"] != null && contactsData["meta"]["pagination"] != null && (int)contactsData["meta"]["pagination"]["totalCount"] > 0)
                    {
                        JArray jarr = (JArray)contactsData["contacts"];
                        if (jarr != null)
                        {
                            custId = jarr[0]["id"].ToString();
                        }
                    }
                }
                else
                {
                   
                    logDAL.InsertLog("WebApi-Zeamster::checkandcreatecustomer", "data query:" + query + ", Error Response:" + varResponse, "", 1, memberId);

                }

            }
            if (string.IsNullOrWhiteSpace(custId))
            {
                //create the contact on Zeamster
                var contact = new AddContactRequest();
                contact.contact = new ContactModel
                {
                    location_id = pcfg.UserConfig2,
                    first_name = firstname,
                    last_name = lastname,
                    email = email

                };
                string payLoadContent = JsonConvert.SerializeObject(contact);
                if (testMode)
                {
                    logDAL.InsertLog("WebApi-Zeamster::CheckAndCreateCustomer", "request:" + payLoadContent, "", 3);
                }

                var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");
                using (var response = httpClient.PostAsync(string.Format("{0}/v2/contacts", postURL), stringContent).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    if (testMode)
                    {
                        logDAL.InsertLog("WebApi-Zeamster::CheckAndCreateCustomer", "response:" + varResponse, "", 3);
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        dynamic contactsData = JObject.Parse(varResponse);
                        if (contactsData != null && contactsData["contact"] != null)
                        {
                            custId = (string)contactsData["contact"]["id"];

                            if (!string.IsNullOrWhiteSpace(custId))
                            {
                                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                                userDAL.UpdateGatewayCustId(email, custId);
                            }
                        }
                    }
                    else
                    {
                        logDAL.InsertLog("WebApi-Zeamster::checkandcreatecustomer", "data:" + payLoadContent + ", Error Response:" + varResponse, "", 1);

                    }
                }

            }
            httpClient.Dispose();
            return custId;
        }


        private static string CheckAndGetAccountVaultId(string custId, string lastFour, string expDate, string zipCode, string address1, Configuration pcfg, ref bool avsCheckRequired)
        {
            string acctVaultId = "";
            address1 = address1 + "";
            zipCode = zipCode + "";
            var cardsMatched = GetTokenizedCardsByCustomer(custId, pcfg, lastFour, expDate);

            if (cardsMatched != null && cardsMatched.Count > 0)
            {
                var card = cardsMatched[0];
                acctVaultId = cardsMatched[0].card_token;
                if ((card.address1.ToLower().Trim() == address1.Trim() || string.IsNullOrWhiteSpace(address1)) && (card.zip_code == zipCode || string.IsNullOrWhiteSpace(zipCode)))
                {
                    avsCheckRequired = false;
                }
                else
                {
                    avsCheckRequired = true;
                }


            }
            return acctVaultId;
        }


        public static HttpClient GetZeamsterHttpClient(string postURL, string userId, string apiKey, string developerId)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(postURL);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("developer-id", developerId);
            httpClient.DefaultRequestHeaders.Add("user-id", userId);
            httpClient.DefaultRequestHeaders.Add("user-api-key", apiKey);

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
                    pr = AuthOnly(wineryID, invoiceId, pcfg, payment);
                }
                else if (payment.Type == Payments.Transaction1.ChargeType.Capture)
                {
                    pr = AuthOnlyComplete(wineryID, invoiceId, pcfg, payment);
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
                logDAL.InsertLog("Zeamster.ProcessCreditCard", "MemberId:" + wineryID.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "", 1, wineryID);
            }

            return pr;
        }

        private static string GetLastNodeJSONString(string jsonData)
        {
            string ret = jsonData;
            if (jsonData.IndexOf("{") > -1 && jsonData.IndexOf("[") > -1)
            {


                int indexQuoteStart = jsonData.IndexOf('[');
                int indexQuoteEnd = jsonData.IndexOf(']');

                ret = jsonData.Substring(indexQuoteStart + 1, indexQuoteEnd - indexQuoteStart - 1).Replace("\"", "");

            }

            return ret;
        }
        private static TransactionResult PerformTransaction(ZeamsterTransRequest request, bool testMode, Configuration pcfg, Payments.Transaction1 payment)
        {
            string payLoadContent = JsonConvert.SerializeObject(request);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);


            string postURL = LiveUrl;

            //If test mode, use test url
            if (testMode)
            {
                postURL = TestUrl;
                logDAL.InsertLog("WebApi-Zeamster::ProcessCreditCard", "request:" + payLoadContent, "", 3);
            }

            HttpClient httpClient = GetZeamsterHttpClient(postURL, pcfg.MerchantLogin, pcfg.MerchantPassword, pcfg.UserConfig1);
            var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");
            TransactionResult pr = new TransactionResult();
            pr.PaymentGateway = Configuration.Gateway.Zeamster;
            pr.PayType = Common.Common.PaymentType.CreditCard;
            pr.Amount = payment.Amount;
            pr.Card = payment.Card;
            pr.ProcessedBy = payment.ProcessedBy;
            pr.TransactionType = (Common.Payments.Transaction.ChargeType)payment.Type;

            string errMessage = "";
            try
            {
                request.transaction.account_vault_id = request.transaction.account_vault_id + "";
            }
            catch { }
            
            if (!string.IsNullOrWhiteSpace(request.transaction.account_vault_id) && request.transaction.account_vault_id.Contains("AVSFAILED"))
            {
                pr.Status = TransactionResult.StatusType.Failed;
                string avsCheck = request.transaction.account_vault_id.Replace("AVSFAILED", "");
                switch (avsCheck.ToUpper())
                {
                    case "BAD":
                        errMessage = "Both street and zip do not match.";
                        break;
                    case "STREET":
                        errMessage = "Street does not match.";
                        break;
                    case "ZIP":
                        errMessage = "Zip does not match.";
                        break;
                }
                pr.Detail = avsCheck.ToUpper() + " " + errMessage;

            }
            else
            {
                using (var response = httpClient.PostAsync(string.Format("{0}/{1}", postURL, TRANS_ENDPOINT), stringContent).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    if (testMode)
                    {
                        logDAL.InsertLog("WebApi-Zeamster::ProcessCreditCard", "response:" + varResponse, "", 3);
                    }

                    pr.Status = TransactionResult.StatusType.Failed;
                    if (response.IsSuccessStatusCode)
                    {
                        dynamic tranResponse = JObject.Parse(varResponse);

                        if (tranResponse != null && tranResponse["transaction"] != null && Convert.ToInt32(tranResponse["transaction"]["status_id"]) != 301)
                        {
                            var tranRespObj = tranResponse["transaction"];
                            pr.Status = TransactionResult.StatusType.Success;
                            if (!string.IsNullOrWhiteSpace((string)tranRespObj["reason_code_id"]))
                            {
                                pr.ResponseCode = (string)tranRespObj["reason_code_id"];
                            }
                            if (!string.IsNullOrWhiteSpace((string)tranRespObj["id"]))
                            {
                                pr.TransactionID = (string)tranRespObj["id"];
                            }
                            if (!string.IsNullOrWhiteSpace((string)tranRespObj["auth_code"]))
                            {
                                pr.ApprovalCode = (string)tranRespObj["auth_code"];
                            }
                            if(request.transaction.action != "refund")
                                pr.Detail = "GOOD Street or zip are both good";
                        }
                    }
                    else
                    {
                        pr.Detail =  GetLastNodeJSONString(varResponse);

                        logDAL.InsertLog("WebApi-Zeamster::ProcessCreditCard", "data:" + payLoadContent + ", Error Response:" + varResponse, "", 1);

                    }



                }
            }
            return pr;
        }

        public static TransactionResult Sale(int memberId, int invoiceId, Configuration pcfg, Payments.Transaction1 payment)
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

            ZeamsterTransRequest request = new ZeamsterTransRequest();
            ZeamsterTransaction tran = new ZeamsterTransaction();
            tran.action = "sale";
            tran.payment_method = "cc";
            tran.location_id = pcfg.UserConfig2;
            tran.transaction_amount = Math.Round(payment.Amount, 2).ToString();
            if (!string.IsNullOrEmpty(payment.Card.CardToken))
            {
                tran.account_vault_id = payment.Card.CardToken;
            }
            else
            {
                tran.account_holder_name = payment.Card.CustName;
                tran.account_number = payment.Card.Number;
                tran.exp_date = payment.Card.ExpMonth + Common.Common.Right(payment.Card.ExpYear, 2);
            }
            string InvoiceRev = "";
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            string bookingCode = eventDAL.GetBookingCodeByReservationID(invoiceId);
            bookingCode = eventDAL.GetBookingCodeByReservationID(invoiceId);
            if (!string.IsNullOrWhiteSpace(bookingCode))
            {
                InvoiceRev = string.Format("CP-{0}-{1}", memberId, bookingCode);
            }
            else
            {
                InvoiceRev = string.Format("CP-{0}-{1}", memberId, payment.CheckOrRefNumber);
            }
            tran.order_num = InvoiceRev;
            request.transaction = tran;
            return PerformTransaction(request, testMode, pcfg, payment);
        }

        public static TransactionResult AuthOnly(int memberId, int invoiceId, Configuration pcfg, Payments.Transaction1 payment)
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

            ZeamsterTransRequest request = new ZeamsterTransRequest();
            ZeamsterTransaction tran = new ZeamsterTransaction();
            tran.action = "authonly";
            tran.payment_method = "cc";
            tran.location_id = pcfg.UserConfig2;
            tran.transaction_amount = Math.Round(payment.Amount, 2).ToString();
            if (!string.IsNullOrEmpty(payment.Card.CardToken))
            {
                tran.account_vault_id = payment.Card.CardToken;
            }
            else
            {
                tran.account_holder_name = payment.Card.CustName;
                tran.account_number = payment.Card.Number;
                tran.exp_date = payment.Card.ExpMonth + Common.Common.Right(payment.Card.ExpYear, 2);
            }
            string InvoiceRev = "";
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            string bookingCode = eventDAL.GetBookingCodeByReservationID(invoiceId);
            bookingCode = eventDAL.GetBookingCodeByReservationID(invoiceId);
            if (!string.IsNullOrWhiteSpace(bookingCode))
            {
                InvoiceRev = string.Format("CP-{0}-{1}", memberId, bookingCode);
            }
            else
            {
                InvoiceRev = string.Format("CP-{0}-{1}", memberId, payment.CheckOrRefNumber);
            }
            tran.order_num = InvoiceRev;
            request.transaction = tran;
            return PerformTransaction(request, testMode, pcfg, payment);
        }

        public static TransactionResult AuthOnlyComplete(int memberId, int invoiceId, Configuration pcfg, Payments.Transaction1 payment)
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

            string postURL = LiveUrl;

            //If test mode, use test url
            if (testMode)
            {
                postURL = TestUrl;
            }
            string payLoadContent = "{ \"transaction\": { \"action\": \"authcomplete\", " + "\"transaction_amount\":\"" + Math.Round(payment.Amount, 2).ToString() + "\"}}";
            HttpClient httpClient = GetZeamsterHttpClient(postURL, pcfg.MerchantLogin, pcfg.MerchantPassword, pcfg.UserConfig1);
            var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");

            pr.PaymentGateway = Configuration.Gateway.Zeamster;
            pr.PayType = Common.Common.PaymentType.CreditCard;
            pr.Amount = payment.Amount;
            pr.Card = payment.Card;
            pr.ProcessedBy = payment.ProcessedBy;
            pr.TransactionType = (Common.Payments.Transaction.ChargeType)payment.Type;

            using (var response = httpClient.PostAsync(string.Format("{0}/{1}/{2}", postURL, TRANS_ENDPOINT, payment.TransactionID), stringContent).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                pr.Status = TransactionResult.StatusType.Failed;
                if (response.IsSuccessStatusCode)
                {
                    dynamic tranResponse = JObject.Parse(varResponse);

                    if (tranResponse != null && tranResponse["transaction"] != null && Convert.ToInt32(tranResponse["transaction"]["status_id"]) != 301)
                    {
                        var tranRespObj = tranResponse["transaction"];
                        pr.Status = TransactionResult.StatusType.Success;
                        if (!string.IsNullOrWhiteSpace((string)tranRespObj["reason_code_id"]))
                        {
                            pr.ResponseCode = (string)tranRespObj["reason_code_id"];
                        }
                        if (!string.IsNullOrWhiteSpace((string)tranRespObj["id"]))
                        {
                            pr.TransactionID = (string)tranRespObj["id"];
                        }
                        if (!string.IsNullOrWhiteSpace((string)tranRespObj["auth_code"]))
                        {
                            pr.ApprovalCode = (string)tranRespObj["auth_code"];
                        }

                    }
                }
                else
                {

                    pr.Detail = GetLastNodeJSONString(varResponse);
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi-Zeamster::ProcessCreditCard", "data:" + payLoadContent + ", Error Response:" + varResponse, "", 1);

                }

            }

            return pr;
        }

         public static TransactionResult Void(int memberId, int invoiceId, Configuration pcfg, Payments.Transaction1 payment)
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

            string postURL = LiveUrl;

            //If test mode, use test url
            if (testMode)
            {
                postURL = TestUrl;
            }
            string payLoadContent = "{ \"transaction\": { \"action\": \"void\" }}";
            HttpClient httpClient = GetZeamsterHttpClient(postURL, pcfg.MerchantLogin, pcfg.MerchantPassword, pcfg.UserConfig1);
            var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");

            pr.PaymentGateway = Configuration.Gateway.Zeamster;
            pr.PayType = Common.Common.PaymentType.CreditCard;
            pr.Amount = payment.Amount;
            pr.Card = payment.Card;
            pr.ProcessedBy = payment.ProcessedBy;
            pr.TransactionType = (Common.Payments.Transaction.ChargeType)payment.Type;

            using (var response = httpClient.PutAsync(string.Format("{0}/{1}/{2}", postURL, TRANS_ENDPOINT, payment.TransactionID), stringContent).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                pr.Status = TransactionResult.StatusType.Failed;
                if (response.IsSuccessStatusCode)
                {
                    dynamic tranResponse = JObject.Parse(varResponse);

                    if (tranResponse != null && tranResponse["transaction"] != null && Convert.ToInt32(tranResponse["transaction"]["status_id"]) != 301)
                    {
                        var tranRespObj = tranResponse["transaction"];
                        pr.Status = TransactionResult.StatusType.Success;
                        if (!string.IsNullOrWhiteSpace((string)tranRespObj["reason_code_id"]))
                        {
                            pr.ResponseCode = (string)tranRespObj["reason_code_id"];
                        }
                        if (!string.IsNullOrWhiteSpace((string)tranRespObj["id"]))
                        {
                            pr.TransactionID = (string)tranRespObj["id"];
                        }
                        if (!string.IsNullOrWhiteSpace((string)tranRespObj["auth_code"]))
                        {
                            pr.ApprovalCode = (string)tranRespObj["auth_code"];
                        }

                    }
                }
                else
                {

                    pr.Detail = GetLastNodeJSONString(varResponse);
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi-Zeamster::ProcessCreditCard", "data:" + payLoadContent + ", Error Response:" + varResponse, "", 1);

                }

            }

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
            ZeamsterTransRequest request = new ZeamsterTransRequest();
            ZeamsterTransaction tran = new ZeamsterTransaction();
            tran.action = "refund";
            tran.payment_method = "cc";
            tran.location_id = pcfg.UserConfig2;
            tran.transaction_amount = Math.Round(payment.Amount, 2).ToString();
            tran.previous_transaction_id = payment.TransactionID;

            request.transaction = tran;
            return PerformTransaction(request, testMode, pcfg, payment);

        }

        private static string PerformAVSCheck(string accountValutId, string address1, string zipcode, bool testMode, Configuration pcfg)
        {
            var avsRequest = new ZeamsterTransRequest();
            avsRequest.transaction = new ZeamsterTransaction
            {
                account_vault_id = accountValutId,
                action = "avsonly",
                billing_street = address1,
                billing_zip = zipcode,
                payment_method = "cc",
                location_id = pcfg.UserConfig2

            };
            string payLoadContent = JsonConvert.SerializeObject(avsRequest);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            string ret = "BAD";

            string postURL = LiveUrl;

            //If test mode, use test url
            if (testMode)
            {
                postURL = TestUrl;
                logDAL.InsertLog("WebApi-Zeamster::PerformAVSCheck", "request:" + payLoadContent, "", 3);
            }

            HttpClient httpClient = GetZeamsterHttpClient(postURL, pcfg.MerchantLogin, pcfg.MerchantPassword, pcfg.UserConfig1);
            var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");

            using (var response = httpClient.PostAsync(string.Format("{0}/{1}", postURL, TRANS_ENDPOINT), stringContent).Result)
            {
                var varResponse = response.Content.ReadAsStringAsync().Result;
                if (testMode)
                {
                    logDAL.InsertLog("WebApi-Zeamster::PerformAVSCheck", "response:" + varResponse, "", 3);
                }

                if (response.IsSuccessStatusCode)
                {
                    dynamic tranResponse = JObject.Parse(varResponse);

                    if (tranResponse != null && tranResponse["transaction"] != null && Convert.ToInt32(tranResponse["transaction"]["status_id"]) != 301)
                    {
                        var tranRespObj = tranResponse["transaction"];
                        ret = (string)tranRespObj["avs"]; ;

                    }
                }
                else
                {
                    logDAL.InsertLog("WebApi-Zeamster::PerformAVSCheck", "data:" + payLoadContent + ", Error Response:" + varResponse, "", 1);

                }
            }

            return ret;
        }

    }
}
