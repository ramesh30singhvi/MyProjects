using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CPReservationApi.Model;
using CPReservationApi.Common;
using CPReservationApi.DAL;

namespace CPReservationApi.WebApi.Services
{
    public class Fareharbor
    {
        static private ViewModels.AppSettings _appSettings;

        public Fareharbor(IOptions<ViewModels.AppSettings> settings)
        {
            _appSettings = settings.Value;
        }
        const string liveUrl = "https://fareharbor.com/";
        const string testUrl = "https://demo.fareharbor.com/";
        const string companiesUrl = "api/external/v1/companies/";

        private static string _errorResponse = "";

        private static string GetFareHarborBaseRequestURL()
        {
            string requestUri = "";
            bool isLive = false;
            if (_appSettings.QueueName == "emailqueue")
            {
                isLive = true;
            }

            if (isLive)
            {
                requestUri = liveUrl + companiesUrl;
            }
            else
            {
                requestUri = testUrl + companiesUrl;
            }
            return requestUri;
        }

        public static FareharborModel GetFareharborCompanies()
        {
            FareharborModel modelList = null;
            try
            {
                string requestUri = "";
                
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.FareharborAPI).ToList();
                string userName = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.FareharborAPI_UserName);
                string password = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.FareharborAPI_Password);

                requestUri = GetFareHarborBaseRequestURL();
                requestUri = requestUri + "?api-app=" + password + "&api-user=" + userName;
                HttpClient client = new HttpClient();

                using (var response = client.GetAsync(requestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    modelList = JsonConvert.DeserializeObject<FareharborModel>(varResponse);
                }
            }
            catch (Exception ex)
            {

                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetCompaniesFareharbor:  " + ex.Message.ToString(), "",1,0);

            }
            return modelList;
        }

        public static FareharborCompanyItem GetFareharborCompanyItem(int memberId)
        {
            FareharborCompanyItem modelList = null;
            try
            {
                string requestUri = "";

                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.FareharborAPI).ToList();
                string userName = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.FareharborAPI_UserName);
                string password = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.FareharborAPI_Password);

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                string ThirdPartyTransaction = eventDAL.GetWineryById(memberId).ThirdPartyTransaction;


                requestUri = GetFareHarborBaseRequestURL();
                requestUri = requestUri + ThirdPartyTransaction + "/items/?api-app=" + password + "&api-user=" + userName;

                HttpClient client = new HttpClient();

                using (var response = client.GetAsync(requestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    modelList = JsonConvert.DeserializeObject<FareharborCompanyItem>(varResponse);
                }
            }
            catch (Exception ex)
            {

                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetFareharborCompanyItem:  " + ex.Message.ToString(), "",1,memberId);

            }
            return modelList;
        }

        public static AvailabilityModel GetAvailabilityByItemAndDateRange(int memberId, int itemId, DateTime dtFromDate, DateTime dtToDate )
        {
            AvailabilityModel availabilityModel = null;
            try
            {
                string requestUri = "";

                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.FareharborAPI).ToList();
                string userName = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.FareharborAPI_UserName);
                string password = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.FareharborAPI_Password);

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                string ThirdPartyTransaction = eventDAL.GetWineryById(memberId).ThirdPartyTransaction;

                requestUri = GetFareHarborBaseRequestURL();
                string fromDate = dtFromDate.ToString("yyyy-MM-dd");
                string toDate = dtToDate.ToString("yyyy-MM-dd");

                if (fromDate.Equals(toDate))
                {
                    requestUri = requestUri + ThirdPartyTransaction + string.Format("/items/{0}/minimal/availabilities/date/{1}", itemId, fromDate);
                }
                else
                {
                    requestUri = requestUri + ThirdPartyTransaction + string.Format("/items/{0}/minimal/availabilities/date-range/{1}/{2}", itemId, fromDate, toDate);
                }
                requestUri += "/?api-app=" + password + "&api-user=" + userName;


                HttpClient client = new HttpClient();

                using (var response = client.GetAsync(requestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    availabilityModel = JsonConvert.DeserializeObject<AvailabilityModel>(varResponse);
                }
            }
            catch (Exception ex)
            {

                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetAvailabilityByItemAndDateRange:  " + ex.Message.ToString(), "",1,memberId);

            }
            return availabilityModel;
        }

    }
}
