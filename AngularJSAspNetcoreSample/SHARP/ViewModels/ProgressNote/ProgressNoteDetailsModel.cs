using System.Collections.Generic;

namespace SHARP.ViewModels.ProgressNote
{
    public class ProgressNoteDetailsModel
    {
        public int? KeywordsTotalCount { get; set; }

        public IReadOnlyCollection<ProgressNoteModel> ProgressNotes { get; set; }
        public int? TimeZoneOffset { get; internal set; }
    }
}
