using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public interface ICellarPassAuthenticationStateProvider
    {
        Task MarkUserAsAuthenticated(UserDetailViewModel user);
        Task MarkUserAsLoggedOut();
        Task<Member> GetCurrentMemberAsync();
        void ChangeCurrentMember();
        event Action NotifyUICurrentMemberChange;
    }
}
