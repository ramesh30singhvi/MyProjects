using System;
namespace SHARP.ViewModels.Report
{
	public class ReportFallModel
	{
		public int OrganizationID { get; set; }
        public int FacilityID { get; set; }
        public int Year { get; set; }
        public int[] Months { get; set; }
    }
}

