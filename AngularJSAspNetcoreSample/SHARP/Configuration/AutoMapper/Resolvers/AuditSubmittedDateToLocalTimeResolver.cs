using AutoMapper;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.Common.Constants;
using SHARP.Common.Models;
using System;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class AuditSubmittedDateToLocalTimeResolver<TDest> : IValueResolver<AuditDto, TDest, DateTime>
    {
        private readonly string _userTimeZone;

        public AuditSubmittedDateToLocalTimeResolver(AppConfig appConfig)
        {
            _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }

        public DateTime Resolve(AuditDto source, TDest destination, DateTime destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(_userTimeZone))
            {
                return source.SubmittedDate;
            }

            var userLocalTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(source.SubmittedDate, _userTimeZone);

            return userLocalTime;
        }
    }
}
