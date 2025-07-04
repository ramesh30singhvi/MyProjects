using Blazored.LocalStorage;
using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class MemberService : IMemberService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IApiManager _apiManager;

        public MemberService(IApiManager apiManager, ILocalStorageService localStorage)
        {
            _apiManager = apiManager;
            _localStorage = localStorage;
        }

        public async Task<MembersViewModel> GetMembersAsync(int userId)
        {
            try
            {
                MembersViewModel members = await _apiManager.GetAsync<MembersViewModel>("member/list");
                if (members.DefaultMember == 0)
                {
                    members.DefaultMember = 26;
                }
                var currentMember = members.Members.FirstOrDefault(x => x.Id == members.DefaultMember);
                if (members != null)
                {
                    await _localStorage.SetItemAsync("members", members.Members);
                    await _localStorage.SetItemAsync("currentMember", currentMember);
                }
                return members;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new MembersViewModel();
            }
        }

        public async Task<List<ReservationsViewModel>> GetReservationListAsync(int memberId, Guid userId)
        {
            try
            {
                List<ReservationsViewModel> reservations = await _apiManager.GetAsync<List<ReservationsViewModel>>("member/member-reservations/" + memberId + "/" + userId);
                return reservations;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<ReservationsViewModel>();
            }
        }

        public async Task<List<MemberReviewsViewModel>> GetMemberReviewsAsync(int memberId, Guid userId)
        {
            try
            {
                List<MemberReviewsViewModel> reviews = await _apiManager.GetAsync<List<MemberReviewsViewModel>>("member/member-reviews/" + memberId + "/" + userId);
                return reviews;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<MemberReviewsViewModel>();
            }
        }
    }
}
