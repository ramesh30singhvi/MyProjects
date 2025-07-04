using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IShippingCarrierViewModel
    {
        Task<GetShippingCarriersResponse> GetAllShippingCarriers();
        Task<GetShippingCarrierServiceCodesResponse> GetShippingCarrierServiceCodesByCarrierId(int carrierId);
        Task<GetShippingCarrierServiceCodesResponse> GetAllShippingCarrierServiceCodes();
    }
}
