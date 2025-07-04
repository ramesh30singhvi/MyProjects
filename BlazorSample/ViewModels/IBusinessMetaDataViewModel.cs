using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessMetaDataViewModel
    {
        Task<BusinessMetaDataListResponse> GetBusinessMetaDataListAsync(int businessId, int category);
        Task<BusinessMetaDataResponse> GetBusinessMetaDataAsync(int id);
        Task<BusinessMetaDataResponse> AddUpdateBusinessMetaDataAsync(BusinessMetaDataRequestModel model);
        Task<BusinessMetaDataXProductTypeListResponse> GetBusinessMetaDataXProductTypeListAsync();
        Task<BusinessMetaDataXProductTypeItemListResponse> GetBusinessMetaDataXProductTypeItemListAsync(int? id, int businessId);
        Task<BusinessMetaDataXProductTypeListResponse> AddUpdateBusinessMetaDataXProductTypeAsync(BusinessMetaDataXProductTypeRequestModel model);
        Task<BaseResponse> SaveBusinessMetaDataSortOrder(BusinessMetaDataSortRequestModel model);
    }
}
