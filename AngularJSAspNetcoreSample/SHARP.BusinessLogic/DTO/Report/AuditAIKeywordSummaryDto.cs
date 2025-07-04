using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public class AuditAIKeywordSummaryDto
    {
        public int Id { get; set; }

        public int AuditAIPatientPdfNotesID { get; set; }

        public string Keyword { get; set; }

        public string Summary { get; set; }

        public bool Accept { get; set; }
    }
}
