using SHARP.BusinessLogic.DTO.Organization;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public class SendReportDto
    {
        public OptionDto Facility { get; set; }

        public OrganizationDto Organization { get; set; }

        public long ExpiredTime { get; set; }
        public IReadOnlyCollection<PortalReportDto> Reports { get; set; }
    }
}
