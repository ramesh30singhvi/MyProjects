using SHARP.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
    public class AuditAIPatientPdfNotes
    {
        public int Id { get; set; }

        public int AuditAIReportV2Id { get; set; }

        public byte[] PdfNotes { get; set; }

        public string PatientName { get; set; }

        public string PatientId { get; set; }

        public string DateTime { get; set; }

        public AuditAIReportV2 Audit { get; set; }

        public ICollection<AuditAIKeywordSummary> Summaries { get; set; }

        public AuditAIPatientPdfNotes()
        {
            Summaries = new List<AuditAIKeywordSummary>();
        }

    }
}
