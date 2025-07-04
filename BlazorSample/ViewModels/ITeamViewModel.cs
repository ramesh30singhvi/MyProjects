using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ITeamViewModel
    {
        Task<TeamResponse> GetBusinessTeams(int businessId);
        Task<UserBusinessTeamResponse> GetUserBusinessTeams(int userId);
        Task<TeamDetailResponse> GetBusinessTeamDetail(Guid userGUID);
        Task<AddTeamResponse> AddBusinessTeam(AddTeamRequestModel request);
        Task<RemoveTeamMemberResponse> RemoveBusinessTeamMember(int teamId, int businessId);
        Task<int> GenerateUserPin(Guid userGUID);
        Task<BaseResponse> SetDefaultBusinessTeam(int userId, int businessId);
        Task<TeamListByRoleResponse> GetBusinessTeamByRole(int businessId, int role);
        Task<ExportTeamMemberListResponse> ExportTeamMembers(int businessId);
        Task<InviteUserResponse> AddUpdateInviteUser(InviteUserRequestModel requestModel);
    }
}
