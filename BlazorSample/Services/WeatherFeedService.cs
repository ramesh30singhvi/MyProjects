using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class WeatherFeedService: IWeatherFeedService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public WeatherFeedService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<WeatherFeedResponse> GetWeatherFeed(string zipCode, string geoLat, string geoLong, int nofDaysInfo)
        {
            try
            {
                return await _apiManager.GetAsync<WeatherFeedResponse>($"WeatherFeed/get-weather-feed?zipcode={zipCode}&geoLat={geoLat}&geoLong={geoLong}&nofDaysInfo={nofDaysInfo}");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new WeatherFeedResponse();
            }
        }
    }
}
