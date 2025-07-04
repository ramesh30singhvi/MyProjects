using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public SalesOrderService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<AddUpdateSalesOrderResponse> AddUpdateSalesOrder(OMSSettingsRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<OMSSettingsRequestModel, AddUpdateSalesOrderResponse>(_configuration["App:SettingsApiUrl"] + "SalesOrder/add-update", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdateSalesOrderResponse();
            }
        }

        public async Task<GetSalesOrderResponse> GetSalesOrderDetails(int businessPropertyId)
        {
            try
            {
                return await _apiManager.GetAsync<GetSalesOrderResponse>(_configuration["App:SettingsApiUrl"] + "SalesOrder/details/" + businessPropertyId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetSalesOrderResponse();
            }
        }
    }
}
