using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class RegionViewModel : IRegionViewModel
    {
        private IRegionService _regionService;

        public RegionViewModel(IRegionService regionService)
        {
            _regionService = regionService;
        }

        public async Task<RegionListResponse> GetRegions()
        {
            RegionListResponse response = await _regionService.GetRegions();
            return response;
        }

        public async Task<SubRegionListResponse> GetSubRegionsByRegionId(int regionId)
        {
            SubRegionListResponse response = await _regionService.GetSubRegionsByRegionId(regionId);
            return response;
        }
    }
}
