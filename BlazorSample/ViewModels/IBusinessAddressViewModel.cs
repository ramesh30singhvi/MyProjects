using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessAddressViewModel
    {
        Task<List<BusinessAddressViewModel>> GetBusinessAddresses(int businessId);
        Task<GetBusinessAddressResponse> GetBusinessAddressByIdGuid(Guid AddressGuid);
    }
}
