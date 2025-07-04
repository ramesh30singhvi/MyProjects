
using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
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
    public class SaPressReleaseService : ISaPressReleaseService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public SaPressReleaseService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<CPPressReleaseListResponse> GetPressReleaseListAsync(int? id, Guid? idGuid, int? pRStatus)
        {
            try
            {
                return await _apiManager.GetAsync<CPPressReleaseListResponse>("SaPressRelease/get-press-release-list?id=" + id + "&idGuid=" + idGuid + "&pRStatus=" + pRStatus);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPPressReleaseListResponse();
            }
        }

        public async Task<CPPressReleaseResponse> GetPressReleaseByIdAsync(Guid idGuid)
        {
            try
            {
                return await _apiManager.GetAsync<CPPressReleaseResponse>("SaPressRelease/get-press-release/" + idGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPPressReleaseResponse();
            }
        }
        public async Task<CPPressReleaseResponse> AddUpdatePressReleaseAsync(CPPressReleaseRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CPPressReleaseResponse>("SaPressRelease/add-update-press-release", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPPressReleaseResponse();
            }
        }
    }
}
