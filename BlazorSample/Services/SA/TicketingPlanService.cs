using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
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
    public class TicketingPlanService : ITicketingPlanService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public TicketingPlanService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<TicketingPlanResponse> AddUpdateTicketingPlan(TicketingPlanRequestModel requestModel)
        {
            try
            {
                return await _apiManager.PostAsync<TicketingPlanResponse>("TicketingPlan/add-update-ticketing-plan", requestModel);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new TicketingPlanResponse();
            }
        }
        public async Task<TicketingPlanListResponse> GetTicketingPlanList()
        {
            try
            {
                return await _apiManager.GetAsync<TicketingPlanListResponse>("TicketingPlan/get-ticketing-plans");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new TicketingPlanListResponse();
            }
        }
        public async Task<TicketingPlanResponse> GetTicketingPlanById(Guid idGuid)
        {
            try
            {
                return await _apiManager.GetAsync<TicketingPlanResponse>("TicketingPlan/get-ticketing-plan/" + idGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new TicketingPlanResponse();
            }
        }
        public async Task<BaseResponse> DeleteTicketingPlan(Guid idGuid)
        {
            try
            {
                return await _apiManager.DeleteAsync<BaseResponse>("TicketingPlan/delete-ticketing-plan/" + idGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }      
    }
}
