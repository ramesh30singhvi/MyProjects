using System;

namespace SHARP.ViewModels.Audit
{
    public class AddKeywordModel
    {
        public int AuditId { get; set; }

        public int FormVersionId { get; set; }

        public int KeywordId { get; set; }

        public string Resident { get; set; }

        public string? CustomKeyword { get; set; }

        public DateTime? ProgressNoteDate { get; set; }

        public string ProgressNoteTime { get; set; }

        public string Description { get; set; }

        public OptionModel HighAlertCategory { get; set; }

        public string HighAlertDescription { get; set; }

        public string HighAlertNotes { get; set;}
    }
}
