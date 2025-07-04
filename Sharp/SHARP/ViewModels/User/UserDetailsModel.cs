using SHARP.BusinessLogic.DTO;
using SHARP.Common.Enums;
using SHARP.DAL.Enums;
using System.Collections.Generic;

namespace SHARP.ViewModels.User
{
    public class UserDetailsModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public TimeZoneModel TimeZone { get; set; }

        public IEnumerable<OptionModel> Roles { get; set; }

        public IEnumerable<OptionModel> Organizations { get; set; }

        public IEnumerable<OptionModel> Facilities { get; set; }

        public IEnumerable<OptionModel> Teams { get; set; }

        public UserStatus Status { get; set; }

        public string Position { get; set; }
    }
}
