using CPReservationApi.DAL;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using AuthorizeNet.Api.Controllers.Bases;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using static CPReservationApi.Common.Payments;
using Newtonsoft.Json;

namespace CPReservationApi.WebApi.Services
{
    public class AuthNet
    {
        static private AppSettings _appSettings;
        public AuthNet(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
        }

        private static void SetupEnvironment(bool testMode, Configuration pcfg)
        {
            // set whether to use the sandbox environment, or production enviornment
            if (testMode)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
            else
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = pcfg.MerchantLogin,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = pcfg.MerchantPassword,
            };
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
                SetupEnvironment(testMode, pcfg);
                var creditCard = new creditCardType
                {
                    cardNumber = request.number,
                    expirationDate = request.exp_month.PadLeft(2, '0') + Common.Common.Right(request.exp_year, 2)
                };
                paymentType cc = new paymentType { Item = creditCard };
                List<customerPaymentProfileType> paymentProfileList = new List<customerPaymentProfileType>();
                customerPaymentProfileType ccPaymentProfile = new customerPaymentProfileType();
                ccPaymentProfile.payment = cc;
                var userInfo = request.user_info;
                if (request.user_id > 0 && userInfo == null)
                {
                    var userDAL = new UserDAL(Common.Common.ConnectionString);
                    var userDetails = userDAL.GetUserDetailsbyId(request.user_id);

                    if (userDetails != null)
                    {
                        userInfo.first_name = userDetails.first_name;
                        userInfo.email = userDetails.email;
                        userInfo.last_name = userDetails.last_name;
                        userInfo.phone_number = userDetails.phone_number;
                        if (userDetails.address != null)
                        {
                            userInfo.address = new UserAddress
                            {
                                city = userDetails.address.city,
                                country = userDetails.address.city,
                                state = userDetails.address.state,
                                zip_code= userDetails.address.zip_code,
                            };
                        }
                    }
                }
                if (userInfo != null)
                {
                    ccPaymentProfile.billTo = new customerAddressType();
                    if (!string.IsNullOrWhiteSpace(request.user_info.first_name))
                    {
                        ccPaymentProfile.billTo.firstName = request.user_info.first_name;
                    }
                    if (!string.IsNullOrWhiteSpace(request.user_info.last_name))
                    {
                        ccPaymentProfile.billTo.lastName = request.user_info.last_name;
                    }
                    if (!string.IsNullOrWhiteSpace(request.user_info.phone_number))
                    {
                        ccPaymentProfile.billTo.phoneNumber = request.user_info.phone_number;
                    }
                    if (!string.IsNullOrWhiteSpace(request.user_info.email))
                    {
                        ccPaymentProfile.billTo.email = request.user_info.email;
                    }
                    if (request.user_info.address != null)
                    {
                        if (!string.IsNullOrWhiteSpace(request.user_info.address.city))
                        {
                            ccPaymentProfile.billTo.city = request.user_info.address.city;
                        }
                        if (!string.IsNullOrWhiteSpace(request.user_info.address.state))
                        {
                            ccPaymentProfile.billTo.state = request.user_info.address.state;
                        }
                        if (!string.IsNullOrWhiteSpace(request.user_info.address.zip_code))
                        {
                            ccPaymentProfile.billTo.zip = request.user_info.address.zip_code;
                        }
                        if (!string.IsNullOrWhiteSpace(request.user_info.address.country))
                        {
                            if (request.user_info.address.country.ToLower() == "us" || request.user_info.address.country.ToLower() == "united states")
                            {
                                ccPaymentProfile.billTo.country = "USA";
                            }
                            else
                            {
                                ccPaymentProfile.billTo.country = request.user_info.address.country;
                            }
                        }
                    }

                }

                
                paymentProfileList.Add(ccPaymentProfile);

                customerProfileType customerProfile = new customerProfileType();
                customerProfile.merchantCustomerId = Common.StringHelpers.GenerateRandomString(10, false);
                //customerProfile.email = emailId;
                customerProfile.paymentProfiles = paymentProfileList.ToArray();
                //customerProfile.shipToList = addressInfoList.ToArray();

                var custProfilerequest = new createCustomerProfileRequest { profile = customerProfile, validationMode = validationModeEnum.none };

                // instantiate the controller that will call the service
                var controller = new createCustomerProfileController(custProfilerequest);
                controller.Execute();

                // get the response from the service (errors contained if any)
                createCustomerProfileResponse response = controller.GetApiResponse();

                // validate response 
                if (response != null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        if (response.messages.message != null)
                        {
                            resp = new TokenizedCard
                            {
                                card_token = response.customerProfileId + "~" + response.customerPaymentProfileIdList[0],
                                last_four_digits = Common.Common.Right(request.number, 4),
                                first_four_digits = Common.Common.Left(request.number, 4),
                                is_expired = false,
                                card_type= Payments.GetCardType(request.number)
                            };
                        }
                        else
                        {
                            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                            logDAL.InsertLog("WebApi-AuthNet::TokenziedCard", "Member Id:" + request.member_id.ToString() + ", Card Number:" + request.number + ", Error code:" + JsonConvert.SerializeObject(response.messages), "",1,request.member_id);
                        }
                    }
                    else
                    {
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                        logDAL.InsertLog("WebApi-AuthNet::TokenziedCard", "Member Id:" + request.member_id.ToString() + ", Card Number:" + request.number + ", Error code:" + response.messages.message[0].code + ", Error message:" + response.messages.message[0].text, "",1,request.member_id);
                    }
                }
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "WebApi-AuthNet::Tokenized Card:  " + ex.Message.ToString(), "",1,request.member_id);

                if (ex.Message.ToString().IndexOf("invalid authentication") > -1)
                {
                    resp = new TokenizedCard
                    {
                        ErrorMessage = "User authentication failed due to invalid authentication values."
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
            return resp;

        }

        public static TransactionResult ProcessCreditCard(int wineryID, int invoiceId, Configuration pcfg, Payments.Transaction1 payment)
        {
            TransactionResult pr = new TransactionResult();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

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

                //Setup Gateway
                SetupEnvironment(testMode, pcfg);

                //Result Object
                //parese token to get cust profile Id and paymentprofile Id
                string customerProfileId = "", customerPaymentProfileId = "";
                customerProfilePaymentType profileToCharge = new customerProfilePaymentType();

                string transType = "";
                transactionRequestType transactionRequest = new transactionRequestType
                {
                    amount = Math.Round(payment.Amount,2)
                };
                //if (payment.Type == Payments.Transaction1.ChargeType.Sale || payment.Type == Payments.Transaction1.ChargeType.AuthOnly)
                //{
                //handle both with card token and without card token
                if (!string.IsNullOrWhiteSpace(payment.Card.CardToken))
                {
                    try
                    {
                        string[] arr = payment.Card.CardToken.Split('~');
                        customerProfileId = arr[0];
                        customerPaymentProfileId = arr[1];
                    }
                    catch
                    {
                        customerProfileId = "";
                        customerPaymentProfileId = "";
                    }
                    //

                    if (!string.IsNullOrWhiteSpace(customerProfileId) && !string.IsNullOrWhiteSpace(customerPaymentProfileId))
                    {
                        profileToCharge.customerProfileId = customerProfileId;
                        profileToCharge.paymentProfile = new paymentProfile { paymentProfileId = customerPaymentProfileId };
                        transactionRequest.profile = profileToCharge;
                    }
                }
                else
                {
                    var creditCard = new creditCardType
                    {
                        cardNumber = payment.Card.Number,
                        expirationDate = payment.Card.ExpMonth.PadLeft(2, '0') + Common.Common.Right(payment.Card.ExpYear, 2),
                        cardCode = (payment.Card.CVV + "").Trim()
                    };

                    var billingAddress = new customerAddressType
                    {
                        firstName = payment.User.first_name,
                        lastName = payment.User.last_name,
                        address = payment.User.address.address_1,
                        city = payment.User.address.city,
                        zip = payment.User.address.zip_code

                    };

                    var paymentType = new paymentType { Item = creditCard };
                    transactionRequest.billTo = billingAddress;
                    transactionRequest.payment = paymentType;

                }
                //}

                //Transaction Sale Type
                switch (payment.Type)
                {
                    case Payments.Transaction1.ChargeType.Sale:
                    case Payments.Transaction1.ChargeType.Capture:
                        transType = transactionTypeEnum.authCaptureTransaction.ToString();
                        //if there was a pre auth and the transid is passed then do priorAuthcapture
                        if (!string.IsNullOrWhiteSpace(payment.TransactionID))
                        {
                            transType = transactionTypeEnum.priorAuthCaptureTransaction.ToString();
                            transactionRequest.refTransId = payment.TransactionID;
                        }
                        break;
                    case Payments.Transaction1.ChargeType.AuthOnly:
                        transType = transactionTypeEnum.authOnlyTransaction.ToString();
                        break;
                    case Payments.Transaction1.ChargeType.Credit:
                        transType = transactionTypeEnum.refundTransaction.ToString();
                        transactionRequest.refTransId = payment.TransactionID;
                        break;
                    case Payments.Transaction1.ChargeType.Void:
                        transType = transactionTypeEnum.voidTransaction.ToString();
                        transactionRequest.refTransId = payment.TransactionID;
                        //Search for existing transaction

                        break;
                }

                transactionRequest.transactionType = transType;

                string reference = string.Empty;

                try
                {
                    switch (payment.Transaction)
                    {
                        case Payments.Transaction1.TransactionType.Billing:
                            reference = "CP-" + invoiceId;
                            break;
                        case Payments.Transaction1.TransactionType.Rsvp:
                            string bookingCode = eventDAL.GetBookingCodeByReservationID(invoiceId);
                            if ((!object.ReferenceEquals(bookingCode, string.Empty)))
                            {
                                reference = string.Format("CP-{0}-{1}", memberId, bookingCode);
                            }
                            else
                            {
                                reference = string.Format("CP-{0}-{1}", memberId, payment.CheckOrRefNumber);
                            }
                            break;
                        case Payments.Transaction1.TransactionType.TicketSale:
                            reference = string.Format("CP-Tickets-{0}-{1}", wineryID.ToString().PadLeft(4, '0'), invoiceId.ToString().PadLeft(8, '0'));
                            break;
                    }
                }
                catch { }

                var Order = new orderType
                {
                    invoiceNumber = reference,
                    description = "Payment through CellarPass"
                };

                transactionRequest.order = Order;

                var request = new createTransactionRequest { transactionRequest = transactionRequest };

                // instantiate the collector that will call the service
                var controller = new createTransactionController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();
                pr.PaymentGateway = Configuration.Gateway.AuthorizeNet;
                pr.PayType = Common.Common.PaymentType.CreditCard;
                pr.Amount = Math.Round(payment.Amount, 2);
                pr.Card = payment.Card;
                pr.ProcessedBy = payment.ProcessedBy;
                pr.TransactionType = (Common.Payments.Transaction.ChargeType)payment.Type;

                if (response != null)
                {
                    logDAL.InsertLog("AuthNet.ProcessCreditCard", "Authorize.net: Request" + JsonConvert.SerializeObject(request), "", 3, wineryID);
                    logDAL.InsertLog("AuthNet.ProcessCreditCard", "Authorize.net: Response" + JsonConvert.SerializeObject(response), "", 3, wineryID);

                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        if (response.transactionResponse.messages != null)
                        {
                            if (response.transactionResponse.errors != null && response.transactionResponse.errors.Length > 0 && response.transactionResponse.errors[0].errorText.Length > 0)
                            {
                                pr.Status = TransactionResult.StatusType.Failed;
                                pr.Detail = response.transactionResponse.errors[0].errorText;
                                pr.ResponseCode = response.transactionResponse.errors[0].errorCode;
                            }
                            else
                            {
                                if (JsonConvert.SerializeObject(response).ToLower().IndexOf("declined") > -1 || JsonConvert.SerializeObject(response).ToLower().IndexOf(" your order is currently being reviewed") > -1)
                                    pr.Status = TransactionResult.StatusType.Failed;
                                else
                                    pr.Status = TransactionResult.StatusType.Success;

                                pr.ResponseCode = response.transactionResponse.responseCode;
                                pr.TransactionID = response.transactionResponse.transId;
                                pr.ApprovalCode = response.transactionResponse.authCode;
                                pr.Detail = response.transactionResponse.messages[0].description;
                            }
                        }
                        else
                        {
                            pr.Status = TransactionResult.StatusType.Failed;
                            pr.Detail = response.transactionResponse.errors[0].errorText;
                            pr.ResponseCode = response.transactionResponse.errors[0].errorCode;
                        }
                    }
                    else
                    {
                        pr.Status = TransactionResult.StatusType.Failed;
                        if (response.transactionResponse != null && response.transactionResponse.errors != null)
                        {
                            pr.Detail = response.transactionResponse.errors[0].errorText;
                            pr.ResponseCode = response.transactionResponse.errors[0].errorCode;
                        }
                        else
                        {
                            pr.Detail = response.messages.message[0].text;
                            pr.ResponseCode = response.messages.message[0].code;
                        }
                    }
                }
                else
                {
                    pr.Status = TransactionResult.StatusType.Failed;
                    pr.Detail = "Null Response from Authorize.net";
                }
            }
            catch (Exception ex)
            {
                pr.Status = Common.Payments.TransactionResult.StatusType.Failed;
                
                logDAL.InsertLog("AuthNet.ProcessCreditCard", "MemberId:" + wineryID.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "",1,wineryID);

            }

            return pr;
        }
    }
}
