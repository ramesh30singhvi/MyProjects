using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IPOSCashDrawerViewModel
    {
        Task<AddUpdatePOSCashDrawerResponse> AddUpdatePOSCashDrawer(POSCashDrawerModel model);
        Task<GetPOSCashDrawerDetailsResponse> GetPOSCashDrawerDetails(int Id, string IdGuid);
        Task<GetPOSCashDrawersResponse> GetPOSCashDrawers(int businessId);
        Task<GetCPCashDrawerModelsResponse> GetCPCashDrawerModels();
    }
}
