using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessMetaDataViewModel : IBusinessMetaDataViewModel
    {
        private IBusinessMetaDataService _businessMetaDataService;

        public BusinessMetaDataViewModel(IBusinessMetaDataService businessMetaDataService)
        {
            _businessMetaDataService = businessMetaDataService;
        }
        public async Task<BusinessMetaDataListResponse> GetBusinessMetaDataListAsync(int businessId, int category)
        {
            return await _businessMetaDataService.GetBusinessMetaDataListAsync(businessId, category);
        }
        public async Task<BusinessMetaDataResponse> GetBusinessMetaDataAsync(int id)
        {
            return await _businessMetaDataService.GetBusinessMetaDataAsync(id);
        }
        public async Task<BusinessMetaDataResponse> AddUpdateBusinessMetaDataAsync(BusinessMetaDataRequestModel model)
        {
            return await _businessMetaDataService.AddUpdateBusinessMetaDataAsync(model);
        }
        public async Task<BusinessMetaDataXProductTypeListResponse> GetBusinessMetaDataXProductTypeListAsync()
        {
            return await _businessMetaDataService.GetBusinessMetaDataXProductTypeListAsync();
        }
        public async Task<BusinessMetaDataXProductTypeItemListResponse> GetBusinessMetaDataXProductTypeItemListAsync(int? id, int businessId)
        {
            return await _businessMetaDataService.GetBusinessMetaDataXProductTypeItemListAsync(id, businessId);
        }
        public async Task<BusinessMetaDataXProductTypeListResponse> AddUpdateBusinessMetaDataXProductTypeAsync(BusinessMetaDataXProductTypeRequestModel model)
        {
            return await _businessMetaDataService.AddUpdateBusinessMetaDataXProductTypeAsync(model);
        }
        public async Task<BaseResponse> SaveBusinessMetaDataSortOrder(BusinessMetaDataSortRequestModel model)
        {
            return await _businessMetaDataService.SaveBusinessMetaDataSortOrder(model);
        }
    }
}
