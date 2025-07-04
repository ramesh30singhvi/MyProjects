using System.ComponentModel.DataAnnotations;

namespace SHARP.ViewModels.User
{
    public class InvitationPortalUserModel
    {
        public int UserId { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string UserEmail { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
