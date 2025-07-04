using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class AlternateTemporaryLoginService : IAlternateTemporaryLoginService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public AlternateTemporaryLoginService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<BaseResponse> SendAlternateTemporaryLoginLink(AlternateTemporaryLoginRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<AlternateTemporaryLoginRequestModel,BaseResponse>("AlternateTemporaryLogin/send-alternate-temporary-login-link", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
    }
}
