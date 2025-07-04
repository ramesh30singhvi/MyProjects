using SHARP.Common.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SHARP.ViewModels.User
{
    public class CreatePortalUserModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public OptionModel Role { get; set; }

        public OptionModel Organization { get; set; }

        public bool FacilityUnlimited { get; set; }

        public IEnumerable<OptionModel> Facilities { get; set; }

        public string Position { get; set; }

    }
}
