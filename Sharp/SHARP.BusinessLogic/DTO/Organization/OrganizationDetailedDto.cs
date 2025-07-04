using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Organization
{
    public class OrganizationDetailedDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string OperatorName { get; set; }
        public string OperatorEmail { get; set; }

        public bool AttachPortalReport { get; set; }
        public int FacilityCount { get; set; }
        public IEnumerable<RecipientDto> Recipients { get; set; }

        public IEnumerable<OrganizationPortalFeatureDto> PortalFeatures { get; set; }
    }
}
