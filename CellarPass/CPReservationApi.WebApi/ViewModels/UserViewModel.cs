using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class UserRequest
    {
        /// <summary>
        /// Email of User (Required)
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// Password of User (Required)
        /// </summary>
        public string password { get; set; }
        /// <summary>
        /// First Name of User (Required)
        /// </summary>
        public string first_name { get; set; }
        /// <summary>
        /// Last Name of User (Required)
        /// </summary>
        public string last_name { get; set; }
        /// <summary>
        /// Country of User (Required)
        /// </summary>
        public string country { get; set; }
        /// <summary>
        /// Zip of User (Required)
        /// </summary>
        public string zip { get; set; }
        /// <summary>
        /// Home Phone of User (Required)
        /// </summary>
        public string home_phone { get; set; }
        /// <summary>
        /// Id of member (Required)
        /// </summary>
        public int member_id { get; set; }
        
        public string address1 { get; set; }
       
        public string address2 { get; set; }
       
        public string city { get; set; }
        
        public string state { get; set; }
        public int concierge_type { get; set; }
    }

    public class User
    {
        public User()
        {
            roles = new List<Role>();
        }
        public int user_id { get; set; }
        public string user_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string title { get; set; }
        public List<Role> roles { get; set; }
        public Address address { get; set; }
    }

    public class UserResponse : BaseResponse2
    {
        public int user_id { get; set; }
    }

    public class UpdateAccountResponse : BaseResponse2
    {
        public string email { get; set; }
    }

    public class Role
    {
        public int id { get; set; }
        public string role_name { get; set; }
    }
    public class UsermemberRequest
    {
        public UsermemberRequest()
        {
            role_id = new List<int>();
        }
        /// <summary>
        /// Id of member (Required)
        /// </summary>
        public int member_id { get; set; }
        /// <summary>
        /// Role_Id's list of Users (Required)
        /// </summary>
        public List<int> role_id { get; set; }
    }
    public class UsermemberResponse : BaseResponse2
    {
        public UsermemberResponse()
        {
            data = new List<UserDetailModel>();
        }
        public List<UserDetailModel> data { get; set; }
    }

    public class UsersbyAffiliateIdResponse : BaseResponse2
    {
        public UsersbyAffiliateIdResponse()
        {
            data = new List<UserDetail2Model>();
        }
        public List<UserDetail2Model> data { get; set; }
    }

    public class ConciergeUserResponse : BaseResponse2
    {
        public ConciergeUserResponse()
        {
            data = new List<ConciergeUserDetailModel>();
        }
        public List<ConciergeUserDetailModel> data { get; set; }
    }

    public class UserdetailResponse : BaseResponse2
    {
        public UserdetailResponse()
        {
            data = new UserDetail2Model();
        }
        public UserDetail2Model data { get; set; }
    }

    public class GuestmemberResponse : BaseResponse2
    {
        public GuestmemberResponse()
        {
            data = new List<GuestDetailModel>();
        }
        public List<GuestDetailModel> data { get; set; }
    }
    public class Address
    {
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip_code { get; set; }
        public string country { get; set; }
        public string home_phone { get; set; }
        public string work_phone { get; set; }
        public string cell_phone { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string user_name { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string reset_code { get; set; }
        public string new_password { get; set; }
    }

    public class ForgotPasswordResponse : BaseResponse2
    {
        public ForgotPasswordResponse()
        {
            data = new ForgotPasswordModel();
        }
        public ForgotPasswordModel data { get; set; }
    }

    public class ForgotPasswordModel
    {
        public string user_name { get; set; }
    }

    public class DisableUserRequest
    {
        public int user_id { get; set; }
        public string password { get; set; }
    }

    public class DisableUserResponse : BaseResponse2
    {
        
    }

    public class UserFavoriteRequestModel
    {
        public int member_id { get; set; } = 0;
        public int user_id { get; set; } = 0;
    }

    public class UserFavoriteEventRequestModel
    {
        public int user_id { get; set; } = 0;
        public int event_id { get; set; } = 0;
    }

    public class SetUserFavoritesResponse : BaseResponse2
    {

    }

    public class favoritememberResponse : BaseResponse2
    {
        public favoritememberResponse()
        {
            data = new List<UserFavoriteMemberViewModel>();
        }
        public List<UserFavoriteMemberViewModel> data { get; set; }
    }

    public class favoriteeventsResponse : BaseResponse2
    {
        public favoriteeventsResponse()
        {
            data = new List<FavoriteEventViewModel>();
        }
        public List<FavoriteEventViewModel> data { get; set; }
    }

    
}

