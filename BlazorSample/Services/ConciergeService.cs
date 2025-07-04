using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel;
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
    public class ConciergeService : IConciergeService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public ConciergeService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<ConciergeListResponse> GetCPConcierges(int userId)
        {
            try
            {
                ConciergeListResponse response = await _apiManager.GetAsync<ConciergeListResponse>(_configuration["App:PeopleApiUrl"] + "Concierge/list");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ConciergeListResponse();
            }
        }

        public async Task<ConciergeDetailResponse> GetCPConciergeById(Guid ConciergeGuid)
        {
            try
            {
                ConciergeDetailResponse response = await _apiManager.GetAsync<ConciergeDetailResponse>(_configuration["App:PeopleApiUrl"] + "Concierge/detail/" + ConciergeGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ConciergeDetailResponse();
            }
        }
        public async Task<ConciergeDetailResponse> AddUpdateCPConcierge(ConciergeRequestModel request)
        {
            try
            {
                ConciergeDetailResponse response = await _apiManager.PostAsync<ConciergeRequestModel, ConciergeDetailResponse>(_configuration["App:PeopleApiUrl"] + "Concierge/add-update-concierge", request);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ConciergeDetailResponse();
            }
        }
    }
}
