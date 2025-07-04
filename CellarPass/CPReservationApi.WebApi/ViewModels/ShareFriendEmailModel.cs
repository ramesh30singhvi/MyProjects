using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CPReservationApi.Common.Email;

namespace CPReservationApi.WebApi.ViewModels
{
    public class ShareFriendRequest
    {
        public int reservation_id { get; set; }
        public List<Common.ShareFriends> share_friends{ get; set; }
        public string share_message { get; set; }
        public bool send_copy_to_user { get; set; }
    }

    //public class ShareFriends
    //{
    //    public string share_friend_email { get; set; }
    //    public string share_friend_first_name { get; set; }
    //    public string share_friend_last_name { get; set; }
    //}

    public class ShareFriendResponse : BaseResponse
    {
        public ShareFriendResponse()
        {
            data = new EmailResponse();
        }
        public EmailResponse data { get; set; }
    }

}
