using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class OpenTableMemberModel
    {
        public int rid { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string postal_code { get; set; }
        public string phone_number { get; set; }
        public string metro_name { get; set; }
        public string reservation_url { get; set; }
        public string profile_url { get; set; }
        public string natural_reservation_url { get; set; }
        public string natural_profile_url { get; set; }
        public bool is_restaurant_in_group { get; set; }
        public string aggregate_score { get; set; }
        public string price_quartile { get; set; }

        private int _reviewCount = 0;
        public int? review_count {
            get
            {
                return _reviewCount;
            }
            set
            {
                if (value != null && value.HasValue)
                {
                    _reviewCount = value.Value;
                }
                else
                {
                    _reviewCount = 0;
                }
            }
        }
        public List<string> category { get; set; }
    }

    public class OpenTableDirectoryListResponseModel
    {
        public int offset { get; set; }
        public int limit { get; set; }
        public int total_items { get; set; }
        public List<OpenTableMemberModel> items { get; set; }
    }

    public class OpenTableTokenResponseModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }
}
