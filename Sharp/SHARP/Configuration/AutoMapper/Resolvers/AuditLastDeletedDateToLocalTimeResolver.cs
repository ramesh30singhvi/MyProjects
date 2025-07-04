using AutoMapper;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.Common.Constants;
using SHARP.Common.Models;
using System;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class AuditLastDeletedDateToLocalTimeResolver<TDest> : IValueResolver<AuditDto, TDest, DateTime?>
    {
        private readonly string _userTimeZone;

        public AuditLastDeletedDateToLocalTimeResolver(AppConfig appConfig)
        {
            _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }

        public DateTime? Resolve(AuditDto source, TDest destination, DateTime? destMember, ResolutionContext context)
        {
            if (source.LastDeletedDate == null)
                return null;

            if (string.IsNullOrEmpty(_userTimeZone))
            {
                return source.LastDeletedDate;
            }

            var userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(source.LastDeletedDate.Value, TimeZoneInfo.FindSystemTimeZoneById(_userTimeZone));

            return userLocalTime;
        }
    }
}
