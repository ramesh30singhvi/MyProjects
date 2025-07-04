using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using System;
using System.Threading.Tasks;
using static CellarPassAppAdmin.Shared.Models.ViewModel.ShippingRateViewModel;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IShippingRatesViewModel
    {
        Task<ShippingRateResponse> AddUpdateShippingRates(ShippingRateRequestModel model);
        Task<ShippingRatesResponse> GetShippingRates(int businessId);
        Task<ShippingRateResponse> GetShippingRate(int Id, Guid IdGUID);
        Task<BaseResponse> DeleteShippingRate(int Id);
        Task<BaseResponse> AdjustRatesForMultipleShippingRates(AdjustRatesRequestModel model);
        Task<BaseResponse> DeleteShippingCharge(int Id);
        Task<AddUpdateShippingChargeResponse> AddUpdateShippingCharge(ShippingChargeRequestModel model);
    }
}
