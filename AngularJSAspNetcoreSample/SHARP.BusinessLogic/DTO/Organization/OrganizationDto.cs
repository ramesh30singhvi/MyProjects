using System.Collections;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Organization
{
    public class OrganizationDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Unlimited { get; set; }

        public string OperatorName { get; set; }
        public string OperatorEmail { get; set; }

        public bool AttachPortalReport { get; set; }

        public IEnumerable<OrganizationPortalFeatureDto> PortalFeatures { get; set; }
    }
}
