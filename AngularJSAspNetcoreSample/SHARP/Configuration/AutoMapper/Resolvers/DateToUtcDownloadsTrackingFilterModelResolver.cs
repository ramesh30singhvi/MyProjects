using AutoMapper;
using Newtonsoft.Json;
using SHARP.BusinessLogic.Extensions;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Filtration;
using SHARP.Common.Helpers;
using SHARP.ViewModels.Portal;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class DateToUtcDownloadsTrackingFilterModelResolver : IValueResolver<PortalDownloadsTrackingViewFilterModel, PortalDownloadsTrackingViewFilter, DateFilterModel>
    {
        private readonly string _userTimeZone;

        public DateToUtcDownloadsTrackingFilterModelResolver(IUserService userService)
        {
            try
            {
                _userTimeZone = userService.GetCurrentUserTimeZone();
            }
            catch (NotFoundException) { }
        }

        public DateFilterModel Resolve(PortalDownloadsTrackingViewFilterModel source, PortalDownloadsTrackingViewFilter destination, DateFilterModel destMember, ResolutionContext context)
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
