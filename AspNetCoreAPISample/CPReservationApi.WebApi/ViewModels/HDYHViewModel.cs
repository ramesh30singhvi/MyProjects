using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class HDYHResponse : BaseResponse2
    {
        public HDYHResponse()
        {
            data = new List<HDYH>();
        }
        public List<HDYH> data { get; set; }
    }

    public class FAQResponse : BaseResponse2
    {
        public FAQResponse()
        {
            data = new List<FAQ>();
        }
        public List<FAQ> data { get; set; }
    }

    public class MemberFaqResponse : BaseResponse2
    {
        public MemberFaqResponse()
        {
            data = new List<MemberFaqModel>();
        }
        public List<MemberFaqModel> data { get; set; }
    }

    public class ContactReasonResponse : BaseResponse2
    {
        public ContactReasonResponse()
        {
            data = new ContactReason();
        }
        public ContactReason data { get; set; }
    }

    public class TagResponse : BaseResponse2
    {
        public TagResponse()
        {
            data = new List<TagModel>();
        }
        public List<TagModel> data { get; set; }
    }

    public class GuestTagResponse : BaseResponse2
    {
        public GuestTagResponse()
        {
            data = new List<GuestTagModel>();
        }
        public List<GuestTagModel> data { get; set; }
    }

    public class UserNewslettersResponse : BaseResponse2
    {
        public UserNewslettersResponse()
        {
            data = new List<UserNewsletterModel>();
        }
        public List<UserNewsletterModel> data { get; set; }
    }

    public class WineryTypeResponse : BaseResponse2
    {
        public WineryTypeResponse()
        {
            data = new List<WineryTypesModel>();
        }
        public List<WineryTypesModel> data { get; set; }
    }
}
