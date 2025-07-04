using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public class SystemUpdatesViewModel : ISystemUpdatesViewModel
    {
        private readonly ISystemUpdatesService _systemUpdatesService;
        public SystemUpdatesViewModel(ISystemUpdatesService systemUpdatesService)
        {
            _systemUpdatesService = systemUpdatesService;
        }

        public async Task<SystemUpdatesResponse> CreateSystemUpdatesAsync(SystemUpdatesModel model)
        {
            return await _systemUpdatesService.CreateSystemUpdatesAsync(model);
        }

        public async Task<SystemUpdatesListResponse> DeleteSystemUpdate(int Id)
        {
            return await _systemUpdatesService.DeleteSystemUpdate(Id);
        }

        public async Task<SystemUpdatesResponse> GetSystemUpdateByIdGUID(Guid IdGUID)
        {
            return await _systemUpdatesService.GetSystemUpdateByIdGUID(IdGUID);
        }

        public async Task<SystemUpdatesListResponse> GetSystemUpdates()
        {
            return await _systemUpdatesService.GetSystemUpdates();
        }
    }
}
