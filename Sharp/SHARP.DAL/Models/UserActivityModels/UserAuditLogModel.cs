using SHARP.Common.Enums;

namespace SHARP.DAL.Models.UserActivityModels
{
    public class UserAuditLogModel
    {
        public string SubmittedByUser {  get; set; }
        public int AuditId { get; set; }
        public string FormName { get; set; }
        public AuditStatus Status { get; set; }
        public string AuditorsTime { get; set; }
        public string Duration { get; set; }
    }
}
