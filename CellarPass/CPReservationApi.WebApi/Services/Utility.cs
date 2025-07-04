using CPReservationApi.WebApi.ViewModels;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using static eWineryWebServices.EWSWebServicesSoapClient;
using CPReservationApi.DAL;
using Haukcode.TaxifyDotNet;
using CPReservationApi.Model;
using CPReservationApi.Common;
using System.Linq;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Lookups.V1;
using Twilio.Rest.Api.V2010.Account;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using static CPReservationApi.Common.Payments;
using System.IO;
using RestSharp;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Net;
using Google.Maps;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ClosedXML.Excel;
using System.Security.Policy;
using ImageResizer.ExtensionMethods;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using eWineryWebServices;

namespace CPReservationApi.WebApi.Services
{
    public class Utility
    {
        public static MobileNumberStatus SMSVerified_System(string phone)
        {
            MobileNumberStatus mobileNumberStatus = MobileNumberStatus.unverified;

            try
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.twilio).ToList();
                string twilio_ACCOUNTSID = string.Empty;
                string twilio_AUTHTOKEN = string.Empty;
                bool twilio_DisableVerificationService = false;
                //string phone = "+15108675309";
                if ((settingsGroup != null))
                {
                    if (settingsGroup.Count > 0)
                    {
                        twilio_ACCOUNTSID = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.twilio_ACCOUNTSID);
                        twilio_AUTHTOKEN = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.twilio_AUTHTOKEN);
                        twilio_DisableVerificationService = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.twilio_DisableVerificationService);

                        if (twilio_ACCOUNTSID.Length > 0 && twilio_AUTHTOKEN.Length > 0 && twilio_DisableVerificationService == false)
                        {
                            TwilioClient.Init(twilio_ACCOUNTSID, twilio_AUTHTOKEN);

                            var phoneNumber = PhoneNumberResource.Fetch(
                                new PhoneNumber(phone),
                                type: new List<string> { "carrier" });

                            if (phoneNumber.Carrier["error_code"] == null)
                                mobileNumberStatus = MobileNumberStatus.verified;
                            else
                                mobileNumberStatus = MobileNumberStatus.failed;
                            Console.WriteLine(phoneNumber.Carrier["name"]);
                            Console.WriteLine(phoneNumber.Carrier["type"]);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                mobileNumberStatus = MobileNumberStatus.failed;
                string err = e.Message;
            }


            return mobileNumberStatus;

        }

        public static bool SMSSend_System(string smsbody, string smsfromphone, string smstophone)
        {
            try
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.twilio).ToList();
                string twilio_ACCOUNTSID = string.Empty;
                string twilio_AUTHTOKEN = string.Empty;
                bool twilio_DisableVerificationService = false;
                if (settingsGroup != null)
                {
                    if (settingsGroup.Count > 0)
                    {
                        twilio_ACCOUNTSID = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.twilio_ACCOUNTSID);
                        twilio_AUTHTOKEN = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.twilio_AUTHTOKEN);
                        twilio_DisableVerificationService = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.twilio_DisableVerificationService);

                        if (twilio_ACCOUNTSID.Length > 0 && twilio_AUTHTOKEN.Length > 0 && twilio_DisableVerificationService == false)
                        {
                            TwilioClient.Init(twilio_ACCOUNTSID, twilio_AUTHTOKEN);

                            var message = MessageResource.Create(
                                body: smsbody,
                                from: new PhoneNumber(smsfromphone),
                                to: new PhoneNumber(smstophone)
                            );
                        }

                    }
                }
            }
            catch (Exception e)
            {
                string err = e.Message;
            }
            return true;
        }

        public static string GenerateEmailButton(string buttonText, string buttonLink, string buttonColor, string buttonBorderColor, string buttonPaddingWidth, string buttonPaddingHeight, string buttonTextColor)
        {
            string buttonHtml = "";
            //buttonHtml = string.Format("<table width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\"> <tr> <td> <table border=\"0\" cellspacing=\"0\" cellpadding=\"0\"> <tr> <td align=\"center\" style=\"-webkit-border-radius: 3px; -moz-border-radius: 3px; border-radius: 3px;\" bgcolor=\"{2}\"><a href=\"{1}\" target=\"_blank\" style=\"font-size: 16px; font-family: Helvetica, Arial, sans-serif; color: {6}; text-decoration: none; text-decoration: none; -webkit-border-radius: 3px; -moz-border-radius: 3px; border-radius: 3px; padding: {5} {4}; border: 1px solid {3}; display: inline-block;\">{0}</a></td></tr></table> </td></tr></table>", buttonText, buttonLink, buttonColor, buttonBorderColor, buttonWidth, buttonHeight, buttonTextColor);
            buttonHtml = "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"border-collapse:separate;line-height:100%;\">";
            buttonHtml += "<tr>";
            buttonHtml += String.Format("<td align=\"center\" bgcolor=\"{0}\" role=\"presentation\" style=\"border:none;border-radius:6px;cursor:auto;padding:{1} {2};background:{3};\" valign=\"middle\">", buttonColor, buttonPaddingHeight, buttonPaddingWidth, buttonColor);
            buttonHtml += String.Format("<a class=\"cssbutton\" href=\"{0}\" style=\"background:{1};color:{2};font-family:Helvetica, sans - serif;font-size:   18px;font-weight:600;line-height:120%;Margin:0;text-decoration:none;text-transform:none;\" target=\"_blank\">", buttonLink, buttonColor, buttonTextColor);
            buttonHtml += buttonText;
            buttonHtml += "</a>";
            buttonHtml += "</td>";
            buttonHtml += "</tr>";
            buttonHtml += "</table>";
            return buttonHtml;
        }

        public static string GetGoogleTimeZoneNameById(int timezoneId)
        {
            string timezoneName = "America/Los_Angeles";
            switch (timezoneId)
            {
                case 5:
                    timezoneName = "America/Los_Angeles";
                    break;
                case 4:
                    timezoneName = "America/Denver";
                    break;
                case 1:
                    timezoneName = "America/Denver";
                    break;
                case 3:
                    timezoneName = "America/Chicago";
                    break;
                case 2:
                    timezoneName = "America/New_York";
                    break;
            }
            return timezoneName;
        }

        public static string GetGoogleStatusTextbyId(int status)
        {
            string statusName = "";
            switch (status)
            {
                case 0:
                    statusName = "Confirmed";
                    break;
                case 1:
                    statusName = "Checked-in";
                    break;
                case 2:
                    statusName = "Cancelled";
                    break;
                case 3:
                    statusName = "No Show";
                    break;
                case 4:
                    statusName = "Rescheduled";
                    break;
                case 5:
                    statusName = "Guest Delayed";
                    break;
                case 6:
                    statusName = "Updated";
                    break;
                case 7:
                    statusName = "Yelp Temporary";
                    break;
                case 8:
                    statusName = "Initiated";
                    break;
            }
            return statusName;
        }

        public static string GetFriendlyURL(string eventTitle, int eventId, string eventURL)
        {
            if (eventTitle == null || eventId == 0)
            {
                return "";
            }

            string url = "";

            if (eventURL != "")
            {
                eventURL = eventURL.Replace("events/", "");
                url = string.Format("events/{0}", eventURL);
            }
            else
                url = string.Format("events/{0}-{1}", Regex.Replace(eventTitle, "[^A-Za-z0-9 ]+", "").Replace("  ", " ").Replace(" ", "-").TrimEnd('-'), eventId);

            return url.ToLower();
        }

        public static string GetBannerImageHtmlByWineryId(int WineryId)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            string bannerImageHtml = string.Empty;
            string bannerImage = eventDAL.GetImageNameByWineryID(WineryId);
            string path = "https://cdncellarpass.blob.core.windows.net/photos/profiles/";
            string Image = "https://cdncellarpass.blob.core.windows.net/photos/profiles/default-profile-banner.jpg";

            if (!string.IsNullOrEmpty(bannerImage))
            {
                Image = path + bannerImage;
            }

            bannerImageHtml = string.Format("<img src=\"{0}\" width=\"600\" alt=\" border=\"0\" style=\"font-family:Arial, sans-serif;font-size:14px;line-height:16px;color:#ffffff;display:block;max-width:600px;\" class=\"em_full_img\">", Image);

            return bannerImageHtml;
        }

        public static string GetBannerImageHtmlForTicketEvent(string bannerImage)
        {

            string imgPath = StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure) + "/";
            //+ bannerImage + "?clearcache=" + DateTime.UtcNow.Millisecond.ToString();
            string defaultBannerImage = "default-ticket-event-banner.jpg";

            if (string.IsNullOrWhiteSpace(bannerImage))
            {
                bannerImage = imgPath + defaultBannerImage;
            }
            else
            {
                bannerImage = imgPath + bannerImage + "?clearcache=" + DateTime.UtcNow.Millisecond.ToString();
            }


            string bannerImageHtml = string.Format("<img src=\"{0}\" width=\"600\" alt=\" border=\"0\" style=\"font-family:Arial, sans-serif;font-size:14px;line-height:16px;color:#ffffff;display:block;max-width:600px;\" class=\"em_full_img\">", bannerImage);

            return bannerImageHtml;
        }

        public static string GetBannerImageForTicketEvent(string bannerImage)
        {

            string imgPath = StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure) + "/";
            //+ bannerImage + "?clearcache=" + DateTime.UtcNow.Millisecond.ToString();
            string defaultBannerImage = "default-ticket-event-banner.jpg";

            if (string.IsNullOrWhiteSpace(bannerImage))
            {
                bannerImage = imgPath + defaultBannerImage;
            }
            else
            {
                bannerImage = imgPath + bannerImage + "?clearcache=" + DateTime.UtcNow.Millisecond.ToString();
            }

            return bannerImage;
        }

        public static bool purcgecdnasset(string FilePath)
        {
            bool ret = false;
            try
            {
                var client = new RestClient("https://login.microsoftonline.com/83361567-0f76-4010-b61b-8df59822e433/oauth2/token");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("grant_type", "client_credentials");
                request.AddParameter("client_id", "a292e0c9-ea46-497f-a19c-eb62a557ac64");
                request.AddParameter("client_secret", "");
                request.AddParameter("resource", "https://management.core.windows.net");

                IRestResponse response = client.Execute(request);

                var purgeClient = new RestClient("https://management.azure.com/subscriptions/f4864dcf-94f5-4f2c-962d-d623c4c5e746/resourceGroups/cellarpass_v3/providers/Microsoft.Cdn/profiles/cdncellarpass/endpoints/cdncellarpass/purge?api-version=2019-04-15");

                var purgeRequest = new RestRequest(Method.POST);
                purgeRequest.AddHeader("content-type", "application/json");
                var content = JsonConvert.DeserializeObject<dynamic>(response.Content);
                purgeRequest.AddHeader("authorization", "Bearer" + " " + content.access_token);
                var listing = new List<string>();
                listing.Add(FilePath);
                var data = @"{ 'contentPaths': " + JsonConvert.SerializeObject(listing) + "}";
                purgeRequest.AddParameter("application/json", data, ParameterType.RequestBody);
                IRestResponse purgeResponse = purgeClient.Execute(purgeRequest);

                if (purgeResponse.StatusCode == System.Net.HttpStatusCode.Accepted)
                    ret = true;
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("purgecdnasset", "Errors:" + ex.Message, "", 1, 0);

                ret = false;
            }

            return ret;
        }

        public static string GenerateLogoImageTag(int memberId)
        {
            string imageTag = "";
            string imagePath = StringHelpers.GetImagePath(ImageType.memberImage, ImagePathType.azure) + "/";
            if (memberId > 0)
            {
                imageTag = string.Format("<img style=\"display:block;\" alt=\"Business Logo\" src=\"{0}{1}_logo_crop.png\" border=\"0\" class=\"em_logo_img\" height=\"100\">", imagePath, memberId);
            }

            return imageTag;
        }


        public async static Task<string> GetMapImageHtmlByLocation(int locationId, string googleAPIKey)
        {

            string mapImageFOlder = StringHelpers.GetImagePath(ImageType.LocationMaps, ImagePathType.azure) + "/";
            string mapInageURl = mapImageFOlder + locationId.ToString() + "_dot.jpg";
            string mapHTML = "";
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            try
            {
                //check if image exists
                bool imageExists = CheckIfCDNImagexists(mapInageURl);
                //get data from DB for location
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var location = eventDAL.GetLocationMapDataByID(locationId);
                if (!imageExists)
                {


                    if (location != null)
                    {
                        bool isSuccess = await CreateMapImageForLocation(location, googleAPIKey);

                        if (isSuccess)
                        {
                            mapHTML = string.Format("<a href=\"{0}\"><img align=\"center\" border=\"0\" src=\"{1}\" alt=\"Map Image\" title=\"Map Image\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 100%;max-width: 280px;\" width=\"280\" /></a>", location.map_and_directions_url, mapInageURl);
                        }
                    }
                    //create and save image
                }
                else
                {
                    if (location != null)
                        mapHTML = string.Format("<a href=\"{0}\"><img align=\"center\" border=\"0\" src=\"{1}\" alt=\"Map Image\" title=\"Map Image\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 100%;max-width: 280px;\" width=\"280\" /></a>", location.map_and_directions_url, mapInageURl);

                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "GetMapImageHtmlByLocation:  " + ex.Message.ToString(), "Web Api", 1, 0);
            }
            return mapHTML;
        }

        public async static Task<bool> CreateMapImageForLocation(LocationMapModel location, string googleAPIKey)
        {
            string mapImageFOlder = StringHelpers.GetImagePath(ImageType.LocationMaps, ImagePathType.azure) + "/";
            string mapImageName = location.location_id.ToString() + "_dot.jpg";
            string mapInageURl = mapImageFOlder + mapImageName;
            bool isSuccess = false;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            try
            {
                GoogleSigned.AssignAllServices(new GoogleSigned(googleAPIKey));
                var request = new Google.Maps.StaticMaps.StaticMapRequest();
                request.Size = new MapSize(400, 400);
                request.MapType = MapTypes.Roadmap;
                request.Zoom = 12;
                string locationCoordinates = string.Format("{0},{1}", location.geo_latitude, location.geo_longitude);
                request.Center = new Location(locationCoordinates);

                MapMarkersCollection mapMarkers = new MapMarkersCollection();

                Location l = new Location(locationCoordinates);

                mapMarkers.Add(l);

                request.Markers = mapMarkers;

                var response = await new Google.Maps.StaticMaps.StaticMapService().GetImageAsync(request);

                if (response != null)
                {
                    UploadFileToStorage(response, mapImageName, ImageType.LocationMaps);
                    isSuccess = true;
                }
                else
                { isSuccess = false; }
                //UploadFileToStorage

            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "CreateMapImageForLocation:  " + ex.Message.ToString(), "Web Api", 1, 0);
            }

            return isSuccess;
        }

        public async static Task<string> GetMapImageHtmlByLocation(string googleAPIKey, string geo_latitude, string geo_longitude, int event_id,string googleurl)
        {

            string mapImageFOlder = StringHelpers.GetImagePath(ImageType.LocationMaps, ImagePathType.azure) + "/";
            string mapInageURl = mapImageFOlder + event_id.ToString() + "_dot.jpg";
            string mapHTML = "";
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            try
            {
                //check if image exists
                bool imageExists = CheckIfCDNImagexists(mapInageURl);
                //get data from DB for location
                if (!imageExists)
                {
                    bool isSuccess = await CreateMapImageForLocation(googleAPIKey, geo_latitude, geo_longitude, event_id);

                    if (isSuccess)
                    {
                        mapHTML = string.Format("<a href=\"{0}\"><img align=\"center\" border=\"0\" src=\"{1}\" alt=\"Map Image\" title=\"Map Image\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 100%;max-width: 280px;\" width=\"280\" /></a>", googleurl, mapInageURl);
                    }
                }
                else
                {
                    mapHTML = string.Format("<a href=\"{0}\"><img align=\"center\" border=\"0\" src=\"{1}\" alt=\"Map Image\" title=\"Map Image\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 100%;max-width: 280px;\" width=\"280\" /></a>", googleurl, mapInageURl);
                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "GetMapImageHtmlByLocation:  " + ex.Message.ToString(), "Web Api", 1, 0);
            }
            return mapHTML;
        }

        public async static Task<bool> CreateMapImageForLocation(string googleAPIKey,string geo_latitude,string geo_longitude,int event_id)
        {
            string mapImageFOlder = StringHelpers.GetImagePath(ImageType.LocationMaps, ImagePathType.azure) + "/";
            string mapImageName = event_id.ToString() + "_dot.jpg";
            string mapInageURl = mapImageFOlder + mapImageName;
            bool isSuccess = false;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            try
            {
                GoogleSigned.AssignAllServices(new GoogleSigned(googleAPIKey));
                var request = new Google.Maps.StaticMaps.StaticMapRequest();
                request.Size = new MapSize(400, 400);
                request.MapType = MapTypes.Roadmap;
                request.Zoom = 12;
                string locationCoordinates = string.Format("{0},{1}", geo_latitude, geo_longitude);
                request.Center = new Location(locationCoordinates);

                MapMarkersCollection mapMarkers = new MapMarkersCollection();

                Location l = new Location(locationCoordinates);

                mapMarkers.Add(l);

                request.Markers = mapMarkers;

                var response = await new Google.Maps.StaticMaps.StaticMapService().GetImageAsync(request);

                if (response != null)
                {
                    UploadFileToStorage(response, mapImageName, ImageType.LocationMaps);
                    isSuccess = true;
                }
                else
                { isSuccess = false; }
                //UploadFileToStorage

            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "CreateMapImageForLocation:  " + ex.Message.ToString(), "Web Api", 1, 0);
            }

            return isSuccess;
        }

        public static bool CheckIfCDNImagexists(string cdnImagePath)
        {
            bool imageExists = false;
            HttpWebResponse response = null;
            var request = (HttpWebRequest)WebRequest.Create(cdnImagePath);
            request.Method = "HEAD";


            try
            {
                response = (HttpWebResponse)request.GetResponse();
                imageExists = true;
            }
            catch (WebException ex)
            {
                imageExists = false;
            }
            finally
            {
                // Don't forget to close your response.
                if (response != null)
                {
                    response.Close();
                }
            }

            return imageExists;
        }

        public async Task<decimal> GetTax(int event_id, int quantity, decimal rsvpTotal, int location_id = 0, bool ignore_sales_tax = true)
        {
            decimal Tax = 0;
            int member_id = 0;
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                LocationModel locationModel = new LocationModel();

                if (event_id > 0)
                {
                    EventModel eventModel = eventDAL.GetEventById(event_id);
                    member_id = eventModel.MemberID;
                    if (ignore_sales_tax == false) //eventModel.ChargeSalesTax || 
                    {
                        locationModel = eventDAL.GetLocationByID(eventModel.LocationId);
                        location_id = locationModel.location_id;
                    }
                }
                else if (location_id > 0)
                {
                    locationModel = eventDAL.GetLocationByID(location_id);
                    member_id = locationModel.member_id;

                    if (!eventDAL.GetWineryById(locationModel.member_id).ChargeSalesTaxOnPrivateEvents)
                        location_id = 0;
                }

                if (location_id > 0)
                {
                    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.taxify).ToList();
                    string apikey = string.Empty;

                    if (settingsGroup != null)
                    {
                        if (settingsGroup.Count > 0)
                        {
                            apikey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.taxify_ApiKey);
                        }
                    }

                    TaxifyClient taxservices = new TaxifyClient(apikey);

                    List<Haukcode.TaxifyDotNet.Models.TaxLineDetail> lines = new List<Haukcode.TaxifyDotNet.Models.TaxLineDetail>();

                    Haukcode.TaxifyDotNet.Models.TaxLineDetail item = new Haukcode.TaxifyDotNet.Models.TaxLineDetail();

                    item.Quantity = quantity;
                    item.ActualExtendedPrice = rsvpTotal;
                    item.ItemKey = "test";

                    lines.Add(item);

                    Haukcode.TaxifyDotNet.Models.Address destinationaddress = new Haukcode.TaxifyDotNet.Models.Address();

                    destinationaddress.PostalCode = locationModel.address.zip_code;
                    destinationaddress.Street1 = locationModel.address.address_1;
                    destinationaddress.City = locationModel.address.city;
                    destinationaddress.Region = locationModel.address.state;

                    Haukcode.TaxifyDotNet.Messages.CalculateTaxResponse responce = null;
                    responce = await taxservices.CalculateTaxAsync(lines.ToArray(), destinationaddress);

                    if (responce.ResponseStatus == 0)
                    {
                        string strError = string.Empty;
                        if (responce.Errors.Length > 0)
                        {
                            foreach (var error in responce.Errors)
                            {
                                strError += error.Code + ":" + error.Message + Environment.NewLine;
                            }
                            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                            logDAL.InsertLog("Taxify Tax Error", "Errors:" + strError, "", 1, member_id);
                        }
                    }
                    else
                        Tax = (decimal)responce.SalesTaxAmount;
                }
            }
            catch { }

            return Tax;
        }

        public async Task<decimal> GetTaxByEventId(int member_id,decimal eventPrice,string locationzip_code,string locationaddress_1,string locationcity,string locationstate)
        {
            decimal Tax = 0;
            try
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString_readonly);

                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.taxify).ToList();
                string apikey = string.Empty;

                if (settingsGroup != null)
                {
                    if (settingsGroup.Count > 0)
                    {
                        apikey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.taxify_ApiKey);
                    }
                }

                TaxifyClient taxservices = new TaxifyClient(apikey);

                List<Haukcode.TaxifyDotNet.Models.TaxLineDetail> lines = new List<Haukcode.TaxifyDotNet.Models.TaxLineDetail>();

                Haukcode.TaxifyDotNet.Models.TaxLineDetail item = new Haukcode.TaxifyDotNet.Models.TaxLineDetail();

                item.Quantity = 1;
                item.ActualExtendedPrice = eventPrice;
                item.ItemKey = "test";

                lines.Add(item);

                Haukcode.TaxifyDotNet.Models.Address destinationaddress = new Haukcode.TaxifyDotNet.Models.Address();

                destinationaddress.PostalCode = locationzip_code;
                destinationaddress.Street1 = locationaddress_1;
                destinationaddress.City = locationcity;
                destinationaddress.Region = locationstate;

                Haukcode.TaxifyDotNet.Messages.CalculateTaxResponse responce = null;
                responce = await taxservices.CalculateTaxAsync(lines.ToArray(), destinationaddress);

                if (responce.ResponseStatus == 0)
                {
                    string strError = string.Empty;
                    if (responce.Errors.Length > 0)
                    {
                        foreach (var error in responce.Errors)
                        {
                            strError += error.Code + ":" + error.Message + Environment.NewLine;
                        }
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                        logDAL.InsertLog("Taxify Tax Error", "Errors:" + strError, "", 1, member_id);
                    }
                }
                else
                    Tax = (decimal)responce.SalesTaxAmount;
            }
            catch { }

            return Tax;
        }

        public async Task<decimal> GetTaxByLocationId(int location_id)
        {
            decimal Tax = 0;
            int member_id = 0;
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                LocationModel locationModel = new LocationModel();
                locationModel = eventDAL.GetLocationByID(location_id);

                if (location_id > 0)
                {
                    member_id = locationModel.member_id;

                    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.taxify).ToList();
                    string apikey = string.Empty;

                    if (settingsGroup != null)
                    {
                        if (settingsGroup.Count > 0)
                        {
                            apikey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.taxify_ApiKey);
                        }
                    }

                    TaxifyClient taxservices = new TaxifyClient(apikey);

                    List<Haukcode.TaxifyDotNet.Models.TaxLineDetail> lines = new List<Haukcode.TaxifyDotNet.Models.TaxLineDetail>();

                    Haukcode.TaxifyDotNet.Models.TaxLineDetail item = new Haukcode.TaxifyDotNet.Models.TaxLineDetail();

                    item.Quantity = 1;
                    item.ActualExtendedPrice = 100;
                    item.ItemKey = "test";

                    lines.Add(item);

                    Haukcode.TaxifyDotNet.Models.Address destinationaddress = new Haukcode.TaxifyDotNet.Models.Address();

                    destinationaddress.PostalCode = locationModel.address.zip_code;
                    destinationaddress.Street1 = locationModel.address.address_1;
                    destinationaddress.City = locationModel.address.city;
                    destinationaddress.Region = locationModel.address.state;

                    Haukcode.TaxifyDotNet.Messages.CalculateTaxResponse responce = null;
                    responce = await taxservices.CalculateTaxAsync(lines.ToArray(), destinationaddress);

                    if (responce.ResponseStatus == 0)
                    {
                        string strError = string.Empty;
                        if (responce.Errors.Length > 0)
                        {
                            foreach (var error in responce.Errors)
                            {
                                strError += error.Code + ":" + error.Message + Environment.NewLine;
                            }
                            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                            logDAL.InsertLog("Taxify Tax Error", "Errors:" + strError, "", 1, member_id);
                        }
                    }
                    else
                        Tax = (decimal)responce.SalesTaxAmount;
                }
            }
            catch { }

            return Tax;
        }

        public async Task<decimal> GetTicketTax(int event_id, int quantity, decimal ticketTotal)
        {
            decimal Tax = 0;
            int member_id = 0;
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
            SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

            //LocationModel locationModel = new LocationModel();

            //if (event_id > 0)
            //{
            //    var LocationId = ticketDAL.GetVenueLocationIdByEventId(event_id);
            //    if (LocationId > 0)
            //    {
            //        locationModel = eventDAL.GetLocationByID(LocationId);
            //    }
            //}

            if (event_id > 0)
            {
                string eventPassword = string.Empty;
                TicketEventDetailModel ticketEvent = ticketDAL.GetTicketEventDetailsById(event_id, ref eventPassword);
                member_id = ticketEvent.member_id;

                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.taxify).ToList();
                string apikey = string.Empty;

                if (settingsGroup != null)
                {
                    if (settingsGroup.Count > 0)
                    {
                        apikey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.taxify_ApiKey);
                    }
                }

                TaxifyClient taxservices = new TaxifyClient(apikey);

                List<Haukcode.TaxifyDotNet.Models.TaxLineDetail> lines = new List<Haukcode.TaxifyDotNet.Models.TaxLineDetail>();

                Haukcode.TaxifyDotNet.Models.TaxLineDetail item = new Haukcode.TaxifyDotNet.Models.TaxLineDetail();

                item.Quantity = quantity;
                item.ActualExtendedPrice = ticketTotal;
                item.ItemKey = "test";

                lines.Add(item);

                Haukcode.TaxifyDotNet.Models.Address destinationaddress = new Haukcode.TaxifyDotNet.Models.Address();

                destinationaddress.PostalCode = ticketEvent.venue_zip;
                destinationaddress.Street1 = ticketEvent.venue_address_1;
                destinationaddress.City = ticketEvent.venue_city;
                destinationaddress.Region = ticketEvent.venue_state;

                Haukcode.TaxifyDotNet.Messages.CalculateTaxResponse responce = null;
                responce = await taxservices.CalculateTaxAsync(lines.ToArray(), destinationaddress);

                if (responce.ResponseStatus == 0)
                {
                    string strError = string.Empty;
                    if (responce.Errors.Length > 0)
                    {
                        foreach (var error in responce.Errors)
                        {
                            strError += error.Code + ":" + error.Message + Environment.NewLine;
                        }
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                        logDAL.InsertLog("Taxify Tax Error", "Errors:" + strError, "", 1, member_id);
                    }
                }
                else
                    Tax = (decimal)responce.SalesTaxAmount;
            }

            return Tax;
        }


        public static decimal CalculateFeeTotal(decimal ticketPrice, decimal serviceFee, decimal perTicketFee, decimal maxTicketFee, decimal gratuity)
        {
            decimal feeTotal = (decimal)0;

            ticketPrice = ticketPrice + gratuity;

            if (ticketPrice > (decimal)0)
            {
                feeTotal = (ticketPrice * serviceFee) + perTicketFee;

                if (maxTicketFee > (decimal)0)
                {
                    if (maxTicketFee < feeTotal)
                        feeTotal = maxTicketFee;
                }
            }

            return decimal.Round(feeTotal, 2, MidpointRounding.AwayFromZero);
        }

        public static decimal CalculateGratuity(decimal price, decimal gratuityPercentage)
        {
            // Gratuity
            decimal Gratuity = (decimal)0;
            // calculate tip
            if ((gratuityPercentage > (decimal)0) & (price > (decimal)0))
                Gratuity = Math.Round((price * gratuityPercentage), 2);

            return Gratuity;
        }


        public static List<DepositpolicyModel> GetChargeFeeWithoutPayment(int defaultvalue)
        {
            var depositpolicyModel = new List<DepositpolicyModel>();
            depositpolicyModel.AddRange(new DepositpolicyModel[] {
                    new DepositpolicyModel{
                        id = 0,
                        name="No Fee",
                        requires_credit_card=false,
                        is_default=defaultvalue ==0,
                        cvv_required=false
                    },
                     new DepositpolicyModel{
                        id = 3,
                        name="Comp’d with Min. Purchase",
                        requires_credit_card=false,
                        is_default=defaultvalue ==3,
                        cvv_required=false
                    },
                      new DepositpolicyModel{
                        id = 8,
                        name="Comp’d with Min. Purchase- Require CC",
                        requires_credit_card=true,
                        is_default=defaultvalue ==8,
                        cvv_required=false
                    },
                       new DepositpolicyModel{
                        id = 7,
                        name="Collect Upon Arrival",
                        requires_credit_card=false,
                        is_default=defaultvalue ==7,
                        cvv_required=false
                    },
                       new DepositpolicyModel{
                        id = 1,
                        name="Collect Upon Arrival - Require CC",
                        requires_credit_card=true,
                        is_default=defaultvalue ==1,
                        cvv_required=false
                    },
                    //   new DepositpolicyModel{
                    //    id = 13,
                    //    name="Charged Upon Booking (Offline)- CC Required",
                    //    requires_credit_card=true,
                    //    is_default=defaultvalue ==13,
                    //    cvv_required=false
                    //},
                });
            return depositpolicyModel;
        }

        public static List<PreferredVisitDurationModel> GetPreferredVisitDuration()
        {
            var listModel = new List<PreferredVisitDurationModel>();
            listModel.AddRange(new PreferredVisitDurationModel[] {
                    new PreferredVisitDurationModel{
                        id = 0,
                        name="30 Minutes"
                    },
                    new PreferredVisitDurationModel{
                        id = 1,
                        name="1 Hour"
                    },new PreferredVisitDurationModel{
                        id = 2,
                        name="2 Hours"
                    },
                    new PreferredVisitDurationModel{
                        id = 3,
                        name="3 Hours"
                    },
                    new PreferredVisitDurationModel{
                        id = 4,
                        name="4 Hours"
                    },
                    new PreferredVisitDurationModel{
                        id = 5,
                        name="5 Hours"
                    },
                    new PreferredVisitDurationModel{
                        id = 6,
                        name="6 Hours"
                    },
                    new PreferredVisitDurationModel{
                        id = 7,
                        name="7 Hours"
                    },
                    new PreferredVisitDurationModel{
                        id = 8,
                        name="8 Hours"
                    },
                    new PreferredVisitDurationModel{
                        id = 9,
                        name="9 Hours"
                    },
                    new PreferredVisitDurationModel{
                        id = 10,
                        name="10 Hours"
                    },
                    new PreferredVisitDurationModel{
                        id = 11,
                        name="10 Hours +"
                    },
                });
            return listModel;
        }

        public static List<PreferredVisitDurationModel> GetReviewValue()
        {
            var listModel = new List<PreferredVisitDurationModel>();
            listModel.AddRange(new PreferredVisitDurationModel[] {
                    new PreferredVisitDurationModel{
                        id = 1,
                        name="Poor"
                    },new PreferredVisitDurationModel{
                        id = 2,
                        name="Average"
                    },
                    new PreferredVisitDurationModel{
                        id = 3,
                        name="Good"
                    },
                    new PreferredVisitDurationModel{
                        id = 4,
                        name="Very Good"
                    },
                    new PreferredVisitDurationModel{
                        id = 5,
                        name="Outstanding"
                    },
                });
            return listModel;
        }

        public static List<ReasonforVisitModel> GetReasonforVisit()
        {
            var listModel = new List<ReasonforVisitModel>();
            listModel.AddRange(new ReasonforVisitModel[] {
                    new ReasonforVisitModel{
                        id = 0,
                        name="Baby Shower"
                    },
                    new ReasonforVisitModel{
                        id = 1,
                        name="Birthday"
                    },
                    new ReasonforVisitModel{
                        id = 2,
                        name="Bridal / Wedding Shower"
                    },
                    new ReasonforVisitModel{
                        id = 3,
                        name="Corporate Event"
                    },
                    new ReasonforVisitModel{
                        id = 4,
                        name="Elopment"
                    },
                    new ReasonforVisitModel{
                        id = 5,
                        name="Engagement"
                    },
                    new ReasonforVisitModel{
                        id = 6,
                        name="Graduation"
                    },
                    new ReasonforVisitModel{
                        id = 7,
                        name="Holiday Event"
                    },
                    new ReasonforVisitModel{
                        id = 8,
                        name="Rehearsal Dinner"
                    },
                    new ReasonforVisitModel{
                        id = 9,
                        name="Wedding Anniversary"
                    },
                    new ReasonforVisitModel{
                        id = 10,
                        name="Wedding Reception"
                    },
                });
            return listModel;
        }

        public static List<DepositpolicyModel> GetChargeFeeRSVP(int defaultvalue)
        {
            var depositpolicyModel = new List<DepositpolicyModel>();
            depositpolicyModel.AddRange(new DepositpolicyModel[] {
                    new DepositpolicyModel{
                        id = 0,
                        name="No Fee",
                        requires_credit_card=false,
                        is_default=defaultvalue ==0,
                        cvv_required=false
                    },
                     new DepositpolicyModel{
                        id = 3,
                        name="Comp’d with Min. Purchase",
                        requires_credit_card=false,
                        is_default=defaultvalue ==3,
                        cvv_required=false
                    },
                      new DepositpolicyModel{
                        id = 8,
                        name="Comp’d with Min. Purchase- Require CC",
                        requires_credit_card=true,
                        is_default=defaultvalue ==8,
                        cvv_required=false
                    },
                      new DepositpolicyModel{
                        id = 11,
                        name="Charge 25% Deposit Upon Booking",
                        requires_credit_card=true,
                        is_default=defaultvalue ==11,
                        cvv_required=true
                    },
                      new DepositpolicyModel{
                        id = 12,
                        name="Charge 50% Deposit Upon Booking",
                        requires_credit_card=true,
                        is_default=defaultvalue ==12,
                        cvv_required=true
                    },
                       new DepositpolicyModel{
                        id = 7,
                        name="Collect Upon Arrival",
                        requires_credit_card=false,
                        is_default=defaultvalue ==7,
                        cvv_required=false
                    },
                       new DepositpolicyModel{
                        id = 1,
                        name="Collect Upon Arrival - Require CC",
                        requires_credit_card=true,
                        is_default=defaultvalue ==1,
                        cvv_required=false
                    },
                    //   new DepositpolicyModel{
                    //    id = 13,
                    //    name="Charged Upon Booking (Offline)- CC Required",
                    //    requires_credit_card=true,
                    //    is_default=defaultvalue ==13,
                    //    cvv_required=false
                    //},
                       new DepositpolicyModel{
                        id = 4,
                        name="Charge Immediately- Accept Declined",
                        requires_credit_card=true,
                        is_default=defaultvalue ==4,
                        cvv_required=true
                    },
                       new DepositpolicyModel
                       {
                           id = 10,
                           name = "Charge Immediately- Reject Declined",
                           requires_credit_card = true,
                           is_default = defaultvalue == 10,
                           cvv_required = true
                       },
                       new DepositpolicyModel{
                        id = 6,
                        name="Auto-charge 48 Hours Prior- Require CC",
                        requires_credit_card=true,
                        is_default=defaultvalue ==6,
                        cvv_required=false
                    },
                       new DepositpolicyModel{
                        id = 5,
                        name="Auto-charge 24 Hours Prior- Require CC",
                        requires_credit_card=true,
                        is_default=defaultvalue ==5,
                        cvv_required=false
                    },
                       new DepositpolicyModel{
                        id = 9,
                        name="Auto-charge Upon Arrival Date (AM)- Require CC",
                        requires_credit_card=true,
                        is_default=defaultvalue ==9,
                        cvv_required=false
                    },
                });
            return depositpolicyModel;
        }

        #region eWinery methods

        public static async Task<List<eWineryCustomerViewModel>> ResolveCustomer(string Username, string Password, string emailAddress, int MemberId)
        {
            List<eWineryCustomerViewModel> modelList = new List<eWineryCustomerViewModel>();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            try
            {
                //Service
                eWineryWebServices.EWSWebServicesSoapClient ws = new eWineryWebServices.EWSWebServicesSoapClient(EndpointConfiguration.EWSWebServicesSoap);
                //Member Req
                eWineryWebServices.GetMembersRequest wsReq = new eWineryWebServices.GetMembersRequest();

                //Credentials
                wsReq.PartnerKeyID = new Guid("9BB707B1-FE9C-48A9-ADEE-361E268B9064");
                wsReq.Username = Username;
                wsReq.Password = Password;
                //ws.
                //Properties
                wsReq.IncludeClubMemberships = true;
                //Pass guid if available otherwise we lookup by email

                if (emailAddress.IndexOf("@") > -1)
                    wsReq.Email = emailAddress;
                else
                    wsReq.LastName = emailAddress;


                //Club Resp
                eWineryWebServices.GetMembersResponse wsResp = await ws.GetMembersAsync(wsReq);
                if (wsResp != null)
                {
                    if (wsResp.IsSuccessful)
                    {
                        if (wsResp.Members.Length > 0)
                        {
                            //Get first customer that matches

                            foreach (var item in wsResp.Members)
                            {
                                var cust = new eWineryCustomerViewModel();
                                cust.member_id = item.MemberID.ToString();
                                cust.email = item.Email;
                                cust.first_name = item.FirstName;
                                cust.last_name = item.LastName;
                                cust.state = item.State;
                                cust.city = item.City;

                                cust.phone = item.Phone1.Replace("+1 ", "").Replace("+1", "");

                                cust.phone = "+1 " + cust.phone;

                                cust.address1 = item.Address1;
                                cust.address2 = item.Address2;

                                DateTime date_modified = Convert.ToDateTime("1/1/1900");

                                if (item.DateModified != null && item.DateModified.Value != null)
                                    cust.date_modified = item.DateModified.Value;
                                else
                                    cust.date_modified = date_modified;


                                cust.country_code = item.CountryCode + "";

                                if (cust.country_code.ToLower() == "us")
                                {
                                    cust.zip_code = item.ZipCode + "";
                                    if (cust.zip_code.Length > 5)
                                        cust.zip_code = cust.zip_code.Substring(0, 5);
                                }
                                else
                                    cust.zip_code = item.ZipCode;

                                bool member_status = false;
                                //Clubs
                                List<eWinery_club> clubs = new List<eWinery_club>();
                                if ((item.ClubMemberships != null))
                                {
                                    foreach (var club in item.ClubMemberships)
                                    {
                                        if (club.IsActive.Value == true)
                                        {
                                            member_status = true;
                                            clubs.Add(new eWinery_club(club.ClubLevelID.ToString(), club.ClubLevel, club.IsActive.Value));
                                        }
                                    }
                                }
                                cust.memberships = clubs;

                                cust.member_status = member_status;

                                //Account Types
                                List<eWinery_member_type> aTypes = new List<eWinery_member_type>();
                                if ((item.MemberTypes != null))
                                {
                                    foreach (var mtype in item.MemberTypes)
                                    {
                                        aTypes.Add(new eWinery_member_type(mtype.MemberTypeID.ToString(), mtype.MemberTypeDescription));
                                    }
                                }

                                cust.account_types = aTypes;

                                cust.account_note = await GeteWineryMemberNotes(Username, Password, cust.member_id);
                                modelList.Add(cust);
                            }
                        }
                    }
                    else
                    {
                        logDAL.InsertLog("Utility", "ResolveCustomer:  ErrorMessage-" + wsResp.ErrorMessage, "", 3, MemberId);
                    }

                }
            }
            catch (Exception ex)
            {
                //logDAL.InsertLog("Utility", "ResolveCustomer:  UserName-" + Username + ",Password-" + Password + ",Email-" + emailAddress + ",error-" + ex.Message, "",1,MemberId);
            }
            return modelList;

        }

        public static async Task<AccountNote> GeteWineryMemberNotes(string Username, string Password, string MemberId)
        {
            AccountNote note = null;
            try
            {
                eWineryWebServices.EWSWebServicesSoapClient ws = new eWineryWebServices.EWSWebServicesSoapClient(EndpointConfiguration.EWSWebServicesSoap);
                //Service

                //Note Req
                eWineryWebServices.GetMemberNotesRequest wsReq = new eWineryWebServices.GetMemberNotesRequest();
                wsReq.PartnerKeyID = new Guid("9BB707B1-FE9C-48A9-ADEE-361E268B9064");
                wsReq.Username = Username;
                wsReq.Password = Password;
                wsReq.IncludeSystemNotes = false;
                wsReq.MemberID = new Guid(MemberId);
                //Club Resp
                eWineryWebServices.GetMemberNotesResponse wsResp = await ws.GetMemberNotesAsync(wsReq);

                string strNote = string.Empty;
                if ((wsResp != null))
                {
                    if (wsResp.IsSuccessful)
                    {
                        if (wsResp.MemberNotes.Count() > 0)
                        {
                            foreach (var item in wsResp.MemberNotes)
                            {
                                string val = strNote.Trim().Length > 0 ? ", " : "";
                                strNote = strNote + val + item.Notes;
                                note.note_date = item.DateAdded;
                            }
                            note.note = strNote;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
            }

            return note;
        }




        public static async Task<bool> eWineryCreateContact(int memberId, string username, string password, eWineryCustomerViewModel cust, ReferralType referralType = ReferralType.CellarPass)
        {
            var result = false;
            if (cust != null && !string.IsNullOrWhiteSpace(cust.email) && !cust.email.ToLower().Contains("@noemail"))
            {
                result = await eWinerySaveCustomer(memberId, username, password, cust, referralType);
            }
            return result;
        }


        private static async Task<bool> eWinerySaveCustomer(int memberID, string userName, string password, eWineryCustomerViewModel cust, ReferralType referralType)
        {
            string thirdPartyId = "";
            bool isSucess = false;
            eWineryWebServices.EWSWebServicesSoapClient ws = new eWineryWebServices.EWSWebServicesSoapClient(EndpointConfiguration.EWSWebServicesSoap);

            //Member Req
            eWineryWebServices.AddUpdateMemberRequest wsReq = new eWineryWebServices.AddUpdateMemberRequest();

            //Credentials
            wsReq.PartnerKeyID = new Guid("9BB707B1-FE9C-48A9-ADEE-361E268B9064");
            wsReq.Username = userName;
            wsReq.Password = password;

            //Member Add
            eWineryWebServices.MemberAdd memberAdd = new eWineryWebServices.MemberAdd();

            memberAdd.Email = cust.email;
            memberAdd.Username = cust.email;
            memberAdd.FirstName = cust.first_name;
            memberAdd.LastName = cust.last_name;
            memberAdd.City = cust.city;
            memberAdd.State = cust.state;
            memberAdd.ZipCode = cust.zip_code;
            memberAdd.Phone1 = cust.phone;
            memberAdd.AltAccountNumber = cust.cellarpass_id.ToString();
            //? Is this ok to pass here ?
            SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

            List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.eWinery).ToList();
            if (settingsGroup != null && settingsGroup.Count > 0)
            {
                switch (referralType)
                {
                    case ReferralType.Widget:
                        memberAdd.SalesAssociateID = new Guid(settingsGroup.Where(s => s.Key == Common.Common.SettingKey.ewinery_widget).Select(s => s.Value).FirstOrDefault());
                        break;
                    case ReferralType.BackOffice:
                        memberAdd.SalesAssociateID = new Guid(settingsGroup.Where(s => s.Key == Common.Common.SettingKey.ewinery_back_office).Select(s => s.Value).FirstOrDefault());
                        break;
                    case ReferralType.Referrer:
                        memberAdd.SalesAssociateID = new Guid(settingsGroup.Where(s => s.Key == Common.Common.SettingKey.ewinery_concierge).Select(s => s.Value).FirstOrDefault());
                        break;
                    case ReferralType.Affiliate:
                        memberAdd.SalesAssociateID = new Guid(settingsGroup.Where(s => s.Key == Common.Common.SettingKey.ewinery_affiliate).Select(s => s.Value).FirstOrDefault());
                        break;
                    case ReferralType.WineryReferral:
                        memberAdd.SalesAssociateID = new Guid(settingsGroup.Where(s => s.Key == Common.Common.SettingKey.ewinery_referral).Select(s => s.Value).FirstOrDefault());
                        break;
                    case ReferralType.TablePro:
                        memberAdd.SalesAssociateID = new Guid(settingsGroup.Where(s => s.Key == Common.Common.SettingKey.ewinery_table_pro).Select(s => s.Value).FirstOrDefault());
                        break;
                }
            }
            //Properties
            wsReq.Member = memberAdd;

            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            string reqJSOn = JsonConvert.SerializeObject(wsReq);
            string respJSOn = "";
            logDAL.InsertLog("WebApi", "Vinsuite.SaveCustomer, Request: " + reqJSOn, "", 3, memberID);
            //Member Resp
            eWineryWebServices.AddUpdateMemberResponse wsResp = await ws.AddUpdateMemberAsync(wsReq);

            if ((wsResp != null))
            {
                respJSOn = JsonConvert.SerializeObject(wsResp);
                logDAL.InsertLog("WebApi", "Vinsuite.SaveCustomer, Response: " + respJSOn, "", 3, memberID);
                if (wsResp.IsSuccessful)
                {
                    thirdPartyId = wsResp.MemberID.ToString();
                    isSucess = true;
                }
                else
                {

                    logDAL.InsertLog("WebApi", "Vinsuite.SaveCustomer, Error: " + wsResp.ErrorMessage, "", 1, memberID);
                }

            }
            else
            {
                logDAL.InsertLog("WebApi", "Vinsuite.SaveCustomer, No response recevied ", "", 1, memberID);
            }


            //If we get a id back from eWinery then update the relationship in CellarPass with id

            if (!object.ReferenceEquals(thirdPartyId.Trim(), string.Empty))
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                userDAL.UpdateUserWinery(memberID, cust.cellarpass_id, thirdPartyId);

            }
            return isSucess;
        }

        public static async Task<string> eWinerySendOrder(int reservationId, string userName, string password)
        {
            // Processed by
            string processedBy = "CellarPass System";
            string extOrderId = "";
            eWineryWebServices.EWSWebServicesSoapClient ws = new eWineryWebServices.EWSWebServicesSoapClient(EndpointConfiguration.EWSWebServicesSoap);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            var rsvp = eventDAL.GetReservationDetailsbyReservationId(reservationId);

            if (rsvp != null)
            {
                if (rsvp.status == 2 | rsvp.status == 7 | rsvp.status == 8)
                    return "";

                // Get Phone
                string Phone = rsvp.user_detail.phone_number.Replace("+1", "").Replace("+", "");

                var winery = eventDAL.GetWineryById(rsvp.member_id);

                var wsReq = new eWineryWebServices.AddOrderRequest();
                //Credentials
                wsReq.PartnerKeyID = new Guid("9BB707B1-FE9C-48A9-ADEE-361E268B9064");
                wsReq.Username = userName;
                wsReq.Password = password;

                var wsRes = new eWineryWebServices.AddOrderResponse();
                var order = new eWineryWebServices.OrderAdd();
                var reqOrder = new eWineryWebServices.AddOrderRequest();

                // ## ORDER ##
                // Order Details
                // ?order.OrderType = "Reservation"
                order.OrderNumberPrefix = string.Format("CP-{0}-{1}-", rsvp.member_id, rsvp.booking_code);
                order.OrderNumber = reservationId;
                order.IsReservation = true;
                // order.OrderType = "Reservation"

                // Booked Date</Option
                // Event Date</option
                // Order Sync Date

                // Set Date to use
                if (winery.UpsertOrderDateAs == "1")
                    order.DateCompleted = rsvp.booking_date;
                else if (winery.UpsertOrderDateAs == "2")
                    order.DateCompleted = rsvp.event_start_date;
                else
                    order.DateCompleted = Times.ToTimeZoneTime(DateTime.UtcNow);// Date.Now

                // Ship Date
                if (winery.UpsertShipDateAs == "1")
                    order.RequestedShipDate = rsvp.booking_date;
                else if (winery.UpsertShipDateAs == "2")
                    order.RequestedShipDate = rsvp.event_start_date;

                string Address1 = string.IsNullOrWhiteSpace(rsvp.location_address1) ? "" : rsvp.location_address1;
                string Address2 = string.IsNullOrWhiteSpace(rsvp.location_address2) ? "" : rsvp.location_address2;
                string City = string.IsNullOrWhiteSpace(rsvp.location_city) ? "" : rsvp.location_city;
                string State = string.IsNullOrWhiteSpace(rsvp.location_state) ? "" : rsvp.location_state;
                string Zip = string.IsNullOrWhiteSpace(rsvp.location_zip) ? "" : rsvp.location_zip;

                if (Address1.Trim() == string.Empty)
                    Address1 = winery.WineryAddress.address_1 + "";
                if (Address2.Trim() == string.Empty)
                    Address2 = winery.WineryAddress.address_2 + "";
                if (City.Trim() == string.Empty)
                    City = winery.WineryAddress.city + "";
                if (State.Trim() == string.Empty)
                    State = winery.WineryAddress.state + "";
                if (Zip.Trim() == string.Empty)
                    Zip = winery.WineryAddress.zip_code;
                if (Phone.Trim() == string.Empty)
                    Phone = winery.BusinessPhone + "";

                // Billing Address
                order.BillingFirstName = rsvp.user_detail.first_name;
                order.BillingLastName = rsvp.user_detail.last_name;
                order.BillingCompany = ""; //winery.DisplayName;
                order.BillingAddress1 = Address1;
                order.BillingAddress2 = Address2;
                order.BillingCity = City;
                order.BillingState = State;
                order.BillingZipCode = Zip;
                order.BillingPhone = Phone;
                order.BillingEmail = rsvp.user_detail.email;

                // Sale Associates 'Load eWinery Config Settings if any
                try
                {
                    List<Settings.Setting> eWineryGroup = Settings.GetSettingGroup(rsvp.member_id, Common.Common.SettingGroup.eWinery);

                    if (eWineryGroup != null)
                    {
                        if (eWineryGroup.Count > 0)
                        {
                            string strId = "";
                            switch ((ReferralType)rsvp.referral_type)
                            {
                                case ReferralType.Widget:
                                    {
                                        // NOTE: Sending SalesAssociate Crashes Service. Just send SalesAssociateID.
                                        order.SalesAssociateID = new Guid(Settings.GetStrValue(eWineryGroup, Common.Common.SettingKey.ewinery_widget));
                                        break;
                                    }

                                case var case1 when case1 == ReferralType.BackOffice:
                                    {
                                        order.SalesAssociateID = new Guid(Settings.GetStrValue(eWineryGroup, Common.Common.SettingKey.ewinery_back_office));
                                        break;
                                    }

                                case var case2 when case2 == ReferralType.Referrer:
                                    {
                                        order.SalesAssociateID = new Guid(Settings.GetStrValue(eWineryGroup, Common.Common.SettingKey.ewinery_concierge));
                                        break;
                                    }

                                case var case3 when case3 == ReferralType.Affiliate:
                                    {
                                        order.SalesAssociateID = new Guid(Settings.GetStrValue(eWineryGroup, Common.Common.SettingKey.ewinery_affiliate));
                                        break;
                                    }

                                case var case4 when case4 == ReferralType.WineryReferral:
                                    {
                                        order.SalesAssociateID = new Guid(Settings.GetStrValue(eWineryGroup, Common.Common.SettingKey.ewinery_referral));
                                        break;
                                    }

                                case var case5 when case5 == ReferralType.TablePro:
                                    {
                                        order.SalesAssociateID = new Guid(Settings.GetStrValue(eWineryGroup, Common.Common.SettingKey.ewinery_table_pro));
                                        break;
                                    }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }

                // order.SourceCode = 

                // Shipping for Reporting Purposes
                order.ShippingAddress1 = Address1;
                order.ShippingAddress2 = Address2;
                order.ShippingCompany = ""; // winery.DisplayName;
                order.ShippingCity = City;
                order.ShippingState = State;
                order.ShippingZipCode = Zip;

                // ## Item Count ## - Start with a 0 index array
                int totalItems = 0;


                // Get AddOns here - if any to get a total array count
                //List<AddOns.AddOnItem> purchasedAddOns = AddOns.GetReservationAddOnItems(RsvpID);
                var purchasedAddOns = rsvp.reservation_addon;

                // Increase count with add on count
                totalItems = totalItems + purchasedAddOns.Count;

                int total_guests = rsvp.total_guests;

                if (rsvp.fee_type == 1)
                    total_guests = 1;

                // ## ORDER ITEM ##
                var orderItem = new eWineryWebServices.OrderItemAdd[totalItems + 1];

                orderItem[0] = new eWineryWebServices.OrderItemAdd();
                orderItem[0].Name = rsvp.event_name;
                orderItem[0].SKU = string.IsNullOrWhiteSpace(rsvp.rms_sku) ? rsvp.event_id.ToString() : rsvp.rms_sku;
                orderItem[0].Quantity = total_guests;

                // Price
                decimal pricePerItem = (decimal)0;
                decimal discountPerItem = (decimal)0;
                // discount per person
                if (rsvp.discount_amount > 0)
                    discountPerItem = rsvp.discount_amount / total_guests;
                if (rsvp.fee_per_person > 0)
                {
                    pricePerItem = rsvp.fee_per_person - discountPerItem;
                    // make sure we can't have negatives
                    if (pricePerItem < (decimal)0)
                        pricePerItem = (decimal)0;
                }

                // Set Price
                orderItem[0].Price = pricePerItem;

                decimal cost = (decimal)0;

                if (rsvp.event_id > 0)
                {
                    cost = eventDAL.GetEventById(rsvp.event_id ?? 0).Cost;

                    if (cost > pricePerItem)
                    {
                        cost = pricePerItem;
                    }

                    orderItem[0].CostOfGoodsSold = pricePerItem;
                }

                // ## ADD ONS ##
                if (!(purchasedAddOns == null))
                {
                    // Sort
                    purchasedAddOns = purchasedAddOns.OrderBy(f => f.name).ThenByDescending(f => f.price).ToList();
                    // Index for item array
                    int itemIdx = 1;
                    foreach (var AddOn in purchasedAddOns)
                    {
                        var itemAddOn = new eWineryWebServices.OrderItemAdd();
                        itemAddOn.Name = AddOn.name;
                        itemAddOn.SKU = AddOn.sku;
                        itemAddOn.Quantity = AddOn.qty;

                        decimal addOnPrice = AddOn.price;
                        // If total order amount is 0 then set this to 0
                        if (rsvp.fee_due == 0)
                            addOnPrice = (decimal)0;
                        itemAddOn.Price = addOnPrice;

                        // Add to array
                        orderItem[itemIdx] = itemAddOn;
                        // Increate index
                        itemIdx += 1;
                    }
                }

                // add item to order array
                order.OrderItems = orderItem;

                // Notes
                order.OrderNotes = rsvp.guest_note;

                // Totals
                order.SubTotal = rsvp.fee_due - rsvp.sales_tax;
                order.Taxes = rsvp.sales_tax;
                order.Total = rsvp.fee_due;

                // Payment
                order.CreditCardNumber = string.IsNullOrWhiteSpace(rsvp.pay_card.number) ? "" : StringHelpers.Decryption(rsvp.pay_card.number);
                order.CreditCardExpirationMonth = rsvp.pay_card.exp_month;
                order.CreditCardExpirationYear = rsvp.pay_card.exp_year;

                // Card Type 
                order.CreditCardType = rsvp.pay_card.card_type;

                // Shipping
                order.Shipping = 0;


                // Pass Order(s)
                wsReq.Order = order;

                // ## RESPONSE ##
                wsRes = await ws.AddOrderAsync(wsReq);
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                if (wsRes.IsSuccessful)
                {
                    extOrderId = wsRes.OrderID.ToString();
                    eventDAL.InsertExportLog((int)Common.Common.ExportType.eWinery, reservationId, "Success- " + wsRes.OrderID.ToString(), 1, rsvp.amount_paid, processedBy);
                    eventDAL.ReservationV2StatusNote_Create(reservationId, rsvp.status, rsvp.member_id, "", false, 0, 0, 0, "SYNC - Order upserted to VinSuite");

                    //return true;
                }
                else
                {
                    logDAL.InsertLog("WebApi", "Vinsuite.SaveOrder- BookingCode: " + rsvp.booking_code + ", Error:" + wsRes.ErrorMessage, "", 1, rsvp.member_id);
                    eventDAL.InsertExportLog((int)Common.Common.ExportType.eWinery, reservationId, wsRes.ErrorMessage, 0, rsvp.amount_paid, processedBy);
                }

                //return false;
            }
            return extOrderId;


        }

        public static async void SaveOrUpdateContact(int memberId, string email, eWineryCustomerViewModel cust)
        {
            try
            {
                string thirdPartyId = "";
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WineryModel winery = await Task.Run(() => eventDAL.GetWineryById(memberId));

                if ((winery != null))
                {
                    if (winery.eMemberEnabled && winery.eMemberUserNAme.Length > 0 && winery.eMemberPAssword.Length > 0)
                    {
                        eWineryCustomerViewModel contact = new eWineryCustomerViewModel();
                        thirdPartyId = contact.member_id + "";
                        if (thirdPartyId.Trim().Length == 0)
                        {
                            await eWineryCreateContact(memberId, winery.eMemberUserNAme, winery.eMemberPAssword, cust);
                        }
                        else if (eventDAL.IsEnableUpdate3rdParty(memberId))
                        {
                            await eWineryCreateContact(memberId, winery.eMemberUserNAme, winery.eMemberPAssword, cust);
                            if (!object.ReferenceEquals(thirdPartyId.Trim(), string.Empty))
                            {
                                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                                userDAL.UpdateUserWinery(memberId, cust.cellarpass_id, thirdPartyId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("Utility", "SaveOrUpdateContact:  wineryId: " + memberId.ToString() + ", UserId: " + cust.email + "Message: " + ex.Message + " " + ex.StackTrace, "", 1, memberId);
            }
        }

        public static async Task<List<AccountType>> GetClubTypes(string Username, string Password)
        {

            List<AccountType> list = new List<AccountType>();

            try
            {
                eWineryWebServices.EWSWebServicesSoapClient ws = new eWineryWebServices.EWSWebServicesSoapClient(EndpointConfiguration.EWSWebServicesSoap);
                //Service

                //Club Req
                eWineryWebServices.GetClubsRequest wsReq = new eWineryWebServices.GetClubsRequest();
                wsReq.PartnerKeyID = new Guid("9BB707B1-FE9C-48A9-ADEE-361E268B9064");
                wsReq.Username = Username;
                wsReq.Password = Password;
                wsReq.IncludeInactiveClubs = true;
                //Club Resp
                eWineryWebServices.GetClubsResponse wsResp = await ws.GetClubsAsync(wsReq);


                if ((wsResp != null))
                {

                    if (wsResp.IsSuccessful)
                    {
                        eWineryWebServices.WineClub[] eClubs = null;
                        eClubs = wsResp.WineClubs;

                        foreach (eWineryWebServices.WineClub club in eClubs)
                        {
                            AccountType at = new AccountType();
                            at.ContactTypeId = club.ClubLevelID.ToString();
                            at.ContactType = club.Title;
                            at.ActiveClub = club.IsActive ?? false;
                            at.ThirdPartyType = Common.Common.ThirdPartyType.eWinery;
                            list.Add(at);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
            }

            return list;
        }
        public static async Task<string> eWineryUpsertContact(string username, string password, eWineryCustomerViewModel cust)
        {
            string thirdPartyId = "";
            try
            {
                //Service
                eWineryWebServices.EWSWebServicesSoapClient ws = new eWineryWebServices.EWSWebServicesSoapClient(EndpointConfiguration.EWSWebServicesSoap);

                //Member Req
                eWineryWebServices.AddUpdateMemberRequest wsReq = new eWineryWebServices.AddUpdateMemberRequest();

                //Credentials
                wsReq.PartnerKeyID = new Guid("9BB707B1-FE9C-48A9-ADEE-361E268B9064");
                wsReq.Username = username;
                wsReq.Password = password;

                //Member Add
                eWineryWebServices.MemberAdd memberAdd = new eWineryWebServices.MemberAdd();

                if (!string.IsNullOrEmpty(cust.member_id))
                {
                    memberAdd.MemberID = new Guid(cust.member_id);
                }

                memberAdd.Email = string.IsNullOrEmpty(cust.email) ? "" : cust.email;
                memberAdd.Username = string.IsNullOrEmpty(cust.email) ? "" : cust.email;
                memberAdd.FirstName = string.IsNullOrEmpty(cust.first_name) ? "" : cust.first_name;
                memberAdd.LastName = string.IsNullOrEmpty(cust.last_name) ? "" : cust.last_name;
                memberAdd.City = string.IsNullOrEmpty(cust.city) ? "" : cust.city;
                memberAdd.State = string.IsNullOrEmpty(cust.state) ? "" : cust.state;
                memberAdd.ZipCode = string.IsNullOrEmpty(cust.zip_code) ? "" : cust.zip_code;
                memberAdd.Phone1 = string.IsNullOrEmpty(cust.phone) ? "" : cust.phone;
                memberAdd.Address1 = string.IsNullOrEmpty(cust.address1) ? "" : cust.address1;
                memberAdd.Address2 = string.IsNullOrEmpty(cust.address2) ? "" : cust.address2;
                memberAdd.CountryCode = string.IsNullOrEmpty(cust.country_code) ? "" : cust.country_code;
                memberAdd.AltAccountNumber = Convert.ToString(cust.cellarpass_id);
                //? Is this ok to pass here ?

                //Properties
                wsReq.Member = memberAdd;

                //Member Resp
                eWineryWebServices.AddUpdateMemberResponse wsResp = await ws.AddUpdateMemberAsync(wsReq);

                if ((wsResp != null))
                {
                    if (wsResp.IsSuccessful)
                    {
                        thirdPartyId = wsResp.MemberID.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.Debug, "ThirdParty", "eWinery.upsertContact: " + ex.StackTrace.ToString());
            }
            return thirdPartyId;
        }

        public static async Task<bool> eWineryTestCredentials(string username, string password)
        {
            bool ret = false;
            try
            {
                //Service
                eWineryWebServices.EWSWebServicesSoapClient ws = new eWineryWebServices.EWSWebServicesSoapClient(EndpointConfiguration.EWSWebServicesSoap);

                //Member Req
                eWineryWebServices.TestConnectionRequest wsReq = new eWineryWebServices.TestConnectionRequest();

                //Credentials
                wsReq.PartnerKeyID = new Guid("9BB707B1-FE9C-48A9-ADEE-361E268B9064");
                wsReq.Username = username;
                wsReq.Password = password;

                //Member Resp
                eWineryWebServices.TestConnectionResponse wsResp = await ws.TestConnectionAsync(wsReq);

                if ((wsResp != null))
                {
                    if (wsResp.IsSuccessful)
                    {
                        ret = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.InsertLog(Log.LogType.Debug, "ThirdParty", "eWinery.upsertContact: " + ex.StackTrace.ToString());
            }
            return ret;
        }

        #endregion


        public static async Task<string> SaveOrUpdateContactThirdParty(int memberId, UserDetailModel user, ReferralType referralType, int rsvpId = 0, string ShopifyUrl = "", string ShopifyAuthToken = "", bool optin = false)
        {

            bool isSuccess = false;
            string msg = "error";
            string address = "";
            string addlInfo = string.Empty;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            try

            {
                if (user != null && !string.IsNullOrWhiteSpace(user.first_name) && user.first_name.IndexOf("&") > -1)
                {
                    user.first_name = user.first_name.Split('&')[0];
                }

                if (user != null && user.address != null)
                {
                    address = user.address.address_1 + "";

                    if (!string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(user.address.address_2))
                        address += ", " + user.address.address_2;
                    else if (string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(user.address.address_2))
                        address += user.address.address_2;
                }
                else if (user != null && user.address == null)
                {
                    user.address = new Model.UserAddress();
                }
                if (!string.IsNullOrWhiteSpace(user.email) && user.email.IndexOf("@noemail") > -1)
                    return "error";

                //Added below logic as per ticket # 2874 to not resync customer if it has already been synced once and has a valid GUID
                //if (user != null && !string.IsNullOrWhiteSpace(user.membership_number))
                //    return true;

                addlInfo = "MemberId: " + memberId.ToString() + ", Email: " + user.email;

                bool isbLoyalEnabled = CheckIfBloyalEnabledForMember(memberId);
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WineryModel memberModel = eventDAL.GetWineryById(memberId);
                if (memberModel != null)
                {
                    if ((memberModel.EnableClubVin65 || memberModel.EnableVin65) && !string.IsNullOrWhiteSpace(memberModel.Vin65UserName) && !string.IsNullOrWhiteSpace(memberModel.Vin65Password))
                    {
                        addlInfo += ", ThirdPartyName: Vin65";
                        //check if contact already exists
                        List<Vin65Model> modelList = new List<Vin65Model>();
                        string vin65Id = "";
                        modelList = await Vin65GetContacts(user.email, memberModel.Vin65Password, memberModel.Vin65UserName);
                        if (modelList != null && modelList.Count > 0)
                        {
                            vin65Id = modelList[0].Vin65ID;
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            userDAL.UpdateUserWinery(memberId, user.user_id, vin65Id);
                        }

                        if (string.IsNullOrEmpty(vin65Id))
                        {
                            vin65Id = await Vin65UpsertContact(memberModel.Vin65Password, memberModel.Vin65UserName, address, user.address.city, vin65Id,
                            user.address.country, user.email, user.first_name, user.last_name, user.phone_number, user.address.state, user.address.zip_code, optin);

                            if (!string.IsNullOrEmpty(vin65Id))
                            {
                                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                                userDAL.UpdateUserWinery(memberId, user.user_id, vin65Id);
                            }
                        }

                        if (!string.IsNullOrEmpty(vin65Id))
                        {
                            if (rsvpId > 0)
                                eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to WineDirect");

                            isSuccess = true;
                            msg = "success";
                        }


                        if (isSuccess)
                        {
                            if (string.IsNullOrEmpty(vin65Id))
                            {
                                modelList = await Vin65GetContacts(user.email, memberModel.Vin65Password, memberModel.Vin65UserName);
                                if (modelList != null && modelList.Count > 0)
                                {
                                    vin65Id = modelList[0].Vin65ID;
                                }
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(vin65Id) && rsvpId > 0)
                                {
                                    string PayCardToken = eventDAL.GetPayCardTokenByRsvpId(rsvpId);

                                    if (!string.IsNullOrEmpty(PayCardToken))
                                    {
                                        TempCardDetail data = eventDAL.GetTempCardDetail(memberId, PayCardToken);

                                        if (data != null && !string.IsNullOrEmpty(data.PayCardToken))
                                        {
                                            int year = Convert.ToInt32(data.PayCardExpYear);
                                            int month = Convert.ToInt32(data.PayCardExpMonth);
                                            string CCId = await Vin65AddUpdateCreditCard(data.Vin65Password, data.Vin65Username, month, year, data.PayCardCustName, vin65Id, data.PayCardType, data.PayCardNumber);

                                            if (!string.IsNullOrEmpty(CCId))
                                            {
                                                eventDAL.UpdateReservationCreditCardReferenceNumber(rsvpId, CCId);
                                                //eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to WineDirect");
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }

                    }
                    else if (memberModel.EnableClubemember && !string.IsNullOrWhiteSpace(memberModel.eMemberUserNAme) && !string.IsNullOrWhiteSpace(memberModel.eMemberPAssword))
                    {
                        addlInfo += ", ThirdPartyName: eWinery";

                        List<eWineryCustomerViewModel> modelList = new List<eWineryCustomerViewModel>();
                        modelList = await Task.Run(() => Utility.ResolveCustomer(memberModel.eMemberUserNAme, memberModel.eMemberPAssword, user.email, memberId));

                        if (modelList != null && modelList.Count > 0)
                        {
                            isSuccess = true;
                            msg = "success";
                            //If we get a id back from eWinery then update the relationship in CellarPass with id

                            if (!string.IsNullOrWhiteSpace(modelList[0].member_id))
                            {
                                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                                userDAL.UpdateUserWinery(memberId, user.user_id, modelList[0].member_id);
                                if (rsvpId > 0)
                                    eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to VinSuite");
                                //addlInfo += ", user with email " + user.email + " already exists with Id:" + modelList[0].member_id;
                                //logDAL.InsertLog("Utility.SaveOrUpdateContactThirdParty", addlInfo, "", 3);
                            }
                            else
                            {
                                addlInfo += ", user with email " + user.email + " already exists but no Id  returned from lookup";
                                logDAL.InsertLog("Utility.SaveOrUpdateContactThirdParty", addlInfo, "", 1, memberId);
                                isSuccess = false;
                            }
                        }
                        else
                        {
                            eWineryCustomerViewModel cust = new eWineryCustomerViewModel
                            {
                                member_id = "",
                                email = user.email,
                                first_name = user.first_name,
                                last_name = user.last_name,
                                city = user.address.city,
                                state = user.address.state,
                                zip_code = user.address.zip_code,
                                phone = user.phone_number.Replace("+1 ", "").Replace("+1", ""),
                                cellarpass_id = user.user_id
                            };
                            isSuccess = await Utility.eWineryCreateContact(memberId, memberModel.eMemberUserNAme, memberModel.eMemberPAssword, cust, referralType);

                            if (isSuccess)
                            {
                                msg = "success";
                                if (rsvpId > 0)
                                    eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to VinSuite");
                            }

                        }
                    }
                    else if (memberModel.EnableClubSalesforce && !string.IsNullOrWhiteSpace(memberModel.SalesForceUserName)
                        && !string.IsNullOrWhiteSpace(memberModel.SalesForcePassword) && !string.IsNullOrWhiteSpace(memberModel.SalesForceSecurityToken))
                    {
                        addlInfo += ", ThirdPartyName: SalesForce";

                        var SalesForceuser = await SalesForceResolveCustomer(memberModel.SalesForceUserName, memberModel.SalesForcePassword, memberModel.SalesForceSecurityToken, user.email, memberModel.ClubStatusField);

                        if (SalesForceuser != null && !string.IsNullOrEmpty(SalesForceuser.membership_number))
                        {
                            isSuccess = true;
                            msg = "success";

                            if (rsvpId > 0)
                                eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to Salesforce");
                        }
                        else
                        {
                            isSuccess = await SalesForceSaveContact(memberModel.SalesForceUserName, memberModel.SalesForcePassword, memberModel.SalesForceSecurityToken, user);

                            if (isSuccess)
                            {
                                msg = "success";
                                if (rsvpId > 0)
                                    eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to Salesforce");
                            }
                        }
                    }
                    else if (memberModel.EnableVintegrate && !string.IsNullOrWhiteSpace(memberModel.VintegrateAPIPassword)
                        && !string.IsNullOrWhiteSpace(memberModel.VintegrateAPIUsername) && !string.IsNullOrWhiteSpace(memberModel.VintegratePartnerID))
                    {
                        addlInfo += ", ThirdPartyName: Vintegrate";
                        var contact = new VintegrateWebService.GetCustomerRequest();
                        string contactID = "";
                        contact.EMailAddress = user.email;
                        contact.UserName = memberModel.VintegrateAPIUsername;
                        contact.PartnerKey = memberModel.VintegratePartnerID;
                        contact.Password = memberModel.VintegrateAPIPassword;
                        var svc = new VintegrateWebService.CustomerServiceSoapClient(VintegrateWebService.CustomerServiceSoapClient.EndpointConfiguration.CustomerServiceSoap);
                        var resp = await svc.CellarPassGetCustomerAsync(contact);
                        if (resp != null && resp.Body != null && resp.Body.CellarPassGetCustomerResult != null)
                        {
                            var result = resp.Body.CellarPassGetCustomerResult;
                            if (result.Status && result.CustomerInfo != null && result.CustomerInfo.Count() > 0)
                            {
                                contactID = result.CustomerInfo[0].CustomerID;
                                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                                userDAL.UpdateUserWinery(memberId, user.user_id, contactID);
                                if (rsvpId > 0)
                                    eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to Vintegrate");
                            }
                        }

                        if (string.IsNullOrEmpty(contactID))
                        {
                            var upsertContact = new VintegrateWebService.UpsertCustomerRequest
                            {

                                CustomerID = contactID,
                                BilltoAddress1 = CheckZeroLenghtString(user.address.address_1),
                                BilltoAddress2 = CheckZeroLenghtString(user.address.address_2),
                                BilltoCity = CheckZeroLenghtString(user.address.city),
                                BilltoState = CheckZeroLenghtString(user.address.state),
                                BilltoZip = CheckZeroLenghtString(user.address.zip_code),
                                BilltoFirstName = CheckZeroLenghtString(user.first_name),
                                BilltoLastName = CheckZeroLenghtString(user.last_name),
                                BilltoCompanyName = " ",
                                BilltoPhone1 = CheckZeroLenghtString(user.phone_number),
                                BilltoPhone2 = CheckZeroLenghtString(user.cell_phone),
                                BilltoPhone3 = " ",

                                ContactFirstName = CheckZeroLenghtString(user.first_name),
                                ContactLastName = CheckZeroLenghtString(user.last_name),
                                EmailAddress = CheckZeroLenghtString(user.email),
                                ContactAddress1 = CheckZeroLenghtString(user.address.address_1),
                                ContactAddress2 = CheckZeroLenghtString(user.address.address_2),
                                ContactCity = CheckZeroLenghtString(user.address.city),
                                ContactState = CheckZeroLenghtString(user.address.state),
                                ContactZip = CheckZeroLenghtString(user.address.zip_code),
                                ContactCompanyName = " ",
                                ContactPhone1 = CheckZeroLenghtString(user.phone_number),
                                ContactPhone2 = CheckZeroLenghtString(user.cell_phone),
                                ContactPhone3 = " ",

                                ShipToAddress1 = CheckZeroLenghtString(user.address.address_1),
                                ShipToAddress2 = CheckZeroLenghtString(user.address.address_2),
                                ShipToCity = CheckZeroLenghtString(user.address.city),
                                ShipToState = CheckZeroLenghtString(user.address.state),
                                ShipToZip = CheckZeroLenghtString(user.address.zip_code),
                                ShipToCompanyName = " ",
                                ShipToPhone1 = CheckZeroLenghtString(user.phone_number),
                                ShipToPhone2 = CheckZeroLenghtString(user.cell_phone),
                                ShipToPhone3 = " "
                            };
                            var upsertContactResp = await svc.CellarPassUpsertCustomerAsync(upsertContact);
                            if (upsertContactResp != null && upsertContactResp.Body != null && upsertContactResp.Body.CellarPassUpsertCustomerResult != null)
                            {
                                var result = upsertContactResp.Body.CellarPassUpsertCustomerResult;
                                if (result.Status && result.CustomerID != null)
                                {
                                    contactID = result.CustomerID.ToString();
                                    isSuccess = true;
                                    msg = "success";
                                    if (rsvpId > 0)
                                        eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to Vintegrate");
                                }
                            }
                        }

                        isSuccess = ((contactID + "").Length > 0);

                        if (isSuccess)
                            msg = "success";

                    }
                    else if (memberModel.EnableClubCommerce7 && !string.IsNullOrWhiteSpace(memberModel.Commerce7Password)
                        && !string.IsNullOrWhiteSpace(memberModel.Commerce7Username) && !string.IsNullOrWhiteSpace(memberModel.Commerce7Tenant))
                    {
                        addlInfo += ", ThirdPartyName: Commerce7";

                        Commerce7CustomerModel commerce7CustomerModel = new Commerce7CustomerModel();

                        //memberModel

                        string zip_code = (user.address.zip_code + "").Replace(" ", "");

                        if (string.IsNullOrEmpty(zip_code))
                            zip_code = memberModel.WineryAddress.zip_code;

                        var userList = await Task.Run(() => Utility.GetCommerce7CustomersByNameOrEmail(user.email, memberModel.Commerce7Username, memberModel.Commerce7Password, memberModel.Commerce7Tenant, memberId));

                        if (userList != null && userList.Count > 0)
                        {
                            commerce7CustomerModel.CustId = userList[0].membership_number;
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            userDAL.UpdateUserWinery(memberId, user.user_id, commerce7CustomerModel.CustId);
                            commerce7CustomerModel.AlreadyExists = true;
                        }
                        else
                        {
                            commerce7CustomerModel = await CheckAndUpdateCommerce7Customer(memberModel.Commerce7Username.Trim(), memberModel.Commerce7Password.Trim(), memberModel.Commerce7Tenant.Trim(), user.first_name, user.last_name, "",
                                                                                user.address.address_1, user.address.address_2, user.address.city,
                                                                                user.address.state, zip_code, user.address.country, user.email, user.phone_number, "", memberId, rsvpId);

                            if ((commerce7CustomerModel.CustId + "").Length > 0)
                            {
                                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                                userDAL.UpdateUserWinery(memberId, user.user_id, commerce7CustomerModel.CustId);
                            }
                        }

                        isSuccess = (commerce7CustomerModel.CustId + "").Length > 0;

                        if (isSuccess)
                            msg = "success";

                        if (commerce7CustomerModel.Exceeded)
                        {
                            isSuccess = false;
                            msg = "exceeded";
                        }

                        try
                        {
                            if (isSuccess)
                            {
                                if (!string.IsNullOrEmpty(commerce7CustomerModel.CustId) && rsvpId > 0)
                                {
                                    //if (!commerce7CustomerModel.AlreadyExists)
                                    eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to Commerce7");

                                    string PayCardToken = eventDAL.GetPayCardTokenByRsvpId(rsvpId);

                                    if (!string.IsNullOrEmpty(PayCardToken))
                                    {
                                        TempCardDetail data = eventDAL.GetTempCardDetail(memberId, PayCardToken);

                                        if (data != null && !string.IsNullOrEmpty(data.PayCardToken))
                                        {
                                            int year = Convert.ToInt32(data.PayCardExpYear);
                                            int month = Convert.ToInt32(data.PayCardExpMonth);

                                            CreateCreditCard model = new CreateCreditCard();
                                            //model.cardBrand = data.PayCardType;
                                            //model.gateway = data.Vin65Username;
                                            //model.maskedCardNumber = data.PayCardNumber;
                                            //model.tokenOnFile = PayCardToken;
                                            //model.expiryMo = month;
                                            //model.expiryYr = year;
                                            //model.cardHolderName = data.PayCardCustName;
                                            //model.isDefault = true;

                                            //string CCId = await Task.Run(() => Utility.CreateCommerce7CreditCard(model, memberModel.Commerce7Username, memberModel.Commerce7Password, memberModel.Commerce7Tenant, commerce7CustomerModel.CustId, memberId));

                                            //if (!string.IsNullOrEmpty(CCId))
                                            //{
                                            //    eventDAL.UpdateReservationCreditCardReferenceNumber(rsvpId, CCId);

                                            //}

                                            data = eventDAL.GetTempCardByNumberDetail(memberId, data.PayCardNumber);

                                            if (data != null && !string.IsNullOrEmpty(data.PayCardToken))
                                            {
                                                year = Convert.ToInt32(data.PayCardExpYear);
                                                month = Convert.ToInt32(data.PayCardExpMonth);

                                                model = new CreateCreditCard();
                                                model.cardBrand = data.PayCardType;
                                                model.gateway = data.Vin65Username;
                                                model.maskedCardNumber = data.PayCardNumber;
                                                model.tokenOnFile = PayCardToken;
                                                model.expiryMo = month;
                                                model.expiryYr = year;
                                                model.cardHolderName = data.PayCardCustName;
                                                model.isDefault = true;
                                                model.cvv2 = data.cvv;

                                                await Task.Run(() => Utility.CreateCommerce7CreditCard(model, memberModel.Commerce7Username, memberModel.Commerce7Password, memberModel.Commerce7Tenant, commerce7CustomerModel.CustId, memberId));

                                            }
                                        }

                                        
                                    }
                                }
                            }
                        }
                        catch { }

                    }
                    else if (memberModel.EnableClubOrderPort && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiKey)
                        && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiToken) && !string.IsNullOrWhiteSpace(memberModel.OrderPortClientId))
                    {
                        addlInfo += ", ThirdPartyName: OrderPort";
                        var userDetailModel = await GetCustomersByNameOrEmail(user.email, memberModel.OrderPortApiKey, memberModel.OrderPortApiToken, memberModel.OrderPortClientId);
                        string CustId = "";
                        if (userDetailModel != null && userDetailModel.Count > 0)
                        {
                            CustId = userDetailModel[0].membership_number;
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            userDAL.UpdateUserWinery(memberId, user.user_id, CustId);
                        }

                        if (string.IsNullOrEmpty(CustId))
                        {
                            PayloadModel payloadModel = new PayloadModel();
                            payloadModel.CustomerUuid = CustId;

                            payloadModel.BillingAddress.FirstName = user.first_name;
                            payloadModel.BillingAddress.LastName = user.last_name;
                            payloadModel.BillingAddress.Company = "";
                            payloadModel.BillingAddress.Address1 = user.address.address_1 + "";
                            payloadModel.BillingAddress.Address2 = user.address.address_2 + "";
                            payloadModel.BillingAddress.City = user.address.city + "";
                            payloadModel.BillingAddress.State = user.address.state + "";
                            payloadModel.BillingAddress.ZipCode = user.address.zip_code + "";
                            payloadModel.BillingAddress.Country = user.address.country;
                            payloadModel.BillingAddress.Email = user.email;
                            payloadModel.BillingAddress.Phone = user.phone_number;
                            CustId = await UpsertCustomerDetails(payloadModel, memberModel.OrderPortApiKey, memberModel.OrderPortApiToken, memberModel.OrderPortClientId, memberId);
                        }

                        isSuccess = ((CustId + "").Length > 0);

                        if (isSuccess)
                        {
                            msg = "success";
                            if (rsvpId > 0)
                                eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to OrderPort");
                        }
                    }
                    else if (memberModel.EnableClubShopify && !string.IsNullOrWhiteSpace(memberModel.ShopifyAppPassword)
                        && !string.IsNullOrWhiteSpace(memberModel.ShopifyAppStoreName) && !string.IsNullOrWhiteSpace(memberModel.ShopifyPublishKey))
                    {
                        addlInfo += ", ThirdPartyName: Shopify";
                        string CustId = await GetShopifyCustomerIdByEmail(ShopifyUrl, ShopifyAuthToken, memberId, user.email);
                        if (!string.IsNullOrEmpty(CustId))
                        {
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            userDAL.UpdateUserWinery(memberId, user.user_id, CustId);
                        }

                        if (string.IsNullOrEmpty(CustId))
                        {
                            CreateShopifyModel m = new CreateShopifyModel();

                            CreateShopifyCustomer customer = new CreateShopifyCustomer();
                            CreateShopifyAddress a = new CreateShopifyAddress();
                            List<CreateShopifyAddress> lista = new List<CreateShopifyAddress>();

                            customer.email = user.email;
                            customer.first_name = user.first_name;
                            customer.last_name = user.last_name;
                            customer.phone = user.phone_number.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
                            customer.verified_email = true;

                            a.address1 = user.address.address_1;
                            a.city = user.address.city;
                            a.country = user.address.country;
                            a.first_name = user.first_name;
                            a.last_name = user.last_name;
                            a.phone = user.phone_number.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
                            a.province = user.address.state;
                            a.zip = user.address.zip_code;

                            lista.Add(a);

                            customer.addresses = lista;

                            m.member_id = memberId;
                            m.customer = customer;

                            CustId = await Utility.CreateShopifyCustomer(m, ShopifyUrl, ShopifyAuthToken);
                        }

                        if (!string.IsNullOrEmpty(CustId))
                        {
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            userDAL.UpdateUserWinery(memberId, user.user_id, CustId);

                            msg = "success";
                            if (rsvpId > 0)
                                eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to Shopify");
                        }
                    }
                    else if (memberModel.EnableClubBigCommerce && !string.IsNullOrWhiteSpace(memberModel.BigCommerceAceessToken)
                        && !string.IsNullOrWhiteSpace(memberModel.BigCommerceStoreId))
                    {
                        addlInfo += ", ThirdPartyName: BigCommerce";
                        string CustId = await Task.Run(() => Utility.CheckAndUpdateBigCommerceCustomer(memberModel, memberId, user.email));

                        if (!string.IsNullOrEmpty(CustId))
                        {
                            msg = "success";
                            if (rsvpId > 0)
                                eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to BigCommerce");
                        }
                    }
                    else if (isbLoyalEnabled)
                    {
                        addlInfo += ", ThirdPartyName: bLoyal";
                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(memberId, (int)Common.Common.SettingGroup.bLoyal).ToList();
                        isSuccess = await bLoyalSaveContact(settingsGroup, user);

                        if (isSuccess)
                        {
                            msg = "success";
                            if (rsvpId > 0)
                                eventDAL.ReservationV2StatusNote_Create(rsvpId, 0, memberId, "", false, 0, 0, 0, "SYNC - Customer record upserted to bLoyal");
                        }
                    }
                }
            }

            catch (Exception ex)
            {

                logDAL.InsertLog("Utility", "Core API server: https://cellarpasscore.azurewebsites.net, SaveorUpdateContactThirdParty::" + addlInfo + "  ErrorMessage -" + ex.Message + " " + ex.StackTrace, "", 1, memberId);

            }

            return msg;
        }


        public static async Task<bool> AddUpdateNoteToThirdParty(int memberId, AccountNote noteInfo, string contactId)
        {

            bool isSuccess = false;
            string addlInfo = string.Empty;
            try

            {


                addlInfo = "MemberId: " + memberId.ToString() + ", contactId: " + contactId;

                bool isbLoyalEnabled = CheckIfBloyalEnabledForMember(memberId);
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WineryModel memberModel = eventDAL.GetWineryById(memberId);
                if (memberModel != null)
                {
                    if (memberModel.EnableClubVin65 && !string.IsNullOrWhiteSpace(memberModel.Vin65UserName) && !string.IsNullOrWhiteSpace(memberModel.Vin65Password))
                    {
                        addlInfo += ", ThirdPartyName: Vin65";
                        //check if contact already exists
                        if (!string.IsNullOrEmpty(contactId))
                        {
                            isSuccess = await Vin65AddUpdateNote(memberModel.Vin65Password, memberModel.Vin65UserName, noteInfo.note_id, noteInfo.subject, contactId, noteInfo.note, (DateTime)noteInfo.note_date, memberId);
                        }
                        else
                            isSuccess = true;

                    }
                    else if (memberModel.EnableClubemember && !string.IsNullOrWhiteSpace(memberModel.eMemberUserNAme) && !string.IsNullOrWhiteSpace(memberModel.eMemberPAssword))
                    {
                        addlInfo += ", ThirdPartyName: eWinery";


                    }
                    else if (memberModel.EnableClubSalesforce && !string.IsNullOrWhiteSpace(memberModel.SalesForceUserName)
                        && !string.IsNullOrWhiteSpace(memberModel.SalesForcePassword) && !string.IsNullOrWhiteSpace(memberModel.SalesForceSecurityToken))
                    {
                        addlInfo += ", ThirdPartyName: SalesForce";


                    }
                    else if (memberModel.EnableClubCommerce7 && !string.IsNullOrWhiteSpace(memberModel.Commerce7Password)
                        && !string.IsNullOrWhiteSpace(memberModel.Commerce7Username) && !string.IsNullOrWhiteSpace(memberModel.Commerce7Tenant))
                    {
                        addlInfo += ", ThirdPartyName: Commerce7";

                    }

                    else if (memberModel.EnableClubOrderPort && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiKey)
                        && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiToken) && !string.IsNullOrWhiteSpace(memberModel.OrderPortClientId))
                    {
                        addlInfo += ", ThirdPartyName: OrderPort";

                    }
                    else if (isbLoyalEnabled)
                    {
                        addlInfo += ", ThirdPartyName: bLoyal";

                    }
                }
            }

            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("Utility", "SaveorUpdateContactThirdParty::" + addlInfo + "  ErrorMessage -" + ex.Message + " " + ex.StackTrace, "", 1, memberId);

            }

            return isSuccess;
        }

        public static async Task<List<AccountNote>> GetThirdPartyNotes(int memberId, string external_id, string email)
        {
            List<AccountNote> notes = new List<AccountNote>();
            string addlInfo = string.Empty;
            try

            {


                addlInfo = "MemberId: " + memberId.ToString() + ", external contact Id: " + external_id + ", email:" + email;

                bool isbLoyalEnabled = CheckIfBloyalEnabledForMember(memberId);
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WineryModel memberModel = eventDAL.GetWineryById(memberId);
                if (memberModel != null)
                {
                    if (memberModel.EnableClubVin65 && !string.IsNullOrWhiteSpace(memberModel.Vin65UserName) && !string.IsNullOrWhiteSpace(memberModel.Vin65Password))
                    {
                        addlInfo += ", ThirdPartyName: Vin65";
                        //check if contact already exists
                        if (string.IsNullOrWhiteSpace(external_id))
                        {
                            List<Vin65Model> modelList = new List<Vin65Model>();
                            modelList = await Vin65GetContacts(email, memberModel.Vin65Password, memberModel.Vin65UserName);
                            if (modelList != null && modelList.Count > 0)
                            {
                                external_id = modelList[0].Vin65ID;
                            }
                        }

                        if (!string.IsNullOrEmpty(external_id))
                        {
                            notes = await Vin65GetNotesForContact(memberModel.Vin65Password, memberModel.Vin65UserName, external_id, memberId);
                        }


                    }
                    else if (memberModel.EnableClubemember && !string.IsNullOrWhiteSpace(memberModel.eMemberUserNAme) && !string.IsNullOrWhiteSpace(memberModel.eMemberPAssword))
                    {
                        addlInfo += ", ThirdPartyName: eWinery";


                    }
                    else if (memberModel.EnableClubSalesforce && !string.IsNullOrWhiteSpace(memberModel.SalesForceUserName)
                        && !string.IsNullOrWhiteSpace(memberModel.SalesForcePassword) && !string.IsNullOrWhiteSpace(memberModel.SalesForceSecurityToken))
                    {
                        addlInfo += ", ThirdPartyName: SalesForce";

                        if (string.IsNullOrWhiteSpace(external_id))
                        {
                            var SalesForceuser = await SalesForceResolveCustomer(memberModel.SalesForceUserName, memberModel.SalesForcePassword, memberModel.SalesForceSecurityToken, email, memberModel.ClubStatusField);

                            if (SalesForceuser != null)
                            {
                                external_id = SalesForceuser.membership_number;
                            }
                        }

                    }
                    else if (memberModel.EnableVintegrate && !string.IsNullOrWhiteSpace(memberModel.VintegrateAPIPassword)
                        && !string.IsNullOrWhiteSpace(memberModel.VintegrateAPIUsername) && !string.IsNullOrWhiteSpace(memberModel.VintegratePartnerID))
                    {
                        addlInfo += ", ThirdPartyName: Vintegrate";

                        if (string.IsNullOrWhiteSpace(external_id))
                        {
                            var contact = new VintegrateWebService.GetCustomerRequest();

                            contact.EMailAddress = email;
                            contact.UserName = memberModel.VintegrateAPIUsername;
                            contact.PartnerKey = memberModel.VintegratePartnerID;
                            contact.Password = memberModel.VintegrateAPIPassword;
                            var svc = new VintegrateWebService.CustomerServiceSoapClient(VintegrateWebService.CustomerServiceSoapClient.EndpointConfiguration.CustomerServiceSoap);
                            var resp = await svc.CellarPassGetCustomerAsync(contact);
                            if (resp != null && resp.Body != null && resp.Body.CellarPassGetCustomerResult != null)
                            {
                                var result = resp.Body.CellarPassGetCustomerResult;
                                if (result.Status && result.CustomerInfo != null && result.CustomerInfo.Count() > 0)
                                {
                                    external_id = result.CustomerInfo[0].CustomerID;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(external_id))
                        {

                        }


                    }
                    else if (memberModel.EnableClubCommerce7 && !string.IsNullOrWhiteSpace(memberModel.Commerce7Password)
                        && !string.IsNullOrWhiteSpace(memberModel.Commerce7Username) && !string.IsNullOrWhiteSpace(memberModel.Commerce7Tenant))
                    {
                        addlInfo += ", ThirdPartyName: Commerce7";
                        if (string.IsNullOrWhiteSpace(external_id))
                        {
                            var userList = await Task.Run(() => Utility.GetCommerce7CustomersByNameOrEmail(email, memberModel.Commerce7Username, memberModel.Commerce7Password, memberModel.Commerce7Tenant, memberId));

                            if (userList != null && userList.Count > 0)
                            {
                                external_id = userList[0].membership_number;
                            }
                        }
                        if (!string.IsNullOrEmpty(external_id))
                        {

                        }
                    }

                    else if (memberModel.EnableClubOrderPort && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiKey)
                        && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiToken) && !string.IsNullOrWhiteSpace(memberModel.OrderPortClientId))
                    {
                        addlInfo += ", ThirdPartyName: OrderPort";
                        if (string.IsNullOrEmpty(external_id))
                        {


                            var userDetailModel = await GetCustomersByNameOrEmail(email, memberModel.OrderPortApiKey, memberModel.OrderPortApiToken, memberModel.OrderPortClientId);
                            string CustId = "";
                            if (userDetailModel != null && userDetailModel.Count > 0)
                            {
                                CustId = userDetailModel[0].membership_number;
                            }
                        }

                        if (!string.IsNullOrEmpty(external_id))
                        {

                        }
                    }
                    else if (isbLoyalEnabled)
                    {
                        addlInfo += ", ThirdPartyName: bLoyal";
                    }
                }
            }

            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("Utility", "GetThirdPartyNotes::" + addlInfo + "  ErrorMessage -" + ex.Message + " " + ex.StackTrace, "", 1, memberId);

            }

            return notes;
        }

        private static string CheckZeroLenghtString(string data)
        {
            string ret = " ";
            if (!string.IsNullOrWhiteSpace(data))
                ret = data;

            return ret;
        }

        #region bLoyal Methods



        public static bool CheckIfBloyalEnabledForMember(int member_id)
        {
            bool bLoyalClubLookupEnabled = false;
            SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
            List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(member_id, (int)Common.Common.SettingGroup.bLoyal).ToList();
            if ((settingsGroup != null))
            {
                if (settingsGroup.Count > 0)
                {
                    bool ret = false;
                    dynamic dbSettings = settingsGroup.Where(f => f.Key == Common.Common.SettingKey.bLoyalApiClubLookup).FirstOrDefault();

                    if ((dbSettings != null))
                    {
                        bool.TryParse(dbSettings.Value, out ret);
                    }

                    if (ret == true)
                    {
                        bLoyalClubLookupEnabled = true;
                    }
                }
            }
            return bLoyalClubLookupEnabled;
        }
        private static bloyalsvc.Credential GetCredentials(List<Settings.Setting> listSettings)
        {
            bloyalsvc.Credential cred = new bloyalsvc.Credential();
            cred.ApplicationId = new Guid("a1da6beb-08bd-4caf-be98-11f7426f907a");
            if (listSettings != null)
            {
                if (listSettings.Count > 0)
                {
                    cred.Domain = Settings.GetStrValue(listSettings, Common.Common.SettingKey.bLoyalApiCompany);
                    cred.UserName = Settings.GetStrValue(listSettings, Common.Common.SettingKey.bLoyalApiUsername);
                    cred.Password = Settings.GetStrValue(listSettings, Common.Common.SettingKey.bLoyalApiPassword);
                    cred.DeviceKey = Settings.GetStrValue(listSettings, Common.Common.SettingKey.bLoyalApiDeviceKey);
                }
            }

            return cred;
        }

        private static bloyalOrdersvc.Credential GetCredentialsOrderSvc(List<Settings.Setting> listSettings)
        {
            bloyalOrdersvc.Credential cred = new bloyalOrdersvc.Credential();
            cred.ApplicationId = new Guid("a1da6beb-08bd-4caf-be98-11f7426f907a");
            if (listSettings != null)
            {
                if (listSettings.Count > 0)
                {
                    cred.Domain = Settings.GetStrValue(listSettings, Common.Common.SettingKey.bLoyalApiCompany);
                    cred.UserName = Settings.GetStrValue(listSettings, Common.Common.SettingKey.bLoyalApiUsername);
                    cred.Password = Settings.GetStrValue(listSettings, Common.Common.SettingKey.bLoyalApiPassword);
                    cred.DeviceKey = Settings.GetStrValue(listSettings, Common.Common.SettingKey.bLoyalApiDeviceKey);
                }
            }

            return cred;
        }

        private static bloyalsvc.LoyaltyEngine GetLoyaltyEngineService(List<Settings.Setting> listSettings)
        {
            string baseUrl = "https://ws.bloyal.com";

            string settingURL = Settings.GetStrValue(listSettings, Common.Common.SettingKey.bLoyalApiBaseURL);

            if (settingURL.Trim() != string.Empty)
                baseUrl = settingURL;

            System.ServiceModel.EndpointAddress address = new System.ServiceModel.EndpointAddress(String.Format("{0}/ws35/LoyaltyEngine.svc", baseUrl));
            System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();

            if (baseUrl.Contains("https:"))
                binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;

            binding.MaxReceivedMessageSize = 1024000;
            // Set to a big enough amount for the search results to return.
            bloyalsvc.LoyaltyEngineClient svc = new bloyalsvc.LoyaltyEngineClient(binding, address);

            return svc;
        }

        private static bloyalOrdersvc.OrderProcessingClient GetOrderProcessingClient(List<Settings.Setting> listSettings)
        {
            string baseUrl = "https://ws.bloyal.com";

            string settingURL = Settings.GetStrValue(listSettings, Common.Common.SettingKey.bLoyalApiBaseURL);

            if (settingURL.Trim() != string.Empty)
                baseUrl = settingURL;

            System.ServiceModel.EndpointAddress address = new System.ServiceModel.EndpointAddress(String.Format("{0}/3.0.1/OrderProcessing.svc", baseUrl));
            System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();

            if (baseUrl.Contains("https:"))
                binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;

            binding.MaxReceivedMessageSize = 1024000;
            // Set to a big enough amount for the search results to return.
            bloyalOrdersvc.OrderProcessingClient svc = new bloyalOrdersvc.OrderProcessingClient(binding, address);

            return svc;
        }

        public static async Task<UserDetailModel> bLoyalResolveCustomer(List<Settings.Setting> settingsGroup, string emailAddress)
        {
            UserDetailModel user = null;


            var cred = GetCredentials(settingsGroup);
            var svc = GetLoyaltyEngineService(settingsGroup);

            bloyalsvc.Customer cust = null;
            int custId = 0;
            var tranCustomer = new bloyalsvc.TransactionCustomer
            {
                AccountNumber = "",
                EmailAddress = emailAddress
            };
            cust = await svc.ResolveCustomerAsync(cred, tranCustomer);
            if (cust != null)
            {
                custId = cust.Id;
                user = new UserDetailModel
                {
                    email = emailAddress,
                    first_name = cust.FirstName,
                    last_name = cust.LastName,
                    membership_number = cust.Id.ToString(),
                    address = new Model.UserAddress
                    {
                        address_1 = cust.Address1 + "",
                        address_2 = cust.Address2 + "",
                        city = cust.City + "",
                        state = cust.State + "",
                        zip_code = cust.ZipCode + "",
                    },
                    cell_phone = Utility.FormatTelephoneNumber(cust.MobilePhone + "", cust.Country + ""),
                    phone_number = Utility.FormatTelephoneNumber(cust.Phone1 + "", cust.Country + "")
                };

                if (custId > 0)
                {
                    var clubMemship = await svc.GetCustomerMembershipSummaryAsync(cred, custId);
                    if (clubMemship != null && clubMemship.ClubMemberships.Count() > 0 && Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.bLoyalApiClubLookup))
                    {
                        var activeClubs = clubMemship.ClubMemberships.Where(c => c.Status == bloyalsvc.ClubMemberStatus.Active).ToList();
                        if (activeClubs != null && activeClubs.Count > 0)
                        {
                            List<string> listcontacttypes = new List<string>();
                            foreach (var c in activeClubs)
                            {
                                listcontacttypes.Add(c.ClubName);
                            }
                            user.contact_types = listcontacttypes;

                            user.member_status = listcontacttypes.Count > 0;

                            if (user.member_status)
                                user.customer_type = 1;
                            else
                                user.customer_type = 0;
                        }
                    }
                }
            }
            return user;
        }

        private static async Task<bool> bLoyalSaveContact(List<Settings.Setting> settingsGroup, UserDetailModel userDetail)
        {
            if (userDetail == null || userDetail.email.ToLower().IndexOf("@noemail") > -1)
                return false;

            var cred = GetCredentials(settingsGroup);
            var svc = GetLoyaltyEngineService(settingsGroup);

            bloyalsvc.Customer customer = null;
            int custId = 0;
            var tranCustomer = new bloyalsvc.TransactionCustomer
            {
                AccountNumber = "",
                EmailAddress = userDetail.email
            };
            customer = await svc.ResolveCustomerAsync(cred, tranCustomer);
            int saveId = 0;
            if (customer == null)
            {
                customer = new bloyalsvc.Customer();
                customer.AccountNumber = "";
                customer.ExternalId = userDetail.user_id.ToString(); // This should be the ID your application uses to uniquely identify the customer.
                customer.FirstName = userDetail.first_name;
                customer.LastName = userDetail.last_name;
                if (!string.IsNullOrWhiteSpace(userDetail.phone_number))
                    userDetail.phone_number = userDetail.phone_number.Replace("+1", "");
                customer.Phone1 = userDetail.phone_number;
                customer.ZipCode = userDetail.address.zip_code;
                customer.EmailAddress = userDetail.email;
                customer.City = userDetail.address.city;
                customer.State = userDetail.address.state;

                saveId = await svc.SaveCustomerAsync(cred, customer, false);
            }
            else
                saveId = customer.Id;


            return (saveId > 0);
        }

        public static async Task<String> bLoyalSendOrder(List<Settings.Setting> settingsGroup, int reservationId)
        {
            int member_id = 0;
            bool isSuccess = false;
            string extOrderId = "";
            //check if reservation exists and is in proper status
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            var reservation = eventDAL.GetReservationDetailsbyReservationId(reservationId);

            if (reservation != null && reservation.status != (int)Email.ReservationStatus.Initiated && reservation.status != (int)Email.ReservationStatus.YelpInitiated && reservation.status != (int)Email.ReservationStatus.Cancelled)
            {
                member_id = reservation.member_id;
                var cred = GetCredentialsOrderSvc(settingsGroup);
                var svc = GetOrderProcessingClient(settingsGroup);

                bool isAlreadySynced = eventDAL.CheckIfOrderAlreadySynced(reservationId, Common.Common.ExportType.bLoyal, ref extOrderId);
                if (isAlreadySynced)
                {
                    return extOrderId;
                }

                var winery = eventDAL.GetWineryById(reservation.member_id);

                if (winery != null)
                {
                    string address1 = winery.WineryAddress.address_1;
                    string address2 = winery.WineryAddress.address_2;
                    string city = winery.WineryAddress.city;
                    string state = winery.WineryAddress.state;
                    string zip = winery.WineryAddress.zip_code;

                    DateTime orderDate = DateTime.Now;
                    DateTime shipDate = reservation.booking_date;

                    // Set Date to use
                    if (winery.UpsertOrderDateAs == "1")
                        orderDate = reservation.booking_date;
                    else if (winery.UpsertOrderDateAs == "2")
                        orderDate = reservation.event_start_date;

                    // Ship Date
                    if (winery.UpsertShipDateAs == "1")
                        shipDate = reservation.booking_date;
                    else if (winery.UpsertShipDateAs == "2")
                        shipDate = reservation.event_start_date;


                    var order = new bloyalOrdersvc.SalesOrder();

                    // Order Details
                    // ?order.OrderType = "Reservation"
                    order.Title = "CellarPass Order: " + orderDate.ToString("MM/dd/yyyy");
                    order.Channel = ""; // "WebStore"
                    order.ExternalId = string.Format("CP-{0}-{1}", reservation.member_id, reservation.booking_code);

                    var custDetails = await bLoyalResolveCustomer(settingsGroup, reservation.user_detail.email);
                    order.Customer = new bloyalOrdersvc.Customer();
                    if (custDetails != null)
                    {

                        order.Customer.Id = int.Parse(custDetails.membership_number);

                    }
                    else
                    {
                        order.Customer.Id = 0;
                    }

                    if (reservation.user_detail != null)
                    {
                        order.Customer.EmailAddress = reservation.user_detail.email;
                        order.Customer.FirstName = reservation.user_detail.first_name;
                        order.Customer.LastName = reservation.user_detail.last_name;
                        order.Customer.Phone1 = reservation.user_detail.phone_number.Replace("+1", "").Replace("+", "");

                        if (reservation.user_detail.address != null)
                        {
                            order.Customer.ZipCode = reservation.user_detail.address.zip_code;
                            order.Customer.City = reservation.user_detail.address.city;
                            order.Customer.State = reservation.user_detail.address.state;
                        }

                        var shipment = new bloyalOrdersvc.OrderShipment();

                        shipment.Recipient = new bloyalOrdersvc.PartySummary();
                        shipment.Recipient.Address1 = address1;
                        shipment.Recipient.City = city;
                        shipment.Recipient.State = state;
                        shipment.Recipient.ZipCode = zip;
                        shipment.Recipient.FirstName = reservation.user_detail.first_name;
                        shipment.Recipient.LastName = reservation.user_detail.last_name;
                        shipment.Recipient.Country = "US";
                        shipment.ShipDate = shipDate;
                        shipment.CarrierCode = "";
                        shipment.ServiceCode = "";
                        shipment.Charge = 0;
                        shipment.Type = "Pickup";    // Shipment Type can be "Ship", "Pickup", or "Electronic"

                        // Assign Shipment
                        // Dim orderShipment As com.bloyal.ws.OrderShipment() = New com.bloyal.ws.OrderShipment(0) {}
                        var orderShipment = new List<bloyalOrdersvc.OrderShipment>();
                        orderShipment.Add(shipment);
                        order.Shipments = orderShipment.ToArray();

                        // ## ORDER ITEM ##
                        var orderLines = new List<bloyalOrdersvc.OrderLine>();
                        order.Lines = new bloyalOrdersvc.OrderLine[] { };
                        var orderLine = new bloyalOrdersvc.OrderLine();
                        orderLine.ShipmentNumber = "1";
                        orderLine.Product = new bloyalOrdersvc.ProductDetail();
                        orderLine.Product.LookupCode = string.IsNullOrWhiteSpace(reservation.rms_sku) ? reservation.event_id.ToString() : reservation.rms_sku; // IIf(reservation.RmsSku.Trim = "", reservation.EventId, reservation.RmsSku)
                        orderLine.Product.Name = reservation.event_name;

                        // Price
                        decimal pricePerItem = 0;
                        decimal discountPerItem = 0;

                        int total_guests = reservation.total_guests;

                        if (reservation.fee_type == 1)
                            total_guests = 1;

                        // discount per person
                        if (reservation.discount_amount > 0)
                            discountPerItem = reservation.discount_amount / total_guests;
                        if (reservation.fee_per_person > 0)
                        {
                            pricePerItem = reservation.fee_per_person - discountPerItem;
                            // make sure we can't have negatives
                            if (pricePerItem < 0)
                                pricePerItem = 0;
                        }

                        decimal cost = (decimal)0;

                        if (reservation.event_id > 0)
                        {
                            cost = eventDAL.GetEventById(reservation.event_id ?? 0).Cost;

                            if (cost > pricePerItem)
                            {
                                cost = pricePerItem;
                            }

                            orderLine.Product.Cost = pricePerItem;
                        }

                        orderLine.Product.Price = pricePerItem; // (reservation.AmountDue - reservation.SalesTax) / reservation.TotalGuests 'AmountDue instead?
                        orderLine.QuantityOrdered = total_guests; // amount due and 1 here instead?

                        bool EnableService = false;
                        EnableService = winery.EnableService;

                        if (reservation.sales_tax > 0 && EnableService)
                        {
                            var taxdetail = new List<bloyalOrdersvc.TaxDetail>();
                            var tax = new bloyalOrdersvc.TaxDetail();
                            tax.Code = "SC";
                            tax.Rate = reservation.sales_tax_percent / 100;
                            tax.Amount = reservation.sales_tax;
                            taxdetail.Add(tax);
                            orderLine.TaxDetails = taxdetail.ToArray();
                        }

                        orderLines.Add(orderLine);

                        // add item to order
                        order.Lines = orderLines.ToArray();
                        var calcOptions = new List<string>();
                        // Use the BypassShippingCalculations option to by-pass the bLoyal shipping charge calculation
                        // service and use your own external shipping calculations
                        calcOptions.Add("BypassShippingCalculations");
                        // Use the BypassTaxCalculations option to by-pass the bLoyal tax calculation service 
                        // and use your own tax calculations.
                        // calcOptions.Add("BypassTaxCalculations")
                        // Dim options() As String

                        // Calculate Order
                        // order = svc.CalculateOrder(cred, cred.DeviceKey, order, calcOptions.ToArray)

                        // ## PAYMENTS ##
                        string tenderCode = "CP-COMP";

                        if (!(reservation.pay_card == null))
                        {
                            switch (reservation.pay_card.card_type)
                            {
                                case "American Express":
                                    {
                                        tenderCode = "CP-AMEX";
                                        break;
                                    }

                                case "Master Card":
                                    {
                                        tenderCode = "CP-MC";
                                        break;
                                    }

                                case "Visa":
                                    {
                                        tenderCode = "CP-VISA";
                                        break;
                                    }

                                case "Discover":
                                    {
                                        tenderCode = "CP-DISC";
                                        break;
                                    }

                                default:
                                    {
                                        tenderCode = "CP-COMP";
                                        break;
                                    }
                            }
                        }
                        // Card Type 

                        // Payments
                        var listPayments = new List<bloyalOrdersvc.OrderPayment>();
                        var payment = new bloyalOrdersvc.OrderPayment();
                        payment.TenderCode = tenderCode;

                        payment.External = false;
                        payment.Captured = false;
                        payment.AuthorizationCode = "";
                        payment.TransactionCode = "";

                        if (reservation.payment_status == (int)ReservationPaymentStatus.PAIDFULL)
                        {
                            var payments = eventDAL.GetReservationPayments(reservationId);
                            foreach (var p in payments)
                            {
                                payment.TransactionCode = p.transaction_id;
                                payment.AuthorizationCode = p.appoval_code;
                                payment.External = true;
                                payment.Captured = true;
                                order.Status = "Closed";

                                string cardNumber = reservation.pay_card.card_last_four_digits;
                                string cardType = p.payment_card_type == null ? reservation.pay_card.card_type : p.payment_card_type;
                                payment.Title = string.Format("{0} - #### #### #### {1} (Credit Card)", cardType, cardNumber);
                                break;
                            }
                        }

                        if (reservation.fee_due == 0)
                        {
                            payment.External = true;
                            payment.Captured = true;
                            order.Status = "Closed";
                        }

                        payment.Amount = reservation.fee_due;
                        listPayments.Add(payment);

                        // Add payments to order
                        order.Payments = listPayments.ToArray();

                        // order.TotalTax = reservation.SalesTax
                        // order.TotalProductTax = reservation.SalesTax
                        order.TotalAmount = reservation.fee_due;

                        var options = new List<string>();
                        options.Add("CreateCustomer");  // Create the customer if it doesn't exist.   
                        options.Add("ExternalShippingCalculation"); // Use this option if your system is calculating shipping charges (vs bLoyal)
                        if (reservation.sales_tax > 0 && EnableService)
                            options.Add("ExternalTaxCalculation");// Use this option if your system is calculating sales tax (vs bLoyal)


                        // ## SUBMIT ORDER ##
                        string externalOrderNumber = await svc.SubmitOrderAsync(cred, cred.DeviceKey, order, options.ToArray());
                        bool IsExport = false;
                        if (!string.IsNullOrWhiteSpace(externalOrderNumber))
                        {
                            extOrderId = externalOrderNumber;
                            eventDAL.InsertExportLog((int)Common.Common.ExportType.bLoyal, reservationId, "success - " + externalOrderNumber, 1, reservation.fee_due, "");
                            //eventDAL.SaveReservationExportToLog(ExportType.bLoyal, RsvpID, externalOrderNumber, ExportStatus.Success, reservation.AmountDue, processedBy);
                            eventDAL.ReservationV2StatusNote_Create(reservationId, reservation.status, reservation.member_id, "", false, 0, 0, 0, "SYNC - Order upserted to bLoyal");
                            IsExport = true;

                            if (order.Status == "Closed")
                            {
                                // ## CAPTURE ORDER ##
                                await svc.CaptureOrderAsync(cred, cred.DeviceKey, externalOrderNumber, "");

                                // ## picked up ORDER ##
                                var trackingnumber = "".Split(",");
                                await svc.ShipOrderShipmentAsync(cred, externalOrderNumber, "1", shipDate, trackingnumber);
                            }
                        }
                        else
                        {
                            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                            eventDAL.InsertExportLog((int)Common.Common.ExportType.bLoyal, reservationId, "Fail to Sync", 0, reservation.fee_due, "");
                            logDAL.InsertLog("WebApi", "bLoyalSendOrder: " + "SubmitOrder Failed", "", 1, member_id);
                            IsExport = false;
                        }


                    }


                }

            }

            return extOrderId;
        }

        #endregion

        #region SalesForce Methods
        public static async Task<UserDetailModel> SalesForceResolveCustomer(string userName, string password, string securityToken, string emailAddress, string clubStatusField = "Club_Member_Status__c")
        {
            UserDetailModel userData = null;

            string pwd = password + securityToken;
            if (string.IsNullOrWhiteSpace(clubStatusField))
                clubStatusField = "Club_Member_Status__c";

            SF.SoapClient soapClient = new SF.SoapClient();
            SF.loginResponse resp = await soapClient.loginAsync(new SF.LoginScopeHeader(), userName, pwd);

            if (resp != null && resp.result != null)
            {
                string serverURL = resp.result.serverUrl;
                string sqlQuery = "select Id,RMS_Account_Number__c,HomePhone, MailingCity, MailingCountry, MailingStreet, MailingState, MailingPostalCode, LastModifiedDate," + clubStatusField + " from contact where email = '" + emailAddress + "'";
                SF.SessionHeader header = new SF.SessionHeader
                {
                    sessionId = resp.result.sessionId
                };

                var queryResp = await soapClient.queryAsync(header, new SF.QueryOptions { }, new SF.MruHeader { }, null, sqlQuery);
                if (queryResp != null && queryResp.result != null && queryResp.result.size > 0)
                {
                    userData = new UserDetailModel();
                    SF.Contact contact = (SF.Contact)queryResp.result.records[0];
                    userData.first_name = contact.FirstName;
                    userData.last_name = contact.LastName;
                    userData.membership_number = contact.Id;
                    userData.phone_number = Utility.FormatTelephoneNumber(contact.HomePhone + "", contact.MailingCountry);
                    userData.address = new Model.UserAddress
                    {
                        address_1 = contact.MailingStreet,
                        city = contact.MailingCity,
                        state = contact.MailingState,
                        country = contact.MailingCountry,
                        zip_code = contact.MailingPostalCode
                    };
                    userData.member_status = (bool)contact.GetType().GetProperty(clubStatusField).GetValue(contact);
                    if (userData.member_status)
                        userData.customer_type = 1;
                    else
                        userData.customer_type = 0;
                }
            }

            //resp.result.serverUrl;
            return userData;

        }

        private static async Task<bool> SalesForceSaveContact(string userName, string password, string securityToken, UserDetailModel userDetail)
        {
            if (userDetail == null || userDetail.email.ToLower().IndexOf("@noemail") > -1)
                return false;

            string pwd = password + securityToken;
            SF.SoapClient soapClient = new SF.SoapClient();
            SF.loginResponse resp = await soapClient.loginAsync(new SF.LoginScopeHeader(), userName, pwd);

            if (resp != null && resp.result != null)
            {
                string serverURL = resp.result.serverUrl;
                SF.SessionHeader header = new SF.SessionHeader
                {
                    sessionId = resp.result.sessionId
                };
                SF.Contact contact = new SF.Contact();


                contact.Id = userDetail.membership_number;
                if (!string.IsNullOrWhiteSpace(userDetail.first_name) && !string.IsNullOrWhiteSpace(userDetail.last_name))
                {
                    contact.FirstName = userDetail.first_name;
                    contact.LastName = userDetail.last_name;
                }
                contact.Email = userDetail.email;
                contact.LastModifiedDate = DateTime.Now;
                contact.HomePhone = userDetail.phone_number;
                var data = new SF.sObject[] { contact };

                var updResp = await soapClient.updateAsync(header, null, null, null, null, null, null, null, null, null, null, data);

            }

            return true;
        }

        #endregion


        //public static Vin65ContactViewModel GetContactbyEmailId(string Username, string Password, string emailid, string strerror = "")
        //{
        //    Vin65ContactViewModel contact = new Vin65ContactViewModel();
        //    Vin65WebService.ContactServiceChannel ws;
        //    com.vin65.webservices.Request9 req = new com.vin65.webservices.Request9();
        //    com.vin65.webservices.Response9 Response = new com.vin65.webservices.Response9();
        //    com.vin65.webservices.Security Security = new com.vin65.webservices.Security();

        //    Security.Username = Username;
        //    Security.Password = Password;

        //    req.Security = Security;
        //    req.Email = emailid;

        //    Response = ws.SearchContacts(req);

        //    if (Response.RecordCount > 0)
        //    {
        //        contact.Vin65ID = Response.Contacts(0).ContactID;
        //        //contact.RmsID = Response.Contacts(0).
        //        if (DateTime.TryParse(Response.Contacts(0).DateModified))
        //        {
        //            contact.DateModified = Response.Contacts(0).DateModified;
        //        }
        //        else
        //        {
        //            contact.DateModified = DateTime.Now;
        //        }
        //        contact.HomePhone = Response.Contacts(0).Phone;
        //        contact.BillingStreet = Response.Contacts(0).Address;
        //        contact.BillingCity = Response.Contacts(0).City;
        //        contact.BillingState = Response.Contacts(0).StateCode;
        //        contact.BillingZip = Response.Contacts(0).ZipCode;
        //        contact.Email = Response.Contacts(0).Email;
        //        contact.Country = Response.Contacts(0).CountryCode;
        //    }
        //    else
        //    {
        //        //put some code in here to handle no records being returned
        //        string message = "No records returned.";
        //    }
        //    return contact;
        //}

        public static string GenerateChargeDescription(string WineryDisplayName, string DynamicDesc, string strInvoiceId, Configuration.Gateway gateway)
        {

            string desc = "";

            //If Dynamic Desc passed use it, else use Winery Display Name
            string nameDesc = DynamicDesc;
            if (object.ReferenceEquals(DynamicDesc.Trim(), string.Empty))
            {
                nameDesc = WineryDisplayName;
            }

            switch (gateway)
            {

                case Configuration.Gateway.Braintree:
                    string val = Common.StringHelpers.AlphaNumbericOnly(nameDesc);
                    if (val.Length > 12)
                    {
                        desc = string.Format("{0}*TIX{1}", (val.Substring(0, 12)).PadRight(12, '0'), strInvoiceId.ToString().PadLeft(6, '0'));
                    }
                    else
                    {
                        desc = string.Format("{0}*TIX{1}", (val.Substring(0, val.Length)).PadRight(12, '0'), strInvoiceId.ToString().PadLeft(6, '0'));
                    }
                    break;
                default:
                    desc = "";
                    break;
            }

            return desc;

        }

        public static string FormatPhoneNumberDecimal(decimal phoneNumber, string countryCode = "US")
        {
            string PhoneNum = "";
            if (countryCode == "US" | countryCode == "CA")
            {
                if (phoneNumber.ToString().Trim().Length == 10)
                    PhoneNum = string.Format("+1 ({0}) {1}-{2}", phoneNumber.ToString().Substring(0, 3), phoneNumber.ToString().Substring(3, 3), phoneNumber.ToString().Substring(6));
                else if (phoneNumber.ToString().Trim().Length == 11)
                {
                    string modPhone = phoneNumber.ToString();
                    if (Common.Common.Left(modPhone, 1) == "1")
                    {
                        modPhone = modPhone.TrimStart('1');
                        if (!string.IsNullOrWhiteSpace(modPhone))
                            PhoneNum = string.Format("+1 ({0}) {1}-{2}", modPhone.Substring(0, 3), modPhone.Substring(3, 3), modPhone.Substring(6));
                        else
                            PhoneNum = phoneNumber.ToString();
                    }
                }
                else
                    PhoneNum = phoneNumber.ToString();
            }
            else
                PhoneNum = phoneNumber.ToString();

            if (PhoneNum == "0")
                PhoneNum = "";
            return PhoneNum;
        }

        public static decimal ExtractPhone(string expr)
        {

            if (expr == null)
            {
                return 0;
            }

            expr = expr.Replace("+1", "");
            expr = expr.Replace("1 (", "");
            expr = expr.Replace(")", "");
            expr = expr.Replace("(", "");
            expr = expr.Replace("+", "");
            expr = expr.Replace(" ", "").Trim();

            decimal decPhone = 0;
            string numbersOnly = "";

            numbersOnly = string.Join(null, Regex.Split(expr, "[^\\d]"));

            if (!object.ReferenceEquals(numbersOnly.Trim(), string.Empty))
            {
                if (numbersOnly.Length == 11 && numbersOnly.Substring(0, 1) == "1")
                {
                    numbersOnly = numbersOnly.Substring(numbersOnly.Length - 10, 10);
                }
                if (numbersOnly.Trim().Length > 6)
                {
                    decimal.TryParse(numbersOnly, out decPhone);
                }
            }
            return decPhone;
        }

        public static string GenerateUsername(string firstname, string lastname)
        {
            Random random = new Random();
            int rNumber = random.Next(1000, 999999);
            string username = "";


            try
            {
                username = (firstname.Trim().Replace(".", "") + "." + lastname.Trim().Replace(".", "")).Replace(" ", "");
                username = System.Text.RegularExpressions.Regex.Replace(username, "[^a-zA-Z0-9_.]", "").Trim();
                username = username + rNumber.ToString().Trim() + "@noemail.com";
            }
            catch (Exception ex)
            {
                username = System.DateTime.UtcNow.Second.ToString() + rNumber.ToString().Trim() + "@noemail.com";
            }

            return username;

        }

        public static bool ReVerifyPhone(string existingPhone, int MobileStatus)
        {

            bool reverify = false;

            //If already verified ore prev verified but failed and the numbers don't match then reverify
            if ((MobileStatus == (int)MobileNumberStatus.verified || MobileStatus == (int)MobileNumberStatus.failed))
            {
                reverify = true;
            }

            return reverify;

        }

        public static string FormatPhoneNumber(string phoneNumber)
        {
            string PhoneNum = "";
            //Extract just numbers
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                PhoneNum = ExtractPhone(phoneNumber).ToString();
            }
            //if phone is 10 digits then we format it further, else it returns the empty or raw phone numbers (international for example).
            if (PhoneNum.Trim().Length == 10)
            {
                PhoneNum = string.Format("+1 ({0}) {1}-{2}", PhoneNum.Substring(0, 3), PhoneNum.Substring(3, 3), PhoneNum.Substring(6));
            }
            return PhoneNum;
        }

        public static string FormatZoomMeetingId(string MeetingId)
        {
            string FormatMeetingId = MeetingId;

            if (MeetingId.Length == 11)
            {
                FormatMeetingId = MeetingId.Substring(0, 3) + "-" + MeetingId.Substring(3, 4) + "-" + MeetingId.Substring(7, 4);
            }

            return FormatMeetingId;
        }

        public static string FormatTelephoneNumber(string phoneNumber, string country)
        {
            phoneNumber = ExtractPhone(phoneNumber).ToString();
            string FormattedPhone = "";
            try
            {
                country = country + "";

                if (country.ToUpper() != "CN" && country.ToUpper() != "GB" && country.ToUpper() != "US")
                    country = "US";

                switch (country.ToUpper())
                {
                    case "CN":
                        {
                            if (phoneNumber.Length == 10)
                                FormattedPhone = string.Format("({0})-{1}", phoneNumber.Substring(0, 2), phoneNumber.Substring(2, 8));
                            else
                                FormattedPhone = phoneNumber;
                            break;
                        }

                    case "GB":
                        {
                            if (phoneNumber.Length > 10)
                                FormattedPhone = string.Format("{0} {1} {2} {3}", phoneNumber.Substring(0, 2), phoneNumber.Substring(2, 3), phoneNumber.Substring(5, 3), phoneNumber.Substring(8, phoneNumber.Length - 8));
                            else
                                FormattedPhone = phoneNumber;
                            break;
                        }

                    case "US":
                        {
                            if (phoneNumber.Length == 10)
                                FormattedPhone = string.Format("+1 ({0}) {1}-{2}", phoneNumber.Substring(0, 3), phoneNumber.Substring(3, 3), phoneNumber.Substring(6));
                            else if (phoneNumber.Length == 11)
                            {
                                phoneNumber = phoneNumber.TrimStart('1');
                                FormattedPhone = string.Format("+1 ({0}) {1}-{2}", phoneNumber.Substring(0, 3), phoneNumber.Substring(3, 3), phoneNumber.Substring(6, 4));
                            }
                            else if (phoneNumber.Length == 12 || phoneNumber.Length == 13)
                            {
                                phoneNumber = phoneNumber.Replace("+1 ", "").Replace("+1", "");
                                FormattedPhone = string.Format("+1 ({0}) {1}-{2}", phoneNumber.Substring(0, 3), phoneNumber.Substring(3, 3), phoneNumber.Substring(6, 4));
                            }
                            else
                                FormattedPhone = "+1" + phoneNumber;
                            break;
                        }

                    default:
                        {
                            FormattedPhone = "+1" + phoneNumber;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                FormattedPhone = "+1" + phoneNumber;
            }

            return FormattedPhone;
        }

        public static int GenerateSimpleRandomNumber()
        {
            int id = 0;

            Random r = new Random();
            id = r.Next(1000000, 9999999);

            return id;
        }

        

        #region Vin65

        public static string Vin65SendOrder(string Email, string Password, string Username)
        {


            return "";

        }
        public async static Task<List<Vin65Model>> Vin65GetContacts(string Email, string Password, string Username)
        {
            List<Vin65Model> modelList = new List<Vin65Model>();
            bool successful = false;
            string recordCount = string.Empty;
            try
            {
                string soapString = string.Empty;

                if (Email.IndexOf("@") > -1)
                {
                    soapString = Vin65GetContactsSoapRequest(Email, Password, Username);
                }
                else
                {
                    soapString = Vin65GetContactByLastNameSoapRequest(Email, Password, Username);
                }

                //LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                //logDAL.InsertLog("Vin65GetContacts", "soapString: " + soapString, "", 3, 0);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("SOAPAction", "");
                    var content = new StringContent(soapString, Encoding.UTF8, "text/xml");
                    HttpClient wsClient = new HttpClient();
                    wsClient.BaseAddress = new Uri(string.Format("https://webservices.vin65.com/V300/ContactService.cfc?wsdl", Environment.MachineName));
                    using (var response = await client.PostAsync(wsClient.BaseAddress, content))
                    {
                        var soapResponse = await response.Content.ReadAsStringAsync();

                        var soap = XDocument.Parse(soapResponse);

                        //XNamespace ns = "SearchContactsReturn";
                        //soapResponse = getBetween(soapResponse,"","");
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(soapResponse);
                        XmlNodeList isSuccessful = doc.GetElementsByTagName("IsSuccessful");
                        if (isSuccessful.Count >= 1)
                        {
                            successful = Convert.ToBoolean(isSuccessful[0].InnerText);
                        }
                        XmlNodeList recordCountTags = doc.GetElementsByTagName("RecordCount");
                        if (recordCountTags.Count >= 1)
                        {
                            recordCount = recordCountTags[0].InnerText;
                        }

                        XmlNode node = doc.DocumentElement.FirstChild;
                        // Create a list of the child nodes of 
                        // the first node under the root
                        XmlNodeList lstSearchcontact = node.ChildNodes;
                        // Visit each node
                        for (int i = 0; i < lstSearchcontact.Count; i++)
                        {
                            // Look for a node named CastMembers
                            if (lstSearchcontact[i].LocalName == "SearchContactsResponse")
                            {
                                // Once/if you find it,
                                // 1. Access its first child
                                // 2. Create a list of its child nodes
                                XmlNodeList lstSearchContactReturn = lstSearchcontact[i].ChildNodes;
                                // Display the values of the nodes
                                if (lstSearchContactReturn[i].LocalName == "SearchContactsReturn")
                                {
                                    XmlNodeList lstContact = lstSearchContactReturn[i].ChildNodes;
                                    if (lstContact[i].LocalName == "Contacts")
                                    {
                                        XmlNodeList lastContact = lstContact[i].ChildNodes;
                                        for (int k = 0; k < lastContact.Count; k++)
                                        {
                                            var model = new Vin65Model();
                                            if (lastContact[k].LocalName == "Contacts")
                                            {
                                                XmlNodeList lstActor = lastContact[k].ChildNodes;
                                                for (int j = 0; j < lstActor.Count; j++)
                                                {
                                                    switch (lstActor[j].LocalName)
                                                    {
                                                        case "ContactID":
                                                            model.Vin65ID = lstActor[j].InnerText;
                                                            var listClub = await Vin65GetClubMembership(Password, Username, model.Vin65ID);

                                                            if (listClub == null)
                                                                listClub = await Vin65GetClubMembership(Password, Username, model.Vin65ID);

                                                            if (listClub == null)
                                                                listClub = new List<string>();

                                                            var listcontacttypes = await Vin65GetContactTypeForContact(Password, Username, model.Vin65ID);

                                                            if (listcontacttypes == null)
                                                                listcontacttypes = await Vin65GetContactTypeForContact(Password, Username, model.Vin65ID);

                                                            if (listcontacttypes == null)
                                                                listcontacttypes = new List<string>();

                                                            listcontacttypes.AddRange(listClub);

                                                            model.contact_types = listcontacttypes;

                                                            if (listClub.Count > 0)
                                                                model.member_status = true;

                                                            break;
                                                        case "DateModified":
                                                            model.DateModified = Convert.ToDateTime(lstActor[j].InnerText);
                                                            break;
                                                        case "Phone":
                                                            model.HomePhone = lstActor[j].InnerText;
                                                            break;
                                                        case "Address":
                                                            model.BillingStreet = lstActor[j].InnerText;
                                                            break;
                                                        case "Address2":
                                                            model.BillingStreet2 = lstActor[j].InnerText;
                                                            break;
                                                        case "FirstName":
                                                            model.FirstName = lstActor[j].InnerText;
                                                            break;
                                                        case "LastName":
                                                            model.LastName = lstActor[j].InnerText;
                                                            break;
                                                        case "City":
                                                            model.BillingCity = lstActor[j].InnerText;
                                                            break;
                                                        case "StateCode":
                                                            model.BillingState = lstActor[j].InnerText;
                                                            break;
                                                        case "ZipCode":
                                                            model.BillingZip = lstActor[j].InnerText;
                                                            break;
                                                        case "Email":
                                                            model.Email = lstActor[j].InnerText;
                                                            break;
                                                        case "CountryCode":
                                                            model.Country = lstActor[j].InnerText;
                                                            break;
                                                        case "LastOrderDate":
                                                            if (lstActor[j].InnerText != "")
                                                            {
                                                                model.last_order_date = Convert.ToDateTime(lstActor[j].InnerText);
                                                            }
                                                            break;
                                                        case "LifetimeValue":
                                                            if (lstActor[j].InnerText != "")
                                                            {
                                                                model.ltv = Convert.ToDecimal(lstActor[j].InnerText);
                                                            }
                                                            break;
                                                        case "OrderCount":
                                                            if (lstActor[j].InnerText != "")
                                                            {
                                                                model.order_count = (int)Math.Round(Convert.ToDecimal(lstActor[j].InnerText), 0);
                                                            }
                                                            break;
                                                    }
                                                }
                                            }

                                            if (Email.IndexOf("@") > -1)
                                            {
                                                if (model.Email.ToLower() == Email.ToLower())
                                                    modelList.Add(model);
                                            }
                                            else
                                                modelList.Add(model);
                                        }
                                    }
                                }
                            }
                        }
                        //return soapResponse;
                        //return this.ParseSoapResponse(soapResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return modelList;
        }

        public async static Task<string> Vin65AddUpdateCreditCard(string Password, string Username, int CardExpiryMonth, int CardExpiryYear, string NameOnCard, string ContactId, string CreditCradType, string CardNumber)
        {
            bool successful = false;
            string CCID = string.Empty;
            try
            {
                var soapString = Vin65AddUpdateCreditCardSoapRequest(Password, Username, CardExpiryMonth, CardExpiryYear, NameOnCard, ContactId, CreditCradType, CardNumber);
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("SOAPAction", "");
                    var content = new StringContent(soapString, Encoding.UTF8, "text/xml");
                    HttpClient wsClient = new HttpClient();
                    wsClient.BaseAddress = new Uri(string.Format("https://webservices.vin65.com/V300/CreditCardServiceX.cfc?wsdl", Environment.MachineName));
                    using (var response = await client.PostAsync(wsClient.BaseAddress, content))
                    {
                        var soapResponse = await response.Content.ReadAsStringAsync();

                        var soap = XDocument.Parse(soapResponse);

                        XNamespace ns = "AddUpdateCreditCardReturn";
                        //soapResponse = getBetween(soapResponse,"","");
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(soapResponse);
                        XmlNodeList isSuccessful = doc.GetElementsByTagName("IsSuccessful");
                        if (isSuccessful != null)
                        {
                            XmlNodeList CreditCardID = doc.GetElementsByTagName("CreditCardID");
                            bool.TryParse(isSuccessful[0].InnerText, out successful);
                            CCID = CreditCardID[0].InnerText;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return CCID;
        }

        public async static Task<string> Vin65UpsertContact(string Password, string Username, string Address, string City, string ContactID, string CountryCode, string Email, string Firstname, string Lastname, string MainPhone, string StateCode, string ZipCode,bool IsSingleOptIn)
        {
            string vin65Id = string.Empty;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            string responce = "";

            try
            {
                Firstname = Firstname.Replace("&", "");
                Lastname = Lastname.Replace("&", "");
                MainPhone = ExtractPhone(MainPhone).ToString();

                logDAL.InsertLog("Vin65UpsertContact", "Request URL:https://webservices.vin65.com/v201/contactService.cfc?wsdl", "", 3, 0);

                var soapString = Vin65UpsertContactSoapRequest(Password, Username, Address, City, ContactID, CountryCode, Email, Firstname, Lastname, MainPhone, StateCode, ZipCode, IsSingleOptIn);

                logDAL.InsertLog("Vin65UpsertContact", "soapString: " + soapString, "", 3, 0);

               
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("SOAPAction", "");
                    var content = new StringContent(soapString, Encoding.UTF8, "text/xml");
                    HttpClient wsClient = new HttpClient();
                    wsClient.BaseAddress = new Uri(string.Format("https://webservices.vin65.com/v201/contactService.cfc?wsdl", Environment.MachineName));
                    using (var response = await client.PostAsync(wsClient.BaseAddress, content))
                    {
                        var soapResponse = await response.Content.ReadAsStringAsync();

                        responce = soapResponse;
                        //logDAL.InsertLog("Vin65UpsertContact", "Response:" + soapResponse, "", 1, 0);

                        var soap = XDocument.Parse(soapResponse);

                        XNamespace ns = "UpsertContactReturn";
                        //soapResponse = getBetween(soapResponse,"","");
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(soapResponse);
                        XmlNodeList internalKeyCode = doc.GetElementsByTagName("internalKeyCode");
                        if (internalKeyCode != null)
                        {
                            vin65Id = internalKeyCode[0].InnerText;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                
                logDAL.InsertLog("Vin65UpsertContact", "Exception:" + ex.Message, "", 1, 0);
            }

            logDAL.InsertLog("Vin65UpsertContact", "Response:" + responce, "", 1, 0);

            return vin65Id;
        }

        public async static Task<List<string>> Vin65GetContactTypeForContact(string Password, string Username, string contactID)
        {
            List<string> listcontacttypes = new List<string>();
            try
            {
                var soapString = Vin65GetContactTypeForContactSoapRequest(Password, Username, contactID);
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("SOAPAction", "");
                    var content = new StringContent(soapString, Encoding.UTF8, "text/xml");
                    HttpClient wsClient = new HttpClient();
                    wsClient.BaseAddress = new Uri(string.Format("https://webservices.vin65.com/v201/contactService.cfc?wsdl", Environment.MachineName));
                    using (var response = await client.PostAsync(wsClient.BaseAddress, content))
                    {
                        var soapResponse = await response.Content.ReadAsStringAsync();

                        var soap = XDocument.Parse(soapResponse);

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(soapResponse);

                        XmlNode node = doc.DocumentElement.FirstChild;
                        XmlNodeList lstSearchcontact = node.ChildNodes;
                        for (int i = 0; i < lstSearchcontact.Count; i++)
                        {
                            if (lstSearchcontact[i].LocalName == "GetContactTypeForContactResponse")
                            {
                                XmlNodeList lstSearchContactReturn = lstSearchcontact[i].ChildNodes;
                                if (lstSearchContactReturn[i].LocalName == "GetContactTypeForContactReturn")
                                {
                                    XmlNodeList lstContact = lstSearchContactReturn[i].ChildNodes;
                                    for (int k = 0; k < lstContact.Count; k++)
                                    {
                                        if (lstContact[k].LocalName == "contactTypesForContact")
                                        {
                                            XmlNodeList lastContactclub = lstContact[k].ChildNodes;
                                            for (int j = 0; j < lastContactclub.Count; j++)
                                            {
                                                if (lastContactclub[j].LocalName == "contactTypesForContact")
                                                {
                                                    XmlNodeList list = lastContactclub[j].ChildNodes;
                                                    for (int l = 0; l < list.Count; l++)
                                                    {
                                                        if (list[l].LocalName == "ContactType")
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(list[l].InnerText)))
                                                                listcontacttypes.Add(Convert.ToString(list[l].InnerText));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                listcontacttypes = null;
            }
            return listcontacttypes;
        }

        public async static Task<List<string>> Vin65GetClubMembership(string Password, string Username, string contactID)
        {
            List<string> listcontacttypes = new List<string>();
            try
            {
                var soapString = Vin65GetClubMembershipSoapRequest(Password, Username, contactID);
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("SOAPAction", "");
                    var content = new StringContent(soapString, Encoding.UTF8, "text/xml");
                    HttpClient wsClient = new HttpClient();
                    wsClient.BaseAddress = new Uri(string.Format("http://webservices.vin65.com/v201/contactService.cfc?wsdl", Environment.MachineName));
                    using (var response = await client.PostAsync(wsClient.BaseAddress, content))
                    {
                        var soapResponse = await response.Content.ReadAsStringAsync();

                        var soap = XDocument.Parse(soapResponse);

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(soapResponse);


                        XmlNode node = doc.DocumentElement.FirstChild;
                        // Create a list of the child nodes of 
                        // the first node under the root
                        XmlNodeList lstSearchcontact = node.ChildNodes;
                        // Visit each node
                        for (int i = 0; i < lstSearchcontact.Count; i++)
                        {
                            if (lstSearchcontact[i].LocalName == "GetClubMembershipResponse")
                            {
                                // Once/if you find it,
                                // 1. Access its first child
                                // 2. Create a list of its child nodes
                                XmlNodeList lstSearchContactReturn = lstSearchcontact[i].ChildNodes;
                                // Display the values of the nodes
                                if (lstSearchContactReturn[i].LocalName == "GetClubMembershipReturn")
                                {
                                    XmlNodeList lstContact = lstSearchContactReturn[i].ChildNodes;

                                    for (int k = 0; k < lstContact.Count; k++)
                                    {
                                        //if (lstContact[k].LocalName == "isSuccessful")
                                        //{
                                        //    successful= Convert.ToBoolean(lstContact[k].InnerText);
                                        //}
                                        if (lstContact[k].LocalName == "clubMemberships")
                                        {
                                            XmlNodeList lastContactclub = lstContact[k].ChildNodes;
                                            for (int j = 0; j < lastContactclub.Count; j++)
                                            {
                                                if (lastContactclub[j].LocalName == "clubMemberships")
                                                {
                                                    bool successful = false;
                                                    string ClubName = "";
                                                    XmlNodeList list = lastContactclub[j].ChildNodes;
                                                    for (int l = 0; l < list.Count; l++)
                                                    {
                                                        if (list[l].LocalName == "isActive")
                                                        {
                                                            successful = Convert.ToBoolean(list[l].InnerText);
                                                        }

                                                        if (list[l].LocalName == "ClubName")
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(list[l].InnerText)))
                                                                ClubName = Convert.ToString(list[l].InnerText);
                                                        }
                                                    }

                                                    if (successful && !string.IsNullOrEmpty(ClubName))
                                                        listcontacttypes.Add(ClubName);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                listcontacttypes = null;
            }
            return listcontacttypes;
        }

        public async static Task<List<AccountNote>> Vin65GetNotesForContact(string Password, string Username, string contactID, int memberId)
        {
            List<AccountNote> listNotes = new List<AccountNote>();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            var soapString = Vin65GetNotesForContactSoapRequest(Password, Username, contactID);
            logDAL.InsertLog("Vin65GetNotesForContact", "XML Request:" + soapString, "Core API", 3, memberId);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("SOAPAction", "");
                var content = new StringContent(soapString, Encoding.UTF8, "text/xml");
                HttpClient wsClient = new HttpClient();
                wsClient.BaseAddress = new Uri(string.Format("https://webservices.vin65.com/V300/NoteService.cfc?wsdl", Environment.MachineName));
                using (var response = await client.PostAsync(wsClient.BaseAddress, content))
                {
                    var soapResponse = await response.Content.ReadAsStringAsync();
                    logDAL.InsertLog("Vin65GetNotesForContact", "XML Response:" + soapResponse, "Core API", 3, memberId);

                    var soap = XDocument.Parse(soapResponse);

                    //soapResponse = getBetween(soapResponse,"","");
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(soapResponse);

                    XmlNode node = doc.DocumentElement.FirstChild;
                    XmlNodeList lstSearchcontact = node.ChildNodes;
                    for (int i = 0; i < lstSearchcontact.Count; i++)
                    {
                        if (lstSearchcontact[i].LocalName == "SearchNotesResponse")
                        {
                            XmlNodeList lstSearchContactReturn = lstSearchcontact[i].ChildNodes;
                            if (lstSearchContactReturn[i].LocalName == "SearchNotesReturn")
                            {
                                XmlNodeList lstContact = lstSearchContactReturn[i].ChildNodes;
                                for (int k = 0; k < lstContact.Count; k++)
                                {
                                    if (lstContact[k].LocalName == "Notes")
                                    {
                                        XmlNodeList lastContactclub = lstContact[k].ChildNodes;
                                        for (int j = 0; j < lastContactclub.Count; j++)
                                        {
                                            if (lastContactclub[j].LocalName == "Notes")
                                            {
                                                var note = new AccountNote();
                                                XmlNodeList list = lastContactclub[j].ChildNodes;
                                                for (int l = 0; l < list.Count; l++)
                                                {
                                                    if (list[l].LocalName == "NoteID")
                                                    {
                                                        note.note_id = (Convert.ToString(list[l].InnerText));
                                                    }
                                                    else if (list[l].LocalName == "Subject")
                                                    {
                                                        note.subject = (Convert.ToString(list[l].InnerText)).Trim();
                                                    }
                                                    else if (list[l].LocalName == "Note")
                                                    {
                                                        note.note = (Convert.ToString(list[l].InnerText)).Trim();
                                                    }
                                                    else if (list[l].LocalName == "NoteDate")
                                                    {
                                                        note.note_date = ParseDate(Convert.ToString(list[l].InnerText));
                                                    }
                                                    else if (list[l].LocalName == "DateAdded")
                                                    {
                                                        note.date_added = ParseDate(Convert.ToString(list[l].InnerText));
                                                    }
                                                    else if (list[l].LocalName == "DateModified")
                                                    {
                                                        note.date_modified = ParseDate(Convert.ToString(list[l].InnerText));


                                                    }
                                                }
                                                listNotes.Add(note);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return listNotes;
        }

        public async static Task<bool> Vin65AddUpdateNote(string password, string userName, string noteId, string subject, string contactID, string note, DateTime noteDate, int memberId)
        {
            bool successful = false;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            var soapString = Vin65AddUpdateNotesForContactSoapRequest(password, userName, noteId, subject, note, noteDate, contactID);
            logDAL.InsertLog("Vin65AddUpdateNote", "XML Request:" + soapString, "Core API", 3, memberId);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("SOAPAction", "");
                var content = new StringContent(soapString, Encoding.UTF8, "text/xml");
                HttpClient wsClient = new HttpClient();
                wsClient.BaseAddress = new Uri(string.Format("https://webservices.vin65.com/V300/NoteService.cfc?wsdl", Environment.MachineName));
                using (var response = await client.PostAsync(wsClient.BaseAddress, content))
                {
                    var soapResponse = await response.Content.ReadAsStringAsync();
                    logDAL.InsertLog("Vin65AddUpdateNote", "XML Response:" + soapResponse, "Core API", 3, memberId);
                    var soap = XDocument.Parse(soapResponse);
                    //soapResponse = getBetween(soapResponse,"","");
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(soapResponse);
                    XmlNodeList isSuccessful = doc.GetElementsByTagName("IsSuccessful");
                    if (isSuccessful != null && isSuccessful.Count > 0)
                    {
                        bool.TryParse(isSuccessful[0].InnerText, out successful);
                    }
                    else
                    {
                        successful = false;
                        isSuccessful = doc.GetElementsByTagName("faultstring");
                        if (isSuccessful != null && isSuccessful.Count > 0)
                        {
                            string errorMessage = isSuccessful[0].InnerText;
                            throw new Exception(errorMessage);
                        }
                    }


                }
            }

            return successful;
        }

        private static DateTime? ParseDate(string dateStr)
        {
            if (!string.IsNullOrWhiteSpace(dateStr))
            {
                DateTime modifiedDate = DateTime.Now;
                bool isDate = DateTime.TryParse(dateStr, out modifiedDate);
                if (isDate)
                    return modifiedDate;
                else
                    return null;

            }
            else
            {
                return null;
            }
        }

        private static string Vin65GetContactsSoapRequest(string Email, string Password, string Username)
        {
            string SoapRequest = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?><x:Envelope xmlns:x=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v=""http://V300"" xmlns:req=""http://request.base.V300"" xmlns:sea1=""http://searchContacts.contact.V300"">
                                    <x:Header/>
                                    <x:Body>
                                        <v:SearchContacts>
                                            <v:Request>
                                                <req:Security>
                                                    <req:Password>{1}</req:Password>
                                                    <req:Username>{2}</req:Username>
                                                </req:Security>
                                                <sea1:Email>{0}</sea1:Email>
                                            </v:Request>
                                        </v:SearchContacts>
                                    </x:Body>
                                </x:Envelope>", Email, Password, Username);

            return SoapRequest;
        }

        private static string Vin65GetContactByLastNameSoapRequest(string LastName, string Password, string Username)
        {
            string SoapRequest = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?><x:Envelope xmlns:x=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v=""http://V300"" xmlns:req=""http://request.base.V300"" xmlns:sea1=""http://searchContacts.contact.V300"">
                                    <x:Header/>
                                    <x:Body>
                                        <v:SearchContacts>
                                            <v:Request>
                                                <req:Security>
                                                    <req:Password>{1}</req:Password>
                                                    <req:Username>{2}</req:Username>
                                                </req:Security>
                                                <sea1:LastName>{0}</sea1:LastName>
                                            </v:Request>
                                        </v:SearchContacts>
                                    </x:Body>
                                </x:Envelope>", LastName, Password, Username);

            return SoapRequest;
        }

        private static string Vin65GetClubMembershipSoapRequest(string Password, string Username, string contactID)
        {
            string SoapRequest = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?><x:Envelope xmlns:x=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v=""http://v201"" xmlns:bas=""http://base.v201"" xmlns:get3=""http://getClubMembership.contact.v201"">
                                <x:Header/><x:Body>
                                    <v:GetClubMembership>
                                        <v:getClubMembershipRequest>
                                            <bas:password>{1}</bas:password>
                                            <bas:username>{2}</bas:username>
                                            <get3:altClubMembershipID></get3:altClubMembershipID>
                                            <get3:altContactID></get3:altContactID>
                                            <get3:clubMembershipID></get3:clubMembershipID>
                                            <get3:contactID>{0}</get3:contactID>
                                        </v:getClubMembershipRequest>
                                    </v:GetClubMembership>
                                </x:Body></x:Envelope> ", contactID, Password, Username);

            return SoapRequest;
        }

        private static string Vin65GetContactTypeForContactSoapRequest(string Password, string Username, string contactID)
        {
            string SoapRequest = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?><x:Envelope xmlns:x=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v=""http://v201"" xmlns:bas=""http://base.v201"" xmlns:get2=""http://getContactTypeForContact.contact.v201"">
                                    <x:Header/>
                                    <x:Body>
                                        <v:GetContactTypeForContact>
                                            <v:getContactTypeForContactRequest>
                                                <bas:password>{1}</bas:password>
                                                <bas:username>{2}</bas:username>
                                                <get2:altContactID></get2:altContactID>
                                                <get2:contactID>{0}</get2:contactID>
                                            </v:getContactTypeForContactRequest>
                                        </v:GetContactTypeForContact>
                                    </x:Body>
                                </x:Envelope>", contactID, Password, Username);

            return SoapRequest;
        }

        private static string Vin65UpsertContactSoapRequest(string Password, string Username, string Address, string City, string ContactID, string CountryCode, string Email, string Firstname, string Lastname, string MainPhone, string StateCode, string ZipCode,bool IsSingleOptIn)
        {
            string SoapRequest = "";

            if (IsSingleOptIn)
            {
                SoapRequest = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?><soapenv:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v201=""http://v201"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"">
               <soapenv:Header/>
               <soapenv:Body>
                  <v201:UpsertContact soapenv:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                     <upsertContactRequest xsi:type=""ups:UpsertContactRequest"" xmlns:ups=""http://upsertContact.contact.v201"">
			            <password xsi:type=""xsd:string"">{0}</password>
                        <username xsi:type=""xsd:string"">{1}</username>
                        <contacts xsi:type=""ups:UpsertContact"" soapenc:arrayType=""ups:UpsertContact[]"">
			               <Address>{2}</Address>
			               <Address2 xsi:nil=""true""/>
			               <AltAccountNumber xsi:nil=""true""/>
			               <AltContactID xsi:nil=""true""/>
			               <Birthdate xsi:nil=""true""/>
			               <CashierID xsi:nil=""true""/>
			               <CellPhone xsi:nil=""true""/>
			               <City>{3}</City>
			               <Company xsi:nil=""true""/>
			               <ContactID>{4}</ContactID>
			               <CountryCode>{5}</CountryCode>
			               <CreditCardExpirationMonth xsi:nil=""true""/>
			               <CreditCardExpirationYear xsi:nil=""true""/>
			               <CreditCardNameOnCard xsi:nil=""true""/>
			               <CreditCardNumber xsi:nil=""true""/>
			               <CreditCardType xsi:nil=""true""/>
			               <Email>{6}</Email>
			               <FacebookProfileID xsi:nil=""true""/>
			               <Fax xsi:nil=""true""/>
			               <Firstname>{7}</Firstname>
			               <IsSingleOptIn xsi:nil=""true""/>
			               <Lastname>{8}</Lastname>
			               <MainPhone>{9}</MainPhone>
			               <Password xsi:nil=""true""/>
			               <PaymentTerms xsi:nil=""true""/>
			               <PriceLevel xsi:nil=""true""/>
			               <StateCode>{10}</StateCode>
			               <Title xsi:nil=""true""/>
			               <Username xsi:nil=""true""/>
			               <WholesaleNumber xsi:nil=""true""/>
			               <ZipCode>{11}</ZipCode>
			               <isDirectToTrade xsi:nil=""true""/>
			               <isNonTaxable xsi:nil=""true""/>
			            </contacts>
                     </upsertContactRequest>
                  </v201:UpsertContact>
               </soapenv:Body>
            </soapenv:Envelope>", Password, Username, Address, City, ContactID, CountryCode, Email, Firstname, Lastname, MainPhone, StateCode, ZipCode, IsSingleOptIn);
            }
            else
            {
                SoapRequest = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?><soapenv:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v201=""http://v201"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"">
               <soapenv:Header/>
               <soapenv:Body>
                  <v201:UpsertContact soapenv:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                     <upsertContactRequest xsi:type=""ups:UpsertContactRequest"" xmlns:ups=""http://upsertContact.contact.v201"">
			            <password xsi:type=""xsd:string"">{0}</password>
                        <username xsi:type=""xsd:string"">{1}</username>
                        <contacts xsi:type=""ups:UpsertContact"" soapenc:arrayType=""ups:UpsertContact[]"">
			               <Address>{2}</Address>
			               <Address2 xsi:nil=""true""/>
			               <AltAccountNumber xsi:nil=""true""/>
			               <AltContactID xsi:nil=""true""/>
			               <Birthdate xsi:nil=""true""/>
			               <CashierID xsi:nil=""true""/>
			               <CellPhone xsi:nil=""true""/>
			               <City>{3}</City>
			               <Company xsi:nil=""true""/>
			               <ContactID>{4}</ContactID>
			               <CountryCode>{5}</CountryCode>
			               <CreditCardExpirationMonth xsi:nil=""true""/>
			               <CreditCardExpirationYear xsi:nil=""true""/>
			               <CreditCardNameOnCard xsi:nil=""true""/>
			               <CreditCardNumber xsi:nil=""true""/>
			               <CreditCardType xsi:nil=""true""/>
			               <Email>{6}</Email>
			               <FacebookProfileID xsi:nil=""true""/>
			               <Fax xsi:nil=""true""/>
			               <Firstname>{7}</Firstname>
			               <IsSingleOptIn xsi:nil=""false""/>
			               <Lastname>{8}</Lastname>
			               <MainPhone>{9}</MainPhone>
			               <Password xsi:nil=""true""/>
			               <PaymentTerms xsi:nil=""true""/>
			               <PriceLevel xsi:nil=""true""/>
			               <StateCode>{10}</StateCode>
			               <Title xsi:nil=""true""/>
			               <Username xsi:nil=""true""/>
			               <WholesaleNumber xsi:nil=""true""/>
			               <ZipCode>{11}</ZipCode>
			               <isDirectToTrade xsi:nil=""true""/>
			               <isNonTaxable xsi:nil=""true""/>
			            </contacts>
                     </upsertContactRequest>
                  </v201:UpsertContact>
               </soapenv:Body>
            </soapenv:Envelope>", Password, Username, Address, City, ContactID, CountryCode, Email, Firstname, Lastname, MainPhone, StateCode, ZipCode, IsSingleOptIn);
            }
                
            return SoapRequest;
        }

        private static string Vin65AddUpdateCreditCardSoapRequest(string Password, string Username, int CardExpiryMonth, int CardExpiryYear, string NameOnCard, string ContactId, string CreditCradType, string CardNumber)
        {
            string SoapRequest = string.Format(@"<x:Envelope xmlns:x=""http://schemas.xmlsoap.org/soap/envelope/""
                xmlns:def = ""http://DefaultNamespace""
                xmlns:req = ""http://request.base.V300""
                xmlns:add = ""http://addUpdateCreditCard.creditCard.V300""
                xmlns:cre = ""http://_creditcard.creditCard.V300"">
                 <x:Header/>
                  <x:Body>
                       <def:AddUpdateCreditCard>
                            <def:Request>
                                 <req:Security>
                                      <req:Password>{0}</req:Password>
                                       <req:Username>{1}</req:Username>
                                 </req:Security>
                                     <add:CreditCard>
                                          <cre:CardExpiryMonth>{2}</cre:CardExpiryMonth>
                                          <cre:CardExpiryYear>{3}</cre:CardExpiryYear>
                                          <cre:ContactID>{5}</cre:ContactID>
                                          <cre:CreditCardID></cre:CreditCardID>
                                          <cre:CreditCardType>{6}</cre:CreditCardType>
                                          <cre:DateAdded>{8}</cre:DateAdded>
                                          <cre:DateModified>{8}</cre:DateModified>
                                          <cre:EncryptedCardNumber>{7}</cre:EncryptedCardNumber>
                                          <cre:IsPrimary>false</cre:IsPrimary>
                                          <cre:MaskedCardNumber></cre:MaskedCardNumber>
                                          <cre:NameOnCard>{4}</cre:NameOnCard>
                                          <cre:WebsiteID></cre:WebsiteID>
                                     </add:CreditCard>
                                     <add:Mode></add:Mode>
                            </def:Request>
                       </def:AddUpdateCreditCard>
                  </x:Body>
                  </x:Envelope>", Password, Username, CardExpiryMonth, CardExpiryYear, NameOnCard, ContactId, CreditCradType, CardNumber, DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss"));

            return SoapRequest;
        }

        private static string Vin65GetNotesForContactSoapRequest(string Password, string Username, string contactID)
        {
            string SoapRequest = string.Format(@"<x:Envelope
                                                xmlns:x=""http://schemas.xmlsoap.org/soap/envelope/""
                                                xmlns:def=""http://DefaultNamespace""
                                                xmlns:req=""http://request.base.V300""
                                                xmlns:sea=""http://searchNotes.note.V300"">
                                                <x:Header/>
                                                <x:Body>
                                                    <def:SearchNotes>
                                                        <def:Request>
                                                            <req:Security>
                                                                <req:Password>{1}</req:Password>
                                                                <req:Username>{2}</req:Username>
                                                            </req:Security>
                                                            <sea:DateModifiedFrom>2020-06-05T00:00:00</sea:DateModifiedFrom>
                                                            <sea:DateModifiedTo>{3}</sea:DateModifiedTo>
                                                            <sea:KeyCodeID>{0}</sea:KeyCodeID>
                                                            <sea:MaxRows>100</sea:MaxRows>
                                                            <sea:NoteID></sea:NoteID>
                                                            <sea:Page>1</sea:Page>
                                                            <sea:RelatedTo>Contact</sea:RelatedTo>
                                                            <sea:WebsiteIDs></sea:WebsiteIDs>
                                                        </def:Request>
                                                    </def:SearchNotes>
                                                </x:Body>
                                            </x:Envelope>", contactID, Password, Username, DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd") + "T00:00:00");

            return SoapRequest;
        }

        private static string Vin65AddUpdateNotesForContactSoapRequest(string password, string username, string noteId, string subject, string note, DateTime noteDate, string contactID)
        {
            string SoapRequest = string.Format(@"<x:Envelope
                                        xmlns:x=""http://schemas.xmlsoap.org/soap/envelope/""
                                        xmlns:def=""http://DefaultNamespace""
                                        xmlns:req=""http://request.base.V300""
                                        xmlns:add=""http://addUpdateNote.note.V300""
                                        xmlns:not=""http://_note.note.V300"">
                                        <x:Header/>
                                        <x:Body>
                                            <def:AddUpdateNote>
                                                <def:Request>
                                                    <req:Security>
                                                        <req:Password>{0}</req:Password>
                                                        <req:Username>{1}</req:Username>
                                                    </req:Security>
                                                    <add:Mode></add:Mode>
                                                    <add:Note>
                                                        <not:KeyCodeID>{6}</not:KeyCodeID>
                                                        <not:Note>{4}</not:Note>
                                                        <not:NoteDate>{5}</not:NoteDate>
                                                        <not:NoteID>{2}</not:NoteID>
                                                        <not:RelatedTo>Contact</not:RelatedTo>
                                                        <not:Subject>{3}</not:Subject>
                                                        <not:Type>Note</not:Type>
                                                        <not:WebsiteID></not:WebsiteID>
                                                    </add:Note>
                                                </def:Request>
                                            </def:AddUpdateNote>
                                        </x:Body>
                                    </x:Envelope>", password, username, noteId, subject.Length == 0 ? "Contact Account Note" : subject, note, noteDate.ToString("yyyy-MM-ddThh:mm:ss"), contactID);

            return SoapRequest;
        }

        private static string Vin65UpsertOrderSoapRequest(string Password, string Username, ReservationDetailModel rsvp)
        {
            string SoapRequest = string.Format(@"<?xml version=""1.0"" encoding=""UTF - 8""?><soapenv:Envelope xmlns:xsi = ""http://www.w3.org/2001/XMLSchema-instance"" xmlns: xsd = ""http://www.w3.org/2001/XMLSchema"" xmlns: soapenv = ""http://schemas.xmlsoap.org/soap/envelope/"">
               <soapenv:Header/>
                <soapenv:Body>
                   <UpsertOrder>
                     <Request>
                       <Security>
                         <Password>{0}</Password>
                         <Username>{1}</Username>
                       </Security>
                       <Orders>
                         <item>
                           <OrderNumber>{2}</OrderNumber>
                           <OrderDate>{3}</OrderDate>
                           <OrderType>{4}</OrderType>
                           <BillingEmail>{5}</BillingEmail>
                              <BillingFirstName>{6}</BillingFirstName>
                              <BillingLastName>{7}</BillingLastName>
                              <OrderItems>
                                <item>
                                  <ProductSKU>{8}</ProductSKU>
                                     <Quantity>{9}</Quantity>
                                     <Price>{10}</Price>
                                   </item>
                                 </OrderItems>
                                 <Tenders>
                                   <item>
                                     <PaymentType>{11}</ PaymentType>
                                     <AmountTendered>{12}</AmountTendered>
                                   </item>
                                 </Tenders>
                                 <SubTotal>{13}</SubTotal>
                                 <Total>{14}</Total>
                               </item>
                               <item>
                                 <OrderNumber>{15}</OrderNumber>
                                 <OrderType>{16}</OrderType>
                               </item>
                             </Orders>
                           </Request>
                         </UpsertOrder>
                       </soapenv:Body>
                        </soapenv:Envelope>", Password, Username);

            return SoapRequest;
        }

        //private static string Vin65UpsertOrderSoapRequest(string Password, string Username, ReservationDetailModel rsvp)
        //{
        //    string SoapRequest = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?><soapenv:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v201=""http://v201"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"">
        //       <soapenv:Header/>
        //       <soapenv:Body>
        //          <v201:UpsertContact soapenv:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
        //             <upsertContactRequest xsi:type=""ups:UpsertContactRequest"" xmlns:ups=""http://upsertContact.contact.v201"">
        //       <password xsi:type=""xsd:string"">{0}</password>
        //                <username xsi:type=""xsd:string"">{1}</username>
        //                <contacts xsi:type=""ups:UpsertContact"" soapenc:arrayType=""ups:UpsertContact[]"">
        //          <Address>{2}</Address>
        //          <Address2 xsi:nil=""true""/>
        //          <AltAccountNumber xsi:nil=""true""/>
        //          <AltContactID xsi:nil=""true""/>
        //          <Birthdate xsi:nil=""true""/>
        //          <CashierID xsi:nil=""true""/>
        //          <CellPhone xsi:nil=""true""/>
        //          <City>{3}</City>
        //          <Company xsi:nil=""true""/>
        //          <ContactID>{4}</ContactID>
        //          <CountryCode>{5}</CountryCode>
        //          <CreditCardExpirationMonth xsi:nil=""true""/>
        //          <CreditCardExpirationYear xsi:nil=""true""/>
        //          <CreditCardNameOnCard xsi:nil=""true""/>
        //          <CreditCardNumber xsi:nil=""true""/>
        //          <CreditCardType xsi:nil=""true""/>
        //          <Email>{6}</Email>
        //          <FacebookProfileID xsi:nil=""true""/>
        //          <Fax xsi:nil=""true""/>
        //          <Firstname>{7}</Firstname>
        //          <IsSingleOptIn xsi:nil=""true""/>
        //          <Lastname>{8}</Lastname>
        //          <MainPhone>{9}</MainPhone>
        //          <Password xsi:nil=""true""/>
        //          <PaymentTerms xsi:nil=""true""/>
        //          <PriceLevel xsi:nil=""true""/>
        //          <StateCode>{10}</StateCode>
        //          <Title xsi:nil=""true""/>
        //          <Username xsi:nil=""true""/>
        //          <WholesaleNumber xsi:nil=""true""/>
        //          <ZipCode>{11}</ZipCode>
        //          <isDirectToTrade xsi:nil=""true""/>
        //          <isNonTaxable xsi:nil=""true""/>
        //       </contacts>
        //             </upsertContactRequest>
        //          </v201:UpsertContact>
        //       </soapenv:Body>
        //    </soapenv:Envelope>", Password, Username, Address, City, ContactID, CountryCode, Email, Firstname, Lastname, MainPhone, StateCode, ZipCode);
        //    return SoapRequest;
        //}


        #endregion


        #region Order Port

        public async static Task<List<UserDetailModel>> GetCustomersByNameOrEmail(string keyword, string apiKey, string apiToken, string clientId)
        {
            List<UserDetailModel> userDetails = new List<UserDetailModel>();
            CustomersDetails modelList = null;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            if (!string.IsNullOrEmpty(keyword) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiToken) && !string.IsNullOrEmpty(clientId))
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                int memberId = eventDAL.GetWineryIdByOrderPortData(apiKey);

                try
                {
                    HttpClient httpClient = GetHttpClient(apiKey, apiToken, clientId);

                    string addlInfo = "httpClient.BaseAddress:-" + httpClient.BaseAddress.ToString();
                    addlInfo = addlInfo + ",httpClient.DefaultRequestHeaders:-" + httpClient.DefaultRequestHeaders.ToString();
                    addlInfo = addlInfo + ",httpClient.MaxResponseContentBufferSize:-" + httpClient.MaxResponseContentBufferSize;
                    addlInfo = addlInfo + ",httpClient.Timeout:-" + httpClient.Timeout.ToString();

                    logDAL.InsertLog("WebApi", "GetCustomersByNameOrEmail: " + addlInfo, "", 3, memberId);

                    string RequestUri = $"/api/customers/search/?lastname={keyword}";
                    if (keyword.IndexOf("@") > -1)
                    {
                        RequestUri = $"/api/customers/search/?email={keyword}";
                    }

                    logDAL.InsertLog("WebApi", "GetCustomersByNameOrEmail: RequestUri:" + RequestUri, "", 3, memberId);

                    //httpClient.ToString();
                    using (var response = httpClient.GetAsync(RequestUri).Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;

                        logDAL.InsertLog("WebApi", "GetCustomersByNameOrEmail: varResponse:" + varResponse, "", 3, memberId);

                        modelList = JsonConvert.DeserializeObject<CustomersDetails>(varResponse);
                    }

                    if (modelList != null)
                    {
                        if (modelList.Status == "Success")
                        {
                            foreach (var item in modelList.Data.Customers)
                            {
                                UserDetailModel userDetailModel = new UserDetailModel();
                                userDetailModel.membership_number = item.CustomerUuid;
                                userDetailModel.email = item.Email;
                                userDetailModel.first_name = item.FirstName;
                                userDetailModel.last_name = item.LastName;

                                userDetailModel.phone_number = Utility.FormatTelephoneNumber(item.Phone ?? "", item.Country);
                                userDetailModel.is_restricted = false;
                                //UserDetailModel userModel = await Task.Run(() => GetCustomerAcDetailsByEmailOrUuid(item.CustomerUuid, apiKey, apiToken, clientId));
                                //if (userModel != null)
                                //{
                                //    userDetailModel.contact_types = userModel.contact_types;
                                //    userDetailModel.member_status = userModel.member_status;
                                //    userDetailModel.customer_type = userModel.customer_type;
                                //}

                                Model.UserAddress addr = new Model.UserAddress();

                                addr.address_1 = item.Address1;
                                addr.address_2 = item.Address2;
                                addr.city = item.City;
                                addr.state = item.State;

                                addr.country = item.Country + "";

                                if (addr.country.ToLower() == "us")
                                {
                                    addr.zip_code = item.ZipCode + "";
                                    if (addr.zip_code.Length > 5)
                                        addr.zip_code = addr.zip_code.Substring(0, 5);
                                }
                                else
                                    addr.zip_code = item.ZipCode;

                                userDetailModel.address = addr;

                                userDetails.Add(userDetailModel);
                            }
                        }
                    }

                    httpClient.Dispose();
                }
                catch (Exception ex)
                {
                    string addlInfo = "ThirdPartyName: OrderPort, Orderport ClientId:" + clientId + ", api token :" + apiToken;
                    logDAL.InsertLog("WebApi", "GetCustomersByNameOrEmail: " + addlInfo + " Error: " + ex.ToString(), "", 1, memberId);
                }

                try
                {
                    foreach (var item in userDetails)
                    {
                        UserDetailModel userModel = await Task.Run(() => GetCustomerAcDetailsByEmailOrUuid(item.membership_number, apiKey, apiToken, clientId, memberId));
                        if (userModel != null)
                        {
                            item.contact_types = userModel.contact_types;
                            item.member_status = userModel.member_status;
                            item.customer_type = userModel.customer_type;
                        }
                    }
                }
                catch { }

            }

            return userDetails;
        }

        public async static Task<UserDetailModel> GetCustomerAcDetailsByEmailOrUuid(string emailOrUuid, string apiKey, string apiToken, string clientId, int memberId)
        {
            UserDetailModel userDetailModel = null;
            try
            {
                HttpClient httpClient = GetHttpClient(apiKey, apiToken, clientId);
                using (var response = httpClient.GetAsync($"/api/customers/{emailOrUuid}").Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    CustomerAcDetails custAcDetails = JsonConvert.DeserializeObject<CustomerAcDetails>(varResponse);

                    if (custAcDetails != null)
                    {
                        userDetailModel = new UserDetailModel();
                        userDetailModel.membership_number = custAcDetails.Data.CustomerUuid;
                        userDetailModel.first_name = custAcDetails.Data.BillingAddress.FirstName;
                        userDetailModel.last_name = custAcDetails.Data.BillingAddress.LastName;
                        userDetailModel.last_updated_date_time = custAcDetails.Data.LastUpdatedDateTime ?? DateTime.UtcNow;
                        userDetailModel.email = custAcDetails.Data.BillingAddress.Email;
                        userDetailModel.phone_number = custAcDetails.Data.BillingAddress.Phone;
                        userDetailModel.address.address_1 = custAcDetails.Data.BillingAddress.Address1;
                        userDetailModel.address.address_2 = custAcDetails.Data.BillingAddress.Address2;
                        userDetailModel.address.city = custAcDetails.Data.BillingAddress.City;
                        userDetailModel.address.state = custAcDetails.Data.BillingAddress.State;

                        userDetailModel.address.country = custAcDetails.Data.BillingAddress.Country + "";

                        if (userDetailModel.address.country.ToLower() == "us")
                        {
                            userDetailModel.address.zip_code = custAcDetails.Data.BillingAddress.ZipCode + "";
                            if (userDetailModel.address.zip_code.Length > 5)
                                userDetailModel.address.zip_code = userDetailModel.address.zip_code.Substring(0, 5);
                        }
                        else
                            userDetailModel.address.zip_code = custAcDetails.Data.BillingAddress.ZipCode;

                        List<string> listcontacttypes = new List<string>();

                        if (custAcDetails.Data.WineClubMemberships.Count > 0)
                        {
                            userDetailModel.customer_type = 1;
                            userDetailModel.member_status = true;

                            foreach (var wineClub in custAcDetails.Data.WineClubMemberships)
                            {
                                listcontacttypes.Add(wineClub.ClubName);
                            }
                        }

                        userDetailModel.contact_types = listcontacttypes;
                    }
                }
                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                string addlInfo = "ThirdPartyName: OrderPort, Orderport ClientId:" + clientId + ", api token :" + apiToken;
                logDAL.InsertLog("WebApi", "GetCustomerAcDetailsByEmailOrUuid: " + addlInfo + " ,Error: " + ex.ToString(), "", 1, memberId);
            }
            return userDetailModel;
        }

        public async static Task<CustomerClassesResponse> GetCustomerClassesForWinery(string apiKey, string apiToken, string clientId, int memberId)
        {
            CustomerClassesResponse customerClasses = null;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            try
            {
                HttpClient httpClient = GetHttpClient(apiKey, apiToken, clientId);
                string RequestUri = $"/api/customerclass/";
                //var httpResp = await httpClient.GetAsync(RequestUri).ConfigureAwait(false);
                using (var response = await httpClient.GetAsync(RequestUri).ConfigureAwait(false))
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    customerClasses = JsonConvert.DeserializeObject<CustomerClassesResponse>(varResponse);
                }
                httpClient.Dispose();
            }
            catch (Exception e)
            {
                logDAL.InsertLog("WebApi", "GetCustomerClassesForWinery:  " + e.ToString(), "", 1, memberId);
                string msg = e.Message;
            }
            return customerClasses;
        }

        public async static Task<string> UpsertCustomerDetails(PayloadModel payloadModel, string apiKey, string apiToken, string clientId, int memberId, bool IgnoreUserSync = false)
        {
            //return "";  //(request tict number /tickets/3604) (next tict 4152)

            if (IgnoreUserSync)
                return "";

            string custUuid = string.Empty;
            if (payloadModel != null && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiToken) && !string.IsNullOrEmpty(clientId))
            {

                string payLoadContent = JsonConvert.SerializeObject(payloadModel);
                HttpClient httpClient = GetHttpClient(apiKey, apiToken, clientId);
                var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");
                using (var response = httpClient.PostAsync($"/api/customers/upsert", stringContent).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    CustomerAcDetails custAcDetails = JsonConvert.DeserializeObject<CustomerAcDetails>(varResponse);

                    if (custAcDetails != null && custAcDetails.Data != null)
                    {
                        custUuid = custAcDetails.Data.CustomerUuid;
                    }

                    if (string.IsNullOrEmpty(custUuid))
                    {
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                        logDAL.InsertLog("UpsertCustomer", "OrderPort.UpsertCustomerDetails- memberId:" + memberId.ToString() + ", Request:" + payLoadContent + ", responce: " + varResponse, "", 1, memberId);
                    }
                }
                httpClient.Dispose();

            }
            return custUuid + "";
        }

        public async static Task<string> PushOrdersToOrderPort(PayloadOrderModel[] orders, string apiKey, string apiToken, string clientId, int rsvpId)
        {
            string BookingCode = string.Empty;
            PayloadOrderResponseModel payloadOrderResponse = null;
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            int member_id = eventDAL.GetWineryIdByReservationId(rsvpId);

            if (orders != null && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiToken) && !string.IsNullOrEmpty(clientId))
            {
                try
                {
                    string payLoadContent = JsonConvert.SerializeObject(orders);
                    HttpClient httpClient = GetHttpClient(apiKey, apiToken, clientId);
                    var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");

                    using (var response = httpClient.PostAsync($"/api/ordersfeed", stringContent).Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;
                        payloadOrderResponse = JsonConvert.DeserializeObject<PayloadOrderResponseModel>(varResponse);
                    }
                    httpClient.Dispose();

                    string DetailPrePend = string.Empty;

                    if (payloadOrderResponse.Status == "Fail")
                    {
                        string strError = string.Empty;
                        //foreach (var item in payloadOrderResponse.Messages)
                        //{
                        //    strError += item.Text + Environment.NewLine;
                        //    BookingCode = item.AdditionalInfo.AltOrderNumber;
                        //}

                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                        logDAL.InsertLog("ThirdParty", "OrderPort.SaveOrder- ReservationId: " + rsvpId.ToString() + ",Errors:" + strError, "", 1, member_id);
                        DetailPrePend = "Error- " + strError;
                        //DetailPrePend = Common.Common.Left(DetailPrePend.Trim(), 255);
                        eventDAL.InsertExportLog(5, rsvpId, DetailPrePend, 0, orders[0].Payment.Amount, "");
                    }
                    else
                    {
                        eventDAL.InsertExportLog(5, rsvpId, "Success- " + payloadOrderResponse.Data.BatchId, 1, orders[0].Payment.Amount, "");

                        BookingCode = payloadOrderResponse.Data.BatchId.ToString();
                        var reservation = eventDAL.GetReservationDetailsbyReservationId(rsvpId);
                        eventDAL.ReservationV2StatusNote_Create(rsvpId, reservation.status, reservation.member_id, "", false, 0, 0, 0, "SYNC - Order upserted to OrderPort");
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }
            return BookingCode;
        }


        public async static Task<string> PushOrdersToCommerce7(Commerce7OrderModel order, string username, string password, string tenant, int rsvpId, string processedBy = "", bool upsertFulfillment = false)
        {
            string BookingCode = string.Empty;
            //dynamic orderResponse = null;
            string extOrderId = "";
            string errorMessage = "";
            string errors = "";

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            if (eventDAL.AlreadyRsvpExported(rsvpId))
                return "";

            int member_id = eventDAL.GetWineryIdByReservationId(rsvpId);
            string fulfillmentDate = order.orderFulfilledDate;
            order.orderFulfilledDate = null;

            if (upsertFulfillment)
            {
                order.fulfillmentStatus = "Not Fulfilled";
            }

            if (order != null && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(tenant))
            {
                try
                {
                    string payLoadContent = JsonConvert.SerializeObject(order);

                    payLoadContent = payLoadContent.Replace("ordersource", "order-source");

                    HttpClient httpClient = GetCommerce7HttpClient(username, password, tenant);
                    var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");
                    string resp = string.Empty;

                    using (var response = httpClient.PostAsync($"/v1/order", stringContent).Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;
                        var orderResponse = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(varResponse);
                        if (varResponse.ToLower().Contains("id") && varResponse.ToLower().Contains("ordersubmitteddate"))
                        {

                            extOrderId = orderResponse.GetValue("id").ToString();
                        }
                        else
                        {
                            resp = varResponse;

                            if (varResponse.IndexOf("errors") > -1)
                                errors = orderResponse.GetValue("errors").ToString();

                            if (varResponse.IndexOf("message") > -1)
                                errorMessage = orderResponse.GetValue("message").ToString();
                        }

                    }
                    httpClient.Dispose();
                    decimal paidAmount = 0;

                    if (order.tenders != null && order.tenders.Count > 0)
                    {
                        paidAmount = decimal.Parse(order.tenders[0].amountTendered.ToString()) / 100;
                    }

                    string DetailPrePend = string.Empty;

                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                    var reservation = eventDAL.GetReservationDetailsbyReservationId(rsvpId);

                    if (string.IsNullOrWhiteSpace(extOrderId))
                    {
                        logDAL.InsertLog("WebApi", "Commerce7.SaveOrder- ReservationId: " + rsvpId + ", Request:" + payLoadContent, "", 1, member_id);
                        logDAL.InsertLog("WebApi", "Commerce7.SaveOrder- ReservationId: " + rsvpId + ", Response:" + resp, "", 1, member_id);
                        eventDAL.InsertExportLog(6, rsvpId, resp, 0, paidAmount, processedBy);

                        if (resp.IndexOf("Exceeded API Rate Limit") > -1)
                            BookingCode = "exceeded";

                        eventDAL.ReservationV2StatusNote_Create(rsvpId, reservation.status, reservation.member_id, "", false, 0, 0, 0, "Fail to Commerce7 sync Order");
                    }
                    else
                    {
                        logDAL.InsertLog("WebApi", "Commerce7.SaveOrder- ReservationId: " + rsvpId + ", Request:" + payLoadContent, "", 3, member_id);
                        logDAL.InsertLog("WebApi", "Commerce7.SaveOrder- ReservationId: " + rsvpId + ", Response:" + resp, "", 3, member_id);

                        BookingCode = extOrderId;
                        eventDAL.InsertExportLog(6, rsvpId, "Success- " + extOrderId, 1, paidAmount, processedBy);
                        
                        eventDAL.ReservationV2StatusNote_Create(rsvpId, reservation.status, reservation.member_id, "", false, 0, 0, 0, "SYNC - Order upserted to Commerce7");
                        if (upsertFulfillment)
                        {
                            if (order.billTo == null)
                            {
                                order.billTo = new AddressInfo
                                {
                                    firstName = reservation.user_detail.first_name,
                                    lastName = reservation.user_detail.last_name
                                };
                            }
                            order.orderFulfilledDate = fulfillmentDate;
                            await CreateFullfillmentCommerce7(order, extOrderId, member_id, username, password, tenant, rsvpId, reservation.status);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }
            return BookingCode;
        }

        public async static Task CreateFullfillmentCommerce7(Commerce7OrderModel order, string orderId, int member_id, string username, string password, string tenant,int rsvpId, byte reservationstatus)
        {
            string BookingCode = string.Empty;
            //dynamic orderResponse = null;
            string errorMessage = "";
            string errors = "";

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            if (order != null && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(tenant))
            {
                try
                {
                    var objFulfillReq = new
                    {
                        sendTransactionEmail = false,
                        type = "Picked Up",
                        fulfillmentDate = order.orderFulfilledDate,
                        pickedUp = new
                        {
                            pickedUpBy = order.billTo.firstName + " " + order.billTo.lastName
                        },
                        packageCount = 1

                    };
                    string payLoadContent = JsonConvert.SerializeObject(objFulfillReq);
                    logDAL.InsertLog("WebApi", "Commerce7.CreateFullfillmentCommerce7 Request: " + payLoadContent, "", 3, member_id);
                    HttpClient httpClient = GetCommerce7HttpClient(username, password, tenant);
                    var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");
                    string resp = string.Empty;
                    ///order/:id/fulfillment/all
                    string requestURL = string.Format("/v1/order/{0}/fulfillment/all", orderId);

                    using (var response = httpClient.PostAsync(requestURL, stringContent).Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;
                        logDAL.InsertLog("WebApi", "Commerce7.CreateFullfillmentCommerce7 Response: " + varResponse, "", 3, member_id);
                        
                        if (varResponse.IndexOf("errors") > -1)
                        {
                            eventDAL.ReservationV2StatusNote_Create(rsvpId, reservationstatus, member_id, "", false, 0, 0, 0, "Fulfillment attempt failed");

                            var orderResponse = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(varResponse);
                            errors = orderResponse.GetValue("errors").ToString();

                            if (varResponse.IndexOf("message") > -1)
                                errorMessage = orderResponse.GetValue("message").ToString();
                        }
                        else
                        {
                            eventDAL.ReservationV2StatusNote_Create(rsvpId, reservationstatus, member_id, "", false, 0, 0, 0, "Marked as Fulfilled");
                        }

                    }
                    httpClient.Dispose();

                }
                catch (Exception ex)
                {

                    logDAL.InsertLog("WebApi", "Commerce7.CreateFullfillmentCommerce7 Error: " + ex.Message, "", 1, member_id);
                }
            }
        }

        public async static Task<OrderFeedsStatusResponse> GetOrderFeedsStatus(string apiKey, string apiToken, string clientId, long batchId, string altOrderNumber = null)
        {
            OrderFeedsStatusResponse orderFeedsStatus = null;
            if (batchId != 0 && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiToken) && !string.IsNullOrEmpty(clientId))
            {
                try
                {
                    HttpClient httpClient = GetHttpClient(apiKey, apiToken, clientId);
                    using (var response = httpClient.GetAsync($"/api/ordersfeed/{batchId}/{altOrderNumber}").Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;
                        orderFeedsStatus = JsonConvert.DeserializeObject<OrderFeedsStatusResponse>(varResponse);
                    }
                    httpClient.Dispose();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }
            return orderFeedsStatus;
        }

        public async static void GetOrderStatusByAltOrderNumber(string apiKey, string apiToken, string clientId, string altOrderNumber)
        {
            if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiToken) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(altOrderNumber))
            {
                try
                {
                    HttpClient httpClient = GetHttpClient(apiKey, apiToken, clientId);
                    using (var response = httpClient.GetAsync($"/api/feed-status/{altOrderNumber}").Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;
                        //modelList = JsonConvert.DeserializeObject<RootObject>(varResponse);
                    }
                    httpClient.Dispose();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }
        }

        public static string MakePostRequest(string url, object param)
        {
            string jsonResp = string.Empty;

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string requestObj = JsonConvert.SerializeObject(param);


                using (var content = new StringContent(requestObj, Encoding.Unicode, "application/json"))
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    var response = client.PostAsync(new Uri(url), content).Result;

                    if (response.IsSuccessStatusCode)
                        jsonResp = response.Content.ReadAsStringAsync().Result;
                }
            }

            return jsonResp;
        }

        public static string GetCardTypeFromNumber(string cardNumber)
        {
            //https://www.regular-expressions.info/creditcard.html
            if (Regex.Match(cardNumber, @"^4[0-9]{12}(?:[0-9]{3})?$").Success)
            {
                return "001"; // visa;
            }

            if (Regex.Match(cardNumber, @"^(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}$").Success)
            {
                return "002";//MasterCard;
            }

            if (Regex.Match(cardNumber, @"^3[47][0-9]{13}$").Success)
            {
                return "003 "; // AmericanExpress;
            }

            if (Regex.Match(cardNumber, @"^6(?:011|5[0-9]{2})[0-9]{12}$").Success)
            {
                return "004"; // Discover;
            }
            if (Regex.Match(cardNumber, @"^3(?:0[0-5]|[68][0-9])[0-9]{11}$").Success)
            {
                return "005"; // Diners;
            }

            //^3(?:0[0-5]|[68][0-9])[0-9]{4,}$

            if (Regex.Match(cardNumber, @"^(?:2131|1800|35\d{3})\d{11}$").Success)
            {
                return "007"; // JCB;
            }

            throw new Exception("Unknown card.");
        }

        public static HttpClient GetHttpClient(string apiKey, string apiToken, string clientId)
        {
            var handler = new HttpClientHandler();
            handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls;
            HttpClient httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri("https://wwwapps.orderport.net");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
             new MediaTypeWithQualityHeaderValue("application/json"));

            var auth = new
            {
                ApiKey = apiKey,
                ApiToken = apiToken,
                ClientId = clientId
            };

            var authParam = JsonConvert.SerializeObject(auth);
            authParam = Convert.ToBase64String(Encoding.UTF8.GetBytes(authParam));
            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authParam);
            httpClient.DefaultRequestHeaders.Add("api-version", "1.0.0");
            //httpClient.Timeout = TimeSpan.FromMinutes(3);
            return httpClient;
        }

        #endregion

        #region Commerce7

        public async static Task<bool> CheckCommerce7LoginTest(string Commerce7Username, string Commerce7Password, string Commerce7Tenant)
        {
            bool ret = false;
            try
            {
                Customers modelList = null;
                string RequestUri = $"/v1/customer?q=LoginTest";
                HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                using (var response = httpClient.GetAsync(RequestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    modelList = JsonConvert.DeserializeObject<Customers>(varResponse);

                    if (modelList.customers == null)
                    {
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                        logDAL.InsertLog("WebApi", "CheckCommerce7LoginTest: RequestURL:" + "https://api.commerce7.com" + RequestUri + ",DefaultRequestHeaders:" + httpClient.DefaultRequestHeaders.ToString() + ",Response:" + varResponse.ToString(), "", 1, 0);
                    }
                    else
                        ret = true;
                }

                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return ret;
        }

        public async static Task<List<UserDetailModel>> GetCommerce7CustomersByNameOrEmail(string keyword, string Commerce7Username, string Commerce7Password, string Commerce7Tenant, int member_id)
        {
            List<UserDetailModel> userDetails = new List<UserDetailModel>();
            Customers modelList = null;
            try
            {
                string RequestUri = $"/v1/customer?q={keyword}";
                HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                using (var response = httpClient.GetAsync(RequestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    modelList = JsonConvert.DeserializeObject<Customers>(varResponse);

                    if (modelList.customers == null)
                    {
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                        logDAL.InsertLog("WebApi", "GetCommerce7CustomersByNameOrEmail: RequestURL:" + "https://api.commerce7.com" + RequestUri + ",DefaultRequestHeaders:" + httpClient.DefaultRequestHeaders.ToString() + ",Response:" + varResponse.ToString(), "", 1, member_id);
                    }
                }

                if (modelList.total > 0)
                {
                    foreach (var item in modelList.customers)
                    {
                        bool ValidEmail = false;

                        if (keyword.IndexOf("@") > -1)
                        {
                            foreach (var e in item.emails)
                            {
                                if (e.email.ToLower() == keyword.ToLower())
                                    ValidEmail = true;
                            }
                        }
                        else
                            ValidEmail = true;

                        if (ValidEmail)
                        {
                            UserDetailModel userDetailModel = new UserDetailModel();
                            Model.UserAddress addr = new Model.UserAddress();
                            UserDetailModel model = new UserDetailModel();
                            model = await GetCommerce7CustomerAddress(item.id, Commerce7Username, Commerce7Password, Commerce7Tenant, member_id, item.firstName,item.lastName);

                            NoteList noteList = new NoteList();
                            AccountNote notemodel = new AccountNote();
                            notemodel.modified_by = "";
                            notemodel.note = "";
                            noteList = await GetCommerce7NotesById(item.id, Commerce7Username, Commerce7Password, Commerce7Tenant, member_id);

                            string strNote = string.Empty;
                            if (noteList != null && noteList.total > 0)
                            {
                                foreach (var note in noteList.notes)
                                {
                                    string val = strNote.Trim().Length > 0 ? ", " : "";
                                    strNote = strNote + val + note.content;
                                    notemodel.note_date = note.updatedAt;
                                }
                                notemodel.note = strNote;
                            }

                            userDetailModel.account_note = notemodel;

                            userDetailModel.ltv = Convert.ToDecimal(item.orderInformation.lifetimeValue) / 100;

                            userDetailModel.order_count = item.orderInformation.orderCount;
                            userDetailModel.last_order_date = item.orderInformation.lastOrderDate;

                            if (model != null && model.color != null && model.color.Length > 0)
                            {
                                userDetailModel.first_name = model.first_name;
                                userDetailModel.last_name = model.last_name;

                                addr.address_1 = model.address.address_1;
                                addr.address_2 = model.address.address_2;

                                addr.city = model.address.city;
                                addr.state = model.address.state;

                                addr.country = model.address.country + "";

                                if (addr.country.ToLower() == "us")
                                {
                                    addr.zip_code = item.zipCode + "";
                                    if (addr.zip_code.Length > 5)
                                        addr.zip_code = addr.zip_code.Substring(0, 5);
                                }
                                else
                                    addr.zip_code = item.zipCode;
                            }
                            else
                            {
                                userDetailModel.first_name = item.firstName;
                                userDetailModel.last_name = item.lastName;

                                addr.address_1 = "";
                                addr.address_2 = "";

                                addr.city = item.city;
                                addr.state = item.stateCode;

                                addr.country = item.countryCode + "";

                                if (addr.country.ToLower() == "us")
                                {
                                    addr.zip_code = item.zipCode + "";
                                    if (addr.zip_code.Length > 5)
                                        addr.zip_code = addr.zip_code.Substring(0, 5);
                                }
                                else
                                    addr.zip_code = item.zipCode;
                            }

                            if (item.phones.Count > 0)
                            {
                                userDetailModel.phone_number = FormatTelephoneNumber(item.phones[0].phone, item.countryCode);
                            }

                            userDetailModel.address = addr;

                            userDetailModel.membership_number = item.id;
                            userDetailModel.email = item.emails[0].email;
                            
                            userDetailModel.last_updated_date_time = item.lastActivityDate;
                            userDetailModel.is_restricted = false;

                            List<string> listcontacttypes = new List<string>();
                            List<string> listcontacttypesId = new List<string>();

                            if (item.clubs.Count > 0)
                            {
                                //get all active clubs
                                var activeClubs = item.clubs.Where(c => c.cancelDate == null).ToList();
                                if (activeClubs != null && activeClubs.Count > 0)
                                {
                                    userDetailModel.customer_type = 1;
                                    userDetailModel.member_status = true;

                                    foreach (var wineClub in activeClubs)
                                    {
                                        listcontacttypes.Add(wineClub.clubTitle);
                                        listcontacttypesId.Add(wineClub.clubId);
                                    }
                                }
                            }

                            if (item.tags.Count > 0)
                            {
                                //userDetailModel.customer_type = 1;
                                //userDetailModel.member_status = true;

                                foreach (var wineClub in item.tags)
                                {
                                    listcontacttypes.Add(wineClub.title);
                                    listcontacttypesId.Add(wineClub.id);
                                }
                            }

                            if (item.groups.Count > 0)
                            {
                                //userDetailModel.customer_type = 1;
                                //userDetailModel.member_status = true;

                                foreach (var wineClub in item.groups)
                                {
                                    listcontacttypes.Add(wineClub.title);
                                    listcontacttypesId.Add(wineClub.id);
                                }
                            }

                            if (item.groupIds != null && item.groupIds.Count > 0)
                            {
                                foreach (var group in item.groupIds)
                                {
                                    string groupName = string.Empty;
                                    string groupId = string.Empty;
                                    Model.Group groupData = await GetCommerce7GroupById(group.ToString(), Commerce7Username, Commerce7Password, Commerce7Tenant, member_id);

                                    if (groupData != null && groupData.id != null)
                                    {
                                        groupName = groupData.title;
                                        groupId = groupData.id;
                                    }

                                    listcontacttypes.Add(groupName);
                                    listcontacttypesId.Add(groupId);
                                }
                            }

                            userDetailModel.contact_types = listcontacttypes.Distinct().ToList();
                            userDetailModel.contact_types_id = listcontacttypesId.Distinct().ToList();

                            userDetails.Add(userDetailModel);
                        }
                    }
                }

                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetCommerce7CustomersByNameOrEmail:  Email:" + keyword + ",Message:" + ex.Message.ToString(), "", 1, member_id);
            }

            return userDetails;
        }

        public async static Task<string> GetCommerce7CustomerIdByNameOrEmail(string keyword, string Commerce7Username, string Commerce7Password, string Commerce7Tenant, int member_id)
        {
            string membership_number = string.Empty;
            Customers modelList = null;

            try
            {
                string RequestUri = $"/v1/customer?q={keyword}";
                HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                using (var response = httpClient.GetAsync(RequestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    modelList = JsonConvert.DeserializeObject<Customers>(varResponse);

                    if (modelList.customers == null)
                    {
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                        logDAL.InsertLog("WebApi", "GetCommerce7CustomerIdByNameOrEmail:  RequestURL:" + "https://api.commerce7.com" + RequestUri + ",DefaultRequestHeaders:" + httpClient.DefaultRequestHeaders.ToString() + ",Response:" + varResponse.ToString(), "", 1, member_id);
                    }
                }

                if (modelList.total > 0)
                {
                    foreach (var item in modelList.customers)
                    {
                        bool ValidEmail = false;

                        if (keyword.IndexOf("@") > -1)
                        {
                            foreach (var e in item.emails)
                            {
                                if (e.email.ToLower() == keyword.ToLower())
                                    ValidEmail = true;
                            }
                        }
                        else
                            ValidEmail = true;

                        if (ValidEmail)
                        {
                            membership_number = item.id;
                        }
                    }
                }

                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetCommerce7CustomerIdByNameOrEmail:  Email:" + keyword + ",Message:" + ex.Message.ToString(), "", 1, member_id);
            }

            return membership_number;
        }

        public async static Task<UserDetailModel> GetCommerce7CustomerAddress(string Id, string Commerce7Username, string Commerce7Password, string Commerce7Tenant, int member_id, string firstname, string lastname)
        {
            UserDetailModel userDetailModel = new UserDetailModel();
            CustomerAddressList modelList = null;
            try
            {
                string RequestUri = $"/v1/customer/{Id}/address";
                HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                using (var response = httpClient.GetAsync(RequestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    modelList = JsonConvert.DeserializeObject<CustomerAddressList>(varResponse);

                    if (modelList.customerAddresses == null)
                    {
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                        logDAL.InsertLog("WebApi", "GetCommerce7CustomerAddress:  RequestURL:" + "https://api.commerce7.com" + RequestUri + ",DefaultRequestHeaders:" + httpClient.DefaultRequestHeaders.ToString() + ",Response:" + varResponse.ToString(), "", 1, member_id);
                    }
                }

                if (modelList.total > 0)
                {
                    foreach (var item in modelList.customerAddresses)
                    {
                        
                        userDetailModel.phone_number = FormatTelephoneNumber(item.phone, item.countryCode);

                        userDetailModel.first_name = item.firstName;
                        userDetailModel.last_name = item.lastName;

                        Model.UserAddress addr = new Model.UserAddress();
                        userDetailModel.color = item.id;
                        addr.address_1 = item.address;
                        addr.address_2 = item.address2;
                        addr.city = item.city;
                        addr.state = item.stateCode;

                        addr.country = item.countryCode + "";

                        if (addr.country.ToLower() == "us")
                        {
                            addr.zip_code = item.zipCode + "";
                            if (addr.zip_code.Length > 5)
                                addr.zip_code = addr.zip_code.Substring(0, 5);
                        }
                        else
                            addr.zip_code = item.zipCode;

                        userDetailModel.address = addr;

                        if (item.isDefault)
                        {
                            break;
                        }
                    }
                }

                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetCommerce7CustomerAddress:  Id:" + Id + ",Message:" + ex.Message.ToString(), "", 1, member_id);
            }

            return userDetailModel;
        }

        public async static Task<List<string>> GetCommerce7OrderSource(string Commerce7Username, string Commerce7Password, string Commerce7Tenant, int member_id)
        {
            List<string> orderSourcelist = new List<string>();
            MetaDataConfigModel modelList = null;
            try
            {
                string RequestUri = $"/v1/meta-data-config/order";
                HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                using (var response = httpClient.GetAsync(RequestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    modelList = JsonConvert.DeserializeObject<MetaDataConfigModel>(varResponse);

                    if (modelList.metaDataConfigs == null)
                    {
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                        logDAL.InsertLog("WebApi", "GetCommerce7OrderSource: RequestURL:" + "https://api.commerce7.com" + RequestUri + ",DefaultRequestHeaders:" + httpClient.DefaultRequestHeaders.ToString() + ",Response:" + varResponse.ToString(), "", 1, member_id);
                    }
                }

                if (modelList.total > 0)
                {
                    foreach (var item in modelList.metaDataConfigs)
                    {
                        if (item.isRequired && item.options.Count > 0)
                        {
                            orderSourcelist.AddRange(item.options);
                        }
                    }
                }

                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetCommerce7OrderSource: Message:" + ex.Message.ToString(), "", 1, member_id);
            }

            return orderSourcelist;
        }

        public async static Task<Commerce7CustomerModel> CreateCommerce7Customer(CreateCustomer model, string Commerce7Username, string Commerce7Password, string Commerce7Tenant, int memberId,int rsvpId)
        {
            Commerce7CustomerModel custUuid = new Commerce7CustomerModel();
            string responce = string.Empty;
            if (model != null && !string.IsNullOrEmpty(Commerce7Username) && !string.IsNullOrEmpty(Commerce7Password) && !string.IsNullOrEmpty(Commerce7Tenant))
            {
                try
                {
                    string content = JsonConvert.SerializeObject(model);
                    HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    using (var response = httpClient.PostAsync($"/v1/customer", stringContent).Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;
                        responce = varResponse.ToString();
                        CustomerModel custAcDetails = JsonConvert.DeserializeObject<CustomerModel>(varResponse);

                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                        if (custAcDetails != null && custAcDetails.id != null && custAcDetails.id.Length > 0)
                        {
                            custUuid.CustId = custAcDetails.id;

                            logDAL.InsertLog("CreateCommerce7Customer", "Email: " + model.emails[0].email + ",Responce:" + responce + ",Request:" + content, "", 3, memberId);
                        }
                        else
                        {
                            if (rsvpId > 0)
                            {
                                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                                eventDAL.InsertExportLog(6, rsvpId, responce, 0, 0, "");
                            }

                            logDAL.InsertLog("CreateCommerce7Customer", "Email: " + model.emails[0].email + ",Responce:" + responce + ",Request:" + content, "", 1, memberId);

                            if (responce.IndexOf("Exceeded API Rate Limit") > -1)
                            {
                                custUuid.Exceeded = true;
                            }
                        }
                    }
                    httpClient.Dispose();
                }
                catch (Exception ex)
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("CreateCommerce7Customer", "Email: " + model.emails[0].email + ",Error:" + ex.Message, "", 1, memberId);

                    if (rsvpId > 0)
                    {
                        EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                        eventDAL.InsertExportLog(6, rsvpId, "Email: " + model.emails[0].email + ",Error:" + ex.Message, 0, 0, "");
                    }
                }
            }
            return custUuid;
        }

        public async static Task<TokenizedCard> TokenizeAndAddCommerce7Card(IOptions<AppSettings> settings, TokenizedCardRequest request, Configuration pcfg)
        {
            TokenizedCard cardResponse = new TokenizedCard();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            string firstname = "", lastname = "", email = "", zipcode = "", address1 = "", phoneNum = "", country = "", state = "", address2 = "", city = "";

            if (request.user_info != null)
            {
                firstname = request.user_info.first_name;
                lastname = request.user_info.last_name;
                email = request.user_info.email;
                if (request.user_info.address != null)
                {
                    zipcode = request.user_info.address.zip_code;
                    address1 = request.user_info.address.address_1;
                    state = request.user_info.address.state + "";
                    country = request.user_info.address.country + "";
                    address2 = request.user_info.address.address_2 + "";
                    city = request.user_info.address.city + "";
                }
                phoneNum = Convert.ToString(request.user_info.phone_number + "");


            }
            else if (request.user_id > 0)
            {
                UserDAL dal = new UserDAL(Common.Common.ConnectionString);
                var userObj = dal.GetUserById(request.user_id);
                firstname = userObj.first_name;
                lastname = userObj.last_name;
                email = userObj.email;
                if (userObj.address != null)
                {
                    zipcode = userObj.address.zip_code;
                    address1 = userObj.address.address_1;
                    state = request.user_info.address.state + "";
                    country = request.user_info.address.country + "";
                    address2 = request.user_info.address.address_2 + "";
                    city = request.user_info.address.city + "";
                }
                phoneNum = Convert.ToString(request.user_info.phone_number + "");
            }
            else
            {
                Common.StringHelpers.ParseCustomerName(request.cust_name, ref firstname, ref lastname);
                email = firstname + "." + lastname + "@noemail.com";
            }

            if (settings.Value.PaymentTestMode)
            {
                logDAL.InsertLog("Commerce7::TokenizedCard", "Request Rcvd:" + JsonConvert.SerializeObject(request), "WebApi", 3, request.member_id);
            }

            if ((email + "").ToLower().Contains("@noemail.com"))
            {
                cardResponse = new TokenizedCard
                {
                    ErrorMessage = "Invalid customer email. Cannot proceed."
                };

            }
            else
            {
                try
                {

                    //check and create customer if not already exists
                    string c7UserName = pcfg.MerchantLogin;
                    string c7Pwd = pcfg.MerchantPassword;
                    string c7Tenant = pcfg.UserConfig1;

                    var customer = await CheckAndUpdateCommerce7Customer(c7UserName, c7Pwd, c7Tenant, firstname, lastname, "", address1, address2, city, state, zipcode, country, email, phoneNum, "", request.member_id);

                    if (customer != null && !string.IsNullOrWhiteSpace(customer.CustId))
                    {
                        //now create card and attach it
                        CreateCreditCard model1 = new CreateCreditCard
                        {
                            cardNumber = request.number,
                            expiryMo = int.Parse(request.exp_month),
                            expiryYr = int.Parse(request.exp_year),
                            cardHolderName = request.cust_name,
                            cvv2 = request.cvv2,

                        };
                        var cardToken = await CreateCommerce7CreditCard(model1, c7UserName, c7Pwd, c7Tenant, customer.CustId, request.member_id);
                        if (!string.IsNullOrWhiteSpace(cardToken))
                        {
                            cardResponse.card_token = cardToken;
                            cardResponse.last_four_digits = Common.Common.Right(request.number.ToString(), 4);
                            cardResponse.first_four_digits = Common.Common.Left(request.number.ToString(), 4);
                            cardResponse.customer_name = request.cust_name;
                            cardResponse.is_expired = false;
                            cardResponse.card_exp_month = request.exp_month;
                            cardResponse.card_exp_year = request.exp_year;
                        }
                        else
                        {
                            cardResponse.ErrorMessage = "Could not tokenize card";
                        }
                    }
                    else
                    {
                        cardResponse.ErrorMessage = "Could not create customer. Cannot tokenize card";
                    }
                }
                catch (Exception ex)
                {
                    cardResponse.ErrorMessage = ex.Message;
                    logDAL.InsertLog("WebApi", "TokenizeCard:  " + ex.Message.ToString(), "Web Api", 1, request.member_id);

                }

            }
            return cardResponse;
        }

        public async static Task<string> CreateCommerce7CreditCard(CreateCreditCard model, string Commerce7Username, string Commerce7Password, string Commerce7Tenant, string custId, int memberId)
        {
            string CCId = string.Empty;
            string responce = string.Empty;

            if (model != null && !string.IsNullOrEmpty(Commerce7Username) && !string.IsNullOrEmpty(Commerce7Password) && !string.IsNullOrEmpty(Commerce7Tenant))
            {
                string content = JsonConvert.SerializeObject(model);

                try
                {
                    List<CustomerCreditCard> customerCreditCards = await Utility.GetCommerce7CreditCardByCustId(Commerce7Username, Commerce7Password, Commerce7Tenant, custId, memberId);
                    string gatewayName = model.gateway;
                    if (string.IsNullOrWhiteSpace(gatewayName))
                    {
                        customerCreditCards = customerCreditCards.Where(c => string.IsNullOrWhiteSpace(c.gateway) || c.gateway == "Commerce7Payments").ToList();
                    }
                    else
                    {
                        customerCreditCards = customerCreditCards.Where(c => c.gateway == gatewayName).ToList();
                    }
                    foreach (var item in customerCreditCards)
                    {
                        if (item.expiryMo == model.expiryMo && item.expiryYr == model.expiryYr && item.maskedCardNumber == model.maskedCardNumber && item.cardHolderName == model.cardHolderName)
                        {
                            return item.id;
                        }
                    }

                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    logDAL.InsertLog("CreateCommerce7CreditCard", "Request:" + content + ",Request URL:" + $"/v1/customer/{custId}/credit-card", "", 3, memberId);
                    using (var response = httpClient.PostAsync($"/v1/customer/{custId}/credit-card", stringContent).Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;
                        responce = varResponse.ToString();
                        logDAL.InsertLog("CreateCommerce7CreditCard", "Response:" + responce, "", 3, memberId);
                        CreateCreditCardResponce custAcDetails = JsonConvert.DeserializeObject<CreateCreditCardResponce>(varResponse);

                        if (custAcDetails != null && custAcDetails.id != null && custAcDetails.id.Length > 0)
                        {
                            CCId = custAcDetails.id;
                        }
                    }
                    httpClient.Dispose();
                }
                catch (Exception ex)
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("CreateCommerce7CreditCard", "Request: " + content + ",Error:" + ex.Message, "", 1, memberId);
                }
            }
            return CCId;
        }

        public async static Task<List<CustomerCreditCard>> GetCommerce7CreditCardByCustId(string Commerce7Username, string Commerce7Password, string Commerce7Tenant, string custId, int memberId)
        {
            List<CustomerCreditCard> customerCreditCards = new List<CustomerCreditCard>();
            string Commerce7responce = string.Empty;
            string RequestURL = string.Empty;

            if (!string.IsNullOrEmpty(Commerce7Username) && !string.IsNullOrEmpty(Commerce7Password) && !string.IsNullOrEmpty(Commerce7Tenant))
            {
                try
                {
                    HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                    //logDAL.InsertLog("GetCommerce7CreditCardByCustId", "Request URL:" + $"/v1/customer/{custId}/credit-card", "", 3, memberId);
                    RequestURL = "Request URL:" + $"/v1/customer/{custId}/credit-card";

                    using (var response = httpClient.GetAsync($"/v1/customer/{custId}/credit-card").Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;
                        Commerce7responce = varResponse.ToString();
                        //logDAL.InsertLog("GetCommerce7CreditCardByCustId", "Response:" + responce, "", 3, memberId);
                        CustomerCreditCardModel ccList = JsonConvert.DeserializeObject<CustomerCreditCardModel>(varResponse);

                        if (ccList != null && ccList.total > 0)
                        {
                            customerCreditCards = ccList.customerCreditCards;
                        }
                    }
                    httpClient.Dispose();
                }
                catch (Exception ex)
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("GetCommerce7CreditCardByCustId", "Request URL:" + $"/v1/customer/{custId}/credit-card", "", 3, memberId);
                    logDAL.InsertLog("GetCommerce7CreditCardByCustId", "Response:" + Commerce7responce, "", 3, memberId);
                    logDAL.InsertLog("GetCommerce7CreditCardByCustId", "Error:" + ex.Message, "", 1, memberId);
                }
            }
            return customerCreditCards;
        }

        public static string GetCommerce7PaymentGatewayName(Configuration.Gateway paymentGateway)
        {
            string gateway = "";
            switch (paymentGateway)
            {
                case Configuration.Gateway.USAePay:
                    gateway = "USAePay";
                    break;
                case Configuration.Gateway.AuthorizeNet:
                    gateway = "Authorize.Net";
                    break;
                case Configuration.Gateway.Cybersource:
                    gateway = "CyberSource";
                    break;
                case Configuration.Gateway.Stripe:
                    gateway = "Stripe";
                    break;
                case Configuration.Gateway.CardConnect:
                    gateway = "CardConnect";
                    break;
                case Configuration.Gateway.Zeamster:
                    gateway = "Zeamster";
                    break;
                case Configuration.Gateway.Commrece7Payments:
                    gateway = "Commerce7Payments";
                    break;
                //case Configuration.Gateway.PayFlowPro:
                //    gateway = "PayFlowPro";
                //    break;
                //case Configuration.Gateway.CenPos:
                //    gateway = "CenPos";
                //    break;
                //case Configuration.Gateway.Braintree:
                //    gateway = "Braintree";
                //    break;
                //case Configuration.Gateway.WorldPayXML:
                //    gateway = "WorldPayXML";
                //    break;
                //case Configuration.Gateway.OpenEdge:
                //    gateway = "OpenEdge";
                //    break;
                //case Configuration.Gateway.Shift4:
                //    gateway = "Shift4";
                //    break;
                default:
                    gateway = "No Gateway";
                    break;
            }
            return gateway;
        }

        public async static Task<Commerce7CustomerModel> CreateCommerce7CustomerAddresses(AddressInfo model, string Commerce7Username, string Commerce7Password, string Commerce7Tenant, string custId, int member_id)
        {
            Commerce7CustomerModel custUuid = new Commerce7CustomerModel();
            string responce = string.Empty;
            if (model != null && !string.IsNullOrEmpty(Commerce7Username) && !string.IsNullOrEmpty(Commerce7Password) && !string.IsNullOrEmpty(Commerce7Tenant))
            {
                try
                {
                    string content = JsonConvert.SerializeObject(model);
                    HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    using (var response = httpClient.PostAsync($"/v1/customer/{custId}/address", stringContent).Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;
                        responce = varResponse.ToString();
                        CustomerAddress custAcDetails = JsonConvert.DeserializeObject<CustomerAddress>(varResponse);

                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                        if (custAcDetails != null && custAcDetails.id != null && custAcDetails.id.Length > 0)
                        {
                            custUuid.CustId = custAcDetails.id;

                            logDAL.InsertLog("CreateCommerce7CustomerAddresses", "custId: " + custId + ",Errors:" + responce + ",Request:" + content, "", 3, member_id);
                        }
                        else
                        {

                            logDAL.InsertLog("CreateCommerce7CustomerAddresses", "custId: " + custId + ",Errors:" + responce + ",Request:" + content, "", 1, member_id);

                            if (responce.IndexOf("Exceeded API Rate Limit") > -1)
                            {
                                custUuid.Exceeded = true;
                            }
                        }
                    }
                    httpClient.Dispose();
                }
                catch (Exception ex)
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("CreateCommerce7CustomerAddresses", "custId: " + custId + ",Errors:" + responce, "", 1, member_id);
                }
            }
            return custUuid;
        }

        public async static Task<Commerce7CustomerModel> UpdateCommerce7CustomerAddresses(AddressInfo model, string Commerce7Username, string Commerce7Password, string Commerce7Tenant, string custId, string addressId, int member_id)
        {
            Commerce7CustomerModel custUuid = new Commerce7CustomerModel();
            string responce = string.Empty;
            if (model != null && !string.IsNullOrEmpty(Commerce7Username) && !string.IsNullOrEmpty(Commerce7Password) && !string.IsNullOrEmpty(Commerce7Tenant))
            {
                try
                {
                    string content = JsonConvert.SerializeObject(model);
                    HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    using (var response = httpClient.PutAsync($"/v1/customer/{custId}/address/{addressId}", stringContent).Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;
                        responce = varResponse.ToString();
                        CustomerAddress custAcDetails = JsonConvert.DeserializeObject<CustomerAddress>(varResponse);

                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                        if (custAcDetails != null && custAcDetails.id != null && custAcDetails.id.Length > 0)
                        {
                            custUuid.CustId = custAcDetails.id;

                            logDAL.InsertLog("UpdateCommerce7CustomerAddresses", "custId: " + custId + ",Errors:" + responce + ",Request:" + content, "", 3, member_id);
                        }
                        else
                        {

                            logDAL.InsertLog("UpdateCommerce7CustomerAddresses", "custId: " + custId + ",Errors:" + responce + ",Request:" + content, "", 1, member_id);

                            if (responce.IndexOf("Exceeded API Rate Limit") > -1)
                            {
                                custUuid.Exceeded = true;
                            }
                        }
                    }
                    httpClient.Dispose();
                }
                catch (Exception ex)
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("UpdateCommerce7CustomerAddresses", "custId: " + custId + ",Errors:" + responce, "", 1, member_id);
                }
            }
            return custUuid;
        }

        public async static Task<Commerce7CustomerModel> UpdateCommerce7Customer(CreateCustomer model, string Commerce7Username, string Commerce7Password, string Commerce7Tenant, string custId, int memberId)
        {
            Commerce7CustomerModel custUuid = new Commerce7CustomerModel();
            string responce = string.Empty;
            string addlInfo = "ThirdPartyName: Commerce7,  Commerce7Username:" + Commerce7Username + ", Commerce7Tenant :" + Commerce7Tenant;
            if (model != null && !string.IsNullOrEmpty(Commerce7Username) && !string.IsNullOrEmpty(Commerce7Password) && !string.IsNullOrEmpty(Commerce7Tenant))
            {
                try
                {
                    string content = JsonConvert.SerializeObject(model);
                    HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    using (var response = httpClient.PutAsync($"/v1/customer/{custId}", stringContent).Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;
                        responce = varResponse.ToString();
                        CustomerModel custAcDetails = JsonConvert.DeserializeObject<CustomerModel>(varResponse);

                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                        if (custAcDetails != null && custAcDetails.id != null && custAcDetails.id.Length > 0)
                        {
                            custUuid.CustId = custAcDetails.id;
                            logDAL.InsertLog("UpdateCommerce7Customer", addlInfo + " Email: " + model.emails[0].email + ",Responce:" + responce + ",Request:" + content, "", 3, memberId);
                        }
                        else
                        {
                            logDAL.InsertLog("UpdateCommerce7Customer", addlInfo + " Email: " + model.emails[0].email + ",Responce:" + responce + ",Request:" + content, "", 1, memberId);

                            if (responce.IndexOf("Exceeded API Rate Limit") > -1)
                            {
                                custUuid.Exceeded = true;
                            }
                        }
                    }
                    httpClient.Dispose();
                }
                catch (Exception ex)
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("UpdateCommerce7Customer", addlInfo + " Email: " + model.emails[0].email + ",Error:" + responce, "", 1, memberId);
                }
            }
            return custUuid;
        }


        public async static Task<Commerce7CustomerModel> CheckAndUpdateCommerce7Customer(string Commerce7Username, string Commerce7Password, string Commerce7Tenant, string FirstName, string LastName, string Company, string Address1, string Address2, string City, string State, string ZipCode, string Country, string Email, string Phone, string DefaultAccountTypeId, int memberId,int rsvpId = 0)
        {
            Commerce7CustomerModel commerce7CustomerModel = new Commerce7CustomerModel();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

            try
            {
                if (!string.IsNullOrWhiteSpace(Commerce7Username)
                                                && !string.IsNullOrWhiteSpace(Commerce7Password) && !string.IsNullOrWhiteSpace(Commerce7Tenant)
                                                 && !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName) && !string.IsNullOrWhiteSpace(Email))
                {
                    var userDetailModel = new List<UserDetailModel>();

                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                    //int memberId = eventDAL.GetWineryIdByCommerce7Data(Commerce7Username, Commerce7Password);

                    string emailMarketingStatus = userDAL.GetEmailMarketingStatus(memberId, Email.Replace(" ", ""));

                    CreateCustomer createCustomer = new CreateCustomer();
                    AddressInfo createAddresses = new AddressInfo();

                    createCustomer.firstName = FirstName;
                    createCustomer.lastName = LastName;
                    createCustomer.city = !string.IsNullOrEmpty(City) ? City : "";
                    createCustomer.stateCode = !string.IsNullOrEmpty(State) ? State : "";

                    ZipCode = (ZipCode + "").Replace(" ", "");

                    if (!string.IsNullOrEmpty(ZipCode) && ZipCode.Length > 3)
                        createCustomer.zipCode = ZipCode;
                    else
                    {
                        WineryModel memberModel = eventDAL.GetWineryById(memberId);
                        createCustomer.zipCode = memberModel.WineryAddress.zip_code;
                    }

                    if (emailMarketingStatus.Length > 0)
                        createCustomer.emailMarketingStatus = emailMarketingStatus;

                    createCustomer.birthDate = "1950-01-01";

                    List<CreateEmail> emails = new List<CreateEmail>();
                    CreateEmail email = new CreateEmail();
                    email.email = Email.Replace(" ", "");

                    emails.Add(email);
                    createCustomer.emails = emails;

                    List<CreatePhone> Phones = new List<CreatePhone>();
                    CreatePhone createPhone = new CreatePhone();
                    createPhone.phone = Common.StringHelpers.FormatTelephoneCommerce7(Phone, Country);

                    if (!string.IsNullOrEmpty(createPhone.phone))
                    {
                        if (createPhone.phone.StartsWith("+1") && !Country.ToLower().Equals("us"))
                        {
                            Country = "US";
                        }

                        Phones.Add(createPhone);
                    }

                    createCustomer.phones = Phones;

                    createCustomer.countryCode = Country;

                    //commerce7CustomerModel.ValidPhoneNumber = createPhone.phone;

                    createAddresses.firstName = FirstName;
                    createAddresses.lastName = LastName;

                    createAddresses.company = !string.IsNullOrEmpty(Company) ? Company : "";
                    createAddresses.address = !string.IsNullOrEmpty(Address1) ? Address1 : "";
                    createAddresses.address2 = !string.IsNullOrEmpty(Address2) ? Address2 : "";
                    createAddresses.city = !string.IsNullOrEmpty(City) ? City : "";
                    createAddresses.stateCode = !string.IsNullOrEmpty(State) ? State : "";
                    createAddresses.zipCode = createCustomer.zipCode;

                    createAddresses.countryCode = Country;

                    createAddresses.phone = createPhone.phone;

                    createAddresses.birthDate = "1950-01-01";

                    userDetailModel = await Task.Run(() => GetCommerce7CustomersByNameOrEmail(Email, Commerce7Username, Commerce7Password, Commerce7Tenant, memberId));
                    if (userDetailModel != null && userDetailModel.Count > 0)
                    {
                        //commerce7CustomerModel = await UpdateCommerce7Customer(createCustomer, Commerce7Username, Commerce7Password, Commerce7Tenant, userDetailModel[0].membership_number, memberId);

                        //if (string.IsNullOrEmpty(commerce7CustomerModel.CustId))
                        //{
                        //    commerce7CustomerModel.CustId = userDetailModel[0].membership_number;
                        //    commerce7CustomerModel.AlreadyExists = true;
                        //}

                        commerce7CustomerModel.CustId = userDetailModel[0].membership_number;
                        commerce7CustomerModel.AlreadyExists = true;
                    }
                    else
                    {
                        //if (DefaultAccountTypeId != null && DefaultAccountTypeId.Length > 0)
                        //{
                        //    string groupName = await GetCommerce7GroupById(DefaultAccountTypeId, Commerce7Username, Commerce7Password, Commerce7Tenant);
                        //    List<string> group = new List<string>();
                        //    if (groupName != null && groupName.Length > 0)
                        //    {
                        //        group.Add(DefaultAccountTypeId);
                        //        createCustomer.groupIds = group;
                        //    }
                        //}

                        commerce7CustomerModel = await CreateCommerce7Customer(createCustomer, Commerce7Username, Commerce7Password, Commerce7Tenant, memberId,rsvpId);
                    }


                    if (!string.IsNullOrWhiteSpace(commerce7CustomerModel.CustId) && !string.IsNullOrEmpty(Address1))
                    {
                        UserDetailModel model = new UserDetailModel();

                        model = await GetCommerce7CustomerAddress(commerce7CustomerModel.CustId, Commerce7Username, Commerce7Password, Commerce7Tenant, memberId, "","");

                        if (model != null && model.color != null && model.color.Length > 0)
                        {
                            //await UpdateCommerce7CustomerAddresses(createAddresses, Commerce7Username, Commerce7Password, Commerce7Tenant, commerce7CustomerModel.CustId, model.color, memberId);
                        }
                        else
                        {
                            await CreateCommerce7CustomerAddresses(createAddresses, Commerce7Username, Commerce7Password, Commerce7Tenant, commerce7CustomerModel.CustId, memberId);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                userDAL.RemoveUserFromAutoSyncingQueue(Email);
                logDAL.InsertLog("WebApi", "CheckAndUpdateCommerce7Customer:  Email:" + Email + ",Message:" + ex.Message.ToString(), "", 1, memberId);
            }

            return commerce7CustomerModel;
        }


        public async static Task<GroupList> GetCommerce7Groups(string Commerce7Username, string Commerce7Password, string Commerce7Tenant)
        {
            GroupList groupList = null;
            try
            {
                HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                using (var response = httpClient.GetAsync($"/v1/group").Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    groupList = JsonConvert.DeserializeObject<GroupList>(varResponse);
                }
                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return groupList;
        }

        
        public async static Task<ClubList> GetCommerce7Clubs(string Commerce7Username, string Commerce7Password, string Commerce7Tenant)
        {
            ClubList clubList = null;
            try
            {
                HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                using (var response = httpClient.GetAsync($"/v1/club").Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    clubList = JsonConvert.DeserializeObject<ClubList>(varResponse);

                    if (clubList.clubs == null)
                    {
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                        logDAL.InsertLog("WebApi", "GetCommerce7Clubs: RequestURL: https://api.commerce7.com/v1/club ,DefaultRequestHeaders:" + httpClient.DefaultRequestHeaders.ToString() + ",Response:" + varResponse.ToString(), "", 1, 0);
                    }
                }
                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return clubList;
        }

        public async static Task<NoteList> GetCommerce7NotesById(string Id, string Commerce7Username, string Commerce7Password, string Commerce7Tenant, int member_id)
        {
            NoteList noteList = null;
            try
            {
                HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                using (var response = httpClient.GetAsync($"/v1/note?customerId={Id}").Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    noteList = JsonConvert.DeserializeObject<NoteList>(varResponse);

                    if (noteList.notes == null)
                    {
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                        logDAL.InsertLog("WebApi", "GetCommerce7NotesById: RequestURL: https://api.commerce7.com/v1/note?customerId={Id} ,DefaultRequestHeaders:" + httpClient.DefaultRequestHeaders.ToString() + ",Response:" + varResponse.ToString(), "", 1, member_id);
                    }
                }
                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return noteList;
        }

        public async static Task<Model.Group> GetCommerce7GroupById(string Id, string Commerce7Username, string Commerce7Password, string Commerce7Tenant, int memberid)
        {
            //string groupName = string.Empty;
            Model.Group group = null;
            try
            {
                string RequestUri = $"/v1/group/{Id}";
                HttpClient httpClient = GetCommerce7HttpClient(Commerce7Username, Commerce7Password, Commerce7Tenant);
                using (var response = httpClient.GetAsync(RequestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    group = JsonConvert.DeserializeObject<Model.Group>(varResponse);

                    if (group == null)
                    {
                        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                        logDAL.InsertLog("WebApi", "GetCommerce7GroupById: RequestURL: " + RequestUri + ", DefaultRequestHeaders:" + httpClient.DefaultRequestHeaders.ToString() + ",Response:" + varResponse.ToString(), "", 1, memberid);
                    }
                }

                //if (group != null && group.id != null)
                //{
                //    groupName = group.title;
                //}

                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetCommerce7GroupById:  Id:" + Id + ",Message:" + ex.Message.ToString(), "", 1, memberid);
            }

            return group;
        }

        public static HttpClient GetCommerce7HttpClient(string Commerce7Username, string Commerce7Password, string Commerce7Tenant)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://api.commerce7.com/v1");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            var byteArray = Encoding.ASCII.GetBytes(Commerce7Username + ":" + Commerce7Password);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            httpClient.DefaultRequestHeaders.Add("tenant", Commerce7Tenant);
            return httpClient;
        }

        public static HttpClient GetCommerce7HttpClientV2(string Commerce7Username, string Commerce7Password, string Commerce7Tenant)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://api.commerce7.com/v2");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            var byteArray = Encoding.ASCII.GetBytes(Commerce7Username + ":" + Commerce7Password);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            httpClient.DefaultRequestHeaders.Add("tenant", Commerce7Tenant);
            return httpClient;
        }
        #endregion

        #region Shopify

        public static HttpClient GetShopifyHttpClient(string ShopifyUrl, string ShopifyAuthToken)
        {
            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("AuthenticateKey", ShopifyAuthToken);
            return httpClient;
        }

        public static async Task<ShopifyCustomerModel> GetShopifyCustomerById(string ShopifyUrl, string ShopifyAuthToken, int member_id, string Id)
        {
            var shopifyCustomerModel = new ShopifyCustomerModel();
            try
            {
                HttpClient httpClient = GetShopifyHttpClient(ShopifyUrl, ShopifyAuthToken);

                string requestUri = string.Format("{0}GetCustomerById?id={1}&member_id={2}", ShopifyUrl, Id, member_id);

                using (var response = httpClient.GetAsync(requestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    shopifyCustomerModel = JsonConvert.DeserializeObject<ShopifyCustomerModel>(varResponse);
                }

                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return shopifyCustomerModel;
        }

        public static async Task<ShopifyAllCustomerModel> GetShopifyAllCustomers(string ShopifyUrl, string ShopifyAuthToken, int member_id)
        {
            var shopifyCustomerModel = new ShopifyAllCustomerModel();
            try
            {
                HttpClient httpClient = GetShopifyHttpClient(ShopifyUrl, ShopifyAuthToken);

                string requestUri = string.Format("{0}GetCustomers?member_id={1}", ShopifyUrl, member_id);

                using (var response = httpClient.GetAsync(requestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    shopifyCustomerModel = JsonConvert.DeserializeObject<ShopifyAllCustomerModel>(varResponse);
                }

                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return shopifyCustomerModel;
        }

        public async static Task<string> CreateShopifyCustomer(CreateShopifyModel model, string ShopifyUrl, string ShopifyAuthToken)
        {
            string custUuid = string.Empty;
            if (model != null && !string.IsNullOrEmpty(ShopifyUrl) && !string.IsNullOrEmpty(ShopifyAuthToken))
            {
                custUuid = await GetShopifyCustomerIdByEmail(ShopifyUrl, ShopifyAuthToken, model.member_id, model.customer.email);

                if (string.IsNullOrEmpty(custUuid))
                {
                    string payLoadContent = JsonConvert.SerializeObject(model);
                    HttpClient httpClient = GetShopifyHttpClient(ShopifyUrl, ShopifyAuthToken);

                    var stringContent = new StringContent(payLoadContent, Encoding.UTF8, "application/json");

                    string requestUri = string.Format("{0}CreateCustomer", ShopifyUrl);

                    using (var response = httpClient.PostAsync(requestUri, stringContent).Result)
                    {
                        var varResponse = response.Content.ReadAsStringAsync().Result;
                        CreateShopifyCustomerResponse custAcDetails = JsonConvert.DeserializeObject<CreateShopifyCustomerResponse>(varResponse);

                        if (custAcDetails != null && custAcDetails.customer != null)
                        {
                            custUuid = custAcDetails.customer.id;
                        }
                        else
                        {
                            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                            logDAL.InsertLog("CreateShopifyCustomer", "Shopify.UpsertCustomerDetails- memberId:" + model.member_id.ToString() + ", Request:" + payLoadContent + ", responce: " + varResponse, "", 1, model.member_id);
                        }
                    }
                    httpClient.Dispose();
                }
            }
            return custUuid + "";
        }

        public async static Task<List<UserDetailModel>> GetShopifyCustomersByNameOrEmail(string ShopifyUrl, string ShopifyAuthToken, int member_id, string keyword)
        {
            List<UserDetailModel> userDetails = new List<UserDetailModel>();
            var list = new ShopifyCustomerListModel();
            try
            {
                HttpClient httpClient = GetShopifyHttpClient(ShopifyUrl, ShopifyAuthToken);

                string requestUri = string.Format("{0}SearchCustomers?keyword={1}&member_id={2}", ShopifyUrl, keyword, member_id);
                using (var response = httpClient.GetAsync(requestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    list = JsonConvert.DeserializeObject<ShopifyCustomerListModel>(varResponse);
                }

                if (list.status == 1)
                {
                    List<AccountTypeDiscountModel> accountTypes = new List<AccountTypeDiscountModel>();
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    accountTypes = userDAL.LoadAccountTypes(member_id, Common.Common.ThirdPartyType.Shopify);

                    foreach (var item in list.customers)
                    {
                        UserDetailModel userDetailModel = new UserDetailModel();
                        Model.UserAddress addr = new Model.UserAddress();

                        userDetailModel.first_name = item.first_name;
                        userDetailModel.last_name = item.last_name;
                        userDetailModel.email = item.email;
                        userDetailModel.membership_number = item.id;

                        NoteList noteList = new NoteList();
                        AccountNote notemodel = new AccountNote();
                        notemodel.modified_by = "";
                        notemodel.note = "";

                        userDetailModel.account_note = notemodel;

                        addr.address_1 = item.default_address.address1;
                        addr.address_2 = item.default_address.address2;

                        userDetailModel.phone_number = FormatTelephoneNumber(item.phone, item.default_address.country_code);

                        addr.city = item.default_address.city;
                        addr.state = item.default_address.province_code;

                        addr.country = item.default_address.country_code + "";

                        addr.zip_code = item.default_address.zip;

                        userDetailModel.address = addr;

                        ShopifyCustomerModel shopifyCustomerModel = await GetShopifyCustomerById(ShopifyUrl, ShopifyAuthToken, member_id, item.id);

                        userDetailModel.last_updated_date_time = shopifyCustomerModel.customer.updated_at;
                        userDetailModel.is_restricted = false;

                        List<string> listcontacttypes = new List<string>();

                        if (!string.IsNullOrEmpty(shopifyCustomerModel.customer.tags))
                        {
                            listcontacttypes = shopifyCustomerModel.customer.tags.Split(",").ToList();

                            if (listcontacttypes.Count > 0)
                            {
                                foreach (var i in listcontacttypes)
                                {
                                    dynamic dbContactType = accountTypes.Where(f => f.ContactType.Trim().ToLower() == i.Trim().ToLower()).FirstOrDefault();

                                    if ((dbContactType != null) && (dbContactType != null))
                                    {
                                        userDetailModel.customer_type = 1;
                                        userDetailModel.member_status = true;
                                    }
                                }
                            }
                        }

                        userDetailModel.contact_types = listcontacttypes;

                        userDetails.Add(userDetailModel);
                    }
                }

                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return userDetails;
        }

        public async static Task<string> GetShopifyCustomerIdByEmail(string ShopifyUrl, string ShopifyAuthToken, int member_id, string keyword)
        {
            string UId = string.Empty;
            var list = new ShopifyCustomerListModel();
            try
            {
                HttpClient httpClient = GetShopifyHttpClient(ShopifyUrl, ShopifyAuthToken);

                string requestUri = string.Format("{0}SearchCustomers?keyword={1}&member_id={2}", ShopifyUrl, keyword, member_id);
                using (var response = httpClient.GetAsync(requestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    list = JsonConvert.DeserializeObject<ShopifyCustomerListModel>(varResponse);
                }

                if (list.status == 1)
                {
                    UId = list.customers[0].id;
                }

                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return UId;
        }

        #endregion

        #region ReserveCloud

        public static HttpClient GetReserveCloudHttpClient(string ReserveCloudUrl, string ReServeCloudApiUserName, string ReServeCloudApiPassword)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(ReserveCloudUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            var byteArray = Encoding.ASCII.GetBytes(ReServeCloudApiUserName + ":" + ReServeCloudApiPassword);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            return httpClient;
        }

        public static List<ReserveCloudReservation> GetReserveCloudReservation(DateTime selecteddate, string ReserveCloudUrl, string ReServeCloudSiteId, string ReServeCloudApiUserName, string ReServeCloudApiPassword)
        {
            var reserveCloudReservation = new List<ReserveCloudReservation>();
            try
            {
                RootObject modelList = null;
                HttpClient httpClient = GetReserveCloudHttpClient(ReserveCloudUrl, ReServeCloudApiUserName, ReServeCloudApiPassword);

                string requestUri = string.Format("{0}/gateway/request?requestName=get_reservations_{1}&requestGuid=66c00aa1-d09b-481e-868d-0f35e5314250&maxResults=100&filters=[['startDate','EQUAL_TO','{2}']]", ReserveCloudUrl, ReServeCloudSiteId, selecteddate.ToShortDateString());
                //using (var response = httpClient.GetAsync("https://qa.reservecloud.com/gateway/request?requestName=get_reservations_BER&requestGuid=66c00aa1-d09b-481e-868d-0f35e5314250&maxResults=100&filters=[['startDate','EQUAL_TO','" + selecteddate.ToShortDateString() + "']]").Result)
                //using (var response = httpClient.GetAsync("https://www.reservecloud.com/gateway/request?requestName=get_reservations_BER&requestGuid=66c00aa1-d09b-481e-868d-0f35e5314250&maxResults=100&filters=[['startDate','EQUAL_TO','"+ selecteddate.ToShortDateString() + "']]").Result)
                using (var response = httpClient.GetAsync(requestUri).Result)
                {
                    var varResponse = response.Content.ReadAsStringAsync().Result;
                    modelList = JsonConvert.DeserializeObject<RootObject>(varResponse);

                    if (modelList != null && modelList.count > 0)
                    {
                        foreach (var item in modelList.results)
                        {
                            ReserveCloudReservation model = new ReserveCloudReservation();
                            model.function_name = item[0];
                            model.function_start_date = item[1];
                            model.function_type_name = item[2];
                            model.locations_name = item[3];
                            model.function_start_time = item[4];
                            model.function_end_time = item[5];
                            model.setup_minutes = item[6];
                            model.teardown_minutes = item[7];
                            model.function_attendance = item[8];
                            model.function_number = item[9];
                            model.setup_style = item[10];
                            model.site_name = item[11];
                            model.primary_contact_first_name = item[12];
                            model.primary_contact_last_name = item[13];
                            model.referral_type = item[14];
                            model.event_number = item[15];
                            model.event_salesperson_first_name = item[16];
                            model.event_salesperson_last_name = item[17];
                            model.event_status = item[18];
                            model.event_type = item[19];
                            model.event_start_date = item[20];
                            model.estimated_attendance = item[21];
                            model.event_name = item[22];
                            model.owner_first_name = item[23];
                            model.owner_last_name = item[24];
                            model.primary_contact_phone = item[25];
                            model.primary_contact_mobile = item[26];

                            reserveCloudReservation.Add(model);
                        }
                    }
                }

                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return reserveCloudReservation;
        }

        #endregion

        #region Users
        public static async Task<List<UserDetailModel>> GetUsersByEmail(string keyword, int member_id = 0, bool ignore_local_db = false, string ShopifyUrl = "", string ShopifyAuthToken = "")
        {
            var userList = new List<UserDetailModel>();

            if (!string.IsNullOrEmpty(keyword))
            {
                string FormatPhoneNumber = Utility.FormatPhoneNumber(keyword);
                int searchType = 1;

                if (FormatPhoneNumber.Length > 0 && FormatPhoneNumber != "0" && keyword.IndexOf("@") == -1)
                {
                    searchType = 0;
                    keyword = FormatPhoneNumber;
                }

                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WineryModel memberModel = await Task.Run(() => eventDAL.GetWineryById(member_id));
                bool bLoyalClubLookupEnabled = false;
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(member_id, (int)Common.Common.SettingGroup.bLoyal).ToList();
                if ((settingsGroup != null))
                {
                    if (settingsGroup.Count > 0)
                    {
                        bool ret = false;
                        dynamic dbSettings = settingsGroup.Where(f => f.Key == Common.Common.SettingKey.bLoyalApiClubLookup).FirstOrDefault();

                        if ((dbSettings != null))
                        {
                            bool.TryParse(dbSettings.Value, out ret);
                        }

                        if (ret == true)
                        {
                            bLoyalClubLookupEnabled = true;
                        }
                    }
                }

                try
                {
                    if (keyword.ToLower().IndexOf("@noemail") == -1)
                    {
                        if ((memberModel.EnableClubVin65 && !string.IsNullOrWhiteSpace(memberModel.Vin65UserName) && !string.IsNullOrWhiteSpace(memberModel.Vin65Password)) ||
                        (memberModel.EnableClubemember && !string.IsNullOrWhiteSpace(memberModel.eMemberUserNAme) && !string.IsNullOrWhiteSpace(memberModel.eMemberPAssword)) ||
                        (memberModel.EnableClubOrderPort && !string.IsNullOrWhiteSpace(memberModel.OrderPortClientId) && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiKey) && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiToken)) ||
                        (memberModel.EnableClubCommerce7 && !string.IsNullOrWhiteSpace(memberModel.Commerce7Username) && !string.IsNullOrWhiteSpace(memberModel.Commerce7Password) && !string.IsNullOrWhiteSpace(memberModel.Commerce7Tenant)) ||
                        (memberModel.EnableClubShopify && !string.IsNullOrWhiteSpace(memberModel.ShopifyPublishKey) && !string.IsNullOrWhiteSpace(memberModel.ShopifyAppPassword) && !string.IsNullOrWhiteSpace(memberModel.ShopifyAppStoreName)) ||
                        (memberModel.EnableClubBigCommerce && !string.IsNullOrWhiteSpace(memberModel.BigCommerceAceessToken) && !string.IsNullOrWhiteSpace(memberModel.BigCommerceStoreId))
                        )
                        {
                            if (memberModel.EnableClubVin65 && !string.IsNullOrWhiteSpace(memberModel.Vin65UserName) && !string.IsNullOrWhiteSpace(memberModel.Vin65Password))
                            {
                                List<Vin65Model> modelList = new List<Vin65Model>();
                                modelList = await Task.Run(() => Utility.Vin65GetContacts(keyword, memberModel.Vin65Password, memberModel.Vin65UserName));

                                foreach (var item in modelList)
                                {
                                    UserDetailModel vin65model = new UserDetailModel();
                                    vin65model.email = item.Email;
                                    vin65model.first_name = item.FirstName;
                                    vin65model.last_name = item.LastName;
                                    vin65model.phone_number = Utility.FormatTelephoneNumber(item.HomePhone, item.Country);
                                    vin65model.membership_number = item.Vin65ID;
                                    vin65model.ltv = item.ltv;
                                    vin65model.last_order_date = item.last_order_date;
                                    vin65model.member_status = item.member_status;
                                    vin65model.order_count = item.order_count;
                                    vin65model.is_restricted = false;

                                    Model.UserAddress addr = new Model.UserAddress();

                                    addr.address_1 = item.BillingStreet;
                                    addr.address_2 = item.BillingStreet2;
                                    addr.city = item.BillingCity;
                                    addr.state = item.BillingState;

                                    addr.country = item.Country + "";

                                    if (addr.country.ToLower() == "us")
                                    {
                                        addr.zip_code = item.BillingZip + "";
                                        if (addr.zip_code.Length > 5)
                                            addr.zip_code = addr.zip_code.Substring(0, 5);
                                    }
                                    else
                                        addr.zip_code = item.BillingZip;

                                    vin65model.address = addr;

                                    if (vin65model.member_status == true)
                                    {
                                        vin65model.customer_type = 1;
                                    }
                                    else
                                    {
                                        vin65model.customer_type = 0;
                                    }

                                    AccountNote notemodel = new AccountNote();
                                    notemodel.modified_by = "";
                                    notemodel.note = "";
                                    if (member_id > 0 && ignore_local_db == false)
                                    {
                                        var guestPerformanceModel = new GuestPerformance();
                                        guestPerformanceModel = userDAL.GetGuestPerformanceData(vin65model.email, member_id);

                                        if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                                        {
                                            vin65model.completed_count = guestPerformanceModel.completed_count;
                                            vin65model.visits_count = guestPerformanceModel.visits_count;
                                            vin65model.cancellations_count = guestPerformanceModel.cancellations_count;
                                            vin65model.no_shows_count = guestPerformanceModel.no_shows_count;
                                            vin65model.account_note = userDAL.GetAccountNote(member_id, guestPerformanceModel.user_id);
                                            vin65model.color = guestPerformanceModel.color;
                                            vin65model.roles = guestPerformanceModel.roles;
                                            vin65model.user_id = guestPerformanceModel.user_id;
                                            vin65model.mobile_number = Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.Country);
                                            vin65model.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                            vin65model.region_most_visited = guestPerformanceModel.region_most_visited;
                                            vin65model.company_name = guestPerformanceModel.company_name;
                                            vin65model.gender = guestPerformanceModel.gender;

                                            vin65model.title = guestPerformanceModel.title;
                                            vin65model.work_number = guestPerformanceModel.work_number;

                                            if (vin65model.address != null && (vin65model.address.zip_code + "").Length == 0)
                                                vin65model.address.zip_code = guestPerformanceModel.zip_code;

                                            if (vin65model.address != null && (vin65model.address.address_1 + "").Length == 0)
                                                vin65model.address.address_1 = guestPerformanceModel.address_1;

                                            if (vin65model.address != null && (vin65model.address.address_2 + "").Length == 0)
                                                vin65model.address.address_2 = guestPerformanceModel.address_2;

                                            if (vin65model.phone_number != null && (vin65model.phone_number + "").Length == 0)
                                                vin65model.phone_number = guestPerformanceModel.phone_number;

                                            if (vin65model.address != null && (vin65model.address.city + "").Length == 0)
                                                vin65model.address.city = guestPerformanceModel.city;

                                            if (vin65model.address != null && (vin65model.address.state + "").Length == 0)
                                                vin65model.address.state = guestPerformanceModel.state;

                                            vin65model.sms_opt_out = guestPerformanceModel.sms_opt_out;
                                            vin65model.last_reservation_id = guestPerformanceModel.last_reservation_id;
                                            vin65model.last_check_in_date = guestPerformanceModel.last_check_in_date;
                                            vin65model.birth_date = guestPerformanceModel.birth_date;
                                            vin65model.user_tags = guestPerformanceModel.user_tags;
                                            vin65model.user_image = guestPerformanceModel.user_image;
                                        }
                                        else
                                        {
                                            vin65model.account_note = notemodel;
                                        }
                                    }
                                    else
                                    {
                                        vin65model.account_note = notemodel;
                                    }


                                    vin65model.contact_types = item.contact_types;

                                    userList.Add(vin65model);
                                }
                            }
                            else if (memberModel.EnableClubOrderPort && !string.IsNullOrWhiteSpace(memberModel.OrderPortClientId) && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiKey) && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiToken))
                            {
                                userList = await Task.Run(() => Utility.GetCustomersByNameOrEmail(keyword, memberModel.OrderPortApiKey, memberModel.OrderPortApiToken, memberModel.OrderPortClientId));

                                if (member_id > 0 && ignore_local_db == false)
                                {
                                    foreach (var item in userList)
                                    {
                                        AccountNote notemodel = new AccountNote();
                                        notemodel.modified_by = "";
                                        notemodel.note = "";
                                        var guestPerformanceModel = new GuestPerformance();
                                        guestPerformanceModel = userDAL.GetGuestPerformanceData(item.email, member_id);

                                        if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                                        {
                                            item.completed_count = guestPerformanceModel.completed_count;
                                            item.visits_count = guestPerformanceModel.visits_count;
                                            item.cancellations_count = guestPerformanceModel.cancellations_count;
                                            item.no_shows_count = guestPerformanceModel.no_shows_count;
                                            item.account_note = notemodel; // guestPerformanceModel.account_note;
                                            item.color = guestPerformanceModel.color;
                                            item.roles = guestPerformanceModel.roles;
                                            item.user_id = guestPerformanceModel.user_id;
                                            item.mobile_number = Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                                            item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                            item.region_most_visited = guestPerformanceModel.region_most_visited;
                                            item.company_name = guestPerformanceModel.company_name;
                                            item.gender = guestPerformanceModel.gender;

                                            item.title = guestPerformanceModel.title;
                                            item.work_number = guestPerformanceModel.work_number;

                                            if (item.address != null && (item.address.zip_code + "").Length == 0)
                                                item.address.zip_code = guestPerformanceModel.zip_code;

                                            if (item.address != null && (item.address.address_1 + "").Length == 0)
                                                item.address.address_1 = guestPerformanceModel.address_1;

                                            if (item.address != null && (item.address.address_2 + "").Length == 0)
                                                item.address.address_2 = guestPerformanceModel.address_2;

                                            if (item.phone_number != null && (item.phone_number + "").Length == 0)
                                                item.phone_number = guestPerformanceModel.phone_number;

                                            if (item.address != null && (item.address.city + "").Length == 0)
                                                item.address.city = guestPerformanceModel.city;

                                            if (item.address != null && (item.address.state + "").Length == 0)
                                                item.address.state = guestPerformanceModel.state;

                                            item.sms_opt_out = guestPerformanceModel.sms_opt_out;
                                            item.last_reservation_id = guestPerformanceModel.last_reservation_id;
                                            item.last_check_in_date = guestPerformanceModel.last_check_in_date;
                                            item.birth_date = guestPerformanceModel.birth_date;
                                            item.user_tags = guestPerformanceModel.user_tags;
                                            item.user_image = guestPerformanceModel.user_image;
                                        }
                                        else
                                        {
                                            item.account_note = notemodel;
                                        }
                                    }
                                }

                            }
                            else if (memberModel.EnableClubCommerce7 && !string.IsNullOrWhiteSpace(memberModel.Commerce7Username) && !string.IsNullOrWhiteSpace(memberModel.Commerce7Password) && !string.IsNullOrWhiteSpace(memberModel.Commerce7Tenant))
                            {
                                userList = await Task.Run(() => Utility.GetCommerce7CustomersByNameOrEmail(keyword, memberModel.Commerce7Username, memberModel.Commerce7Password, memberModel.Commerce7Tenant, member_id));

                                if (member_id > 0 && ignore_local_db == false)
                                {
                                    foreach (var item in userList)
                                    {
                                        var guestPerformanceModel = new GuestPerformance();
                                        guestPerformanceModel = userDAL.GetGuestPerformanceData(item.email, member_id);

                                        if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                                        {
                                            item.completed_count = guestPerformanceModel.completed_count;
                                            item.visits_count = guestPerformanceModel.visits_count;
                                            item.cancellations_count = guestPerformanceModel.cancellations_count;
                                            item.no_shows_count = guestPerformanceModel.no_shows_count;
                                            item.color = guestPerformanceModel.color;
                                            item.roles = guestPerformanceModel.roles;
                                            item.user_id = guestPerformanceModel.user_id;
                                            item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                            item.region_most_visited = guestPerformanceModel.region_most_visited;
                                            item.company_name = guestPerformanceModel.company_name;
                                            item.gender = guestPerformanceModel.gender;

                                            item.title = guestPerformanceModel.title;
                                            item.work_number = guestPerformanceModel.work_number;

                                            if (item.address != null && (item.address.zip_code + "").Length == 0)
                                                item.address.zip_code = guestPerformanceModel.zip_code;

                                            if (item.address != null && (item.address.address_1 + "").Length == 0)
                                                item.address.address_1 = guestPerformanceModel.address_1;

                                            if (item.address != null && (item.address.address_2 + "").Length == 0)
                                                item.address.address_2 = guestPerformanceModel.address_2;

                                            if (item.phone_number != null && (item.phone_number + "").Length == 0)
                                                item.phone_number = guestPerformanceModel.phone_number;

                                            if (item.address != null && (item.address.city + "").Length == 0)
                                                item.address.city = guestPerformanceModel.city;

                                            if (item.address != null && (item.address.state + "").Length == 0)
                                                item.address.state = guestPerformanceModel.state;

                                            item.sms_opt_out = guestPerformanceModel.sms_opt_out;

                                            AccountNote accountNote = userDAL.GetAccountNote(member_id, guestPerformanceModel.user_id);

                                            if (accountNote != null && accountNote.note != null && accountNote.note.Trim().Length > 0)
                                            {
                                                if (item.account_note != null && item.account_note.note != null && item.account_note.note.Trim().Length > 0)
                                                {
                                                    if (accountNote.note.IndexOf(item.account_note.note.Trim()) == -1)
                                                    {
                                                        string note = accountNote.note + ", " + item.account_note.note;
                                                        item.account_note.note = note;
                                                    }
                                                }
                                                else
                                                {
                                                    item.account_note = accountNote;
                                                }
                                            }
                                            item.mobile_number = Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                                            item.last_reservation_id = guestPerformanceModel.last_reservation_id;
                                            item.last_check_in_date = guestPerformanceModel.last_check_in_date;
                                            item.birth_date = guestPerformanceModel.birth_date;
                                            item.user_tags = guestPerformanceModel.user_tags;
                                            item.user_image = guestPerformanceModel.user_image;
                                        }
                                    }
                                }

                            }
                            else if (memberModel.EnableClubemember && !string.IsNullOrWhiteSpace(memberModel.eMemberUserNAme) && !string.IsNullOrWhiteSpace(memberModel.eMemberPAssword))
                            {
                                List<eWineryCustomerViewModel> modelList = new List<eWineryCustomerViewModel>();
                                modelList = await Task.Run(() => Utility.ResolveCustomer(memberModel.eMemberUserNAme, memberModel.eMemberPAssword, keyword, member_id));

                                foreach (var item in modelList)
                                {
                                    UserDetailModel eMemberModel = new UserDetailModel();
                                    eMemberModel.email = item.email;
                                    eMemberModel.first_name = item.first_name;
                                    eMemberModel.last_name = item.last_name;
                                    eMemberModel.phone_number = Utility.FormatTelephoneNumber(item.phone, item.country_code);
                                    eMemberModel.phone_number = item.phone;
                                    eMemberModel.membership_number = item.member_id;
                                    eMemberModel.member_status = item.member_status;
                                    eMemberModel.is_restricted = false;
                                    Model.UserAddress addr = new Model.UserAddress();

                                    addr.address_1 = item.address1;
                                    addr.address_2 = item.address2;
                                    addr.city = item.city;
                                    addr.state = item.state;

                                    addr.country = item.country_code + "";

                                    if (addr.country.ToLower() == "us")
                                    {
                                        addr.zip_code = item.zip_code + "";
                                        if (addr.zip_code.Length > 5)
                                            addr.zip_code = addr.zip_code.Substring(0, 5);
                                    }
                                    else
                                        addr.zip_code = item.zip_code;

                                    eMemberModel.address = addr;

                                    if (eMemberModel.member_status == true)
                                    {
                                        eMemberModel.customer_type = 1;
                                    }
                                    else
                                    {
                                        eMemberModel.customer_type = 0;
                                    }

                                    List<string> listcontacttypes = new List<string>();
                                    foreach (var c in item.memberships)
                                    {
                                        listcontacttypes.Add(c.club_name);
                                    }

                                    eMemberModel.contact_types = listcontacttypes;

                                    if (member_id > 0 && ignore_local_db == false)
                                    {
                                        var guestPerformanceModel = new GuestPerformance();
                                        guestPerformanceModel = userDAL.GetGuestPerformanceData(eMemberModel.email, member_id);

                                        if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                                        {
                                            eMemberModel.completed_count = guestPerformanceModel.completed_count;
                                            eMemberModel.visits_count = guestPerformanceModel.visits_count;
                                            eMemberModel.cancellations_count = guestPerformanceModel.cancellations_count;
                                            eMemberModel.no_shows_count = guestPerformanceModel.no_shows_count;
                                            eMemberModel.color = guestPerformanceModel.color;
                                            eMemberModel.roles = guestPerformanceModel.roles;
                                            eMemberModel.user_id = guestPerformanceModel.user_id;
                                            eMemberModel.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                            eMemberModel.region_most_visited = guestPerformanceModel.region_most_visited;
                                            eMemberModel.company_name = guestPerformanceModel.company_name;
                                            eMemberModel.gender = guestPerformanceModel.gender;

                                            eMemberModel.title = guestPerformanceModel.title;
                                            eMemberModel.work_number = guestPerformanceModel.work_number;

                                            if (eMemberModel.address != null && (eMemberModel.address.zip_code + "").Length == 0)
                                                eMemberModel.address.zip_code = guestPerformanceModel.zip_code;

                                            if (eMemberModel.address != null && (eMemberModel.address.address_1 + "").Length == 0)
                                                eMemberModel.address.address_1 = guestPerformanceModel.address_1;

                                            if (eMemberModel.address != null && (eMemberModel.address.address_2 + "").Length == 0)
                                                eMemberModel.address.address_2 = guestPerformanceModel.address_2;

                                            if (eMemberModel.phone_number != null && (eMemberModel.phone_number + "").Length == 0)
                                                eMemberModel.phone_number = guestPerformanceModel.phone_number;

                                            if (eMemberModel.address != null && (eMemberModel.address.city + "").Length == 0)
                                                eMemberModel.address.city = guestPerformanceModel.city;

                                            if (eMemberModel.address != null && (eMemberModel.address.state + "").Length == 0)
                                                eMemberModel.address.state = guestPerformanceModel.state;

                                            eMemberModel.sms_opt_out = guestPerformanceModel.sms_opt_out;

                                            AccountNote accountNote = userDAL.GetAccountNote(member_id, guestPerformanceModel.user_id);

                                            if (accountNote != null && accountNote.note != null && accountNote.note.Trim().Length > 0)
                                            {
                                                if (item.account_note != null && item.account_note.note != null && item.account_note.note.Trim().Length > 0)
                                                {
                                                    if (accountNote.note.IndexOf(item.account_note.note.Trim()) == -1)
                                                    {
                                                        string note = accountNote.note + ", " + item.account_note.note;
                                                        eMemberModel.account_note.note = note;
                                                    }
                                                }
                                                else
                                                {
                                                    eMemberModel.account_note = accountNote;
                                                }
                                            }
                                            eMemberModel.mobile_number = Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, eMemberModel.address.country);
                                            eMemberModel.last_reservation_id = guestPerformanceModel.last_reservation_id;
                                            eMemberModel.last_check_in_date = guestPerformanceModel.last_check_in_date;
                                            eMemberModel.birth_date = guestPerformanceModel.birth_date;
                                            eMemberModel.user_tags = guestPerformanceModel.user_tags;
                                            eMemberModel.user_image = guestPerformanceModel.user_image;
                                        }
                                    }

                                    userList.Add(eMemberModel);
                                }
                            }
                            else if (memberModel.EnableClubShopify && !string.IsNullOrWhiteSpace(memberModel.ShopifyPublishKey) && !string.IsNullOrWhiteSpace(memberModel.ShopifyAppPassword) && !string.IsNullOrWhiteSpace(memberModel.ShopifyAppStoreName))
                            {
                                userList = await Task.Run(() => Utility.GetShopifyCustomersByNameOrEmail(ShopifyUrl, ShopifyAuthToken, member_id, keyword));
                                if (member_id > 0 && ignore_local_db == false)
                                {
                                    foreach (var item in userList)
                                    {
                                        var guestPerformanceModel = new GuestPerformance();
                                        guestPerformanceModel = userDAL.GetGuestPerformanceData(item.email, member_id);

                                        if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                                        {
                                            item.completed_count = guestPerformanceModel.completed_count;
                                            item.visits_count = guestPerformanceModel.visits_count;
                                            item.cancellations_count = guestPerformanceModel.cancellations_count;
                                            item.no_shows_count = guestPerformanceModel.no_shows_count;
                                            item.color = guestPerformanceModel.color;
                                            item.roles = guestPerformanceModel.roles;
                                            item.user_id = guestPerformanceModel.user_id;
                                            item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                            item.region_most_visited = guestPerformanceModel.region_most_visited;
                                            item.company_name = guestPerformanceModel.company_name;
                                            item.gender = guestPerformanceModel.gender;

                                            item.title = guestPerformanceModel.title;
                                            item.work_number = guestPerformanceModel.work_number;

                                            if (item.address != null && (item.address.zip_code + "").Length == 0)
                                                item.address.zip_code = guestPerformanceModel.zip_code;

                                            if (item.address != null && (item.address.address_1 + "").Length == 0)
                                                item.address.address_1 = guestPerformanceModel.address_1;

                                            if (item.address != null && (item.address.address_2 + "").Length == 0)
                                                item.address.address_2 = guestPerformanceModel.address_2;

                                            if (item.phone_number != null && (item.phone_number + "").Length == 0)
                                                item.phone_number = guestPerformanceModel.phone_number;

                                            if (item.address != null && (item.address.city + "").Length == 0)
                                                item.address.city = guestPerformanceModel.city;

                                            if (item.address != null && (item.address.state + "").Length == 0)
                                                item.address.state = guestPerformanceModel.state;

                                            item.sms_opt_out = guestPerformanceModel.sms_opt_out;

                                            AccountNote accountNote = userDAL.GetAccountNote(member_id, guestPerformanceModel.user_id);

                                            if (accountNote != null && accountNote.note != null && accountNote.note.Trim().Length > 0)
                                            {
                                                if (item.account_note != null && item.account_note.note != null && item.account_note.note.Trim().Length > 0)
                                                {
                                                    if (accountNote.note.IndexOf(item.account_note.note.Trim()) == -1)
                                                    {
                                                        string note = accountNote.note + ", " + item.account_note.note;
                                                        item.account_note.note = note;
                                                    }
                                                }
                                                else
                                                {
                                                    item.account_note = accountNote;
                                                }
                                            }
                                            item.mobile_number = Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                                            item.last_reservation_id = guestPerformanceModel.last_reservation_id;
                                            item.last_check_in_date = guestPerformanceModel.last_check_in_date;
                                            item.birth_date = guestPerformanceModel.birth_date;
                                            item.user_tags = guestPerformanceModel.user_tags;
                                            item.user_image = guestPerformanceModel.user_image;
                                        }
                                    }
                                }

                            }
                            else if (memberModel.EnableClubBigCommerce && !string.IsNullOrWhiteSpace(memberModel.BigCommerceAceessToken) && !string.IsNullOrWhiteSpace(memberModel.BigCommerceStoreId))
                            {
                                userList = await Task.Run(() => Utility.GetBigCommerceCustomersByNameOrEmail(memberModel.BigCommerceStoreId, memberModel.BigCommerceAceessToken, member_id, keyword));
                                if (member_id > 0 && ignore_local_db == false)
                                {
                                    foreach (var item in userList)
                                    {
                                        var guestPerformanceModel = new GuestPerformance();
                                        guestPerformanceModel = userDAL.GetGuestPerformanceData(item.email, member_id);

                                        if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                                        {
                                            item.completed_count = guestPerformanceModel.completed_count;
                                            item.visits_count = guestPerformanceModel.visits_count;
                                            item.cancellations_count = guestPerformanceModel.cancellations_count;
                                            item.no_shows_count = guestPerformanceModel.no_shows_count;
                                            item.color = guestPerformanceModel.color;
                                            item.roles = guestPerformanceModel.roles;
                                            item.user_id = guestPerformanceModel.user_id;
                                            item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                            item.region_most_visited = guestPerformanceModel.region_most_visited;
                                            item.company_name = guestPerformanceModel.company_name;
                                            item.gender = guestPerformanceModel.gender;

                                            item.title = guestPerformanceModel.title;
                                            item.work_number = guestPerformanceModel.work_number;

                                            if (item.address != null && (item.address.zip_code + "").Length == 0)
                                                item.address.zip_code = guestPerformanceModel.zip_code;

                                            if (item.address != null && (item.address.address_1 + "").Length == 0)
                                                item.address.address_1 = guestPerformanceModel.address_1;

                                            if (item.address != null && (item.address.address_2 + "").Length == 0)
                                                item.address.address_2 = guestPerformanceModel.address_2;

                                            if (item.phone_number != null && (item.phone_number + "").Length == 0)
                                                item.phone_number = guestPerformanceModel.phone_number;

                                            if (item.address != null && (item.address.city + "").Length == 0)
                                                item.address.city = guestPerformanceModel.city;

                                            if (item.address != null && (item.address.state + "").Length == 0)
                                                item.address.state = guestPerformanceModel.state;

                                            item.sms_opt_out = guestPerformanceModel.sms_opt_out;

                                            AccountNote accountNote = userDAL.GetAccountNote(member_id, guestPerformanceModel.user_id);

                                            if (accountNote != null && accountNote.note != null && accountNote.note.Trim().Length > 0)
                                            {
                                                if (item.account_note != null && item.account_note.note != null && item.account_note.note.Trim().Length > 0)
                                                {
                                                    if (accountNote.note.IndexOf(item.account_note.note.Trim()) == -1)
                                                    {
                                                        string note = accountNote.note + ", " + item.account_note.note;
                                                        item.account_note.note = note;
                                                    }
                                                }
                                                else
                                                {
                                                    item.account_note = accountNote;
                                                }
                                            }
                                            item.mobile_number = Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                                            item.last_reservation_id = guestPerformanceModel.last_reservation_id;
                                            item.last_check_in_date = guestPerformanceModel.last_check_in_date;
                                            item.birth_date = guestPerformanceModel.birth_date;
                                            item.user_tags = guestPerformanceModel.user_tags;
                                            item.user_image = guestPerformanceModel.user_image;
                                        }
                                    }
                                }

                            }
                        }
                        else if (bLoyalClubLookupEnabled)
                        {
                            var user = await bLoyalResolveCustomer(settingsGroup, keyword);
                            if (user != null)
                            {
                                userList.Add(user);

                                if (member_id > 0 && ignore_local_db == false)
                                {
                                    foreach (var item in userList)
                                    {
                                        AccountNote notemodel = new AccountNote();
                                        notemodel.modified_by = "";
                                        notemodel.note = "";
                                        var guestPerformanceModel = new GuestPerformance();
                                        guestPerformanceModel = userDAL.GetGuestPerformanceData(item.email, member_id);

                                        if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                                        {
                                            item.completed_count = guestPerformanceModel.completed_count;
                                            item.visits_count = guestPerformanceModel.visits_count;
                                            item.cancellations_count = guestPerformanceModel.cancellations_count;
                                            item.no_shows_count = guestPerformanceModel.no_shows_count;
                                            item.account_note = notemodel; // guestPerformanceModel.account_note;
                                            item.color = guestPerformanceModel.color;
                                            item.roles = guestPerformanceModel.roles;
                                            item.user_id = guestPerformanceModel.user_id;
                                            item.mobile_number = Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                                            item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                            item.region_most_visited = guestPerformanceModel.region_most_visited;
                                            item.company_name = guestPerformanceModel.company_name;
                                            item.gender = guestPerformanceModel.gender;

                                            item.title = guestPerformanceModel.title;
                                            item.work_number = guestPerformanceModel.work_number;

                                            if (item.address != null && (item.address.zip_code + "").Length == 0)
                                                item.address.zip_code = guestPerformanceModel.zip_code;

                                            if (item.address != null && (item.address.address_1 + "").Length == 0)
                                                item.address.address_1 = guestPerformanceModel.address_1;

                                            if (item.address != null && (item.address.address_2 + "").Length == 0)
                                                item.address.address_2 = guestPerformanceModel.address_2;

                                            if (item.phone_number != null && (item.phone_number + "").Length == 0)
                                                item.phone_number = guestPerformanceModel.phone_number;

                                            if (item.address != null && (item.address.city + "").Length == 0)
                                                item.address.city = guestPerformanceModel.city;

                                            if (item.address != null && (item.address.state + "").Length == 0)
                                                item.address.state = guestPerformanceModel.state;

                                            item.sms_opt_out = guestPerformanceModel.sms_opt_out;
                                            item.last_reservation_id = guestPerformanceModel.last_reservation_id;
                                            item.last_check_in_date = guestPerformanceModel.last_check_in_date;
                                            item.birth_date = guestPerformanceModel.birth_date;
                                            item.user_tags = guestPerformanceModel.user_tags;
                                            item.user_image = guestPerformanceModel.user_image;
                                        }
                                        else
                                        {
                                            item.account_note = notemodel;
                                        }
                                    }
                                }
                            }
                        }
                        else if (memberModel.EnableClubSalesforce && !string.IsNullOrWhiteSpace(memberModel.SalesForceUserName) && !string.IsNullOrWhiteSpace(memberModel.SalesForcePassword) && !string.IsNullOrWhiteSpace(memberModel.SalesForceSecurityToken))
                        {
                            var user = await SalesForceResolveCustomer(memberModel.SalesForceUserName, memberModel.SalesForcePassword, memberModel.SalesForceSecurityToken, keyword, memberModel.ClubStatusField);
                            if (user != null)
                            {
                                userList.Add(user);

                                if (member_id > 0 && ignore_local_db == false)
                                {
                                    foreach (var item in userList)
                                    {
                                        AccountNote notemodel = new AccountNote();
                                        notemodel.modified_by = "";
                                        notemodel.note = "";
                                        var guestPerformanceModel = new GuestPerformance();
                                        guestPerformanceModel = userDAL.GetGuestPerformanceData(item.email, member_id);

                                        if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                                        {
                                            item.completed_count = guestPerformanceModel.completed_count;
                                            item.visits_count = guestPerformanceModel.visits_count;
                                            item.cancellations_count = guestPerformanceModel.cancellations_count;
                                            item.no_shows_count = guestPerformanceModel.no_shows_count;
                                            item.account_note = notemodel; // guestPerformanceModel.account_note;
                                            item.color = guestPerformanceModel.color;
                                            item.roles = guestPerformanceModel.roles;
                                            item.user_id = guestPerformanceModel.user_id;
                                            item.mobile_number = Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                                            item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                            item.region_most_visited = guestPerformanceModel.region_most_visited;
                                            item.company_name = guestPerformanceModel.company_name;
                                            item.gender = guestPerformanceModel.gender;

                                            item.title = guestPerformanceModel.title;
                                            item.work_number = guestPerformanceModel.work_number;

                                            if (item.address != null && (item.address.zip_code + "").Length == 0)
                                                item.address.zip_code = guestPerformanceModel.zip_code;

                                            if (item.address != null && (item.address.address_1 + "").Length == 0)
                                                item.address.address_1 = guestPerformanceModel.address_1;

                                            if (item.address != null && (item.address.address_2 + "").Length == 0)
                                                item.address.address_2 = guestPerformanceModel.address_2;

                                            if (item.phone_number != null && (item.phone_number + "").Length == 0)
                                                item.phone_number = guestPerformanceModel.phone_number;

                                            if (item.address != null && (item.address.city + "").Length == 0)
                                                item.address.city = guestPerformanceModel.city;

                                            if (item.address != null && (item.address.state + "").Length == 0)
                                                item.address.state = guestPerformanceModel.state;

                                            item.sms_opt_out = guestPerformanceModel.sms_opt_out;
                                            item.last_reservation_id = guestPerformanceModel.last_reservation_id;
                                            item.last_check_in_date = guestPerformanceModel.last_check_in_date;
                                            item.birth_date = guestPerformanceModel.birth_date;
                                            item.user_tags = guestPerformanceModel.user_tags;
                                            item.user_image = guestPerformanceModel.user_image;
                                        }
                                        else
                                        {
                                            item.account_note = notemodel;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            int src = 0;
                            if (memberModel.EnableClubeCellar)
                                src = (int)Common.Common.ClubMembershipApi.eCellar;
                            else if (memberModel.EnableClubAMS)
                                src = (int)Common.Common.ClubMembershipApi.ams;
                            else if (memberModel.EnableClubMicroworks)
                                src = (int)Common.Common.ClubMembershipApi.microworks;
                            else if (memberModel.EnableClubCoresense)
                                src = (int)Common.Common.ClubMembershipApi.coresense;

                            userList = userDAL.GetClubMemberListBykeyword(keyword, member_id, src);
                            foreach (var item in userList)
                            {
                                item.phone_number = Utility.FormatTelephoneNumber(item.phone_number, item.address.country);
                                item.mobile_number = Utility.FormatTelephoneNumber(item.mobile_number, item.address.country);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("GetUsersByEmail API error", "keyword:" + keyword + ",MemberId:" + member_id.ToString() + ",Error:" + ex.Message + ", stacktrace:" + ex.StackTrace, "", 1, member_id);
                }
                if ((userList == null || userList.Count == 0) && ignore_local_db == false)
                {
                    userList = userDAL.GetUsersbykeyword(keyword, member_id, searchType);

                    if (userList.Count == 0)
                    {
                        userList = userDAL.GetUsersbykeyword(keyword, 0, searchType);
                    }

                    foreach (var item in userList)
                    {
                        item.phone_number = Utility.FormatTelephoneNumber(item.phone_number, item.address.country);
                        item.mobile_number = Utility.FormatTelephoneNumber(item.mobile_number, item.address.country);
                    }
                }
            }

            return userList;
        }

        #endregion

        #region UploadFile

        public static string UploadFileToStorage(byte[] source, string fileName, ImageType imageType)
        {
            string storageKey = "IE1tX/aNbqpbRlk464uZeWrmgW81aUlXDFQAFvh+yEpWoYjqQ9FVbPsVaAeGuf0Egr8znvqWlrM64yV70ebJfA==";
            //BitConverter.ToString(image);
            //string my_string = Encoding.Unicode.GetString(image, 0, image.Length);

            string folderPath = string.Empty;

            switch (imageType)
            {
                case ImageType.cpImage:
                    {
                        folderPath = "";
                        break;
                    }

                case ImageType.memberImage:
                    {
                        folderPath = "profiles";
                        break;
                    }

                case ImageType.rsvpEventImage:
                    {
                        folderPath = "events";
                        break;
                    }

                case ImageType.ticketEventImage:
                    {
                        folderPath = "tickets";
                        break;
                    }

                case ImageType.adImage:
                    {
                        folderPath = "ads";
                        break;
                    }

                case ImageType.user:
                    {
                        folderPath = "users";
                        break;
                    }

                case ImageType.ProductImage:
                    {
                        folderPath = "product_images";
                        break;
                    }
                case ImageType.ReceiptLogo:
                    {
                        folderPath = "receipt_logo";
                        break;
                    }
                case ImageType.OrderSignature:
                    {
                        folderPath = "signatures";
                        break;
                    }
                case ImageType.LocationMaps:
                    {
                        folderPath = "location_maps";
                        break;
                    }
            }
            string imgFileName = fileName;
            fileName = System.IO.Path.Combine(folderPath, fileName);

            //Uri serviceUri = StorageAccountBlobUri;

            StorageSharedKeyCredential credential = new StorageSharedKeyCredential("cdncellarpass", storageKey);
            BlobServiceClient blobServiceClient = new BlobServiceClient( new Uri("https://cdncellarpass.blob.core.windows.net"), credential);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("photos");
            bool isExist = containerClient.Exists();
            if (!isExist)
            {
                containerClient = blobServiceClient.CreateBlobContainer("photos");
            }
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            using (var mstream = new MemoryStream(source, false))
            {
                Task.Run(() => blobClient.UploadAsync(mstream, new BlobHttpHeaders { CacheControl = "max-age=86400", ContentType = "image/jpeg" })).Wait();
            }

            string logoURL = StringHelpers.GetImagePath(imageType, ImagePathType.azure) + "/" + imgFileName;

            return logoURL;
        }

        public static string UploadExportFileToStorage(byte[] source, string fileName, ImageType imageType)
        {
            string storageKey = "IE1tX/aNbqpbRlk464uZeWrmgW81aUlXDFQAFvh+yEpWoYjqQ9FVbPsVaAeGuf0Egr8znvqWlrM64yV70ebJfA==";

            string folderPath = string.Empty;

            switch (imageType)
            {
                case ImageType.RsvpExport:
                    {
                        folderPath = "rsvp_exports";
                        break;
                    }
            }

            string imgFileName = fileName;
            fileName = System.IO.Path.Combine(folderPath, fileName);

            StorageSharedKeyCredential credential = new StorageSharedKeyCredential("cdncellarpass", storageKey);
            BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri("https://cdncellarpass.blob.core.windows.net"), credential);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("admin");
            bool isExist = containerClient.Exists();
            if (!isExist)
            {
                containerClient = blobServiceClient.CreateBlobContainer("admin");
            }
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            ////Response.AddHeader("content-disposition", "attachment;filename=BookingsByDayReport_Location_" + SearchDate.ToString("MM_dd_yyyy") + ".xlsx");
            using (var mstream = new MemoryStream(source, false))
            {
                Task.Run(() => blobClient.UploadAsync(mstream, new BlobHttpHeaders { CacheControl = string.Empty, ContentType = "application/zip" })).Wait();
            }

            string logoURL = StringHelpers.GetImagePath(imageType, ImagePathType.azure) + "/" + imgFileName;

            return logoURL;
        }

        public async static Task<string> DeleteExportFileToStorage(int DeleteExportFileDays)
        {
            string storageKey = "IE1tX/aNbqpbRlk464uZeWrmgW81aUlXDFQAFvh+yEpWoYjqQ9FVbPsVaAeGuf0Egr8znvqWlrM64yV70ebJfA==";

            string folderPath = "rsvp_exports";

            StorageSharedKeyCredential credential = new StorageSharedKeyCredential("cdncellarpass", storageKey);
            BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri("https://cdncellarpass.blob.core.windows.net"), credential);
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("admin");
            bool isExist = blobContainerClient.Exists();
            if (isExist)
            {
                var blobItems = blobContainerClient.GetBlobsAsync(prefix: folderPath);
                await foreach (BlobItem blobItem in blobItems)
                {
                    var Property = blobItem.Properties;

                    TimeSpan? diff = DateTime.Today - Property.LastModified;

                    if (diff?.Days > DeleteExportFileDays)
                    {
                        BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                        await blobClient.DeleteIfExistsAsync();
                    }
                }
            }

            return "";
        }

        #endregion

        #region BigCommerce
        public async static Task<List<UserDetailModel>> GetBigCommerceCustomersByNameOrEmail(string storeId, string accessToken, int member_id, string keyword)
        {
            List<UserDetailModel> userDetails = new List<UserDetailModel>();
            var list = new SearchCustomerViewModel();
            try
            {
                BigCommerceClient client = new BigCommerceClient(storeId, accessToken);
                var searchParams = new SearchCustomerParameters();

                if (keyword.Contains("@") && keyword.Contains(".") && keyword.Length > 5)
                {
                    searchParams.email = keyword;
                }
                else
                {
                    searchParams.name = keyword;
                }

                list = client.SearchCustomerRecords(searchParams);

                if (list.status == 1)
                {
                    List<AccountTypeDiscountModel> accountTypes = new List<AccountTypeDiscountModel>();
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    accountTypes = userDAL.LoadAccountTypes(member_id, Common.Common.ThirdPartyType.BigCommerce);

                    foreach (var item in list.data)
                    {
                        UserDetailModel userDetailModel = new UserDetailModel();
                        Model.UserAddress addr = new Model.UserAddress();

                        userDetailModel.first_name = item.first_name;
                        userDetailModel.last_name = item.last_name;
                        userDetailModel.email = item.email;
                        userDetailModel.membership_number = item.id.ToString();

                        NoteList noteList = new NoteList();
                        AccountNote notemodel = new AccountNote();
                        notemodel.modified_by = "";
                        notemodel.note = "";

                        userDetailModel.account_note = notemodel;

                        userDetails.Add(userDetailModel);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return userDetails;
        }

        public async static Task<ApiResponse> GetBigCommerceAllCustomers(string storeId, string accessToken)
        {
            ApiResponse apiResponse = new ApiResponse();
            try
            {
                BigCommerceClient client = new BigCommerceClient(storeId, accessToken);

                var searchParams = new GetAllCustomersParams();

                searchParams.page = 1;

                apiResponse = client.GetAllCustomers(searchParams);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return apiResponse;
        }

        public async static Task<ProductList> GetBigCommerceProducts(string storeId, string accessToken, string keyword, int member_id)
        {
            ProductList list = new ProductList();
            try
            {
                BigCommerceClient client = new BigCommerceClient(storeId, accessToken);

                var getAllProductsParams = new GetAllProductsParams();

                getAllProductsParams.keyword = keyword;
                getAllProductsParams.page = 1;

                list = client.GetAllProducts(getAllProductsParams);
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("GetBigCommerceProducts", "keyword: " + keyword + ", Error: " + ex.Message, "", 1, member_id);
            }

            return list;
        }

        public async static Task<string> BigCommerceCreateOrder(string storeId, string accessToken, ReservationDetailModel reservationDetailModel, string CustId, string processed_by = "")
        {
            string OrderId = string.Empty;
            ApiResponse apiResponse = new ApiResponse();
            try
            {
                BigCommerceClient client = new BigCommerceClient(storeId, accessToken);
                CreateOrderModel model = new CreateOrderModel();
                CreateOrderBillingAddress billing_address = new CreateOrderBillingAddress();
                List<CreateOrderShippingAddress> shipping_addresses = new List<CreateOrderShippingAddress>();
                List<CreateOrderProduct> products = new List<CreateOrderProduct>();

                model.customer_id = Convert.ToInt32(CustId);

                billing_address.first_name = reservationDetailModel.user_detail.first_name;
                billing_address.last_name = reservationDetailModel.user_detail.last_name;

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var memberModel = eventDAL.GetWineryById(reservationDetailModel.member_id);

                billing_address.street_1 = reservationDetailModel.user_detail.address.address_1;
                billing_address.city = reservationDetailModel.user_detail.address.city;
                billing_address.state = reservationDetailModel.user_detail.address.state;
                billing_address.zip = reservationDetailModel.user_detail.address.zip_code;

                if (string.IsNullOrWhiteSpace(billing_address.street_1) || string.IsNullOrWhiteSpace(billing_address.city) || string.IsNullOrWhiteSpace(billing_address.state))
                {
                    billing_address.state = memberModel.WineryAddress.state;
                    billing_address.zip = memberModel.WineryAddress.zip_code;
                    billing_address.city = memberModel.WineryAddress.city;
                    billing_address.street_1 = memberModel.WineryAddress.address_1;
                }
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                billing_address.state = userDAL.GetStateBystatecode(billing_address.state);
                billing_address.country = "United States";
                billing_address.email = reservationDetailModel.user_detail.email;


                CreateOrderShippingAddress shipping_address = new CreateOrderShippingAddress();

                shipping_address.first_name = reservationDetailModel.user_detail.first_name;
                shipping_address.last_name = reservationDetailModel.user_detail.last_name;
                shipping_address.company = "";
                shipping_address.street_1 = billing_address.street_1;
                shipping_address.city = billing_address.city;
                shipping_address.state = billing_address.state;
                shipping_address.zip = billing_address.zip;
                shipping_address.country = "United States";
                shipping_address.email = reservationDetailModel.user_detail.email;

                shipping_addresses.Add(shipping_address);

                CreateOrderProduct product = new CreateOrderProduct();

                product.name = reservationDetailModel.event_name;
                product.quantity = reservationDetailModel.total_guests;
                product.price_inc_tax = reservationDetailModel.fee_due;
                product.price_ex_tax = reservationDetailModel.fee_due - reservationDetailModel.sales_tax;

                products.Add(product);


                model.billing_address = billing_address;
                model.shipping_addresses = shipping_addresses;
                model.products = products;

                apiResponse = client.CreateOrder(model);

                if (apiResponse.status == 1)
                {
                    //dynamic data =  JObject.Parse(apiResponse.data.ToString());
                    OrderId = (string)JObject.Parse(apiResponse.data.ToString()).SelectToken("id");

                    eventDAL.InsertExportLog(8, reservationDetailModel.reservation_id, "Success- " + OrderId, 1, reservationDetailModel.amount_paid, processed_by);
                    var reservation = eventDAL.GetReservationDetailsbyReservationId(reservationDetailModel.reservation_id);
                    eventDAL.ReservationV2StatusNote_Create(reservationDetailModel.reservation_id, reservation.status, reservation.member_id, "", false, 0, 0, 0, "SYNC - Order upserted to BigCommerce");
                }
                else
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                    logDAL.InsertLog("BigCommerceCreateOrder", "RsvpId: " + reservationDetailModel.reservation_id.ToString() + ", Response: " + apiResponse.response.ToString(), "", 1, 0);
                }

            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("BigCommerceCreateOrder", "RsvpId: " + reservationDetailModel.reservation_id.ToString() + ", Response: " + ex.Message, "", 1, 0);
            }

            return OrderId;
        }

        public async static Task<string> BigCommerceCreateCustomers(string storeId, string accessToken, List<CreateCustomers> list)
        {
            string CustId = string.Empty;

            ApiResponse apiResponse = new ApiResponse();
            try
            {
                BigCommerceClient client = new BigCommerceClient(storeId, accessToken);

                apiResponse = client.CreateCustomers(list);

                if (apiResponse.status == 1)
                {
                    var userList = new List<UserDetailModel>();
                    userList = await Task.Run(() => Utility.GetBigCommerceCustomersByNameOrEmail(storeId, accessToken, 0, list[0].email));

                    if (userList != null && userList.Count > 0)
                        CustId = userList[0].membership_number;
                    //CustId = (string)JObject.Parse(apiResponse.data.ToString()).SelectToken("id");
                }
                else
                {
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                    logDAL.InsertLog("BigCommerceCreateCustomers", "Email: " + list[0].email + ", Response: " + apiResponse.response.ToString(), "", 1, 0);
                }
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("BigCommerceCreateCustomers", "Email: " + list[0].email + ", Errors:" + ex.Message, "", 1, 0);
            }

            return CustId;
        }

        public async static Task<string> CheckAndUpdateBigCommerceCustomer(WineryModel memberModel, int memberId, string email)
        {
            string CustId = string.Empty;
            int user_id = 0;
            var userList = new List<UserDetailModel>();
            userList = await Task.Run(() => Utility.GetBigCommerceCustomersByNameOrEmail(memberModel.BigCommerceStoreId, memberModel.BigCommerceAceessToken, memberId, email));

            if (userList != null && userList.Count > 0)
                CustId = userList[0].membership_number;

            if (string.IsNullOrEmpty(CustId))
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                List<CreateCustomers> list = new List<CreateCustomers>();

                var createCustomer = new CreateCustomers();

                List<CreateCustomerAddress> addList = new List<CreateCustomerAddress>();

                UserDetailModel user = userDAL.GetUserDetailsbyemail(email, memberId);

                user_id = user.user_id;

                createCustomer.email = user.email;
                createCustomer.first_name = user.first_name;
                createCustomer.last_name = user.first_name;
                createCustomer.company = "";
                createCustomer.phone = user.phone_number.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
                createCustomer.notes = "";

                var add = new CreateCustomerAddress();
                add.address1 = user.address.address_1;
                add.address2 = user.address.address_2;
                add.address_type = "residential";
                add.city = user.address.city;
                add.company = "";
                add.country_code = user.address.country;
                add.first_name = user.first_name;
                add.last_name = user.last_name;
                add.phone = user.phone_number.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
                add.postal_code = user.address.zip_code;
                if ((string.IsNullOrWhiteSpace(user.address.state) || string.IsNullOrWhiteSpace(user.address.country)) || string.IsNullOrWhiteSpace(add.postal_code) || string.IsNullOrWhiteSpace(add.address1) || string.IsNullOrWhiteSpace(add.city))
                {
                    var userAddr = userDAL.GetUserAddressByZipCode(add.postal_code);
                    add.state_or_province = userDAL.GetStateBystatecode(userAddr.state);
                    add.country_code = memberModel.WineryAddress.country;
                    add.city = userAddr.city;
                    add.address1 = memberModel.WineryAddress.address_1;
                }
                else
                    add.state_or_province = userDAL.GetStateBystatecode(user.address.state);

                if ((string.IsNullOrWhiteSpace(user.address.state) || string.IsNullOrWhiteSpace(user.address.country)) || string.IsNullOrWhiteSpace(add.postal_code) || string.IsNullOrWhiteSpace(add.address1) || string.IsNullOrWhiteSpace(add.city))
                {
                    add.state_or_province = userDAL.GetStateBystatecode(memberModel.WineryAddress.state);
                    add.country_code = memberModel.WineryAddress.country;
                    add.postal_code = memberModel.WineryAddress.zip_code;
                    add.city = memberModel.WineryAddress.city;
                    add.address1 = memberModel.WineryAddress.address_1;
                }

                addList.Add(add);

                createCustomer.addresses = addList;

                list.Add(createCustomer);

                CustId = await Utility.BigCommerceCreateCustomers(memberModel.BigCommerceStoreId, memberModel.BigCommerceAceessToken, list);
            }

            if (!string.IsNullOrEmpty(CustId))
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                userDAL.UpdateUserWinery(memberId, user_id, CustId);
            }

            return CustId;
        }

        #endregion

        #region ExportReservation

        public static string ReservationDetailedExport(int WineryId, DateTime SearchDate)
        {
            string Url = string.Empty;

            string filename = WineryId.ToString() + "_BookingsByDayReport_Det_" + SearchDate.ToString("MM_dd_yyyy") + ".xlsx";

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            List<GetReservationV2ByWineryIdResult> arrDataV2 = eventDAL.GetReservationV2ByWineryId(WineryId, SearchDate);

            DateTime StartDate = DateTime.Now;
            var exportResult = arrDataV2.OrderBy(f => DateTime.Parse(StartDate.ToShortDateString() + " " + f.EventTime)).GroupBy(v => new { v.start, v.EventName, v.EventTime, v.MaxPersons, v.Guests }).Select(x => x.FirstOrDefault()).ToList();

            string str = "";
            string StrDate = SearchDate.ToString("dddd, MMMM-dd-yyyy");
            int RowCount = 4;

            using (XLWorkbook wb = new XLWorkbook())
            {
                IXLWorksheet workSheet = wb.Worksheets.Add("Bookings_Report");

                workSheet.Range("A1:M1").Merge();
                workSheet.Range("A2:M2").Merge();

                workSheet.Cell("A1").Value = "Booking By Day Report- Detailed";
                workSheet.Cell("A2").Style.NumberFormat.Format = "dddd, MMMM-dd-yyyy";
                workSheet.Cell("A2").Value = StrDate;

                workSheet.Cell("B3").Value = "Guest Last";
                workSheet.Cell("C3").Value = "Guest First";
                workSheet.Cell("D3").Value = "Guest Phone";
                workSheet.Cell("E3").Value = "Guest#";
                workSheet.Cell("F3").Value = "Event$";
                workSheet.Cell("G3").Value = "Discount";
                workSheet.Cell("H3").Value = "Total";
                workSheet.Cell("I3").Value = "Type";
                workSheet.Cell("J3").Value = "Date Booked";
                workSheet.Cell("K3").Value = "Referred By";
                workSheet.Cell("L3").Value = "Guest Note";
                workSheet.Cell("M3").Value = "How Did You Hear?";

                foreach (var item in exportResult)
                {
                    str = item.EventTime + "~ " + item.Guests + " of " + item.MaxPersons + "~" + item.EventName;
                    str = str.Replace('~', '\t');
                    str = TabsToSpaces(str, 20);

                    workSheet.Range("B" + RowCount + ":" + "M" + RowCount).Merge();
                    workSheet.Cell("B" + RowCount).Value = str;
                    workSheet.Cell("B" + RowCount).Style.Font.Bold = true;

                    if (!string.IsNullOrEmpty(item.LastName))
                    {
                        RowCount += 1;
                        workSheet.Cell("B" + RowCount).Value = item.LastName;
                        workSheet.Cell("C" + RowCount).Value = item.FirstName;
                        workSheet.Cell("D" + RowCount).Value = Utility.FormatPhoneNumberDecimal(Utility.ExtractPhone(item.PhoneNum));
                        workSheet.Cell("E" + RowCount).Value = item.TotalGuests;
                        workSheet.Cell("F" + RowCount).Value = item.FeePerPerson;
                        workSheet.Cell("G" + RowCount).Value = item.Discount;
                        workSheet.Cell("H" + RowCount).Value = item.PurchaseTotal;
                        workSheet.Cell("I" + RowCount).Value = item.AccountType;
                        workSheet.Cell("J" + RowCount).Value = item.BookingDate;
                        workSheet.Cell("L" + RowCount).Value = item.InternalNote;
                        workSheet.Cell("L" + RowCount).Style.Alignment.WrapText = true;

                        var userList = arrDataV2.Where(f => f.LastName != item.LastName & f.LastName.Length > 0 & f.start == item.start & f.EventName == item.EventName & f.EventTime == item.EventTime & f.MaxPersons == item.MaxPersons & f.Guests == item.Guests);

                        foreach (var item1 in userList)
                        {
                            RowCount += 1;
                            workSheet.Cell("B" + RowCount).Value = item1.LastName;
                            workSheet.Cell("C" + RowCount).Value = item1.FirstName;
                            workSheet.Cell("D" + RowCount).Value = Utility.FormatPhoneNumberDecimal(Utility.ExtractPhone(item1.PhoneNum));
                            workSheet.Cell("E" + RowCount).Value = item1.TotalGuests;
                            workSheet.Cell("F" + RowCount).Value = item1.FeePerPerson;
                            workSheet.Cell("G" + RowCount).Value = item1.Discount;
                            workSheet.Cell("H" + RowCount).Value = item1.PurchaseTotal;
                            workSheet.Cell("I" + RowCount).Value = item1.AccountType;
                            workSheet.Cell("J" + RowCount).Value = item1.BookingDate;
                            workSheet.Cell("L" + RowCount).Value = item1.InternalNote;
                            workSheet.Cell("L" + RowCount).Style.Alignment.WrapText = true;
                        }
                    }
                    RowCount += 5;
                }
                workSheet.Range("A1:M" + RowCount).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                workSheet.Range("A1:M" + RowCount).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                workSheet.Columns("B", "M").AdjustToContents();
                workSheet.Range("A1:A2").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                workSheet.Range("B3:M3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                workSheet.Range("B3:M3").Style.Font.Bold = true;
                workSheet.Cell("A1").Style.Font.Bold = true;

                workSheet.Columns("A").Width = 3;

                var workbookBytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    workbookBytes = ms.ToArray();
                    ms.Flush();
                    ms.Position = 0;
                }

                var zipworkbookBytes = new byte[0];
                zipworkbookBytes = CompressExportFileToZip(workbookBytes, filename);
                filename = filename.Replace(".xlsx", ".zip");

                Url = UploadExportFileToStorage(zipworkbookBytes, filename, ImageType.RsvpExport);
            }
            return Url;
        }

        public static string ReservationSimplifiedExport(int WineryId, DateTime SearchDate)
        {
            //Export_Reservation_Detail.aspx

            string Url = string.Empty;

            string filename = WineryId.ToString() + "_BookingsByDayReport_" + SearchDate.ToString("MM_dd_yyyy") + ".xlsx";

            //Response.ClearHeaders();
            //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.AddHeader("Cache-Control", string.Empty);
            //Response.AddHeader("content-disposition", "attachment;filename=BookingsByDayReport_" + SearchDate.ToString("MM_dd_yyyy") + ".xlsx");

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            List<GetReservationV2ByWineryId_DetailResult> arrDataV2 = eventDAL.GetReservationV2ByWineryId_Detail(WineryId, SearchDate);

            DateTime StartDate = DateTime.Now;
            var exportResult = arrDataV2.OrderBy(f => f.LocationName).ThenBy(f => DateTime.Parse(StartDate.ToShortDateString() + " " + f.EventTime)).GroupBy(v => new { v.LocationName, v.start, v.EventName, v.EventTime, v.MaxPersons, v.Guests }).Select(x => x.FirstOrDefault()).ToList();

            string StrDate = SearchDate.ToString("dddd, MMMM-dd-yyyy");
            string str = "";
            int RowCount = 4;

            using (XLWorkbook wb = new XLWorkbook())
            {
                IXLWorksheet workSheet = wb.Worksheets.Add("Bookings_Report");

                workSheet.Range("A1:I1").Merge();
                workSheet.Range("A2:I2").Merge();

                workSheet.Cell("A1").Value = "Booking By Day Report";
                workSheet.Cell("A2").Style.NumberFormat.Format = "dddd, MMMM-dd-yyyy";
                workSheet.Cell("A2").Value = StrDate;

                workSheet.Cell("B3").Value = "Guest Name";
                workSheet.Cell("C3").Value = "Guest Phone";
                workSheet.Cell("D3").Value = "Guest#";
                workSheet.Cell("E3").Value = "Fee$";
                workSheet.Cell("F3").Value = "Customer Type";
                workSheet.Cell("G3").Value = "Referred By";
                workSheet.Cell("H3").Value = "Internal Notes";
                workSheet.Cell("I3").Value = "Guest Note";

                foreach (var item in exportResult)
                {
                    str = item.EventTime + "~ " + item.Guests + " of " + item.MaxPersons + "~" + item.LocationName + ", " + item.EventName;
                    str = str.Replace('~', '\t');
                    str = TabsToSpaces(str, 20);

                    workSheet.Range("B" + RowCount + ":" + "I" + RowCount).Merge();
                    workSheet.Cell("B" + RowCount).Value = str;
                    workSheet.Cell("B" + RowCount).Style.Font.Bold = true;

                    if (!string.IsNullOrEmpty(item.GuestName))
                    {
                        RowCount += 1;
                        workSheet.Cell("B" + RowCount).Value = item.GuestName;
                        workSheet.Cell("B" + RowCount).Style.Alignment.WrapText = true;
                        workSheet.Cell("C" + RowCount).Value = item.PhoneNum;
                        workSheet.Cell("D" + RowCount).Value = item.TotalGuests;
                        workSheet.Cell("E" + RowCount).Value = item.FeePerPerson;
                        workSheet.Cell("F" + RowCount).Value = item.AccountType;
                        workSheet.Cell("G" + RowCount).Value = item.ReferredBy;
                        workSheet.Cell("H" + RowCount).Value = item.InternalNote;
                        workSheet.Cell("H" + RowCount).Style.Alignment.WrapText = true;
                        workSheet.Cell("I" + RowCount).Value = item.GuestNote;
                        workSheet.Cell("I" + RowCount).Style.Alignment.WrapText = true;

                        var userList = arrDataV2.Where(f => f.GuestName != item.GuestName & f.GuestName.Length > 0 & f.start == item.start & f.EventName == item.EventName & f.LocationName == item.LocationName & f.EventTime == item.EventTime & f.MaxPersons == item.MaxPersons & f.Guests == item.Guests);
                        foreach (var item1 in userList)
                        {
                            RowCount += 1;
                            workSheet.Cell("B" + RowCount).Value = item1.GuestName;
                            workSheet.Cell("B" + RowCount).Style.Alignment.WrapText = true;
                            workSheet.Cell("C" + RowCount).Value = item1.PhoneNum;
                            workSheet.Cell("D" + RowCount).Value = item1.TotalGuests;
                            workSheet.Cell("E" + RowCount).Value = item1.FeePerPerson;
                            workSheet.Cell("F" + RowCount).Value = item1.AccountType;
                            workSheet.Cell("G" + RowCount).Value = item1.ReferredBy;
                            workSheet.Cell("H" + RowCount).Value = item1.InternalNote;
                            workSheet.Cell("H" + RowCount).Style.Alignment.WrapText = true;
                            workSheet.Cell("I" + RowCount).Value = item1.GuestNote;
                            workSheet.Cell("I" + RowCount).Style.Alignment.WrapText = true;
                        }

                    }
                    RowCount += 5;
                }

                workSheet.Range("A1:I" + RowCount).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                workSheet.Range("A1:I" + RowCount).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                workSheet.Columns("B", "I").AdjustToContents();
                workSheet.Range("A1:A2").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                workSheet.Range("B3:I3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                workSheet.Range("B3:I3").Style.Font.Bold = true;
                workSheet.Cell("A1").Style.Font.Bold = true;

                workSheet.Columns("A").Width = 3;

                var workbookBytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    workbookBytes = ms.ToArray();
                    ms.Flush();
                    ms.Position = 0;
                }

                var zipworkbookBytes = new byte[0];
                zipworkbookBytes = CompressExportFileToZip(workbookBytes, filename);
                filename = filename.Replace(".xlsx", ".zip");

                Url = UploadExportFileToStorage(zipworkbookBytes, filename, ImageType.RsvpExport);
            }
            return Url;
        }

        public static string ReservationByLocationExport(int WineryId, DateTime SearchDate)
        {
            //Export_Reservation_Location.aspx

            string Url = string.Empty;

            string filename = WineryId.ToString() + "_BookingsByDayReport_Location_" + SearchDate.ToString("MM_dd_yyyy") + ".xlsx";

            //Response.ClearHeaders();
            //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.AddHeader("Cache-Control", string.Empty);
            //Response.AddHeader("content-disposition", "attachment;filename=BookingsByDayReport_Location_" + SearchDate.ToString("MM_dd_yyyy") + ".xlsx");

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            List<GetReservationV2ByWineryId_DetailResult> arrDataV2 = eventDAL.GetReservationV2ByWineryId_Detail(WineryId, SearchDate);

            DateTime StartDate = DateTime.Now;
            var exportResult = arrDataV2.OrderBy(f => f.LocationName).ThenBy(f => DateTime.Parse(StartDate.ToShortDateString() + " " + f.EventTime)).GroupBy(v => new { v.LocationName, v.start, v.EventName, v.EventTime, v.MaxPersons, v.Guests }).Select(x => x.FirstOrDefault()).ToList();

            string StrDate = SearchDate.ToString("dddd, MMMM-dd-yyyy");
            string str = "";
            int RowCount = 4;
            string InternalNote = "";
            string GuestNote = "";
            string Comma = "";

            using (XLWorkbook wb = new XLWorkbook())
            {
                IXLWorksheet workSheet = wb.Worksheets.Add("Bookings_Report");

                workSheet.Range("A1:G1").Merge();
                workSheet.Range("A2:G2").Merge();

                workSheet.Cell("A1").Value = "Booking By Day Report- by Location";
                workSheet.Cell("A2").Style.NumberFormat.Format = "dddd, MMMM-dd-yyyy";
                workSheet.Cell("A2").Value = StrDate;

                workSheet.Cell("B3").Value = "Guest Name";
                workSheet.Cell("C3").Value = "Guest Phone";
                workSheet.Cell("D3").Value = "Guest#";
                workSheet.Cell("E3").Value = "Order$";
                workSheet.Cell("F3").Value = "Customer Type";
                workSheet.Cell("G3").Value = "Internal/Guest Notes";

                foreach (var item in exportResult)
                {
                    str = item.EventTime + "~ " + item.Guests + " of " + item.MaxPersons + "~" + item.LocationName + ", " + item.EventName;
                    str = str.Replace('~', '\t');
                    str = TabsToSpaces(str, 20);

                    workSheet.Range("B" + RowCount + ":" + "G" + RowCount).Merge();
                    workSheet.Cell("B" + RowCount).Value = str;
                    workSheet.Cell("B" + RowCount).Style.Font.Bold = true;

                    if (!string.IsNullOrEmpty(item.GuestName))
                    {
                        RowCount += 1;
                        InternalNote = "";
                        GuestNote = "";
                        Comma = "";

                        if (!string.IsNullOrEmpty(item.InternalNote))
                        {
                            InternalNote = "Internal Notes: " + item.InternalNote.Trim();
                        }

                        if (!string.IsNullOrEmpty(item.GuestNote))
                        {
                            GuestNote = "Guest Notes: " + item.GuestNote.Trim();
                        }

                        if (!string.IsNullOrEmpty(item.GuestNote) && !string.IsNullOrEmpty(item.InternalNote))
                        {
                            Comma = ", ";
                        }

                        workSheet.Cell("B" + RowCount).Value = item.GuestName;
                        workSheet.Cell("B" + RowCount).Style.Alignment.WrapText = true;
                        workSheet.Cell("C" + RowCount).Value = Utility.FormatPhoneNumberDecimal(Utility.ExtractPhone(item.PhoneNum));
                        workSheet.Cell("D" + RowCount).Value = item.TotalGuests;

                        if (item.FeeDue > 0 && item.BalanceDue == 0)
                        {
                            workSheet.Cell("E" + RowCount).Value = item.FeeDue.ToString() + " PAID";
                        }
                        else
                        {
                            workSheet.Cell("E" + RowCount).Value = item.FeeDue;
                        }

                        workSheet.Cell("E" + RowCount).Style.NumberFormat.Format = "##0.00";

                        if (item.ContactTypes.Trim().Length > 0)
                        {
                            workSheet.Cell("F" + RowCount).Value = item.ContactTypes;
                        }
                        else
                        {
                            workSheet.Cell("F" + RowCount).Value = item.AccountType;
                        }

                        workSheet.Cell("F" + RowCount).Style.Alignment.WrapText = true;

                        workSheet.Cell("G" + RowCount).Value = InternalNote + Comma + GuestNote;
                        workSheet.Cell("G" + RowCount).Style.Alignment.WrapText = true;

                        var userList = arrDataV2.Where(f => f.Email != item.Email & f.Email.Length > 0 & f.LocationName == item.LocationName & f.start == item.start & f.EventName == item.EventName & f.EventTime == item.EventTime & f.MaxPersons == item.MaxPersons & f.Guests == item.Guests);
                        foreach (var item1 in userList)
                        {
                            RowCount += 1;
                            InternalNote = "";
                            GuestNote = "";
                            Comma = "";

                            if (!string.IsNullOrEmpty(item1.InternalNote))
                            {
                                InternalNote = "Internal Notes: " + item1.InternalNote.Trim();
                            }

                            if (!string.IsNullOrEmpty(item1.GuestNote))
                            {
                                GuestNote = "Guest Notes: " + item1.GuestNote.Trim();
                            }

                            if (!string.IsNullOrEmpty(item1.GuestNote) && !string.IsNullOrEmpty(item1.InternalNote))
                            {
                                Comma = ", ";
                            }

                            workSheet.Cell("B" + RowCount).Value = item1.GuestName;
                            workSheet.Cell("B" + RowCount).Style.Alignment.WrapText = true;
                            workSheet.Cell("C" + RowCount).Value = Utility.FormatPhoneNumberDecimal(Utility.ExtractPhone(item1.PhoneNum));
                            workSheet.Cell("D" + RowCount).Value = item1.TotalGuests;

                            if (item1.FeeDue > 0 && item1.BalanceDue == 0)
                            {
                                workSheet.Cell("E" + RowCount).Value = item1.FeeDue.ToString() + " PAID";
                            }
                            else
                            {
                                workSheet.Cell("E" + RowCount).Value = item1.FeeDue;
                            }

                            workSheet.Cell("E" + RowCount).Style.NumberFormat.Format = "##0.00";

                            if (item1.ContactTypes.Trim().Length > 0)
                            {
                                workSheet.Cell("F" + RowCount).Value = item1.ContactTypes;
                            }
                            else
                            {
                                workSheet.Cell("F" + RowCount).Value = item1.AccountType;
                            }

                            workSheet.Cell("F" + RowCount).Style.Alignment.WrapText = true;

                            workSheet.Cell("G" + RowCount).Value = InternalNote + Comma + GuestNote;
                            workSheet.Cell("G" + RowCount).Style.Alignment.WrapText = true;
                        }

                    }
                    RowCount += 5;
                }

                workSheet.Range("A1:G" + RowCount).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                workSheet.Range("A1:G" + RowCount).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                workSheet.Columns("B", "G").AdjustToContents();
                workSheet.Range("A1:A2").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                workSheet.Range("B3:G3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                workSheet.Range("B3:G3").Style.Font.Bold = true;
                workSheet.Cell("A1").Style.Font.Bold = true;

                workSheet.Columns("A").Width = 3;
                workSheet.Columns("B").Width = 19;
                workSheet.Columns("C").Width = 15.5;
                workSheet.Columns("D").Width = 7.4;
                workSheet.Columns("E").Width = 11.2;
                workSheet.Columns("F").Width = 16.85;
                workSheet.Columns("G").Width = 35.5;

                var workbookBytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    workbookBytes = ms.ToArray();
                    ms.Flush();
                    ms.Position = 0;
                }

                var zipworkbookBytes = new byte[0];
                zipworkbookBytes = CompressExportFileToZip(workbookBytes, filename);
                filename = filename.Replace(".xlsx", ".zip");

                Url = UploadExportFileToStorage(zipworkbookBytes, filename, ImageType.RsvpExport);
            }
            return Url;
        }

        public static string FinancialV2ReportByMonth(int WineryId, DateTime SearchDate)
        {
            //Export_Reservation_Location.aspx

            string Url = string.Empty;

            string filename = WineryId.ToString() + "_FinancialReport_" + SearchDate.ToString("MM_dd_yyyy") + ".xlsx";

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            Times.TimeZone timeZone = (Times.TimeZone)eventDAL.GetTimeZonebyWineryId(WineryId);
            int offsetMinutes = Convert.ToInt32(Times.GetOffsetMinutes(timeZone));

            List<GetFinancialV2ReportByMonthResult> arrDataV2 = eventDAL.GetFinancialV2ReportByMonth(WineryId, SearchDate, SearchDate.AddMonths(1).AddDays(-1), offsetMinutes);

            int RowCount = 2;

            using (XLWorkbook wb = new XLWorkbook())
            {
                IXLWorksheet workSheet = wb.Worksheets.Add("Financial_Report");

                workSheet.Range("A1:K1").Merge();

                workSheet.Cell("A1").Value = "Transaction Report (By TXN Date) - " + SearchDate.ToString("MM/dd/yyyy") + " - " + SearchDate.AddMonths(1).AddDays(-1).ToString("MM/dd/yyyy");

                workSheet.Cell("A2").Value = "TXN Date";
                workSheet.Cell("B2").Value = "Date Booked";
                workSheet.Cell("C2").Value = "Event Date";
                workSheet.Cell("D2").Value = "TXN Type";
                workSheet.Cell("E2").Value = "Confirm #";
                workSheet.Cell("F2").Value = "Name";
                workSheet.Cell("G2").Value = "Approval";
                workSheet.Cell("H2").Value = "Transaction ID";
                workSheet.Cell("I2").Value = "Card";
                workSheet.Cell("J2").Value = "Amount$";
                workSheet.Cell("K2").Value = "eCOM Sync?";

                foreach (var item in arrDataV2)
                {
                    RowCount += 1;

                    workSheet.Cell("A" + RowCount).Value = item.TXNDate.ToString("MM/dd/yyyy");
                    workSheet.Cell("B" + RowCount).Value = item.DateBooked.ToString("MM/dd/yyyy");
                    workSheet.Cell("C" + RowCount).Value = item.TXNDate.ToString("MM/dd/yyyy");
                    workSheet.Cell("D" + RowCount).Value = item.TXNType;
                    workSheet.Cell("E" + RowCount).Value = item.ConfirmNum;
                    workSheet.Cell("F" + RowCount).Value = item.UserName;
                    workSheet.Cell("G" + RowCount).Value = item.Approval;
                    workSheet.Cell("H" + RowCount).Value = item.TransactionID;

                    if (!string.IsNullOrEmpty(item.PayCardNumber))
                    {
                        string ccNum = StringHelpers.Decrypt(item.PayCardNumber);
                        if (ccNum.Length>0)
                            workSheet.Cell("I" + RowCount).Value = item.PayCardType + "- " + ccNum.Substring(ccNum.Length - 4);
                        else
                            workSheet.Cell("I" + RowCount).Value = item.PayCardType;
                    }
                    else
                    {
                        workSheet.Cell("I" + RowCount).Value = item.PayCardType;
                    }

                    workSheet.Cell("J" + RowCount).Value = item.Paidamt;
                    workSheet.Cell("J" + RowCount).Style.NumberFormat.Format = "0.00";
                    workSheet.Cell("K" + RowCount).Value = item.ExportType;
                }

                workSheet.Range("A1:K" + RowCount).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                workSheet.Range("A1:K" + RowCount).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                workSheet.Columns("A", "K").AdjustToContents();
                workSheet.Range("A2:K2").Style.Font.Bold = true;
                workSheet.Cell("A1").Style.Font.Bold = true;
                workSheet.Cell("A1").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                workSheet.Range("A2:K2").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                workSheet.Cell("A2").Style.Font.Bold = true;

                var workbookBytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    workbookBytes = ms.ToArray();
                    ms.Flush();
                    ms.Position = 0;
                }

                var zipworkbookBytes = new byte[0];
                zipworkbookBytes = CompressExportFileToZip(workbookBytes, filename);
                filename = filename.Replace(".xlsx", ".zip");

                Url = UploadExportFileToStorage(zipworkbookBytes, filename, ImageType.RsvpExport);
            }
            return Url;
        }

        public static string TransactionReportByMonth(int WineryId, DateTime SearchDate)
        {
            //Export_TransactionReport.aspx

            string Url = string.Empty;

            string filename = WineryId.ToString() + "_TransactionReport_" + SearchDate.ToString("MM_dd_yyyy") + ".xlsx";

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            Times.TimeZone timeZone = (Times.TimeZone)eventDAL.GetTimeZonebyWineryId(WineryId);
            int offsetMinutes = Convert.ToInt32(Times.GetOffsetMinutes(timeZone));

            List<GetTransactionReportByMonthResult> arrDataV2 = eventDAL.GetTransactionReportByMonth(WineryId, SearchDate, SearchDate.AddMonths(1).AddDays(-1), offsetMinutes);

            int RowCount = 2;

            using (XLWorkbook wb = new XLWorkbook())
            {
                IXLWorksheet workSheet = wb.Worksheets.Add("Transaction_Report");

                workSheet.Range("A1:N1").Merge();

                workSheet.Cell("A1").Value = "Transaction Report (By TXN Date) - " + SearchDate.ToString("MM/dd/yyyy") + " - " + SearchDate.AddMonths(1).AddDays(-1).ToString("MM/dd/yyyy");

                workSheet.Cell("A2").Value = "ConfirmID";
                workSheet.Cell("B2").Value = "Event Date";
                workSheet.Cell("C2").Value = "Guest";
                workSheet.Cell("D2").Value = "SKU";
                workSheet.Cell("E2").Value = "Product";
                workSheet.Cell("F2").Value = "Product Type";
                workSheet.Cell("G2").Value = "Qty";
                workSheet.Cell("H2").Value = "Unit Price";
                workSheet.Cell("I2").Value = "Extended";
                workSheet.Cell("J2").Value = "Txn Date";
                workSheet.Cell("K2").Value = "Sales Tax";
                workSheet.Cell("L2").Value = "Gratuity Amount";
                workSheet.Cell("M2").Value = "TransID";
                workSheet.Cell("N2").Value = "TenderType";

                foreach (var item in arrDataV2)
                {
                    RowCount += 1;

                    workSheet.Cell("A" + RowCount).Value = item.BookingCode;
                    workSheet.Cell("B" + RowCount).Value = item.EventDate.ToString("MM/dd/yyyy");
                    workSheet.Cell("C" + RowCount).Value = item.UserName;
                    workSheet.Cell("D" + RowCount).Value = item.SKU;
                    workSheet.Cell("E" + RowCount).Value = item.ProductName;
                    workSheet.Cell("F" + RowCount).Value = item.ProductType;
                    workSheet.Cell("G" + RowCount).Value = item.Qty;
                    workSheet.Cell("H" + RowCount).Value = item.Price;
                    workSheet.Cell("H" + RowCount).Style.NumberFormat.Format = "0.00";
                    workSheet.Cell("I" + RowCount).Value = item.Extended;
                    workSheet.Cell("I" + RowCount).Style.NumberFormat.Format = "0.00";
                    workSheet.Cell("J" + RowCount).Value = item.TransactionDate.ToString("MM/dd/yyyy");
                    workSheet.Cell("K" + RowCount).Value = item.SalesTax;
                    workSheet.Cell("K" + RowCount).Style.NumberFormat.Format = "0.00";
                    workSheet.Cell("L" + RowCount).Value = item.GratuityAmount;
                    workSheet.Cell("L" + RowCount).Style.NumberFormat.Format = "0.00";
                    workSheet.Cell("M" + RowCount).Value = item.TransID;
                    workSheet.Cell("N" + RowCount).Value = item.TenderType;
                }

                workSheet.Range("A1:N" + RowCount).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                workSheet.Range("A1:N" + RowCount).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                workSheet.Columns("A", "N").AdjustToContents();
                workSheet.Range("A2:N2").Style.Font.Bold = true;
                workSheet.Cell("A1").Style.Font.Bold = true;
                workSheet.Cell("A1").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                workSheet.Range("A2:N2").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                workSheet.Cell("A2").Style.Font.Bold = true;

                var workbookBytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    workbookBytes = ms.ToArray();
                    ms.Flush();
                    ms.Position = 0;
                }

                var zipworkbookBytes = new byte[0];
                zipworkbookBytes = CompressExportFileToZip(workbookBytes, filename);
                filename = filename.Replace(".xlsx", ".zip");

                Url = UploadExportFileToStorage(zipworkbookBytes, filename, ImageType.RsvpExport);
            }
            return Url;
        }

        public static string TicketHoldersExport(int WineryId, int EventId,bool Passport)
        {
            //LoadTicketHolders

            string Url = string.Empty;

            string filename = string.Format("{0}_Ticketholders_{1}",EventId,DateTime.Now.ToString("MM_dd_yyyy")) + ".xlsx";

            TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            Times.TimeZone timeZone = (Times.TimeZone)eventDAL.GetTimeZonebyWineryId(WineryId);
            int offsetMinutes = Convert.ToInt32(Times.GetOffsetMinutes(timeZone));

            List<TicketsByEventExport> arrDataV2 = new List<TicketsByEventExport>();

            if (Passport)
                arrDataV2 = ticketDAL.GetPassportTicketsByEvent(EventId, offsetMinutes, WineryId);
            else
                arrDataV2 = ticketDAL.GetTicketsByEvent(EventId, offsetMinutes);

            int RowCount = 1;

            using (XLWorkbook wb = new XLWorkbook())
            {
                IXLWorksheet workSheet = wb.Worksheets.Add("Ticket_Holders_Export");

                workSheet.Cell("A1").Value = "Ticket Id";
                workSheet.Cell("B1").Value = "Ticket Level";
                workSheet.Cell("C1").Value = "Delivery Method";
                workSheet.Cell("D1").Value = "Ticket Status";
                workSheet.Cell("E1").Value = "Order Date";
                workSheet.Cell("F1").Value = "Order #";
                workSheet.Cell("G1").Value = "Ticketholder FirstName";
                workSheet.Cell("H1").Value = "Ticketholder LastName";
                workSheet.Cell("I1").Value = "Ticketholder Email";
                workSheet.Cell("J1").Value = "Ticketholder Title";
                workSheet.Cell("K1").Value = "Ticketholder Company";
                workSheet.Cell("L1").Value = "Ticketholder WPhone";
                workSheet.Cell("M1").Value = "Ticketholder MPhone";
                workSheet.Cell("N1").Value = "Ticketholder Address1";
                workSheet.Cell("O1").Value = "Ticketholder Address2";
                workSheet.Cell("P1").Value = "TicketHolder City";
                workSheet.Cell("Q1").Value = "TicketHolder State";
                workSheet.Cell("R1").Value = "TicketHolder Zip";
                workSheet.Cell("S1").Value = "TicketHolder Country";
                workSheet.Cell("T1").Value = "TicketHolder Gender";
                workSheet.Cell("U1").Value = "TicketHolder Age Group";
                workSheet.Cell("V1").Value = "TicketHolder Age";
                workSheet.Cell("W1").Value = "TicketHolder DOB";
                workSheet.Cell("X1").Value = "TicketHolder Website";
                workSheet.Cell("Y1").Value = "Ticket$";
                workSheet.Cell("Z1").Value = "Gratuity";
                workSheet.Cell("AA1").Value = "Buyer Svc Fee";
                workSheet.Cell("AB1").Value = "Host Svc Fee";
                workSheet.Cell("AC1").Value = "Total Svc Fee";
                workSheet.Cell("AD1").Value = "Sales Tax";
                workSheet.Cell("AE1").Value = "Ticket Total";
                workSheet.Cell("AF1").Value = "CC Proc Fee";
                workSheet.Cell("AG1").Value = "Promo Code";
                workSheet.Cell("AH1").Value = "Access Code";
                workSheet.Cell("AI1").Value = "Order Notes";

                foreach (var item in arrDataV2)
                {
                    RowCount += 1;

                    workSheet.Cell("A" + RowCount).Value = item.TicketId;
                    workSheet.Cell("B" + RowCount).Value = item.TicketLevel;
                    workSheet.Cell("C" + RowCount).Value = item.DeliveryMethod;
                    workSheet.Cell("D" + RowCount).Value = item.TicketStatus;
                    workSheet.Cell("E" + RowCount).Value = item.OrderDate;
                    workSheet.Cell("F" + RowCount).Value = item.Order;
                    workSheet.Cell("G" + RowCount).Value = item.TicketholderFirstName;
                    workSheet.Cell("H" + RowCount).Value = item.TicketholderLastName;
                    workSheet.Cell("I" + RowCount).Value = item.TicketholderEmail;
                    workSheet.Cell("J" + RowCount).Value = item.TicketholderTitle;
                    workSheet.Cell("K" + RowCount).Value = item.TicketholderCompany;
                    workSheet.Cell("L" + RowCount).Value = item.TicketholderWPhone;
                    workSheet.Cell("M" + RowCount).Value = item.TicketholderMPhone;
                    workSheet.Cell("N" + RowCount).Value = item.TicketholderAddress1;
                    workSheet.Cell("O" + RowCount).Value = item.TicketholderAddress2;
                    workSheet.Cell("P" + RowCount).Value = item.TicketHolderCity;
                    workSheet.Cell("Q" + RowCount).Value = item.TicketHolderState;
                    workSheet.Cell("R" + RowCount).Value = item.TicketHolderZip;
                    workSheet.Cell("S" + RowCount).Value = item.TicketHolderCountry;
                    workSheet.Cell("T" + RowCount).Value = item.TicketHolderGender;
                    workSheet.Cell("U" + RowCount).Value = item.TicketHolderAgeGroup;
                    workSheet.Cell("V" + RowCount).Value = item.TicketHolderAge;
                    workSheet.Cell("W" + RowCount).Value = item.TicketHolderDOB;
                    workSheet.Cell("X" + RowCount).Value = item.TicketHolderWebsite;
                    workSheet.Cell("Y" + RowCount).Value = item.Ticket;
                    workSheet.Cell("Z" + RowCount).Value = item.Gratuity;
                    workSheet.Cell("AA" + RowCount).Value = item.BuyerSvcFee;
                    workSheet.Cell("AB" + RowCount).Value = item.HostSvcFee;
                    workSheet.Cell("AC" + RowCount).Value = item.TotalSvcFee;
                    workSheet.Cell("AD" + RowCount).Value = item.SalesTax;
                    workSheet.Cell("AE" + RowCount).Value = item.TicketTotal;
                    workSheet.Cell("AF" + RowCount).Value = item.CCProcFee;
                    workSheet.Cell("AG" + RowCount).Value = item.PromoCode;
                    workSheet.Cell("AH" + RowCount).Value = item.AccessCode;
                    workSheet.Cell("AI" + RowCount).Value = item.OrderNotes;
                }

                var workbookBytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    workbookBytes = ms.ToArray();
                    ms.Flush();
                    ms.Position = 0;
                }

                var zipworkbookBytes = new byte[0];
                zipworkbookBytes = CompressExportFileToZip(workbookBytes, filename);
                filename = filename.Replace(".xlsx", ".zip");

                Url = UploadExportFileToStorage(zipworkbookBytes, filename, ImageType.RsvpExport);
            }
            return Url;
        }

        public static string PassportTicketholdersExport(int WineryId, int EventId)
        {
            //LoadTicketHolders

            string Url = string.Empty;

            string filename = string.Format("{0}_Ticketholders_{1}", EventId, DateTime.Now.ToString("MM_dd_yyyy")) + ".xlsx";

            TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            Times.TimeZone timeZone = (Times.TimeZone)eventDAL.GetTimeZonebyWineryId(WineryId);
            int offsetMinutes = Convert.ToInt32(Times.GetOffsetMinutes(timeZone));

            List<TicketsByEventExport> arrDataV2 = ticketDAL.GetPassportTicketsByEvent(EventId, offsetMinutes, WineryId);

            int RowCount = 1;

            using (XLWorkbook wb = new XLWorkbook())
            {
                IXLWorksheet workSheet = wb.Worksheets.Add("Ticket_Attendees_Export");

                workSheet.Cell("A1").Value = "Ticket Id";
                workSheet.Cell("B1").Value = "Ticket Level";
                workSheet.Cell("C1").Value = "Delivery Method";
                workSheet.Cell("D1").Value = "Ticket Status";
                workSheet.Cell("E1").Value = "Order Date";
                workSheet.Cell("F1").Value = "Order #";
                workSheet.Cell("G1").Value = "Ticketholder FirstName";
                workSheet.Cell("H1").Value = "Ticketholder LastName";
                workSheet.Cell("I1").Value = "Ticketholder Email";
                workSheet.Cell("J1").Value = "Ticketholder Title";
                workSheet.Cell("K1").Value = "Ticketholder Company";
                workSheet.Cell("L1").Value = "Ticketholder WPhone";
                workSheet.Cell("M1").Value = "Ticketholder MPhone";
                workSheet.Cell("N1").Value = "Ticketholder Address1";
                workSheet.Cell("O1").Value = "Ticketholder Address2";
                workSheet.Cell("P1").Value = "TicketHolder City";
                workSheet.Cell("Q1").Value = "TicketHolder State";
                workSheet.Cell("R1").Value = "TicketHolder Zip";
                workSheet.Cell("S1").Value = "TicketHolder Country";
                workSheet.Cell("T1").Value = "TicketHolder Gender";
                workSheet.Cell("U1").Value = "TicketHolder Age Group";
                workSheet.Cell("V1").Value = "TicketHolder Age";
                workSheet.Cell("W1").Value = "TicketHolder DOB";
                workSheet.Cell("X1").Value = "TicketHolder Website";
                workSheet.Cell("Y1").Value = "Ticket$";
                workSheet.Cell("Z1").Value = "Gratuity";
                workSheet.Cell("AA1").Value = "Buyer Svc Fee";
                workSheet.Cell("AB1").Value = "Host Svc Fee";
                workSheet.Cell("AC1").Value = "Total Svc Fee";
                workSheet.Cell("AD1").Value = "Sales Tax";
                workSheet.Cell("AE1").Value = "Ticket Total";
                workSheet.Cell("AF1").Value = "CC Proc Fee";
                workSheet.Cell("AG1").Value = "Promo Code";
                workSheet.Cell("AH1").Value = "Access Code";
                workSheet.Cell("AI1").Value = "Order Notes";

                foreach (var item in arrDataV2)
                {
                    RowCount += 1;

                    workSheet.Cell("A" + RowCount).Value = item.TicketId;
                    workSheet.Cell("B" + RowCount).Value = item.TicketLevel;
                    workSheet.Cell("C" + RowCount).Value = item.DeliveryMethod;
                    workSheet.Cell("D" + RowCount).Value = item.TicketStatus;
                    workSheet.Cell("E" + RowCount).Value = item.OrderDate;
                    workSheet.Cell("F" + RowCount).Value = item.Order;
                    workSheet.Cell("G" + RowCount).Value = item.TicketholderFirstName;
                    workSheet.Cell("H" + RowCount).Value = item.TicketholderLastName;
                    workSheet.Cell("I" + RowCount).Value = item.TicketholderEmail;
                    workSheet.Cell("J" + RowCount).Value = item.TicketholderTitle;
                    workSheet.Cell("K" + RowCount).Value = item.TicketholderCompany;
                    workSheet.Cell("L" + RowCount).Value = item.TicketholderWPhone;
                    workSheet.Cell("M" + RowCount).Value = item.TicketholderMPhone;
                    workSheet.Cell("N" + RowCount).Value = item.TicketholderAddress1;
                    workSheet.Cell("O" + RowCount).Value = item.TicketholderAddress2;
                    workSheet.Cell("P" + RowCount).Value = item.TicketHolderCity;
                    workSheet.Cell("Q" + RowCount).Value = item.TicketHolderState;
                    workSheet.Cell("R" + RowCount).Value = item.TicketHolderZip;
                    workSheet.Cell("S" + RowCount).Value = item.TicketHolderCountry;
                    workSheet.Cell("T" + RowCount).Value = item.TicketHolderGender;
                    workSheet.Cell("U" + RowCount).Value = item.TicketHolderAgeGroup;
                    workSheet.Cell("V" + RowCount).Value = item.TicketHolderAge;
                    workSheet.Cell("W" + RowCount).Value = item.TicketHolderDOB;
                    workSheet.Cell("X" + RowCount).Value = item.TicketHolderWebsite;
                    workSheet.Cell("Y" + RowCount).Value = item.Ticket;
                    workSheet.Cell("Z" + RowCount).Value = item.Gratuity;
                    workSheet.Cell("AA" + RowCount).Value = item.BuyerSvcFee;
                    workSheet.Cell("AB" + RowCount).Value = item.HostSvcFee;
                    workSheet.Cell("AC" + RowCount).Value = item.TotalSvcFee;
                    workSheet.Cell("AD" + RowCount).Value = item.SalesTax;
                    workSheet.Cell("AE" + RowCount).Value = item.TicketTotal;
                    workSheet.Cell("AF" + RowCount).Value = item.CCProcFee;
                    workSheet.Cell("AG" + RowCount).Value = item.PromoCode;
                    workSheet.Cell("AH" + RowCount).Value = item.AccessCode;
                    workSheet.Cell("AI" + RowCount).Value = item.OrderNotes;
                }

                var workbookBytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    workbookBytes = ms.ToArray();
                    ms.Flush();
                    ms.Position = 0;
                }

                var zipworkbookBytes = new byte[0];
                zipworkbookBytes = CompressExportFileToZip(workbookBytes, filename);
                filename = filename.Replace(".xlsx", ".zip");

                Url = UploadExportFileToStorage(zipworkbookBytes, filename, ImageType.RsvpExport);
            }
            return Url;
        }

        public static string CheckInAttendeesExport(int WineryId, int EventId, bool Passport)
        {
            //LoadTicketHolders

            string Url = string.Empty;

            string filename = string.Format("{0}_Attendees_{1}", EventId, DateTime.Now.ToString("MM_dd_yyyy")) + ".xlsx";

            TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            Times.TimeZone timeZone = (Times.TimeZone)eventDAL.GetTimeZonebyWineryId(WineryId);
            int offsetMinutes = Convert.ToInt32(Times.GetOffsetMinutes(timeZone));

            List<ExportCheckInAttendees> arrDataV2 = new List<ExportCheckInAttendees>();

            if (Passport)
                arrDataV2 = ticketDAL.GetTicketPassportCheckInUserHistory(EventId, WineryId);
            else
                arrDataV2 = ticketDAL.GetTicketCheckInUserHistory(EventId, WineryId);

            int RowCount = 1;

            using (XLWorkbook wb = new XLWorkbook())
            {
                IXLWorksheet workSheet = wb.Worksheets.Add("CheckInAttendees_Export");

                workSheet.Cell("A1").Value = "Email";
                workSheet.Cell("B1").Value = "Last Name";
                workSheet.Cell("C1").Value = "First Name";
                workSheet.Cell("D1").Value = "Address 1";
                workSheet.Cell("E1").Value = "Address 2";
                workSheet.Cell("F1").Value = "City";
                workSheet.Cell("G1").Value = "State";
                workSheet.Cell("H1").Value = "Zip Code";
                workSheet.Cell("I1").Value = "Home Phone";
                workSheet.Cell("J1").Value = "Account Type";
                workSheet.Cell("K1").Value = "Date Checked-in";
                workSheet.Cell("L1").Value = "Location";
                workSheet.Cell("M1").Value = "Ticket Id";

                foreach (var item in arrDataV2)
                {
                    RowCount += 1;

                    workSheet.Cell("A" + RowCount).Value = item.Email;
                    workSheet.Cell("B" + RowCount).Value = item.LastName;
                    workSheet.Cell("C" + RowCount).Value = item.FirstName;
                    workSheet.Cell("D" + RowCount).Value = item.Address1;
                    workSheet.Cell("E" + RowCount).Value = item.Address2;
                    workSheet.Cell("F" + RowCount).Value = item.City;
                    workSheet.Cell("G" + RowCount).Value = item.State;
                    workSheet.Cell("H" + RowCount).Value = item.ZipCode;
                    workSheet.Cell("I" + RowCount).Value = item.HomePhone;
                    workSheet.Cell("J" + RowCount).Value = item.AccountType;
                    workSheet.Cell("K" + RowCount).Value = item.DateCheckedin;
                    workSheet.Cell("L" + RowCount).Value = item.Location;
                    workSheet.Cell("M" + RowCount).Value = item.TicketId;
                }

                var workbookBytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    workbookBytes = ms.ToArray();
                    ms.Flush();
                    ms.Position = 0;
                }

                var zipworkbookBytes = new byte[0];
                zipworkbookBytes = CompressExportFileToZip(workbookBytes, filename);
                filename = filename.Replace(".xlsx", ".zip");

                Url = UploadExportFileToStorage(zipworkbookBytes, filename, ImageType.RsvpExport);
            }
            return Url;
        }

        public static string TabsToSpaces(string source, int tabSize)
        {
            var outBuf = new StringBuilder();

            var inBufArray = source.ToCharArray();
            int destinationIndex = 0;
            char c;

            for (int i = 0; i < inBufArray.Length; i++)
            {
                c = inBufArray[i];
                if (c == '\t')
                {
                    do
                    {
                        outBuf.Append(' ');
                    } while ((++destinationIndex % tabSize) != 0);
                }
                else
                {
                    outBuf.Append(c);
                    ++destinationIndex;
                }
            }

            return outBuf.ToString();
        }

        public static byte[] CompressExportFileToZip(byte[] workbookBytes, string filename)
        {
            var zipworkbookBytes = new byte[0];
            using (var memoryStream = new MemoryStream())
            {
                // note "leaveOpen" true, to not dispose memoryStream too early
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    var zipEntry = zipArchive.CreateEntry(filename);
                    using (Stream entryStream = zipEntry.Open())
                    {
                        entryStream.Write(workbookBytes, 0, workbookBytes.Length);
                    }
                }
                // now, after zipArchive is disposed - all is written to memory stream
                zipworkbookBytes = memoryStream.ToArray();
            }
            return zipworkbookBytes;
        }

        #endregion
    }
}

