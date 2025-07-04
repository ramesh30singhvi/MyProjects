using SHARP.Common.Enums;

namespace SHARP.Common.Filtration
{
    public class NumberFilterModel
    {
        public Condition<double> FirstCondition { get; set; }

        public Condition<double> SecondCondition { get; set; }

        public Operator? Operator { get; set; }
    }
}
