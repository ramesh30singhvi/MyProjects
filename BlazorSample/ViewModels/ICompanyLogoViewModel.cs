using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ICompanyLogoViewModel
    {
        Task<CompanyLogoViewModel> SaveCompanyLogo(CompanyLogoRequestViewModel companyLogo);
    }
}
