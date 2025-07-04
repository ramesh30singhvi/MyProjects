using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class ShippingCarrierViewModel : IShippingCarrierViewModel
    {
        private readonly IShippingCarrierService _shippingCarrierService;
        public ShippingCarrierViewModel(IShippingCarrierService shippingCarrierService)
        {
            _shippingCarrierService = shippingCarrierService;
        }
        public async Task<GetShippingCarriersResponse> GetAllShippingCarriers()
        {
            return await _shippingCarrierService.GetAllShippingCarriers();
        }

        public async Task<GetShippingCarrierServiceCodesResponse> GetAllShippingCarrierServiceCodes()
        {
            return await _shippingCarrierService.GetAllShippingCarrierServiceCodes();
        }

        public async Task<GetShippingCarrierServiceCodesResponse> GetShippingCarrierServiceCodesByCarrierId(int carrierId)
        {
            return await _shippingCarrierService.GetShippingCarrierServiceCodesByCarrierId(carrierId);
        }
    }
}
