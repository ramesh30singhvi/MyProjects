using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessAddressesViewModel : IBusinessAddressViewModel
    {
        private IBusinessAddressService _businessAddressService;
        public BusinessAddressesViewModel(IBusinessAddressService businessAddressService)
        {
            _businessAddressService = businessAddressService;
        }

        public async Task<GetBusinessAddressResponse> GetBusinessAddressByIdGuid(Guid AddressGuid)
        {
            return await _businessAddressService.GetBusinessAddressByIdGuid(AddressGuid);
        }

        public async Task<List<BusinessAddressViewModel>> GetBusinessAddresses(int businessId)
        {
            return await _businessAddressService.GetBusinessAddresses(businessId);
        }
    }
}
