using CPReservationApi.Model;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UserDetailViewModel = CPReservationApi.WebApi.ViewModels.UserDetailViewModel;

namespace CPReservationApi.WebApi.Services
{
    public class EJGallo
    {
        static private AppSettings _appSettings;
        public EJGallo(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
        }
        public async Task<bool> NewsletterSignup(int memberId, UserDetailViewModel user_detail, CustomSetting customSetting)
        {

            // Make sure we have an email
            if (string.IsNullOrEmpty(user_detail.email))
                return false;

            // Check for noemail'
            if (user_detail.email.ToLower().Contains("@noemail.com"))
                return false;

            bool success = false;

            // Settings: 1 = "Api Key", 2 = "Brand Code",3 = "Source Name"

            if (customSetting == null)
                return false;

            if (customSetting.id == 0)
                return false;

            bool isLive = false;
            string prodDevEndpoint = "d";
            if (_appSettings.QueueName == "emailqueue")
            {
                prodDevEndpoint = "p";
                isLive = true;
            }


            string url = string.Format("http://{0}.fbwineapps.com/BrandAPI/newsletter/newslettersignup/{1}/0", prodDevEndpoint, customSetting.value_1);

            List<KeyValuePair<string, string>> lstValues = new List<KeyValuePair<string, string>>();
            lstValues.Add(new KeyValuePair<string, string>("brand_code", customSetting.value_2));
            lstValues.Add(new KeyValuePair<string, string>("email", user_detail.email));
            lstValues.Add(new KeyValuePair<string, string>("source_name", customSetting.value_3));
            if (!string.IsNullOrEmpty(user_detail.first_name))
                lstValues.Add(new KeyValuePair<string, string>("first_name", user_detail.first_name));
            if (!string.IsNullOrEmpty(user_detail.last_name))
                lstValues.Add(new KeyValuePair<string, string>("last_name", user_detail.last_name));
            if (!string.IsNullOrEmpty(user_detail.phone_number))
                lstValues.Add(new KeyValuePair<string, string>("phone", user_detail.phone_number));
            if (user_detail.address != null)
            {
                if (!string.IsNullOrEmpty(user_detail.address.city))
                    lstValues.Add(new KeyValuePair<string, string>("city", user_detail.address.city));
                if (!string.IsNullOrEmpty(user_detail.address.state))
                    lstValues.Add(new KeyValuePair<string, string>("state", user_detail.address.state));
                if (!string.IsNullOrEmpty(user_detail.address.zip_code))
                    lstValues.Add(new KeyValuePair<string, string>("zip", user_detail.address.zip_code));
            }
            
            var content = new FormUrlEncodedContent(lstValues);

            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    if (data.ToLower().IndexOf("row inserted") > -1)
                        success = true;
                    else
                    {
                        throw new Exception("Request to EjGallio failed, error:" + data);
                    }
                }                  
                else
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    throw new Exception("Request to EjGallio failed, error:" + data);
                }
                   
            }


            return success;
        }

        public static string MakePostRequest(string url, string requestObj)
        {
            string jsonResp = string.Empty;

            HttpClient client = new HttpClient();

            using (var content = new StringContent(requestObj))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); // content-type header
                client.Timeout = TimeSpan.FromMinutes(5);
                var response = client.PostAsync(new Uri(url), content).Result;

                if (response.IsSuccessStatusCode)
                    jsonResp = response.Content.ReadAsStringAsync().Result;
            }


            return jsonResp;
        }

    }
}
