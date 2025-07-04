using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class TrackerFormDetailsModel : FormDetailsModel
    {
        public IReadOnlyCollection<TrackerQuestionModel> Questions { get; set; }
    }
}
