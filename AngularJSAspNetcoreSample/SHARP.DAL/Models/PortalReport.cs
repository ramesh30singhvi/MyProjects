using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
	public class PortalReport
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public int ReportTypeId { get; set; }

		public ReportType ReportType { get; set; }
		public int ReportCategoryId { get; set; }
		public ReportCategory ReportCategory { get; set; }


        public Organization Organization { get; set; }
        public int OrganizationId { get; set; }
        public Facility Facility { get; set; }
        public int FacilityId { get; set; }

        public Audit Audit { get; set; }
        public int? AuditId { get; set; }

        public int? AuditTypeId { get; set; }

		public AuditType AuditType { get; set; }

        public string StorageContainerName { get; set; }

		public string StorageReportName { get; set; }

		public string StorageURL { get; set; }

		public DateTime CreatedAt { get; set; }

		public int CreatedByUserID { get; set; }

		public User User { get; set; }

		public int? ReportRequestId { get; set; }

        public ICollection<DownloadsTracking> DownloadsTracking { get; set; }

    }

}
