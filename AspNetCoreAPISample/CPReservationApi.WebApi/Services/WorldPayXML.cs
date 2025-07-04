using CPReservationApi.DAL;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Xml.Linq;
using static CPReservationApi.Common.Payments;

namespace CPReservationApi.WebApi.Services
{
    public class WorldPayXML
    {
        static private AppSettings _appSettings;
        public WorldPayXML(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
        }

        const string LiveUrlServices = "https://transaction.elementexpress.com/";
        const string TestUrlServices = "https://certtransaction.elementexpress.com/";

        const string LiveUrlPASS = "https://services.elementexpress.com/";
        const string TestUrlPASS = "https://certservices.elementexpress.com/";

        public static TokenizedCard TokenziedCard(TokenizedCardRequest cardRequest, Configuration pcfg)
        {
            TokenizedCard resp = null;
            try
            {
                string URL = GetPASSRequestUrl();
                bool accountExists = false;
                string xmlRequest = GetXMLRequestPaymentAccountQuery(pcfg, cardRequest);
                var responseAcctQuery = MakeXMLHttpCall(xmlRequest, URL);

                if (!string.IsNullOrWhiteSpace(responseAcctQuery))
                {
                    XDocument doc = XDocument.Parse(responseAcctQuery);
                    string jsonText = JsonConvert.SerializeXNode(doc);
                    dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);

                    if (dyn.PaymentAccountQueryResponse.Response.ExpressResponseCode == "0")
                    {
                        accountExists = true;
                        resp = new TokenizedCard
                        {
                            card_token = dyn.PaymentAccountQueryResponse.Response.QueryData.Items.Item.PaymentAccountID,
                            customer_name = cardRequest.cust_name,
                            last_four_digits = Common.Common.Right(cardRequest.number, 4),
                            first_four_digits = Common.Common.Left(cardRequest.number, 4),
                            is_expired = false,
                            card_type = Payments.GetCardType(cardRequest.number)
                        };
                    }
                }

                if (!accountExists)
                {
                    xmlRequest = GetXMLRequestPaymentAccountCreate(pcfg, cardRequest);
                    var response = MakeXMLHttpCall(xmlRequest, URL);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        XDocument doc = XDocument.Parse(response);
                        string jsonText = JsonConvert.SerializeXNode(doc);
                        dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);

                        try
                        {
                            resp = new TokenizedCard
                            {
                                card_token = dyn.PaymentAccountCreateResponse.Response.PaymentAccount.PaymentAccountID,
                                customer_name = cardRequest.cust_name,
                                last_four_digits = Common.Common.Right(cardRequest.number, 4),
                                first_four_digits = Common.Common.Left(cardRequest.number, 4),
                                is_expired = false,
                                card_type = Payments.GetCardType(cardRequest.number)
                            };
                        }
                        catch
                        {
                            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                            logDAL.InsertLog("WebApi", "WorldPayXML::Tokenized Card:  Error tokenizing card. MemberId: " + cardRequest.member_id.ToString() + ", Error:" + response, "",1,cardRequest.member_id);
                        }
                    }
                }

                return resp;

            }
            catch (Exception ex)
            {

                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "WorldPayXML::Tokenized Card:  MemberId: " + cardRequest.member_id.ToString() + ", Error:" + ex.Message.ToString(), "",1,cardRequest.member_id);

            }
            return resp;

        }

        private static string GetPASSRequestUrl()
        {
            bool testMode = false;
            if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }
            if (testMode)
                return TestUrlPASS;
            else
                return LiveUrlPASS;
        }

        private static string GetServicesRequestUrl()
        {
            bool testMode = false;
            if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
            {
                if (_appSettings.PaymentTestMode == true)
                {
                    testMode = true;
                }
            }
            if (testMode)
                return TestUrlServices;
            else
                return LiveUrlServices;
        }

        private static string MakeXMLHttpCall(string payLoadContent, string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            byte[] bytes;
            bytes = System.Text.Encoding.ASCII.GetBytes(payLoadContent);
            request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();
                return responseStr;
            }
            else
            {
                return "";
            }

        }

        #region "XML Requests"
        public static string BuildHeader(Configuration pcfg)
        {
            string header = string.Format(@"	
                        <Credentials>
		                    <AccountID>{0}</AccountID>
		                    <AccountToken>{1}</AccountToken>
		                    <AcceptorID>{2}</AcceptorID>
	                    </Credentials>
	                    <Application>
		                    <ApplicationID>10390</ApplicationID>
		                    <ApplicationName>CellarPass</ApplicationName>
		                    <ApplicationVersion>1.0</ApplicationVersion>
	                    </Application>", pcfg.MerchantLogin, pcfg.MerchantPassword, pcfg.UserConfig1);

            return header;
        }

        public static string GetXMLRequestPaymentAccountCreate(Configuration pcfg, TokenizedCardRequest cardRequest)
        {
            string header = BuildHeader(pcfg);

            string year = (cardRequest.exp_year.Length > 2 ? cardRequest.exp_year.Substring(cardRequest.exp_year.Length - 2) : cardRequest.exp_year);
            string paymentRefNum = cardRequest.exp_month + year + cardRequest.number.Substring(0, 12);

            string xmlRequest = string.Format(@"
            <PaymentAccountCreate xmlns=""https://services.elementexpress.com"">
                {0}
	            <PaymentAccount>
		            <PaymentAccountType>0</PaymentAccountType>
		            <PaymentAccountReferenceNumber>{4}</PaymentAccountReferenceNumber>
	            </PaymentAccount>
	            <Card>
		            <CardNumber>{1}</CardNumber>
		            <ExpirationMonth>{2}</ExpirationMonth>
		            <ExpirationYear>{3}</ExpirationYear>
	            </Card>
            </PaymentAccountCreate>", header, cardRequest.number, cardRequest.exp_month, year, paymentRefNum);

            return xmlRequest;
        }

        public static string GetXMLRequestPaymentAccountDelete(Configuration pcfg, string accountId)
        {
            string header = BuildHeader(pcfg);

            string xmlRequest = string.Format(@"
            <PaymentAccountDelete xmlns=""https://services.elementexpress.com"">
                {0}
	            <PaymentAccount>
		            <PaymentAccountID>{1}</PaymentAccountID>
		            <PaymentAccountReferenceNumber>000001</PaymentAccountReferenceNumber>
	            </PaymentAccount>
            </PaymentAccountDelete>", header, accountId);

            return xmlRequest;
        }

        public static string GetXMLRequestPaymentAccountUpdate(Configuration pcfg, string accountId, TokenizedCardRequest cardRequest)
        {
            string header = BuildHeader(pcfg);

            string xmlRequest = string.Format(@"
            <PaymentAccountUpdate xmlns=""https://services.elementexpress.com"">
                {0}
	            <PaymentAccount>
		            <PaymentAccountID>{1}</PaymentAccountID>
		            <PaymentAccountType>0</PaymentAccountType>
		            <PaymentAccountReferenceNumber>000001</PaymentAccountReferenceNumber>
	            </PaymentAccount>
	            <Card>
		            <CardNumber>{2}</CardNumber>
		            <ExpirationMonth>{3}</ExpirationMonth>
		            <ExpirationYear>{4}</ExpirationYear>
	            </Card>
            </PaymentAccountUpdate>", header, accountId, cardRequest.number, cardRequest.exp_month, (cardRequest.exp_year.Length > 2 ? cardRequest.exp_year.Substring(cardRequest.exp_year.Length - 2) : cardRequest.exp_year));

            return xmlRequest;
        }

        public static string GetXMLRequestPaymentAccountQuery(Configuration pcfg, TokenizedCardRequest cardRequest)
        {
            string header = BuildHeader(pcfg);
            string year = (cardRequest.exp_year.Length > 2 ? cardRequest.exp_year.Substring(cardRequest.exp_year.Length - 2) : cardRequest.exp_year);
            string paymentRefNum = cardRequest.exp_month + year + cardRequest.number.Substring(0, 12);

            string xmlRequest = string.Format(@"
            <PaymentAccountQuery xmlns=""https://services.elementexpress.com"">
                {0}
	            <PaymentAccountParameters>
		            <PaymentAccountID></PaymentAccountID>
		            <PaymentAccountType>0</PaymentAccountType>
		            <PaymentAccountReferenceNumber>{3}</PaymentAccountReferenceNumber>
		            <PaymentBrand></PaymentBrand>
		            <ExpirationMonthBegin>{1}</ExpirationMonthBegin>
		            <ExpirationMonthEnd>{1}</ExpirationMonthEnd>
		            <ExpirationYearBegin>{2}</ExpirationYearBegin>
		            <ExpirationYearEnd>{2}</ExpirationYearEnd>
	            </PaymentAccountParameters>
            </PaymentAccountQuery>", header, cardRequest.exp_month, year, paymentRefNum);

            return xmlRequest;
        }

        public static string GetXMLRequestChargeCard(Configuration pcfg, int invoiceId, Payments.Transaction1 payment)
        {
            
            string header = BuildHeader(pcfg);
            string custName = "";
            string address1 = "";
            string address2 = "";
            string city = "";
            string userState = "";
            string zipCode = "";
            string phone = "";
            string email = "";

            if (payment.User != null)
            {
                custName = string.Format("{0} {1}", payment.User.first_name + "", payment.User.last_name);
                phone = payment.User.phone_number;
                email = payment.User.email;

                if (payment.User.address != null)
                {
                    address1 = payment.User.address.address_1 + "";
                    address2 = payment.User.address.address_2 + "";
                    city = payment.User.address.city + "";
                    userState = payment.User.address.state + "";
                    zipCode = payment.User.address.zip_code + "";
                }
            }
            string cvvPresentFlg = "1";
            //if (payment.Card != null && !string.IsNullOrWhiteSpace(payment.Card.CVV))
            //    cvvPresentFlg = "2";

            string xmlRequest = string.Format(@"
            <CreditCardSale xmlns=""https://transaction.elementexpress.com"">
                {0}
                <PaymentAccount>
	                <PaymentAccountID>{1}</PaymentAccountID>
                </PaymentAccount>
	            <Address>
		            <BillingName>{2}</BillingName>
		            <BillingEmail>{3}</BillingEmail>
		            <BillingPhone>{4}</BillingPhone>
		            <BillingAddress1>{5}</BillingAddress1>
		            <BillingAddress2>{6}</BillingAddress2>
		            <BillingCity>{7}</BillingCity>
		            <BillingState>{8}</BillingState>
		            <BillingZipcode>{9}</BillingZipcode>
		            <ShippingName></ShippingName>
		            <ShippingEmail></ShippingEmail>
		            <ShippingPhone></ShippingPhone>
		            <ShippingAddress1></ShippingAddress1>
		            <ShippingAddress2></ShippingAddress2>
		            <ShippingCity></ShippingCity>
		            <ShippingState></ShippingState>
		            <ShippingZipcode></ShippingZipcode>
		            <AddressEditAllowed></AddressEditAllowed>
	            </Address>
	            <Transaction>
		            <TransactionAmount>{10}</TransactionAmount>
		            <MarketCode>3</MarketCode>
		            <ReferenceNumber>{11}</ReferenceNumber>
		            <TicketNumber>{11}</TicketNumber>
		            <PartialApprovedFlag>0</PartialApprovedFlag>
		            <DuplicateCheckDisableFlag>0</DuplicateCheckDisableFlag>
		            <PaymentType>3</PaymentType>
		            <SubmissionType>1</SubmissionType>
		            <NetworkTransactionID></NetworkTransactionID>
		            <RecurringFlag>0</RecurringFlag>
	            </Transaction>
	            <Terminal>
		            <TerminalID>001</TerminalID>
		            <TerminalType>2</TerminalType>
		            <TerminalCapabilityCode>5</TerminalCapabilityCode>
		            <TerminalEnvironmentCode>6</TerminalEnvironmentCode>
		            <CardPresentCode>3</CardPresentCode>
		            <CVVPresenceCode>{12}</CVVPresenceCode>
		            <CardInputCode>4</CardInputCode>
		            <CardholderPresentCode>7</CardholderPresentCode>
		            <MotoECICode>7</MotoECICode>
	            </Terminal>
            </CreditCardSale>", header, payment.Card.CardToken, custName, email, phone, address1, address2, city, userState, zipCode, Math.Round(payment.Amount,2) , invoiceId, cvvPresentFlg);

            return xmlRequest;
        }

        public static string GetXMLRequestAuthorizeCard(Configuration pcfg, int invoiceId, Payments.Transaction1 payment)
        {

            string header = BuildHeader(pcfg);
            string custName = "";
            string address1 = "";
            string address2 = "";
            string city = "";
            string userState = "";
            string zipCode = "";
            string phone = "";
            string email = "";

            if (payment.User != null)
            {
                custName = string.Format("{0} {1}", payment.User.first_name + "", payment.User.last_name);
                phone = payment.User.phone_number;
                email = payment.User.email;

                if (payment.User.address != null)
                {
                    address1 = payment.User.address.address_1 + "";
                    address2 = payment.User.address.address_2 + "";
                    city = payment.User.address.city + "";
                    userState = payment.User.address.state + "";
                    zipCode = payment.User.address.zip_code + "";
                }
            }
            string cvvPresentFlg = "1";
            //if (payment.Card != null && !string.IsNullOrWhiteSpace(payment.Card.CVV))
            //    cvvPresentFlg = "2";

            string xmlRequest = string.Format(@"
            <CreditCardAuthorization xmlns=""https://transaction.elementexpress.com"">
                {0}
                <PaymentAccount>
	                <PaymentAccountID>{1}</PaymentAccountID>
                </PaymentAccount>
	            <Address>
		            <BillingName>{2}</BillingName>
		            <BillingEmail>{3}</BillingEmail>
		            <BillingPhone>{4}</BillingPhone>
		            <BillingAddress1>{5}</BillingAddress1>
		            <BillingAddress2>{6}</BillingAddress2>
		            <BillingCity>{7}</BillingCity>
		            <BillingState>{8}</BillingState>
		            <BillingZipcode>{9}</BillingZipcode>
		            <ShippingName></ShippingName>
		            <ShippingEmail></ShippingEmail>
		            <ShippingPhone></ShippingPhone>
		            <ShippingAddress1></ShippingAddress1>
		            <ShippingAddress2></ShippingAddress2>
		            <ShippingCity></ShippingCity>
		            <ShippingState></ShippingState>
		            <ShippingZipcode></ShippingZipcode>
		            <AddressEditAllowed></AddressEditAllowed>
	            </Address>
	            <Transaction>
		            <TransactionAmount>{10}</TransactionAmount>
		            <MarketCode>3</MarketCode>
		            <ReferenceNumber>{11}</ReferenceNumber>
		            <TicketNumber>{11}</TicketNumber>
		            <PartialApprovedFlag>0</PartialApprovedFlag>
		            <DuplicateCheckDisableFlag>0</DuplicateCheckDisableFlag>
		            <PaymentType>0</PaymentType>
		            <SubmissionType>0</SubmissionType>
		            <NetworkTransactionID></NetworkTransactionID>
		            <RecurringFlag>0</RecurringFlag>
	            </Transaction>
	            <Terminal>
		            <TerminalID>001</TerminalID>
		            <TerminalType>2</TerminalType>
		            <TerminalCapabilityCode>5</TerminalCapabilityCode>
		            <TerminalEnvironmentCode>6</TerminalEnvironmentCode>
		            <CardPresentCode>3</CardPresentCode>
		            <CVVPresenceCode>{12}</CVVPresenceCode>
		            <CardInputCode>4</CardInputCode>
		            <CardholderPresentCode>7</CardholderPresentCode>
		            <MotoECICode>7</MotoECICode>
	            </Terminal>
            </CreditCardAuthorization>", header, payment.Card.CardToken, custName, email, phone, address1, address2, city, userState, zipCode, Math.Round(payment.Amount, 2), invoiceId, cvvPresentFlg);

            return xmlRequest;
        }

        public static string GetXMLRequestAuthorizeCardComplete(Configuration pcfg, int invoiceId, Payments.Transaction1 payment)
        {

            string header = BuildHeader(pcfg);
            string custName = "";
            string address1 = "";
            string address2 = "";
            string city = "";
            string userState = "";
            string zipCode = "";
            string phone = "";
            string email = "";

            if (payment.User != null)
            {
                custName = string.Format("{0} {1}", payment.User.first_name + "", payment.User.last_name);
                phone = payment.User.phone_number;
                email = payment.User.email;

                if (payment.User.address != null)
                {
                    address1 = payment.User.address.address_1 + "";
                    address2 = payment.User.address.address_2 + "";
                    city = payment.User.address.city + "";
                    userState = payment.User.address.state + "";
                    zipCode = payment.User.address.zip_code + "";
                }
            }
            string cvvPresentFlg = "1";
            //if (payment.Card != null && !string.IsNullOrWhiteSpace(payment.Card.CVV))
            //    cvvPresentFlg = "2";

            string xmlRequest = string.Format(@"
            <CreditCardAuthorizationCompletion xmlns=""https://transaction.elementexpress.com"">
                {0}
	            <Transaction>
                    <TransactionID>{1}</TransactionID>
		            <TransactionAmount>{2}</TransactionAmount>
		            <MarketCode>3</MarketCode>
		            <ReferenceNumber>{3}</ReferenceNumber>
		            <TicketNumber>{3}</TicketNumber>
		            <PartialApprovedFlag>0</PartialApprovedFlag>
		            <DuplicateCheckDisableFlag>0</DuplicateCheckDisableFlag>
		            <PaymentType>0</PaymentType>
		            <SubmissionType>0</SubmissionType>
		            <NetworkTransactionID></NetworkTransactionID>
		            <RecurringFlag>0</RecurringFlag>
	            </Transaction>
	            <Terminal>
		            <TerminalID>001</TerminalID>
		            <TerminalType>2</TerminalType>
		            <TerminalCapabilityCode>5</TerminalCapabilityCode>
		            <TerminalEnvironmentCode>6</TerminalEnvironmentCode>
		            <CardPresentCode>3</CardPresentCode>
		            <CVVPresenceCode>{4}</CVVPresenceCode>
		            <CardInputCode>4</CardInputCode>
		            <CardholderPresentCode>7</CardholderPresentCode>
		            <MotoECICode>7</MotoECICode>
	            </Terminal>
            </CreditCardAuthorizationCompletion>", header, payment.TransactionID, Math.Round(payment.Amount, 2), invoiceId, cvvPresentFlg);

            return xmlRequest;
        }

        public static string GetXMLRequestVoidCard(Configuration pcfg, int invoiceId, Payments.Transaction1 payment)
        {

            string header = BuildHeader(pcfg);

            string xmlRequest = string.Format(@"
            <CreditCardVoid xmlns=""https://transaction.elementexpress.com"">
                {0}
	            <Transaction>
		            <TransactionID>{1}</TransactionID>
		            <TransactionAmount>{2}</TransactionAmount>
		            <ReferenceNumber>{3}</ReferenceNumber>
		            <TicketNumber>{3}</TicketNumber>
		            <MarketCode>3</MarketCode>
	            </Transaction>
	            <Terminal>
		            <TerminalID>0001</TerminalID>
		            <TerminalType>2</TerminalType>
		            <TerminalCapabilityCode>5</TerminalCapabilityCode>
		            <TerminalEnvironmentCode>6</TerminalEnvironmentCode>
		            <CardPresentCode>3</CardPresentCode>
		            <CVVPresenceCode>1</CVVPresenceCode>
		            <CardInputCode>4</CardInputCode>
		            <CardholderPresentCode>7</CardholderPresentCode>
		            <MotoECICode>7</MotoECICode>
	            </Terminal>
            </CreditCardVoid>", header, payment.TransactionID, Math.Round(payment.Amount,2), invoiceId);

            return xmlRequest;
        }

        public static string GetXMLRequestRefundCard(Configuration pcfg, int invoiceId, Payments.Transaction1 payment)
        {

            string header = BuildHeader(pcfg);

            string xmlRequest = string.Format(@"
            <CreditCardReturn xmlns=""https://transaction.elementexpress.com"">
                {0}
	            <Transaction>
		            <TransactionID>{1}</TransactionID>
		            <TransactionAmount>{2}</TransactionAmount>
		            <ReferenceNumber>{3}</ReferenceNumber>
		            <TicketNumber>{3}</TicketNumber>
		            <MarketCode>3</MarketCode>
	            </Transaction>
	            <Terminal>
		            <TerminalID>0001</TerminalID>
		            <TerminalType>2</TerminalType>
		            <TerminalCapabilityCode>5</TerminalCapabilityCode>
		            <TerminalEnvironmentCode>6</TerminalEnvironmentCode>
		            <CardPresentCode>3</CardPresentCode>
		            <CVVPresenceCode>1</CVVPresenceCode>
		            <CardInputCode>4</CardInputCode>
		            <CardholderPresentCode>7</CardholderPresentCode>
		            <MotoECICode>7</MotoECICode>
	            </Terminal>
            </CreditCardReturn>", header, payment.TransactionID, Math.Round(payment.Amount, 2), invoiceId);

            return xmlRequest;
        }

        public static string GetXMLRequestCardReversal(Configuration pcfg, int invoiceId, Payments.Transaction1 payment)
        {

            string header = BuildHeader(pcfg);

            string xmlRequest = string.Format(@"
            <CreditCardReversal xmlns=""https://transaction.elementexpress.com"">
                {0}
	            <Transaction>
                    <ReversalType>1</ReversalType>
		            <TransactionID>{1}</TransactionID>
		            <TransactionAmount>{2}</TransactionAmount>
		            <ReferenceNumber>{3}</ReferenceNumber>
		            <TicketNumber>{3}</TicketNumber>
		            <MarketCode>3</MarketCode>
	            </Transaction>
	            <Terminal>
		            <TerminalID>0001</TerminalID>
		            <TerminalType>2</TerminalType>
		            <TerminalCapabilityCode>5</TerminalCapabilityCode>
		            <TerminalEnvironmentCode>6</TerminalEnvironmentCode>
		            <CardPresentCode>3</CardPresentCode>
		            <CVVPresenceCode>1</CVVPresenceCode>
		            <CardInputCode>4</CardInputCode>
		            <CardholderPresentCode>7</CardholderPresentCode>
		            <MotoECICode>7</MotoECICode>
	            </Terminal>
            </CreditCardReversal>", header, payment.TransactionID, Math.Round(payment.Amount, 2), invoiceId);

            return xmlRequest;
        }

        #endregion
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


                switch (payment.Type)
                {
                    case Payments.Transaction1.ChargeType.Sale:
                        if (!string.IsNullOrWhiteSpace(payment.TransactionID))
                        {
                            ProcessAuthComplete(invoiceId, payment, pcfg, true, ref pr, testMode);
                        }
                        else
                        {
                            ProcessSaleAuthCard(invoiceId, payment, pcfg, true, ref pr, testMode);
                        }
                        
                        break;
                    case Payments.Transaction1.ChargeType.AuthOnly:
                        ProcessAuthOnlyCard(invoiceId, payment, pcfg, false, ref pr, testMode);
                        break;
                    case Payments.Transaction1.ChargeType.Credit:
                        ProcessRefund(invoiceId, payment, pcfg, ref pr, testMode);
                        break;
                    case Payments.Transaction1.ChargeType.Void:
                        ProcessVoid(invoiceId, payment, pcfg, ref pr, testMode);
                        break;
                }
                pr.PaymentGateway = Configuration.Gateway.WorldPayXML;
                pr.PayType = Common.Common.PaymentType.CreditCard;
                pr.Amount = payment.Amount;
                pr.Card = payment.Card;
                pr.ProcessedBy = payment.ProcessedBy;
                pr.TransactionType = (Common.Payments.Transaction.ChargeType)payment.Type;
                pr.Amount = payment.Amount;
            }
            catch (Exception ex)
            {
                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WorldPayXML.ProcessCreditCard", "MemberId:" + wineryID.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "",1,wineryID);
            }
            
            return pr;
        }

        private static void ProcessSaleAuthCard(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, bool captureCard, ref TransactionResult pr, bool isTestMode = false)
        {
            string URL = GetServicesRequestUrl();
            string xmlRequest = GetXMLRequestChargeCard(pcfg, invoiceId, payment);
            var response = MakeXMLHttpCall(xmlRequest, URL);

            try
            {
                if (!string.IsNullOrWhiteSpace(response))
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi", "WorldPayXML.ProcessCreditCard: Request:" + xmlRequest + "", "", 3, payment.WineryId);
                    logDAL.InsertLog("WebApi", "WorldPayXML.ProcessCreditCard: Response:" + response, "", 3, payment.WineryId);

                    XDocument doc = XDocument.Parse(response);
                    string jsonText = JsonConvert.SerializeXNode(doc);
                    dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);

                    if (dyn != null){
                        try
                        {
                            if (dyn.CreditCardSaleResponse != null)
                            {
                                if (dyn.CreditCardSaleResponse.Response.ExpressResponseCode == "0")
                                {
                                    //charge approved
                                    pr.Status = TransactionResult.StatusType.Success;
                                    pr.TransactionID = dyn.CreditCardSaleResponse.Response.Transaction.TransactionID;
                                    pr.ResponseCode = dyn.CreditCardSaleResponse.Response.ExpressResponseCode;
                                    pr.ApprovalCode = dyn.CreditCardSaleResponse.Response.Transaction.ApprovalNumber;
                                }
                                else
                                {
                                    pr.Status = TransactionResult.StatusType.Failed;
                                    pr.Detail = dyn.CreditCardSaleResponse.Response.ExpressResponseMessage;
                                    pr.ResponseCode = dyn.CreditCardSaleResponse.Response.ExpressResponseCode;
                                }
                            }
                            else
                            {
                                pr.Status = TransactionResult.StatusType.Failed;
                                pr.Detail = dyn.Response.Response.ExpressResponseMessage;
                                pr.ResponseCode = dyn.Response.Response.ExpressResponseCode;
                            }
                        }
                        catch
                        {
                            pr.Status = TransactionResult.StatusType.Failed;
                            pr.Detail = dyn.Response.Response.ExpressResponseMessage;
                            pr.ResponseCode = dyn.Response.Response.ExpressResponseCode;
                        }
                    }
                    else
                    {
                        pr.Status = TransactionResult.StatusType.Failed;
                        pr.Detail = "Unknown error from gateway";
                        pr.ResponseCode = "";
                    }
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Unknown error from gateway";
                    pr.ResponseCode = "";
                }
            }
            catch
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";
            }
        }

        private static void ProcessAuthOnlyCard(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, bool captureCard, ref TransactionResult pr, bool isTestMode = false)
        {
            string URL = GetServicesRequestUrl();
            string xmlRequest = GetXMLRequestAuthorizeCard(pcfg, invoiceId, payment);
            var response = MakeXMLHttpCall(xmlRequest, URL);

            try
            {
                if (!string.IsNullOrWhiteSpace(response))
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi", "WorldPayXML.ProcessAuthOnlyCard: Request:" + xmlRequest + "", "", 3, payment.WineryId);
                    logDAL.InsertLog("WebApi", "WorldPayXML.ProcessAuthOnlyCard: Response:" + response, "", 3, payment.WineryId);

                    XDocument doc = XDocument.Parse(response);
                    string jsonText = JsonConvert.SerializeXNode(doc);
                    dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);

                    if (dyn != null && dyn.CreditCardSaleResponse != null)
                    {
                        if (dyn.CreditCardSaleResponse.Response.ExpressResponseCode == "0")
                        {
                            //charge approved
                            pr.Status = TransactionResult.StatusType.Success;
                            pr.TransactionID = dyn.CreditCardSaleResponse.Response.Transaction.TransactionID;
                            pr.ResponseCode = dyn.CreditCardSaleResponse.Response.ExpressResponseCode;
                            pr.ApprovalCode = dyn.CreditCardSaleResponse.Response.Transaction.ApprovalNumber;
                        }
                        else
                        {
                            pr.Status = TransactionResult.StatusType.Failed;
                            pr.Detail = dyn.CreditCardSaleResponse.Response.ExpressResponseMessage;
                            pr.ResponseCode = dyn.CreditCardSaleResponse.Response.ExpressResponseCode;
                        }
                    }
                    else
                    {
                        pr.Status = TransactionResult.StatusType.Failed;
                        pr.Detail = "Unknown error from gateway";
                        pr.ResponseCode = "";
                    }
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Unknown error from gateway";
                    pr.ResponseCode = "";
                }
            }
            catch
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";
            }
        }

        private static void ProcessAuthComplete(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, bool captureCard, ref TransactionResult pr, bool isTestMode = false)
        {
            string URL = GetServicesRequestUrl();
            string xmlRequest = GetXMLRequestAuthorizeCardComplete(pcfg, invoiceId, payment);
            var response = MakeXMLHttpCall(xmlRequest, URL);

            try
            {
                if (!string.IsNullOrWhiteSpace(response))
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi", "WorldPayXML.ProcessAuthComplete: Request:" + xmlRequest + "", "", 3, payment.WineryId);
                    logDAL.InsertLog("WebApi", "WorldPayXML.ProcessAuthComplete: Response:" + response, "", 3, payment.WineryId);

                    XDocument doc = XDocument.Parse(response);
                    string jsonText = JsonConvert.SerializeXNode(doc);
                    dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);

                    if (dyn != null && dyn.CreditCardSaleResponse != null)
                    {
                        if (dyn.CreditCardSaleResponse.Response.ExpressResponseCode == "0")
                        {
                            //charge approved
                            pr.Status = TransactionResult.StatusType.Success;
                            pr.TransactionID = dyn.CreditCardSaleResponse.Response.Transaction.TransactionID;
                            pr.ResponseCode = dyn.CreditCardSaleResponse.Response.ExpressResponseCode;
                            pr.ApprovalCode = dyn.CreditCardSaleResponse.Response.Transaction.ApprovalNumber;
                        }
                        else
                        {
                            pr.Status = TransactionResult.StatusType.Failed;
                            pr.Detail = dyn.CreditCardSaleResponse.Response.ExpressResponseMessage;
                            pr.ResponseCode = dyn.CreditCardSaleResponse.Response.ExpressResponseCode;
                        }
                    }
                    else
                    {
                        pr.Status = TransactionResult.StatusType.Failed;
                        pr.Detail = "Unknown error from gateway";
                        pr.ResponseCode = "";
                    }
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Unknown error from gateway";
                    pr.ResponseCode = "";
                }
            }
            catch
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";
            }
        }

        private static void ProcessVoid(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, ref TransactionResult pr, bool isTestMode = false)
        {
            string URL = GetServicesRequestUrl();
            string xmlRequest = GetXMLRequestVoidCard(pcfg, invoiceId, payment);
            var response = MakeXMLHttpCall(xmlRequest, URL);

            if (!string.IsNullOrWhiteSpace(response))
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "WorldPayXML.ProcessVoid: Request:" + xmlRequest + "", "", 3, payment.WineryId);
                logDAL.InsertLog("WebApi", "WorldPayXML.ProcessVoid: Response:" + response, "", 3, payment.WineryId);

                XDocument doc = XDocument.Parse(response);
                string jsonText = JsonConvert.SerializeXNode(doc);
                dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);
                if (dyn.CreditCardVoidResponse.Response.ExpressResponseCode == "0")
                {
                    //charge approved
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = dyn.CreditCardVoidResponse.Response.Transaction.TransactionID;
                    pr.ResponseCode = dyn.CreditCardVoidResponse.Response.ExpressResponseCode;
                    pr.ApprovalCode = dyn.CreditCardVoidResponse.Response.Transaction.ApprovalNumber;
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = dyn.CreditCardVoidResponse.Response.ExpressResponseMessage;
                    pr.ResponseCode = dyn.CreditCardVoidResponse.Response.ExpressResponseCode;
                }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";
            }
        }

        private static void ProcessRefund(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, ref TransactionResult pr, bool isTestMode = false)
        {

            string URL = GetServicesRequestUrl();
            string xmlRequest = GetXMLRequestRefundCard(pcfg, invoiceId, payment);
            var response = MakeXMLHttpCall(xmlRequest, URL);


            if (!string.IsNullOrWhiteSpace(response))
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "WorldPayXML.ProcessRefund: Request:" + xmlRequest + "", "", 3, payment.WineryId);
                logDAL.InsertLog("WebApi", "WorldPayXML.ProcessRefund: Response:" + response, "", 3, payment.WineryId);

                XDocument doc = XDocument.Parse(response);
                string jsonText = JsonConvert.SerializeXNode(doc);
                dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);
                if (dyn.CreditCardReturnResponse.Response.ExpressResponseCode == "0")
                {
                    //charge approved
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = dyn.CreditCardReturnResponse.Response.Transaction.TransactionID;
                    pr.ResponseCode = dyn.CreditCardReturnResponse.Response.ExpressResponseCode;
                    pr.ApprovalCode = dyn.CreditCardReturnResponse.Response.Transaction.ApprovalNumber;

                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = dyn.CreditCardReturnResponse.Response.ExpressResponseMessage;
                    pr.ResponseCode = dyn.CreditCardReturnResponse.Response.ExpressResponseCode;

                }
            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";
            }
        }


        private static void ProcessReversal(int invoiceId, Payments.Transaction1 payment, Configuration pcfg, ref TransactionResult pr, bool isTestMode = false)
        {

            string URL = GetServicesRequestUrl();
            string xmlRequest = GetXMLRequestCardReversal(pcfg, invoiceId, payment);
            var response = MakeXMLHttpCall(xmlRequest, URL);


            if (!string.IsNullOrWhiteSpace(response))
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "WorldPayXML.ProcessReversal: Request:" + xmlRequest + "", "", 3, payment.WineryId);
                logDAL.InsertLog("WebApi", "WorldPayXML.ProcessReversal: Response:" + response, "", 3, payment.WineryId);

                XDocument doc = XDocument.Parse(response);
                string jsonText = JsonConvert.SerializeXNode(doc);
                dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);
                if (dyn.CreditCardReversalResponse.Response.ExpressResponseCode == "0")
                {
                    //charge approved
                    pr.Status = TransactionResult.StatusType.Success;
                    pr.TransactionID = dyn.CreditCardReversalResponse.Response.Transaction.TransactionID;
                    pr.ResponseCode = dyn.CreditCardReversalResponse.Response.ExpressResponseCode;
                    pr.ApprovalCode = dyn.CreditCardReversalResponse.Response.Transaction.ApprovalNumber;

                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = dyn.CreditCardReversalResponse.Response.ExpressResponseMessage;
                    pr.ResponseCode = dyn.CreditCardReversalResponse.Response.ExpressResponseCode;

                }

            }
            else
            {
                pr.Status = TransactionResult.StatusType.Failed;
                pr.Detail = "Unknown error from gateway";
                pr.ResponseCode = "";

            }
        }

    }
}
