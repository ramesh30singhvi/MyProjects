using SHARP.Common.Enums;
using System.Collections.Generic;

namespace SHARP.ViewModels.User
{
    public class UserModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public IEnumerable<string> Roles { get; set; }

        public OrganizationAccess Access { get; set; }

        public FacilityAccess FacilityAccess { get; set; }

        public UserStatus Status { get; set; }
    }

    public class OrganizationAccess
    {
        public bool Unlimited { get; set; }

        public IEnumerable<string> Organizations { get; set; }
    }

    public class FacilityAccess
    {
        public bool Unlimited { get; set; }

        public IEnumerable<string> Facilities { get; set; }
    }
}
