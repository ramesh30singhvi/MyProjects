using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
    public  class HighAlertAuditValue
    {
        public int Id { get; set; }

        public string ReportName { get; set; }

        public int AuditId { get; set; }

        public int ReportAiId { get; set; }
        public int AuditTableColumnValueId { get; set; }

        public int HighAlertCategoryId { get; set; }

        public string HighAlertDescription { get; set; }

        public string HighAlertNotes { get; set; }

        public Audit Audit { get; set; }
        public AuditTableColumnValue AuditTableColumnValue { get; set; }
        public HighAlertCategory    HighAlertCategory { get; set; }

        public int CreatedByUserId { get;set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<HighAlertStatusHistory> HighAlertStatusHistory { get; set; }


        public HighAlertAuditValue()
        {
            HighAlertStatusHistory = new List<HighAlertStatusHistory>();
        }
    }
}
