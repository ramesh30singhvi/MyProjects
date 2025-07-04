using AutoMapper;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.Common.Models;
using System;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class ReportAIContentCreatedDateToLocalTimeResolver<TDest> : IValueResolver<ReportAIContentDto, TDest, DateTime?>
    {
        private readonly string _userTimeZone;

        public ReportAIContentCreatedDateToLocalTimeResolver(AppConfig appConfig)
        {
            _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }

        public DateTime? Resolve(ReportAIContentDto source, TDest destination, DateTime? destMember, ResolutionContext context)
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
