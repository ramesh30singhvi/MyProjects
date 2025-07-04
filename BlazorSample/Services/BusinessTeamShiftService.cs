using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class BusinessTeamShiftService : IBusinessTeamShiftService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public BusinessTeamShiftService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<BusinessTeamShiftResponse> BusinessTeamShiftCheckIn(BusinessTeamShiftRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessTeamShiftResponse>(_configuration["App:PeopleApiUrl"] + "BusinessTeamShift/check-in/", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTeamShiftResponse();
            }
        }

        public async Task<BusinessTeamShiftListResponse> BusinessTeamShiftCheckOut(BusinessTeamShiftRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessTeamShiftListResponse>(_configuration["App:PeopleApiUrl"] + "BusinessTeamShift/check-out/", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTeamShiftListResponse();
            }
        }

        public async Task<BusinessTeamShiftResponse> BusinessTeamShiftSEndBreak(BusinessTeamShiftRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessTeamShiftResponse>(_configuration["App:PeopleApiUrl"] + "BusinessTeamShift/end-break/", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTeamShiftResponse();
            }
        }

        public async Task<BusinessTeamShiftResponse> BusinessTeamShiftStartBreak(BusinessTeamShiftRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessTeamShiftResponse>(_configuration["App:PeopleApiUrl"] + "BusinessTeamShift/start-break/", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTeamShiftResponse();
            }
        }
    }
}
