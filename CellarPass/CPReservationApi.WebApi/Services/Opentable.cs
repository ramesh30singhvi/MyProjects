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
    public class Opentable
    {
        static private ViewModels.AppSettings _appSettings;

        public Opentable(IOptions<ViewModels.AppSettings> settings)
        {
            _appSettings = settings.Value;
        }
        const string liveUrl = "https://oauth.opentable.com/";
        const string testUrl = "https://oauth-pp.opentable.com/";
        const string tokenUrl = "api/v2/oauth/token?grant_type=client_credentials";

        private static string _errorResponse = "";
        private static string dirUrlTest = "https://platform.otqa.com/";
        private static string dirUrlLive = "https://platform.opentable.com/";
        public async static Task<string> GetOpenTableList(int offset=0,int limit =10)
        {
            try
            {
                bool isLive = false;
                if (_appSettings.QueueName == "emailqueue")
                {
                    isLive = true;
                }
                string Url = "sync/directory?offset=" + offset.ToString() + "&limit=" + limit.ToString() + "&country=US";
                if (isLive)
                {
                    Url = dirUrlLive + Url;
                }
                else
                {
                    Url = dirUrlTest + Url;
                }


                string jsonResp = string.Empty;
                _errorResponse = "";
                var generateToken = GetOauthTokenForOpenTable();
                OpenTableTokenResponseModel resultToken = JsonConvert.DeserializeObject<OpenTableTokenResponseModel>(generateToken.ToString());

                if (resultToken != null)
                {

                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resultToken.access_token);
                    var response = await client.GetAsync(Url);

                    if (response.IsSuccessStatusCode)
                        jsonResp = response.Content.ReadAsStringAsync().Result;
                    else
                        _errorResponse = response.Content.ReadAsStringAsync().Result;

                    return jsonResp;
                }
                else
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi", "OpentableList api error:  " + _errorResponse, "",1,0);
                    return "";
                }
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "OpentableList api:  " + ex.Message.ToString(), "",1,0);

            }
            return _errorResponse;


        }

        #region Supporting Methods

        public static string GetOauthTokenForOpenTable()
        {

            try
            {
                string Url = "";

                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.OpenTableAPI).ToList();
                string userName = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.OpenTableAPI_UserName);
                string password = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.OpenTableAPI_Password);

                bool isLive = false;
                if (_appSettings.QueueName == "emailqueue")
                {
                    isLive = true;
                }

                if (isLive)
                {
                    Url = liveUrl + tokenUrl;
                }
                else
                {
                    Url = testUrl + tokenUrl;                    
                }

                string jsonResp = string.Empty;
                _errorResponse = "";

                HttpClient client = new HttpClient();
                var authorization = Generate_Auth_Header(userName, password);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
                var values = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            };

                var content = new FormUrlEncodedContent(values);
                var response = client.PostAsync(Url, content).Result;

                if (response.IsSuccessStatusCode)
                    jsonResp = response.Content.ReadAsStringAsync().Result;
                else
                    _errorResponse = response.Content.ReadAsStringAsync().Result;

                return jsonResp;
            }
            catch (Exception ex)
            {

                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "Oauth opentable token:  " + ex.Message.ToString(), "",1,0);

            }
            return _errorResponse;
        }
        private static string Generate_Auth_Header(string client_id,string client_secret)
        {
            if(!((String.IsNullOrEmpty(client_id)) && (String.IsNullOrEmpty(client_secret))))
            {
                string idSecret = client_id + ":" + client_secret;
                string encodedSecert = Base64Encode(idSecret);
                return encodedSecert.ToString();
            }
            else
            {
                return "Client id or password is empty.";
            }

        }
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }



        #endregion
    }
}
