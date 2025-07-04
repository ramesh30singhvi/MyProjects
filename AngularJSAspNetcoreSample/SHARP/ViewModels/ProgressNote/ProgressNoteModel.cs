using System;

namespace SHARP.ViewModels.ProgressNote
{
    public class ProgressNoteModel
    {
        public int Id { get; set; }

        public string Resident { get; set; }
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime EffectiveDate { get; set; }

        public string ProgressNoteType { get; set; }

        public string ProgressNoteText { get; set; }
    }
}
