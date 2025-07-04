using System.Linq;
using CPReservationApi.Model;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System;
using CPReservationApi.Common;
using static CPReservationApi.Common.Email;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;


namespace CPReservationApi.DAL
{
    public class UserDAL : BaseDataAccess
    {
        public UserDAL(string connectionString) : base(connectionString)
        {
        }


        public bool UpdateGatewayCustId(int userId, string custId)
        {
            string sql = "update [User] set GatewayCustId=@custId where Id=@UserId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", userId));
            parameterList.Add(GetParameter("@custId", custId));

            int ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);

            return (ret > 0);

        }

        public bool UpdateGatewayCustId(string userName, string custId)
        {
            string sql = "update [User] set GatewayCustId=@custId where UserName=@userName";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@userName", userName));
            parameterList.Add(GetParameter("@custId", custId));

            int ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);

            return (ret > 0);

        }

        public string GetGatewayCustId(int userId)
        {
            string custId = "";
            string sql = "Select Isnull(GatewayCustId, '') GatewayCustId from [User] where Id=@UserId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", userId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        custId = Convert.ToString(dataReader["GatewayCustId"]);
                    }
                }
            }

            return custId;

        }

        public bool UpdateFavoriteRegion(int userId, int regionId)
        {
            string sql = "update [User] set PreferredAppellation=@RegionId where Id=@UserId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", userId));
            parameterList.Add(GetParameter("@RegionId", regionId));

            int ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);

            return (ret > 0);

        }

        public string GetCountryISOByCountryName(string countryName)
        {
            string sql = "Select top 1 Iso from [Country] where Name=@Country or ISO3=@Country";
            string countryISO = "";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Country", countryName));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        countryISO = Convert.ToString(dataReader["Iso"]);
                    }
                }
            }

            return countryISO.Trim();
        }
        public UserDetailModel GetUserDetailsbyemail(string email, int WineryID = 0,int UserID=0)
        {
            bool covidSurveyEnabled = false;
            bool covidWaiverEnabled = false;
            if (WineryID > 0)
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(WineryID, (int)Common.Common.SettingGroup.member);
                covidSurveyEnabled = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_enable_covid_survey);
                covidWaiverEnabled = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_enable_covid_waiver);
            }

            var userDetailModel = new UserDetailModel();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryID));
            parameterList.Add(GetParameter("@UserName", email));
            parameterList.Add(GetParameter("@UserID", UserID));

            using (DbDataReader dataReader = GetDataReader("GetUserDetailsByUserName", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        userDetailModel.user_id = Convert.ToInt32(dataReader["Id"]);
                        userDetailModel.email = Convert.ToString(dataReader["UserName"]);
                        userDetailModel.first_name = Convert.ToString(dataReader["FirstName"]);
                        userDetailModel.last_name = Convert.ToString(dataReader["LastName"]);
                        userDetailModel.phone_number = Convert.ToString(dataReader["HomePhoneStr"]);
                        userDetailModel.mobile_number = Convert.ToString(dataReader["CellPhoneStr"]);
                        userDetailModel.mobile_number_status = Convert.ToInt32(dataReader["MobilePhoneStatus"]);
                        userDetailModel.membership_number = Convert.ToString(dataReader["membershipNumber"]);
                        userDetailModel.color = Convert.ToString(dataReader["colorcode"]);
                        userDetailModel.customer_type = Convert.ToInt32(dataReader["AccountType"]);

                        if (dataReader["MarketingOptinDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["MarketingOptinDate"].ToString()))
                            userDetailModel.marketing_opt_in_date = Convert.ToDateTime(dataReader["MarketingOptinDate"]);

                        if (!string.IsNullOrWhiteSpace(dataReader["emailMarketingStatus"].ToString()))
                            userDetailModel.email_marketing_status = Convert.ToString(dataReader["emailMarketingStatus"]);

                        List<int> listrole = new List<int>();
                        if (!string.IsNullOrWhiteSpace(Convert.ToString(dataReader["UserRole"])))
                        {
                            string[] roles = Convert.ToString(dataReader["UserRole"]).Split(",".ToCharArray());
                            foreach (var role in roles)
                            {
                                listrole.Add(Convert.ToInt32(role));
                            }
                        }

                        userDetailModel.roles = listrole;
                        userDetailModel.visits_count = Convert.ToInt32(dataReader["Visitscount"]);
                        userDetailModel.cancellations_count = Convert.ToInt32(dataReader["Cancellationscount"]);
                        userDetailModel.no_shows_count = Convert.ToInt32(dataReader["Noshowscount"]);
                        userDetailModel.completed_count = Convert.ToInt32(dataReader["completedcount"]);

                        UserAddress addr = new UserAddress();

                        addr.address_1 = Convert.ToString(dataReader["Address1"]);
                        addr.address_2 = Convert.ToString(dataReader["Address2"]);
                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);

                        addr.country = Convert.ToString(dataReader["country"]) + "";

                        if (addr.country.ToLower() == "us")
                        {
                            addr.zip_code = Convert.ToString(dataReader["Zip"]) + "";
                            if (addr.zip_code.Length > 5)
                                addr.zip_code = addr.zip_code.Substring(0, 5);
                        }
                        else
                            addr.zip_code = Convert.ToString(dataReader["Zip"]);

                        userDetailModel.address = addr;
                        //userDetailModel.company_name = Convert.ToString(dataReader["CompanyName"]);
                        //userDetailModel.work_phone = Convert.ToString(dataReader["WorkPhoneStr"]);
                        //userDetailModel.customer_type = Convert.ToInt32(dataReader["AccountType"]);
                        //userDetailModel.affiliate_type = Convert.ToInt32(dataReader["AffiliateType"]);

                        userDetailModel.account_note = GetAccountNote(WineryID, userDetailModel.user_id);

                        userDetailModel.gateway_cust_id = Convert.ToString(dataReader["GatewayCustId"]);
                        userDetailModel.password_change_key = Convert.ToString(dataReader["PasswordChangeKey"]);
                        userDetailModel.date_created = Convert.ToDateTime(dataReader["DateCreated"]);

                        if (covidSurveyEnabled || covidWaiverEnabled)
                        {
                            if (!string.IsNullOrEmpty(userDetailModel.email))
                            {
                                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                                SurveyWaiverStatus surveyWaiverStatus = eventDAL.GetSurveyWaiverStatusByEmailAndMemberId(WineryID, userDetailModel.email);

                                userDetailModel.survey_expire_date = surveyWaiverStatus.survey_expire_date;
                                userDetailModel.survey_status = surveyWaiverStatus.survey_status;
                                userDetailModel.waiver_status = surveyWaiverStatus.waiver_status;
                                userDetailModel.survey_modified_date = surveyWaiverStatus.modified_date;
                            }
                        }
                    }
                }
            }
            return userDetailModel;
        }

        public string GetUserByUserNameOrId(string email, int WineryID = 0, int UserID = 0)
        {
           string email_address=string.Empty;

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryID));
            parameterList.Add(GetParameter("@UserName", email));
            parameterList.Add(GetParameter("@UserID", UserID));

            using (DbDataReader dataReader = GetDataReader("GetUserByUserNameOrId", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        email_address = Convert.ToString(dataReader["UserName"]);
                    }
                }
            }
            return email_address;
        }

        public User2Model GetUserByUserName(string email, int WineryID)
        {
            var userDetailModel = new User2Model();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryID));
            parameterList.Add(GetParameter("@UserName", email));

            using (DbDataReader dataReader = GetDataReader("GetUserByUserName", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        userDetailModel.user_id = Convert.ToInt32(dataReader["Id"]);
                        userDetailModel.email = email;
                        userDetailModel.first_name = Convert.ToString(dataReader["FirstName"]);
                        userDetailModel.last_name = Convert.ToString(dataReader["LastName"]);
                        userDetailModel.company = Convert.ToString(dataReader["CompanyName"]);
                        userDetailModel.is_conceirge = Convert.ToBoolean(dataReader["Isconceirge"]);

                        List<int> listrole = new List<int>();
                        if (!string.IsNullOrWhiteSpace(Convert.ToString(dataReader["UserRole"])))
                        {
                            string[] roles = Convert.ToString(dataReader["UserRole"]).Split(",".ToCharArray());
                            foreach (var role in roles)
                            {
                                listrole.Add(Convert.ToInt32(role));
                            }
                        }

                        userDetailModel.roles = listrole;
                    }
                }
            }
            return userDetailModel;
        }

        public UserDetail GetUserDetailsbyId(int userId, int WineryID = 0)
        {
            var userDetailModel = new UserDetail();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryID));
            parameterList.Add(GetParameter("@UserId", userId));

            using (DbDataReader dataReader = GetDataReader("GetUserDetailsByUserId", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        userDetailModel.user_id = Convert.ToInt32(dataReader["Id"]);
                        userDetailModel.email = Convert.ToString(dataReader["UserName"]);
                        userDetailModel.first_name = Convert.ToString(dataReader["FirstName"]);
                        userDetailModel.last_name = Convert.ToString(dataReader["LastName"]);
                        userDetailModel.phone_number = Convert.ToString(dataReader["HomePhoneStr"]);
                        userDetailModel.customer_type = Convert.ToInt32(dataReader["AccountType"]);
                        userDetailModel.mobile_number = Convert.ToString(dataReader["CellPhoneStr"]);
                        userDetailModel.mobile_number_status = Convert.ToInt32(dataReader["MobilePhoneStatus"]);

                        if (dataReader["MarketingOptinDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["MarketingOptinDate"].ToString()))
                            userDetailModel.marketing_opt_in_date = Convert.ToDateTime(dataReader["MarketingOptinDate"]);

                        if (!string.IsNullOrWhiteSpace(dataReader["emailMarketingStatus"].ToString()))
                            userDetailModel.email_marketing_status = Convert.ToString(dataReader["emailMarketingStatus"]);

                        ReservationUserAddress addr = new ReservationUserAddress();

                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);

                        addr.country = Convert.ToString(dataReader["country"]) + "";

                        if (addr.country.ToLower() == "us")
                        {
                            addr.zip_code = Convert.ToString(dataReader["Zip"]) + "";
                            if (addr.zip_code.Length > 5)
                                addr.zip_code = addr.zip_code.Substring(0, 5);
                        }
                        else
                            addr.zip_code = Convert.ToString(dataReader["Zip"]);

                        userDetailModel.address = addr;
                        //userDetailModel.company_name = Convert.ToString(dataReader["CompanyName"]);
                        //userDetailModel.work_phone = Convert.ToString(dataReader["WorkPhoneStr"]);
                        //userDetailModel.customer_type = Convert.ToInt32(dataReader["AccountType"]);
                        //userDetailModel.affiliate_type = Convert.ToInt32(dataReader["AffiliateType"]);

                        userDetailModel.account_note = GetAccountNote(WineryID, userDetailModel.user_id);


                    }
                }
            }
            return userDetailModel;
        }

        public List<UserDetailModel> GetUsersbykeyword(string keyword, int member_Id = 0, int searchType = 1, string firstName = "", string lastName = "", string email = "", string mobile = "")
        {
            List<UserDetailModel> listUsers = new List<UserDetailModel>();


            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", member_Id));
            parameterList.Add(GetParameter("@SearchValue", keyword));
            parameterList.Add(GetParameter("@searchType", searchType));
            parameterList.Add(GetParameter("@firstName", firstName));
            parameterList.Add(GetParameter("@lastName", lastName));
            parameterList.Add(GetParameter("@email", email));
            parameterList.Add(GetParameter("@mobile", mobile));

            using (DbDataReader dataReader = GetDataReader("GetUserListBykeyword", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        UserDetailModel userDetailModel = new UserDetailModel();
                        userDetailModel.user_id = Convert.ToInt32(dataReader["Id"]);
                        userDetailModel.email = Convert.ToString(dataReader["UserName"]);
                        userDetailModel.first_name = Convert.ToString(dataReader["FirstName"]);
                        userDetailModel.last_name = Convert.ToString(dataReader["LastName"]);
                        userDetailModel.phone_number = Convert.ToString(dataReader["HomePhoneStr"]);
                        userDetailModel.mobile_number = Convert.ToString(dataReader["CellPhoneStr"]);
                        userDetailModel.mobile_number_status = Convert.ToInt32(dataReader["MobilePhoneStatus"]);
                        userDetailModel.membership_number = Convert.ToString(dataReader["membershipNumber"]);
                        userDetailModel.color = Convert.ToString(dataReader["colorcode"]);
                        userDetailModel.customer_type = Convert.ToInt32(dataReader["AccountType"]);
                        userDetailModel.company_name = Convert.ToString(dataReader["CompanyName"]);

                        List<int> listrole = new List<int>();
                        if (!string.IsNullOrWhiteSpace(Convert.ToString(dataReader["UserRole"])))
                        {
                            string[] roles = Convert.ToString(dataReader["UserRole"]).Split(",".ToCharArray());
                            foreach (var role in roles)
                            {
                                listrole.Add(Convert.ToInt32(role));
                            }
                        }

                        if (dataReader["BirthDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["BirthDate"].ToString()))
                        {
                            userDetailModel.birth_date = Convert.ToDateTime(dataReader["BirthDate"]);
                        }

                        userDetailModel.roles = listrole;
                        userDetailModel.visits_count = Convert.ToInt32(dataReader["Visitscount"]);
                        userDetailModel.cancellations_count = Convert.ToInt32(dataReader["Cancellationscount"]);
                        userDetailModel.no_shows_count = Convert.ToInt32(dataReader["Noshowscount"]);
                        userDetailModel.completed_count = Convert.ToInt32(dataReader["completedcount"]);
                        userDetailModel.region_most_visited = Convert.ToInt32(dataReader["RegionMostVisited"]);
                        userDetailModel.is_restricted = false;
                        UserAddress addr = new UserAddress();

                        addr.address_1 = Convert.ToString(dataReader["Address1"]);
                        addr.address_2 = Convert.ToString(dataReader["Address2"]);
                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);

                        addr.country = Convert.ToString(dataReader["country"]) + "";

                        if (addr.country.ToLower() == "us")
                        {
                            addr.zip_code = Convert.ToString(dataReader["Zip"]) + "";
                            if (addr.zip_code.Length > 5)
                                addr.zip_code = addr.zip_code.Substring(0, 5);
                        }
                        else
                            addr.zip_code = Convert.ToString(dataReader["Zip"]);

                        userDetailModel.address = addr;
                        //userDetailModel.company_name = Convert.ToString(dataReader["CompanyName"]);
                        //userDetailModel.work_phone = Convert.ToString(dataReader["WorkPhoneStr"]);
                        //userDetailModel.customer_type = Convert.ToInt32(dataReader["AccountType"]);
                        //userDetailModel.affiliate_type = Convert.ToInt32(dataReader["AffiliateType"]);

                        userDetailModel.account_note = GetAccountNote(member_Id, userDetailModel.user_id);
                        listUsers.Add(userDetailModel);
                    }
                }
            }
            return listUsers;
        }

        public GuestPerformance GetGuestPerformanceData(string keyword, int member_Id)
        {
            GuestPerformance userDetailModel = new GuestPerformance();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", member_Id));
            parameterList.Add(GetParameter("@SearchValue", keyword));

            using (DbDataReader dataReader = GetDataReader("GetGuestPerformanceData", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        userDetailModel.user_id = Convert.ToInt32(dataReader["id"]);
                        userDetailModel.color = Convert.ToString(dataReader["colorcode"]);

                        List<int> listrole = new List<int>();
                        if (!string.IsNullOrWhiteSpace(Convert.ToString(dataReader["UserRole"])))
                        {
                            string[] roles = Convert.ToString(dataReader["UserRole"]).Split(",".ToCharArray());
                            foreach (var role in roles)
                            {
                                listrole.Add(Convert.ToInt32(role));
                            }
                        }

                        userDetailModel.roles = listrole;
                        userDetailModel.visits_count = Convert.ToInt32(dataReader["Visitscount"]);
                        userDetailModel.cancellations_count = Convert.ToInt32(dataReader["Cancellationscount"]);
                        userDetailModel.no_shows_count = Convert.ToInt32(dataReader["Noshowscount"]);
                        userDetailModel.completed_count = Convert.ToInt32(dataReader["completedcount"]);
                        userDetailModel.mobile_number = Convert.ToString(dataReader["CellPhoneStr"]);
                        userDetailModel.account_note = GetAccountNote(member_Id, userDetailModel.user_id);
                        userDetailModel.mobile_number_status = Convert.ToInt32(dataReader["MobilePhoneStatus"]);
                        userDetailModel.region_most_visited = Convert.ToInt32(dataReader["RegionMostVisited"]);
                        userDetailModel.zip_code = Convert.ToString(dataReader["zip"]);
                        userDetailModel.address_1 = Convert.ToString(dataReader["address1"]);
                        userDetailModel.city = Convert.ToString(dataReader["city"]);
                        userDetailModel.state = Convert.ToString(dataReader["state"]);
                        userDetailModel.address_2 = Convert.ToString(dataReader["address2"]);
                        userDetailModel.phone_number = Convert.ToString(dataReader["HomePhoneStr"]);
                        userDetailModel.sms_opt_out = Convert.ToBoolean(dataReader["OptOutFromSMS"]);
                        userDetailModel.last_reservation_id = Convert.ToInt32(dataReader["LastReservationId"]);
                        userDetailModel.company_name = Convert.ToString(dataReader["CompanyName"]);
                        userDetailModel.gender = Convert.ToString(dataReader["Gender"]);

                        userDetailModel.title = Convert.ToString(dataReader["Title"]);
                        userDetailModel.work_number = Convert.ToString(dataReader["WorkPhoneStr"]);

                        if (dataReader["LastCheckInDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["LastCheckInDate"].ToString()))
                        {
                            userDetailModel.last_check_in_date = Convert.ToDateTime(dataReader["LastCheckInDate"]);
                        }

                        if (dataReader["BirthDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["BirthDate"].ToString()))
                        {
                            userDetailModel.birth_date = Convert.ToDateTime(dataReader["BirthDate"]);
                        }
                        userDetailModel.user_tags = Convert.ToString(dataReader["UserTags"]);
                        userDetailModel.user_image = Convert.ToString(dataReader["UserImage"]);

                        if (string.IsNullOrEmpty(userDetailModel.city) || string.IsNullOrEmpty(userDetailModel.state))
                        {
                            var userAddr = GetUserAddressByZipCode(userDetailModel.zip_code);
                            userDetailModel.state = GetStateBystatecode(userAddr.state);
                            userDetailModel.city = userAddr.city;
                        }
                    }
                }
            }
            return userDetailModel;
        }

        public List<UserDetailModel> GetClubMemberListBykeyword(string keyword, int member_Id = 0, int src = -1)
        {
            List<UserDetailModel> listUsers = new List<UserDetailModel>();


            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", member_Id));
            parameterList.Add(GetParameter("@SearchValue", keyword));
            parameterList.Add(GetParameter("@src", src));

            using (DbDataReader dataReader = GetDataReader("GetClubMemberListBykeyword", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        UserDetailModel userDetailModel = new UserDetailModel();
                        userDetailModel.user_id = Convert.ToInt32(dataReader["Id"]);
                        userDetailModel.email = Convert.ToString(dataReader["Email"]);
                        userDetailModel.first_name = Convert.ToString(dataReader["FirstName"]);
                        userDetailModel.last_name = Convert.ToString(dataReader["LastName"]);
                        userDetailModel.phone_number = Convert.ToString(dataReader["HomePhone"]);
                        userDetailModel.mobile_number = Convert.ToString(dataReader["mobilephone"]);
                        userDetailModel.is_restricted = false;

                        List<string> listcontacttypes = new List<string>();
                        if (Convert.ToString(dataReader["type"]).Length > 0)
                        {
                            listcontacttypes.Add(Convert.ToString(dataReader["type"]));
                        }

                        if (Convert.ToString(dataReader["tier"]).Length > 0 && Convert.ToString(dataReader["type"]) != Convert.ToString(dataReader["tier"]))
                        {
                            listcontacttypes.Add(Convert.ToString(dataReader["tier"]));
                        }

                        if (listcontacttypes.Count > 0)
                            userDetailModel.member_status = true;

                        userDetailModel.contact_types = listcontacttypes;
                        userDetailModel.mobile_number_status = 0;
                        userDetailModel.membership_number = "";
                        userDetailModel.color = "";
                        userDetailModel.customer_type = 1;

                        // userDetailModel.roles = listrole;
                        userDetailModel.visits_count = 0;
                        userDetailModel.cancellations_count = 0;
                        userDetailModel.no_shows_count = 0;
                        userDetailModel.completed_count = 0;

                        UserAddress addr = new UserAddress();

                        addr.address_1 = Convert.ToString(dataReader["Address1"]);
                        addr.address_2 = Convert.ToString(dataReader["Address2"]);
                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);
                        addr.country = Convert.ToString(dataReader["country"]) + "";

                        if (addr.country.ToLower() == "us")
                        {
                            addr.zip_code = Convert.ToString(dataReader["Zip"]) + "";
                            if (addr.zip_code.Length > 5)
                                addr.zip_code = addr.zip_code.Substring(0, 5);
                        }
                        else
                            addr.zip_code = Convert.ToString(dataReader["Zip"]);

                        userDetailModel.address = addr;
                        //userDetailModel.customer_type =0;

                        AccountNote notemodel = new AccountNote();
                        notemodel.modified_by = "";
                        notemodel.note = "";
                        userDetailModel.account_note = notemodel;

                        if (member_Id > 0)
                        {
                            var guestPerformanceModel = new GuestPerformance();
                            guestPerformanceModel = GetGuestPerformanceData(keyword, member_Id);

                            if (guestPerformanceModel != null && guestPerformanceModel.user_id > 0)
                            {
                                userDetailModel.completed_count = guestPerformanceModel.completed_count;
                                userDetailModel.visits_count = guestPerformanceModel.visits_count;
                                userDetailModel.cancellations_count = guestPerformanceModel.cancellations_count;
                                userDetailModel.no_shows_count = guestPerformanceModel.no_shows_count;
                                userDetailModel.color = guestPerformanceModel.color;
                                userDetailModel.roles = guestPerformanceModel.roles;
                                userDetailModel.user_id = guestPerformanceModel.user_id;
                                userDetailModel.mobile_number = guestPerformanceModel.mobile_number;
                                userDetailModel.account_note = GetAccountNote(member_Id, guestPerformanceModel.user_id);
                                userDetailModel.region_most_visited = guestPerformanceModel.region_most_visited;

                                if (userDetailModel.address != null && (userDetailModel.address.zip_code + "").Length == 0)
                                    userDetailModel.address.zip_code = guestPerformanceModel.zip_code;

                                if (userDetailModel.address != null && (userDetailModel.address.address_1 + "").Length == 0)
                                    userDetailModel.address.address_1 = guestPerformanceModel.address_1;

                                if (userDetailModel.address != null && (userDetailModel.address.address_2 + "").Length == 0)
                                    userDetailModel.address.address_2 = guestPerformanceModel.address_2;

                                if (userDetailModel.address != null && (userDetailModel.address.city + "").Length == 0)
                                    userDetailModel.address.city = guestPerformanceModel.city;

                                if (userDetailModel.address != null && (userDetailModel.address.state + "").Length == 0)
                                    userDetailModel.address.state = guestPerformanceModel.state;

                            }
                        }

                        listUsers.Add(userDetailModel);
                    }
                }
            }
            return listUsers;
        }

        public List<UserDetailModel> GetUsersByRoleID(int wineryID, List<int> roleID)
        {
            List<UserDetailModel> model = new List<UserDetailModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", wineryID));
            parameterList.Add(GetParameter("@RoleID", string.Join(",", roleID)));

            using (DbDataReader dataReader = GetDataReader("GetUsersByRoleID", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        UserAddress addr = new UserAddress();

                        addr.address_1 = Convert.ToString(dataReader["Address1"]);
                        addr.address_2 = Convert.ToString(dataReader["Address2"]);
                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);
                        addr.country = Convert.ToString(dataReader["Country"]) + "";

                        if (addr.country.ToLower() == "us")
                        {
                            addr.zip_code = Convert.ToString(dataReader["Zip"]) + "";
                            if (addr.zip_code.Length > 5)
                                addr.zip_code = addr.zip_code.Substring(0, 5);
                        }
                        else
                            addr.zip_code = Convert.ToString(dataReader["Zip"]);

                        List<int> listrole = new List<int>();
                        if (!string.IsNullOrWhiteSpace(Convert.ToString(dataReader["UserRole"])))
                        {
                            string[] roles = Convert.ToString(dataReader["UserRole"]).Split(",".ToCharArray());
                            foreach (var role in roles)
                            {
                                listrole.Add(Convert.ToInt32(role));
                            }
                        }

                        model.Add(new UserDetailModel
                        {
                            user_id = Convert.ToInt32(dataReader["ID"]),
                            email = Convert.ToString(dataReader["UserName"]),
                            first_name = Convert.ToString(dataReader["FirstName"]),
                            last_name = Convert.ToString(dataReader["LastName"]),
                            phone_number = Convert.ToString(dataReader["HomePhoneStr"]),
                            mobile_number = Convert.ToString(dataReader["CellPhoneStr"]),
                            mobile_number_status = Convert.ToInt32(dataReader["MobilePhoneStatus"]),
                            membership_number = Convert.ToString(dataReader["membershipNumber"]),
                            roles = listrole,
                            no_shows_count = Convert.ToInt32(dataReader["Noshowscount"]),
                            completed_count = Convert.ToInt32(dataReader["completedcount"]),
                            visits_count = Convert.ToInt32(dataReader["Visitscount"]),
                            cancellations_count = Convert.ToInt32(dataReader["Cancellationscount"]),
                            color = Convert.ToString(dataReader["colorcode"]),
                            address = addr,
                            customer_type = Convert.ToInt32(dataReader["AccountType"]),
                            account_note = GetAccountNote(wineryID, Convert.ToInt32(dataReader["ID"])),
                            region_most_visited = Convert.ToInt32(dataReader["RegionMostVisited"]),
                        });
                    }
                }
            }
            return model;
        }

        public List<UserDetail2Model> GetUsersbyAffiliateId(int userID)
        {
            List<UserDetail2Model> model = new List<UserDetail2Model>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@userID", userID));

            using (DbDataReader dataReader = GetDataReader("GetUsersbyAffiliateId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        UserAddress addr = new UserAddress();

                        addr.address_1 = Convert.ToString(dataReader["Address1"]);
                        addr.address_2 = Convert.ToString(dataReader["Address2"]);
                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);
                        addr.country = Convert.ToString(dataReader["Country"]) + "";

                        if (addr.country.ToLower() == "us")
                        {
                            addr.zip_code = Convert.ToString(dataReader["Zip"]) + "";
                            if (addr.zip_code.Length > 5)
                                addr.zip_code = addr.zip_code.Substring(0, 5);
                        }
                        else
                            addr.zip_code = Convert.ToString(dataReader["Zip"]);

                        model.Add(new UserDetail2Model
                        {
                            user_id = Convert.ToInt32(dataReader["ID"]),
                            email = Convert.ToString(dataReader["UserName"]),
                            first_name = Convert.ToString(dataReader["FirstName"]),
                            last_name = Convert.ToString(dataReader["LastName"]),
                            phone_number = Convert.ToString(dataReader["HomePhoneStr"]),
                            mobile_number = Convert.ToString(dataReader["CellPhoneStr"]),
                            mobile_number_status = Convert.ToInt32(dataReader["MobilePhoneStatus"]),
                            color = Convert.ToString(dataReader["colorcode"]),
                            address = addr,
                            region_most_visited = Convert.ToInt32(dataReader["RegionMostVisited"]),
                        });
                    }
                }
            }
            return model;
        }

        public List<ConciergeUserDetailModel> GetUsersbyConciergeId(int ConciergeId)
        {
            List<ConciergeUserDetailModel> model = new List<ConciergeUserDetailModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@userID", ConciergeId));

            using (DbDataReader dataReader = GetDataReader("GetUsersbyAffiliateId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.Add(new ConciergeUserDetailModel
                        {
                            user_id = Convert.ToInt32(dataReader["ID"]),
                            email = Convert.ToString(dataReader["UserName"]),
                            first_name = Convert.ToString(dataReader["FirstName"]),
                            last_name = Convert.ToString(dataReader["LastName"]),
                            home_phone = Convert.ToString(dataReader["HomePhoneStr"]),
                            mobile_number = Convert.ToString(dataReader["CellPhoneStr"]),
                            work_phone = Convert.ToString(dataReader["WorkPhoneStr"]),
                            company_name = Convert.ToString(dataReader["CompanyName"]),
                            title = Convert.ToString(dataReader["Title"]),
                            is_favorite = Convert.ToBoolean(dataReader["IsFavorite"]),
                        });
                    }
                }
            }
            return model;
        }

        public void UpdateConciergeUser(int UserId,int ConciergeId,bool IsFavorite)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@IsFavorite", IsFavorite));
            parameterList.Add(GetParameter("@ConciergeId", ConciergeId));

            ExecuteNonQuery("UpdateUserAffiliate", parameterList, CommandType.StoredProcedure);
        }

        public UserDetail2Model GetUsersbyId(int userID)
        {
            UserDetail2Model model = new UserDetail2Model();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@userID", userID));

            using (DbDataReader dataReader = GetDataReader("GetUsersbyId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        UserAddress addr = new UserAddress();

                        addr.address_1 = Convert.ToString(dataReader["Address1"]);
                        addr.address_2 = Convert.ToString(dataReader["Address2"]);
                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);
                        addr.country = Convert.ToString(dataReader["Country"]) + "";

                        if (addr.country.ToLower() == "us")
                        {
                            addr.zip_code = Convert.ToString(dataReader["Zip"]) + "";
                            if (addr.zip_code.Length > 5)
                                addr.zip_code = addr.zip_code.Substring(0, 5);
                        }
                        else
                            addr.zip_code = Convert.ToString(dataReader["Zip"]);

                        model.user_id = Convert.ToInt32(dataReader["ID"]);
                        model.email = Convert.ToString(dataReader["UserName"]);
                        model.first_name = Convert.ToString(dataReader["FirstName"]);
                        model.last_name = Convert.ToString(dataReader["LastName"]);
                        model.phone_number = Convert.ToString(dataReader["HomePhoneStr"]);
                        model.mobile_number = Convert.ToString(dataReader["CellPhoneStr"]);
                        model.mobile_number_status = Convert.ToInt32(dataReader["MobilePhoneStatus"]);
                        model.color = Convert.ToString(dataReader["colorcode"]);
                        model.address = addr;
                        model.region_most_visited = Convert.ToInt32(dataReader["RegionMostVisited"]);

                        model.company_name = Convert.ToString(dataReader["CompanyName"]);
                        model.birth_date = Convert.ToDateTime(dataReader["BirthDate"]);
                        model.work_phone_str = Convert.ToString(dataReader["WorkPhoneStr"]);
                        model.concierge_type = Convert.ToInt32(dataReader["AffiliateType"]);
                        model.title = Convert.ToString(dataReader["Title"]);
                        model.website = Convert.ToString(dataReader["Website"]);
                        model.gender = Convert.ToString(dataReader["Gender"]);
                        model.age = Convert.ToInt32(dataReader["Age"]);
                        model.role_id = Convert.ToInt32(dataReader["RoleId"]);
                        model.is_concierge = Convert.ToBoolean(dataReader["IsConcierge"]);
                        model.weekly_newsletter = Convert.ToBoolean(dataReader["WeeklyNewsletterSubscribe"]);
                    }
                }
            }
            return model;
        }

        public List<GuestDetailModel> GetGuestsByWineryAndRole(int wineryID, string searchValue)
        {
            List<GuestDetailModel> model = new List<GuestDetailModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", wineryID));
            parameterList.Add(GetParameter("@RoleIDs", '4'));
            parameterList.Add(GetParameter("@Searchvalue", searchValue));

            using (DbDataReader dataReader = GetDataReader("GetGuestsByWineryAndRole", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.Add(new GuestDetailModel
                        {
                            user_id = Convert.ToInt32(dataReader["Id"]),
                            email = Convert.ToString(dataReader["UserName"]),
                            first_name = Convert.ToString(dataReader["FirstName"]),
                            last_name = Convert.ToString(dataReader["LastName"]),
                            phone_number = Convert.ToString(dataReader["HomePhoneStr"]),
                            membership_number = Convert.ToString(dataReader["CustomerNumber"]),
                            color = Convert.ToString(dataReader["ColorCode"]),
                            company_name = Convert.ToString(dataReader["CompanyName"]),
                            title = Convert.ToString(dataReader["Title"]),
                            work_phone = Convert.ToString(dataReader["WorkPhoneStr"]),
                            affiliate_type = Convert.ToString(dataReader["AffiliateType"]),
                            login_date = Convert.ToDateTime(dataReader["LoginDate"]),
                        });
                    }
                }
            }
            return model;
        }

        public int CheckUserWinery(int WineryId, int UserId)
        {
            int Id = 0;

            string sql = "select id from UserWinery where UserId=@UserId and WineryId=@WineryId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@WineryId", WineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Id = Convert.ToInt32(dataReader["id"]);
                    }
                }
            }
            return Id;
        }

        public MobileNumberStatus GetMobilePhoneStatusByMobilePhoneNumber(string CellPhone, ref int UserId)
        {
            MobileNumberStatus mobilePhoneStatus = MobileNumberStatus.unverified;
            UserId = 0;

            string sql = "select top 1 MobilePhoneStatus,Id from [user] (nolock) where Replace(Replace(Replace(Replace(Replace(CellPhoneStr,' ',''),'(',''),')',''),'+',''),'-','') like @CellPhone";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@CellPhone", '%' + CellPhone));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        mobilePhoneStatus = (MobileNumberStatus)Convert.ToInt32(dataReader["MobilePhoneStatus"]);
                        UserId = Convert.ToInt32(dataReader["Id"]);
                    }
                }
            }
            return mobilePhoneStatus;
        }

        public List<AccountTypeDiscountModel> LoadAccountTypeDiscounts(int eventId)
        {
            List<AccountTypeDiscountModel> list = new List<AccountTypeDiscountModel>();

            string sql = "select at.Id,at.ContactTypeId,at.ContactType,a.MemberBenefit,a.MemberBenefitReq,a.MemberBenefit,MemberBenefitCustomValue from Event_AccountTypes (nolock) a join ThirdParty_AccountTypes (nolock) at on at.Id = a.ThirdParty_AccountTypes_Id Where a.Event_Id = @eventId And at.IsAvailable = 1";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@eventId", eventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        AccountTypeDiscountModel accountTypeDiscountModel = new AccountTypeDiscountModel();
                        accountTypeDiscountModel.Id = Convert.ToInt32(dataReader["Id"]);
                        accountTypeDiscountModel.ContactTypeId = Convert.ToString(dataReader["ContactTypeId"]);
                        accountTypeDiscountModel.ContactType = Convert.ToString(dataReader["ContactType"]);
                        accountTypeDiscountModel.MemberBenefit = (Common.Common.DiscountType)Convert.ToInt32(dataReader["MemberBenefit"]);
                        accountTypeDiscountModel.MemberBenefitReq = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                        accountTypeDiscountModel.MemberBenefitId = Convert.ToInt32(dataReader["MemberBenefit"]);
                        accountTypeDiscountModel.MemberBenefitCustomValue = Convert.ToInt32(dataReader["MemberBenefitCustomValue"]);
                        list.Add(accountTypeDiscountModel);
                    }
                }
            }
            return list;
        }

        public List<AccountTypeDiscountModel> LoadAddOnsAccountTypeDiscounts(int AddOn_Group_Id, int AddOn_Item_Id)
        {
            List<AccountTypeDiscountModel> list = new List<AccountTypeDiscountModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@AddOnGroupId", AddOn_Group_Id));
            parameterList.Add(GetParameter("@AddOnItemsId", AddOn_Item_Id));

            using (DbDataReader dataReader = GetDataReader("GetEventAddOnsAccountTypes", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        AccountTypeDiscountModel accountTypeDiscountModel = new AccountTypeDiscountModel();
                        accountTypeDiscountModel.Id = Convert.ToInt32(dataReader["Id"]);
                        accountTypeDiscountModel.ContactTypeId = Convert.ToString(dataReader["ContactTypeId"]);
                        accountTypeDiscountModel.ContactType = Convert.ToString(dataReader["ContactType"]);
                        accountTypeDiscountModel.MemberBenefit = (Common.Common.DiscountType)Convert.ToInt32(dataReader["MemberBenefit"]);
                        accountTypeDiscountModel.MemberBenefitReq = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                        accountTypeDiscountModel.MemberBenefitId = Convert.ToInt32(dataReader["MemberBenefit"]);
                        accountTypeDiscountModel.MemberBenefitCustomValue = Convert.ToInt32(dataReader["MemberBenefitCustomValue"]);
                        list.Add(accountTypeDiscountModel);
                    }
                }
            }
            return list;
        }

        //public List<AccountTypeDiscountModel> LoadAddOnsAccountTypeDiscounts(int AddOn_Group_Id, int AddOn_Item_Id)
        //{
        //    List<AccountTypeDiscountModel> list = new List<AccountTypeDiscountModel>();

        //    var parameterList = new List<DbParameter>();
        //    parameterList.Add(GetParameter("@AddOnGroupId", AddOn_Group_Id));
        //    parameterList.Add(GetParameter("@AddOnGroupItemsId", AddOn_Item_Id));

        //    using (DbDataReader dataReader = GetDataReader("GetEventAddOnsAccountTypesId", parameterList, CommandType.StoredProcedure))
        //    {
        //        if (dataReader != null && dataReader.HasRows)
        //        {
        //            while (dataReader.Read())
        //            {
        //                AccountTypeDiscountModel accountTypeDiscountModel = new AccountTypeDiscountModel();
        //                accountTypeDiscountModel.Id = Convert.ToInt32(dataReader["Id"]);
        //                accountTypeDiscountModel.ContactTypeId = Convert.ToString(dataReader["ContactTypeId"]);
        //                accountTypeDiscountModel.ContactType = Convert.ToString(dataReader["ContactType"]);
        //                accountTypeDiscountModel.MemberBenefit = (Common.Common.DiscountType)Convert.ToInt32(dataReader["MemberBenefit"]);
        //                accountTypeDiscountModel.MemberBenefitReq = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
        //                accountTypeDiscountModel.MemberBenefitId = Convert.ToInt32(dataReader["MemberBenefit"]);
        //                accountTypeDiscountModel.MemberBenefitCustomValue = Convert.ToInt32(dataReader["MemberBenefitCustomValue"]);
        //                list.Add(accountTypeDiscountModel);
        //            }
        //        }
        //    }
        //    return list;
        //}

        public string GetAddOnsGroupNameById(int AddOn_Group_Id)
        {
            string groupName = "";

            string sql = "select [Name] from AddOn_Group where id=@AddOn_Group_Id";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@AddOn_Group_Id", AddOn_Group_Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        groupName = Convert.ToString(dataReader["Name"]);
                    }
                }
            }
            return groupName;
        }

        public List<AccountTypeDiscountModel> LoadAccountTypes(int wineryId, Common.Common.ThirdPartyType thirdPartyType)
        {
            List<AccountTypeDiscountModel> list = new List<AccountTypeDiscountModel>();

            string sql = "select Id,ContactTypeId,ContactType from ThirdParty_AccountTypes where wineryid=@wineryId and ThirdPartyId=@thirdPartyType and ActiveClub = 1";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@wineryId", wineryId));
            parameterList.Add(GetParameter("@thirdPartyType", (int)thirdPartyType));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        AccountTypeDiscountModel accountTypeDiscountModel = new AccountTypeDiscountModel();
                        accountTypeDiscountModel.Id = Convert.ToInt32(dataReader["Id"]);
                        accountTypeDiscountModel.ContactTypeId = Convert.ToString(dataReader["ContactTypeId"]);
                        accountTypeDiscountModel.ContactType = Convert.ToString(dataReader["ContactType"]);

                        list.Add(accountTypeDiscountModel);
                    }
                }
            }
            return list;
        }

        public decimal GetEventMemberBenefitCustomValue(int eventId)
        {
            decimal MemberBenefitCustomValue = 0;

            string sql = "select MemberBenefitCustomValue from Events (nolock) Where EventId = @eventId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@eventId", eventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        MemberBenefitCustomValue = Convert.ToInt32(dataReader["MemberBenefitCustomValue"]);
                    }
                }
            }
            return MemberBenefitCustomValue;
        }

        public void UpdateUserWinery(int UserId, int WineryId, int RoleId, string CreatedByUser, string CustomerRefNum, string RmsId, int AccountType)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@RoleId", RoleId));
            parameterList.Add(GetParameter("@CreatedByUser", CreatedByUser));
            parameterList.Add(GetParameter("@CustomerRefNum", CustomerRefNum));
            parameterList.Add(GetParameter("@RmsId", RmsId));
            parameterList.Add(GetParameter("@AccountType", AccountType));
            ExecuteNonQuery("UserWinery_INSERT", parameterList, CommandType.StoredProcedure);
        }

        public void UpdateUserWinery(int UserId, int WineryId, int RoleId)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@RoleId", RoleId));
            
            ExecuteNonQuery("UpdateUserWinery", parameterList, CommandType.StoredProcedure);
        }

        public void UpdateAccount(UpdateAccountRequest model)
        {
            var parameterList = new List<DbParameter>();
    
            parameterList.Add(GetParameter("@Email", model.email));
            parameterList.Add(GetParameter("@FirstName", model.first_name));
            parameterList.Add(GetParameter("@LastName", model.last_name));
            parameterList.Add(GetParameter("@Address1", model.address_1));
            parameterList.Add(GetParameter("@Address2", model.address_2));
            parameterList.Add(GetParameter("@City", model.city));
            parameterList.Add(GetParameter("@State", model.state));
            parameterList.Add(GetParameter("@Country", model.country));
            parameterList.Add(GetParameter("@Zip", model.zip));
            parameterList.Add(GetParameter("@BirthDate", model.birth_date));
            parameterList.Add(GetParameter("@CompanyName", model.company_name));
            parameterList.Add(GetParameter("@HomePhoneStr", model.home_phone_str));
            parameterList.Add(GetParameter("@CellPhoneStr", model.cell_phone_str));
            parameterList.Add(GetParameter("@PreferredAppellation", model.preferred_appellation));

            ExecuteNonQuery("UpdateAccount", parameterList, CommandType.StoredProcedure);
        }

        public int CreateUser(string UserName, string Password, string FirstName, string LastName, string Country, string Zip, string PhoneNum, int AccountType = 0, int SMSVerified = 0, int RegionVisitedMost = 0, string custId = "", string City = "", string State = "", string Address1 = "", string Address2 = "",int Source = 0,int Conciergetype = 0,int RoleId = 4)
        {
            int id = 0;

            int Verified = 0;
            if (SMSVerified == (int)MobileNumberStatus.verified)
            {
                Verified = 1;
            }
            else
            {
                Verified = 0;
            }

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserName", UserName));
            parameterList.Add(GetParameter("@Password", Password));
            parameterList.Add(GetParameter("@FirstName", FirstName));
            parameterList.Add(GetParameter("@LastName", LastName));
            parameterList.Add(GetParameter("@Address1", Address1));
            parameterList.Add(GetParameter("@Address2", Address2));
            parameterList.Add(GetParameter("@City", City));
            parameterList.Add(GetParameter("@State", State));
            parameterList.Add(GetParameter("@Country", Country));
            parameterList.Add(GetParameter("@Zip", Zip));
            parameterList.Add(GetParameter("@PhoneNum", PhoneNum));
            parameterList.Add(GetParameter("@RoleId", RoleId));
            parameterList.Add(GetParameter("@createdByUser", ""));
            parameterList.Add(GetParameter("@CustomerNumber", ""));
            parameterList.Add(GetParameter("@SecondaryEmail", ""));
            parameterList.Add(GetParameter("@HomePhone", PhoneNum));
            parameterList.Add(GetParameter("@BirthDate", "1/1/1900"));
            parameterList.Add(GetParameter("@AccountType", AccountType));
            parameterList.Add(GetParameter("@Active", true));
            parameterList.Add(GetParameter("@SMSVerified", Verified));
            parameterList.Add(GetParameter("@mobilephonestatus", SMSVerified));
            parameterList.Add(GetParameter("@regionMostVisited", RegionVisitedMost));
            parameterList.Add(GetParameter("@GatewayCustId", custId));
            parameterList.Add(GetParameter("@Source", Source));
            parameterList.Add(GetParameter("@Conciergetype", Conciergetype));

            //id = Convert.ToInt32(ExecuteScalar("User_INSERT", parameterList));

            using (DbDataReader dataReader = GetDataReader("User_INSERT", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        id = Convert.ToInt32(dataReader["Id"]);
                    }
                }

            }

            return id;
        }
        public int UpdateUserWinery(int wineryId, int userId, string customerReferenceNumber, string rmsId = "", int accountType = -1)
        {
            List<UserDetailModel> model = new List<UserDetailModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", wineryId));
            parameterList.Add(GetParameter("@UserID", userId));
            parameterList.Add(GetParameter("@CustomerReferenceNumber", customerReferenceNumber));
            parameterList.Add(GetParameter("@RmsId", rmsId));
            parameterList.Add(GetParameter("@AccountType", accountType));
            parameterList.Add(GetParameter("@DateModified", DateTime.UtcNow));

            string sqlQuery = "UPDATE UserWinery  SET CustomerReferenceNumber = @CustomerReferenceNumber, RmsId = (CASE WHEN LEN(@RmsId) > 0 THEN @RmsId ELSE RmsId END), DateModified = @DateModified WHERE WineryID = @WineryID AND UserID = @UserID;";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text);
        }

        public int UpdateUserWineryMarketingStatus(int wineryId, int userId, string emailMarketingStatus)
        {
            List<UserDetailModel> model = new List<UserDetailModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", wineryId));
            parameterList.Add(GetParameter("@UserID", userId));
            parameterList.Add(GetParameter("@emailMarketingStatus", emailMarketingStatus));
            parameterList.Add(GetParameter("@MarketingOptinDate", DateTime.UtcNow));
            parameterList.Add(GetParameter("@DateModified", DateTime.UtcNow));

            string sqlQuery = "UPDATE UserWinery  SET emailMarketingStatus = @emailMarketingStatus, MarketingOptinDate = @MarketingOptinDate, DateModified = @DateModified WHERE WineryID = @WineryID AND UserID = @UserID;";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text);
        }

        public int UpdateMobilePhoneStatusById(string CellPhone, int MobilePhoneStatus)
        {
            List<UserDetailModel> model = new List<UserDetailModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MobilePhoneStatus", MobilePhoneStatus));
            parameterList.Add(GetParameter("@CellPhone", '%' + CellPhone));

            string sqlQuery = "UPDATE [user] SET MobilePhoneStatus = @MobilePhoneStatus WHERE Replace(Replace(Replace(Replace(Replace(CellPhoneStr,' ',''),'(',''),')',''),'+',''),'-','') like @CellPhone";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text);
        }

        public bool UpdateUserSMSVerifiedbyId(int userid, int SMSVerified, string homephonestr)
        {
            var parameterList = new List<DbParameter>();
            int Verified = 0;
            if (SMSVerified == (int)MobileNumberStatus.verified)
            {
                Verified = 1;
            }
            else
            {
                Verified = 0;
            }

            parameterList.Add(GetParameter("@SMSVerified", Verified));
            parameterList.Add(GetParameter("@mobilephonestatus", SMSVerified));
            parameterList.Add(GetParameter("@homephonestr", homephonestr));
            parameterList.Add(GetParameter("@UserID", userid));
            string sqlQuery = "UPDATE [User]  SET SMSVerified = @SMSVerified,mobilephonestatus = @mobilephonestatus, homephonestr = @homephonestr where Id = @UserID";
            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public AccountNote GetAccountNote(int WineryId, int UserId)
        {
            AccountNote notemodel = new AccountNote();

            string sql = "select top 1 currentuser,notedate,Note from InternalNotes (nolock) where notetype=2 and refid=@UserId and wineryid= @WineryId order by notedate desc";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@WineryId", WineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        notemodel.modified_by = Convert.ToString(dataReader["currentuser"]);
                        notemodel.note_date = Convert.ToDateTime(dataReader["notedate"]);
                        notemodel.note = Convert.ToString(dataReader["Note"]);
                    }
                }
            }
            return notemodel;
        }

        public string GetEmailMarketingStatus(int WineryId, string UserName)
        {
            string emailMarketingStatus = string.Empty;

            string sql = "select top 1 isnull(emailMarketingStatus,'') emailMarketingStatus from [UserWinery] uw join [user] u on uw.userId =u.Id where userName=@UserName and WineryId= @WineryId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserName", UserName));
            parameterList.Add(GetParameter("@WineryId", WineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        emailMarketingStatus = Convert.ToString(dataReader["emailMarketingStatus"]);
                    }
                }
            }
            return emailMarketingStatus;
        }

        public string GetEmailMarketingStatus(int WineryId, int UserId)
        {
            string emailMarketingStatus = string.Empty;

            string sql = "select top 1 isnull(emailMarketingStatus,'') emailMarketingStatus from [UserWinery] where UserId=@UserId and WineryId= @WineryId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@WineryId", WineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        emailMarketingStatus = Convert.ToString(dataReader["emailMarketingStatus"]);
                    }
                }
            }
            return emailMarketingStatus;
        }

        public UserDetailModel GetUserById(int UserId)
        {
            var userDetailModel = new UserDetailModel();

            string sql = "select FirstName,LastName,colorcode,username from [user] where id =@UserId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", UserId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        userDetailModel.first_name = Convert.ToString(dataReader["FirstName"]);
                        userDetailModel.email = Convert.ToString(dataReader["username"]);
                        userDetailModel.last_name = Convert.ToString(dataReader["LastName"]);
                        userDetailModel.color = Convert.ToString(dataReader["colorcode"]);
                    }
                }
            }
            return userDetailModel;
        }

        public string GetUserNameById(int UserId)
        {
            var userDetailModel = new UserDetailModel();
            string username = string.Empty;

            string sql = "select UserName from [user] where id =@UserId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", UserId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        username = Convert.ToString(dataReader["UserName"]);
                    }
                }
            }
            return username;
        }

        public string GetUserEmailById(int Id)
        {
            string email = string.Empty;

            string sql = "select username from [user] (nolock) where id=@Id";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        email = Convert.ToString(dataReader["username"]);
                    }
                }
            }
            return email;
        }

        public int GetUserRoleIdById(string email)
        {
            int roleId = 4;

            string sql = "select roleid from [User] where username = @email";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@email", email));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        roleId = Convert.ToInt32(dataReader["roleid"]);
                    }
                }
            }
            return roleId;
        }

        public bool EmailAlreadyExists(string email)
        {
            bool ret = false;

            string sql = "select Id from [User] (nolock) where username = @email";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@email", email));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }

        public string GetStateBystatecode(string statecode)
        {
            string state = string.Empty;

            string sql = "select [state] from States where statecode=@statecode";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@statecode", statecode));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        state = Convert.ToString(dataReader["state"]);
                    }
                }
            }
            return state;
        }

        public int GetUserFavRegionById(int Id)
        {
            int favRegionId = 0;

            string sql = "select PreferredAppellation from [user] where id=@Id";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        favRegionId = Convert.ToInt32(dataReader["PreferredAppellation"]);
                    }
                }
            }
            return favRegionId;
        }

        public bool IsReturningUser(int id, int memberId)
        {
            bool isRetUser = false;

            string sql = "select count(ReservationId) as cnt from ReservationV2 where UserId=@Id and WineryId=@MemberId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", id));
            parameterList.Add(GetParameter("@MemberId", memberId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        isRetUser = Convert.ToInt32(dataReader["cnt"]) > 0;
                    }
                }
            }
            return isRetUser;
        }

        public bool UpdateUserAccountNotes(int userid, int WineryId, string UserNote, string CurrentUser)
        {
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@userid", userid));
            parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@UserNote", UserNote));
            parameterList.Add(GetParameter("@CurrentUser", CurrentUser));
            parameterList.Add(GetParameter("@NoteDate", DateTime.UtcNow));

            AccountNote accountNote = GetAccountNote(WineryId, userid);

            string sqlQuery = string.Empty;
            if (string.IsNullOrEmpty(accountNote.modified_by))
            {
                sqlQuery = "INSERT INTO [InternalNotes] ([RefId],[NoteType],[Note],[NoteDate],[CurrentUser],[WineryID]) VALUES (@userid,2,@UserNote,@NoteDate,@CurrentUser,@WineryId)";
            }
            else
            {
                sqlQuery = "UPDATE [InternalNotes] SET [Note] = @UserNote,[NoteDate] = @NoteDate,[CurrentUser] = @CurrentUser WHERE RefId=@userid and WineryID=@WineryId";
            }

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public ClientModel ValidateLogin(int member_id, string username, string password, AppType appType)
        {
            ClientModel clientModel = new ClientModel();
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            string sql = string.Empty;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@member_id", member_id));
            if (appType == AppType.BOXOFFICEAPP)
            {
                sql = "select DisplayName,SmsNumber,BillingPlan,TicketPlan,bp.Name as BillingPlanName,tp.Name as TicketPlanName,PlanType,EnableVin65,eWineryEnabled,EnableSalesforce,EnableClubVin65,EnableClubeCellar,EnableVintegrate,EnableClubeWinery,EnableClubSalesforce, w.InventoryMode,EnableGLPPoS,EnableCellarScout,PrivateRsvpConfirmationEmailTemplate from Winery w left join BillingPlans bp on bp.id=w.BillingPlan left join Tickets_Plans tp on tp.id=w.TicketPlan where w.id=@member_id and AttendeeAppUsername=@username and AttendeeAppPassword=@password";
                parameterList.Add(GetParameter("@username", username));
                parameterList.Add(GetParameter("@password", password));
            }
            else
            {
                List<SettingModel> settingsGroup = eventDAL.GetSettingGroup(member_id, Common.Common.SettingGroup.member);

                if ((settingsGroup != null))
                {
                    if (settingsGroup.Count > 0)
                    {
                        string savedUsername = "";
                        string savedPassword = "";
                        dynamic dbusername = settingsGroup.Where(f => f.Key == Common.Common.SettingKey.member_table_pro_username).FirstOrDefault();
                        dynamic dbpassword = settingsGroup.Where(f => f.Key == Common.Common.SettingKey.member_table_pro_password).FirstOrDefault();

                        if ((dbusername != null) && (dbpassword != null))
                        {
                            savedUsername = dbusername.Value;
                            savedPassword = dbpassword.Value;
                        }

                        if ((savedUsername == username.Trim()) && (savedPassword == password.TrimEnd()))
                            sql = "select DisplayName,SmsNumber,BillingPlan,TicketPlan,bp.Name as BillingPlanName,tp.Name as TicketPlanName,PlanType,EnableVin65,eWineryEnabled,EnableSalesforce,EnableClubVin65,EnableClubeCellar,EnableVintegrate,EnableClubeWinery,EnableClubSalesforce, w.InventoryMode,EnableGLPPoS,EnableCellarScout,PrivateRsvpConfirmationEmailTemplate from Winery w left join BillingPlans bp on bp.id=w.BillingPlan left join Tickets_Plans tp on tp.id=w.TicketPlan where w.id=@member_id";
                        else if (savedUsername == username.Trim())
                            clientModel.client_name = "API Username and/or API Password does not match on record";
                    }
                }
            }

            if (sql.Length > 0)
            {
                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            clientModel.client_authorized = true;
                            clientModel.client_id = member_id;
                            clientModel.client_name = Convert.ToString(dataReader["DisplayName"]);
                            clientModel.sms_number = Convert.ToString(dataReader["SmsNumber"]);
                            clientModel.inventory_mode = Convert.ToInt32(dataReader["InventoryMode"]);
                            clientModel.pos_enabled = Convert.ToBoolean(dataReader["EnableGLPPoS"]);
                            clientModel.cellar_scout_member = Convert.ToBoolean(dataReader["EnableCellarScout"]);
                            clientModel.private_rsvp_email_template_id = Convert.ToInt32(dataReader["PrivateRsvpConfirmationEmailTemplate"]);

                            bool bLoyalEnabled = false;
                            bool bLoyalClubLookupEnabled = false;
                            string club_partner = string.Empty;
                            string ecom_provider = string.Empty;

                            List<SettingModel> settingsGroup = eventDAL.GetSettingGroup(member_id, Common.Common.SettingGroup.bLoyal);

                            if ((settingsGroup != null))
                            {
                                if (settingsGroup.Count > 0)
                                {
                                    string bLoyalApiEnabled = "";
                                    string bLoyalApiClubLookup = "";
                                    dynamic dbbLoyalApiEnabled = settingsGroup.Where(f => f.Key == Common.Common.SettingKey.bLoyalApiEnabled).FirstOrDefault();
                                    dynamic dbbLoyalApiClubLookup = settingsGroup.Where(f => f.Key == Common.Common.SettingKey.bLoyalApiClubLookup).FirstOrDefault();

                                    if ((dbbLoyalApiEnabled != null))
                                    {
                                        bLoyalApiEnabled = dbbLoyalApiEnabled.Value;
                                    }

                                    if ((dbbLoyalApiClubLookup != null))
                                    {
                                        bLoyalApiClubLookup = dbbLoyalApiClubLookup.Value;
                                    }

                                    if (bLoyalApiEnabled.ToLower() == "true")
                                    {
                                        bLoyalEnabled = true;
                                    }

                                    if (bLoyalApiClubLookup.ToLower() == "true")
                                    {
                                        bLoyalClubLookupEnabled = true;
                                    }
                                }
                            }

                            if (Convert.ToBoolean(dataReader["EnableVin65"]))
                            {
                                ecom_provider = "Vin65";
                            }
                            else if (Convert.ToBoolean(dataReader["eWineryEnabled"]))
                            {
                                ecom_provider = "eWinery";
                            }
                            else if (Convert.ToBoolean(dataReader["EnableSalesforce"]))
                            {
                                ecom_provider = "Salesforce";
                            }
                            else if (bLoyalEnabled)
                            {
                                ecom_provider = "bLoyal";
                            }
                            else
                            {
                                ecom_provider = "None";
                            }

                            if (Convert.ToBoolean(dataReader["EnableClubVin65"]))
                            {
                                club_partner = "Vin65";
                            }
                            else if (Convert.ToBoolean(dataReader["EnableClubeCellar"]))
                            {
                                club_partner = "eCellar";
                            }
                            else if (Convert.ToBoolean(dataReader["EnableVintegrate"]))
                            {
                                club_partner = "Vintegrate";
                            }
                            else if (Convert.ToBoolean(dataReader["EnableClubeWinery"]))
                            {
                                club_partner = "eWinery";
                            }
                            else if (Convert.ToBoolean(dataReader["EnableClubSalesforce"]))
                            {
                                club_partner = "Salesforce";
                            }
                            else if (bLoyalClubLookupEnabled)
                            {
                                club_partner = "bLoyal";
                            }
                            else
                            {
                                club_partner = "None";
                            }

                            clientModel.club_partner = club_partner;
                            clientModel.ecom_provider = ecom_provider;

                            RsvpPlan rsvpPlan = null;
                            TicketingPlan ticketingPlan = null;

                            int rsvpPlanId = Convert.ToInt32(dataReader["BillingPlan"]);
                            int ticketPlanId = Convert.ToInt32(dataReader["TicketPlan"]);

                            if (rsvpPlanId > 0)
                            {
                                rsvpPlan = new RsvpPlan();
                                rsvpPlan.id = rsvpPlanId;
                                rsvpPlan.plan_name = Convert.ToString(dataReader["BillingPlanName"]);
                                rsvpPlan.plan_type = Convert.ToInt32(dataReader["PlanType"]);

                            }

                            if (ticketPlanId > 0)
                            {
                                ticketingPlan = new TicketingPlan();
                                ticketingPlan.id = Convert.ToInt32(dataReader["TicketPlan"]);
                                ticketingPlan.plan_name = Convert.ToString(dataReader["TicketPlanName"]);
                            }



                            clientModel.ticketing_plan = ticketingPlan;
                            clientModel.rsvp_plan = rsvpPlan;
                        }
                    }
                }
            }

            return clientModel;
        }

        public bool UpdatePasswordChangeKey(int userId, string PasswordChangeKey)
        {
            string sql = "update [User] set PasswordChangeKey=@PasswordChangeKey where Id=@UserId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", userId));
            parameterList.Add(GetParameter("@PasswordChangeKey", PasswordChangeKey));

            int ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);

            return (ret > 0);

        }

        public UserDetailModel GetUserIdBybyPasswordKey(string PasswordKey)
        {
            UserDetailModel userModel = new UserDetailModel();

            string sql = "select Id,username from [user] where PasswordChangeKey=@PasswordKey";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@PasswordKey", PasswordKey));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        userModel.user_id = Convert.ToInt32(dataReader["Id"]);
                        userModel.email = Convert.ToString(dataReader["username"]);
                    }
                }
            }
            return userModel;
        }

        public bool ResetPassword(int userId, string Password)
        {
            string sql = "update [User] set [Password]=@Password,PasswordChangeKey='' where Id=@UserId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", userId));
            parameterList.Add(GetParameter("@Password", Password));

            int ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);

            return (ret > 0);

        }

        public bool DisableUser(int UserId, string password)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@Password", password));

            int ret = ExecuteNonQuery("DisableUser", parameterList, CommandType.StoredProcedure);
            return (ret > 0);
        }


        public bool RemoveUserFromAutoSyncingQueue(string userName)
        {
            string sql = "update [User] set ThirdPartyautosync=ThirdPartyautosync+1 where UserName=@username";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@username", userName));

            int ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);

            return (ret > 0);

        }

        public bool IsUserLockedOut(int id)
        {
            string sql = "select LockoutCount, LockoutDate from [user] where Id=@id";
            bool isLockedOut = false;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@id", id));
            try
            {
                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            int lockedCount = Convert.ToInt32(dataReader["LockoutCount"]);
                            DateTime dtLockedDate = Convert.ToDateTime(dataReader["LockoutDate"]);
                            if (lockedCount >= 6 && DateTime.UtcNow < dtLockedDate.AddMinutes(30))
                            {
                                isLockedOut = true;
                            }
                        }
                    }
                }
            }
            catch { }
            return isLockedOut;

        }

        public List<UsersForSync> GetUserNameForSync(int WineryId)
        {
            List<UsersForSync> list = new List<UsersForSync>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", WineryId));

            using (DbDataReader dataReader = GetDataReader("GetUserNameForSync", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        UsersForSync data = new UsersForSync();

                        data.Id = Convert.ToInt32(dataReader["Id"]);
                        data.UserName = Convert.ToString(dataReader["UserName"]);

                        list.Add(data);
                    }
                }
            }

            return list;

        }

        public List<UsersForSync> GetUserDetailsForSync()
        {
            List<UsersForSync> list = new List<UsersForSync>();

            using (DbDataReader dataReader = GetDataReader("GetUserDetailsForSync", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        UsersForSync data = new UsersForSync();

                        data.Id = Convert.ToInt32(dataReader["Id"]);
                        data.MemberId = Convert.ToInt32(dataReader["WineryId"]);
                        data.UserName = Convert.ToString(dataReader["UserName"]);

                        list.Add(data);
                    }
                }
            }

            return list;

        }

        public bool UpdateUserThirdPartyAutoSync(int userid)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@userid", userid));

            string sqlQuery = "update [user] set ThirdPartyautosync = ThirdPartyautosync + 1 where Id = @userid";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public int GetInventoryModeForMember(int memberId)
        {
            int invMode = 0;
            //select InventoryMode from Winery where Id=26
            string sql = "select InventoryMode from Winery where Id=@MemberId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", memberId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        invMode = Convert.ToInt32(dataReader["InventoryMode"]);
                    }
                }
            }
            return invMode;
        }

        public UserAddress GetUserAddressByZipCode(string ZipCode)
        {
            UserAddress addr = new UserAddress();

            string sql = "select top 1 city,state from [user] where zip=@ZipCode and len(address1)>0 and len(city)>0 and len(state)>0 order by id desc";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ZipCode", ZipCode));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        addr.city = Convert.ToString(dataReader["city"]);
                        addr.state = Convert.ToString(dataReader["state"]);
                    }
                }
            }
            return addr;
        }

        public List<GuestTagModel> GetUserGuestTags(int UserId, int WineryId)
        {
            var list = new List<GuestTagModel>();

            string sql = "select ugt.id as Id ,gt.Id as TagId, gt.Tag as Tag from User_Guest_Tags ugt inner join Guest_Tags gt on gt.Id = ugt.TagId where ugt.UserId = @UserId and ugt.MemberId = @MemberId order by SortOrder";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@MemberId", WineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new GuestTagModel();

                        model.id = Convert.ToInt32(dataReader["TagId"]);
                        model.tag = Convert.ToString(dataReader["Tag"]);

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public MyAccountDetailsModel GetMyAccountDetails(int UserId)
        {
            MyAccountDetailsModel data = new MyAccountDetailsModel();

            var user_reservation = new List<UserReservationsViewModel>();
            var user_orders = new List<UserOrdersViewModel>();

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@UserId", UserId));

            using (DbDataReader dataReader = GetDataReader("GetMyAccountDetails", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        data.user_name = Convert.ToString(dataReader["UserName"]);
                        data.first_name = Convert.ToString(dataReader["FirstName"]);
                        data.last_name = Convert.ToString(dataReader["LastName"]);
                        data.company_name = Convert.ToString(dataReader["CompanyName"]);
                        data.birth_date = Convert.ToDateTime(dataReader["BirthDate"]);
                        data.country = Convert.ToString(dataReader["Country"]);
                        data.address_1 = Convert.ToString(dataReader["Address1"]);
                        data.address_2 = Convert.ToString(dataReader["Address2"]);
                        data.state = Convert.ToString(dataReader["State"]);
                        data.city = Convert.ToString(dataReader["City"]);
                        data.zip = Convert.ToString(dataReader["Zip"]);
                        data.cell_phone_str = Convert.ToString(dataReader["CellPhoneStr"]);
                        data.home_phone_str = Convert.ToString(dataReader["HomePhoneStr"]);
                        data.work_phone_str = Convert.ToString(dataReader["WorkPhoneStr"]);
                        data.preferred_appellation = Convert.ToInt32(dataReader["PreferredAppellation"]);
                        data.is_concierge = Convert.ToBoolean(dataReader["IsConcierge"]);
                        data.concierge_type = Convert.ToInt32(dataReader["ConciergeType"]);
                        data.title = Convert.ToString(dataReader["Title"]);
                        data.website = Convert.ToString(dataReader["Website"]);
                        data.gender = Convert.ToString(dataReader["Gender"]);
                        data.age = Convert.ToInt32(dataReader["Age"]);
                        data.role_id = Convert.ToInt32(dataReader["RoleId"]);
                        data.favorites_count = Convert.ToInt32(dataReader["FavoritesCount"]);
                        data.earned_points = Convert.ToInt32(dataReader["EarnedPoints"]);
                        data.next_reward_points = Convert.ToInt32(dataReader["NextRewardPoints"]);
                        data.goal_percentage = Convert.ToInt32(dataReader["GoalPercentage"]);
                        data.user_image = Convert.ToString(dataReader["UserImage"]);
                        data.system_site_gated = Convert.ToBoolean(dataReader["system_site_gated"]);
                        data.site_wide_message = Convert.ToString(dataReader["SiteWideMessage"]);
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var model = new UserReservationsViewModel();
                            //           BookingDate IsPastEvent        
                            //         EventEndTime        
                            //         CancelPolicy    CancelTime MemberBusinessPhone
                            model.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                            model.member_id = Convert.ToInt32(dataReader["WineryId"]);
                            model.member_name = Convert.ToString(dataReader["WineryName"]);
                            model.guest_last_name = Convert.ToString(dataReader["GuestLastName"]);
                            model.event_name = Convert.ToString(dataReader["EventName"]);
                            model.booking_code = Convert.ToString(dataReader["BookingCode"]);
                            model.status = (ReservationStatus)Convert.ToInt32(dataReader["Status"]);
                            model.total_guests = Convert.ToInt32(dataReader["TotalGuests"]);
                            model.star = Convert.ToInt32(dataReader["Star"]);
                            model.purchase_url = Convert.ToString(dataReader["PurchaseUrl"]);
                            model.booking_guid = Convert.ToString(dataReader["BookingGuid"]);
                            model.allow_cancel = Convert.ToBoolean(dataReader["AllowCancel"]);
                            model.destination_name = Convert.ToString(dataReader["DestinationName"]);
                            model.limit_my_account = Convert.ToBoolean(dataReader["LimitMyAccount"]);
                            model.covid19_required = Convert.ToString(dataReader["Covid19Required"]);
                            model.waiver_required = Convert.ToString(dataReader["WaiverRequired"]);
                            model.survey_date = Convert.ToDateTime(dataReader["SurveyDate"]);
                            model.survey_expiry_date = Convert.ToDateTime(dataReader["SurveyExpiryDate"]);
                            model.is_waiver = Convert.ToBoolean(dataReader["IsWaiver"]);

                            DateTime EventDate = Convert.ToDateTime(dataReader["EventDate"]);
                            model.event_date_format = EventDate.ToString("dddd, MMM d yyyy", CultureInfo.InvariantCulture) + " at " + EventDate.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                            model.event_time_format = EventDate.ToString("HH:mm", CultureInfo.InvariantCulture);
                            
                            model.is_reviewed = model.star <= 0;

                            if (model.status == ReservationStatus.Pending)
                            {
                                model.status_text = "Confirmed";
                                model.status_css_class = "badge-rsvp-confirmed";
                            }
                            else if (model.status == ReservationStatus.Completed)
                            {
                                model.status_text = "Checked-in";
                                model.status_css_class = "badge-rsvp-checked-in";
                            }
                            else if (model.status == ReservationStatus.Cancelled)
                            {
                                model.status_text = "Cancelled";
                                model.status_css_class = "badge-rsvp-cancelled";
                            }
                            else if (model.status == ReservationStatus.NoShow)
                            {
                                model.status_text = "No Show";
                                model.status_css_class = "badge-rsvp-no-show";
                            }
                            else if (model.status == ReservationStatus.Rescheduled)
                            {
                                model.status_text = "Rescheduled";
                                model.status_css_class = "badge-rsvp-rescheduled";
                            }
                            else if (model.status == ReservationStatus.GuestDelayed)
                            {
                                model.status_text = "Guest Delayed";
                                model.status_css_class = "badge-rsvp-guest-delayed";
                            }
                            else if (model.status == ReservationStatus.Updated)
                            {
                                model.status_text = "Updated";
                                model.status_css_class = "badge-rsvp-updated";
                            }
                            else if (model.status == ReservationStatus.YelpInitiated)
                            {
                                model.status_text = "Yelp Temporary";
                                model.status_css_class = "badge-rsvp-yelp-initiated";
                            }
                            else if (model.status == ReservationStatus.Initiated)
                            {
                                model.status_text = "Initiated";
                                model.status_css_class = "badge-rsvp-initiated";
                            }

                            user_reservation.Add(model);
                        }
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var model = new UserOrdersViewModel();
                            
                            model.event_title = Convert.ToString(dataReader["EventTitle"]);
                            model.member_name = Convert.ToString(dataReader["WineryName"]);
                            model.order_id = Convert.ToInt32(dataReader["OrderId"]);
                            model.no_of_tickets = Convert.ToInt32(dataReader["NoOfTickets"]);
                            model.order_guid = Convert.ToString(dataReader["OrderGuid"]);
                            model.purchase_url = Convert.ToString(dataReader["PurchaseUrl"]);
                            model.is_self_print = Convert.ToBoolean(dataReader["IsSelfPrint"]);
                            model.primary_category = Convert.ToInt32(dataReader["PrimaryCategory"]);

                            DateTime StartDateTime= Convert.ToDateTime(dataReader["StartDateTime"]);
                            DateTime EndDateTime = Convert.ToDateTime(dataReader["EndDateTime"]);

                            model.order_total_text = Convert.ToDecimal(dataReader["OrderTotal"]).ToString("C", CultureInfo.GetCultureInfo("en-US"));
                            model.is_single_date = StartDateTime.Date == EndDateTime.Date;
                            model.start_date_time_formated = StartDateTime.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
                            model.end_date_time_formated = EndDateTime.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
                            model.order_date_formated = Convert.ToDateTime(dataReader["OrderDate"]).ToString("MMMM d, yyyy hh:mm tt", CultureInfo.InvariantCulture);

                            TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                            model.event_url = ticketDAL.GetFriendlyURL(model.event_title, Convert.ToInt32(dataReader["EventId"]));

                            user_orders.Add(model);
                        }
                    }

                    data.user_orders = user_orders;
                    data.user_reservation = user_reservation;
                }
            }

            return data;
        }

        public MyAccountDetailsV2Model GetMyAccountDetailsV2(int UserId)
        {
            MyAccountDetailsV2Model data = new MyAccountDetailsV2Model();

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@UserId", UserId));

            using (DbDataReader dataReader = GetDataReader("GetMyAccountDetailsV2", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        data.user_name = Convert.ToString(dataReader["UserName"]);
                        data.first_name = Convert.ToString(dataReader["FirstName"]);
                        data.last_name = Convert.ToString(dataReader["LastName"]);
                        data.company_name = Convert.ToString(dataReader["CompanyName"]);
                        data.birth_date = Convert.ToDateTime(dataReader["BirthDate"]);
                        data.country = Convert.ToString(dataReader["Country"]);
                        data.address_1 = Convert.ToString(dataReader["Address1"]);
                        data.address_2 = Convert.ToString(dataReader["Address2"]);
                        data.state = Convert.ToString(dataReader["State"]);
                        data.city = Convert.ToString(dataReader["City"]);
                        data.zip = Convert.ToString(dataReader["Zip"]);
                        data.cell_phone_str = Convert.ToString(dataReader["CellPhoneStr"]);
                        data.home_phone_str = Convert.ToString(dataReader["HomePhoneStr"]);
                        data.work_phone_str = Convert.ToString(dataReader["WorkPhoneStr"]);
                        data.preferred_appellation = Convert.ToInt32(dataReader["PreferredAppellation"]);
                        data.is_concierge = Convert.ToBoolean(dataReader["IsConcierge"]);
                        data.concierge_type = Convert.ToInt32(dataReader["ConciergeType"]);
                        data.title = Convert.ToString(dataReader["Title"]);
                        data.website = Convert.ToString(dataReader["Website"]);
                        data.gender = Convert.ToString(dataReader["Gender"]);
                        data.age = Convert.ToInt32(dataReader["Age"]);
                        data.role_id = Convert.ToInt32(dataReader["RoleId"]);
                        data.favorites_count = Convert.ToInt32(dataReader["FavoritesCount"]);
                        data.earned_points = Convert.ToInt32(dataReader["EarnedPoints"]);
                        data.next_reward_points = Convert.ToInt32(dataReader["NextRewardPoints"]);
                        data.goal_percentage = Convert.ToInt32(dataReader["GoalPercentage"]);
                        data.user_image = Convert.ToString(dataReader["UserImage"]);
                        data.system_site_gated = Convert.ToBoolean(dataReader["system_site_gated"]);
                        data.site_wide_message = Convert.ToString(dataReader["SiteWideMessage"]);
                        data.reservation_count = Convert.ToInt32(dataReader["ReservationCount"]);
                        data.ticket_orders_count = Convert.ToInt32(dataReader["TicketOrderCount"]);
                        data.waitlist_count = Convert.ToInt32(dataReader["waitlistCount"]);
                        data.itinerary_count = Convert.ToInt32(dataReader["itineraryCount"]);
                    }
                }
            }

            return data;
        }


        public bool SetUserFavorites(int WineryId, int UserId,int EventId)
        {         
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@EventId", EventId));


            int ret = ExecuteNonQuery("SetUserfavoritesWinery", parameterList, CommandType.StoredProcedure);

            return (ret > 0);
        }

        public bool RemoveUserFavorites(int WineryId, int UserId, int EventId)
        {
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@EventId", EventId));

            int ret = ExecuteNonQuery("RemoveUserfavoritesWinery", parameterList, CommandType.StoredProcedure);

            return (ret > 0);
        }

        public List<UserNewsletterModel> GetNewslettersByUserId(int UserId)
        {
            var list = new List<UserNewsletterModel>();

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@UserId", UserId));

            using (DbDataReader dataReader = GetDataReader("GetNewslettersByUserId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new UserNewsletterModel();

                        model.id = Convert.ToInt32(dataReader["id"]);
                        model.newsletter = Convert.ToString(dataReader["Newsletter"]);
                        model.is_subscribed = Convert.ToBoolean(dataReader["is_subscribed"]);

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public void SubscribedUserNewsletter(int UserId, List<int> newsletter_ids)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@UserId", UserId));
            //parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@NewsletterIds", String.Join(",", newsletter_ids)));

            ExecuteNonQuery("SubscribedUserNewsletter", parameterList, CommandType.StoredProcedure);
        }

        public MyAccountDataViewModel GetMyAccountData(int user_Id)
        {
            var model = new MyAccountDataViewModel();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@userId", user_Id));

            using (DbDataReader dataReader = GetDataReader("GetMyAccountData", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.user_name = Convert.ToString(dataReader["UserName"]);
                        model.first_name = Convert.ToString(dataReader["FirstName"]);
                        model.last_name = Convert.ToString(dataReader["LastName"]);
                        model.is_concierge = Convert.ToBoolean(dataReader["IsConcierge"]);
                        model.concierge_type = Convert.ToInt32(dataReader["ConciergeType"]);
                        model.itinerary_count = Convert.ToInt32(dataReader["ItineraryCount"]);
                        model.reservations_count = Convert.ToInt32(dataReader["ReservationsCount"]);
                        model.ticket_order_count = Convert.ToInt32(dataReader["TicketOrderCount"]);
                        model.waitlist_count = Convert.ToInt32(dataReader["WaitListCount"]);
                        model.favorites_count = Convert.ToInt32(dataReader["FavoritesCount"]);
                        model.special_offer_count = Convert.ToInt32(dataReader["SpecialOfferCount"]);
                        model.image_url = Convert.ToString(dataReader["UserImage"]);
                    }
                }
            }
            return model;
        }

        public UserSessionModel GetUser(string username, string reqpassword, out string errorMessage)
        {
            string Password = string.Empty;
            int LockoutCount = 0;
            DateTime LockoutDate = Convert.ToDateTime("1/1/1900");

            UserSessionModel data = new UserSessionModel();
            var user_wineries = new List<UserSessionWineryModel>();

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@Email", username));

            using (DbDataReader dataReader = GetDataReader("GetUserDetailByUserName", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        data.user_name = Convert.ToString(dataReader["UserName"]);
                        data.first_name = Convert.ToString(dataReader["FirstName"]);
                        data.last_name = Convert.ToString(dataReader["LastName"]);
                        data.default_role_id = Convert.ToInt32(dataReader["RoleId"]);
                        data.user_id = Convert.ToInt32(dataReader["UserId"]);
                        Password = Convert.ToString(dataReader["Password"]);
                        LockoutCount = Convert.ToInt32(dataReader["LockoutCount"]);
                        LockoutDate = Convert.ToDateTime(dataReader["LockoutDate"]);
                        data.login_status = LoginStatus.Success;
                    }

                    if (data.user_id > 0)
                    {
                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                var model = new UserSessionWineryModel();

                                model.role_id = Convert.ToInt32(dataReader["RoleId"]);
                                model.winery_id = Convert.ToInt32(dataReader["WineryId"]);
                                model.customer_reference_number = Convert.ToString(dataReader["CustomerReferenceNumber"]);

                                user_wineries.Add(model);
                            }
                        }
                    }
                        
                    data.user_wineries = user_wineries;
                }
            }

            if (data.user_id == 0)
            {
                data.login_status = LoginStatus.Failed;
                errorMessage = GetLoginMessage(data);
                return data;
            }

            // Check user is locked out.
            var locked = IsLockedOut(LockoutCount, LockoutDate);
            if (locked != null)
            {
                data.login_status = locked.Value;
                errorMessage = GetLoginMessage(data);
                return data;
            }

            string encryptpassword = StringHelpers.EncryptOneWay(reqpassword);

            if (Password != encryptpassword)
            {
                UpdateUserLockout(data.user_id, false, LockoutCount, LockoutDate);
                data.login_status = LoginStatus.Failed;
                errorMessage = GetLoginMessage(data);
                return data;
            }

            UpdateUserLockout(data.user_id, true, LockoutCount, LockoutDate);

            

            errorMessage = GetLoginMessage(data);
            return data;
        }

        public void UpdateUserLockout(int userId, bool isResetLockout,int LockoutCount,DateTime LockoutDate)
        {
            // if blocked we do not do anything
            if (LockoutCount == -9999)
            {
                return;
            }

            // Not blocked so check the normal lockout
            if (isResetLockout && LockoutCount > -9999)
            {
                LockoutCount = 0;
            }
            else
            {
                if (LockoutDate > DateTime.UtcNow.AddMinutes(-30))
                {
                    // If the last failed login was within 30 minutes then update lock count
                    LockoutCount += 1;
                }
                else
                {
                    // If not in last 30 then start at 1 again
                    LockoutCount = 1;
                }
            }

            // Set date at first failed attempt so we can check the 30 min threshold and set it at 3 so we can use that date to check the 24 waiting period
            if (LockoutCount == 1 || LockoutCount == 3)
            {
                LockoutDate = DateTime.UtcNow;
            }

            string sql = "update [User] set LoginDate=@LoginDate,LockoutDate=@LockoutDate,LockoutCount=@LockoutCount where Id=@UserId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@LoginDate", DateTime.Now));
            parameterList.Add(GetParameter("@LockoutDate", LockoutDate));
            parameterList.Add(GetParameter("@LockoutCount", LockoutCount));
            parameterList.Add(GetParameter("@UserId", userId));

            int ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);
        }

        private LoginStatus? IsLockedOut(int LockoutCount,DateTime LockoutDate)
        {
            if (LockoutCount == -9999)
            {
                return LoginStatus.Blocked;
            }
            else if (LockoutCount >= 3 && DateTime.UtcNow < LockoutDate.AddMinutes(30))
            {
                return LoginStatus.Locked;
            }

            return null;
        }

        private string GetLoginMessage(UserSessionModel model)
        {
            if (model.login_status == LoginStatus.Failed)
            {
                return "We didn't recognize the email or password you entered.";
            }
            else if (model.login_status == LoginStatus.Locked)
            {
                return "We're sorry, but your CellarPass account has been temporarily locked because of too many invalid login attempts. <p>As a security precaution, you must wait 30 minutes before requesting a password reset.</p>";
            }
            else if (model.login_status == LoginStatus.Blocked)
            {
                return "<i class=\"fa fa-exclamation-triangle\" aria-hidden=\"true\"></i> Account Locked Out <i class=\"fa fa-exclamation-triangle\" aria-hidden=\"true\"></i><br><br>When you accumulate three or more \"no-show\" reservations in a calendar year, your account is automatically deactivated and suspended from being able to book any Reservations. We have established this practice to ensure that our partners can enjoy the patronage of guests who regularly check-in or cancel their reservations if they cannot honor them.<br><br>If you find this message to be in error, please contact CellarPass at 707-255-4390 Monday through Friday, 9AM – 5PM Pacific.";
            }

            return string.Empty;
        }

        public List<UserFavoriteMemberViewModel> GetFavoriteMembers(int userID)
        {
            List<UserFavoriteMemberViewModel> list = new List<UserFavoriteMemberViewModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@userID", userID));

            using (DbDataReader dataReader = GetDataReader("GetUserFavoriteMembers", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new UserFavoriteMemberViewModel();
                              
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.address_1 = Convert.ToString(dataReader["Address1"]);
                        model.address_2 = Convert.ToString(dataReader["Address2"]);
                        model.appelation = Convert.ToString(dataReader["Appelation"]);
                        model.city = Convert.ToString(dataReader["City"]);
                        model.country = Convert.ToString(dataReader["Country"]);
                        model.display_name = Convert.ToString(dataReader["DisplayName"]);
                        model.member_ava = Convert.ToString(dataReader["WineryAva"]);
                        model.purchase_url = Convert.ToString(dataReader["PurchaseURL"]);
                        model.reviews = Convert.ToInt32(dataReader["Reviews"]);
                        model.star = Convert.ToDecimal(dataReader["Star"]);
                        model.state = Convert.ToString(dataReader["State"]);
                        model.zip = Convert.ToString(dataReader["Zip"]);

                        string Address = model.address_1;

                        if (!string.IsNullOrWhiteSpace(model.address_2))
                        {
                            Address += model.address_2 + ", ";
                        }
                        Address += model.city + " ";
                        Address += model.state + " ";
                        Address += model.country + " ";
                        Address += model.zip;

                        model.address = Address;

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<FavoriteEventViewModel> GetFavoriteEvents(int userID)
        {
            List<FavoriteEventViewModel> list = new List<FavoriteEventViewModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@userID", userID));

            using (DbDataReader dataReader = GetDataReader("GetUserFavoriteEventsV3", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new FavoriteEventViewModel();

                        model.end_date_time = Convert.ToDateTime(dataReader["EndDateTime"]);
                        model.event_id = Convert.ToInt32(dataReader["EventId"]);
                        model.event_image = Convert.ToString(dataReader["EventImage"]);
                        model.event_title = Convert.ToString(dataReader["EventTitle"]);
                        model.is_favorites = Convert.ToBoolean(dataReader["IsFavorites"]);
                        model.location_id = Convert.ToInt32(dataReader["LocationId"]);
                        model.max_capacity = Convert.ToInt32(dataReader["MaxCapacity"]);
                        model.Offer_id = Convert.ToInt32(dataReader["OfferId"]);
                        model.offer_member_name = Convert.ToString(dataReader["OfferWineryName"]);
                        model.organizer_name = Convert.ToString(dataReader["OrganizerName"]);
                        model.primary_category = Convert.ToString(dataReader["PrimaryCategory"]);
                        model.secondary_category = Convert.ToString(dataReader["SecondaryCategory"]);
                        model.start_date_time = Convert.ToDateTime(dataReader["StartDateTime"]);
                        model.tickets_sold = Convert.ToInt32(dataReader["TicketsSold"]);
                        model.time_zone = Convert.ToString(dataReader["TimeZone"]);
                        model.venue_city = Convert.ToString(dataReader["VenueCity"]);
                        model.venue_county = Convert.ToString(dataReader["VenueCounty"]);
                        model.venue_state = Convert.ToString(dataReader["VenueState"]);
                        model.state_name = Convert.ToString(dataReader["StateName"]);


                        model.event_image_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImage"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        model.event_image_big = Convert.ToString(dataReader["EventImageBig"]);
                        model.event_image_big_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImageBig"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));

                        if (model.start_date_time.Minute > 0)
                        {
                            model.event_date = model.start_date_time.ToString("dddd, MMM d yyyy, h:mmtt");
                        }
                        else
                        {
                            model.event_date = model.start_date_time.ToString("dddd, MMM d yyyy, htt");
                        }

                        model.is_single_date = model.start_date_time.Date == model.end_date_time.Date;
                        model.start_date_month = model.start_date_time.ToString("MMM");
                        model.start_date_date = model.start_date_time.ToString("dd").TrimStart('0');
                        model.end_date_month = model.end_date_time.ToString("MMM");
                        model.end_date_date = model.end_date_time.ToString("dd").TrimStart('0');

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public bool SaveUserImage(int user_id, string filename)
        {
            string sql = "update [user] set UserImage = @UserImage where id = @Id";
            int ret = 0;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", user_id));
            parameterList.Add(GetParameter("@UserImage", filename));
            try
            {
                ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);
            }
            catch (Exception e)
            {
                return false;
            }
            return (ret > 0);
        }

        public bool ChangePassword(string username, string password)
        {
            string sql = "update [user] set [password] = @password where UserName = @username";
            int ret = 0;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@username", username));
            parameterList.Add(GetParameter("@password", password));
            try
            {
                ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);
            }
            catch (Exception e)
            {
                return false;
            }
            return (ret > 0);
        }

        public int CreateUser(CreateAccountModel model)
        {
            int id = 0;
            try
            {
                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@UserName", model.user_name));
                parameterList.Add(GetParameter("@Password", model.password));
                parameterList.Add(GetParameter("@FirstName", StringHelpers.ToTitleCase(model.first_name)));
                parameterList.Add(GetParameter("@LastName", StringHelpers.ToTitleCase(model.last_name)));
                parameterList.Add(GetParameter("@Address1", model.address_1));
                parameterList.Add(GetParameter("@Address2", model.address_2));
                parameterList.Add(GetParameter("@City", model.city));
                parameterList.Add(GetParameter("@State", model.state));
                parameterList.Add(GetParameter("@Country", model.country));
                parameterList.Add(GetParameter("@Zip", model.zip_code));
                parameterList.Add(GetParameter("@PhoneNum", model.phone_number));
                parameterList.Add(GetParameter("@createdByUser", ""));
                parameterList.Add(GetParameter("@CustomerNumber", ""));
                parameterList.Add(GetParameter("@SecondaryEmail", ""));
                parameterList.Add(GetParameter("@HomePhone", 0));
                parameterList.Add(GetParameter("@BirthDate", "1/1/1900"));
                parameterList.Add(GetParameter("@Active", true));
                parameterList.Add(GetParameter("@SMSVerified", 0));
                parameterList.Add(GetParameter("@mobilephonestatus", 0));
                parameterList.Add(GetParameter("@regionMostVisited", model.destination_id));
                parameterList.Add(GetParameter("@CompanyName", model.company_name));

                if (model.is_concierge)
                {
                    parameterList.Add(GetParameter("@RoleId", 6));
                    parameterList.Add(GetParameter("@AccountType", 2));
                    parameterList.Add(GetParameter("@Conciergetype", model.concierge_type));
                }
                else
                {
                    parameterList.Add(GetParameter("@RoleId", 4));
                    parameterList.Add(GetParameter("@AccountType", 0));
                    parameterList.Add(GetParameter("@Conciergetype", 0));
                }

                using (DbDataReader dataReader = GetDataReader("User_INSERT", parameterList))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            id = Convert.ToInt32(dataReader["Id"]);
                        }
                    }

                }

            }
            catch (Exception e)
            {
                return 0;
            }
            
            return id;
        }

        public bool UpdateWeeklyNewsletter(string email, bool WeeklyNewsletter)
        {
            string sql = "update [User] set WeeklyNewsletterSubscribe=@WeeklyNewsletter where UserName=@email";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@email", email));
            parameterList.Add(GetParameter("@WeeklyNewsletter", WeeklyNewsletter));

            int ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);

            return (ret > 0);

        }
    }
}
