using SHARP.Common.Enums;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.User
{
    public class UserDto
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public IEnumerable<string> Roles { get; set; }

        public bool Unlimited { get; set; }

        public bool FacilityUnlimited { get; set; }

        public IEnumerable<string> Organizations { get; set; }

        public IEnumerable<string> Facilities { get; set; }

        public UserStatus Status { get; set; }

        public int SiteId { get; set; }
    }
}
