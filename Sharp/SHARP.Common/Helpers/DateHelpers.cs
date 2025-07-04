using SHARP.Common.Filtration;
using System;

namespace SHARP.Common.Helpers
{
    public static class DateHelpers
    {
        public static DateFilterModel ConvertToUtcDate(DateFilterModel dateFilterModel, string userTimeZone)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);

            if (dateFilterModel.FirstCondition != null)
            {
                dateFilterModel.FirstCondition.From = TimeZoneInfo.ConvertTimeToUtc(dateFilterModel.FirstCondition.From, timeZoneInfo);
            }

            if (dateFilterModel.FirstCondition.To.HasValue)
            {
                dateFilterModel.FirstCondition.To = TimeZoneInfo.ConvertTimeToUtc(dateFilterModel.FirstCondition.To.Value, timeZoneInfo);
            }

            if (dateFilterModel.SecondCondition != null)
            {
                dateFilterModel.SecondCondition.From = TimeZoneInfo.ConvertTimeToUtc(dateFilterModel.SecondCondition.From, timeZoneInfo);
            }

            if (dateFilterModel.SecondCondition != null && dateFilterModel.SecondCondition.To.HasValue)
            {
                dateFilterModel.SecondCondition.To = TimeZoneInfo.ConvertTimeToUtc(dateFilterModel.SecondCondition.To.Value, timeZoneInfo);
            }

            return dateFilterModel;
        }

        public static DateTime GetTimeZoneCurrentDate(string userTimeZone)
        {
            if (string.IsNullOrEmpty(userTimeZone))
            {
                return DateTime.UtcNow;
            }

            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, userTimeZone).Date;
        }

        public static DateTime ConvertDateTimeToDateTimeWithTimeZone(DateTime dateTime, string userTimeZone)
        {
            if (string.IsNullOrEmpty(userTimeZone))
            {
                return dateTime;
            }
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, userTimeZone);
        }

        public static DateTime ConvertDateTimeToDateTimeWithUTC(DateTime dateTime, string userTimeZone)
        {
            if (string.IsNullOrEmpty(userTimeZone))
            {
                return dateTime;
            }
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);

            return TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZoneInfo);
        }
    }
}
