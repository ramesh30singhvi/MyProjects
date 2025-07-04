using SHARP.Common.Enums;
using System;

namespace SHARP.DAL.Models
{
    public class GetUserActivities
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public int? AuditId { get; set; }

        public ActionType ActionType { get; set; }

        public DateTime ActionTime { get; set; }

        public string UserAgent { get; set; }

        public string IP { get; set; }

        public string UserName { get; set; }

        public string AuditType { get; set; }

        public string AuditName { get; set; }
    }
}
