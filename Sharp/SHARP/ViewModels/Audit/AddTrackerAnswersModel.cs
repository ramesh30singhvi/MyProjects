using System.Collections.Generic;

namespace SHARP.ViewModels.Audit
{
    public class AddEditTrackerAnswersModel
    {
        public IReadOnlyCollection<AddEditTrackerAnswerModel> Answers { get; set; }

        public OptionModel HighAlertCategory { get; set; }

        public string HighAlertDescription { get; set; }

        public string HighAlertNotes { get; set; }
    }
}
