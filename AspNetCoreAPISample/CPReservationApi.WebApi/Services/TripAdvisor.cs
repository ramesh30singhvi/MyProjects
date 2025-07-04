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
using Newtonsoft.Json.Linq;

namespace CPReservationApi.WebApi.Services
{
    public class TripAdvisor
    {
        static private ViewModels.AppSettings _appSettings;

        public TripAdvisor(IOptions<ViewModels.AppSettings> settings)
        {
            _appSettings = settings.Value;
        }
        const string tripAdvisorAPIURL = "https://api.tripadvisor.com/api/partner/2.0/location/{0}?key={1}";


        public async static Task<bool> UpdateTripadvisorReviews()
        {
            bool isSuccess = false;
            string key = GetTripAdvisorKey();

            if (!string.IsNullOrWhiteSpace(key))
            {
                //get all active members with TripAdvisorIds
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                List<TripAdvisorMember> tripAdvisorMembers = eventDAL.GetTripAdvisorActiveMembers();

                if (tripAdvisorMembers != null && tripAdvisorMembers.Count > 0)
                {
                    foreach (TripAdvisorMember member in tripAdvisorMembers)
                    {
                        try
                        {
                            string requestUri = String.Format(tripAdvisorAPIURL, member.TripAdvisorId, key);

                            HttpClient client = new HttpClient();
                            client.Timeout = TimeSpan.FromMinutes(5);
                            using (var response = client.GetAsync(requestUri).Result)
                            {
                                var varResponse = response.Content.ReadAsStringAsync().Result;
                                dynamic tripAdvData = JObject.Parse(varResponse);
                                eventDAL.UpdateTripAdvisorRating(member.MemberId, varResponse, Convert.ToString(tripAdvData["rating"]));
                            }
                        }
                        catch (Exception ex)
                        {
                            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                            logDAL.InsertLog("WebApi", "UpdateTripadvisorReviews:  MemberId:-" + member.MemberId.ToString() + ",Message:-" + ex.Message.ToString(), "", 1, member.MemberId);
                        }
                    }
                    isSuccess = true;
                }
            }

            return isSuccess;
        }

        #region Supporting Methods

        private static string GetTripAdvisorKey()
        {
            string tripAdvisorKey = "";
            try
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.TripAdvisor).ToList();
                tripAdvisorKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.tripadvisor_content_api);
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "TripAdvisor api:  " + ex.Message.ToString(), "",1,0);
            }

            return tripAdvisorKey;
        }



        #endregion
    }
}
