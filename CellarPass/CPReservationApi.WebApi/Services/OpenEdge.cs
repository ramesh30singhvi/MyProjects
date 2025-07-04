using CPReservationApi.DAL;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using static CPReservationApi.Common.Payments;

namespace CPReservationApi.WebApi.Services
{
    public class OpenEdge
    {
        static private AppSettings _appSettings;
        public OpenEdge(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
        }
        
        const string LiveUrl = "https://gw.t3secure.net/x-chargeweb.dll";
        const string TestUrl = "https://test.t3secure.net/x-chargeweb.dll";
        public async static Task<Common.Payments.TransactionResult> ProcessOpenEdgeCreditCard(int wineryID, int invoiceId, Common.Payments.Configuration pcfg, Common.Payments.Transaction payment)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            //Transaction Result
            Common.Payments.TransactionResult pr = new Common.Payments.TransactionResult();

            try
            {
                int memberId = wineryID;

                if (memberId == -1)
                {
                    memberId = payment.WineryId;
                }

                //Create Invoice Id String
                string InvoiceRev = "";
                switch (payment.Transactions)
                {
                    case Common.Payments.Transaction.TransactionType.Billing:
                        InvoiceRev = "CP-" + invoiceId;
                        break;
                    case Common.Payments.Transaction.TransactionType.Rsvp:
                        string bookingCode = eventDAL.GetBookingCodeByReservationID(invoiceId);
                        if ((!object.ReferenceEquals(bookingCode, string.Empty)))
                        {
                            InvoiceRev = string.Format("CP-{0}-{1}", memberId, bookingCode);
                        }
                        else
                        {
                            InvoiceRev = string.Format("CP-{0}-{1}", memberId, payment.CheckOrRefNumber);
                        }
                        break;
                    case Common.Payments.Transaction.TransactionType.TicketSale:
                        InvoiceRev = string.Format("CP-Tickets-{0}-{1}", memberId.ToString().PadLeft(4, '0'), invoiceId.ToString().PadLeft(8, '0'));
                        break;
                }

                //Get Test Mode Value
                bool TestMode = false;
                if ((!string.IsNullOrEmpty(_appSettings.PaymentDebug)))
                {
                    if (_appSettings.PaymentTestMode == true)
                    {
                        TestMode = true;
                    }
                }

                //XML REQUEST
                string xmlStr = "";
                xmlStr += "<?xml version=\"1.0\"?>";
                xmlStr += "<GatewayRequest>";
                xmlStr += "<SpecVersion>XWeb3.5</SpecVersion>";
                xmlStr += "<POSType>PC</POSType>";
                xmlStr += "<Industry>ECOMMERCE</Industry>";
                xmlStr += "<PinCapabilities>FALSE</PinCapabilities>";
                xmlStr += "<TrackCapabilities>NONE</TrackCapabilities>";
                xmlStr += "<DuplicateMode>CHECKING_ON</DuplicateMode>";

                // xmlStr += "<TrackingID>1234567890123456</TrackingID>"

                //Setup Gateway
                xmlStr += string.Format("<XWebID>{0}</XWebID>", pcfg.MerchantLogin);
                xmlStr += string.Format("<AuthKey>{0}</AuthKey>", pcfg.MerchantPassword);
                xmlStr += string.Format("<TerminalID>{0}</TerminalID>", pcfg.UserConfig1);

                //Amount
                xmlStr += string.Format("<Amount>{0}</Amount>", Math.Round(payment.Amount, 2));

                //Transaction Sale Type
                string transactionType = "";

                switch (payment.Type)
                {
                    case Common.Payments.Transaction.ChargeType.Sale:
                        transactionType = "CreditSaleTransaction";
                        xmlStr += "<CreateAlias>FALSE</CreateAlias>";
                        xmlStr += "<CustomerPresent>FALSE</CustomerPresent>";
                        xmlStr += "<CardPresent>FALSE</CardPresent>";
                        if (!string.IsNullOrWhiteSpace(payment.Card.CardToken))
                        {
                            xmlStr += string.Format("<Alias>{0}</Alias>", payment.Card.CardToken);
                        }
                        else
                        {
                            xmlStr += string.Format("<AcctNum>{0}</AcctNum>", payment.Card.Number);
                            xmlStr += string.Format("<ExpDate>{0}</ExpDate>", payment.Card.ExpMonth.PadLeft(2, '0') + Common.Common.Right(payment.Card.ExpYear, 2));

                            //cvv
                            if ((payment.Card.CVV != null))


                            {
                                if (!object.ReferenceEquals(payment.Card.CVV.Trim(), string.Empty))
                                {
                                    xmlStr += string.Format("<CardCode>{0}</CardCode>", payment.Card.CVV);
                                }
                            }
                        }

                        //xmlStr += String.Format("<Address>{0}</Address>", "123 Anystreet Drive")
                        xmlStr += string.Format("<ZipCode>{0}</ZipCode>", payment.User.ZipCode);

                        break;
                    case Common.Payments.Transaction.ChargeType.Capture:
                        transactionType = "CreditCaptureTransaction";
                        xmlStr += "<CreateAlias>FALSE</CreateAlias>";
                        xmlStr += "<CustomerPresent>FALSE</CustomerPresent>";
                        xmlStr += "<CardPresent>FALSE</CardPresent>";
                        if(!string.IsNullOrWhiteSpace(payment.TransactionID))
                              xmlStr += string.Format("<TransactionID>{0}</TransactionID>", payment.TransactionID);
                        if (!string.IsNullOrWhiteSpace(payment.Card.CardToken))
                        {
                            xmlStr += string.Format("<Alias>{0}</Alias>", payment.Card.CardToken);
                        }
                        else
                        {
                            xmlStr += string.Format("<AcctNum>{0}</AcctNum>", payment.Card.Number);
                            xmlStr += string.Format("<ExpDate>{0}</ExpDate>", payment.Card.ExpMonth.PadLeft(2, '0') + Common.Common.Right(payment.Card.ExpYear, 2));

                            //cvv
                            if ((payment.Card.CVV != null))


                            {
                                if (!object.ReferenceEquals(payment.Card.CVV.Trim(), string.Empty))
                                {
                                    xmlStr += string.Format("<CardCode>{0}</CardCode>", payment.Card.CVV);
                                }
                            }
                        }
                        break;
                    case Common.Payments.Transaction.ChargeType.AuthOnly:

                        transactionType = "CreditAuthTransaction";

                        xmlStr += "<CreateAlias>FALSE</CreateAlias>";
                        xmlStr += "<CustomerPresent>FALSE</CustomerPresent>";
                        xmlStr += "<CardPresent>FALSE</CardPresent>";

                        if (!string.IsNullOrWhiteSpace(payment.Card.CardToken))
                        {
                            xmlStr += string.Format("<Alias>{0}</Alias>", payment.Card.CardToken);
                        }
                        else
                        {
                            xmlStr += string.Format("<AcctNum>{0}</AcctNum>", payment.Card.Number);
                            xmlStr += string.Format("<ExpDate>{0}</ExpDate>", payment.Card.ExpMonth.PadLeft(2, '0') + Common.Common.Right(payment.Card.ExpYear, 2));

                            //cvv
                            if ((payment.Card.CVV != null))


                            {
                                if (!object.ReferenceEquals(payment.Card.CVV.Trim(), string.Empty))
                                {
                                    xmlStr += string.Format("<CardCode>{0}</CardCode>", payment.Card.CVV);
                                }
                            }
                        }

                        //xmlStr += String.Format("<Address>{0}</Address>", "123 Anystreet Drive")
                        xmlStr += string.Format("<ZipCode>{0}</ZipCode>", payment.User.ZipCode);

                        break;
                    case Common.Payments.Transaction.ChargeType.Credit:

                        transactionType = "CreditReturnTransaction";
                        xmlStr += "<CustomerPresent>FALSE</CustomerPresent>";
                        xmlStr += "<CardPresent>FALSE</CardPresent>";
                        xmlStr += string.Format("<TransactionID>{0}</TransactionID>", payment.TransactionID);

                        break;
                    case Common.Payments.Transaction.ChargeType.Void:
                        transactionType = "CreditVoidTransaction";
                        xmlStr += string.Format("<TransactionID>{0}</TransactionID>", payment.TransactionID);
                        break;
                }

                xmlStr += string.Format("<TransactionType>{0}</TransactionType>", transactionType);

                //Close XML
                xmlStr += "</GatewayRequest>";

                //URL
                string postURL = LiveUrl;

                //If test mode, use test url
                if (TestMode)
                {
                    postURL = TestUrl;
                }
                logDAL.InsertLog("ProcessOpenEdgeCreditCard", "MemberId: " + wineryID.ToString() + ", Request:" + xmlStr, "Core API", 3,wineryID);
                //Send XML and Get Response
                string responseStr = await postXMLData(postURL, xmlStr);
                logDAL.InsertLog("ProcessOpenEdgeCreditCard", "MemberId: " + wineryID.ToString() + ", Response:" + responseStr, "Core API", 3,wineryID);
                //Response Object
                GatewayResponse xmlResponse = new GatewayResponse();

                //Parse XML response
                try
                {
                    System.Xml.Serialization.XmlSerializer xml_serializer = new System.Xml.Serialization.XmlSerializer(typeof(GatewayResponse));
                    
                    StringReader string_reader = new StringReader(responseStr);
                    xmlResponse = (GatewayResponse)xml_serializer.Deserialize(string_reader);
                    string_reader.Dispose();
                }
                catch (Exception ex)
                {
                }

                //Set some properties of the transaction result
                pr.PaymentGateway = Common.Payments.Configuration.Gateway.OpenEdge;
                pr.PayType = Common.Common.PaymentType.CreditCard;
                pr.Amount = payment.Amount;
                pr.Card = payment.Card;
                pr.ProcessedBy = payment.ProcessedBy;
                pr.TransactionType = (Common.Payments.Transaction.ChargeType)payment.Type;

                //Check Result

                if ((xmlResponse != null))
                {
                    //General 
                    pr.ResponseCode = xmlResponse.ResponseCode;
                    pr.TransactionID = xmlResponse.TransactionID;
                    pr.AvsResponse = xmlResponse.AVSResponseCode;

                    //Check for success or failure

                    if (xmlResponse.ResponseCode == "000")
                    {
                        //Success
                        pr.Status = Common.Payments.TransactionResult.StatusType.Success;
                        pr.ApprovalCode = xmlResponse.ApprovalCode == null ? "" : xmlResponse.ApprovalCode;
                        pr.Detail = xmlResponse.ResponseDescription == null ? "" : xmlResponse.ResponseDescription;

                    }
                    else
                    {
                        //Error
                        pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                        pr.Detail = xmlResponse.ResponseDescription == null ? "" : xmlResponse.ResponseDescription;

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
                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                logDAL.InsertLog("ProcessOpenEdgeCreditCard", "MemberId:" + wineryID.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "",1,wineryID);
            }

            return pr;

        }

        public async static Task<string> postXMLData(string destinationUrl, string requestXml)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
            byte[] bytes = null;
            bytes = System.Text.Encoding.ASCII.GetBytes(requestXml);
            request.ContentType = "text/xml; encoding='utf-8'";
            request.Method = "POST";
            Stream requestStream = await request.GetRequestStreamAsync();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Dispose();
            WebResponse response = await request.GetResponseAsync();
            Stream responseStream = response.GetResponseStream();
            string responseStr = new StreamReader(responseStream).ReadToEnd();
            return responseStr;
        }
        public partial class GatewayResponse
        {

            ///<remarks/>
            public string ResponseCode { get; set; }


            ///<remarks/>
            public string ResponseDescription { get; set; }

            ///<remarks/>
            public string TrackingID { get; set; }

            ///<remarks/>
            public string TransactionID { get; set; }

            ///<remarks/>
            public decimal Amount { get; set; }

            ///<remarks/>
            public string CardType { get; set; }

            ///<remarks/>
            public string CardBrand { get; set; }

            ///<remarks/>
            public string CardBrandShort { get; set; }

            ///<remarks/>
            public string MaskedAcctNum { get; set; }

            ///<remarks/>
            public string ExpDate { get; set; }

            ///<remarks/>
            public string AcctNumSource { get; set; }

            ///<remarks/>
            public string Alias { get; set; }

            ///<remarks/>
            public string ProcessorResponse { get; set; }

            ///<remarks/>
            public string BatchNum { get; set; }

            ///<remarks/>
            public decimal BatchAmount { get; set; }

            ///<remarks/>
            public string ApprovalCode { get; set; }

            ///<remarks/>
            public string AVSResponseCode { get; set; }

            ///<remarks/>
            public string CardCodeResponse { get; set; }

            ///<remarks/>
            public int ReceiptID { get; set; }
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
            try
            {

                //XML REQUEST
                string xmlStr = "";
                xmlStr += "<?xml version=\"1.0\"?>";
                xmlStr += "<GatewayRequest>";
                xmlStr += "<SpecVersion>XWeb3.5</SpecVersion>";
                xmlStr += "<POSType>PC</POSType>";
                xmlStr += "<Industry>ECOMMERCE</Industry>";
                xmlStr += "<PinCapabilities>FALSE</PinCapabilities>";
                xmlStr += "<TrackCapabilities>NONE</TrackCapabilities>";
                //xmlStr += "<DuplicateMode>CHECKING_ON</DuplicateMode>";

                // xmlStr += "<TrackingID>1234567890123456</TrackingID>"

                //Setup Gateway
                xmlStr += string.Format("<XWebID>{0}</XWebID>", pcfg.MerchantLogin);
                xmlStr += string.Format("<AuthKey>{0}</AuthKey>", pcfg.MerchantPassword);
                xmlStr += string.Format("<TerminalID>{0}</TerminalID>", pcfg.UserConfig1);
                xmlStr += string.Format("<TransactionType>{0}</TransactionType>", "AliasCreateTransaction");
                xmlStr += string.Format("<AcctNum>{0}</AcctNum>", request.number);
                xmlStr += string.Format("<ExpDate>{0}</ExpDate>", request.exp_month.PadLeft(2, '0') + Common.Common.Right(request.exp_year, 2));

                //cvv
                if (!string.IsNullOrWhiteSpace(request.cvv2))
                {
                    xmlStr += string.Format("<CardCode>{0}</CardCode>", request.cvv2);
                }
                xmlStr += "</GatewayRequest>";

                //URL
                string postURL = LiveUrl;

                //If test mode, use test url
                if (testMode)
                {
                    postURL = TestUrl;
                }

                //Send XML and Get Response
                string responseStr = Task.Run(() => postXMLData(postURL, xmlStr)).Result;


                //Response Object
                GatewayResponse xmlResponse = new GatewayResponse();

                //Parse XML response
                try
                {
                    System.Xml.Serialization.XmlSerializer xml_serializer = new System.Xml.Serialization.XmlSerializer(typeof(GatewayResponse));
                    StringReader string_reader = new StringReader(responseStr);
                    xmlResponse = (GatewayResponse)xml_serializer.Deserialize(string_reader);
                    string_reader.Dispose();
                }
                catch (Exception ex)
                {
                }

                if (xmlResponse != null)
                {
                    if (!string.IsNullOrEmpty(xmlResponse.Alias))
                    {
                        //Success
                        resp = new TokenizedCard
                        {
                            card_token = xmlResponse.Alias,
                            last_four_digits = Common.Common.Right(request.number, 4),
                            first_four_digits = Common.Common.Left(request.number, 4),
                            card_type = Payments.GetCardType(request.number)
                        };

                    }
                    else
                    {
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                        logDAL.InsertLog("WebApi", "OpenEdge::Tokenized Card: Error, MemberId-" + request.member_id.ToString() + ",ResponseCode - " + xmlResponse.ResponseCode +  ", ResponseDescription-" + xmlResponse.ResponseDescription + ", TransactionID-" + xmlResponse.TransactionID, "",1,request.member_id);
                        resp = new TokenizedCard
                        {
                            ErrorMessage= xmlResponse.ResponseDescription,
                            ErrorCode= xmlResponse.ResponseCode
                        };
                    }
                }
                else
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi", "OpenEdge::Tokenized Card: Error, Request-" + xmlStr + ",Response:" + responseStr, "",1,request.member_id);
                }
            }
            catch (Exception ex)
            {

                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "OpenEdge::Tokenized Card:  MemberId-" + request.member_id.ToString() + ",Error-" + ex.Message.ToString(), "",1,request.member_id);

            }
            return resp;

        }
    }
}
