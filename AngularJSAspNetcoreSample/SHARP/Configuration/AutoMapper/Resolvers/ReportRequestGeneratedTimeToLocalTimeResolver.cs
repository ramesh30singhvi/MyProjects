using AutoMapper;
using SHARP.BusinessLogic.DTO.ReportRequest;
using SHARP.Common.Constants;
using SHARP.Common.Models;
using SHARP.ViewModels.ReportRequest;
using System;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class ReportRequestGeneratedTimeToLocalTimeResolver : IValueResolver<ReportRequestDto, ReportRequestGridModel, DateTime?>
    {
        private readonly string _userTimeZone;

        public ReportRequestGeneratedTimeToLocalTimeResolver(AppConfig appConfig)
        {
            _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }

        public DateTime? Resolve(ReportRequestDto source, ReportRequestGridModel destination, DateTime? destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(_userTimeZone) || !source.GeneratedTime.HasValue)
            {
                return source.GeneratedTime;
            }

            var userLocalTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(source.GeneratedTime.Value, _userTimeZone);

            return userLocalTime;
        }
    }
}
