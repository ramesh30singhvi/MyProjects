using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class TeamViewModel : ITeamViewModel
    {
        private ITeamService _teamService;

        public TeamViewModel(ITeamService teamService)
        {
            _teamService = teamService;
        }

        public async Task<TeamResponse> GetBusinessTeams(int businessId)
        {
            TeamResponse response = await _teamService.GetBusinessTeams(businessId);
            return response;
        }
        public async Task<UserBusinessTeamResponse> GetUserBusinessTeams(int userId)
        {
            return await _teamService.GetUserBusinessTeams(userId);
        }

        public async Task<TeamDetailResponse> GetBusinessTeamDetail(Guid userGUID)
        {
            TeamDetailResponse response = await _teamService.GetBusinessTeamDetail(userGUID);
            return response;
        }

        public async Task<AddTeamResponse> AddBusinessTeam(AddTeamRequestModel request)
        {
            var response = await _teamService.AddBusinessTeam(request);
            return response;
        }

        public async Task<RemoveTeamMemberResponse> RemoveBusinessTeamMember(int teamId, int businessId)
        {
            var response = await _teamService.RemoveBusinessTeamMember(teamId, businessId);
            return response;
        }

        public async Task<int> GenerateUserPin(Guid userGUID)
        {
            return await _teamService.GenerateUserPin(userGUID);
        }
        public async Task<BaseResponse> SetDefaultBusinessTeam(int userId, int businessId)
        {
            return await _teamService.SetDefaultBusinessTeam(userId, businessId);
        }

        public async Task<TeamListByRoleResponse> GetBusinessTeamByRole(int businessId, int role)
        {
            return await _teamService.GetBusinessTeamByRole(businessId, role);
        }

        public async Task<ExportTeamMemberListResponse> ExportTeamMembers(int businessId)
        {
            ExportTeamMemberListResponse response = await _teamService.ExportTeamMembers(businessId);
            return response;
        }

        #region Invite User
        public async Task<InviteUserResponse> AddUpdateInviteUser(InviteUserRequestModel requestModel)
        {
            return await _teamService.AddUpdateInviteUser(requestModel);
        }
        #endregion Invite User
    }
}
