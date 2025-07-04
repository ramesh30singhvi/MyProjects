using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services.SA
{
    public class ServiceAgreementService : IServiceAgreementService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        private static string _contentBaseUrl;
        public ServiceAgreementService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
            _contentBaseUrl = _configuration["App:ContentApiUrl"];
        }
        public async Task<ServiceAgreementResponse> CreateServiceAgreement(ServiceAgreementModel model)
        {
            try
            {
                var response = await _apiManager.PostAsync<ServiceAgreementModel, ServiceAgreementResponse>(_contentBaseUrl + "ServiceAgreements", model);
                return response;
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new ServiceAgreementResponse();
            }
        }
        public async Task<ServiceAgreementsResponse> GetServiceAgreements()
        {
            try
            {
                var response = await _apiManager.GetAsync<ServiceAgreementsResponse>(string.Format("{0}ServiceAgreements", _contentBaseUrl));
                return response;
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new ServiceAgreementsResponse();
            }
        }
        public async Task<ServiceAgreementResponse> GetServiceAgreement(Guid IdGUID)
        {
            try
            {
                var response = await _apiManager.GetAsync<ServiceAgreementResponse>(string.Format("{0}ServiceAgreements/get-service-agreement/{1}", _contentBaseUrl, IdGUID));
                return response;
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new ServiceAgreementResponse();
            }
        }

        public async Task<ServiceAgreementsResponse> DeleteServiceAgreement(int Id)
        {
            try
            {
                var response = await _apiManager.DeleteAsync<ServiceAgreementsResponse>(string.Format("{0}ServiceAgreements/delete-service-agreement/{1}", _contentBaseUrl, Id));
                return response;
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new ServiceAgreementsResponse();
            }
        }

    }
}
