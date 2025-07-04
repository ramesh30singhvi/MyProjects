using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public interface ISaPressReleaseViewModel
    {
        Task<CPPressReleaseListResponse> GetPressReleaseListAsync(int? id, Guid? idGuid, int? pRStatus);
        Task<CPPressReleaseResponse> GetPressReleaseByIdAsync(Guid idGuid);
        Task<CPPressReleaseResponse> AddUpdatePressReleaseAsync(CPPressReleaseRequestModel model);
    }
}
