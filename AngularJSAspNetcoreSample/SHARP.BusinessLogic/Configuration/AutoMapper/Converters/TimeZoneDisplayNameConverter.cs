using AutoMapper;
using SHARP.Common.Constants;
using SHARP.DAL.Models;
using System;

namespace SHARP.BusinessLogic.Configuration.AutoMapper.Converters
{
    public class TimeZoneDisplayNameConverter : IValueConverter<FacilityTimeZone, string>
    {
        public string Convert(FacilityTimeZone facilityTimeZone, ResolutionContext context)
        {
            if (facilityTimeZone == null)
            {
                return string.Empty;
            }

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(facilityTimeZone.Name);

            return $"{timeZoneInfo.DisplayName} ({facilityTimeZone.ShortName})";

            /*return $"(UTC {(timeZoneInfo.BaseUtcOffset < TimeSpan.Zero ? CommonConstants.MINUS : string.Empty)}" +
                $"{timeZoneInfo.BaseUtcOffset.ToString(@"hh\:mm")}) " +
                $"{facilityTimeZone.DisplayName} " +
                $"({facilityTimeZone.ShortName})";*/
        }
    }
}
