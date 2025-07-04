using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models.UserActivityModels
{
    public class UserActivitiesModel
    {
        public List<UserActivity> UserActivity { get; set; }
        public List<User> User { get; set; }
        public List<UserAuditLogModel> UserAuditLog { get; set; }
        public List<UserSummaryModel> UserSummary { get; set; }
    }
}
