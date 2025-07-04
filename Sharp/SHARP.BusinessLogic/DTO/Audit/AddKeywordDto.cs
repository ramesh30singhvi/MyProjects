using System;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class AddKeywordDto
    {
        public int AuditId { get; set; }

        public int KeywordId { get; set; }

        public string Resident { get; set; }

        public DateTime ProgressNoteDate { get; set; }

        public TimeSpan? ProgressNoteTime { get; set; }

        public string Description { get; set; }

        public OptionDto HighAlertCategory { get; set; }

        public string HighAlertDescription { get; set; }

        public string HighAlertNotes { get; set; }
    }
}
