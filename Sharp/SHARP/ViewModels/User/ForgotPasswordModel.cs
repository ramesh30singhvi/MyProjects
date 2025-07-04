using System.ComponentModel.DataAnnotations;

namespace SHARP.ViewModels.User
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
     
    }
}
