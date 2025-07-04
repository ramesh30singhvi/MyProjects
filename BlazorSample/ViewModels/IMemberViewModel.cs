using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IMemberViewModel
    {
        Task<MembersViewModel> GetMembers();
        Task<List<ReservationsViewModel>> GetReservations(int memberId, Guid userId);
        Task<List<MemberReviewsViewModel>> GetMemberReviews(int memberId, Guid userId);
    }
}
