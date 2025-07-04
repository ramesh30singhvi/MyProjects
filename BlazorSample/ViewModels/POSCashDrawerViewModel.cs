using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class POSCashDrawerViewModel : IPOSCashDrawerViewModel
    {
        private IPOSCashDrawersService _pOSCashDrawersService;

        public POSCashDrawerViewModel(IPOSCashDrawersService pOSCashDrawersService)
        {
            _pOSCashDrawersService = pOSCashDrawersService;
        }
        public async Task<AddUpdatePOSCashDrawerResponse> AddUpdatePOSCashDrawer(POSCashDrawerModel model)
        {
            return await _pOSCashDrawersService.AddUpdatePOSCashDrawer(model);
        }

        public async Task<GetPOSCashDrawerDetailsResponse> GetPOSCashDrawerDetails(int Id, string IdGuid)
        {
            return await _pOSCashDrawersService.GetPOSCashDrawerDetails(Id, IdGuid);
        }

        public async Task<GetPOSCashDrawersResponse> GetPOSCashDrawers(int businessId)
        {
            return await _pOSCashDrawersService.GetPOSCashDrawers(businessId);
        }

        public async Task<GetCPCashDrawerModelsResponse> GetCPCashDrawerModels()
        {
            return await _pOSCashDrawersService.GetCPCashDrawerModels();
        }
    }
}
