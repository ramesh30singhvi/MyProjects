using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class ProductServiceV2 : IProductServiceV2
    {
        private readonly IApiManager _apiManager;

        public ProductServiceV2(IApiManager apiManager)
        {
            _apiManager = apiManager;
        }

        public async Task<List<ProductModel>> GetProductsAsync(int memberId)
        {
            try
            {
                List<ProductModel> products = await _apiManager.GetAsync<List<ProductModel>>("product/list?memberId=" + memberId);
                return products;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<ProductModel>();
            }
        }

        public async Task<POSKeysResponse> GetPosKeysListAsync(int memberId)
        {
            try
            {
                POSKeysResponse posKeys = await _apiManager.GetAsync<POSKeysResponse>("product/poskeyslist?memberId=" + memberId);
                return posKeys;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new POSKeysResponse();
            }
        }

        public async Task<AddUpdatePOSKeyResponse> AddUpdatePosKeyAsync(PosKeyRequestModel posKey)
        {
            try
            {
                AddUpdatePOSKeyResponse key = await _apiManager.PostAsync<PosKeyRequestModel, AddUpdatePOSKeyResponse>("product/addupdateposkey", posKey);
                return key;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdatePOSKeyResponse();
            }
        }

        public async Task<RemovePOSKeyResponse> RemovePosKeyAsync(RemovePosKeyRequesModel model)
        {
            try
            {
                RemovePOSKeyResponse result = await _apiManager.PostAsync<RemovePosKeyRequesModel, RemovePOSKeyResponse>("product/removeposkey", model);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new RemovePOSKeyResponse();
            }
        }

        public async Task<UploadImageResponse> UploadProductImageAsync(ImageUploadRequestModel model)
        {
            try
            {
                UploadImageResponse response = await _apiManager.PostAsync<ImageUploadRequestModel, UploadImageResponse>("product/upload-product-image", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UploadImageResponse();
            }
        }
        public async Task<UploadImageResponse> UploadGiftCardDesignImage(ImageUploadRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<UploadImageResponse>("product/upload-gift-card-design-image", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UploadImageResponse();
            }
        }
    }
}
