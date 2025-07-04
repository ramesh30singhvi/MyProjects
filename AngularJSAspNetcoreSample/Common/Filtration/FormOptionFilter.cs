using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class FormOptionFilter : FilterModel
    {
        public IReadOnlyCollection<int> OrganizationIds { get; set; }
    }
}
