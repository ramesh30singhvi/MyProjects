using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class WeatherFeedViewModel : IWeatherFeedViewModel
    {
        private readonly IWeatherFeedService _weatherFeedService;
        public WeatherFeedViewModel(IWeatherFeedService weatherFeedService)
        {
            _weatherFeedService = weatherFeedService;
        }
        public async Task<WeatherFeedResponse> GetWeatherFeed(string zipCode, string geoLat, string geoLong, int noOfDaysInfo)
        {
            return await _weatherFeedService.GetWeatherFeed(zipCode, geoLat, geoLong, noOfDaysInfo);
        }
    }
}
