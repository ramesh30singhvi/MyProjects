using System;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Report
{
	public class FallReportDto
	{
		public List<ByMonth> ByMonth { get; set; }
        public List<ByShift> ByShift { get; set; }
        public List<ByPlace> ByPlace { get; set; }
        public List<ByActivity> ByActivity { get; set; }

        public FallReportDto() {
            this.ByMonth = new List<ByMonth>();
            this.ByShift = new List<ByShift>();
            this.ByPlace = new List<ByPlace>();
            this.ByActivity = new List<ByActivity>();
        }
    }


	public class ByDay
	{
		public int Monday { get; set; }
        public int Tuesday { get; set; }
        public int Wednesday { get; set; }
        public int Thursday { get; set; }
        public int Friday { get; set; }
        public int Saturday { get; set; }
        public int Sunday { get; set; }

        public ByDay() {
            this.Monday = 0;
            this.Tuesday = 0;
            this.Wednesday = 0;
            this.Thursday = 0;
            this.Friday = 0;
            this.Saturday = 0;
            this.Sunday = 0;
        }
    }

    public class ByMonth
    {
        public string Name { get; set; }
        public int Total { get; set; }
        public int MajorInjury { get; set; }
        public int SentToHospital { get; set; }
        public ByDay ByDay { get; set; }

        public ByMonth() {
            this.Total = 0;
            this.MajorInjury = 0;
            this.SentToHospital = 0;
            this.ByDay = new ByDay();
        }
    }

    public class ByTime
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public ByTime() {
            this.Name = "";
            this.Count = 0;
        }
    }

    public class ByShift
    {
        public string Name { get; set; }
        public List<ByTime> ByTime { get; set; }

        public ByShift() {
            this.Name = "";
            this.ByTime = new List<ByTime>();
        }
    }

    public class ByPlace
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public ByPlace()
        {
            this.Name = "";
            this.Count = 0;
        }
    }

    public class ByActivity
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public ByActivity()
        {
            this.Name = "";
            this.Count = 0;
        }
    }
}

