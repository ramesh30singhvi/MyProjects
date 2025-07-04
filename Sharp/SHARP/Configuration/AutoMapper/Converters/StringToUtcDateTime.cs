using AutoMapper;
using Microsoft.AspNetCore.Http;
using SHARP.Http;
using System;

namespace SHARP.Configuration.AutoMapper.Converters
{
    public class StringToUtcDateTimeConverter: ITypeConverter<string, DateTime?>
    {
        private readonly string _userTimeZone;

        public StringToUtcDateTimeConverter(IHttpContextAccessor httpContextAccessor)
        {
            _userTimeZone = httpContextAccessor.HttpContext.GetUserTimeZone();
        }

        public DateTime? Convert(string source, DateTime? destination, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source))
            {
                return default(DateTime?);
            }

            if(!DateTime.TryParse(source, out DateTime result))
            {
                return default(DateTime?);
            }

            if (string.IsNullOrEmpty(_userTimeZone))
            {
                return result;
            }

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(_userTimeZone);

            var utcTime = TimeZoneInfo.ConvertTimeToUtc(result, timeZoneInfo);

            return utcTime;
        }
    }
}
