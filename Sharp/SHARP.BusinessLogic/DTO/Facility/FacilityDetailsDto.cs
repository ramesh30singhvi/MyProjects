using SHARP.BusinessLogic.DTO.Organization;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Facility
{
    public class FacilityDetailsDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string LegalName { get; set; }

        public OptionDto Organization { get; set; }

        public TimeZoneOptionDto TimeZone { get; set; }

        public IReadOnlyCollection<FacilityRecipientDto> Recipients { get; set; }

        public IReadOnlyCollection<OrganizationPortalFeatureDto> PortalFeatures { get; set; }

        public bool IsActive { get; set; }
    }
}
