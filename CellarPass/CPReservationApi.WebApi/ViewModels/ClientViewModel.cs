using CPReservationApi.Model;

namespace CPReservationApi.WebApi.ViewModels
{
    public class ClientResponse : BaseResponse2
    {
        public ClientResponse()
        {
            data = new ClientModel();
        }
        public ClientModel data { get; set; }
    }

    public class LoginResponse : BaseResponse2
    {
        public LoginResponse()
        {
            data = new UserSessionModel();
        }
        public UserSessionModel data { get; set; }
    }

    public class LoginRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}
