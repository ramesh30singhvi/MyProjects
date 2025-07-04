using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Entities.v4;
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
    public class HoursService : IHoursService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public HoursService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<BusinessHoursResponse> GetBusinessHoursAsync(int businessPropertyId)
        {
            try
            {
                BusinessHoursResponse response = await _apiManager.GetAsync<BusinessHoursResponse>(_configuration["App:SettingsApiUrl"] + "Hours/get-business-hours/" + businessPropertyId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessHoursResponse();
            }
        }
        public async Task<BusinessHoursResponse> SaveBusinessHoursAsync(List<BusinessHour> hours)
        {
            try
            {
                BusinessHoursResponse response = await _apiManager.PostAsync<List<BusinessHour>, BusinessHoursResponse>(_configuration["App:SettingsApiUrl"] + "Hours/add-update-business-hours", hours);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessHoursResponse();
            }
        }
    }
}
