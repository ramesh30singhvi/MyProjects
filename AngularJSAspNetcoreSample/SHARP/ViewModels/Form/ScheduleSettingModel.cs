using SHARP.Common.Enums;
using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class ScheduleSettingModel
    {
        public int Id { get; set; }

        public ScheduleType ScheduleType { get; set; }

        public IReadOnlyCollection<int> Days { get; set; }
    }
}
