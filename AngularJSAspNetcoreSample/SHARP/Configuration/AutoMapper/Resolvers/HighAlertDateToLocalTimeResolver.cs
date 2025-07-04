using AutoMapper;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.Common.Constants;
using SHARP.Common.Models;
using System;
using System.Linq;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class HighAlertDateToLocalTimeResolver<TDest> : IValueResolver<HighAlertValueDto, TDest, DateTime>
    {
        private readonly string _userTimeZone;

        public HighAlertDateToLocalTimeResolver(AppConfig appConfig)
        {
            if (appConfig.Application.Any())
                _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }
        public DateTime Resolve(HighAlertValueDto source, TDest destination, DateTime destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(_userTimeZone))
            {
                return source.CreatedAt;
            }

            var userLocalTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(source.CreatedAt, _userTimeZone);

            return userLocalTime;
        }
    }
}
