using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinesstypeViewModel : IBusinessTypeViewModel
    {
        private IBusinessTypeService _businessTypeService;
        public BusinesstypeViewModel(IBusinessTypeService businessTypeService)
        {
            _businessTypeService = businessTypeService;
        }
        public async Task<List<BusinessTypeViewModel>> GetBusinessTypes()
        {
            return await _businessTypeService.GetBusinessTypes();
        }
    }
}
