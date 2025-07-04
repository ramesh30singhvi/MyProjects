using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ISystemUpdatesViewModel
    {
        Task<SystemUpdatesListResponse> GetSystemUpdates();
        Task<SystemUpdatesResponse> GetSystemUpdateByIdGUID(Guid IdGUID);
        Task<SystemUpdatesResponse> CreateSystemUpdatesAsync(SystemUpdatesModel model);
        Task<SystemUpdatesListResponse> DeleteSystemUpdate(int Id);
    }
}
