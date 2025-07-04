using System.ComponentModel.DataAnnotations;

namespace SHARP.Authentication
{
    public class FacilityAccessModel
    {
        [Required(ErrorMessage = "Facility Name is required")]
        public string FacilityName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public string Expired { get; set; }

        public string OrganizationName { get; set; }

        public int OrganizationID { get; set; }

        public int FacilityId { get; set; }
    }
}
