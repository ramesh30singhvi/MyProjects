using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CellarPassAppAdmin.Shared.Models.ViewModel.ShippingRateViewModel;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class ShippingRatesViewModel : IShippingRatesViewModel
    {
        private IShippingRatesService _shippingRatesService;
        public ShippingRatesViewModel(IShippingRatesService shippingRatesService)
        {
            _shippingRatesService = shippingRatesService;
        }

        public async Task<AddUpdateShippingChargeResponse> AddUpdateShippingCharge(ShippingChargeRequestModel model)
        {
            return await _shippingRatesService.AddUpdateShippingCharge(model);
        }

        public async Task<ShippingRateResponse> AddUpdateShippingRates(ShippingRateRequestModel model)
        {
            return await _shippingRatesService.AddUpdateShippingRates(model);
        }

        public async Task<BaseResponse> AdjustRatesForMultipleShippingRates(AdjustRatesRequestModel model)
        {
            return await _shippingRatesService.AdjustRatesForMultipleShippingRates(model);
        }

        public async Task<BaseResponse> DeleteShippingCharge(int Id)
        {
            return await _shippingRatesService.DeleteShippingCharge(Id);
        }

        public async Task<BaseResponse> DeleteShippingRate(int Id)
        {
            return await _shippingRatesService.DeleteShippingRate(Id);
        }

        public async Task<ShippingRateResponse> GetShippingRate(int Id, Guid IdGUID)
        {
            return await _shippingRatesService.GetShippingRate(Id, IdGUID);
        }

        public async Task<ShippingRatesResponse> GetShippingRates(int businessId)
        {
            return await _shippingRatesService.GetShippingRates(businessId);
        }
    }
}
