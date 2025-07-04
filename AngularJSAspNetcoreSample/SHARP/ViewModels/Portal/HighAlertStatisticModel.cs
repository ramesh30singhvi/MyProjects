using System.Collections.Generic;

namespace SHARP.ViewModels.Portal
{
    public class HighAlertStatisticModel
    {
        public int Total24Hours { get; set; }

        public int Closed24MHours { get; set;}

        public IReadOnlyCollection<HighAlertCategoryStatisticModel> HighAlertCategoryStatisticsFor24Hour { get; set; }

        public int Total48Hours { get; set; }

        public int Closed48MHours { get; set; }

        public IReadOnlyCollection<HighAlertCategoryStatisticModel> HighAlertCategoryStatisticsFor48Hour { get; set; }
    }
}
