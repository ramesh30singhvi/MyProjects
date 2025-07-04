using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Facility
{
    public class AddFacilityDto
    {
        public string Name { get; set; }

        public int? OrganizationId { get; set; }

        public int? TimeZoneId { get; set; }

        public IReadOnlyCollection<FacilityRecipientDto> Recipients { get; set; }

        public string LegalName { get; set; }

        public bool IsActive { get; set; }
    }
}
