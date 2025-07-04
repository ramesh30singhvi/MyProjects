using SHARP.Common.Enums;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class FormSettingDto
    {
        public int Id { get; set; }

        public FormSettingType SettingType { get; set; }

        public ScheduleSettingDto ScheduleSetting { get; set; }
    }
}
