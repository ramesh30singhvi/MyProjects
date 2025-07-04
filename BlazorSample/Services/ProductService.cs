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
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class ProductService : IProductService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public ProductService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<AddEditProductResponse> AddUpdateProductAsync(ProductRequestModel model)
        {
            try
            {
                AddEditProductResponse response = await _apiManager.PostAsync<ProductRequestModel, AddEditProductResponse>(_configuration["App:ProductApiUrl"] + "Product/add-update-product", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddEditProductResponse();
            }
        }

        public async Task<GetProductDetailsResponse> GetProductDetails(int productId, string productGuid, int dataMode)
        {
            try
            {
                GetProductDetailsResponse response = await _apiManager.GetAsync<GetProductDetailsResponse>(_configuration["App:ProductApiUrl"] + "Product/details?productId=" + productId + "&productGuid=" + productGuid + "&dataMode=" + dataMode);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetProductDetailsResponse();
            }
        }

        public async Task<GetProductGiftCardDetailsResponse> GetProductGiftCardDetails(int businessId, int dataMode)
        {
            try
            {
                GetProductGiftCardDetailsResponse response = await _apiManager.GetAsync<GetProductGiftCardDetailsResponse>(_configuration["App:ProductApiUrl"] + "Product/product-gift-card-details?businessId=" + businessId + "&dataMode=" + dataMode);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetProductGiftCardDetailsResponse();
            }
        }

        public async Task<SearchProductResponse> SearchProducts(int businessId, bool activeOnly, string keyword, string listType="")
        {
            try
            {
                SearchProductResponse response = await _apiManager.GetAsync<SearchProductResponse>(_configuration["App:ProductApiUrl"] + "Product/list?businessId=" + businessId + "&activeOnly=" + activeOnly + "&keyword=" + keyword+"&listType="+listType);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SearchProductResponse();
            }
        }

        public async Task<ExportProductResponse> ExportProducts(int businessId, bool activeOnly, string keyword)
        {
            try
            {
                ExportProductResponse response = await _apiManager.GetAsync<ExportProductResponse>(_configuration["App:ProductApiUrl"] + "Product/export-products?businessId=" + businessId + "&activeOnly=" + activeOnly + "&keyword=" + keyword);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ExportProductResponse();
            }
        }

        public async Task<BaseResponse> UpdateProductStatus(ProductStatusRequestModel model)
        {
            try
            {
                BaseResponse response = await _apiManager.PostAsync<ProductStatusRequestModel, BaseResponse>(_configuration["App:ProductApiUrl"] + "Product/update-product-status", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<ProductTypeResponse> GetProductTypes()
        {
            try
            {
                ProductTypeResponse response = await _apiManager.GetAsync<ProductTypeResponse>(_configuration["App:ProductApiUrl"] + "Product/product-types");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ProductTypeResponse();
            }
        }

        public async Task<ProductTagResponse> GetProductTags(int businessId)
        {
            try
            {
                ProductTagResponse response = await _apiManager.GetAsync<ProductTagResponse>(_configuration["App:ProductApiUrl"] + "Product/product-tags?businessId=" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ProductTagResponse();
            }
        }

        public async Task<GetPOSFavoriteResponse> GetPOSFavorites(int businessId, int sortMode, bool activeOnly)
        {
            try
            {
                GetPOSFavoriteResponse response = await _apiManager.GetAsync<GetPOSFavoriteResponse>(_configuration["App:ProductApiUrl"] + "Product/pos-favorites?businessId=" + businessId + "&sortMode=" + sortMode + "&activeOnly=" + activeOnly);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetPOSFavoriteResponse();
            }
        }

        public async Task<SavePOSFavoriteResponse> SavePOSFavorites(SavePOSFavoriteRequestModel model)
        {
            try
            {
                SavePOSFavoriteResponse response = await _apiManager.PostAsync<SavePOSFavoriteRequestModel, SavePOSFavoriteResponse>(_configuration["App:ProductApiUrl"] + "Product/save-pos-favorites", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SavePOSFavoriteResponse();
            }
        }

        public async Task<BaseResponse> DeletePOSFavorite(int id, int businessId)
        {
            try
            {
                BaseResponse response = await _apiManager.DeleteAsync<BaseResponse>(_configuration["App:ProductApiUrl"] + "Product/delete-POS-Favorite?productId=" + id + "&businessId=" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddEditProductResponse();
            }
        }

        public async Task<List<ProductGalleryImageModel>> SaveProductGalleryImage(ProductGalleryImageRequestModel model)
        {
            try
            {
                var response = await _apiManager.PostAsync<ProductGalleryImageRequestModel, ProductGalleryImagesResponse>(_configuration["App:ProductApiUrl"] + "Product/add-update-product-gallery-image", model);
                return response.data;
            }
            catch(HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new List<ProductGalleryImageModel>();
            }
        }

        public async Task<List<ProductGalleryImageModel>> DeleteProductGalleryImage(int Id, int ProductId)
        {
            try
            {
                var response = await _apiManager.DeleteAsync<ProductGalleryImagesResponse>(string.Format("{0}Product/delete-product-gallery-image/{1}/{2}", _configuration["App:ProductApiUrl"], Id, ProductId));
                return response.data;
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new List<ProductGalleryImageModel>();
            }
        }

        public async Task<AddEditGiftCardDenominationResponse> AddUpdateGiftCardDenomination(GiftCardDenominationRequestModel model)
        {
            try
            {
                AddEditGiftCardDenominationResponse response = await _apiManager.PostAsync<GiftCardDenominationRequestModel, AddEditGiftCardDenominationResponse>(_configuration["App:ProductApiUrl"] + "Product/add-update-gift-card-denomination", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddEditGiftCardDenominationResponse();
            }
        }

        public async Task<AddEditGiftCardResponse> AddUpdateGiftCard(GiftCardRequestModel model)
        {
            try
            {
                AddEditGiftCardResponse response = await _apiManager.PostAsync<GiftCardRequestModel, AddEditGiftCardResponse>(_configuration["App:ProductApiUrl"] + "Product/add-update-gift-card-denomination", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddEditGiftCardResponse();
            }
        }

        public async Task<BaseResponse> DeleteGiftCardDenomination(int id)
        {
            try
            {
                BaseResponse response = await _apiManager.DeleteAsync<BaseResponse>(_configuration["App:ProductApiUrl"] + "Product/delete-gift-card-denomination/" + id);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<BaseResponse> DeleteGiftCard(int id, string idGUID)
        {
            try
            {
                BaseResponse response = await _apiManager.DeleteAsync<BaseResponse>(_configuration["App:ProductApiUrl"] + "Product/delete-gift-card?id=" + id + "&idGUID=" + idGUID);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddEditProductResponse();
            }
        }

        public async Task<AddEditProductGiftCardResponse> AddUpdateProductGiftCardAsync(ProductGiftCardRequestModel model)
        {
            try
            {
                AddEditProductGiftCardResponse response = await _apiManager.PostAsync<ProductGiftCardRequestModel, AddEditProductGiftCardResponse>(_configuration["App:ProductApiUrl"] + "Product/add-update-product-gift-card", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddEditProductGiftCardResponse();
            }
        }

        public async Task<GiftCardDenominationByProductIdResponse> GetGiftCardDenominationByProductId(int productId)
        {
            try
            {
                GiftCardDenominationByProductIdResponse response = await _apiManager.GetAsync<GiftCardDenominationByProductIdResponse>(_configuration["App:ProductApiUrl"] + "Product/gift-card-denomination?productId=" + productId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GiftCardDenominationByProductIdResponse();
            }
        }

        public async Task<GiftCardTransactionHistoryResponse> GetGiftCardTransactionHistory(int businessId,int orderId, string giftCardGuid)
        {
            try
            {
                GiftCardTransactionHistoryResponse response = await _apiManager.GetAsync<GiftCardTransactionHistoryResponse>(_configuration["App:ProductApiUrl"] + "Product/gift-card-transaction-history?businessId="+ businessId + "&orderId=" + orderId + "&giftCardGuid=" + giftCardGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GiftCardTransactionHistoryResponse();
            }
        }

        public async Task<GetGiftCardManagerResponse> GetGiftCardManager(int businessId)
        {
            try
            {
                GetGiftCardManagerResponse response = await _apiManager.GetAsync<GetGiftCardManagerResponse>(_configuration["App:ProductApiUrl"] + "Product/gift-card-manager?businessId=" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetGiftCardManagerResponse();
            }
        }

        public async Task<BusinessGiftCardDetailsResponse> GetBusinessGiftCardDetails(string idGuid, int id, string accountNumber)
        {
            try
            {
                BusinessGiftCardDetailsResponse response = await _apiManager.GetAsync<BusinessGiftCardDetailsResponse>(_configuration["App:ProductApiUrl"] + "Product/business-gift-card-details?idGuid=" + idGuid + "&id=" + id + "&accountNumber=" + accountNumber);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessGiftCardDetailsResponse();
            }
        }

        public async Task<BusinessGiftCardPriceDetailsResponse> GetNewBusinessGiftCardByAccountNumber(string accountNumber, int businessId)
        {
            try
            {
                BusinessGiftCardPriceDetailsResponse response = await _apiManager.GetAsync<BusinessGiftCardPriceDetailsResponse>($"{_configuration["App:ProductApiUrl"]}Product/get-new-business-gift-card-by-account-number?accountNumber={accountNumber}&businessId={businessId}");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessGiftCardPriceDetailsResponse();
            }
        }

        public async Task<GiftCardDepletionResponse> GetGiftCardDepletionReport(int businessId, DateTime fromDate, DateTime toDate, string giftCardGuid = "")
        {
            try
            {
                GiftCardDepletionResponse response = await _apiManager.GetAsync<GiftCardDepletionResponse>(_configuration["App:ProductApiUrl"] + "Product/gift-card-depletion-report?businessId=" + businessId + "&fromDate=" + fromDate + "&toDate=" + toDate + "&giftCardGuid=" + giftCardGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GiftCardDepletionResponse();
            }
        }

        public async Task<GiftCardDesignsResponse> GetGiftCardDesigns()
        {
            try
            {
                GiftCardDesignsResponse response = await _apiManager.GetAsync<GiftCardDesignsResponse>(_configuration["App:ProductApiUrl"] + "Product/gift-card-designs?");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GiftCardDesignsResponse();
            }
        }

        public async Task<GiftCardDesignsResponse> GetBusinessGiftCardDesigns()
        {
            try
            {
                GiftCardDesignsResponse response = await _apiManager.GetAsync<GiftCardDesignsResponse>(_configuration["App:ProductApiUrl"] + "Product/business-gift-card-designs?");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GiftCardDesignsResponse();
            }
        }
        public async Task<AddBusinessGiftCardBatchResponse> AddBusinessGiftCardBatch(BusinessGiftCardBatchRequestModel model)
        {
            try
            {
                AddBusinessGiftCardBatchResponse response = await _apiManager.PostAsync<BusinessGiftCardBatchRequestModel, AddBusinessGiftCardBatchResponse>(_configuration["App:ProductApiUrl"] + "Product/add-business_gift_card_batch", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddBusinessGiftCardBatchResponse();
            }
        }
        public async Task<BusinessGiftCardBatchListResponse> GetBusinessGiftCardBatchList(int businessId)
        {
            try
            {
                BusinessGiftCardBatchListResponse response = await _apiManager.GetAsync<BusinessGiftCardBatchListResponse>(_configuration["App:ProductApiUrl"] + "Product/gift-card-batch-list?businessId=" + businessId);
                return response;
    }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessGiftCardBatchListResponse();
}
        }
        public async Task<GiftCardDesignsResponse> GetGiftCardDesignsByProduct(int productId)
        {
            try
            {
                GiftCardDesignsResponse response = await _apiManager.GetAsync<GiftCardDesignsResponse>(_configuration["App:ProductApiUrl"] + "Product/gift-card-designs-by-product?productId="+productId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GiftCardDesignsResponse();
            }
        }

        public async Task<SearchProductResponse> GetProductListBasedOnMetaName(string metaName)
        {
            try
            {
                SearchProductResponse response = await _apiManager.GetAsync<SearchProductResponse>(_configuration["App:ProductApiUrl"] + "Product/get-productList-baseOn-metaData?name=" + metaName);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SearchProductResponse();
            }
        }
    }
}
