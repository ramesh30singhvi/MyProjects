using CPReservationApi.Common;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.WebApi.Services
{
    public class Ext_Mailgun
    {
        private IMemoryCache _cache;
        public Ext_Mailgun(IMemoryCache cache)
        {
            _cache = cache;
        }
        
        private class MailConfig
        {
            public string ApiKey { get; set; }
            public string Domain { get; set; }
        }

        private static MailConfig GetMailConfiguration(int memberId, MailgunDomain domain, bool clearCache)
        {
            MailConfig mgConfig = new MailConfig();

            //Get config from DB
            List<Settings.Setting> settingsGroup = Settings.GetSettingGroup(memberId, Common.Common.SettingGroup.mailgun).ToList();

            if (settingsGroup != null)
            {
                if (settingsGroup.Count > 0)
                {
                    switch (domain)
                    {
                        case MailgunDomain.SystemMessages:
                            mgConfig.ApiKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_01_key);
                            mgConfig.Domain = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_01_domain);
                            break;
                        case MailgunDomain.Invitations:
                            mgConfig.ApiKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_02_key);
                            mgConfig.Domain = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_02_domain);
                            break;
                        case MailgunDomain.Reservations:
                            mgConfig.ApiKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_03_key);
                            mgConfig.Domain = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_03_domain);
                            break;
                        case MailgunDomain.Ticketing:
                            mgConfig.ApiKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_04_key);
                            mgConfig.Domain = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_04_domain);
                            break;
                        case MailgunDomain.Reminder:
                            mgConfig.ApiKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_05_key);
                            mgConfig.Domain = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_05_domain);
                            break;
                        case MailgunDomain.Notification:
                            mgConfig.ApiKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_06_key);
                            mgConfig.Domain = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_06_domain);
                            break;
                        case MailgunDomain.Reviews:
                            mgConfig.ApiKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_07_key);
                            mgConfig.Domain = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailgun_07_domain);
                            break;
                    }
                }
            }

            return mgConfig;

        }

        private class MailGunMessagesResponse
        {
            public string id { get; set; }
            public string message { get; set; }
        }


        public class MailGunStatsResponse
        {
            public string description { get; set; }
            public string end { get; set; }
            public string resolution { get; set; }
            public string start { get; set; }
            public Stat[] stats { get; set; }
            public string tag { get; set; }
        }

        //Classes to suporrt the MailGunStatsResponse
        public class Accepted
        {
            public int incoming { get; set; }
            public int outgoing { get; set; }
            public int total { get; set; }
        }

        public class Delivered
        {
            public int smtp { get; set; }
            public int http { get; set; }
            public int total { get; set; }
        }

        public class Temporary
        {
            public int espblock { get; set; }
        }

        public class Permanent
        {
            public int suppress_bounce { get; set; }
            public int suppress_unsubscribe { get; set; }
            public int bounce { get; set; }
            public int total { get; set; }
        }

        public class Failed
        {
            public Temporary temporary { get; set; }
            public Permanent permanent { get; set; }
        }

        public class Opened
        {
            public int total { get; set; }
            public int unique { get; set; }
        }

        public class Clicked
        {
            public int total { get; set; }
        }

        public class Unsubscribed
        {
            public int total { get; set; }
        }

        public class Complained
        {
            public int total { get; set; }
        }

        public class Stat
        {
            public string time { get; set; }
            public Accepted accepted { get; set; }
            public Delivered delivered { get; set; }
            public Failed failed { get; set; }
            public Opened opened { get; set; }
            public Clicked clicked { get; set; }
            public Unsubscribed unsubscribed { get; set; }
            public Complained complained { get; set; }
        }
        // END Classes to support the MailGunStatsResponse

    }
}
