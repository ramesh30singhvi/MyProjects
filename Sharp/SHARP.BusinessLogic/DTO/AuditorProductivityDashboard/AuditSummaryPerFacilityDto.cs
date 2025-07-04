using SHARP.BusinessLogic.DTO.Facility;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.AuditorProductivityDashboard
{
    public class AuditSummaryPerFacilityDto
    {
        public FacilityOptionDto Facility { get; set; }

        public IReadOnlyCollection<Tuple<string,int>> FormProductivityResult { get; set; }

        public int TotalCount { get; set; }
    }
}
