using AutoMapper;
using Newtonsoft.Json;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Filtration;
using SHARP.Common.Helpers;
using SHARP.ViewModels.Portal;
using SHARP.BusinessLogic.Extensions;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class DateToUtcLoginsTrackingFilterModelResolver : IValueResolver<PortalLoginsTrackingViewFilterModel, PortalLoginsTrackingViewFilter, DateFilterModel>
    {
        private readonly string _userTimeZone;

        public DateToUtcLoginsTrackingFilterModelResolver(IUserService userService)
        {
            try
            {
                _userTimeZone = userService.GetCurrentUserTimeZone();
            }
            catch (NotFoundException) { }
        }

        public DateFilterModel Resolve(PortalLoginsTrackingViewFilterModel source, PortalLoginsTrackingViewFilter destination, DateFilterModel destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.Date))
            {
                return default;
            }

            var dateFilterModel = JsonConvert.DeserializeObject<DateFilterModel>(source.Date);

            if (string.IsNullOrEmpty(_userTimeZone))
            {
                return dateFilterModel;
            }

            return DateHelpers.ConvertToUtcDate(dateFilterModel, _userTimeZone);
        }
    }
}
