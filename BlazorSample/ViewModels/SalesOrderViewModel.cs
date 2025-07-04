using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class SalesOrderViewModel : ISalesOrderViewModel
    {
        private readonly ISalesOrderService _salesOrderService;
        public SalesOrderViewModel(ISalesOrderService salesOrderService)
        {
            _salesOrderService = salesOrderService;
        }

        public async Task<AddUpdateSalesOrderResponse> AddUpdateSalesOrder(OMSSettingsRequestModel model)
        {
            return await _salesOrderService.AddUpdateSalesOrder(model);
        }

        public async Task<GetSalesOrderResponse> GetSalesOrderDetails(int businessPropertyId)
        {
            return await _salesOrderService.GetSalesOrderDetails(businessPropertyId);
        }
    }
}
