
using System.Collections.Generic;

namespace SHARP.ViewModels.Report
{
    public class AuditAIPatientPdfNotesViewModel
    {
        public int Id { get; set; }

        public int ReportId { get; set; }

      //  public string PatientNotes { get; set; }


        public string PatientName { get; set; }

        public string PatientId { get; set; }

        public string DateTime { get; set; }

        public ICollection<AuditAIKeywordSummaryViewModel> Summaries { get; set; }

    }
}
