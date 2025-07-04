using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IConciergeViewModel
    {
        Task<ConciergeListResponse> GetCPConcierges();
        Task<ConciergeDetailResponse> GetCPConciergeById(Guid ConciergeGuid);
        Task<ConciergeDetailResponse> AddUpdateCPConcierge(ConciergeRequestModel request);
        List<ConciergeTypeModel> GetConciergeTypes();
    }
}
