using SHARP.Common.Enums;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class FormOrganizationDto
    {
        public int Id { get; set; }

        public FormDto Form { get; set; }

        public FormSettingType SettingType { get; set; }

        public ScheduleSettingDto ScheduleSetting { get; set; }
    }
}
