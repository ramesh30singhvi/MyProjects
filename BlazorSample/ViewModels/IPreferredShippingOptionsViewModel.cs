using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IPreferredShippingOptionsViewModel
    {
        Task<GetPreferredShippingOptionsListResponse> GetPreferredShippingOptions(int businessId);
    }
}
