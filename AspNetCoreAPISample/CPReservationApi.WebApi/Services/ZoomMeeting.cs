using CPReservationApi.Common;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.Services
{
    public class ZoomMeeting
    {
        static private AppSettings _appSettings;
        const string _baseOAuthURL = "https://zoom.us/oauth/token";
        const string _createMeetingURL = "https://api.zoom.us/v2/users/me/meetings";
        const string _addMeetingRegistrantURL = "https://api.zoom.us/v2/meetings/{0}/registrants";
        const string _deleteMeetingURL = "https://api.zoom.us/v2/meetings/{0}";
        const string _deleteMeetingRegistrantURL = "https://api.zoom.us/v2/meetings/{0}/registrants/status";
        public ZoomMeeting(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
        }

        private static string GetZoomBasicAuthCode()
        {
            string authorization = "";
            try
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.system).ToList();
                authorization = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.system_zoom_basic_auth);
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "ZoomMeeting api:  " + ex.Message.ToString(), "",1,0);
            }

            return authorization;
        }

        public static async Task<ZoomToken> GetZoomToken(int memberId)
        {
            ZoomToken token = null;
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                token = eventDAL.GetZoomTokenByMember(memberId);

                if (token != null)
                {
                    string authCode = GetZoomBasicAuthCode();
                    //check if token has already expired. If yes then get refresh token back and update it in DB
                    if (DateTime.UtcNow >= token.Expires)
                    {
                        ZoomToken newToken = await RefeshAccessToken(token.RefreshToken, authCode, memberId);
                        token = newToken;
                    }
                }

            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetZoomToken api:  " + ex.Message.ToString(), "",1,memberId);
                token = null;
            }

            return token;
        }

        public static async Task<ZoomToken> RefeshAccessToken(string refreshToken, string authCode, int memberId)
        {
            HttpClient client = new HttpClient();
            ZoomToken token = null;
            try
            {
                string url = _baseOAuthURL + "?grant_type=refresh_token&refresh_token=" + refreshToken;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authCode);
                var response = await client.PostAsync(url, new StringContent(""));
                string jsonResp = "", errorResponse = "";
                if (response.IsSuccessStatusCode)
                {
                    jsonResp = response.Content.ReadAsStringAsync().Result;
                    var accesstokenResp = JsonConvert.DeserializeObject<RequestAccessTokenResponse>(jsonResp);
                    if (accesstokenResp != null)
                    {
                        token = new ZoomToken
                        {
                            AccessToken = accesstokenResp.access_token,
                            ExpiresIn = accesstokenResp.expires_in,
                            RefreshToken = accesstokenResp.refresh_token,
                            TokenType = accesstokenResp.token_type,
                            Scope = accesstokenResp.scope,
                            MemberId = memberId
                        };
                        EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                        eventDAL.UpdateZoomTokenOfMember(token);
                    }
                }
                else
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    errorResponse = response.Content.ReadAsStringAsync().Result;
                    logDAL.InsertLog("WebApi", "Error calling Zoom RefeshAccessToken method. Error returned:  " + errorResponse, "",1,memberId);
                }
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "RefeshAccessToken api:  " + ex.Message.ToString(), "",1,memberId);
                token = null;
            }

            return token;

        }

        public static async Task<bool> AddMeetingRegistrant(ZoomToken Token, int reservationId,long MeetingId, ViewModels.UserDetailViewModel user_detail, int slotId, int slotType, DateTime StartDate,int MeetingBehavior,int memberId,string join_url)
        {
            HttpClient client = new HttpClient();
            ZoomAddMeetingRegistrantResponse addMeetingRegistrantResp = new ZoomAddMeetingRegistrantResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            try
            {
                string url = string.Format(_addMeetingRegistrantURL, MeetingId.ToString());

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token.AccessToken);

                var addMeetingRegistrantRequest = new ZoomAddMeetingRegistrantRequest();

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                addMeetingRegistrantRequest.email = user_detail.email;
                addMeetingRegistrantRequest.first_name = user_detail.first_name;
                addMeetingRegistrantRequest.last_name = user_detail.last_name;
                addMeetingRegistrantRequest.city = user_detail.address.city;
                addMeetingRegistrantRequest.country = user_detail.address.country;
                addMeetingRegistrantRequest.zip = user_detail.address.zip_code;
                addMeetingRegistrantRequest.phone = user_detail.phone_number;

                string payLoadContent = JsonConvert.SerializeObject(addMeetingRegistrantRequest);

                bool ret = await AddMeetingRegistrant(Token.AccessToken, reservationId, MeetingId, payLoadContent, slotId, slotType, StartDate, MeetingBehavior, join_url);

                if (ret == false)
                {
                    string authCode = GetZoomBasicAuthCode();
                    ZoomToken newToken = await RefeshAccessToken(Token.RefreshToken, authCode, memberId);
                    await AddMeetingRegistrant(newToken.AccessToken, reservationId, MeetingId, payLoadContent, slotId, slotType, StartDate, MeetingBehavior, join_url);
                }
                    
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "AddMeetingRegistrant api:  " + ex.Message.ToString(), "",1,memberId);
            }

            return true;
        }

        public static async Task<bool> AddMeetingRegistrant(string AccessToken, int reservationId, long MeetingId, string payLoadContent, int slotId, int slotType, DateTime StartDate, int MeetingBehavior,string join_url)
        {
            HttpClient client = new HttpClient();
            ZoomAddMeetingRegistrantResponse addMeetingRegistrantResp = new ZoomAddMeetingRegistrantResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            int member_id = eventDAL.GetWineryIdByReservationId(reservationId);

            try
            {
                string url = string.Format(_addMeetingRegistrantURL, MeetingId.ToString());

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                var addMeetingRegistrantRequest = new ZoomAddMeetingRegistrantRequest();

                var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, stringContent);
                string jsonResp = "", errorResponse = "";
                if (response.IsSuccessStatusCode)
                {
                    jsonResp = response.Content.ReadAsStringAsync().Result;
                    addMeetingRegistrantResp = JsonConvert.DeserializeObject<ZoomAddMeetingRegistrantResponse>(jsonResp);
                    if (addMeetingRegistrantResp != null)
                    {
                        eventDAL.InsertZoomMeetingInfo(reservationId, slotId, slotType, MeetingId, join_url, addMeetingRegistrantResp.registrant_id, StartDate, MeetingBehavior);
                    }
                    logDAL.InsertLog("WebApi", "Zoom AddMeetingRegistrant method. responce:  " + jsonResp, "",3, member_id);
                }
                else
                {
                    errorResponse = response.Content.ReadAsStringAsync().Result;
                    logDAL.InsertLog("WebApi", "Error calling Zoom AddMeetingRegistrant method. Error returned:  " + errorResponse + ",ReservationId:-" + reservationId.ToString() + ",Request:-" + payLoadContent, "",1, member_id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "AddMeetingRegistrant api:  " + ex.Message.ToString() + ",ReservationId:-" + reservationId.ToString(), "",1, member_id);
                return false;
            }

            return true;
        }

        public static async Task<bool> DeleteMeeting(long MeetingId,int memberId)
        {
            HttpClient client = new HttpClient();

            try
            {
                ZoomToken token = await GetZoomToken(memberId);

                if (token == null)
                    return false;

                bool ret = await DeleteMeeting(token.AccessToken, MeetingId);

                if (ret == false)
                {
                    string authCode = GetZoomBasicAuthCode();
                    ZoomToken newToken = await RefeshAccessToken(token.RefreshToken, authCode, memberId);
                    await DeleteMeeting(newToken.AccessToken, MeetingId);
                }
                    

            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "DeleteMeeting api:  " + ex.Message.ToString() + ",MeetingId:-" + MeetingId.ToString(), "",1, memberId);
            }

            return true;
        }

        public static async Task<bool> DeleteMeeting(string AccessToken,long MeetingId)
        {
            HttpClient client = new HttpClient();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                string url = string.Format(_deleteMeetingURL, MeetingId.ToString());

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                var response = await client.DeleteAsync(url);
                string errorResponse = "";
                if (response.IsSuccessStatusCode)
                {
                    eventDAL.DeleteZoomMeetingInfoByMeetingId(MeetingId);
                    logDAL.InsertLog("WebApi", "DeleteMeeting api:  MeetingId:-" + MeetingId.ToString(), "",3, 0);
                    return true;
                }
                else
                {
                    errorResponse = response.Content.ReadAsStringAsync().Result;
                    logDAL.InsertLog("WebApi", "Error calling Zoom DeleteMeeting method. Error returned:  " + errorResponse + ",MeetingId:-" + MeetingId.ToString(), "",1,0);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "DeleteMeeting api:  " + ex.Message.ToString() + ",MeetingId:-" + MeetingId.ToString(), "",1,0);
                return false;
            }
        }

        public static async Task<ZoomCreateMeetingResponse> CreateMeeting(EventRuleModel eventRuleModel, ZoomToken token, DateTime StartDate, int memberId)
        {
            HttpClient client = new HttpClient();
            ZoomCreateMeetingResponse createMeetingResp = new ZoomCreateMeetingResponse();
            //long MeetingId = 0;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            try
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(memberId, (int)Common.Common.SettingGroup.member);
                int zoomMeetingIdType = Settings.GetIntValue(settingsGroup, Common.Common.SettingKey.member_zoom_meeting_id_setting);

                bool requirePassword = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_zoom_require_password);
                string zoomMeetingPassword = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_zoom_password);

                string url = _createMeetingURL;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                var createMeetingRequest = new ZoomCreateMeetingRequest();

                DateTime start_time = StartDate.Date.Add(eventRuleModel.StartTime);

                string starttime = Times.ToUniversalTime(start_time, (Times.TimeZone)eventRuleModel.TimeZoneId).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");

                createMeetingRequest.agenda = eventRuleModel.Description;
                createMeetingRequest.duration = eventRuleModel.Duration;
                createMeetingRequest.start_time = starttime;
                createMeetingRequest.topic = eventRuleModel.eventname;

                if (requirePassword && !string.IsNullOrEmpty(zoomMeetingPassword))
                {
                    createMeetingRequest.password = zoomMeetingPassword;
                }

                SettingsRequest settings = new SettingsRequest();

                settings.approval_type = 0;

                //Generate Automatically (0)
                //Personal Meeting ID XXX-XXX-XXX (1)
                if (zoomMeetingIdType == 1)
                {
                    settings.use_pmi = true;
                }

                createMeetingRequest.settings = settings;
                string payLoadContent = JsonConvert.SerializeObject(createMeetingRequest);

                var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, stringContent);
                string jsonResp = "", errorResponse = "";
                if (response.IsSuccessStatusCode)
                {
                    jsonResp = response.Content.ReadAsStringAsync().Result;
                    createMeetingResp = JsonConvert.DeserializeObject<ZoomCreateMeetingResponse>(jsonResp);
                    if (createMeetingResp != null)
                    {
                        //MeetingId = createMeetingResp.id;
                    }
                    logDAL.InsertLog("WebApi", "CreateMeeting api:  jsonResp-" + jsonResp, "",3,memberId);
                }
                else
                {
                    errorResponse = response.Content.ReadAsStringAsync().Result;
                    logDAL.InsertLog("WebApi", "Error calling Zoom CreateMeeting method. Error returned:  " + errorResponse + ",request: " + payLoadContent, "",1, memberId);
                    return null;
                }
            }
            catch (Exception ex)
            {
                
                logDAL.InsertLog("WebApi", "CreateMeeting api:  " + ex.Message.ToString(), "",1, memberId);
                return null;
            }

            return createMeetingResp;
        }

        public static async Task<long> CreateMeeting(int slotId, int slotType,DateTime StartDate, int reservationId, ViewModels.UserDetailViewModel user_detail,int memberId)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            HttpClient client = new HttpClient();
            ZoomCreateMeetingResponse createMeetingResp = new ZoomCreateMeetingResponse();
            long MeetingId = 0;
            string join_url = string.Empty;

            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                EventRuleModel eventRuleModel = eventDAL.GetEventcapacity(slotId, slotType);

                if (eventRuleModel.MeetingBehavior == 0)
                    return 0;

                ZoomToken token = await GetZoomToken(memberId);

                if (token == null)
                    return 0;

                bool IsCreateMeeting = false;
                ZoomMeetingInfo zoomMeetingInfo = new ZoomMeetingInfo();

                if (eventRuleModel.MeetingBehavior == 1) //Unique Meeting
                {
                    IsCreateMeeting = true;
                }
                else if (eventRuleModel.MeetingBehavior == 2) //Add to Existing
                {
                    zoomMeetingInfo = eventDAL.GetZoomMeetingInfo(slotId, slotType, StartDate);
                }

                if (IsCreateMeeting || zoomMeetingInfo.Id == 0)
                {
                    ZoomCreateMeetingResponse resp = await CreateMeeting(eventRuleModel, token, StartDate,memberId);

                    if (resp == null)
                    {
                        string authCode = GetZoomBasicAuthCode();
                        ZoomToken newToken = await RefeshAccessToken(token.RefreshToken, authCode, memberId);
                        resp = await CreateMeeting(eventRuleModel, newToken, StartDate,memberId);
                    }

                    if (resp != null && resp.id>0)
                    {
                        MeetingId = resp.id;
                        join_url = resp.join_url;
                    }
                }

                if (MeetingId > 0)
                {
                    eventDAL.InsertZoomMeetingInfo(0, slotId, slotType, MeetingId,join_url, "", StartDate, eventRuleModel.MeetingBehavior);
                    bool ret = await AddMeetingRegistrant(token, reservationId, MeetingId, user_detail, slotId, slotType, StartDate, eventRuleModel.MeetingBehavior,memberId, join_url);
                }
                else if (zoomMeetingInfo.Id >0)
                {
                    MeetingId = zoomMeetingInfo.MeetingId;
                    bool ret = await AddMeetingRegistrant(token, reservationId, zoomMeetingInfo.MeetingId, user_detail, slotId, slotType, StartDate, eventRuleModel.MeetingBehavior, memberId, join_url);
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {
                
                logDAL.InsertLog("WebApi", "CreateMeeting api:  " + ex.Message.ToString(), "",1, memberId);
                return 0;
            }

            return MeetingId;
        }

        public static async Task<bool> DeleteMeetingRegistrant(int memberId, long MeetingId,string registrantid,string registrantemail,int reservationId)
        {
            HttpClient client = new HttpClient();

            try
            {
                ZoomToken token = await GetZoomToken(memberId);

                if (token == null)
                    return false;

                var zoomMeetingRegistrantStatusRequest = new ZoomMeetingRegistrantStatusRequest();

                zoomMeetingRegistrantStatusRequest.action = "cancel";

               List<Registrant> listregistrant = new List<Registrant>();
                Registrant registrant = new Registrant();

                registrant.id = registrantid;
                registrant.email = registrantemail;

                listregistrant.Add(registrant);

                zoomMeetingRegistrantStatusRequest.registrants = listregistrant;

                string payLoadContent = JsonConvert.SerializeObject(zoomMeetingRegistrantStatusRequest);

                bool ret = await DeleteMeetingRegistrant(payLoadContent, token.AccessToken, MeetingId,reservationId);

                if (ret == false)
                {
                    string authCode = GetZoomBasicAuthCode();
                    ZoomToken newToken = await RefeshAccessToken(token.RefreshToken, authCode, memberId);
                    await DeleteMeetingRegistrant(payLoadContent, newToken.AccessToken, MeetingId, reservationId);
                }
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "DeleteMeetingRegistrant api:  " + ex.Message.ToString(), "",1, memberId);
                return false;
            }
            return true;
        }

        public static async Task<bool> DeleteMeetingRegistrant(string payLoadContent,string AccessToken,long MeetingId, int reservationId)
        {
            HttpClient client = new HttpClient();

            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            int memberId = eventDAL.GetWineryIdByReservationId(reservationId);
            try
            {
                string url = string.Format(_deleteMeetingRegistrantURL, MeetingId.ToString());

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                var zoomMeetingRegistrantStatusRequest = new ZoomMeetingRegistrantStatusRequest();

                var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(url, stringContent);
                string errorResponse = "";
                if (response.IsSuccessStatusCode)
                {
                    eventDAL.DeleteZoomMeetingInfoByReservationId(reservationId);
                    logDAL.InsertLog("WebApi", "DeleteMeetingRegistrant api ReservationId:-" + reservationId.ToString(), "",3, memberId);
                    return true;
                }
                else
                {
                    
                    errorResponse = response.Content.ReadAsStringAsync().Result;
                    logDAL.InsertLog("WebApi", "Error calling Zoom DeleteMeetingRegistrant method. Error returned:  " + errorResponse, "",1, memberId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "DeleteMeetingRegistrant api:  " + ex.Message.ToString(), "",1, memberId);
                return false;
            }
        }
    }
}
