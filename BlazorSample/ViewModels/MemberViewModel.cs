using CellarPassAppAdmin.Shared.Services;
using System.Threading.Tasks;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Collections.Generic;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using System;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class MemberViewModel: IMemberViewModel
    {
        private IMemberService _memberService;
        public MemberViewModel(IMemberService memberService)
        {
            _memberService = memberService;
        }
        public async Task<MembersViewModel> GetMembers()
        {
            MembersViewModel members = await _memberService.GetMembersAsync(0);
            return members;
        }

        public async Task<List<ReservationsViewModel>> GetReservations(int memberId, Guid userId)
        {
            List<ReservationsViewModel> reservations = await _memberService.GetReservationListAsync(memberId, userId);
            return reservations;
        }

        public async Task<List<MemberReviewsViewModel>> GetMemberReviews(int memberId, Guid userId)
        {
            List<MemberReviewsViewModel> reviews = await _memberService.GetMemberReviewsAsync(memberId, userId);
            return reviews;
        }
    }
}
