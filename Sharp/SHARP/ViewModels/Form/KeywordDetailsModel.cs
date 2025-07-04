using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class KeywordDetailsModel : FormDetailsModel
    {
        public IReadOnlyCollection<OptionModel> Keywords { get; set; }
    }
}
