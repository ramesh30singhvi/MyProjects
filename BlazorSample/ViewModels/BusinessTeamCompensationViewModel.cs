using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessTeamCompensationViewModel : IBusinessTeamCompensationViewModel
    {
        private IBusinessTeamCompensationService _businessTeamCompensationService;

        public BusinessTeamCompensationViewModel(IBusinessTeamCompensationService businessTeamCompensationService)
        {
            _businessTeamCompensationService = businessTeamCompensationService;
        }
        public async Task<BusinessTeamCompensationListResponse> GetBusinessTeamCompensationListAsync(int businessTeamId)
        {
            return await _businessTeamCompensationService.GetBusinessTeamCompensationListAsync(businessTeamId);
        }
        public async Task<BusinessTeamCompensationResponse> AddUpdateBusinessTeamCompensationListAsync(BusinessTeamCompensationRequestModel model)
        {
            return await _businessTeamCompensationService.AddUpdateBusinessTeamCompensationListAsync(model);
        }
    }
}
