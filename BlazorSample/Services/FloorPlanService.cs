using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Client.ViewModels;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class FloorPlanService : IFloorPlanService
    {
        private readonly IApiManager _apiManager;

        public FloorPlanService(IApiManager apiManager)
        {
            _apiManager = apiManager;
        }
        public async Task<List<FloorPlanModel>> GetFloorPlanAsync(int memberId, bool active_only)
        {
            try
            {
                List<FloorPlanModel> receiptSetting = await _apiManager.GetAsync<List<FloorPlanModel>>("FloorPlan/getfloorplan?memberId=" + memberId + "&active_only=" + active_only);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<FloorPlanModel>();
            }
        }
        public async Task<List<SeatingReportTagModel>> GetTagsAsync(int memberId)
        {
            try
            {
                List<SeatingReportTagModel> receiptSetting = await _apiManager.GetAsync<List<SeatingReportTagModel>>("FloorPlan/gettag?memberId=" + memberId);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<SeatingReportTagModel>();
            }
        }
    }
}
