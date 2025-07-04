using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
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
    public class SystemUpdatesService : ISystemUpdatesService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        private static string _contentBaseUrl;
        public SystemUpdatesService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
            _contentBaseUrl = _configuration["App:ContentApiUrl"];
        }
        public async Task<SystemUpdatesListResponse> GetSystemUpdates()
        {
            try
            {
                var response = await _apiManager.GetAsync<SystemUpdatesListResponse>(string.Format("{0}SystemUpdates/get-system-updates", _contentBaseUrl));
                return response;               
            }
            catch(HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SystemUpdatesListResponse();
            }
        }

        public async Task<SystemUpdatesResponse> CreateSystemUpdatesAsync(SystemUpdatesModel model)
        {
            try
            {
                var response = await _apiManager.PostAsync<SystemUpdatesModel, SystemUpdatesResponse>(_contentBaseUrl + "SystemUpdates/create-system-updates", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SystemUpdatesResponse();
            }
        }

        public async Task<SystemUpdatesResponse> GetSystemUpdateByIdGUID(Guid IdGUID)
        {
            try
            {
                var response = await _apiManager.GetAsync<SystemUpdatesResponse>(string.Format("{0}SystemUpdates/get-system-update-by-IdGUID/{1}", _contentBaseUrl, IdGUID));
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SystemUpdatesResponse();
            }
        }

        public async Task<SystemUpdatesListResponse> DeleteSystemUpdate(int Id)
        {
            try
            {
                var response = await _apiManager.DeleteAsync<SystemUpdatesListResponse>(string.Format("{0}SystemUpdates/delete-system-update/{1}", _contentBaseUrl, Id));
                return response;
            }
            catch(HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SystemUpdatesListResponse();
            }
        }
    }
}
