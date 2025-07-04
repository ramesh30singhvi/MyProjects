using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class LogViewModel : ILogViewModel
    {
        private ILogService _logService;
        public LogViewModel(ILogService logService)
        {
            _logService = logService;
        }

        public async Task<List<LogModel>> GetLogs()
        {
            List<LogModel> logs = await _logService.GetLogAsync();
            return logs;
        }
    }
}
