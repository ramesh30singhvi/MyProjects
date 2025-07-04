using SHARP.DAL.Models;
using System;

namespace SHARP.ViewModels.Audit
{
    public class KeywordModel
    {
        public int Id { get; set; }

        public KeywordOptionModel Keyword { get; set; }

        public HighAlertValueModel HighAlertAuditValue { get; set; }

        public string Resident { get; set; }

        public DateTime ProgressNoteDate { get; set; }

        public string ProgressNoteTime { get; set; }

        public string ProgressNoteDateTime { get; set; }

        public string Description { get; set; }

        
    }
}
