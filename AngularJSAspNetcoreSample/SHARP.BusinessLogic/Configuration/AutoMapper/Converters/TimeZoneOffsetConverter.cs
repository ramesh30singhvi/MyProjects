using AutoMapper;
using SHARP.DAL.Models;
using System;

namespace SHARP.BusinessLogic.Configuration.AutoMapper.Converters
{
    public class TimeZoneOffsetConverter : IValueConverter<FacilityTimeZone, int>
    {
        public int Convert(FacilityTimeZone facilityTimeZone, ResolutionContext context)
        {
            if (facilityTimeZone == null)
            {
                return default(int);
            }

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(facilityTimeZone.Name);

            return timeZoneInfo.BaseUtcOffset.Hours;
        }
    }
}
