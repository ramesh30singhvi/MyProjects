using System;

namespace SHARP.DAL.Models
{
    [Obsolete]
    public class FacilityAuditTemplate
    {
        public int Id { get; set; }

        public int FacilityId { get; set; }

        public int FormId { get; set; }

        public string Name { get; set; }

        public int? LegacyFacilityTemplateId { get; set; }

        public Facility Facility { get; set; }

        public Form Form { get; set; }
    }
}
