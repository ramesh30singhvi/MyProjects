using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class RegionService : IRegionService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public RegionService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<RegionListResponse> GetRegions()
        {
            try
            {
                RegionListResponse response = await _apiManager.GetAsync<RegionListResponse>(_configuration["App:ContentApiUrl"] + "Region/list");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new RegionListResponse();
            }
        }

        public async Task<SubRegionListResponse> GetSubRegionsByRegionId(int regionId)
        {
            try
            {
                SubRegionListResponse response = await _apiManager.GetAsync<SubRegionListResponse>(_configuration["App:ContentApiUrl"] + "Region/sub-regions-by-regionid/" + regionId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SubRegionListResponse();
            }
        }
    }
}
