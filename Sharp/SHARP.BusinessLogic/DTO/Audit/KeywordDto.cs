using System;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class KeywordDto
    {
        public int Id { get; set; }

        public string Resident { get; set; }

        public DateTime ProgressNoteDate { get; set; }

        public TimeSpan? ProgressNoteTime { get; set; }

        public string ProgressNoteDateTime { get; set; }

        public string Description { get; set; }

        public KeywordOptionDto Keyword { get; set; }

        public HighAlertValueDto HighAlertAuditValue { get; set; }
    }
 }
