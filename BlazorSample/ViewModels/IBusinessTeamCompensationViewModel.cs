using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessTeamCompensationViewModel
    {
        Task<BusinessTeamCompensationListResponse> GetBusinessTeamCompensationListAsync(int businessTeamId);
        Task<BusinessTeamCompensationResponse> AddUpdateBusinessTeamCompensationListAsync(BusinessTeamCompensationRequestModel model);
    }
}
