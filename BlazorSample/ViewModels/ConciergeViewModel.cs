using CellarPassAppAdmin.Shared.Enums;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using EnumsNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class ConciergeViewModel : IConciergeViewModel
    {
        private IConciergeService _conciergeService;

        public ConciergeViewModel(IConciergeService conciergeService)
        {
            _conciergeService = conciergeService;
        }

        public async Task<ConciergeListResponse> GetCPConcierges()
        {
            var concierges = await _conciergeService.GetCPConcierges(0);
            return concierges;
        }

        public async Task<ConciergeDetailResponse> GetCPConciergeById(Guid ConciergeGuid)
        {
            var concierge = await _conciergeService.GetCPConciergeById(ConciergeGuid);
            return concierge;
        }

        public async Task<ConciergeDetailResponse> AddUpdateCPConcierge(ConciergeRequestModel request)
        {
            var result = await _conciergeService.AddUpdateCPConcierge(request);
            return result;
        }

        public List<ConciergeTypeModel> GetConciergeTypes()
        {
            var conciergeTypes = new List<ConciergeTypeModel>();
            conciergeTypes.AddRange(Enum.GetValues(typeof(ConciergeType)).Cast<ConciergeType>().Select(
                        (item, index) => new ConciergeTypeModel
                        {
                            Name = ((ConciergeType)index).AsString(EnumFormat.Description),
                            Value = index,
                        }).ToList());
            return conciergeTypes;
        }
    }
}
