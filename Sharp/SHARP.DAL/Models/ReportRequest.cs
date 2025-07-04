using SHARP.Common.Enums;
using System;

namespace SHARP.DAL.Models
{
    public class ReportRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string AuditType { get; set; }

        public int OrganizationId { get; set; }

        public int? FacilityId { get; set; }

        public int FormId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public DateTime RequestedTime { get; set; }

        public DateTime? GeneratedTime { get; set; }

        public string Report { get; set; }

        public string Exception { get; set; }

        public ReportRequestStatus Status { get; set; }

        public User User { get; set; }

        public Organization Organization { get; set; }

        public Facility Facility { get; set; }

        public Form Form { get; set; }
    }
}
