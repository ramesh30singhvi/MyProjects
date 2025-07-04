using SHARP.Common.Enums;

namespace SHARP.DAL.Models
{
    public class FormScheduleSetting
    {
        public int FormOrganizationId { get; set; }

        public ScheduleType ScheduleType { get; set; }

        public string Days { get; set; }

        public FormOrganization FormOrganization { get; set; }
    }
}
