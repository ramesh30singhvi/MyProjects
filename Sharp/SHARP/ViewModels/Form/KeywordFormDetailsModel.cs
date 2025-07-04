using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class KeywordFormDetailsModel : FormDetailsModel
    {
        public IReadOnlyCollection<KeywordOptionModel> Keywords { get; set; }
    }
}
