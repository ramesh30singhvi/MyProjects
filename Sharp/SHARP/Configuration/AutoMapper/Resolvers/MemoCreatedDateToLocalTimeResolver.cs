using AutoMapper;
using SHARP.BusinessLogic.DTO.Memo;
using SHARP.Common.Constants;
using SHARP.Common.Models;
using SHARP.ViewModels.Memo;
using System;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class MemoCreatedDateToLocalTimeResolver : IValueResolver<MemoDto, MemoModel, DateTime>
    {
        private readonly string _userTimeZone;

        public MemoCreatedDateToLocalTimeResolver(AppConfig appConfig)
        {
            _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }

        public DateTime Resolve(MemoDto source, MemoModel destination, DateTime destMember, ResolutionContext context)
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
