using SHARP.Common.Enums;

namespace SHARP.ViewModels.Form
{
    public class FormSettingModel
    {
        public int Id { get; set; }

        public FormSettingType SettingType { get; set; }

        public ScheduleSettingModel ScheduleSetting { get; set; }
    }
}
