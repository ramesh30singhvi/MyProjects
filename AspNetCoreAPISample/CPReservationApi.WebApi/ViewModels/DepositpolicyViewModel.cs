using System.Collections.Generic;
using CPReservationApi.Model;

namespace CPReservationApi.WebApi.ViewModels
{
    public class DepositpolicyModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool requires_credit_card { get; set; }
        public bool is_default { get; set; } = false;
        public bool cvv_required { get; set; }
    }

    public class DepositpolicyResponse : BaseResponse2
    {
        public DepositpolicyResponse()
        {
            data = new List<DepositpolicyModel>();
        }
        public List<DepositpolicyModel> data { get; set; }
    }

    public class PreferredVisitDurationResponse : BaseResponse2
    {
        public PreferredVisitDurationResponse()
        {
            data = new List<PreferredVisitDurationModel>();
        }
        public List<PreferredVisitDurationModel> data { get; set; }
    }

    public class AffiliateTypesResponse : BaseResponse2
    {
        public AffiliateTypesResponse()
        {
            data = new List<KeyValueModel>();
        }
        public List<KeyValueModel> data { get; set; }
    }

    public class KeyValueModel
    {
        public string text { get; set; }
        public string value { get; set; }
    }

    public class PreferredVisitDurationModel
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class ReasonforVisitResponse : BaseResponse2
    {
        public ReasonforVisitResponse()
        {
            data = new List<ReasonforVisitModel>();
        }
        public List<ReasonforVisitModel> data { get; set; }
    }

    public class ReasonforVisitModel
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class CountryResponse : BaseResponse2
    {
        public CountryResponse()
        {
            data = new List<CountryModel>();
        }
        public List<CountryModel> data { get; set; }
    }
}
