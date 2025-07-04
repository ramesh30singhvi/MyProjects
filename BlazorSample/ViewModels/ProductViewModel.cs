using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class ProductViewModel : IProductViewModel
    {
        private IProductServiceV2 _productServiceV2;
        private IProductService _productService;

        public ProductViewModel(IProductServiceV2 productServiceV2, IProductService productService)
        {
            _productServiceV2 = productServiceV2;
            _productService = productService;
        }

        public async Task<List<ProductModel>> GetProducts(int memberId)
        {
            List<ProductModel> products = await _productServiceV2.GetProductsAsync(memberId);
            return products;
        }

        public async Task<POSKeysResponse> GetPosKeysList(int memberId)
        {
            POSKeysResponse posKeys = await _productServiceV2.GetPosKeysListAsync(memberId);
            return posKeys;
        }

        public async Task<AddUpdatePOSKeyResponse> AddUpdatePosKey(PosKeyRequestModel posKey)
        {
            AddUpdatePOSKeyResponse key = await _productServiceV2.AddUpdatePosKeyAsync(posKey);
            return key;
        }

        public async Task<RemovePOSKeyResponse> RemovePosKey(RemovePosKeyRequesModel model)
        {
            RemovePOSKeyResponse result = await _productServiceV2.RemovePosKeyAsync(model);
            return result;
        }

        public async Task<UploadImageResponse> UploadProductImage(ImageUploadRequestModel model)
        {
            UploadImageResponse response = await _productServiceV2.UploadProductImageAsync(model);
            return response;
        }

        public async Task<SearchProductResponse> SearchProducts(int businessId, bool activeOnly, string keyword, string listType="")
        {
            SearchProductResponse response = await _productService.SearchProducts(businessId, activeOnly, keyword, listType);
            return response;
        }

        public async Task<ExportProductResponse> ExportProducts(int businessId, bool activeOnly, string keyword)
        {
            ExportProductResponse response = await _productService.ExportProducts(businessId, activeOnly, keyword);
            return response;
        }

        public async Task<GetProductDetailsResponse> GetProductDetails(int productId, string productGuid, int dataMode)
        {
            GetProductDetailsResponse response = await _productService.GetProductDetails(productId, productGuid, dataMode);
            return response;
        }

        public async Task<GetProductGiftCardDetailsResponse> GetProductGiftCardDetails(int businessId, int dataMode)
        {
            GetProductGiftCardDetailsResponse response = await _productService.GetProductGiftCardDetails(businessId, dataMode);
            return response;
        }

        public async Task<AddEditProductResponse> AddUpdateProduct(ProductRequestModel model)
        {
            AddEditProductResponse response = await _productService.AddUpdateProductAsync(model);
            return response;
        }

        public async Task<BaseResponse> UpdateProductStatus(ProductStatusRequestModel model)
        {
            BaseResponse response = await _productService.UpdateProductStatus(model);
            return response;
        }

        public async Task<ProductTypeResponse> GetProductTypes()
        {
            ProductTypeResponse response = await _productService.GetProductTypes();
            return response;
        }

        public async Task<ProductTagResponse> GetProductTags(int businessId)
        {
            return await _productService.GetProductTags(businessId);
        }

        public async Task<GetPOSFavoriteResponse> GetPOSFavorites(int businessId, int sortMode, bool activeOnly)
        {
            return await _productService.GetPOSFavorites(businessId, sortMode, activeOnly);
        }

        public async Task<SavePOSFavoriteResponse> SavePOSFavorites(SavePOSFavoriteRequestModel model)
        {
            return await _productService.SavePOSFavorites(model);
        }

        public async Task<BaseResponse> DeletePOSFavorite(int id, int businessId)
        {
            return await _productService.DeletePOSFavorite(id, businessId);
        }

        public async Task<List<ProductGalleryImageModel>> SaveProductGalleryImage(ProductGalleryImageRequestModel model)
        {
            return await _productService.SaveProductGalleryImage(model);
        }

        public async Task<List<ProductGalleryImageModel>> DeleteProductGalleryImage(int id, int productId)
        {
            return await _productService.DeleteProductGalleryImage(id, productId);
        }

        public async Task<AddEditGiftCardDenominationResponse> AddUpdateGiftCardDenomination(GiftCardDenominationRequestModel model)
        {
            return await _productService.AddUpdateGiftCardDenomination(model);
        }

        public async Task<AddEditGiftCardResponse> AddUpdateGiftCard(GiftCardRequestModel model)
        {
            return await _productService.AddUpdateGiftCard(model);
        }

        public async Task<BaseResponse> DeleteGiftCardDenomination(int id)
        {
            return await _productService.DeleteGiftCardDenomination(id);
        }

        public async Task<BaseResponse> DeleteGiftCard(int id, string idGUID)
        {
            return await _productService.DeleteGiftCard(id, idGUID);
        }

        public async Task<AddEditProductGiftCardResponse> AddUpdateProductGiftCard(ProductGiftCardRequestModel model)
        {
            AddEditProductGiftCardResponse response = await _productService.AddUpdateProductGiftCardAsync(model);
            return response;
        }
        public async Task<GetGiftCardManagerResponse> GetGiftCardManager(int businessId)
        {
            GetGiftCardManagerResponse response = await _productService.GetGiftCardManager(businessId);
            return response;
        }

        public async Task<BusinessGiftCardPriceDetailsResponse> GetNewBusinessGiftCardByAccountNumber(string accountNumber, int businessId)
        {
            return await _productService.GetNewBusinessGiftCardByAccountNumber(accountNumber, businessId);
        }

        public async Task<BusinessGiftCardDetailsResponse> GetBusinessGiftCardDetails(string idGuid, int id, string accountNumber)
        {
            return await _productService.GetBusinessGiftCardDetails(idGuid, id, accountNumber);
        }

        public async Task<GiftCardDenominationByProductIdResponse> GetGiftCardDenominationByProductId(int productId)
        {
            return await _productService.GetGiftCardDenominationByProductId(productId);
        }

        public async Task<GiftCardDesignsResponse> GetGiftCardDesigns()
        {
            return await _productService.GetGiftCardDesigns();
        }

        public async Task<GiftCardDesignsResponse> GetGiftCardDesignsByProduct(int productId)
        {
            return await _productService.GetGiftCardDesignsByProduct(productId);
        }

        public async Task<GiftCardDesignsResponse> GetBusinessGiftCardDesigns()
        {
            return await _productService.GetBusinessGiftCardDesigns();
        }
        public async Task<SearchProductResponse> GetProductListBasedOnMetaName(string metaName)
        {
            return await _productService.GetProductListBasedOnMetaName(metaName);
        }
    }
}
