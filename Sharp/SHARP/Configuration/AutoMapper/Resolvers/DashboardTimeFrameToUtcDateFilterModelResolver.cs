using AutoMapper;
using Newtonsoft.Json;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Helpers;
using SHARP.ViewModels.Dashboard;
using System;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class DashboardTimeFrameToUtcDateFilterModelResolver : IValueResolver<DashboardFilterModel, DashboardFilter, DateFilterModel>
    {
        private readonly string _userTimeZone;

        public DashboardTimeFrameToUtcDateFilterModelResolver(IUserService userService)
        {
            _userTimeZone = userService.GetCurrentUserTimeZone();
        }

        public DateFilterModel Resolve(DashboardFilterModel source, DashboardFilter destination, DateFilterModel destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.TimeFrame) || string.IsNullOrEmpty(_userTimeZone))
            {
                return default;
            }

            var dateFilterModel = new DateFilterModel();

            var userLocalDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, _userTimeZone).Date;

            switch (source.TimeFrame)
            {
                case CommonConstants.DAY:
                    dateFilterModel.FirstCondition = new Condition<DateTime>()
                    {
                        Type = CompareType.Equals,
                        From = userLocalDate,
                        To = userLocalDate.AddDays(1)
                    };
                    break;
                case CommonConstants.WEEK:
                    var weekStart = userLocalDate.AddDays(-(int)userLocalDate.DayOfWeek);

                    dateFilterModel.FirstCondition = new Condition<DateTime>()
                    {
                        Type = CompareType.InRange,
                        From = weekStart,
                        To = weekStart.AddDays(7)
                    };
                    break;
                case CommonConstants.MONTH:
                    var monthStart = userLocalDate.AddDays(1 - userLocalDate.Day);

                    dateFilterModel.FirstCondition = new Condition<DateTime>()
                    {
                        Type = CompareType.InRange,
                        From = monthStart,
                        To = monthStart.AddMonths(1)
                    };
                    break;
                default:
                    dateFilterModel = JsonConvert.DeserializeObject<DateFilterModel>(source.TimeFrame);
                    break;
            }

            return DateHelpers.ConvertToUtcDate(dateFilterModel, _userTimeZone);
        }
    }
}
