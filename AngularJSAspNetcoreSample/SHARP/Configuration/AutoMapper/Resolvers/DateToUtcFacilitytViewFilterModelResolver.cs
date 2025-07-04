using AutoMapper;
using Newtonsoft.Json;
using SHARP.BusinessLogic.Extensions;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Filtration;
using SHARP.Common.Helpers;
using SHARP.ViewModels.Audit;
using SHARP.ViewModels.Portal;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class DateToUtcFacilitytViewFilterModelResolver : IValueResolver<PortalReportFacilityViewFilterModel, PortalReportFacilityViewFilter, DateFilterModel>
    {
        private readonly string _userTimeZone;

        public DateToUtcFacilitytViewFilterModelResolver(IUserService userService)
        {
            try
            {
                _userTimeZone = userService.GetCurrentUserTimeZone();
            }catch(NotFoundException) { }
        }

        public DateFilterModel Resolve(PortalReportFacilityViewFilterModel source, PortalReportFacilityViewFilter destination, DateFilterModel destMember, ResolutionContext context)
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
