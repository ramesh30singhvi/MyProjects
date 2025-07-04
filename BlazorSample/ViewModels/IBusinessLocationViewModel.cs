using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessLocationViewModel
    {
        Task<BusinessLocationResponse> GetBusinessLocations(int businessId);
        Task<BusinessLocationDetailResponse> GetBusinessLocationDetail(Guid locationGUID);
        Task<AddUpdateBusinessLocationResponse> AddUpdateBusinessLocation(BusinessLocationRequestModel request);
    }
}
