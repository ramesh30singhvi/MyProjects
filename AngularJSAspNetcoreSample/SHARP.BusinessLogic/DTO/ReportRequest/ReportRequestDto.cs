using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.DTO.User;
using SHARP.Common.Enums;
using System;

namespace SHARP.BusinessLogic.DTO.ReportRequest
{
    public class ReportRequestDto
    {
        public int Id { get; set; }

        public UserOptionDto User { get; set; }

        public string AuditType { get; set; }

        public OptionDto Organization { get; set; }

        public OptionDto Facility { get; set; }

        public FormOptionDto Form { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public DateTime RequestedTime { get; set; }

        public DateTime? GeneratedTime { get; set; }

        public string Report { get; set; }

        public string Exception { get; set; }

        public ReportRequestStatus Status { get; set; }
    }
}
