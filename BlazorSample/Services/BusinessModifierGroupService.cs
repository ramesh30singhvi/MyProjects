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
    public class BusinessModifierGroupService : IBusinessModifierGroupService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public BusinessModifierGroupService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<BusinessModifierGroupListResponse> GetBusinessModifierGroupListAsync()
        {
            try
            {
                return await _apiManager.GetAsync<BusinessModifierGroupListResponse>(_configuration["App:ProductApiUrl"] + "BusinessModifierGroup/get-business-modifier-group/");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessModifierGroupListResponse();
            }
        }
        public async Task<BusinessModifierGroupResponse> GetBusinessModifierGroupAsync(int? id, Guid? idGUID)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessModifierGroupResponse>(_configuration["App:ProductApiUrl"] + "BusinessModifierGroup/get-business-modifier-group-detail?Id=" + id + "&IdGUID=" + idGUID);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessModifierGroupResponse();
            }
        }
        public async Task<BusinessModifierGroupResponse> AddUpdateBusinessModifierGroupAsync(BusinessModifierGroupRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessModifierGroupResponse>(_configuration["App:ProductApiUrl"] + "BusinessModifierGroup/add-update-business-modifier-group", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessModifierGroupResponse();
            }
        }

        public async Task<BusinessModifierGroupListResponse> DeleteBusinessModifierGroupByIdAsync(int id)
        {
            try
            {
                return await _apiManager.DeleteAsync<BusinessModifierGroupListResponse>(_configuration["App:ProductApiUrl"] + "BusinessModifierGroup/delete-business-modifier-group-by-id/" + id);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessModifierGroupListResponse();
            }
        }
        public async Task<BusinessModifierGroupItemListResponse> GetBusinessModifierGroupItemListAsync(int businessModifierGroupId)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessModifierGroupItemListResponse>(_configuration["App:ProductApiUrl"] + "BusinessModifierGroup/get-business-modifier-group-item/" + businessModifierGroupId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessModifierGroupItemListResponse();
            }
        }
    }
}
