using System.ComponentModel.DataAnnotations;

namespace SHARP.ViewModels.User
{
    public class ResetPasswordModel
    {
        public string Token { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

    }
}
