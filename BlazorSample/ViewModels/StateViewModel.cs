using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class StateViewModel : IStateViewModel
    {
        private IStateService _stateService;

        public StateViewModel(IStateService stateService)
        {
            _stateService = stateService;
        }

        public async Task<StateResponse> GetStates()
        {
            StateResponse response = await _stateService.GetStatesAsync();
            return response;
        }
    }
}
