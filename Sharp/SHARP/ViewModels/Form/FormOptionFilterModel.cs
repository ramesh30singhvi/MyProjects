using SHARP.ViewModels.Base;
using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class FormOptionFilterModel : BaseFilterModel
    {
        public IReadOnlyCollection<int> OrganizationIds { get; set; } = new List<int>();
    }
}
