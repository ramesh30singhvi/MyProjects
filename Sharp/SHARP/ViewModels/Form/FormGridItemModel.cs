using SHARP.Common.Enums;

namespace SHARP.ViewModels.Form
{
    public class FormGridItemModel
    {
        public int FormOrganizationId { get; set; }

        public int FormId { get; set; }

        public string Name { get; set; }

        public string AuditType { get; set; }

        public FormSettingType SettingType { get; set; }

        public ScheduleSettingModel ScheduleSetting { get; set; }

        public bool IsFormActive { get; set; }
    }
}
