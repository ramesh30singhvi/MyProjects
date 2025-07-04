using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SHARP.DAL.Models.Views
{
    public class AuditorProductivityInputView
    {
        public int Id { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? CompletionTime { get; set; }

        public int? UserId { get; set; }

        public string UserFullName { get; set; }

        public string UserTimezone { get; set; }

        public int? FacilityId { get; set; }

        public string FacilityName { get; set; }

        public int? AuditTypeId { get; set; }

        public string TypeOfAudit { get; set; }

        public int? NoOfFilteredAuditsAllType { get; set; }

        public string HandlingTime { get; set; }

        public string AHTPerAudit { get; set; }

        public int? TargetAHTPerResident { get; set; }
        [NotMapped]
        public string Hour { get; set; }

        public int? NoOfFilteredAudits { get; set; }

        public string FinalAHT { get; set; }

        public string Month { get; set; }

        public int? NoOfResidents { get; set; }

        public User User { get; set; }

        public Facility Facility { get; set; }

        public Form AuditType { get; set; }
        [NotMapped]
        public bool OverTimeUsed { get; set; }
    }
}
