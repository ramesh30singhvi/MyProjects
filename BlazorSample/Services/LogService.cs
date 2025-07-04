using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class LogService : ILogService
    {
        private readonly IApiManager _apiManager;

        public LogService(IApiManager apiManager)
        {
            _apiManager = apiManager;
        }

        public Task CreateCosmosClientAsync() { return null; }

        public async Task<List<LogModel>> GetLogAsync()
        {
            try
            {
                List<LogModel> logs = await _apiManager.GetAsync<List<LogModel>>("log/get");
                return logs;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<LogModel>();
            }
        }
    }
}
