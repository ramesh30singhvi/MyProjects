using System;
using System.Collections.Generic;
using System.Linq;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.Common
{
    public class Settings
    {

        //public enum SettingGroup
        //{
        //    Google = 10,
        //    bLoyal = 20,
        //    directory = 30,
        //    member = 40,
        //    twilio = 50,
        //    taxify = 60,
        //    mailgun = 70,
        //    mailchimp = 80,
        //    mailchimp_cp = 800,
        //    facebook = 90
        //}

        //public enum SettingKey
        //{

        //    GoogleClientID = 10,
        //    GoogleClientSecret = 11,
        //    GoogleRefreshToken = 12,
        //    GoogleCalendarAuth = 13,

        //    bLoyalApiEnabled = 20,
        //    bLoyalApiCompany = 21,
        //    bLoyalApiUsername = 22,
        //    bLoyalApiPassword = 23,
        //    bLoyalApiDeviceKey = 24,
        //    bLoyalApiBaseURL = 25,
        //    bLoyalApiClubLookup = 26,

        //    directory_byAppoinmentOnly = 30,

        //    member_byAppoinmentOnly = 30,

        //    member_defaultAccount = 41,

        //    twilio_ACCOUNTSID = 51,
        //    twilio_AUTHTOKEN = 52,

        //    taxify_ApiKey = 61,

        //    mailgun_01_key = 71,
        //    mailgun_01_domain = 72,
        //    mailgun_02_key = 73,
        //    mailgun_02_domain = 74,

        //    mailchimp_key = 81,
        //    mailchimp_list = 82,
        //    mailchimp_listname = 83,
        //    mailchimp_store = 84,

        //    mailchimp_cp_key = 801,
        //    mailchimp_cp_adminList = 802,
        //    mailchimp_cp_adminListName = 803,
        //    mailchimp_cp_guestList = 804,
        //    mailchimp_cp_guestListName = 805,
        //    mailchimp_cp_newsletterList = 806,
        //    mailchimp_cp_newsletterListName = 807,
        //    mailchimp_cp_specialOffersList = 808,
        //    mailchimp_cp_specialOffersListName = 809,
        //    mailchimp_cp_billingList = 810,
        //    mailchimp_cp_billingListName = 811,
        //    mailchimp_cp_conciergeList = 812,
        //    mailchimp_cp_conciergeListName = 813,

        //    facebook_pixel = 91

        //}

        public class Setting
        {

            private int _id;
            public int Id
            {
                get { return _id; }
                set { _id = value; }
            }

            private int _memberId;
            public int MemberId
            {
                get { return _memberId; }
                set { _memberId = value; }
            }

            private SettingGroup _group;
            public SettingGroup Group
            {
                get { return _group; }
                set { _group = value; }
            }

            private SettingKey _key;
            public SettingKey Key
            {
                get { return _key; }
                set { _key = value; }
            }

            private string _value;
            public string Value
            {
                get { return _value; }
                set { _value = value; }
            }

        }

        public static List<Setting> GetSettingGroup(int memberId, SettingGroup group)
        {

            List<Setting> listSettings = new List<Setting>();

            
            
            return listSettings;

        }

        public static int GetIntValue(List<Settings.Setting> settings, SettingKey key)
        {
            int value = 0;
            Int32.TryParse(settings.Where(f => f.Key == key).FirstOrDefault().Value,out value);
            return value;
        }


        public static string GetStrValue(List<Settings.Setting> settings, SettingKey key)
        {
            string value = "";
            dynamic dbSettings = settings.Where(f => f.Key == key).FirstOrDefault();
            if ((dbSettings != null))
            {
                value = dbSettings.Value;
            }
            else
            {
                value = "";
            }
            return value;
        }


        public static bool GetBoolValue(List<Settings.Setting> settings, SettingKey key)
        {
            bool value = false;
            dynamic dbSettings = settings.Where(f => f.Key == key).FirstOrDefault();
            if ((dbSettings != null))
            {
                bool.TryParse(dbSettings.Value,out value);
            }
            return value;
        }

        

    }
}
