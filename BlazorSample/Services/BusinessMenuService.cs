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
    public class BusinessMenuService : IBusinessMenuService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public BusinessMenuService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<BusinessMenuListResponse> GetBusinessMenuListAsync()
        {
            try
            {
                return await _apiManager.GetAsync<BusinessMenuListResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/get-business-menu/");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuListResponse();
            }
        }
        public async Task<BusinessMenuResponse> GetBusinessMenuAsync(int? id, Guid? idGUID)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessMenuResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/get-business-menu-detail?Id=" + id + "&IdGUID=" + idGUID);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuResponse();
            }
        }
        public async Task<BusinessMenuResponse> AddUpdateBusinessMenuAsync(BusinessMenuRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessMenuResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/add-update-business-menu", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuResponse();
            }
        }

        public async Task<BusinessMenuListResponse> DeleteBusinessMenuByIdAsync(int id)
        {
            try
            {
                return await _apiManager.DeleteAsync<BusinessMenuListResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/delete-business-menu-by-id/" + id);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuListResponse();
            }
        }
        public async Task<BusinessMenuGroupListResponse> GetBusinessMenuGroupListAsync(int businessMenuId)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessMenuGroupListResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/get-business-menu-group?BusinessMenuId=" + businessMenuId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuGroupListResponse();
            }
        }
        public async Task<BusinessMenuGroupResponse> GetBusinessMenuGroupAsync(int? id, Guid? idGUID)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessMenuGroupResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/get-business-menu-group-detail?Id=" + id + "&IdGUID=" + idGUID);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuGroupResponse();
            }
        }
        public async Task<BusinessMenuGroupResponse> AddUpdateBusinessMenuGroupAsync(BusinessMenuGroupRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessMenuGroupResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/add-update-business-menu-group", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuGroupResponse();
            }
        }
        public async Task<BusinessMenuGroupListResponse> DeleteBusinessMenuGroupByIdAsync(int id)
        {
            try
            {
                return await _apiManager.DeleteAsync<BusinessMenuGroupListResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/delete-business-menu-group-by-id/" + id);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuGroupListResponse();
            }
        }
        public async Task<BusinessMenuGroupListResponse> ArrangeBusinessMenuGroupAsync(List<ArrangeBusinessMenuGroupRequestModel> models)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessMenuGroupListResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/arrange-business-menu-group", models);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuGroupListResponse();
            }
        }
        public async Task<BusinessMenuGroupItemListResponse> GetBusinessMenuGroupItemListAsync(int? businessMenuGroupId, int? productId)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessMenuGroupItemListResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/get-business-menu-group-item?BusinessMenuGroupId=" + businessMenuGroupId + "&ProductId=" + productId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuGroupItemListResponse();
            }
        }
        public async Task<BusinessMenuGroupItemResponse> GetBusinessMenuGroupItemAsync(int? id)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessMenuGroupItemResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/get-business-menu-group-item-detail?Id=" + id);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuGroupItemResponse();
            }
        }
        public async Task<BusinessMenuGroupItemResponse> AddUpdateBusinessMenuGroupItemAsync(BusinessMenuGroupItemRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessMenuGroupItemResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/add-update-business-menu-group-item", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuGroupItemResponse();
            }
        }

        public async Task<BusinessMenuGroupItemListResponse> DeleteBusinessMenuGroupItemByIdAsync(int id)
        {
            try
            {
                return await _apiManager.DeleteAsync<BusinessMenuGroupItemListResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/delete-business-menu-group-item-by-id/" + id);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuGroupItemListResponse();
            }
        }
        public async Task<BusinessMenuGroupItemListResponse> ArrangeBusinessMenuGroupItemAsync(List<BusinessMenuGroupItemRequestModel> models)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessMenuGroupItemListResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/arrange-business-menu-group-item", models);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuGroupItemListResponse();
            }
        }

        public async Task<BusinessMenuGroupItemListResponse> AddUpdateBusinessMenuGroupItemsAsync(BusinessMenuGroupProductRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessMenuGroupItemListResponse>(_configuration["App:ProductApiUrl"] + "BusinessMenu/add-update-business-menu-group-items", model);
           }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMenuGroupItemListResponse();
            }
        }
    }
}
