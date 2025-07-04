using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class HoursViewModel : IHoursViewModel
    {
        private IHoursService _hoursService;

        public HoursViewModel(IHoursService hoursService)
        {
            _hoursService = hoursService;
        }
        public async Task<BusinessHoursResponse> GetBusinessHours(int businessPropertyId)
        {
            return await _hoursService.GetBusinessHoursAsync(businessPropertyId);
        }
        public async Task<BusinessHoursResponse> AddUpdateBusinessHour(List<BusinessHour> hour)
        {
            return await _hoursService.SaveBusinessHoursAsync(hour);
        }
    }
}
