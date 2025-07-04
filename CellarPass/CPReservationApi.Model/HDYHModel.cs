using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class HDYHModel
    {
        public string answer_1 { get; set; }

        public string answer_2 { get; set; }

        public string answer_3 { get; set; }

        public string answer_4 { get; set; }

        public string answer_5 { get; set; }

        public string answer_6 { get; set; }

        public string answer_7 { get; set; }

        public string answer_8 { get; set; }

        public string answer_9 { get; set; }

        public string answer_10 { get; set; }
    }

    public class HDYH
    {
        public string choice { get; set; }
    }

    public class FAQ
    {
        public string question { get; set; }
        public string answer { get; set; }
    }

    public class MemberFaqModel : FAQ
    {
        public int business_id { get; set; }
    }

    public class ContactReason
    {
        public List<string> contactreason { get; set; }
    }

    public class TagModel
    {
        public int id { get; set; }
        public int member_id { get; set; }
        public string tag { get; set; }
        public Common.TagType tag_type { get; set; }
        public string tag_type_desc { get; set; } = "NA";
    }

    public class GuestTagModel
    {
        public int id { get; set; }
        public string tag { get; set; }
    }

    public class UserNewsletterModel
    {
        public int id { get; set; }
        public string newsletter { get; set; }
        public bool is_subscribed { get; set; }
    }

    public class SubscribedUserNewsletterModel
    {
        public int user_id { get; set; }
        //public int member_id { get; set; }
        public List<int> newsletter_ids { get; set; }
        //public bool is_subscribed { get; set; }
    }

    public class WineryTypesModel
    {
        public int id { get; set; }

        public string internal_name { get; set; }

        public string friendly_name { get; set; }

        public string friendly_url { get; set; }

        public bool active { get; set; }
    }

    public class CountryModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string country_code { get; set; }
    }
}
