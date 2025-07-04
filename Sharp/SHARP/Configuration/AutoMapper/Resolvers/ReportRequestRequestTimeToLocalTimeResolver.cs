using AutoMapper;
using SHARP.BusinessLogic.DTO.ReportRequest;
using SHARP.Common.Constants;
using SHARP.Common.Models;
using SHARP.ViewModels.ReportRequest;
using System;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class ReportRequestRequestTimeToLocalTimeResolver : IValueResolver<ReportRequestDto, ReportRequestGridModel, DateTime>
    {
        private readonly string _userTimeZone;

        public ReportRequestRequestTimeToLocalTimeResolver(AppConfig appConfig)
        {
            _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }

        public DateTime Resolve(ReportRequestDto source, ReportRequestGridModel destination, DateTime destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(_userTimeZone))
            {
                return source.RequestedTime;
            }

            var userLocalTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(source.RequestedTime, _userTimeZone);

            return userLocalTime;
        }
    }
}
