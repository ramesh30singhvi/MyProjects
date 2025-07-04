using System;
using System.Collections.Generic;
using static SHARP.BusinessLogic.DTO.Report.WoundReportDto.ByMonth;

namespace SHARP.BusinessLogic.DTO.Report
{
	public class WoundReportDto
	{
        public List<ByMonth> byMonths { get; set; }

        public WoundReportDto()
        {
            this.byMonths = new List<ByMonth>();
        }

        public class ByMonth
        {
            public string Name { get; set; }
            public int Total { get; set; }
            public int InHouseAcquired { get; set; }
            public int ReHospitalization { get; set; }
            public List<ByType> byTypes { get; set; }

            public ByMonth()
            {
                this.Total = 0;
                this.InHouseAcquired = 0;
                this.ReHospitalization = 0;
                this.byTypes = new List<ByType>();
            }

            public class ByType
            {
                public string Name { get; set; }
                public int Count { get; set; }

                public ByType()
                {
                    this.Count = 0;
                }
            }
        }
    }


}

