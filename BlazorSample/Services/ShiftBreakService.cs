using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class ShiftBreakService : IShiftBreakService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public ShiftBreakService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<ShiftBreakListResponse> GetShiftBreakListAsync(int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<ShiftBreakListResponse>(_configuration["App:PeopleApiUrl"] + "ShiftBreak/get-shift-breaks/" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ShiftBreakListResponse();
            }
        }
        public async Task<ShiftBreakListResponse> AddUpdateShiftBreakListAsync(List<ShiftBreakRequestModel> models)
        {
            try
            {
                return await _apiManager.PostAsync<ShiftBreakListResponse>(_configuration["App:PeopleApiUrl"] + "ShiftBreak/add-update-shift-break", models);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ShiftBreakListResponse();
            }
        }
    }
}
