using CellarPassAppAdmin.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ILogViewModel
    {
        Task<List<LogModel>> GetLogs();
    }
}
