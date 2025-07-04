using SHARP.Common.Enums;
using System.Collections.Generic;

namespace SHARP.ViewModels.User
{
    public class CreateUserModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string TimeZone { get; set; }

        public IEnumerable<int> Roles { get; set; }

        public IEnumerable<int> Organizations { get; set; }

        public IEnumerable<int> Facilities { get; set; }

        public IEnumerable<int> Teams { get; set; }

        public bool Unlimited { get; set; }

        public bool FacilityUnlimited { get; set; }

        public UserStatus Status { get; set; }

        public string Position { get; set; }
    }
}
