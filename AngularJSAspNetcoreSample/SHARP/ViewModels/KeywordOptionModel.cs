
using System.Collections.Generic;

namespace SHARP.ViewModels
{
    public class KeywordOptionModel : OptionModel
    {
        public bool? Trigger { get; set; }

        public ICollection<OptionModel> FormsTriggeredByKeyword { get; set; }
    }
}
