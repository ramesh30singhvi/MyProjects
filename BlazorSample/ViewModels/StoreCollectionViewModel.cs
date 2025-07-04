using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class StoreCollectionViewModel : IStoreCollectionViewModel
    {
        private readonly IStoreCollectionService _storeCollectionService;
        public StoreCollectionViewModel(IStoreCollectionService storeCollectionService)
        {
            _storeCollectionService = storeCollectionService;
        }
        public async Task<AddEditStoreCollectionDetailResponse> AddUpdateCollection(StoreCollectionRequestModel model)
        {
            return await _storeCollectionService.AddUpdateCollection(model);
        }
        public async Task<BaseResponse> DeleteStoreCollection(int id)
        {
            return await _storeCollectionService.DeleteStoreCollection(id);
        }
        public async Task<StoreCollectionsWithProductsPOSResponse> GetAllCollectionsWithProductsPOS(int businessId)
        {
            return await _storeCollectionService.GetAllCollectionsWithProductsPOS(businessId);
        }
        public async Task<StoreCollectionsBySelectionTypeResponse> GetCollectionsBySelectionType(int businessId)
        {
            return await _storeCollectionService.GetCollectionsBySelectionType(businessId);
        }
        public async Task<GetStoreCollectionDetailResponse> GetStoreCollectionDetails(string collectionGuid, int collectionId)
        {
            return await _storeCollectionService.GetStoreCollectionDetails(collectionGuid, collectionId);
        }
        public async Task<StoreCollectionsAdminResponse> GetStoreCollectionsAdmin(int businessId)
        {
            StoreCollectionsAdminResponse response = await _storeCollectionService.GetStoreCollectionsAdmin(businessId);
            return response;
        }
        public async Task<StoreCollectionsPOSResponse> GetStoreCollectionsOMS(int businessId)
        {
            return await _storeCollectionService.GetStoreCollectionsOMS(businessId);
        }
        public async Task<StoreCollectionsPOSResponse> GetStoreCollectionsPOS(int businessId)
        {
            return await _storeCollectionService.GetStoreCollectionsPOS(businessId);
        }
        public async Task<BaseResponse> SavePOSCollectionSortOrder(SavePOSCollectionSortRequestModel model)
        {
            return await _storeCollectionService.SavePOSCollectionSortOrder(model);
        }
    }
}
