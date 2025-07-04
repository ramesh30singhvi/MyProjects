using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class HighAlertValueDto
    {
        public int Id { get; set; }

        public string ReportName { get; set; }
        public OptionDto HighAlertCategory { get; set; }

        public string[] HighAlertCategoryPotentialAreas { get; set; }

        public string HighAlertDescription { get; set; }

        public string HighAlertNotes { get; set; }

        public OptionDto HighAlertStatus { get; set; }

        public int ReportTypeId { get; set; }

        public int CreatedByUserId { get; set; }

        public double? TotalCompliance { get; set; }

        public DateTime CreatedAt { get; set; }

        public AuditDto Audit { get; set;}

        public string ChangedBy { get; set; }

        public string UserNotes { get; set; }

    }
}
