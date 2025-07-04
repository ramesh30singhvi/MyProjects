using System.ComponentModel.DataAnnotations;

namespace SHARP.Authentication
{
    public class ClientPortalLoginModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FacilityName { get; set; }
        public int Organization { get; set; }
        public string EmailToken { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email {  get; set; }

    }
}
