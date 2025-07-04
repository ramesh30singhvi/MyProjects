using CPReservationApi.DAL;
using CPReservationApi.Model;
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

namespace CPReservationApi.WebApi.Services
{
    public class Cybersource
    {
        static private ViewModels.AppSettings _appSettings;
        private static string _baseURL = "apitest.cybersource.com";
        private static string _errorResponse = "";
        public Cybersource(IOptions<ViewModels.AppSettings> settings)
        {
            _appSettings = settings.Value;
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
                var keyRequest = MakeGenerateKeyRequest(pcfg, testMode);

                if (keyRequest != null && !string.IsNullOrWhiteSpace(keyRequest.KeyId))
                {
                    var requestObj = new TokenizeRequest
                        (
                            KeyId: keyRequest.KeyId,
                            CardInfo: new Flexv1tokensCardInfo
                            (
                                CardExpirationYear: request.exp_year,
                                CardNumber: request.number,
                                CardType: Payments.GetCardType(request.number, "cybersource"),
                                CardExpirationMonth: request.exp_month
                            )
                        );
                    var configDictionary = GetConfiguration(pcfg, testMode);
                    var clientConfig = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);

                    var apiInstance = new TokenizationApi(clientConfig);

                    logDAL.InsertLog("WebApi", "Cybersource.TokenziedCard: Request:" + JsonConvert.SerializeObject(requestObj), "", 3,request.member_id);

                    var tokenResp = apiInstance.Tokenize(requestObj);

                    logDAL.InsertLog("WebApi", "Cybersource.TokenziedCard: Response:" + JsonConvert.SerializeObject(tokenResp), "", 3,request.member_id);

                    resp = new TokenizedCard
                    {
                        card_token = tokenResp.Token,
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
                logDAL.InsertLog("WebApi", "Cybersource::Tokenized Card:  " + ex.Message.ToString(), "",1,request.member_id);
            }
            return resp;
        }

        private static FlexV1KeysPost200Response MakeGenerateKeyRequest(Configuration pcfg, bool isTestMode)
        {
            var requestObj = new GeneratePublicKeyRequest("None");
            var configDictionary = GetConfiguration(pcfg, isTestMode);
            var clientConfig = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);
            var apiInstance = new KeyGenerationApi(clientConfig);
            FlexV1KeysPost200Response result = null;
            try
            {
                result = apiInstance.GeneratePublicKey(requestObj);
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "Cybersource::Tokenized Card:  " + ex.Message.ToString(), "",1,0);
            }
            return result;
        }

        private static Dictionary<string, string> GetConfiguration(Configuration pcfg, bool isTestMode)
        {
            Dictionary<string, string> configurationDict = new Dictionary<string, string>();
            configurationDict.Add("authenticationType", "HTTP_SIGNATURE");
            configurationDict.Add("merchantID", pcfg.MerchantLogin);
            configurationDict.Add("merchantsecretKey", pcfg.MerchantPassword);
            configurationDict.Add("merchantKeyId",pcfg.UserConfig2);
            configurationDict.Add("keysDirectory", "Resource");
            configurationDict.Add("keyFilename", pcfg.MerchantLogin);
            if(isTestMode)
                configurationDict.Add("runEnvironment", "cybersource.environment.sandbox");
            else
                configurationDict.Add("runEnvironment", "cybersource.environment.production");
            configurationDict.Add("keyAlias", "testrest");
            configurationDict.Add("keyPass", "testrest");
            configurationDict.Add("enableLog", "FALSE");
            configurationDict.Add("logDirectory", string.Empty);
            configurationDict.Add("logFileName", string.Empty);
            configurationDict.Add("logFileMaxSize", "5242880");
            configurationDict.Add("timeout", "300000");
            configurationDict.Add("proxyAddress", string.Empty);
            configurationDict.Add("proxyPort", string.Empty);

            return configurationDict;
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
                    _baseURL = "api.cybersource.com";

                var postData = new { encryptionType = "None" };
                string reqURL = "https://" + _baseURL;
                switch (payment.Type)
                {
                    case Payments.Transaction1.ChargeType.Sale:
                        ProcessSaleAuthCard(invoiceId, payment, pcfg, true, ref pr, testMode);
                        break;
                    case Payments.Transaction1.ChargeType.AuthOnly:
                        ProcessSaleAuthCard(invoiceId, payment, pcfg, false, ref pr, testMode);
                        break;
                    case Payments.Transaction1.ChargeType.Credit:
                        ProcessRefund(invoiceId, payment, pcfg, "refunds", ref pr, testMode);
                        break;
                    case Payments.Transaction1.ChargeType.Void:
                        ProcessVoid(invoiceId, payment, pcfg, "voids", ref pr, testMode);
                        break;
                }
                pr.PaymentGateway = Configuration.Gateway.Cybersource;
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
                logDAL.InsertLog("Cybersource.ProcessCreditCard", "MemberId:" + wineryID.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "",1,wineryID);
            }
            return pr;
        }

        private static CreatePaymentRequest CreateRequestForCharge(int invoiceId, Payments.Transaction1 payment, bool captureCard)
        {
            if (!string.IsNullOrWhiteSpace(payment.Card.CardToken))
            {
                var processingInformationObj = new Ptsv2paymentsProcessingInformation() { CommerceIndicator = "internet" };

                var clientReferenceInformationObj = new Ptsv2paymentsClientReferenceInformation { Code = invoiceId.ToString() };

                var orderInformationObj = new Ptsv2paymentsOrderInformation();
                var data = new CreatePaymentRequest
                {
                    ClientReferenceInformation = new Ptsv2paymentsClientReferenceInformation
                    {
                        Code = invoiceId.ToString()
                    },
                    ProcessingInformation = new Ptsv2paymentsProcessingInformation
                    {
                        Capture = captureCard,
                        CommerceIndicator = "internet"
                    },
                    PaymentInformation = new Ptsv2paymentsPaymentInformation
                    {
                        Customer = new Ptsv2paymentsPaymentInformationCustomer
                        {
                            CustomerId = payment.Card.CardToken
                        }
                    },

                    OrderInformation = new Ptsv2paymentsOrderInformation
                    {
                        AmountDetails = new Ptsv2paymentsOrderInformationAmountDetails
                        {
                            TotalAmount = Math.Round(payment.Amount, 2).ToString(),
                            Currency = "USD"
                        },
                        BillTo = new Ptsv2paymentsOrderInformationBillTo
                        {
                            FirstName = payment.User.first_name,
                            LastName = payment.User.last_name,
                            Address1 = payment.User.address.address_1,
                            Address2 = payment.User.address.address_2,
                            Locality = payment.User.address.city,
                            AdministrativeArea = payment.User.address.state,
                            PostalCode = payment.User.address.zip_code,
                            Country = payment.User.address.country,
                            Email = payment.User.email,
                            PhoneNumber = payment.User.phone_number
                        }
                    },
                };
                return data;
            }
            else
            {
                var data = new CreatePaymentRequest
                {
                    ClientReferenceInformation = new Ptsv2paymentsClientReferenceInformation
                    {
                        Code = invoiceId.ToString()
                    },
                    ProcessingInformation = new Ptsv2paymentsProcessingInformation
                    {
                        CommerceIndicator = "internet"
                    },
                    PaymentInformation = new Ptsv2paymentsPaymentInformation
                    {
                        Card = new Ptsv2paymentsPaymentInformationCard
                        {
                            Number = payment.Card.Number,
                            ExpirationMonth = payment.Card.ExpMonth,
                            ExpirationYear = payment.Card.ExpYear,
                            SecurityCode = payment.Card.CVV
                        }
                    },

                    OrderInformation = new Ptsv2paymentsOrderInformation
                    {
                        AmountDetails = new Ptsv2paymentsOrderInformationAmountDetails
                        {
                            TotalAmount = Math.Round(payment.Amount, 2).ToString(),
                            Currency = "USD"
                        },
                        BillTo = new Ptsv2paymentsOrderInformationBillTo
                        {
                            FirstName = payment.User.first_name,
                            LastName = payment.User.last_name,
                            Address1 = payment.User.address.address_1,
                            Address2 = payment.User.address.address_2,
                            Locality = payment.User.address.city,
                            AdministrativeArea = payment.User.address.state,
                            PostalCode = payment.User.address.zip_code,
                            Country = payment.User.address.country,
                            Email = payment.User.email,
                            PhoneNumber = payment.User.phone_number
                        }
                    },
                };
                return data;
            }
        }

        private static void ProcessSaleAuthCard(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, bool captureCard, ref TransactionResult pr, bool isTestMode = false)
        {
            var requestObj = CreateRequestForCharge(invoiceId, payment, captureCard);
            var configDictionary = GetConfiguration(pcfg, isTestMode);
            var clientConfig = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);
            var apiInstance = new PaymentsApi(clientConfig);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            try
            {
                logDAL.InsertLog("WebApi", "Cybersource.ProcessSaleAuthCard: Request:" + JsonConvert.SerializeObject(requestObj), "", 3,payment.WineryId);

                var result = apiInstance.CreatePayment(requestObj);

                logDAL.InsertLog("WebApi", "Cybersource.ProcessSaleAuthCard: Response:" + JsonConvert.SerializeObject(result), "", 3,payment.WineryId);

                if (result != null)
                {
                    if (result.ErrorInformation != null && !string.IsNullOrWhiteSpace(result.ErrorInformation.Reason))
                    {
                        pr.Status = TransactionResult.StatusType.Failed;
                        pr.Detail = result.ErrorInformation.Message;
                        pr.ResponseCode = result.ErrorInformation.Reason;

                        logDAL.InsertLog("WebApi", "Cybersource.ProcessCreditCard: Response:" + JsonConvert.SerializeObject(result.ErrorInformation), "", 3,payment.WineryId);
                    }
                    else
                    {
                        pr.Status = TransactionResult.StatusType.Success;
                        pr.TransactionID = result.Id;
                        pr.ResponseCode = result.ProcessorInformation.ResponseCode;
                        pr.ApprovalCode = result.ProcessorInformation.ApprovalCode;
                    }
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Unknown error from gateway";
                    pr.ResponseCode = "";

                    logDAL.InsertLog("WebApi", "Cybersource.ProcessCreditCard: Request:" + JsonConvert.SerializeObject(requestObj) + ",Unknown error from gateway", "", 3,payment.WineryId);
                }
            }
            catch (Exception ex)
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";

                logDAL.InsertLog("Cybersource.ProcessCreditCard", "MemberId:" + payment.WineryId.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "",1,payment.WineryId);
            }
        }

        private static void ProcessVoid(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, string action, ref TransactionResult pr, bool isTestMode = false)
        {
            var requestObj = new VoidPaymentRequest
            {
                ClientReferenceInformation = new Ptsv2paymentsidreversalsClientReferenceInformation(invoiceId.ToString())
            };
            var configDictionary = GetConfiguration(pcfg, isTestMode);
            var clientConfig = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);
            var apiInstance = new VoidApi(clientConfig);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            logDAL.InsertLog("WebApi", "Cybersource.ProcessVoid: Request:" + JsonConvert.SerializeObject(requestObj) + ",TransactionID:" + payment.TransactionID, "", 3,payment.WineryId);

            var result = apiInstance.VoidPayment(requestObj, payment.TransactionID);

            logDAL.InsertLog("WebApi", "Cybersource.ProcessVoid: Response:" + JsonConvert.SerializeObject(result) + ",TransactionID:" + payment.TransactionID, "", 3,payment.WineryId);

            if (result != null)
            {
                if (result.VoidAmountDetails == null || string.IsNullOrWhiteSpace(result.VoidAmountDetails.VoidAmount))
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = result.Status;
                    pr.ResponseCode = "";
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = result.Id;
                }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";
            }
        }

        private static void ProcessRefund(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, string action, ref TransactionResult pr, bool isTestMode = false)
        {
            var requestObj = new RefundPaymentRequest
            {
                ClientReferenceInformation = new Ptsv2paymentsClientReferenceInformation(invoiceId.ToString()),
                OrderInformation = new Ptsv2paymentsidrefundsOrderInformation
                {
                    AmountDetails = new Ptsv2paymentsidcapturesOrderInformationAmountDetails
                    {
                        TotalAmount = Math.Round(payment.Amount, 2).ToString(),
                        Currency = "USD"
                    }
                }
            };
            var configDictionary = GetConfiguration(pcfg, isTestMode);
            var clientConfig = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);
            var apiInstance = new RefundApi(clientConfig);

            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            logDAL.InsertLog("WebApi", "Cybersource.ProcessRefund: Request:" + JsonConvert.SerializeObject(requestObj) + ",TransactionID:" + payment.TransactionID, "", 3,payment.WineryId);

            var result = apiInstance.RefundPayment(requestObj, payment.TransactionID);

            logDAL.InsertLog("WebApi", "Cybersource.ProcessRefund: Response:" + JsonConvert.SerializeObject(result) + ",TransactionID:" + payment.TransactionID, "", 3,payment.WineryId);

            if (result != null)
            {
                if (result.RefundAmountDetails == null || string.IsNullOrWhiteSpace(result.RefundAmountDetails.RefundAmount))
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = result.Status;
                    pr.ResponseCode = "";
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = result.Id;
                }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";

            }
        }

        #region Supporting Methods
        public static string MakePostRequestCyberSource(string url, Configuration pcfg, object param)
        {
            string jsonResp = string.Empty;
            _errorResponse = "";

            HttpClient client = new HttpClient();

            string requestObj = JsonConvert.SerializeObject(param);
            AddHeaders(ref client, pcfg, requestObj, url);

            using (var content = new StringContent(requestObj))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); // content-type header
                client.Timeout = TimeSpan.FromMinutes(5);
                url = "https://" + _baseURL + url;
                var response = client.PostAsync(new Uri(url), content).Result;

                if (response.IsSuccessStatusCode)
                    jsonResp = response.Content.ReadAsStringAsync().Result;
                else
                {
                    _errorResponse = response.Content.ReadAsStringAsync().Result;

                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi::MakePostRequestCyberSource", "MerchantLogin:" + pcfg.MerchantLogin + ", Response error:" + _errorResponse, "",1,0);
                }
            }


            return jsonResp;
        }


        private static void AddHeaders(ref HttpClient client, Configuration pcfg, string request, string resource)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("v-c-merchant-id", pcfg.MerchantLogin);
            /* Add Request Header :: "Date" The date and time that the message was originated from.
             * "HTTP-date" format as defined by RFC7231. */
            String gmtDateTime = DateTime.Now.ToUniversalTime().ToString("r");
            client.DefaultRequestHeaders.Add("Date", gmtDateTime);

            /* Add Request Header :: "Host"
            * Sandbox Host: apitest.cybersource.com
            * Production Host: api.cybersource.com */
            client.DefaultRequestHeaders.Add("Host", _baseURL);


            /* Add Request Header :: "Digest"
             * Digest is SHA-256 hash of payload that is BASE64 encoded */
            var digest = GenerateDigest(request);
            client.DefaultRequestHeaders.Add("Digest", digest);


            /* Add Request Header :: "Signature"
             * Signature header contains keyId, algorithm, headers and signature as paramters
             * method getSignatureHeader() has more details */
            StringBuilder signature = GenerateSignature(request, digest, "", gmtDateTime, "post", resource, pcfg);
            client.DefaultRequestHeaders.Add("Signature", signature.ToString());
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }


        /* This method returns value for paramter Signature which is then passed to Signature header
                 * paramter 'Signature' is calucated based on below key values and then signed with SECRET KEY -
                 * host: Sandbox (apitest.cybersource.com) or Production (api.cybersource.com) hostname
                 * date: "HTTP-date" format as defined by RFC7231.
                 * (request-target): Should be in format of httpMethod: path
                      Example: "post /pts/v2/payments"
                 * Digest: Only needed for POST calls.
                      digestString = BASE64( HMAC-SHA256 ( Payload ));
                      Digest: “SHA-256=“ + digestString;
                 * v-c-merchant-id: set value to Cybersource Merchant ID
                      This ID can be found on EBC portal*/

        static StringBuilder GenerateSignature(String request, String digest, String keyid, String gmtDateTime, String method, string resource, Configuration pcfg)
        {
            StringBuilder signatureHeaderValue = new StringBuilder();
            String algorithm = "HmacSHA256";
            String postHeaders = "host date (request-target) digest v-c-merchant-id";
            String getHeaders = "host date (request-target) v-c-merchant-id";
            String url = "https://" + _baseURL + resource;
            String getRequestTarget = method + " " + resource;
            String postRequestTarget = method + " " + resource;

            try
            {

                // Generate the Signature       

                StringBuilder signatureString = new StringBuilder();
                signatureString.Append('\n');
                signatureString.Append("host");
                signatureString.Append(": ");
                signatureString.Append(_baseURL);
                signatureString.Append('\n');
                signatureString.Append("date");
                signatureString.Append(": ");
                signatureString.Append(gmtDateTime);
                signatureString.Append('\n');
                signatureString.Append("(request-target)");
                signatureString.Append(": ");
                if (method.Equals("post"))
                {
                    signatureString.Append(postRequestTarget);
                    signatureString.Append('\n');
                    signatureString.Append("digest");
                    signatureString.Append(": ");
                    signatureString.Append(digest);
                }
                else
                    signatureString.Append(getRequestTarget);
                signatureString.Append('\n');
                signatureString.Append("v-c-merchant-id");
                signatureString.Append(": ");
                signatureString.Append(pcfg.MerchantLogin);
                signatureString.Remove(0, 1);

                byte[] signatureByteString = System.Text.Encoding.UTF8.GetBytes(signatureString.ToString());

                byte[] decodedKey = Convert.FromBase64String(pcfg.MerchantPassword);

                HMACSHA256 aKeyId = new HMACSHA256(decodedKey);

                byte[] hashmessage = aKeyId.ComputeHash(signatureByteString);
                String base64EncodedSignature = Convert.ToBase64String(hashmessage);

                signatureHeaderValue.Append("keyid=\"" + pcfg.UserConfig2 + "\"");
                signatureHeaderValue.Append(", algorithm=\"" + algorithm + "\"");
                if (method.Equals("post"))
                {
                    signatureHeaderValue.Append(", headers=\"" + postHeaders + "\"");
                }
                else if (method.Equals("get"))
                {
                    signatureHeaderValue.Append(", headers=\"" + getHeaders + "\"");
                }
                signatureHeaderValue.Append(", signature=\"" + base64EncodedSignature + "\"");


                // Writing Generated Token to file.    
                //File.WriteAllText("..\\signatureHeaderValue.txt", signatureHeaderValue.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Oops : " + ex.ToString());
            }

            return signatureHeaderValue;
        }

        static String GenerateDigest(String request)
        {
            String digest = "DIGEST_PLACEHOLDER";

            try
            {
                // Generate the Digest 
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] payloadBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(request));
                    digest = Convert.ToBase64String(payloadBytes);
                    digest = "SHA-256=" + digest;
                }

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Oops : " + ex.ToString());
            }

            return digest;
        }

        #endregion
    }
}
