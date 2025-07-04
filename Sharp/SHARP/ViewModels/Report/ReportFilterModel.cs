using SHARP.Common.Enums;
using SHARP.ViewModels.Base;
using System.Collections.Generic;

namespace SHARP.ViewModels.Report
{
    public class ReportFilterModel : BaseFilterModel
    {
        public IReadOnlyCollection<string> Name { get; set; } = new List<string>();
    }
}
