using CPReservationApi.Common;
using CPReservationApi.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class TokenziedCardResponse : BaseResponse
    {

        public TokenziedCardResponse()
        {
            data = new TokenizedCard();
        }
        public TokenizedCard data { get; set; }

    }

    public class TokenizeCardAndAuthorizationResponse : BaseResponse
    {

        public TokenizeCardAndAuthorizationResponse()
        {
            data = new TokenizeCardAndAuthorization();
        }
        public TokenizeCardAndAuthorization data { get; set; }

    }

    public class TokenizeCardAndAuthorization
    {
        public decimal authorized_amount { get; set; }
        public string approval_code { get; set; }
    }

    public class TokenziedCardListResponse : BaseResponse
    {

        public TokenziedCardListResponse()
        {
            data = new List<TokenizedCard>();
        }
        public List<TokenizedCard> data { get; set; }

    }

    public class CreditCardListResponse : BaseResponse
    {

        public CreditCardListResponse()
        {
            data = new List<CreditCardDetail>();
        }
        public List<CreditCardDetail> data { get; set; }

    }

    public class TokenizedCard
    {
        public string card_token { get; set; }
        public string last_four_digits { get; set; }
        public string first_four_digits { get; set; }
        public bool is_expired { get; set; }
        public string customer_name { get; set; }
        public string card_type { get; set; }
        public string cust_id { get; set; }
        public string card_expiration { get; set; }
        public string card_exp_month { get; set; }
        public string card_exp_year { get; set; }

        [JsonIgnore]
        public string address1 { get; set; } = "";
        [JsonIgnore]
        public string zip_code { get; set; } = "";

        public string ErrorMessage { get; set; } = "";
        public string ErrorCode { get; set; } = "";
    }

    public class zeamsterError
    {
        public string name { get; set; }
        public string message { get; set; }
        public int code { get; set; }
        public int status { get; set; }
    }

    public class zeamsterError2
    {
        public List<string> location_id { get; set; }
        public List<string> exp_date { get; set; }
        public List<string> account_number { get; set; }
        public List<string> billing_zip { get; set; }
    }

    public class zeamsterError2Root
    {
        public zeamsterError2 errors { get; set; }
    }

    public class USAePayErrorRoot
    {
        public string error { get; set; }
        public int errorcode { get; set; }
    }

    public class TokenizedCardRequest : PayCard
    {
        public int member_id {get; set;}
        public ModuleType source_module { get; set; }
        public int ticket_event_id { get; set; }
        public int user_id { get; set; }
        public UserDetailViewModel user_info { get; set; }
        public bool ignore_avs_error { get; set; } = false;
        public int attempts { get; set; }
    }

    public class TokenizedCardAuthorizationRequest : PayCard
    {
        public int reservation_id { get; set; }
    }
}
