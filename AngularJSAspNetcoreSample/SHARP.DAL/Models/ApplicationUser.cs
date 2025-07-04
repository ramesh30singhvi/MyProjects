using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SHARP.DAL.Models
{
    [NotMapped]
    public class ApplicationUser : IdentityUser
    {
        public ICollection<ApplicationUserRole> UserRoles { get; set; }

        public User SharUser { get; set; }

        public TwoFAToken Token { get; set; }
    }
}
