using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using System;

namespace SHARP.Extensions
{
    public static class FilterConditionExtensions
    {
        public static Condition<DateTime> SetDateCondition(this Condition<DateTime> condition)
        {
            if (condition == null)
            {
                return default;
            }

            switch (condition.Type)
            {
                case CompareType.Equals:
                case CompareType.NotEqual:
                    condition.To = condition.From.AddDays(1);
                    return condition;
                case CompareType.InRange:
                    condition.To = condition.To.Value.AddDays(1);
                    return condition;
                default:
                    return condition;
            }
        }

        public static Condition<double> SetNumberCondition(this Condition<double> condition)
        {
            if (condition == null)
            {
                return default;
            }

            switch (condition.Type)
            {
                case CompareType.Equals:
                case CompareType.NotEqual:
                    condition.To = (int)condition.From;
                    return condition;
                case CompareType.InRange:
                    condition.To = condition.To.Value;
                    return condition;
                default:
                    return condition;
            }
        }
    }
}
