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
    public class Commerce7Gateway
    {
        static private ViewModels.AppSettings _appSettings;
        private static string _baseURL = "https://boltgw-uat.cardconnect.com/cardconnect/rest/";
        public Commerce7Gateway(IOptions<ViewModels.AppSettings> settings)
        {
            _appSettings = settings.Value;
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
                    //case Payments.Transaction1.ChargeType.Sale:
                    //    ProcessSaleAuthCard(invoiceId, pcfg, payment, ref pr, true);
                    //    break;
                    //case Payments.Transaction1.ChargeType.Credit:
                    //    ProcessRefund(invoiceId, payment, pcfg, "refunds", ref pr);
                    //    break;
                    //case Payments.Transaction1.ChargeType.Void:
                    //    ProcessVoid(invoiceId, payment, pcfg, "voids", ref pr);
                    //    break;
                }


                pr.PaymentGateway = Configuration.Gateway.Commrece7Payments;
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
                logDAL.InsertLog("Commerce7Gateway.ProcessCreditCard", "MemberId:" + wineryID.ToString() + ",IncoiceId:" + invoiceId.ToString() + ",Error:" + ex.Message, "", 1, wineryID);
            }

            return pr;
        }
    }
}
