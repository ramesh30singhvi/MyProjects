using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface ITableauServerService
    {
        Task<string> GetTableauAuthenticationTicketAsync();
    }
}
