using SHARP.Common.Enums;

namespace SHARP.Common.Filtration
{
    public class Condition<TValue> where TValue : struct
    {
        public TValue From { get; set; }

        public TValue? To { get; set; }

        public CompareType Type { get; set; }
    }
}
