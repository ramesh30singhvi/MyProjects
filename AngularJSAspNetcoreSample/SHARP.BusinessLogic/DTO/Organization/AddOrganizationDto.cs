using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Organization
{
    public class AddOrganizationDto
    {
        public string Name { get; set; }

        public string OperatorName { get; set; }
        public string OperatorEmail { get; set; }

        public bool AttachPortalReport { get; set; }
        public IEnumerable<string> Recipients { get; set; }
    }
}
