using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Enums;
using CellarPassAppAdmin.Shared.Helpers;
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
    public class SaContentService : ISaContentService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public SaContentService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<string> GetAdminLoginPageContentAsync()
        {
            try
            {
                return await _apiManager.GetAsync<string>("SaContent/get-admin-login-page-content");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return string.Empty;
            }
        }
        public async Task<CPContentResponse> GetCPContentByContentTypeAsync(int id, int contentType, string contentName)
        {
            try
            {
                if(contentName == EnumHelper.GetDescription(ContentBlocks.PrivacyPolicyPlatform) || contentName == EnumHelper.GetDescription(ContentBlocks.TermsAndConditions))
                {
                    return await _apiManager.GetAsync<CPContentResponse>($"SaContent/get-cp-content-by-type-allow-unauthorized?id={id}&contentType={contentType}&contentName={contentName}");
                }
                else
                {
                    return await _apiManager.GetAsync<CPContentResponse>($"SaContent/get-cp-content-by-type?id={id}&contentType={contentType}&contentName={contentName}");
                }
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPContentResponse();
            }
        }
        public async Task<CPContentResponse> AddUpdateCPContentAsync(CPContentRequestModel content)
        {
            try
            {
                return await _apiManager.PostAsync<CPContentRequestModel, CPContentResponse>("SaContent/add-update-cp-content", content);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPContentResponse();
            }
        }

        public async Task<CPContentNamesByContentTypeResponse> GetCPContentNamesByContentType(int contentType)
        {
            try
            {
                return await _apiManager.GetAsync<CPContentNamesByContentTypeResponse>("SaContent/get-cp-content-names-by-type/" + contentType);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPContentNamesByContentTypeResponse();
            }
        }
    }
}
