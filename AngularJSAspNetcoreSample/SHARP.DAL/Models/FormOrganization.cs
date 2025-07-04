using SHARP.Common.Enums;

namespace SHARP.DAL.Models
{
    public class FormOrganization
    {
        public int Id { get; set; }

        public int FormId { get; set; }

        public int OrganizationId { get; set; }

        public FormSettingType SettingType { get; set; }

        public Form Form { get; set; }

        public Organization Organization { get; set; }

        public FormScheduleSetting ScheduleSetting { get; set; }
    }
}
