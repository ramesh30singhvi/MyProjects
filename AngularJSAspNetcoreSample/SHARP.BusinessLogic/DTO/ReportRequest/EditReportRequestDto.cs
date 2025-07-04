using SHARP.Common.Enums;
using System;

namespace SHARP.BusinessLogic.DTO.ReportRequest
{
    public class EditReportRequestDto
    {
        public int Id { get; set; }

        public string Report { get; set; }

        public DateTime? GeneratedTime { get; set; }

        public string Exception { get; set; }

        public ReportRequestStatus Status { get; set; }
    }
}
