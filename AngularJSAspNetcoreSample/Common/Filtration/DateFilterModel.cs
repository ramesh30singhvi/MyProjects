using SHARP.Common.Enums;
using System;

namespace SHARP.Common.Filtration
{
    public class DateFilterModel
    {
        public Condition<DateTime> FirstCondition { get; set; }

        public Condition<DateTime> SecondCondition { get; set; }

        public Operator? Operator { get; set; }
    }
}
