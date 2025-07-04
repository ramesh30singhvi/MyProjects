using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class CountryViewModel : ICountryViewModel
    {
        private ICountryService _countryService;

        public CountryViewModel(ICountryService countryService)
        {
            _countryService = countryService;
        }

        public async Task<CountryResponse> GetCountries()
        {
            CountryResponse response = await _countryService.GetCountriesAsync();
            return response;
        }
    }
}
