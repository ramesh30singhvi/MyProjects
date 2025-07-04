using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IShippingZoneViewModel
    {
        Task<AddUpdateShippingZoneResponse> AddUpdateShippingZone(ShippingZoneRequestModel model);
        Task<GetShippingZoneListResponse> GetShippingZones(int businessId);
        Task<GetShippingZoneResponse> GetShippingZoneByIdGUID(Guid guid);
        Task<GetShippingZoneListResponse> DeleteShippingZone(int Id, int businessId);
    }
}
