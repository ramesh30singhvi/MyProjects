using System.Collections.Generic;

namespace SHARP.DAL.Models
{
    public class Facility
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int OrganizationId { get; set; }

        public bool Active { get; set; }

        public string State { get; set; }

        public string City { get; set; }

        public string AddressLine1 { get; set; }

        public int? BedCount { get; set; }

        public int? FacId { get; set; }

        public int? OrgId { get; set; }

        public string Phone { get; set; }

        public int? LegacyId { get; set; }

        public int TimeZoneId { get; set; }

        public string LegalName { get; set; }

        public Organization Organization { get; set; }

        public FacilityTimeZone TimeZone { get; set; }

        public ICollection<Patient> Patients { get; set; }

        public ICollection<Audit> Audits { get; set; }

        public ICollection<PortalReport> PortalReport { get; set; }

        public ICollection<FacilityRecipient> Recipients { get; set; }

        public ICollection<UserFacility> UserFacilities { get; set; }


    }
}
