using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public class CPSettingViewModel : ICPSettingViewModel
    {
        private readonly ICPSettingService _cPSettingService;
        public CPSettingViewModel(ICPSettingService cPSettingService)
        {
            _cPSettingService = cPSettingService;
        }
        public async Task<CPSettingResponse> AddUpdateCPSettingAsync(CPSettingRequestModel model)
        {
            return await _cPSettingService.AddUpdateCPSettingAsync(model);
        }
        public async Task<CPSettingResponse> GetCPSettingAsync(int? id, string metaNamespace, string metaKey)
        {
            return await _cPSettingService.GetCPSettingAsync(id, metaNamespace, metaKey);
        }
        public async Task<CPSettingListResponse> GetCPSettingListAsync(string metaNamespace)
        {
            return await _cPSettingService.GetCPSettingListAsync(metaNamespace);
        }
    }
}
