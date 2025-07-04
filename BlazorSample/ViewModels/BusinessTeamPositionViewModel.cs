using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessTeamPositionViewModel : IBusinessTeamPositionViewModel
    {
        private IBusinessTeamPositionService _businessTeamPositionService;

        public BusinessTeamPositionViewModel(IBusinessTeamPositionService businessTeamPositionService)
        {
            _businessTeamPositionService = businessTeamPositionService;
        }
        public async Task<BusinessTeamPositionListResponse> GetBusinessTeamPositionListAsync(int businessTeamCompensationId)
        {
            return await _businessTeamPositionService.GetBusinessTeamPositionListAsync(businessTeamCompensationId);
        }
        public async Task<BusinessTeamPositionResponse> AddUpdateBusinessTeamPositionListAsync(BusinessTeamPositionRequestModel model)
        {
            return await _businessTeamPositionService.AddUpdateBusinessTeamPositionListAsync(model);
        }
    }
}
