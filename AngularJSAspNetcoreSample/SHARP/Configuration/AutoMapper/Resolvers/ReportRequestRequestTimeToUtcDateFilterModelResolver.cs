using AutoMapper;
using Newtonsoft.Json;
using SHARP.Common.Constants;
using SHARP.Common.Filtration;
using SHARP.Common.Helpers;
using SHARP.Common.Models;
using SHARP.ViewModels.ReportRequest;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class ReportRequestRequestTimeToUtcDateFilterModelResolver : IValueResolver<ReportRequestFilterModel, ReportRequestFilter, DateFilterModel>
    {
        private readonly string _userTimeZone;

        public ReportRequestRequestTimeToUtcDateFilterModelResolver(AppConfig appConfig)
        {
            _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
        }

        public DateFilterModel Resolve(ReportRequestFilterModel source, ReportRequestFilter destination, DateFilterModel destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.RequestedTime))
            {
                return default;
            }

            var dateFilterModel = JsonConvert.DeserializeObject<DateFilterModel>(source.RequestedTime);

            if (string.IsNullOrEmpty(_userTimeZone))
            {
                return dateFilterModel;
            }

            return DateHelpers.ConvertToUtcDate(dateFilterModel, _userTimeZone);
        }
    }
}
