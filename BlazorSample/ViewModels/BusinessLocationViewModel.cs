using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessLocationViewModel : IBusinessLocationViewModel
    {
        private IBusinessLocationService _locationService;

        public BusinessLocationViewModel(IBusinessLocationService locationService)
        {
            _locationService = locationService;
        }

        public async Task<BusinessLocationResponse> GetBusinessLocations(int businessId)
        {
            return await _locationService.GetBusinessLocations(businessId);
        }

        public async Task<BusinessLocationDetailResponse> GetBusinessLocationDetail(Guid locationGUID)
        {
            return await _locationService.GetBusinessLocationDetail(locationGUID);
        }

        public async Task<AddUpdateBusinessLocationResponse> AddUpdateBusinessLocation(BusinessLocationRequestModel request)
        {
            return await _locationService.AddUpdateBusinessLocation(request);
        }
    }
}
