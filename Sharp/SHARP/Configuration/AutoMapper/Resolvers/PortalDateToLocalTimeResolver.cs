using AutoMapper;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.Common.Constants;
using SHARP.Common.Models;
using System;
using System.Linq;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class PortalDateToLocalTimeResolver<TDest>  : IValueResolver<PortalReportDto, TDest, DateTime>
    {
        private readonly string _userTimeZone;

        public PortalDateToLocalTimeResolver(AppConfig appConfig)
        {
            if (appConfig.Application.Any())
                _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }

        public DateTime Resolve(PortalReportDto source, TDest destination, DateTime destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(_userTimeZone))
            {
                return source.CreatedDate;
            }

            var userLocalTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(source.CreatedDate, _userTimeZone);

            return userLocalTime;
        }
    }

}
