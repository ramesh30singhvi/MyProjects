using SHARP.ViewModels.Organization;
using System.Collections.Generic;

namespace SHARP.ViewModels.Facility
{
    public class FacilityDetailsModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string LegalName { get; set; }

        public OptionModel Organization { get; set; }

        public TimeZoneOptionModel TimeZone { get; set; }

        public IReadOnlyCollection<FacilityRecipientModel> Recipients { get; set; }

        public IReadOnlyCollection<OrganizationPortalFeatureModel> PortalFeatures { get; set; }

        public bool IsActive { get; set; }
    }
}
