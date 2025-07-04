using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IStoreCollectionViewModel
    {
        Task<StoreCollectionsAdminResponse> GetStoreCollectionsAdmin(int businessId); 
        Task<StoreCollectionsPOSResponse> GetStoreCollectionsOMS(int businessId); 
        Task<StoreCollectionsPOSResponse> GetStoreCollectionsPOS(int businessId); 
        Task<StoreCollectionsWithProductsPOSResponse> GetAllCollectionsWithProductsPOS(int businessId); 
        Task<AddEditStoreCollectionDetailResponse> AddUpdateCollection(StoreCollectionRequestModel model); 
        Task<BaseResponse> SavePOSCollectionSortOrder(SavePOSCollectionSortRequestModel model); 
        Task<GetStoreCollectionDetailResponse> GetStoreCollectionDetails(string collectionGuid, int collectionId); 
        Task<BaseResponse> DeleteStoreCollection(int id); 
        Task<StoreCollectionsBySelectionTypeResponse> GetCollectionsBySelectionType(int businessId); 
    }
}
