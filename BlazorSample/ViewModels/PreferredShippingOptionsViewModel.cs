using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class PreferredShippingOptionsViewModel : IPreferredShippingOptionsViewModel
    {
        private readonly IPreferredShippingOptionsService _preferredShippingOptionsService;
        public PreferredShippingOptionsViewModel(IPreferredShippingOptionsService preferredShippingOptionsService)
        {
            _preferredShippingOptionsService = preferredShippingOptionsService;
        }
        public async Task<GetPreferredShippingOptionsListResponse> GetPreferredShippingOptions(int businessId)
        {
            return await _preferredShippingOptionsService.GetPreferredShippingOptions(businessId);
        }
    }
}
