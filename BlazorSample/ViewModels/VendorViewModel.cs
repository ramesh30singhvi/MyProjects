using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class VendorViewModel : IVendorViewModel
    {
        private IVendorService _vednorService;

        public VendorViewModel(IVendorService vednorService)
        {
            _vednorService = vednorService;
        }

        public async Task<BusinessVendorListResponse> GetBusinessVendors(int businessId)
        {
            BusinessVendorListResponse response = await _vednorService.GetBusinessVendorsAsync(businessId);
            return response;
        }

        public async Task<BusinessVendorResponse> GetBusinessVendorById(Guid id)
        {
            BusinessVendorResponse response = await _vednorService.GetBusinessVendorByIdAsync(id);
            return response;
        }

        public async Task<BusinessVendorResponse> AddUpdateBusinessVendor(BusinessVendorRequestModel request)
        {
            BusinessVendorResponse response = await _vednorService.AddUpdateBusinessVendorAsync(request);
            return response;
        }
    }
}
