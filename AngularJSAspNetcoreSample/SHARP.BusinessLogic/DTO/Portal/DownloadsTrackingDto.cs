using SHARP.DAL.Models;
using System;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Portal
{
    public class DownloadsTrackingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public DateTime? ReportDate { get; set; }
        public int? NumberOfDownloads { get; set; }
        public DateTime? DateAndTime { get; set; }
        public ICollection<DownloadsTrackingDetails> DownloadsTrackingDetails { get; set; }
    }
}
