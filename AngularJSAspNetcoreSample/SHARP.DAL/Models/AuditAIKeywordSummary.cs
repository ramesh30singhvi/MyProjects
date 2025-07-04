using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
    public class AuditAIKeywordSummary
    {
        public int Id { get; set; }

        public int AuditAIPatientPdfNotesID { get; set; }

        public string Keyword { get; set; }

        public string Summary { get; set; }

        public bool Accept { get; set; }

    }
}
