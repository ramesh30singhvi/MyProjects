using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class AddFormKeywordModel
    {
        public int FormVersionId { get; set; }

        public string Name { get; set; }

        public int? Hidden { get; set; }

        public bool? Trigger { get; set; }

       public IReadOnlyCollection<OptionModel> FormsTriggeredByKeyword { get; set; }
    }
}
