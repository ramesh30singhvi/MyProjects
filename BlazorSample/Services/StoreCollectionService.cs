using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Enums;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class StoreCollectionService : IStoreCollectionService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public StoreCollectionService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<AddEditStoreCollectionDetailResponse> AddUpdateCollection(StoreCollectionRequestModel model)
        {
            try
            {
                AddEditStoreCollectionDetailResponse response = await _apiManager.PostAsync<StoreCollectionRequestModel, AddEditStoreCollectionDetailResponse>(_configuration["App:ProductApiUrl"] + "StoreCollection/add-update-collection", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddEditStoreCollectionDetailResponse();
            }
        }

        public async Task<BaseResponse> DeleteStoreCollection(int id)
        {
            try
            {
                BaseResponse response = await _apiManager.DeleteAsync<BaseResponse>(_configuration["App:ProductApiUrl"] + "StoreCollection/delete-store-collection/" + id);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddEditProductResponse();
            }
        }

        public async Task<StoreCollectionsWithProductsPOSResponse> GetAllCollectionsWithProductsPOS(int businessId)
        {
            try
            {
                StoreCollectionsWithProductsPOSResponse response = await _apiManager.GetAsync<StoreCollectionsWithProductsPOSResponse>(_configuration["App:ProductApiUrl"] + "StoreCollection/collections-products-pos?businessId=" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new StoreCollectionsWithProductsPOSResponse();
            }
        }

        public async Task<StoreCollectionsBySelectionTypeResponse> GetCollectionsBySelectionType(int businessId, CollectionSelectionType selectionType = CollectionSelectionType.Manual)
        {
            try
            {
                StoreCollectionsBySelectionTypeResponse response = await _apiManager.GetAsync<StoreCollectionsBySelectionTypeResponse>(_configuration["App:ProductApiUrl"] + "StoreCollection/collections-by-selection-type?businessId=" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new StoreCollectionsBySelectionTypeResponse();
            }
        }

        public async Task<GetStoreCollectionDetailResponse> GetStoreCollectionDetails(string collectionGuid, int collectionId)
        {
            try
            {
                GetStoreCollectionDetailResponse response = await _apiManager.GetAsync<GetStoreCollectionDetailResponse>(_configuration["App:ProductApiUrl"] + "StoreCollection/store-collection-details?collectionGuid=" + collectionGuid + "&collectionId=" + collectionId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetStoreCollectionDetailResponse();
            }
        }

        public async Task<StoreCollectionsAdminResponse> GetStoreCollectionsAdmin(int businessId)
        {
            try
            {
                StoreCollectionsAdminResponse response = await _apiManager.GetAsync<StoreCollectionsAdminResponse>(_configuration["App:ProductApiUrl"] + "StoreCollection/collections?businessId=" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new StoreCollectionsAdminResponse();
            }
        }

        public async Task<StoreCollectionsPOSResponse> GetStoreCollectionsOMS(int businessId)
        {
            try
            {
                StoreCollectionsPOSResponse response = await _apiManager.GetAsync<StoreCollectionsPOSResponse>(_configuration["App:ProductApiUrl"] + "StoreCollection/collections-oms?businessId=" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new StoreCollectionsPOSResponse();
            }
        }

        public async Task<StoreCollectionsPOSResponse> GetStoreCollectionsPOS(int businessId)
        {
            try
            {
                StoreCollectionsPOSResponse response = await _apiManager.GetAsync<StoreCollectionsPOSResponse>(_configuration["App:ProductApiUrl"] + "StoreCollection/collections-pos?businessId=" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new StoreCollectionsPOSResponse();
            }
        }

        public async Task<BaseResponse> SavePOSCollectionSortOrder(SavePOSCollectionSortRequestModel model)
        {
            try
            {
                BaseResponse response = await _apiManager.PostAsync<SavePOSCollectionSortRequestModel, BaseResponse>(_configuration["App:ProductApiUrl"] + "StoreCollection/save-pos-collection-sort", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
    }
}
