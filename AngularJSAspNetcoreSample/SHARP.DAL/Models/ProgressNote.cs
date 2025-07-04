using System;

namespace SHARP.DAL.Models
{
    public class ProgressNote
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public string Completed { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime EffectiveDate { get; set; }

        public int? FacId { get; set; }

        public string OrgUuid { get; set; }

        public int? ProgressNoteId { get; set; }

        public string ProgressNoteType { get; set; }

        public string SummaryText { get; set; }

        public string TextSection { get; set; }

        public string ProgressNoteText { get; set; }

        public Patient Patient { get; set; }
    }
}
