using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public interface ICPSettingViewModel
    {
        Task<CPSettingResponse> AddUpdateCPSettingAsync(CPSettingRequestModel model);
        Task<CPSettingResponse> GetCPSettingAsync(int? id, string metaNamespace, string metaKey);
        Task<CPSettingListResponse> GetCPSettingListAsync(string metaNamespace);
    }
}
