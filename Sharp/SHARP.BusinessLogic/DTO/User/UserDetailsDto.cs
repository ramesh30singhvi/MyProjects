using SHARP.Common.Enums;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.User
{
    public class UserDetailsDto
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string TimeZone { get; set; }

        public IEnumerable<OptionDto> Roles { get; set; }

        public IEnumerable<OptionDto> Organizations { get; set; }

        public IEnumerable<OptionDto> Facilities { get; set; }

        public IEnumerable<OptionDto> Teams { get; set; }
        public UserStatus Status { get; set; }

        public string Position { get; set; }
    }
}
