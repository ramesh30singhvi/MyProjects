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
    public class DateToUtcHighAlertDateFilterModelResolver : IValueResolver<HighAlertPortalFilterModel, HighAlertPortalFilter, DateFilterModel>
    {
        private readonly string _userTimeZone;

        public DateToUtcHighAlertDateFilterModelResolver(IUserService userService, AppConfig appConfig)
        {
            try
            {
               // _userTimeZone = userService?.GetCurrentUserTimeZone();
               if(appConfig.Application.Any())
                _userTimeZone = appConfig.Application[CommonConstants.USER_TIME_ZONE]?.ToString();
            }
            catch(NotFoundException)
            { }
        }

        public DateFilterModel Resolve(HighAlertPortalFilterModel source, HighAlertPortalFilter destination, DateFilterModel destMember, ResolutionContext context)
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
