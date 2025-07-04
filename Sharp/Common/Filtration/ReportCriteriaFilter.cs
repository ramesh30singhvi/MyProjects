using System;
namespace SHARP.Common.Filtration
{
	public class ReportCriteriaFilter
	{
        public int OrganizationID { get; set; }
        public int[] FacilityIDs { get; set; }
        public string FromAuditDate { get; set; }
        public string ToAuditDate { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int CompliantType { get; set; }
        public int[] FormVersionIds { get; set; }
        public int[] QuestionsIds { get; set; }
    }
}

