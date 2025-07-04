using System;
namespace SHARP.Common.Filtration
{
	public class ReportFallFilterModel
	{
        public int OrganizationID { get; set; }
        public int FacilityID { get; set; }
        public int Year { get; set; }
        public int[] Months { get; set; }
    }
}

