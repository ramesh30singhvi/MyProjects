using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SHARP.DAL.Models.Views
{
    public class AuditorProductivitySummaryPerAuditorView
    {
        public DateTime? StartTime { get; set; }

        public int? UserId { get; set; }

        public string UserFullName { get; set; }

        public int? FacilityId { get; set; }

        public string FacilityName { get; set; }

        public int? AuditTypeId { get; set; }

        public string TypeOfAudit { get; set; }

        public int? FinalAHT { get; set; }

        public int? NoOfFilteredAudits { get; set; }

        public int? NoOfResidents { get; set; }


        public string UserTimezone { get; set; }
        [NotMapped]
        public User User { get; set; }
        [NotMapped]
        public Facility Facility { get; set; }
        [NotMapped]
        public Form AuditType { get; set; }
        public int? TargetAHTPerResident { get;  set; }

        [NotMapped]
        public bool OverTimeUsed { get; set; }

        [NotMapped]
        public int? AHTPerResident { get; set; }
    }
}
