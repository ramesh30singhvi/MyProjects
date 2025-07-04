using System;

namespace SHARP.ViewModels.ReportRequest
{
    public class ReportRequestGridModel
    {
        public int Id { get; set; }

        public string AuditType { get; set; }

        public string OrganizationName { get; set; }

        public string FacilityName { get; set; }

        public string FormName { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string UserFullName { get; set; }

        public DateTime RequestedTime { get; set; }

        public DateTime? GeneratedTime { get; set; }

        public string Report { get; set; }

        public int Status { get; set; }
    }
}
