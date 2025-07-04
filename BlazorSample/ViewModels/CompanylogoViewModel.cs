using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class CompanylogoViewModel : ICompanyLogoViewModel
    {
        private ICompanyLogoService _companyLogoService;
        public CompanylogoViewModel(ICompanyLogoService companyLogoService)
        {
            _companyLogoService = companyLogoService;
        }
        public async Task<CompanyLogoViewModel> SaveCompanyLogo(CompanyLogoRequestViewModel companyLogo)
        {
            return await _companyLogoService.SaveCompanyLogo(companyLogo);
        }
    }
}
