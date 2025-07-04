using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IStateViewModel
    {
        Task<StateResponse> GetStates();
    }
}
