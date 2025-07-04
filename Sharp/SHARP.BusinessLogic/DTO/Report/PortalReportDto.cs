using SHARP.BusinessLogic.DTO.User;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public class PortalReportDto
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Name { get; set; }

        public OptionDto Organization { get; set; }

        public OptionDto Facility {  get; set; }

        public UserOptionDto CreateByUser { get; set; }

        public OptionDto ReportRange { get; set; }

        public OptionDto ReportCategory { get; set; }

        public OptionDto ReportType { get; set; }

        public int AuditId { get; set; }

        public OptionDto AuditType { get; set; }

        public double? TotalCompliance { get; set; }

        public int? ReportRequestId { get; set; }

        public string StorageContainerName { get; set; }

        public string StorageReportName { get; set; }

        public string StorageURL { get; set; }
    }
}
