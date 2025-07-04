using CellarPassAppAdmin.Client.Exceptions;
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
    public class ProductModifierGroupService : IProductModifierGroupService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public ProductModifierGroupService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<ProductModifierGroupListResponse> GetProductModifierGroupListAsync(int? productId)
        {
            try
            {
                return await _apiManager.GetAsync<ProductModifierGroupListResponse>(_configuration["App:ProductApiUrl"] + "ProductModifierGroup/get-product-modifier-group?ProductId=" + productId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ProductModifierGroupListResponse();
            }
        }
        public async Task<ProductModifierGroupListResponse> AddUpdateProductModifierGroupAsync(ProductModifierGroupRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<ProductModifierGroupListResponse>(_configuration["App:ProductApiUrl"] + "ProductModifierGroup/add-update-product-modifier-group", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ProductModifierGroupListResponse();
            }
        }
        public async Task<ProductModifierGroupItemListResponse> GetProductModifierGroupItemListAsync(int? productModifierGroupId, int? businessModifierGroupid)
        {
            try
            {
                return await _apiManager.GetAsync<ProductModifierGroupItemListResponse>(_configuration["App:ProductApiUrl"] + "ProductModifierGroup/get-product-modifier-group-item?ProductModifierGroupId=" + productModifierGroupId + "&BusinessModifierGroupid=" + businessModifierGroupid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ProductModifierGroupItemListResponse();
            }
        }
        public async Task<ProductModifierGroupItemListResponse> AddUpdateProductModifierGroupItemAsync(ProductModifierGroupItemRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<ProductModifierGroupItemListResponse>(_configuration["App:ProductApiUrl"] + "ProductModifierGroup/add-update-product-modifier-group-item", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ProductModifierGroupItemListResponse();
            }
        }
        public async Task<ProductModifierGroupListResponse> DeleteProductModifierGroupByIdAsync(int id)
        {
            try
            {
                return await _apiManager.DeleteAsync<ProductModifierGroupListResponse>(_configuration["App:ProductApiUrl"] + "ProductModifierGroup/delete-product-modifier-group-by-id/" + id);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ProductModifierGroupListResponse();
            }
        }
    }
}
