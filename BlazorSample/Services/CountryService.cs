using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class CountryService : ICountryService
    {
        private readonly IApiManager _apiManager;

        public CountryService(IApiManager apiManager)
        {
            _apiManager = apiManager;
        }

        public async Task<CountryResponse> GetCountriesAsync()
        {
            try
            {
                CountryResponse response = await _apiManager.GetAsync<CountryResponse>("country/list");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CountryResponse();
            }
        }
    }
}
