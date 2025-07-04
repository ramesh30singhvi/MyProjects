using SHARP.Common.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SHARP.DAL.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string FullName { get; set; }
        public UserStatus Status { get; set; }
        public string TimeZone { get; set; }
        public int SiteId { get; set; }

        public string Position { get; set; }
        public ApplicationUser IdentityUser { get; set; }
        public ICollection<UserOrganization> UserOrganizations { get; set; }
        public ICollection<UserFacility> UserFacilities { get; set; }
        public ICollection<TrustedFEClient> TrustedFEClient { get; set; }
        public ICollection<Audit> Audits { get; set; }
        public ICollection<Audit> DeletedAudits { get; set; }
        public ICollection<AuditStatusHistory> AuditStatusHistory { get; set; }
        public ICollection<UserActivity> Activities { get; set; }
        public ICollection<LoginsTracking> LoginsTracking { get; set; }
        public ICollection<DownloadsTracking> DownloadsTracking { get; set; }
        public ICollection<UserTeam> UserTeams { get; set; }

    }
}
