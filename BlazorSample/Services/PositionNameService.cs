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
    public class PositionNameService : IPositionNameService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public PositionNameService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<CPPositionNameListResponse> GetPositionNameAsync(int? id)
        {
            try
            {
                return await _apiManager.GetAsync<CPPositionNameListResponse>(_configuration["App:PeopleApiUrl"] + "PositionName/get-positionNames?id=" + id);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPPositionNameListResponse();
            }
        }       
    }
}
