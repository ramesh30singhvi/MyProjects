using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Entities.v4;
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
    public class DepartmentService : IDepartmentService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public DepartmentService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<BusinessDepartmentListResponse> GetBusinessDepartmentListAsync(int businessId)
        {
            try
            {
                BusinessDepartmentListResponse response = await _apiManager.GetAsync<BusinessDepartmentListResponse>(_configuration["App:SettingsApiUrl"] + "Department/get-business-department-list/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessDepartmentListResponse();
            }
        }

        public async Task<BusinessDepartmentResponse> GetBusinessDepartmentByIdAsync(Guid id)
        {
            try
            {
                BusinessDepartmentResponse response = await _apiManager.GetAsync<BusinessDepartmentResponse>(_configuration["App:SettingsApiUrl"] + "Department/get-business-department-by-id/" + id);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessDepartmentResponse();
            }
        }

        public async Task<BusinessDepartmentResponse> AddUpdateBusinessDepartmentAsync(BusinessDepartmentRequestModel businessDepartment)
        {
            try
            {
                BusinessDepartmentResponse response = await _apiManager.PostAsync<BusinessDepartmentRequestModel, BusinessDepartmentResponse>(_configuration["App:SettingsApiUrl"] + "Department/add-update-business-department", businessDepartment);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessDepartmentResponse();
            }
        }

        public async Task<BusinessDepartmentListResponse> DeleteBusinessDepartmentByIdAsync(Guid id)
        {
            try
            {
                BusinessDepartmentListResponse response = await _apiManager.DeleteAsync<BusinessDepartmentListResponse>(_configuration["App:SettingsApiUrl"] + "Department/delete-business-department-by-id/" + id);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessDepartmentListResponse();
            }
        }
    }
}
