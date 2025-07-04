using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Portal
{
    public  class HighAlertStatisticDto
    {
        public int Total24Hours { get; set; }

        public int Closed24MHours { get; set; }

        public IReadOnlyCollection<HighAlertCategoryStatisticDto> HighAlertCategoryStatisticsFor24Hour { get; set; }

        public int Total48Hours { get; set; }

        public int Closed48MHours { get; set; }

        public IReadOnlyCollection<HighAlertCategoryStatisticDto> HighAlertCategoryStatisticsFor48Hour { get; set; }

    }
}
