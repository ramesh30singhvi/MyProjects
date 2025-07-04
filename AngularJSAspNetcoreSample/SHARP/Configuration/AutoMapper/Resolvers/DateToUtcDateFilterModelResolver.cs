using AutoMapper;
using Newtonsoft.Json;
using SHARP.BusinessLogic.Extensions;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.Common.Filtration;
using SHARP.Common.Helpers;
using SHARP.Common.Models;
using SHARP.ViewModels.Portal;
using System.Linq;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class DateToUtcDateFilterModelResolver : IValueResolver<PortalReportFilterModel, PortalReportFilter, DateFilterModel>
    {
        private readonly string _userTimeZone;

        public DateToUtcDateFilterModelResolver( AppConfig appConfig)
        {
            if (appConfig.Application.Any())
                _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }

        public DateFilterModel Resolve(PortalReportFilterModel source, PortalReportFilter destination, DateFilterModel destMember, ResolutionContext context)
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
