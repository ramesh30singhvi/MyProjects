using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IInventoryViewModel
    {
        Task<InventoryDetailsResponse> GetInventoryDetails(int businessId);
        Task<InventoryLocationsResponse> GetInventoryLocations(int businessId);
        Task<BaseResponse> UpdateInventory(List<InventoryUpdateRequestModel> model);
        Task<GetBusinessReturnReasonsResponse> GetBusinessReturnReasons(int businessId);
        Task<BaseResponse> AddUpdateBusinessInventorySettings(BusinessInventoryAddUpdateRequestModel model);
        Task<GetSalesChannelsForInventoryLocationResponse> GetSalesChannelsForInventoryLocation(Guid locationGUID);
    }
}
