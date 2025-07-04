using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Portal
{
    public  class HighAlertCategoryStatisticDto
    {
        public OptionDto HighAlertCategory { get; set; }

        public int Percent { get; set; }

        public int CountUnClosedHighAlert { get; set; }
    }
}
