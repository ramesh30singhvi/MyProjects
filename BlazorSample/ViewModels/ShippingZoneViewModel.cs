using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class ShippingZoneViewModel : IShippingZoneViewModel
    {
        private readonly IShippingZoneService _shippingZoneService;
        public ShippingZoneViewModel(IShippingZoneService shippingZoneService)
        {
            _shippingZoneService = shippingZoneService;
        }

        public async Task<AddUpdateShippingZoneResponse> AddUpdateShippingZone(ShippingZoneRequestModel model)
        {
            return await _shippingZoneService.AddUpdateShippingZone(model);
        }

        public async Task<GetShippingZoneListResponse> DeleteShippingZone(int Id, int businessId)
        {
            return await _shippingZoneService.DeleteShippingZone(Id, businessId);
        }

        public async Task<GetShippingZoneResponse> GetShippingZoneByIdGUID(Guid guid)
        {
            return await _shippingZoneService.GetShippingZoneByIdGUID(guid);
        }

        public async Task<GetShippingZoneListResponse> GetShippingZones(int businessId)
        {
            return await _shippingZoneService.GetShippingZones(businessId);
        }
    }
}
