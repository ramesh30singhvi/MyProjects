using SHARP.Common.Enums;

namespace SHARP.ViewModels.User
{
    public class AddUserActivityModel
    {
        public ActionType ActionType { get; set; }

        public int? AuditId { get; set; }

        public int? UpdatedUserId { get; set; }

        public string LoginUsername { get; set; }
    }
}
