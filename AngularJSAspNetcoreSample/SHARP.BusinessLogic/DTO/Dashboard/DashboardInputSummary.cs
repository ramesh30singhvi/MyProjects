using System;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Dashboard
{
	public class DashboardInputSummary
	{
		public String Auditor { get; set; }
		public DashboardInputSummaryShift[] DashboardInputSummaryShift { get; set; }
    }

	public class DashboardInputSummaryShift
	{
		public String Name { get; set; }
		public List<string> FormNames { get; set; }
		public int Total { get; set; }
	}
}

