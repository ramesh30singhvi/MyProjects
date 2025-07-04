using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IRegionViewModel
    {
        Task<RegionListResponse> GetRegions();
        Task<SubRegionListResponse> GetSubRegionsByRegionId(int regionId);
    }
}
