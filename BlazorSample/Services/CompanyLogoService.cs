using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class CompanyLogoService : ICompanyLogoService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        private static string _settingsBaseUrl;

        public CompanyLogoService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
            _settingsBaseUrl = _configuration["App:SettingsApiUrl"];
        }
        public async Task<CompanyLogoViewModel> SaveCompanyLogo(CompanyLogoRequestViewModel companyLogo)
        {
            try
            {
                var response = await _apiManager.PostAsync<CompanyLogoRequestViewModel, CompanyLogoResponse>(string.Format("{0}CompanyLogo", _settingsBaseUrl), companyLogo);
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CompanyLogoViewModel();
            }
        }
    }
}
