using SHARP.Common.Enums;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class ScheduleSettingDto
    {
        public int Id { get; set; }

        public ScheduleType ScheduleType { get; set; }

        public IReadOnlyCollection<int> Days { get; set; }
    }
}
