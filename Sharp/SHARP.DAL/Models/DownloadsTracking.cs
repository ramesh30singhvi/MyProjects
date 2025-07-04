using System;
using System.Collections.Generic;

namespace SHARP.DAL.Models
{
    public class DownloadsTracking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PortalReportId { get; set; }
        public int? NumberOfDownloads { get; set; }
        public DateTime? DateAndTime { get; set; }

        public DateTime? PortalReportCreatedAt { get; set; }

        public User User { get; set; }
        public PortalReport PortalReport { get; set; }
        public ICollection<DownloadsTrackingDetails>  DownloadsTrackingDetails { get; set; }
    }
}
