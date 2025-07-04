using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessTeamPositionViewModel
    {
        Task<BusinessTeamPositionListResponse> GetBusinessTeamPositionListAsync(int businessTeamCompensationId);
        Task<BusinessTeamPositionResponse> AddUpdateBusinessTeamPositionListAsync(BusinessTeamPositionRequestModel model);
    }
}
