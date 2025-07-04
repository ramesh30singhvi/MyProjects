using AutoMapper;
using Newtonsoft.Json;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Filtration;
using SHARP.Common.Helpers;
using SHARP.ViewModels.Report;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class ReportAIContentCreatedDateToUtcDateFilterModelResolver : IValueResolver<AuditAIReportFilterModel, AuditAIReportFilter, DateFilterModel>
    {
        private readonly string _userTimeZone;

        public ReportAIContentCreatedDateToUtcDateFilterModelResolver(IUserService userService)
        {
            _userTimeZone = userService.GetCurrentUserTimeZone();
        }

        public DateFilterModel Resolve(AuditAIReportFilterModel source, AuditAIReportFilter destination, DateFilterModel destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.CreatedAt))
            {
                return default;
            }

            var dateFilterModel = JsonConvert.DeserializeObject<DateFilterModel>(source.CreatedAt);

            if (string.IsNullOrEmpty(_userTimeZone))
            {
                return dateFilterModel;
            }

            return DateHelpers.ConvertToUtcDate(dateFilterModel, _userTimeZone);
        }
    }
}
