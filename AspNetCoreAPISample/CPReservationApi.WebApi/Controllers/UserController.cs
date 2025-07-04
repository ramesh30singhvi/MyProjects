using CPReservationApi.Common;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.Services;
using CPReservationApi.WebApi.ViewModels;
using MailChimp.Net.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static CPReservationApi.Common.Common;
using static CPReservationApi.Common.Email;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using ImageResizer.Configuration;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/user")]
    public class UserController : BaseController
    {

        public static IOptions<AppSettings> _appSetting;
        public UserController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }
        /// <summary>
        /// This method gives user details by email
        /// </summary>
        /// <param name="email">Email of User (Required)</param>
        /// <param name="member_Id">Id of Member (Required)</param>
        /// <returns></returns>
        [Route("userdetails")]
        [HttpGet]
        public async Task<IActionResult> GetUserDetailsbyemail(string email, int member_Id)
        {
            var userDetailResponse = new UserDetailResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                var userDetailModel = new UserDetailModel();

                userDetailModel = userDAL.GetUserDetailsbyemail(email, member_Id);
                if (userDetailModel != null)
                {
                    userDetailModel.mobile_number = Services.Utility.FormatTelephoneNumber(userDetailModel.mobile_number, userDetailModel.address.country);
                    userDetailModel.phone_number = Services.Utility.FormatTelephoneNumber(userDetailModel.phone_number, userDetailModel.address.country);
                    userDetailResponse.success = true;
                    userDetailResponse.data = userDetailModel;
                }
                else
                {
                    userDetailResponse.success = true;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetUserDetailsbyemail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_Id);
            }
            return new ObjectResult(userDetailResponse);
        }

        [Route("userdetailsv2")]
        [HttpGet]
        public async Task<IActionResult> GetUserDetailsbyemailV2(int member_Id, string email_address = "", int user_id = 0)
        {
            var userDetailResponse = new UserDetailResponse();
            try
            {
                if (member_Id > 0 && (!string.IsNullOrWhiteSpace(email_address) || user_id > 0))
                {
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                    var userDetailModel = new UserDetailModel();

                    string email = userDAL.GetUserByUserNameOrId(email_address, member_Id, user_id);
                    if (email != "")
                    {
                        if (email.IndexOf("@noemail") == -1)
                        {
                            if (Email.EmailIsValid(email))
                            {
                                var userList = new List<UserDetailModel>();
                                userList = await Services.Utility.GetUsersByEmail(email, member_Id);

                                if (userList != null && userList.Count > 0)
                                {
                                    userDetailResponse.success = true;
                                    userDetailResponse.data = userList[0];
                                }
                            }
                            else
                            {
                                userDetailResponse.success = true;
                                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                                userDetailResponse.error_info.extra_info = "no record found";
                            }
                        }
                        else
                        {
                            userDetailResponse.success = true;
                            userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                            userDetailResponse.error_info.extra_info = "no record found";
                        }
                    }
                    else
                    {
                        userDetailResponse.success = true;
                        userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        userDetailResponse.error_info.extra_info = "no record found";
                    }
                }
                else
                {
                    userDetailResponse.success = true;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetUserDetailsbyemailV2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_Id);
            }
            return new ObjectResult(userDetailResponse);
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetUsersbykeyword(string keyword, int member_id = 0, bool ignore_local_db = false)
        {
            var userDetailResponse = new UserListResponse();
            try
            {
                var userDetailModel = new List<UserDetailModel>();
                //await Utility.ResolveCustomer("ws_users_encore", "ws2_Encore$", "jelliman@cellarpass.com");
                userDetailModel = await Services.Utility.GetUsersByEmail(keyword, member_id,ignore_local_db, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);

                if (userDetailModel != null && userDetailModel.Count > 0)
                {
                    userDetailResponse.success = true;
                    userDetailResponse.data = userDetailModel;
                }
                else
                {
                    userDetailResponse.success = true;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetUsersbykeyword:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(userDetailResponse);
        }

        [Route("ewinerycustomer")]
        [HttpGet]
        public async Task<IActionResult> GeteWineryCustomer(string user_name,string password, string email_address)
        {
            var userDetailResponse = new eWineryCustomerResponse();
            try
            {
                List<eWineryCustomerViewModel> modelList = new List<eWineryCustomerViewModel>();
                modelList = await Task.Run(() => Services.Utility.ResolveCustomer(user_name, password, email_address,0));

                if (modelList != null && modelList.Count > 0)
                {
                    userDetailResponse.success = true;
                    userDetailResponse.data = modelList;
                }
                else
                {
                    userDetailResponse.success = false;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GeteWineryCustomer:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(userDetailResponse);
        }

        [Route("thirdpartyuserlookup")]
        [HttpGet]
        public async Task<IActionResult> GetUsersLookUpbykeyword(string keyword, int member_id)
        {
            var userDetailResponse = new UserListResponse();
            try
            {
                var userDetailModel = new List<UserDetailModel>();

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WineryModel memberModel = await Task.Run(() => eventDAL.GetWineryById(member_id));
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                bool bLoyalClubLookupEnabled = false;
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Common.Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(member_id, (int)Common.Common.SettingGroup.bLoyal).ToList();
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

                if (memberModel.EnableClubVin65 && !string.IsNullOrWhiteSpace(memberModel.Vin65UserName) && !string.IsNullOrWhiteSpace(memberModel.Vin65Password))
                {
                    List<Vin65Model> modelList = new List<Vin65Model>();
                    modelList = await Task.Run(() => Services.Utility.Vin65GetContacts(keyword, memberModel.Vin65Password, memberModel.Vin65UserName));

                    foreach (var item in modelList)
                    {
                        UserDetailModel vin65model = new UserDetailModel();
                        vin65model.email = item.Email;
                        vin65model.first_name = item.FirstName;
                        vin65model.last_name = item.LastName;
                        vin65model.phone_number = Services.Utility.FormatTelephoneNumber(item.HomePhone, item.Country);
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

                        if (member_id > 0)
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
                                vin65model.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.Country);
                                vin65model.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                vin65model.region_most_visited = guestPerformanceModel.region_most_visited;
                                vin65model.company_name = guestPerformanceModel.company_name;

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

                        userDetailModel.Add(vin65model);
                    }
                }
                else if (memberModel.EnableClubOrderPort && !string.IsNullOrWhiteSpace(memberModel.OrderPortClientId) && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiKey) && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiToken))
                {
                    userDetailModel = await Task.Run(() => Services.Utility.GetCustomersByNameOrEmail(keyword, memberModel.OrderPortApiKey, memberModel.OrderPortApiToken, memberModel.OrderPortClientId));
                    foreach (var item in userDetailModel)
                    {
                        AccountNote notemodel = new AccountNote();
                        notemodel.modified_by = "";
                        notemodel.note = "";
                        if (member_id > 0)
                        {
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
                                item.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                                item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                item.region_most_visited = guestPerformanceModel.region_most_visited;
                                item.company_name = guestPerformanceModel.company_name;

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
                    userDetailModel = await Task.Run(() => Services.Utility.GetCommerce7CustomersByNameOrEmail(keyword, memberModel.Commerce7Username, memberModel.Commerce7Password, memberModel.Commerce7Tenant, member_id));

                    foreach (var item in userDetailModel)
                    {
                        if (member_id > 0)
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
                                item.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                            }
                        }
                    }

                }
                else if (memberModel.EnableClubemember && !string.IsNullOrWhiteSpace(memberModel.eMemberUserNAme) && !string.IsNullOrWhiteSpace(memberModel.eMemberPAssword))
                {
                    List<eWineryCustomerViewModel> modelList = new List<eWineryCustomerViewModel>();
                    modelList = await Task.Run(() => Services.Utility.ResolveCustomer(memberModel.eMemberUserNAme, memberModel.eMemberPAssword, keyword,member_id));

                    foreach (var item in modelList)
                    {
                        UserDetailModel eMemberModel = new UserDetailModel();
                        eMemberModel.email = item.email;
                        eMemberModel.first_name = item.first_name;
                        eMemberModel.last_name = item.last_name;
                        eMemberModel.phone_number = Services.Utility.FormatTelephoneNumber(item.phone, item.country_code);
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

                        if (member_id > 0)
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
                                eMemberModel.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, eMemberModel.address.country);
                            }
                        }

                        userDetailModel.Add(eMemberModel);
                    }
                }
                else if (memberModel.EnableClubShopify && !string.IsNullOrWhiteSpace(memberModel.ShopifyPublishKey) && !string.IsNullOrWhiteSpace(memberModel.ShopifyAppPassword) && !string.IsNullOrWhiteSpace(memberModel.ShopifyAppStoreName))
                {
                    userDetailModel = await Task.Run(() => Services.Utility.GetShopifyCustomersByNameOrEmail(_appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken, member_id, keyword));

                    foreach (var item in userDetailModel)
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
                            item.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                        }
                    }
                }
                else if (memberModel.EnableClubBigCommerce && !string.IsNullOrWhiteSpace(memberModel.BigCommerceAceessToken) && !string.IsNullOrWhiteSpace(memberModel.BigCommerceStoreId))
                {
                    userDetailModel = await Task.Run(() => Services.Utility.GetBigCommerceCustomersByNameOrEmail(memberModel.BigCommerceStoreId, memberModel.BigCommerceAceessToken, member_id, keyword));

                    foreach (var item in userDetailModel)
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
                            item.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                        }
                    }
                }

                else if (bLoyalClubLookupEnabled)
                {
                    var user = await Services.Utility.bLoyalResolveCustomer(settingsGroup, keyword);
                    if (user != null)
                    {
                        userDetailModel.Add(user);

                        foreach (var item in userDetailModel)
                        {
                            AccountNote notemodel = new AccountNote();
                            notemodel.modified_by = "";
                            notemodel.note = "";
                            if (member_id > 0)
                            {
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
                                    item.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                                    item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                    item.region_most_visited = guestPerformanceModel.region_most_visited;
                                    item.company_name = guestPerformanceModel.company_name;

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
                    var user = await Services.Utility.SalesForceResolveCustomer(memberModel.SalesForceUserName, memberModel.SalesForcePassword, memberModel.SalesForceSecurityToken, keyword, memberModel.ClubStatusField);
                    if (user != null)
                    {
                        userDetailModel.Add(user);

                        foreach (var item in userDetailModel)
                        {
                            AccountNote notemodel = new AccountNote();
                            notemodel.modified_by = "";
                            notemodel.note = "";
                            if (member_id > 0)
                            {
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
                                    item.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                                    item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                    item.region_most_visited = guestPerformanceModel.region_most_visited;
                                    item.company_name = guestPerformanceModel.company_name;

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

                    userDetailModel = userDAL.GetClubMemberListBykeyword(keyword, member_id, src);
                    foreach (var item in userDetailModel)
                    {
                        item.phone_number = Services.Utility.FormatTelephoneNumber(item.phone_number, item.address.country);
                        item.mobile_number = Services.Utility.FormatTelephoneNumber(item.mobile_number, item.address.country);
                    }
                }

                if (userDetailModel != null && userDetailModel.Count > 0)
                {
                    userDetailResponse.success = true;
                    userDetailResponse.data = userDetailModel;
                }
                else
                {
                    userDetailResponse.success = true;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetUsersbykeyword:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(userDetailResponse);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [Route("orderportlist")]
        [HttpGet]
        public async Task<IActionResult> GetOrderPortUsersbykeyword(string keyword, int member_id = 0)
        {
            var userDetailResponse = new UserListResponse();
            try
            {
                var userDetailModel = new List<UserDetailModel>();
                userDetailModel = await Services.Utility.GetUsersByEmail(keyword, member_id);

                if (userDetailModel != null && userDetailModel.Count > 0)
                {
                    userDetailResponse.success = true;
                    userDetailResponse.data = userDetailModel;
                }
                else
                {
                    userDetailResponse.success = true;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
                //var userDetailModel = new List<UserDetailModel>();
                //EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                //WineryModel memberModel = await Task.Run(() => eventDAL.GetWineryById(member_id));

                //if (memberModel.EnableOrderPort && !string.IsNullOrWhiteSpace(memberModel.OrderPortClientId)
                //        && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiKey) && !string.IsNullOrWhiteSpace(memberModel.OrderPortApiToken))
                //{
                //    userDetailModel = await Task.Run(() => Utility.GetCustomersByNameOrEmail(keyword, memberModel.OrderPortApiKey, memberModel.OrderPortApiToken, memberModel.OrderPortClientId));
                //}

                //if (userDetailModel != null && userDetailModel.Count > 0)
                //{
                //    userDetailResponse.success = true;
                //    userDetailResponse.data = userDetailModel;
                //}
                //else
                //{
                //    userDetailResponse.success = true;
                //    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                //    userDetailResponse.error_info.extra_info = "no record found";
                //}
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetOrderPortUsersbykeyword:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(userDetailResponse);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [Route("commerce7list")]
        [HttpGet]
        public async Task<IActionResult> GetCommerce7Usersbykeyword(string keyword, int member_id)
        {
            var userDetailResponse = new UserListResponse();
            try
            {
                var userDetailModel = new List<UserDetailModel>();
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WineryModel memberModel = await Task.Run(() => eventDAL.GetWineryById(member_id));

                //memberModel.EnableCommerce7 = true;
                //memberModel.Commerce7Tenant = "cellarpass";
                //memberModel.Commerce7Username = "jelliman@cellarpass.com";
                //memberModel.Commerce7Password = "Supra1997#";

                if (memberModel.EnableCommerce7 && !string.IsNullOrWhiteSpace(memberModel.Commerce7Username)
                        && !string.IsNullOrWhiteSpace(memberModel.Commerce7Password) && !string.IsNullOrWhiteSpace(memberModel.Commerce7Tenant))
                {
                    userDetailModel = await Task.Run(() => Services.Utility.GetCommerce7CustomersByNameOrEmail(keyword, memberModel.Commerce7Username, memberModel.Commerce7Password, memberModel.Commerce7Tenant,member_id));

                    foreach (var item in userDetailModel)
                    {
                        if (member_id > 0)
                        {
                            var guestPerformanceModel = new GuestPerformance();
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            guestPerformanceModel = userDAL.GetGuestPerformanceData(item.email, member_id);

                            if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                            {
                                item.completed_count = guestPerformanceModel.completed_count;
                                item.visits_count = guestPerformanceModel.visits_count;
                                item.cancellations_count = guestPerformanceModel.cancellations_count;
                                item.no_shows_count = guestPerformanceModel.no_shows_count;
                                item.account_note = userDAL.GetAccountNote(member_id, guestPerformanceModel.user_id);
                                item.color = guestPerformanceModel.color;
                                item.roles = guestPerformanceModel.roles;
                                item.user_id = guestPerformanceModel.user_id;
                                item.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                                item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                item.region_most_visited = guestPerformanceModel.region_most_visited;
                                item.company_name = guestPerformanceModel.company_name;

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
                            }
                        }
                    }
                }

                if (userDetailModel != null && userDetailModel.Count > 0)
                {
                    userDetailResponse.success = true;
                    userDetailResponse.data = userDetailModel;
                }
                else
                {
                    userDetailResponse.success = true;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetCommerce7Usersbykeyword:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(userDetailResponse);
        }

        [Route("commerce7ordersource")]
        [HttpGet]
        public async Task<IActionResult> GetCommerce7OrderSource(int member_id)
        {
            var commerce7OrderSourceResponse = new Commerce7OrderSourceResponse();
            try
            {
                List<string> orderSourcelist = new List<string>();
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WineryModel memberModel = await Task.Run(() => eventDAL.GetWineryById(member_id));

                if (memberModel.EnableCommerce7 && !string.IsNullOrWhiteSpace(memberModel.Commerce7Username)
                        && !string.IsNullOrWhiteSpace(memberModel.Commerce7Password) && !string.IsNullOrWhiteSpace(memberModel.Commerce7Tenant))
                {
                    orderSourcelist = await Task.Run(() => Services.Utility.GetCommerce7OrderSource(memberModel.Commerce7Username, memberModel.Commerce7Password, memberModel.Commerce7Tenant, member_id));
                }

                if (orderSourcelist != null && orderSourcelist.Count > 0)
                {
                    commerce7OrderSourceResponse.success = true;

                    Commerce7OrderSourceModel commerce7OrderSourceModel = new Commerce7OrderSourceModel();

                    commerce7OrderSourceModel.name = orderSourcelist;
                    commerce7OrderSourceResponse.data = commerce7OrderSourceModel;
                }
                else
                {
                    commerce7OrderSourceResponse.success = true;
                    commerce7OrderSourceResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    commerce7OrderSourceResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                commerce7OrderSourceResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                commerce7OrderSourceResponse.error_info.extra_info = Common.Common.InternalServerError;
                commerce7OrderSourceResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetCommerce7OrderSource:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(commerce7OrderSourceResponse);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [Route("shopifycustomerlist")]
        [HttpGet]
        public async Task<IActionResult> GetShopifyCustomersByNameOrEmail(string keyword, int member_id)
        {
            var userDetailResponse = new UserListResponse();
            try
            {
                var userDetailModel = new List<UserDetailModel>();
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WineryModel memberModel = await Task.Run(() => eventDAL.GetWineryById(member_id));

                if (memberModel.EnableShopify && !string.IsNullOrWhiteSpace(memberModel.ShopifyAppPassword)
                        && !string.IsNullOrWhiteSpace(memberModel.ShopifyAppStoreName) && !string.IsNullOrWhiteSpace(memberModel.ShopifyPublishKey))
                {
                    userDetailModel = await Task.Run(() => Services.Utility.GetShopifyCustomersByNameOrEmail(_appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken, member_id, keyword));

                    foreach (var item in userDetailModel)
                    {
                        if (member_id > 0)
                        {
                            var guestPerformanceModel = new GuestPerformance();
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            guestPerformanceModel = userDAL.GetGuestPerformanceData(item.email, member_id);

                            if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                            {
                                item.completed_count = guestPerformanceModel.completed_count;
                                item.visits_count = guestPerformanceModel.visits_count;
                                item.cancellations_count = guestPerformanceModel.cancellations_count;
                                item.no_shows_count = guestPerformanceModel.no_shows_count;
                                item.account_note = userDAL.GetAccountNote(member_id, guestPerformanceModel.user_id);
                                item.color = guestPerformanceModel.color;
                                item.roles = guestPerformanceModel.roles;
                                item.user_id = guestPerformanceModel.user_id;
                                item.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                                item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                item.region_most_visited = guestPerformanceModel.region_most_visited;
                                item.company_name = guestPerformanceModel.company_name;

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
                            }
                        }
                    }
                }

                if (userDetailModel != null && userDetailModel.Count > 0)
                {
                    userDetailResponse.success = true;
                    userDetailResponse.data = userDetailModel;
                }
                else
                {
                    userDetailResponse.success = true;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetShopifyCustomersByNameOrEmail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(userDetailResponse);
        }

        [Route("bigcommercecustomerlist")]
        [HttpGet]
        public async Task<IActionResult> GetBigCommerceCustomersByNameOrEmail(string keyword, int member_id)
        {
            var userDetailResponse = new UserListResponse();
            try
            {
                var userDetailModel = new List<UserDetailModel>();
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WineryModel memberModel = await Task.Run(() => eventDAL.GetWineryById(member_id));

                if (memberModel.EnableClubBigCommerce && !string.IsNullOrWhiteSpace(memberModel.BigCommerceAceessToken) && !string.IsNullOrWhiteSpace(memberModel.BigCommerceStoreId))
                {
                    userDetailModel = await Task.Run(() => Services.Utility.GetBigCommerceCustomersByNameOrEmail(memberModel.BigCommerceStoreId, memberModel.BigCommerceAceessToken, member_id, keyword));

                    foreach (var item in userDetailModel)
                    {
                        if (member_id > 0)
                        {
                            var guestPerformanceModel = new GuestPerformance();
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            guestPerformanceModel = userDAL.GetGuestPerformanceData(item.email, member_id);

                            if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                            {
                                item.completed_count = guestPerformanceModel.completed_count;
                                item.visits_count = guestPerformanceModel.visits_count;
                                item.cancellations_count = guestPerformanceModel.cancellations_count;
                                item.no_shows_count = guestPerformanceModel.no_shows_count;
                                item.account_note = userDAL.GetAccountNote(member_id, guestPerformanceModel.user_id);
                                item.color = guestPerformanceModel.color;
                                item.roles = guestPerformanceModel.roles;
                                item.user_id = guestPerformanceModel.user_id;
                                item.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                                item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                                item.region_most_visited = guestPerformanceModel.region_most_visited;
                                item.company_name = guestPerformanceModel.company_name;

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
                            }
                        }
                    }
                }

                if (userDetailModel != null && userDetailModel.Count > 0)
                {
                    userDetailResponse.success = true;
                    userDetailResponse.data = userDetailModel;
                }
                else
                {
                    userDetailResponse.success = true;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetBigCommerceCustomersByNameOrEmail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(userDetailResponse);
        }

        [Route("checkbigcommercelogintest")]
        [HttpGet]
        public async Task<IActionResult> CheckBigCommerceLoginTest(int member_id)
        {
            var response = new ViewModels.BaseResponse();
            try
            {
                ViewModels.ApiResponse apiResponse = new ViewModels.ApiResponse();
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WineryModel memberModel = await Task.Run(() => eventDAL.GetWineryById(member_id));

                if (memberModel.EnableClubBigCommerce && !string.IsNullOrWhiteSpace(memberModel.BigCommerceAceessToken) && !string.IsNullOrWhiteSpace(memberModel.BigCommerceStoreId))
                {
                    apiResponse = await Task.Run(() => Services.Utility.GetBigCommerceAllCustomers(memberModel.BigCommerceStoreId, memberModel.BigCommerceAceessToken));
                    
                    if (apiResponse.status == 1)
                        response.success = true;
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetBigCommerceAllOrders:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(response);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [Route("ewinerylist")]
        [HttpGet]
        public async Task<IActionResult> GeteWineryContactDetailsbyName(string keyword, int member_id)
        {
            var userDetailResponse = new UserListResponse();
            try
            {
                var userDetailModel = new List<UserDetailModel>();
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                WineryModel memberModel = await Task.Run(() => eventDAL.GetWineryById(member_id));

                if (memberModel.EnableClubemember && !string.IsNullOrWhiteSpace(memberModel.eMemberUserNAme) && !string.IsNullOrWhiteSpace(memberModel.eMemberPAssword))
                {
                    List<eWineryCustomerViewModel> modelList = new List<eWineryCustomerViewModel>();
                    modelList = await Task.Run(() => Services.Utility.ResolveCustomer(memberModel.eMemberUserNAme, memberModel.eMemberPAssword, keyword,member_id));

                    foreach (var item in modelList)
                    {
                        UserDetailModel eMemberModel = new UserDetailModel();
                        eMemberModel.email = item.email;
                        eMemberModel.first_name = item.first_name;
                        eMemberModel.last_name = item.last_name;
                        eMemberModel.phone_number = Services.Utility.FormatTelephoneNumber(item.phone, item.country_code);
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

                        if (member_id > 0)
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
                                eMemberModel.region_most_visited = eMemberModel.region_most_visited;
                                eMemberModel.company_name = guestPerformanceModel.company_name;

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
                                            item.account_note.note = note;
                                        }
                                    }
                                    else
                                    {
                                        item.account_note = accountNote;
                                    }
                                }
                                eMemberModel.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, eMemberModel.address.country);
                            }
                        }

                        userDetailModel.Add(eMemberModel);
                    }
                }

                if (userDetailModel != null && userDetailModel.Count > 0)
                {
                    userDetailResponse.success = true;
                    userDetailResponse.data = userDetailModel;
                }
                else
                {
                    userDetailResponse.success = true;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetCommerce7Usersbykeyword:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(userDetailResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("clubmemberlist")]
        [HttpGet]
        public async Task<IActionResult> GetClubMemberUsersbykeyword(string keyword, int member_id, int src)
        {
            var userDetailResponse = new UserListResponse();
            try
            {
                var userDetailModel = new List<UserDetailModel>();
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                userDetailModel = await Task.Run(() => userDAL.GetClubMemberListBykeyword(keyword, member_id, src));
                foreach (var item in userDetailModel)
                {
                    item.phone_number = Services.Utility.FormatTelephoneNumber(item.phone_number, item.address.country);
                    item.mobile_number = Services.Utility.FormatTelephoneNumber(item.mobile_number, item.address.country);

                    if (member_id > 0)
                    {
                        var guestPerformanceModel = new GuestPerformance();
                        guestPerformanceModel = userDAL.GetGuestPerformanceData(item.email, member_id);

                        if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                        {
                            item.completed_count = guestPerformanceModel.completed_count;
                            item.visits_count = guestPerformanceModel.visits_count;
                            item.cancellations_count = guestPerformanceModel.cancellations_count;
                            item.no_shows_count = guestPerformanceModel.no_shows_count;
                            item.account_note = userDAL.GetAccountNote(member_id, guestPerformanceModel.user_id);
                            item.color = guestPerformanceModel.color;
                            item.roles = guestPerformanceModel.roles;
                            item.user_id = guestPerformanceModel.user_id;
                            item.mobile_number = Services.Utility.FormatTelephoneNumber(guestPerformanceModel.mobile_number, item.address.country);
                            item.mobile_number_status = guestPerformanceModel.mobile_number_status;
                            item.region_most_visited = guestPerformanceModel.region_most_visited;
                            item.company_name = guestPerformanceModel.company_name;

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
                        }
                    }
                }

                if (userDetailModel != null && userDetailModel.Count > 0)
                {
                    userDetailResponse.success = true;
                    userDetailResponse.data = userDetailModel;
                }
                else
                {
                    userDetailResponse.success = true;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetClubMemberUsersbykeyword:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(userDetailResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("UpsertOrderPortUser")]
        [HttpPost]
        public async Task<IActionResult> UpsertOrderPortUser(int wineryId, string apiKey, string apiToken, string clientId, string CustomerUuid, string FirstName, string LastName, string Company, string Address1, string Address2, string City, string State, string ZipCode, string Country, string Email, string Phone)
        {
            var userOrderPortResponse = new UserOrderPortResponse();

            try
            {
                PayloadModel payloadModel = new PayloadModel();
                payloadModel.CustomerUuid = CustomerUuid;

                payloadModel.BillingAddress.FirstName = FirstName;
                payloadModel.BillingAddress.LastName = LastName;
                payloadModel.BillingAddress.Company = !string.IsNullOrEmpty(Company) ? Company : "";
                payloadModel.BillingAddress.Address1 = Address1;
                payloadModel.BillingAddress.Address2 = !string.IsNullOrEmpty(Address2) ? Address2 : "";
                payloadModel.BillingAddress.City = !string.IsNullOrEmpty(City) ? City : "";
                payloadModel.BillingAddress.State = !string.IsNullOrEmpty(State) ? State : "";
                payloadModel.BillingAddress.ZipCode = !string.IsNullOrEmpty(ZipCode) ? ZipCode : "";
                payloadModel.BillingAddress.Country = Country;
                payloadModel.BillingAddress.Email = Email;
                payloadModel.BillingAddress.Phone = Phone;
                string CustId = await Services.Utility.UpsertCustomerDetails(payloadModel, apiKey, apiToken, clientId,wineryId);
                if (CustId.Length > 0)
                {
                    userOrderPortResponse.success = true;
                    userOrderPortResponse.data.id = CustId;
                }
                else
                {
                    userOrderPortResponse.success = false;
                }

            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                userOrderPortResponse.success = false;
                logDAL.InsertLog("WebApi", "UpsertOrderPortUser:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,wineryId);
            }
            return new ObjectResult(userOrderPortResponse);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [Route("UpsertCommerce7User")]
        [HttpPost]
        public async Task<IActionResult> UpsertCommerce7User(string Commerce7Username, string Commerce7Password, string Commerce7Tenant, string FirstName, string LastName, string Company, string Address1, string Address2, string City, string State, string ZipCode, string Country, string Email, string Phone, string DefaultAccountTypeId)
        {
            var userOrderPortResponse = new UserOrderPortResponse();
            userOrderPortResponse.success = false;
            try
            {
                Commerce7CustomerModel commerce7CustomerModel = new Commerce7CustomerModel();
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                int memberId = eventDAL.GetWineryIdByCommerce7Data(Commerce7Username, Commerce7Password);

                commerce7CustomerModel = await Services.Utility.CheckAndUpdateCommerce7Customer(Commerce7Username, Commerce7Password, Commerce7Tenant, FirstName, LastName, Company, Address1, Address2, City, State, ZipCode, Country, Email, Phone, DefaultAccountTypeId, memberId);

                if (commerce7CustomerModel.CustId != null && commerce7CustomerModel.CustId.Length > 0)
                {
                    userOrderPortResponse.success = true;
                    userOrderPortResponse.data.id = commerce7CustomerModel.CustId;
                }

                if (commerce7CustomerModel.Exceeded)
                {
                    userOrderPortResponse.success = false;
                    userOrderPortResponse.error_info.description = "Exceeded";
                }
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                int memberId = eventDAL.GetWineryIdByCommerce7Data(Commerce7Username, Commerce7Password);

                userOrderPortResponse.success = false;
                logDAL.InsertLog("WebApi", "UpsertCommerce7User:  Email:" + Email + ",Message:" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1, memberId);
            }
            return new ObjectResult(userOrderPortResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("orderportcustomerclasses")]
        [HttpGet]
        public async Task<IActionResult> GetOrderPortCustomerClasses(string apiKey, string apiToken, string clientId)
        {
            var customerClassesResponse = new CustomerClassesResponse();

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            int memberId = eventDAL.GetWineryIdByOrderPortData(apiKey);
            try
            {
                customerClassesResponse = await Services.Utility.GetCustomerClassesForWinery(apiKey, apiToken, clientId, memberId);
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                

                logDAL.InsertLog("WebApi", "GetOrderPortCustomerClasses:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1, memberId);
            }
            return new ObjectResult(customerClassesResponse);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [Route("commerce7_groups")]
        [HttpGet]
        public async Task<IActionResult> GetCommerce7Groups(string Commerce7Username, string Commerce7Password, string Commerce7Tenant)
        {
            var commerce7ContactTypeList = new Commerce7ContactTypeList();
            List<Commerce7ContactType> groups = new List<Commerce7ContactType>();
            var groupList = new GroupList();
            var clubList = new ClubList();

            try
            {
                groupList = await Services.Utility.GetCommerce7Groups(Commerce7Username, Commerce7Password, Commerce7Tenant);
                clubList = await Services.Utility.GetCommerce7Clubs(Commerce7Username, Commerce7Password, Commerce7Tenant);

                if (clubList != null && clubList.clubs != null)
                {
                    foreach (var item in clubList.clubs)
                    {
                        var group = new Commerce7ContactType();
                        group.id = item.id;
                        group.title = item.title;
                        group.activeclub = true;
                        groups.Add(group);
                    }
                }
                    
                if (groupList != null && groupList.groups != null)
                {
                    foreach (var item in groupList.groups)
                    {
                        var group = new Commerce7ContactType();
                        group.id = item.id;
                        group.title = item.title;
                        group.activeclub = false;
                        groups.Add(group);
                    }
                }
                

                commerce7ContactTypeList.groups = groups;

            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                int memberId = eventDAL.GetWineryIdByCommerce7Data(Commerce7Username, Commerce7Password);

                logDAL.InsertLog("WebApi", "GetCommerce7Groups:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1, memberId);
            }
            return new ObjectResult(commerce7ContactTypeList);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [Route("CheckCommerce7LoginTest")]
        [HttpGet]
        public async Task<IActionResult> CheckCommerce7LoginTest(string Commerce7Username, string Commerce7Password, string Commerce7Tenant)
        {
            ViewModels.BaseResponse resp = new ViewModels.BaseResponse();

            try
            {
                resp.success = await Services.Utility.CheckCommerce7LoginTest(Commerce7Username, Commerce7Password, Commerce7Tenant);
            }
            catch (Exception ex)
            {
                resp.success = false;
            }
            return new ObjectResult(resp);
        }

        /// <summary>
        /// This method gives user details by email
        /// </summary>
        /// <param name="email">Email of User (Required)</param>
        /// <param name="member_Id">Id of Member (Required)</param>
        /// <returns></returns>
        [Route("userexternaldetails")]
        [HttpGet]
        public async Task<IActionResult> GetUserExternalDetailsbyemail(string email, int member_Id)
        {
            var userDetailResponse = new UserExternalDetailResponse();
            try
            {
                if (member_Id > 0)
                {
                    bool recordfound = false;
                    var userDetailModel = new UserExternalDetailModel();

                    var listuserDetailModel = new List<UserDetailModel>();
                    listuserDetailModel = await Services.Utility.GetUsersByEmail(email, member_Id, true, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);

                    if (listuserDetailModel.Count > 0)
                    {
                        recordfound = true;
                        userDetailModel.ltv = listuserDetailModel[0].ltv;
                        userDetailModel.member_status = listuserDetailModel[0].member_status;
                        userDetailModel.last_order_date = listuserDetailModel[0].last_order_date;
                        userDetailModel.contact_types = listuserDetailModel[0].contact_types;

                        if (listuserDetailModel[0].member_status == true)
                        {
                            userDetailModel.customer_type = 1;
                        }
                        else
                        {
                            userDetailModel.customer_type = 0;
                        }

                        userDetailResponse.success = true;
                        userDetailResponse.data = userDetailModel;
                    }
                    
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    var userModel = new List<UserDetailModel>();
                    int searchType = 1;
                    
                    if (recordfound == false)
                    {
                        userModel = userDAL.GetUsersbykeyword(email, 0, searchType);
                        if (userModel.Count > 0)
                        {
                            recordfound = true;
                            userDetailModel.contact_types = userModel[0].contact_types;
                            userDetailModel.customer_type = userModel[0].customer_type;
                            if (userModel[0].customer_type == 1)
                            {
                                userDetailModel.member_status = true;
                            }
                            else
                            {
                                userDetailModel.member_status = false;
                            }
                            userDetailResponse.success = true;
                            userDetailResponse.data = userDetailModel;
                        }
                    }

                    if (recordfound == false)
                    {
                        userDetailResponse.success = true;
                        userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        userDetailResponse.error_info.extra_info = "no record found";
                    }
                }
                else
                {
                    userDetailResponse.success = true;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetUserExternalDetailsbyemail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_Id);
            }
            return new ObjectResult(userDetailResponse);
        }

        /// <summary>
        /// This method is used to create new user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("createuser")]
        [HttpPost]
        public IActionResult CreateUser([FromBody] UserRequest model)
        {
            var userResponse = new UserResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                var user = new User();

                var userDetailModel = new UserDetailModel();

                userDetailModel = userDAL.GetUserDetailsbyemail(model.email, model.member_id);

                if (userDetailModel.user_id > 0)
                {
                    userResponse.success = true;
                    userResponse.user_id = userDetailModel.user_id;
                    userResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    userResponse.error_info.extra_info = "email address already exists";

                    int UsermemberId = userDAL.CheckUserWinery(model.member_id, userDetailModel.user_id);

                    if (UsermemberId == 0)
                    {
                        userDAL.UpdateUserWinery(userDetailModel.user_id, model.member_id, 4, "", "", "", -1);
                    }
                }
                else
                {
                    int Id = userDAL.CreateUser(model.email, model.password, model.first_name, model.last_name, model.country, model.zip, model.home_phone, 0, 0, 0, "", model.city, model.state, model.address1, model.address2, GetSource(HttpContext.Request.Headers["AuthenticateKey"]), model.concierge_type);
                    if (Id > 0)
                    {
                        userResponse.success = true;
                        userResponse.user_id = Id;
                        userDAL.UpdateUserWinery(Id, model.member_id, 4, "", "", "", -1);
                    }
                    else
                    {
                        userResponse.user_id = Id;
                        userResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                        userResponse.error_info.extra_info = "email address already exists";
                    }
                }
            }
            catch (Exception ex)
            {
                userResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userResponse.error_info.extra_info = Common.Common.InternalServerError;
                userResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CreateUser:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(userResponse);
        }

        [Route("createconciergeaccount")]
        [HttpPost]
        public IActionResult CreateConciergeAccount([FromBody] UserRequest model)
        {
            var userResponse = new UserResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                var user = new User();

                var userDetailModel = new UserDetailModel();

                userDetailModel = userDAL.GetUserDetailsbyemail(model.email, model.member_id);

                if (userDetailModel.user_id > 0)
                {
                    bool alreadyexists = false;
                    foreach (var item in userDetailModel.roles)
                    {
                        if (item == 6)
                        {
                            alreadyexists = true;
                            break;
                        }
                    }

                    userResponse.success = true;
                    userResponse.user_id = userDetailModel.user_id;

                    if (alreadyexists)
                    {
                        userResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                        userResponse.error_info.extra_info = "Concierge account already exists";

                        return new ObjectResult(userResponse);
                    }

                    userDAL.UpdateUserWinery(userDetailModel.user_id, model.member_id, 6, "", "", "", -1);
                }
                else
                {
                    int Id = userDAL.CreateUser(model.email, model.password, model.first_name, model.last_name, model.country, model.zip, model.home_phone, 0, 0, 0, "", model.city, model.state, model.address1, model.address2, GetSource(HttpContext.Request.Headers["AuthenticateKey"]), model.concierge_type,6);
                    if (Id > 0)
                    {
                        userResponse.success = true;
                        userResponse.user_id = Id;
                        userDAL.UpdateUserWinery(Id, model.member_id, 6, "", "", "", -1);
                    }
                    else
                    {
                        userResponse.user_id = Id;
                        userResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                        userResponse.error_info.extra_info = "There was an error saving the Concierge account.";
                    }
                }
            }
            catch (Exception ex)
            {
                userResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userResponse.error_info.extra_info = Common.Common.InternalServerError;
                userResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CreateConciergeAccount:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
            }
            return new ObjectResult(userResponse);
        }

        /// <summary>
        /// This method gives list of users by specific role
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("getusersbyrole")]
        [HttpGet]
        public IActionResult GetUsersbyRole(UsermemberRequest model)
        {
            UsermemberResponse usermemberResponse = new UsermemberResponse();
            try
            {

                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                //List<UserDetailModel> userDetailModel = userDAL.GetUsersByRoleID(model.member_id, model.role_id);
                List<UserDetailModel> userListModel = new List<UserDetailModel>();

                userListModel = userDAL.GetUsersByRoleID(model.member_id, model.role_id);
                if (userListModel != null)
                {
                    usermemberResponse.success = true;
                    usermemberResponse.data = userListModel;
                }
                else
                {
                    usermemberResponse.success = true;
                    usermemberResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    usermemberResponse.error_info.extra_info = "no record found";
                }

                //if (userDetailModel.Any())
                //{
                //    usermemberResponse.success = true;
                //    List<User> userListModel = new List<User>();
                //    foreach (var item in userDetailModel)
                //    {
                //        ViewModels.Address addr = new ViewModels.Address();

                //        addr.address_1 = item.address.address_1;
                //        addr.address_2 = item.address.address_2;
                //        addr.city = item.address.city;
                //        addr.country = item.address.country;
                //        addr.state = item.address.state;
                //        addr.cell_phone = item.cell_phone;
                //        addr.home_phone = item.home_phone;
                //        addr.zip_code = item.address.zip_code;
                //        addr.work_phone = item.work_phone;

                //        User user = new User
                //        {
                //            first_name = item.first_name,
                //            last_name = item.last_name,
                //            user_id = item.id,
                //            user_name = item.user_name,
                //            company = item.company_name,
                //            title = item.title,
                //            address=addr
                //        };
                //        if (!string.IsNullOrWhiteSpace(item.roles))
                //        {
                //            string[] roles = item.roles.Split(",".ToCharArray());
                //            foreach (var role in roles)
                //            {
                //                Common.Common.UserRole userRole = (Common.Common.UserRole)Convert.ToInt32(role);
                //                user.roles.Add(new Role
                //                {
                //                    id = Convert.ToInt32(role),
                //                    role_name = userRole.ToString()
                //                });
                //            }
                //        }
                //        userListModel.Add(user);
                //    }
                //    usermemberResponse.data = userListModel;
                //}
            }
            catch (Exception ex)
            {
                usermemberResponse.success = false;
                usermemberResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                usermemberResponse.error_info.extra_info = Common.Common.InternalServerError;
                usermemberResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetUsersbyRole:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(usermemberResponse);
        }
        /// <summary>
        /// This method check third party contact
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[Route("checkcontactonthirdparty")]
        //[HttpGet]
        //public async Task<IActionResult> CheckContactOnThirdParty(CheckContactRequest model)
        //{
        //    CheckContactResponse checkContactResponse = new CheckContactResponse();
        //    checkContactResponse.success = false;
        //    try
        //    {
        //        string strName = string.Empty;
        //        EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
        //        WineryModel memberModel = await Task.Run(() => eventDAL.GetWineryById(model.member_id));
        //        if (memberModel != null)
        //        {
        //            if (memberModel.EnableVin65 && !string.IsNullOrWhiteSpace(memberModel.Vin65UserName)
        //                && !string.IsNullOrWhiteSpace(memberModel.Vin65Password))
        //            {
        //                await Utility.Vin65GetContacts(model.email, memberModel.Vin65Password, memberModel.Vin65UserName);
        //            }
        //            else if (memberModel.eMemberEnabled && !string.IsNullOrWhiteSpace(memberModel.eMemberUserNAme)
        //                && !string.IsNullOrWhiteSpace(memberModel.eMemberPAssword))
        //            {
        //                eWineryCustomerViewModel ememberCustomerViewModel = await Utility.ResolveCustomer(memberModel.eMemberUserNAme, memberModel.eMemberPAssword, model.email);
        //                checkContactResponse.success = !string.IsNullOrWhiteSpace(ememberCustomerViewModel.member_id);
        //                strName = ememberCustomerViewModel.first_name + " " + ememberCustomerViewModel.last_name + " " + ememberCustomerViewModel.state + " " + ememberCustomerViewModel.city;
        //                checkContactResponse.error_info.description = strName;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        checkContactResponse.success = false;
        //        checkContactResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
        //        checkContactResponse.error_info.extra_info = Common.Common.InternalServerError;
        //        checkContactResponse.error_info.description = ex.Message.ToString();
        //        LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
        //        logDAL.InsertLog("WebApi", "CheckContactOnThirdParty:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"]);
        //    }
        //    return new ObjectResult(checkContactResponse);
        //}
        /// <summary>
        /// This method is used to create contact on third party like Vin65, emember, SalesForce etc
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("createcontactonthirdparty")]
        [HttpPost]
        public async Task<IActionResult> CreateContactOnThirdParty([FromBody]CreateContactRequest model)
        {
            CreateContactResponse createContactResponse = new CreateContactResponse { success = false };
            try
            {
                if (model.ignore_user_sync == false)
                {
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                    UserDetailModel user = userDAL.GetUserDetailsbyemail(model.email, model.member_id);
                    string msg = await Services.Utility.SaveOrUpdateContactThirdParty(model.member_id, user, Common.ReferralType.BackOffice,0, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);

                    if (msg == "success")
                        createContactResponse.success = true;
                    else if (msg == "error")
                        createContactResponse.success = false;
                    else if (msg == "exceeded")
                    {
                        createContactResponse.success = false;
                        createContactResponse.error_info.description = "Exceeded";
                    }
                }
            }
            catch (Exception ex)
            {
                createContactResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                createContactResponse.error_info.extra_info = Common.Common.InternalServerError;
                createContactResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "CreateContactOnThirdParty:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(createContactResponse);
        }

        [Route("getreservationsreservecloud")]
        [HttpGet]
        public async Task<IActionResult> GetReservationsReserveCloud(DateTime selected_date, int member_Id)
        {
            var reservationsReserveCloudResponse = new ReservationsReserveCloudResponse();
            try
            {
                var listrsvp = new List<ReserveCloudReservation>();

                //EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                //WineryModel memberModel = await Task.Run(() => eventDAL.GetWineryById(member_Id));

                //listrsvp = Utility.GetReserveCloudReservation(selected_date, _appSetting.Value.ReserveCloudUrl, memberModel.ReServeCloudSiteId, memberModel.ReServeCloudApiUserName, memberModel.ReServeCloudApiPassword);
                //if (listrsvp != null && listrsvp.Count > 0)
                //{
                //    reservationsReserveCloudResponse.success = true;
                //    reservationsReserveCloudResponse.data = listrsvp;
                //}
                //else
                //{
                //    reservationsReserveCloudResponse.success = true;
                //    reservationsReserveCloudResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                //    reservationsReserveCloudResponse.error_info.extra_info = "no record found";
                //}
            }
            catch (Exception ex)
            {
                reservationsReserveCloudResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationsReserveCloudResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationsReserveCloudResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationsReserveCloud:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_Id);
            }
            return new ObjectResult(reservationsReserveCloudResponse);
        }
        [Route("disableuser")]
        [HttpPost]
        public IActionResult DisableUser([FromBody] DisableUserRequest model)
        {
            var disableUserResponse = new DisableUserResponse();
            try
            {
                if (string.IsNullOrEmpty(model.password) || model.user_id == 0)
                {
                    disableUserResponse.success = false;
                    disableUserResponse.error_info.extra_info = "Password is required and cannot be blank.";
                }
                else
                {
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                    bool ret = userDAL.DisableUser(model.user_id, StringHelpers.EncryptOneWay(model.password));

                    if (ret)
                        disableUserResponse.success = true;
                    else
                    {
                        disableUserResponse.success = false;
                        disableUserResponse.error_info.extra_info = "Please enter valid password.";
                    }
                }
            }
            catch (Exception ex)
            {
                disableUserResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                disableUserResponse.error_info.extra_info = Common.Common.InternalServerError;
                disableUserResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "DisableUser:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(disableUserResponse);
        }

        [Route("contactsales")]
        [HttpPost]
        public async Task<IActionResult> ContactSales([FromBody] ContactSalesRequest model)
        {
            var disableUserResponse = new DisableUserResponse();
            try
            {
                if (string.IsNullOrEmpty(model.name) || string.IsNullOrEmpty(model.email) || string.IsNullOrEmpty(model.subject) || string.IsNullOrEmpty(model.message))
                {
                    disableUserResponse.success = false;
                    disableUserResponse.error_info.extra_info = "Sorry there was a problem sending your message.";
                }
                else
                {
                    Common.MailConfig config = new Common.MailConfig
                    {
                        Domain = _appSetting.Value.MailGunPostUrl,
                        ApiKey = _appSetting.Value.MainGunApiKey
                    };

                    AuthMessageSender messageService = new AuthMessageSender();
                    await messageService.SendContactSalesEmail(model, config);

                    disableUserResponse.success = true;
                }
            }
            catch (Exception ex)
            {
                disableUserResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                disableUserResponse.error_info.extra_info = Common.Common.InternalServerError;
                disableUserResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ContactSales:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(disableUserResponse);
        }

        [Route("sendverificationemail")]
        [HttpPost]
        public async Task<IActionResult> SendVerificationEmail([FromBody] VerificationEmailRequest model)
        {
            var disableUserResponse = new DisableUserResponse();
            try
            {
                if (string.IsNullOrEmpty(model.business_email) || string.IsNullOrEmpty(model.first_name) || string.IsNullOrEmpty(model.last_name) || model.member_id == 0)
                {
                    disableUserResponse.success = false;
                    disableUserResponse.error_info.extra_info = "Invalid data. Business email, first name, last name and member Id are required and cannot be blank.";
                }
                else
                {
                    Common.MailConfig config = new Common.MailConfig
                    {
                        Domain = _appSetting.Value.MailGunPostUrl,
                        ApiKey = _appSetting.Value.MainGunApiKey
                    };

                    AuthMessageSender messageService = new AuthMessageSender();
                    await messageService.ProcessVerificationEmail(model, config);

                    disableUserResponse.success = true;
                }
            }
            catch (Exception ex)
            {
                disableUserResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                disableUserResponse.error_info.extra_info = Common.Common.InternalServerError;
                disableUserResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SendVerificationEmail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(disableUserResponse);
        }

        [Route("verifyaccount")]
        [HttpPost]
        public async Task<IActionResult> VerifyAccount([FromBody] VerifyAccountRequest model)
        {
            var disableUserResponse = new DisableUserResponse();
            try
            {
                if (string.IsNullOrEmpty(model.verification_code))
                {
                    disableUserResponse.success = false;
                    disableUserResponse.error_info.extra_info = "Verification is required and cannot be blank.";
                }
                else
                {
                    NameValueCollection decrypted = StringHelpers.DecryptQueryString(model.verification_code);

                    if (decrypted != null && decrypted.Count > 0)
                    {
                        //VerificationType type = VerificationType.NA;
                        //Int32.TryParse(decrypted[VerificationFields.type.ToString()], ref type);

                        string verifyEmail = decrypted[VerificationFields.email.ToString()];
                        int memberId = Int32.Parse(decrypted[VerificationFields.id.ToString()]);
                        VerificationType verificationType = (VerificationType)Int32.Parse(decrypted[VerificationFields.type.ToString()]);

                        if (verificationType == VerificationType.Subscription && memberId > 0)
                        {
                            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                            var winery = eventDAL.GetWineryById(memberId);

                            if (winery != null && winery.EmailAddress.Trim().ToLower().Equals(verifyEmail.Trim().ToLower()) && !winery.IsActive)
                            {
                                if (eventDAL.FinishMemberSignupProcess(memberId))
                                {
                                    QueueService getStarted = new QueueService();

                                    var queueModel = new EmailQueue();
                                    queueModel.EType = (int)EmailType.SysSubscriptionSignup;
                                    queueModel.RsvpId = memberId;

                                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                                    var userDetail = userDAL.GetUserDetailsbyemail(verifyEmail);

                                    if (userDetail != null && userDetail.date_created.AddHours(24) > DateTime.Now && !string.IsNullOrWhiteSpace(userDetail.password_change_key))
                                    {
                                        queueModel.BCode = userDetail.password_change_key;
                                    }

                                    var qData = JsonConvert.SerializeObject(queueModel);

                                    AppSettings _appsettings = _appSetting.Value;
                                    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                                }
                                

                            }

                        }

                       //await messageService.ProcessVerificationEmail(model, config);

                        disableUserResponse.success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                disableUserResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                disableUserResponse.error_info.extra_info = Common.Common.InternalServerError;
                disableUserResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "VerifyAccount:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(disableUserResponse);
        }

        [Route("sendnewuserverificationemail")]
        [HttpPost]
        public async Task<IActionResult> SystemNewUser([FromBody] SystemNewUserRequest model)
        {
            var disableUserResponse = new DisableUserResponse();
            try
            {
                if (string.IsNullOrEmpty(model.email))
                {
                    disableUserResponse.success = false;
                    disableUserResponse.error_info.extra_info = "Email is required and cannot be blank.";
                }
                else
                {
                    MailConfig config = new MailConfig
                    {
                        Domain = _appSetting.Value.MailGunPostUrl,
                        ApiKey = _appSetting.Value.MainGunApiKey
                    };
                    ReservationEmailModel model1 = new ReservationEmailModel
                    {
                        CCGuestEmail = model.email,
                        data = new ReservationEmailModel
                        {
                            CCGuestEmail = model.email
                        }
                    };
                    model1.MailConfig = config;
                    AuthMessageSender messageService = new AuthMessageSender();
                    var response = await messageService.SendSystemNewUser(model1);
                    disableUserResponse.success = true;
                }
            }
            catch (Exception ex)
            {
                disableUserResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                disableUserResponse.error_info.extra_info = Common.Common.InternalServerError;
                disableUserResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SystemNewUser:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(disableUserResponse);
        }

        [Route("verifynewuser")]
        [HttpPost]
        public async Task<IActionResult> VerifyAccountForUser([FromBody] VerifyAccountRequest model)
        {
            var disableUserResponse = new DisableUserResponse();
            try
            {
                disableUserResponse.success = false;

                if (string.IsNullOrEmpty(model.verification_code))
                {
                    disableUserResponse.error_info.extra_info = "Verification Code is required and cannot be blank.";
                }
                else
                {
                    string verifyEmail = StringHelpers.Decrypt(model.verification_code);

                    UserDAL userDAL = new UserDAL(ConnectionString);
                    UserDetailModel userDetailModel = userDAL.GetUserDetailsbyemail(verifyEmail);

                    if (userDetailModel != null && userDetailModel.user_id > 0)
                    {
                        MailConfig config = new MailConfig
                        {
                            Domain = _appSetting.Value.MailGunPostUrl,
                            ApiKey = _appSetting.Value.MainGunApiKey
                        };
                        ReservationEmailModel model1 = new ReservationEmailModel
                        {
                            CCGuestEmail = verifyEmail,
                            data = new ReservationEmailModel
                            {
                                CCGuestEmail = verifyEmail
                            }
                        };
                        model1.MailConfig = config;
                        AuthMessageSender messageService = new AuthMessageSender();
                        var response = await messageService.ProcessSendSysNewAdminUser(model1);

                        disableUserResponse.success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                disableUserResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                disableUserResponse.error_info.extra_info = Common.Common.InternalServerError;
                disableUserResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "verifynewuser:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(disableUserResponse);
        }

        [Route("clubtypes")]
        [HttpGet]
        public async Task<IActionResult> GetClubTypes(string user_name, string password)
        {
            var clubTypesResponse = new ClubTypesResponse();
            try
            {
                var accountType = new List<AccountType>();
                accountType = await Services.Utility.GetClubTypes(user_name, password);

                if (accountType != null && accountType.Count > 0)
                {
                    clubTypesResponse.success = true;
                    clubTypesResponse.data = accountType;
                }
                else
                {
                    clubTypesResponse.success = false;
                    clubTypesResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    clubTypesResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                clubTypesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                clubTypesResponse.error_info.extra_info = Common.Common.InternalServerError;
                clubTypesResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetClubTypes:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(clubTypesResponse);
        }

        [Route("checkandformatphonenumber")]
        [HttpGet]
        public async Task<IActionResult> CheckAndFormatPhoneNumber(string mobile_number, string country_code="US",int mobile_number_format = 0,bool do_twilio_lookup = false)
        {
            var checkAndFormatPhoneNumberResponse = new CheckAndFormatPhoneNumberResponse();
            try
            {
                CheckAndFormatPhoneNumberModel checkAndFormatPhoneNumberModel = new CheckAndFormatPhoneNumberModel();

                string FormatedPhoneNumber = StringHelpers.FormatTelephoneCommerce7(mobile_number, country_code, true, (PhoneNumbers.PhoneNumberFormat)mobile_number_format);

                checkAndFormatPhoneNumberResponse.success = true;
                if (!string.IsNullOrEmpty(FormatedPhoneNumber))
                {
                    checkAndFormatPhoneNumberModel.formated_mobile_number = FormatedPhoneNumber;

                    int UserId = 0;

                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                    string ExtractNumber = StringHelpers.ExtractNumber(FormatedPhoneNumber);

                    checkAndFormatPhoneNumberModel.mobile_number_status = userDAL.GetMobilePhoneStatusByMobilePhoneNumber(ExtractNumber, ref UserId);

                    if (do_twilio_lookup && checkAndFormatPhoneNumberModel.mobile_number_status != MobileNumberStatus.verified)
                    {
                        checkAndFormatPhoneNumberModel.mobile_number_status = Services.Utility.SMSVerified_System(FormatedPhoneNumber);

                        if (UserId>0)
                            userDAL.UpdateMobilePhoneStatusById(ExtractNumber, (int)checkAndFormatPhoneNumberModel.mobile_number_status);
                    }
                }
                else
                {
                    checkAndFormatPhoneNumberModel.formated_mobile_number = "";
                    checkAndFormatPhoneNumberModel.mobile_number_status = MobileNumberStatus.failed;
                }

                checkAndFormatPhoneNumberResponse.data = checkAndFormatPhoneNumberModel;
                checkAndFormatPhoneNumberResponse.success = true;
            }
            catch (Exception ex)
            {
                checkAndFormatPhoneNumberResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                checkAndFormatPhoneNumberResponse.error_info.extra_info = Common.Common.InternalServerError;
                checkAndFormatPhoneNumberResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "CheckAndFormatPhoneNumber:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(checkAndFormatPhoneNumberResponse);
        }

        [Route("testcredentials")]
        [HttpGet]
        public async Task<IActionResult> TestCredentials(string user_name,string password, ThirdPartyType thirdPartyType)
        {
            var response = new ViewModels.BaseResponse();
            try
            {
                if (thirdPartyType == ThirdPartyType.eWinery)
                {
                    response.success = await Services.Utility.eWineryTestCredentials(user_name, password);
                }
                else if (thirdPartyType == ThirdPartyType.vin65)
                {

                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "TestCredentials:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(response);
        }

        [Route("usersync")]
        [HttpGet]
        public async Task<IActionResult> TaskUserSync()
        {
            var baseResponse = new ViewModels.BaseResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            int memberid = 0;
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                string msg = string.Empty;

                DateTime localDateTime= Times.ToTimeZoneTime(DateTime.UtcNow);
                string pstLocalTimeAMPM = localDateTime.ToString("hh:mmtt");
                string pstLocalTime24hr = localDateTime.ToString("HH:mm");

                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Common.Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.usersync).ToList();
                if ((settingsGroup != null))
                {
                    if (settingsGroup.Count > 0)
                    {
                        string scheduleForSyncing = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.usersync_schedule);

                        List<string> lstTimes = ReadAndParseUserSyncSchedule(scheduleForSyncing);

                        if (lstTimes.Count > 0 && (lstTimes.Contains(pstLocalTimeAMPM) || lstTimes.Contains(pstLocalTime24hr)))
                        {
                            logDAL.InsertLog("WebApi", "TaskUserSync:  User Sync task started at " + pstLocalTimeAMPM, HttpContext.Request.Headers["AuthenticateKey"], 3, 0);

                            List <UsersForSync> list = userDAL.GetUserDetailsForSync();

                            foreach (var item in list)
                            {
                                memberid = item.MemberId;
                                UserDetailModel user = userDAL.GetUserDetailsbyemail(item.UserName, item.MemberId);

                                msg = await Services.Utility.SaveOrUpdateContactThirdParty(item.MemberId, user, Common.ReferralType.BackOffice, 0, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);

                                if (msg == "exceeded")
                                    break;

                                userDAL.UpdateUserThirdPartyAutoSync(item.Id);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                baseResponse.success = false;
                baseResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                baseResponse.error_info.extra_info = Common.Common.InternalServerError;
                baseResponse.error_info.description = ex.Message.ToString();

                logDAL.InsertLog("WebApi", "TaskUserSync:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,memberid);
            }

            return new ObjectResult(baseResponse);
        }

        private List<string> ReadAndParseUserSyncSchedule(string userSyncSched)
        {
            List<string> lstTimes = new List<string>();

            string[] arrTimes = userSyncSched.Split(",");

            if (arrTimes.Length > 0)
            {
                foreach (string time in arrTimes)
                {
                    try
                    {
                        DateTime dt = DateTime.Parse("01/01/2019 " + time.Trim());
                        lstTimes.Add(time.Trim());
                    }
                    catch { }
                }
            }
            return lstTimes;
        }

        [Route("userbyusername")]
        [HttpGet]
        public async Task<IActionResult> GetUserByUserName(string email, int member_Id)
        {
            var userResponse = new UserByUserNameResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString_readonly);

                var userDetailModel = new User2Model();

                userDetailModel = userDAL.GetUserByUserName(email, member_Id);
                if (userDetailModel != null)
                {
                    userResponse.success = true;
                    userResponse.data = userDetailModel;
                }
                else
                {
                    userResponse.success = true;
                    userResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userResponse.error_info.extra_info = Common.Common.InternalServerError;
                userResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetUserByUserName:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_Id);
            }
            return new ObjectResult(userResponse);
        }



        [Route("list_by_keyword")]
        [HttpGet]
        public async Task<IActionResult> GetListByKeyword(string firstName, string lastName, string email, string mobile, int member_id = 0)
        {
            var userDetailResponse = new UserListResponse();
            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString_readonly);
            try
            {
                var userDetailModel = new List<UserDetailModel>();
                string FormatPhoneNumber = Services.Utility.FormatPhoneNumber(mobile);
                int search_type = 3;
                string keyword = "";
                firstName = firstName == null ? "" : firstName;
                lastName = lastName == null ? "" : lastName;
                email = email == null ? "" : email;
                mobile = mobile == null ? "" : mobile;
                if (FormatPhoneNumber.Length > 0 && FormatPhoneNumber != "0" && mobile.IndexOf("@") == -1)
                {                
                    mobile = FormatPhoneNumber;
                }  
                userDetailModel =  userDAL.GetUsersbykeyword(keyword,member_id, search_type, firstName, lastName, email, mobile);

                if (userDetailModel != null && userDetailModel.Count > 0)
                {
                    userDetailResponse.success = true;
                    userDetailResponse.data = userDetailModel;
                }
                else
                {
                    userDetailResponse.success = true;
                    userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    userDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                userDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                userDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetListByKeyword:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(userDetailResponse);
        }


        [Route("user_guest_tags")]
        [HttpGet]
        public IActionResult GetUserGuestTags(int user_id, int member_id)
        {
            var tagResponse = new GuestTagResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString_readonly);
                var tag = new List<GuestTagModel>();
                tag = userDAL.GetUserGuestTags(user_id, member_id);

                if (tag != null)
                {
                    tagResponse.success = true;
                    tagResponse.data = tag;
                }
                else
                {
                    tagResponse.success = true;
                    tagResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    tagResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                tagResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                tagResponse.error_info.extra_info = Common.Common.InternalServerError;
                tagResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetUserGuestTags:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(tagResponse);
        }

        [Route("getguestsbyrole")]
        [HttpGet]
        public IActionResult GetGuestsByWineryAndRole(int member_id,string searchvalue)
        {
            GuestmemberResponse usermemberResponse = new GuestmemberResponse();
            try
            {

                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                List<GuestDetailModel> userListModel = new List<GuestDetailModel>();

                userListModel = userDAL.GetGuestsByWineryAndRole(member_id, searchvalue);
                if (userListModel != null)
                {
                    usermemberResponse.success = true;
                    usermemberResponse.data = userListModel;
                }
                else
                {
                    usermemberResponse.success = true;
                    usermemberResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    usermemberResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                usermemberResponse.success = false;
                usermemberResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                usermemberResponse.error_info.extra_info = Common.Common.InternalServerError;
                usermemberResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetGuestsByWineryAndRole:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(usermemberResponse);
        }

        [Route("my_account_details")]
        [HttpGet]
        public IActionResult GetMyAccountDetails(int user_id)
        {
            var myAccountDetailsResponse = new MyAccountDetailsResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString_readonly);
                var myAccountDetailsModel = new MyAccountDetailsV2Model();
                myAccountDetailsModel = userDAL.GetMyAccountDetailsV2(user_id);

                if (myAccountDetailsModel != null)
                {
                    myAccountDetailsResponse.success = true;
                    myAccountDetailsResponse.data = myAccountDetailsModel;
                }
                else
                {
                    myAccountDetailsResponse.success = true;
                    myAccountDetailsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    myAccountDetailsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                myAccountDetailsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                myAccountDetailsResponse.error_info.extra_info = Common.Common.InternalServerError;
                myAccountDetailsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetMyAccountDetails:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, user_id);
            }
            return new ObjectResult(myAccountDetailsResponse);
        }


        [Route("setfavoritemember")]
        [HttpPost]
        public IActionResult SetUserFavorites([FromBody] UserFavoriteRequestModel model)
        {
            int member_id = model.member_id;
            int user_id = model.user_id;
            int event_id = 0;
            var setUserFavoritesResponse = new SetUserFavoritesResponse();
            try
            {
                if(member_id > 0 && user_id > 0)
                {
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    bool setUserFavorites = false;
                    setUserFavorites = userDAL.SetUserFavorites(member_id, user_id, event_id);

                    if (setUserFavorites == true)
                    {
                        setUserFavoritesResponse.success = true;
                    }
                    else
                    {
                        setUserFavoritesResponse.success = true;
                        setUserFavoritesResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        setUserFavoritesResponse.error_info.extra_info = "no record found";
                    }
                }
                else
                {
                    setUserFavoritesResponse.success = true;
                    setUserFavoritesResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    setUserFavoritesResponse.error_info.extra_info = "Need valid memberId and UserId";
                    setUserFavoritesResponse.error_info.description = "Need valid memberId and UserId";
                }

            }
            catch (Exception ex)
            {
                setUserFavoritesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                setUserFavoritesResponse.error_info.extra_info = Common.Common.InternalServerError;
                setUserFavoritesResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "SetUserFavorites:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, user_id);
            }
            return new ObjectResult(setUserFavoritesResponse);
        }

        [Route("removefavoritemember")]
        [HttpPost]
        public IActionResult RemoveUserFavorites([FromBody] UserFavoriteRequestModel model)
        {
            int member_id = model.member_id;
            int user_id = model.user_id;
            int event_id = 0;
            var setUserFavoritesResponse = new SetUserFavoritesResponse();
            try
            {
                if (member_id > 0 && user_id > 0)
                {
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    bool setUserFavorites = false;
                    setUserFavorites = userDAL.RemoveUserFavorites(member_id, user_id, event_id);

                    if (setUserFavorites == true)
                    {
                        setUserFavoritesResponse.success = true;
                    }
                    else
                    {
                        setUserFavoritesResponse.success = true;
                        setUserFavoritesResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        setUserFavoritesResponse.error_info.extra_info = "no record found";
                    }
                }
                else
                {
                    setUserFavoritesResponse.success = true;
                    setUserFavoritesResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    setUserFavoritesResponse.error_info.extra_info = "Need valid memberId and UserId";
                    setUserFavoritesResponse.error_info.description = "Need valid memberId and UserId";
                }

            }
            catch (Exception ex)
            {
                setUserFavoritesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                setUserFavoritesResponse.error_info.extra_info = Common.Common.InternalServerError;
                setUserFavoritesResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "removefavoritemember:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, user_id);
            }
            return new ObjectResult(setUserFavoritesResponse);
        }

        [Route("setfavoriteevent")]
        [HttpPost]
        public IActionResult SetUserFavoritesEvent([FromBody] UserFavoriteEventRequestModel model)
        {
            int member_id = 0;
            int user_id = model.user_id;
            int event_id = model.event_id;
            var setUserFavoritesResponse = new SetUserFavoritesResponse();
            try
            {
                if (event_id > 0 && user_id > 0)
                {
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    bool setUserFavorites = false;
                    setUserFavorites = userDAL.SetUserFavorites(member_id, user_id, event_id);

                    if (setUserFavorites == true)
                    {
                        setUserFavoritesResponse.success = true;
                    }
                    else
                    {
                        setUserFavoritesResponse.success = true;
                        setUserFavoritesResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        setUserFavoritesResponse.error_info.extra_info = "no record found";
                    }
                }
                else
                {
                    setUserFavoritesResponse.success = true;
                    setUserFavoritesResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    setUserFavoritesResponse.error_info.extra_info = "Need valid memberId and UserId";
                    setUserFavoritesResponse.error_info.description = "Need valid memberId and UserId";
                }

            }
            catch (Exception ex)
            {
                setUserFavoritesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                setUserFavoritesResponse.error_info.extra_info = Common.Common.InternalServerError;
                setUserFavoritesResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "SetUserFavorites:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, user_id);
            }
            return new ObjectResult(setUserFavoritesResponse);
        }

        [Route("removefavoriteevent")]
        [HttpPost]
        public IActionResult RemoveUserFavoritesEvent([FromBody] UserFavoriteEventRequestModel model)
        {
            int member_id = 0;
            int user_id = model.user_id;
            int event_id = model.event_id;
            var setUserFavoritesResponse = new SetUserFavoritesResponse();
            try
            {
                if (event_id > 0 && user_id > 0)
                {
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    bool setUserFavorites = false;
                    setUserFavorites = userDAL.RemoveUserFavorites(member_id, user_id, event_id);

                    if (setUserFavorites == true)
                    {
                        setUserFavoritesResponse.success = true;
                    }
                    else
                    {
                        setUserFavoritesResponse.success = true;
                        setUserFavoritesResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        setUserFavoritesResponse.error_info.extra_info = "no record found";
                    }
                }
                else
                {
                    setUserFavoritesResponse.success = true;
                    setUserFavoritesResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    setUserFavoritesResponse.error_info.extra_info = "Need valid memberId and UserId";
                    setUserFavoritesResponse.error_info.description = "Need valid memberId and UserId";
                }

            }
            catch (Exception ex)
            {
                setUserFavoritesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                setUserFavoritesResponse.error_info.extra_info = Common.Common.InternalServerError;
                setUserFavoritesResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "removefavoritemember:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, user_id);
            }
            return new ObjectResult(setUserFavoritesResponse);
        }

        [Route("user_newsletters")]
        [HttpGet]
        public IActionResult GetUserNewsletters(int user_id)
        {
            var newsletterResponse = new UserNewslettersResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString_readonly);
                var tag = new List<UserNewsletterModel>();
                tag = userDAL.GetNewslettersByUserId(user_id);

                if (tag != null)
                {
                    newsletterResponse.success = true;
                    newsletterResponse.data = tag;
                }
                else
                {
                    newsletterResponse.success = true;
                    newsletterResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    newsletterResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                newsletterResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                newsletterResponse.error_info.extra_info = Common.Common.InternalServerError;
                newsletterResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetUserNewsletters:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(newsletterResponse);
        }

        [Route("subscribenewsletter")]
        [HttpPost]
        public IActionResult SubscribedUserNewsletter([FromBody] SubscribedUserNewsletterModel model)
        {
            var response = new BaseResponse2();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                response.success = true;
                userDAL.SubscribedUserNewsletter(model.user_id, model.newsletter_ids);
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateUserNewsletter:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }

        [Route("myaccountdata_v4")]
        [HttpGet]
        public IActionResult GetMyAccountData(int user_Id)
        {
            var myAccountDataResponse = new MyAccountDataResponse();
            try
            {
                if (user_Id > 0)
                {
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString_readonly);
                    var myAccountDataViewModel = new MyAccountDataViewModel();

                    myAccountDataViewModel = userDAL.GetMyAccountData(user_Id);

                    if (myAccountDataViewModel != null)
                    {
                        if (string.IsNullOrEmpty(myAccountDataViewModel.image_url))
                        {
                            string hashedEmail = "";

                            hashedEmail = StringHelpers.GetMd5Hash(myAccountDataViewModel.user_name);

                            myAccountDataViewModel.image_url = String.Format("https://www.gravatar.com/avatar/{0}?s={1}&d={2}&r={3}", hashedEmail, 100, "mm", "G");
                        }
                        else
                            myAccountDataViewModel.image_url = StringHelpers.GetImagePath(ImageType.user, ImagePathType.azure) + "/" + myAccountDataViewModel.image_url + "?nc=" + DateTime.UtcNow.Millisecond.ToString();

                        myAccountDataResponse.success = true;
                        myAccountDataResponse.data = myAccountDataViewModel;
                    }
                    else
                    {
                        myAccountDataResponse.success = true;
                        myAccountDataResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        myAccountDataResponse.error_info.extra_info = "no record found";
                    }
                }
                else {
                    myAccountDataResponse.success = false;
                    myAccountDataResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    myAccountDataResponse.error_info.extra_info = "Need a valid userId";
                }
            }
            catch (Exception ex)
            {
                myAccountDataResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                myAccountDataResponse.error_info.extra_info = Common.Common.InternalServerError;
                myAccountDataResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetMyAccountData:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(myAccountDataResponse);
        }

        [Route("getuseraffiliate")]
        [HttpGet]
        public IActionResult GetUsersbyAffiliateId(int user_Id)
        {
            UsersbyAffiliateIdResponse usermemberResponse = new UsersbyAffiliateIdResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString_readonly);
                List<UserDetail2Model> userListModel = new List<UserDetail2Model>();

                userListModel = userDAL.GetUsersbyAffiliateId(user_Id);
                if (userListModel != null)
                {
                    usermemberResponse.success = true;
                    usermemberResponse.data = userListModel;
                }
                else
                {
                    usermemberResponse.success = true;
                    usermemberResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    usermemberResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                usermemberResponse.success = false;
                usermemberResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                usermemberResponse.error_info.extra_info = Common.Common.InternalServerError;
                usermemberResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetUsersbyAffiliateId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(usermemberResponse);
        }

        [Route("getuserbyid")]
        [HttpGet]
        public async Task<IActionResult> GetUsersbyId(int user_Id)
        {
            UserdetailResponse usermemberResponse = new UserdetailResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString_readonly);
                UserDetail2Model userDetailModel = new UserDetail2Model();

                userDetailModel = userDAL.GetUsersbyId(user_Id);
                if (userDetailModel != null)
                {
                    if (userDetailModel.birth_date.Year > 1900)
                    {
                        userDetailModel.age = DateTime.Now.Year - userDetailModel.birth_date.Year;
                        if (DateTime.Now.DayOfYear < userDetailModel.birth_date.DayOfYear)
                            userDetailModel.age = userDetailModel.age - 1;
                    }

                    //SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                    //List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)SettingGroup.mailchimp_cp);
                    //string mcAPIKey = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_key);
                    //string mcStore = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_adminList);

                    ////call routine and pass data
                    //MailChimpAPI mailChimpAPI = new MailChimpAPI(mcAPIKey, mcStore, "", "", "", mcStore, "");
                    //userDetailModel.weekly_newsletter = await mailChimpAPI.CheckMemberSubscribed(userDetailModel.email);

                    usermemberResponse.success = true;
                    usermemberResponse.data = userDetailModel;
                }
                else
                {
                    usermemberResponse.success = true;
                    usermemberResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    usermemberResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                usermemberResponse.success = false;
                usermemberResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                usermemberResponse.error_info.extra_info = Common.Common.InternalServerError;
                usermemberResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetUsersbyAffiliateId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(usermemberResponse);
        }

        [Route("getusersbyconciergeid")]
        [HttpGet]
        public IActionResult GetUsersbyConciergeId(int concierge_id)
        {
            ConciergeUserResponse usermemberResponse = new ConciergeUserResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString_readonly);
                List<ConciergeUserDetailModel> userListModel = new List<ConciergeUserDetailModel>();

                userListModel = userDAL.GetUsersbyConciergeId(concierge_id);
                if (userListModel != null)
                {
                    usermemberResponse.success = true;
                    usermemberResponse.data = userListModel;
                }
                else
                {
                    usermemberResponse.success = true;
                    usermemberResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    usermemberResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                usermemberResponse.success = false;
                usermemberResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                usermemberResponse.error_info.extra_info = Common.Common.InternalServerError;
                usermemberResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetUsersbyConciergeId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(usermemberResponse);
        }

        [Route("markuserfavoriteforconcierge")]
        [HttpPost]
        public IActionResult UpdateConciergeUser([FromBody] UpdateConciergeUserModel model)
        {
            var response = new BaseResponse2();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                response.success = true;
                userDAL.UpdateConciergeUser(model.user_id, model.concierge_id,model.is_favorite);
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateConciergeUser:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }

        [Route("favoritemembers")]
        [HttpGet]
        public IActionResult GetFavoriteMembers(int user_id)
        {
            favoritememberResponse response = new favoritememberResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString_readonly);
                List<UserFavoriteMemberViewModel> userListModel = new List<UserFavoriteMemberViewModel>();

                userListModel = userDAL.GetFavoriteMembers(user_id);
                if (userListModel != null)
                {
                    response.success = true;
                    response.data = userListModel;
                }
                else
                {
                    response.success = true;
                    response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    response.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetFavoriteMembers:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }

        [Route("favoriteevents")]
        [HttpGet]
        public IActionResult GetFavoriteEvents(int user_id)
        {
            favoriteeventsResponse response = new favoriteeventsResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString_readonly);
                List<FavoriteEventViewModel> userListModel = new List<FavoriteEventViewModel>();

                userListModel = userDAL.GetFavoriteEvents(user_id);
                if (userListModel != null)
                {
                    response.success = true;
                    response.data = userListModel;
                }
                else
                {
                    response.success = true;
                    response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    response.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetFavoriteEvents:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }

        [Route("saveuserimage")]
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult SaveUserImage([FromForm] int user_id)
        {
            var response = new BaseResponse2();
            if (user_id <= 0)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Invalid User Id";
                response.error_info.description = "Invalid User Id";
                return new ObjectResult(response);
            }
            var file = Request.Form.Files[0];
            if (file == null || file.Length == 0)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "No logo file found";
                response.error_info.description = "No logo file found";
                return new ObjectResult(response);
            }
            string fileName = string.Format("{0}_profile.jpg", user_id);
            string logoURL = "";
            using (var memoryStream = new MemoryStream())
            {
                file.OpenReadStream().CopyTo(memoryStream);
                byte[] bytes = memoryStream.ToArray();
                logoURL = Services.Utility.UploadFileToStorage(bytes, fileName, ImageType.user);
            }

            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

            try
            {
                bool isSucess = userDAL.SaveUserImage(user_id, fileName);

                if (isSucess)
                {
                    response.success = true;
                }
                else
                {
                    response.success = false;
                    response.error_info.error_type = (int)Common.Common.ErrorType.None;
                    response.error_info.extra_info = "Error uploading user image";
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SaveUserImage:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }

        [Route("updateaccount")]
        [HttpPost]
        public IActionResult UpdateAccount([FromBody] UpdateAccountRequest model)
        {
            var userResponse = new UpdateAccountResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                if (model.cell_phone_str == null)
                {
                    model.cell_phone_str = model.home_phone_str;
                }

                userDAL.UpdateAccount(model);

                userResponse.email = model.email;
                userResponse.success = true;
            }
            catch (Exception ex)
            {
                userResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userResponse.error_info.extra_info = Common.Common.InternalServerError;
                userResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateAccount:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(userResponse);
        }

        [Route("changepassword")]
        [HttpPost]
        public IActionResult ChangePassword([FromBody] ChangePasswordModel model)
        {
            var userResponse = new UpdateAccountResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                string encryptpassword = StringHelpers.EncryptOneWay(model.password);

                userDAL.ChangePassword(model.email,encryptpassword);

                userResponse.email = model.email;
                userResponse.success = true;
            }
            catch (Exception ex)
            {
                userResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                userResponse.error_info.extra_info = Common.Common.InternalServerError;
                userResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ChangePassword:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(userResponse);
        }

        [Route("createaccount")]
        [HttpPost]
        public IActionResult CreateAccount([FromBody] CreateAccountModel model)
        {
            var clientResponse = new LoginResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                string username = model.user_name;
                string password = model.password;

                string errorMessage = string.Empty;

                bool EmailAlreadyExist = userDAL.EmailAlreadyExists(username);

                if (EmailAlreadyExist)
                {
                    if (model.is_concierge)
                    {
                        errorMessage= "You are already registered with CellarPass. If you would like to sign up as a concierge, please visit the <a>concierge</a> page.";
                    }
                    else
                    {
                        errorMessage= "A user account with that email already exists. Please login with your existing account or use the forgot password feature to reset your password.";
                    }

                    clientResponse.success = false;
                    clientResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    clientResponse.error_info.extra_info = errorMessage;
                    return new ObjectResult(clientResponse);
                }
                else
                {
                    model.password = StringHelpers.EncryptOneWay(model.password);
                    userDAL.CreateUser(model);
                }

                var user = new UserSessionModel();

                user = userDAL.GetUser(username, password, out errorMessage);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    clientResponse.success = false;
                    clientResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    clientResponse.error_info.extra_info = errorMessage;
                    return new ObjectResult(clientResponse);
                }

                user.token_expiry = Convert.ToDouble(_appSetting.Value.JwtExpireMinutes);
                user.token = GenerateJwtToken(user.user_id, user.user_name);

                clientResponse.success = true;
                clientResponse.data = user;
            }
            catch (Exception ex)
            {
                clientResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                clientResponse.error_info.extra_info = Common.Common.InternalServerError;
                clientResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CreateAccount:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(clientResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        private string GenerateJwtToken(int UserId, string UserName)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sid,UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSetting.Value.JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_appSetting.Value.JwtExpireMinutes));

            var token = new JwtSecurityToken(
                _appSetting.Value.JwtIssuer,
                _appSetting.Value.JwtIssuer,
                claims,
                expires: expires,
                notBefore: DateTime.UtcNow,
                signingCredentials: creds
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;

        }

        [Route("validateemail")]
        [HttpGet]
        public async Task<IActionResult> ValidateEmail(string email)
        {
            BaseResponse2 response = new BaseResponse2();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                bool isValid = true;
                var status = eventDAL.IsValidEmail(email);
                if (status == EmailValidStatus.na)
                {
                    //not found in local DB, check with mailgun
                    //Common.MailConfig config = new Common.MailConfig
                    //{
                    //    Domain = _appSetting.Value.MailGunPostUrl,
                    //    ApiKey = _appSetting.Value.MainGunApiKey
                    //};

                    AuthMessageSender messageService = new AuthMessageSender();

                    //isValid = await messageService.CheckEmailValid(config, email);

                    isValid = await messageService.ValidateEmail(email);
                }
                else
                {
                    isValid = (status == EmailValidStatus.valid);
                }

                response.success = isValid;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ValidateEmail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }

        [Route("saveemailoption")]
        [HttpPost]
        public async Task<IActionResult> SaveEmailOption([FromBody] SaveEmailOptionModel model)
        {
            var response = new BaseResponse2();
            try
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)SettingGroup.mailchimp_cp);
                string mcAPIKey = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_key);
                string mcStore = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_adminList);

                //call routine and pass data
                MailChimpAPI mailChimpAPI = new MailChimpAPI(mcAPIKey, mcStore, "", "", "", mcStore, "");

                //var cust = mailChimpAPI.CheckAndCreateCustomer(model.user_id.ToString(), model.first_name, model.last_name, model.email);

                int member = Task.Run(() => mailChimpAPI.CheckAndCreateMember(model.email, false, 0,model.is_subscribed)).Result;

                if (member > 0)
                {
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    userDAL.UpdateWeeklyNewsletter(model.email, model.is_subscribed);
                }
                response.success = true;
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SaveEmailOption:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }
    }
}
