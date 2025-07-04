

namespace CPReservationApi.WebApi.ViewModels
{
    public class CheckContactRequest : BaseRequest
    {
        /// <summary>
        /// Email of User (Required)
        /// </summary>
        public string email { get; set; }
    }
    public class CheckContactResponse : BaseResponse
    {

    }
    public class CreateContactRequest
    {
        public int member_id { get; set; }
        public string email { get; set; }
        public int rsvp_id { get; set; } = 0;
        public bool ignore_user_sync { get; set; } = false;
    }
    public class CreateContactResponse : BaseResponse
    {
        
    }
}
