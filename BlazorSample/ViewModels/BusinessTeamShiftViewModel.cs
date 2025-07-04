using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessTeamShiftViewModel : IBusinessTeamShiftViewModel
    {
        private IBusinessTeamShiftService _shiftBreakService;

        public BusinessTeamShiftViewModel(IBusinessTeamShiftService shiftBreakService)
        {
            _shiftBreakService = shiftBreakService;
        }

        public async Task<BusinessTeamShiftResponse> BusinessTeamShiftCheckIn(BusinessTeamShiftRequestModel model)
        {
            return await _shiftBreakService.BusinessTeamShiftCheckIn(model);
        }

        public async Task<BusinessTeamShiftListResponse> BusinessTeamShiftCheckOut(BusinessTeamShiftRequestModel model)
        {
            return await _shiftBreakService.BusinessTeamShiftCheckOut(model);
        }

        public async Task<BusinessTeamShiftResponse> BusinessTeamShiftSEndBreak(BusinessTeamShiftRequestModel model)
        {
            return await _shiftBreakService.BusinessTeamShiftSEndBreak(model);
        }

        public async Task<BusinessTeamShiftResponse> BusinessTeamShiftStartBreak(BusinessTeamShiftRequestModel model)
        {
            return await _shiftBreakService.BusinessTeamShiftStartBreak(model);
        }
    }
}
