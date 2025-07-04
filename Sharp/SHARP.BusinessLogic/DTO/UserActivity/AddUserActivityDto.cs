using SHARP.Common.Enums;

namespace SHARP.BusinessLogic.DTO.UserActivity
{
    public class AddUserActivityDto
    {
        public int UserId { get; set; }

        public int? AuditId { get; set; }

        public ActionType ActionType { get; set; }

        public string UserAgent { get; set; }

        public string IP { get; set; }

        public int? UpdatedUserId { get; set; }

        public string LoginUsername { get; set; }
    }
}
