using AutoMapper;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.Common.Constants;
using SHARP.Common.Models;
using System;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class AuditSentForApprovalDateToLocalTimeResolver<TDest> : IValueResolver<AuditDto, TDest, DateTime?>
    {
        private readonly string _userTimeZone;

        public AuditSentForApprovalDateToLocalTimeResolver(AppConfig appConfig)
        {
            _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }

        public DateTime? Resolve(AuditDto source, TDest destination, DateTime? destMember, ResolutionContext context)
        {
            if (!source.SentForApprovalDate.HasValue)
            {
                return null;
            }
            else
            {
                if (string.IsNullOrEmpty(_userTimeZone))
                {
                    return source.SentForApprovalDate;
                }

                var userLocalTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId((DateTime)source.SentForApprovalDate, _userTimeZone);

                return userLocalTime;
            }
        }
    }
}
