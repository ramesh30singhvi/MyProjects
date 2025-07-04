using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IProductViewModel
    {
        Task<List<ProductModel>> GetProducts(int memberId);
        Task<POSKeysResponse> GetPosKeysList(int memberId);
        Task<AddUpdatePOSKeyResponse> AddUpdatePosKey(PosKeyRequestModel posKey);
        Task<RemovePOSKeyResponse> RemovePosKey(RemovePosKeyRequesModel model);
        Task<UploadImageResponse> UploadProductImage(ImageUploadRequestModel model);
        Task<SearchProductResponse> SearchProducts(int businessId, bool activeOnly, string keyword, string listType="");
        Task<ExportProductResponse> ExportProducts(int businessId, bool activeOnly, string keyword);
        Task<GetProductDetailsResponse> GetProductDetails(int productId, string productGuid, int dataMode);
        Task<AddEditProductResponse> AddUpdateProduct(ProductRequestModel model);
        Task<BaseResponse> UpdateProductStatus(ProductStatusRequestModel model);
        Task<ProductTypeResponse> GetProductTypes();
        Task<ProductTagResponse> GetProductTags(int businessId);
        Task<GetPOSFavoriteResponse> GetPOSFavorites(int businessId, int sortMode, bool activeOnly);
        Task<SavePOSFavoriteResponse> SavePOSFavorites(SavePOSFavoriteRequestModel model);
        Task<BaseResponse> DeletePOSFavorite(int id, int businessId);
        Task<List<ProductGalleryImageModel>> SaveProductGalleryImage(ProductGalleryImageRequestModel model);
        Task<List<ProductGalleryImageModel>> DeleteProductGalleryImage(int id, int productId);
        Task<AddEditGiftCardDenominationResponse> AddUpdateGiftCardDenomination(GiftCardDenominationRequestModel model);
        Task<AddEditGiftCardResponse> AddUpdateGiftCard(GiftCardRequestModel model);
        Task<BaseResponse> DeleteGiftCardDenomination(int id);
        Task<BaseResponse> DeleteGiftCard(int id, string idGUID);
        Task<AddEditProductGiftCardResponse> AddUpdateProductGiftCard(ProductGiftCardRequestModel model);
        Task<GetProductGiftCardDetailsResponse> GetProductGiftCardDetails(int businessId, int dataMode);
        Task<BusinessGiftCardPriceDetailsResponse> GetNewBusinessGiftCardByAccountNumber(string accountNumber, int businessId);
        Task<BusinessGiftCardDetailsResponse> GetBusinessGiftCardDetails(string idGuid, int id, string accountNumber);
        Task<GetGiftCardManagerResponse> GetGiftCardManager(int businessId);
        Task<GiftCardDenominationByProductIdResponse> GetGiftCardDenominationByProductId(int productId);
        Task<GiftCardDesignsResponse> GetGiftCardDesigns();
        Task<GiftCardDesignsResponse> GetGiftCardDesignsByProduct(int productId);
        Task<GiftCardDesignsResponse> GetBusinessGiftCardDesigns();
        Task<SearchProductResponse> GetProductListBasedOnMetaName(string metaName);

    }
}