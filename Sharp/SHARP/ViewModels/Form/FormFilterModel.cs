using SHARP.ViewModels.Base;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class FormFilterModel : BaseFilterModel
    {
        public IReadOnlyCollection<FilterOptionModel> Organization { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Name { get; set; }

        public IReadOnlyCollection<FilterOptionModel> AuditType { get; set; }

        public IReadOnlyCollection<FilterOptionModel> IsFormActive { get; set; }

        public IReadOnlyCollection<FilterOptionModel> SettingType { get; set; }

        public IReadOnlyCollection<FilterOptionModel> ScheduleSetting { get; set; }

        public int? OrganizationId { get; set; }
    }
}
