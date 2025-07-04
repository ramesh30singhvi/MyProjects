using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class StateService : IStateService
    {
        private readonly IApiManager _apiManager;

        public StateService(IApiManager apiManager)
        {
            _apiManager = apiManager;
        }

        public async Task<StateResponse> GetStatesAsync()
        {
            try
            {
                StateResponse response = await _apiManager.GetAsync<StateResponse>("state/list");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new StateResponse();
            }
        }
    }
}
