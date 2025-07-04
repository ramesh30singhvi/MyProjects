using SHARP.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SHARP.BusinessLogic.DTO.User
{
    public class CreatePortalUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FacilityName { get; set; }
        public string Email { get; set; }

        public string Password { get; set; }

        public OptionDto Role { get; set; }

        public OptionDto Organization { get; set; }

        public bool FacilityUnlimited { get; set; }
        public IEnumerable<OptionDto> Facilities { get; set; }

        public string Position { get; set; }

    }
}
