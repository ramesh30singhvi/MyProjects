using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IWeatherFeedViewModel
    {
        Task<WeatherFeedResponse> GetWeatherFeed(string zipCode, string geoLat, string geoLong, int noOfDaysInfo = 3);
    }
}
