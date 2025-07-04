using AutoMapper;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Helpers;
using System;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class CurrentDateResolver<TSource, TDestination> : IValueResolver<TSource, TDestination, DateTime>
    {
        private readonly string _userTimeZone;

        public CurrentDateResolver(IUserService userService)
        {
            _userTimeZone = userService.GetCurrentUserTimeZone();
        }

        public DateTime Resolve(TSource source, TDestination destination, DateTime destMember, ResolutionContext context)
        {
            return DateHelpers.GetTimeZoneCurrentDate(_userTimeZone);
        }
    }
}
