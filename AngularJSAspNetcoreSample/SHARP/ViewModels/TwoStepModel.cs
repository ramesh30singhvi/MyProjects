using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SHARP.ViewModels
{
    public class TwoStepModel
    {
        [Required]
        public string TwoFactorCode { get; set; }

        [Required]
        public string Username { get; set; }
        //public string Password { get; set; }
    }
}
