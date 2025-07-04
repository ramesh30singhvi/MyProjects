using System;

namespace SHARP.BusinessLogic.DTO.AuditorProductivityDashboard
{
    public class AuditorProductivityAHTPerAuditTypeDto
    {
        public DateTime? StartTime { get; set; }

        public int? UserId { get; set; }

        public string UserFullName { get; set; }

        public int? FacilityId { get; set; }

        public string FacilityName { get; set; }

        public int? AuditTypeId { get; set; }

        public string TypeOfAudit { get; set; }

        public int? NoOfResidents { get; set; }

        public int? FinalAHT { get; set; }

        public int? NoOfFilteredAudits { get; set; }


        public bool? OverTimeUsed { get; set; }

        public int? TargetAHTPerResident { get; set; }

        public int? AHTPerResident { get; set; }
    }
}
