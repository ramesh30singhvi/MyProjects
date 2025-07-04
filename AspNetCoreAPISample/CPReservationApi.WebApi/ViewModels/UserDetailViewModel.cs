using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class UserDetailResponse : BaseResponse
    {
        public UserDetailResponse()
        {
            data = new UserDetailModel();
        }
        public UserDetailModel data { get; set; }
    }

    public class UserByUserNameResponse : BaseResponse
    {
        public UserByUserNameResponse()
        {
            data = new User2Model();
        }
        public User2Model data { get; set; }
    }

    public class UserExternalDetailResponse : BaseResponse
    {
        public UserExternalDetailResponse()
        {
            data = new UserExternalDetailModel();
        }
        public UserExternalDetailModel data { get; set; }
    }

    public class UserExternalDetailModel
    {
        public decimal ltv { get; set; }
        public DateTime? last_order_date { get; set; }
        public bool member_status { get; set; }
        public int customer_type { get; set; }
        public List<string> contact_types { get; set; } = new List<string>();
    }

    public class UserListResponse : BaseResponse
    {
        public UserListResponse()
        {
            data = new List<UserDetailModel>();
        }
        public List<UserDetailModel> data { get; set; }
    }

    public class Commerce7OrderSourceResponse : BaseResponse
    {
        public Commerce7OrderSourceResponse()
        {
            data = new Commerce7OrderSourceModel();
        }
        public Commerce7OrderSourceModel data { get; set; }
    }

    public class Commerce7OrderSourceModel
    {
        public List<string> name { get; set; } = new List<string>();
    }

    public class eWineryCustomerResponse : BaseResponse
    {
        public eWineryCustomerResponse()
        {
            data = new List<eWineryCustomerViewModel>();
        }
        public List<eWineryCustomerViewModel> data { get; set; }
    }

    public class ClubTypesResponse : BaseResponse
    {
        public ClubTypesResponse()
        {
            data = new List<AccountType>();
        }
        public List<AccountType> data { get; set; }
    }

    public class UserOrderPortResponse : BaseResponse
    {
        public UserOrderPortResponse()
        {
            data = new UserOrderPortModel();
        }
        public UserOrderPortModel data { get; set; }
    }

    public class UserOrderPortModel
    {
        public string id { get; set; }
    }

    public class ReservationsReserveCloudResponse : BaseResponse
    {
        public ReservationsReserveCloudResponse()
        {
            data = new List<ReserveCloudReservation>();
        }
        public List<ReserveCloudReservation> data { get; set; }
    }

    public class CheckAndFormatPhoneNumberResponse : BaseResponse
    {
        public CheckAndFormatPhoneNumberResponse()
        {
            data = new CheckAndFormatPhoneNumberModel();
        }
        public CheckAndFormatPhoneNumberModel data { get; set; }
    }

    public class CheckAndFormatPhoneNumberModel
    {
        public string formated_mobile_number { get; set; }
        public Common.MobileNumberStatus mobile_number_status { get; set; } = Common.MobileNumberStatus.failed;
    }

    public class MyAccountDetailsResponse : BaseResponse2
    {
        public MyAccountDetailsResponse()
        {
            data = new MyAccountDetailsV2Model();
        }
        public MyAccountDetailsV2Model data { get; set; }
    }

    public class MyAccountDataResponse : BaseResponse2
    {
        public MyAccountDataResponse()
        {
            data = new MyAccountDataViewModel();
        }
        public MyAccountDataViewModel data { get; set; }
    }
}
