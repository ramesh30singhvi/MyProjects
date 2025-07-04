using AutoMapper;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.Common.Constants;
using SHARP.Common.Models;
using System;

namespace SHARP.Configuration.Resolvers
{
    public class AuditCompletedDateToLocalTimeResolver<TDest> : IValueResolver<AuditDto, TDest, DateTime?>
    {
        private readonly string _userTimeZone;

        public AuditCompletedDateToLocalTimeResolver(AppConfig appConfig)
        {
            _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }

        public DateTime? Resolve(AuditDto source, TDest destination, DateTime? destMember, ResolutionContext context)
        {
            if (!source.AuditCompletedDate.HasValue)
            {
                return null;
            }
            else
            {
                if (string.IsNullOrEmpty(_userTimeZone))
                {
                    return source.AuditCompletedDate;
                }

                var userLocalTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId((DateTime)source.AuditCompletedDate, _userTimeZone);

                return userLocalTime;
            }
        }
    }
}
