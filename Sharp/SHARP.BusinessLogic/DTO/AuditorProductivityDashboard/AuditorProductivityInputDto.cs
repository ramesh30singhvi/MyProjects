using SHARP.BusinessLogic.DTO.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.AuditorProductivityDashboard
{
    public class AuditorProductivityInputDto
    {
        public int Id { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? CompletionTime { get; set; }

        public int? UserId { get; set; }

        public string UserFullName { get; set; }

        public int? FacilityId { get; set; }

        public string FacilityName { get; set; }

        public int? AuditTypeId { get; set; }

        public string TypeOfAudit { get; set; }

        public int? NoOfResidents { get; set; }

        public int? NoOfFilteredAuditsAllType { get; set; }

        public string HandlingTime { get; set; }

        public string AHTPerAudit { get; set; }

        public string Hour { get; set; }

        public int? NoOfFilteredAudits { get; set; }

        public string FinalAHT { get; set; }

        public string Month { get; set; }

        public bool OverTimeUsed { get; set; }

        public int? TargetAHTPerResident { get;set; }


    }
}
