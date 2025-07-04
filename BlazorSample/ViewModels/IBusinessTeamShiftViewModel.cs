using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessTeamShiftViewModel
    {
        Task<BusinessTeamShiftResponse> BusinessTeamShiftCheckIn(BusinessTeamShiftRequestModel model);
        Task<BusinessTeamShiftListResponse> BusinessTeamShiftCheckOut(BusinessTeamShiftRequestModel model);
        Task<BusinessTeamShiftResponse> BusinessTeamShiftStartBreak(BusinessTeamShiftRequestModel model);
        Task<BusinessTeamShiftResponse> BusinessTeamShiftSEndBreak(BusinessTeamShiftRequestModel model);
    }
}
