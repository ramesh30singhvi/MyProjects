using System.Collections.Generic;

namespace SHARP.ViewModels.Organization
{
    public class OrganizationDetailedModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string OperatorName { get; set; }
        public string OperatorEmail { get; set; }

        public bool AttachPortalReport { get; set; }
        public int FacilityCount { get; set; }

        public IEnumerable<RecipientModel> Recipients { get; set; }

        public IEnumerable<OrganizationPortalFeatureModel> PortalFeatures { get; set; }

    }
}
