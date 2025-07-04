using SHARP.BusinessLogic.DTO.User;
using SHARP.BusinessLogic.DTO;
using System;

namespace SHARP.ViewModels.Portal
{
    public class PortalReportViewModal
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Name { get; set; }

        public OptionModel Organization { get; set; }

        public OptionModel Facility { get; set; }

        public UserOptionModel CreateByUser { get; set; }

        public OptionModel ReportRange { get; set; }

        public OptionModel ReportCategory { get; set; }

        public OptionModel ReportType { get; set; }

        public int AuditId { get; set; }
    }
}
