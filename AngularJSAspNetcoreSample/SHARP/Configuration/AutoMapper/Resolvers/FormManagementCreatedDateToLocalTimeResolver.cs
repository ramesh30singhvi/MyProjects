using AutoMapper;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.Common.Constants;
using SHARP.Common.Models;
using System;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class FormManagementCreatedDateToLocalTimeResolver<TDest> : IValueResolver<FormVersionDto, TDest, DateTime?>
    {
        private readonly string _userTimeZone;

        public FormManagementCreatedDateToLocalTimeResolver(AppConfig appConfig)
        {
            _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }

        public DateTime? Resolve(FormVersionDto source, TDest destination, DateTime? destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(_userTimeZone) || !source.CreatedDate.HasValue)
            {
                return source.CreatedDate;
            }

            var userLocalTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(source.CreatedDate.Value, _userTimeZone);

            return userLocalTime;
        }
    }
}
