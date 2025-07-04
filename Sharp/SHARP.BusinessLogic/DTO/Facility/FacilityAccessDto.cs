using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Facility
{
    public  class FacilityAccessDto
    {
        public string FacilityName { get; set; }

        public string Password { get; set; }

        public string Expired { get; set; }

        public int OrganizationId { get; set; }

        public int FacilityId { get; set; }
    }
}
