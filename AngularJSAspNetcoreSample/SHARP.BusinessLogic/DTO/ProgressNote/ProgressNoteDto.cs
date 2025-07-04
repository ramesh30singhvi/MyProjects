using System;

namespace SHARP.BusinessLogic.DTO.ProgressNote
{
    public class ProgressNoteDto
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
