using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class InventoryViewModel : IInventoryViewModel
    {
        private readonly IInventoryService _inventoryService;
        public InventoryViewModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task<BaseResponse> AddUpdateBusinessInventorySettings(BusinessInventoryAddUpdateRequestModel model)
        {
            return await _inventoryService.AddUpdateBusinessInventorySettings(model);
        }

        public async Task<GetBusinessReturnReasonsResponse> GetBusinessReturnReasons(int businessId)
        {
            return await _inventoryService.GetBusinessReturnReasons(businessId);
        }

        public async Task<InventoryDetailsResponse> GetInventoryDetails(int businessId)
        {
            return await _inventoryService.GetInventoryDetails(businessId);
        }
        public async Task<InventoryLocationsResponse> GetInventoryLocations(int businessId)
        {
            return await _inventoryService.GetInventoryLocations(businessId);
        }

        public async Task<BaseResponse> UpdateInventory(List<InventoryUpdateRequestModel> model)
        {
            return await _inventoryService.UpdateInventory(model);
        }

        public async Task<GetSalesChannelsForInventoryLocationResponse> GetSalesChannelsForInventoryLocation(Guid locationGUID)
        {
            return await _inventoryService.GetSalesChannelsForInventoryLocation(locationGUID);
        }
    }
}
