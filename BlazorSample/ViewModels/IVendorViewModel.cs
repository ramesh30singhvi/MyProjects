using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IVendorViewModel
    {
        Task<BusinessVendorListResponse> GetBusinessVendors(int businessId);
        Task<BusinessVendorResponse> GetBusinessVendorById(Guid id);
        Task<BusinessVendorResponse> AddUpdateBusinessVendor(BusinessVendorRequestModel request);
    }
}
