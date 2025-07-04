using AutoMapper;
using Newtonsoft.Json;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Filtration;
using SHARP.Common.Helpers;
using SHARP.ViewModels.Form;

namespace SHARP.Configuration.AutoMapper.Resolvers
{
    public class FormManagementCreatedDateToUtcDateFilterModelResolver : IValueResolver<FormVersionFilterModel, FormVersionFilter, DateFilterModel>
    {
        private readonly string _userTimeZone;

        public FormManagementCreatedDateToUtcDateFilterModelResolver(IUserService userService)
        {
            _userTimeZone = userService.GetCurrentUserTimeZone();
        }

        public DateFilterModel Resolve(FormVersionFilterModel source, FormVersionFilter destination, DateFilterModel destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.CreatedDate))
            {
                return default;
            }

            var dateFilterModel = JsonConvert.DeserializeObject<DateFilterModel>(source.CreatedDate);

            if (string.IsNullOrEmpty(_userTimeZone))
            {
                return dateFilterModel;
            }

            return DateHelpers.ConvertToUtcDate(dateFilterModel, _userTimeZone);
        }
    }
}
