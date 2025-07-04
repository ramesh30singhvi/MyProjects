using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public class AuditAIPatientPdfNotesDto
    {
        public int Id { get; set; }

        public int AuditAIReportV2Id { get; set; }

        public byte[] PdfNotes { get; set; }

        public string PatientName { get; set; }

        public string PatientId { get; set; }

        public string DateTime { get; set; }

        public ICollection<AuditAIKeywordSummaryDto> Summaries { get; set; }

        public AuditAIPatientPdfNotesDto()
        {
            Summaries = new List<AuditAIKeywordSummaryDto>();
        }
    }
}
