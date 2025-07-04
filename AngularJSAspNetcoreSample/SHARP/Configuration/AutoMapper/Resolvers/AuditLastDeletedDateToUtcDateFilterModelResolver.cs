using AutoMapper;
using Newtonsoft.Json;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Filtration;
using SHARP.Common.Helpers;
using SHARP.ViewModels.Audit;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class AuditLastDeletedDateToUtcDateFilterModelResolver : IValueResolver<AuditFilterModel, AuditFilter, DateFilterModel>
    {
        private readonly string _userTimeZone;

        public AuditLastDeletedDateToUtcDateFilterModelResolver(IUserService userService)
        {
            _userTimeZone = userService.GetCurrentUserTimeZone();
        }

        public DateFilterModel Resolve(AuditFilterModel source, AuditFilter destination, DateFilterModel destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.LastDeletedDate))
            {
                return default;
            }

            var dateFilterModel = JsonConvert.DeserializeObject<DateFilterModel>(source.LastDeletedDate);

            if (string.IsNullOrEmpty(_userTimeZone))
            {
                return dateFilterModel;
            }

            return DateHelpers.ConvertToUtcDate(dateFilterModel, _userTimeZone);
        }
    }
}