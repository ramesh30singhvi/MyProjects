using System.Collections.Generic;

namespace SHARP.ViewModels.Facility
{
    public class AddFacilityModel
    {
        public string Name { get; set; }

        public int? OrganizationId { get; set; }

        public int? TimeZoneId { get; set; }

        public IReadOnlyCollection<FacilityRecipientModel> Recipients { get; set; }

        public string LegalName { get; set; }

        public bool IsActive { get; set; }
    }
}
