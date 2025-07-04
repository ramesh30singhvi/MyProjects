using System.ComponentModel.DataAnnotations;

namespace SHARP.ViewModels.Form
{
    public class EditFormModel
    {
        [Required]
        public string FormName { get; set; }
    }
}
