using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class eWineryCustomerViewModel
    {
        public string member_id { get; set; }
        public List<eWinery_member_type> account_types { get; set; }
        public List<eWinery_club> memberships { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string zip_code { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public int cellarpass_id { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public System.DateTime date_modified { get; set; }
        public string country_code { get; set; }
        public bool member_status { get; set; }
        public AccountNote account_note { get; set; }
    }
    public class eWinery_club
    {
        public eWinery_club(string _clubId, string _clubName, bool _clubActive)
        {
            club_id = _clubId;
            club_name = _clubName;
            club_active = _clubActive;
        }

        public string club_id { get; set; }
        public string club_name { get; set; }
        public bool club_active { get; set; }
    }
    public class eWinery_member_type
    {
        public eWinery_member_type(string _typeId, string _typeDesc)
        {
            type_id = _typeId;
            type_desc = _typeDesc;
        }
        public string type_id { get; set; }
        public string type_desc { get; set; }
    }

    public class AccountType
    {
        public string ContactTypeId { get; set; }
        public string ContactType { get; set; }
        public bool ActiveClub { get; set; }
        public Common.Common.ThirdPartyType ThirdPartyType { get; set; }
    }
}
