using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public string AuthToken_Backoffice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AuthToken_BoxOffice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AuthToken_BoxOfficeV2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AuthToken_CellarpassAccount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AuthToken_TablePro { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AuthToken_CheckIn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool AuthenticationRequired { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PaymentDebug { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool PaymentTestMode { get; set; }
        /// <summary>
        /// Azure Storage Connectionstring
        /// </summary>
        public string StorageConnectionString { get; set; }
        public string MainGunApiKey { get; set; }
        public string MailGunPostUrl { get; set; }
        public string USPS_URL { get; set; }
        public string USPS_Username { get; set; }
        public string USPS_Password { get; set; }
        public string certificatePassword { get; set; }
        public string QueueName { get; set; }
        public string ReserveCloudUrl { get; set; }
        public string StripeSecretKey { get; set; }

        public string GoogleAPIKey { get; set; }
        public string Shift4ClientGUID { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string ShopifyUrl { get; set; }
        public string ShopifyAuthToken { get; set; }
        public string ZeamsterDeveloperId { get; set; }
        public string ServiceBusQueueName { get; set; }

        public string IronPDFLicenseKey { get; set; }

        public int DeleteExportFileDays { get; set; }

        public string JwtKey { get; set; }

        public string JwtIssuer { get; set; }

        public int JwtExpireMinutes { get; set; }

        public string GoogleReCaptchaSecretKey { get; set; }
    }
}
