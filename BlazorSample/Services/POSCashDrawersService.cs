using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class POSCashDrawersService : IPOSCashDrawersService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public POSCashDrawersService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<AddUpdatePOSCashDrawerResponse> AddUpdatePOSCashDrawer(POSCashDrawerModel model)
        {
            try
            {
                return await _apiManager.PostAsync<POSCashDrawerModel, AddUpdatePOSCashDrawerResponse>(_configuration["App:SettingsApiUrl"] + "POSCashDrawers/add-update", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdatePOSCashDrawerResponse();
            }
        }
        public async Task<AddUpdatePOSCashDrawerSessionResponse> AddUpdatePOSCashDrawerSession(POSCashDrawerSessionModel model)
        {
            try
            {
                return await _apiManager.PostAsync<POSCashDrawerSessionModel, AddUpdatePOSCashDrawerSessionResponse>(_configuration["App:SettingsApiUrl"] + "POSCashDrawers/add-update-pos-cash-drawer-session", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdatePOSCashDrawerSessionResponse();
            }
        }

        public async Task<AddUpdatePOSDeviceCashDrawerResponse> AddUpdatePOSDeviceCashDrawer(POSDeviceCashDrawerModel model)
        {
            try
            {
                return await _apiManager.PostAsync<POSDeviceCashDrawerModel, AddUpdatePOSDeviceCashDrawerResponse>(_configuration["App:SettingsApiUrl"] + "POSCashDrawers/add-update-pos-device-cash-drawer", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdatePOSDeviceCashDrawerResponse();
            }
        }

        public async Task<GetPOSCashDrawerDetailsResponse> GetPOSCashDrawerDetails(int Id, string IdGuid)
        {
            try
            {
                return await _apiManager.GetAsync<GetPOSCashDrawerDetailsResponse>(_configuration["App:SettingsApiUrl"] + "POSCashDrawers/details?Id=" + Id + "&IdGuid=" + IdGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetPOSCashDrawerDetailsResponse();
            }
        }

        public async Task<GetPOSCashDrawersResponse> GetPOSCashDrawers(int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<GetPOSCashDrawersResponse>(_configuration["App:SettingsApiUrl"] + "POSCashDrawers/list/?businessId=" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetPOSCashDrawersResponse();
            }
        }
        public async Task<GetCPCashDrawerModelsResponse> GetCPCashDrawerModels()
        {
            try
            {
                return await _apiManager.GetAsync<GetCPCashDrawerModelsResponse>(_configuration["App:SettingsApiUrl"] + "POSCashDrawers/cp-cash-drawer-list");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetCPCashDrawerModelsResponse();
            }
        }

    }
}
