using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SHARP.BusinessLogic.DTO.AuditorProductivityDashboard
{
    public class AuditorProductivitySummaryPerAuditorDto
    {
        public DateTime? StartTime { get; set; }

        public int? UserId { get; set; }

        public string UserFullName { get; set; }

        public string UserTimezone { get; set; }

        public bool? IsDST { get; set; }

        public int? FacilityId { get; set; }

        public string FacilityName { get; set; }

        public int? AuditTypeId { get; set; }

        public string TypeOfAudit { get; set; }

        public int? NoOfResidents { get;set; }

        public int? FinalAHT { get; set; }

        public int? NoOfFilteredAudits { get; set; }


        public int? NoOfFilteredAudits8to10 { get; set; }

        public int? UtilizedTime8to10 { get; set; }

        public int? NoOfFilteredAudits10to1 { get; set; }

        public int? UtilizedTime10to1 { get; set; }

        public int? NoOfFilteredAudits1to3 { get; set; }

        public int? UtilizedTime1to3 { get; set; }

        public int? NoOfFilteredAudits3to5 { get; set; }

        public int? UtilizedTime3to5 { get; set; }

        public int? NoOfFilteredAuditsOvertimeHours { get; set; }

        public int? UtilizedTimeOvertimeHours { get; set; }

        public int? TotalNoOfFilteredAudits { get; set; }

        public int? TotalFinalAHT { get; set; }

        public bool? OverTimeUsed { get; set; }

        public int?     TargetAHTPerResident { get; set; }

        public int? AHTPerResident { get; set; }
    }
}
