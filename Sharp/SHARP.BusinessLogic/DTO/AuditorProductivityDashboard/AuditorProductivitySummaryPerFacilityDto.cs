using SHARP.BusinessLogic.DTO.Facility;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.AuditorProductivityDashboard
{
    public  class AuditorProductivitySummaryPerFacilityDto
    {
        public IReadOnlyCollection<AuditSummaryPerFacilityDto> SummaryPerFacilities {  get; set; }
    }
}
