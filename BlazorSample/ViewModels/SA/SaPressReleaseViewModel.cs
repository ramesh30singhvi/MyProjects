using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public class SaPressReleaseViewModel : ISaPressReleaseViewModel
    {
        private readonly ISaPressReleaseService _saPressReleaseService;
        public SaPressReleaseViewModel(ISaPressReleaseService saPressReleaseService)
        {
            _saPressReleaseService = saPressReleaseService;
        }
        public async Task<CPPressReleaseListResponse> GetPressReleaseListAsync(int? id, Guid? idGuid, int? pRStatus)
        {
            return await _saPressReleaseService.GetPressReleaseListAsync(id, idGuid, pRStatus);
        }
        public async Task<CPPressReleaseResponse> GetPressReleaseByIdAsync(Guid idGuid)
        {
            return await _saPressReleaseService.GetPressReleaseByIdAsync(idGuid);
        }
        public async Task<CPPressReleaseResponse> AddUpdatePressReleaseAsync(CPPressReleaseRequestModel model)
        {
            return await _saPressReleaseService.AddUpdatePressReleaseAsync(model);
        }
    }
}
