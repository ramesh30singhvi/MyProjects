using SHARP.ViewModels.Base;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class FormVersionFilterModel : BaseFilterModel
    {
        public IReadOnlyCollection<FilterOptionModel> Name { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Organizations { get; set; }

        public IReadOnlyCollection<FilterOptionModel> AuditType { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Status { get; set; }

        public string CreatedDate { get; set; }
    }
}
