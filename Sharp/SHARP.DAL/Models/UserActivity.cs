using SHARP.Common.Enums;
using System;

namespace SHARP.DAL.Models
{
    public class UserActivity
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public int? AuditId { get; set; }

        public ActionType ActionType { get; set; }

        public DateTime ActionTime { get; set; }

        public string UserAgent { get; set; }

        public string IP { get; set; }

        public int? UpdatedUserId { get; set; }

        public string LoginUsername { get; set; }

        public User User { get; set; }

        public Audit Audit { get; set; }

        public User UpdatedUser { get; set; }

    }
}
