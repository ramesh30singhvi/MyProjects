using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class TeamService : ITeamService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public TeamService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<TeamResponse> GetBusinessTeams(int businessId)
        {
            try
            {
                TeamResponse response = await _apiManager.GetAsync<TeamResponse>(_configuration["App:PeopleApiUrl"] + "Team/list/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new TeamResponse();
            }
        }

        public async Task<UserBusinessTeamResponse> GetUserBusinessTeams(int userId)
        {
            try
            {
                return await _apiManager.GetAsync<UserBusinessTeamResponse>(_configuration["App:PeopleApiUrl"] + "Team/get-user-Business-teams/" + userId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserBusinessTeamResponse();
            }
        }

        public async Task<TeamDetailResponse> GetBusinessTeamDetail(Guid userGUID)
        {
            try
            {
                TeamDetailResponse response = await _apiManager.GetAsync<TeamDetailResponse>(_configuration["App:PeopleApiUrl"] + "Team/detail/" + userGUID);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new TeamDetailResponse();
            }
        }

        public async Task<AddTeamResponse> AddBusinessTeam(AddTeamRequestModel request)
        {
            try
            {
                AddTeamResponse response = await _apiManager.PostAsync<AddTeamRequestModel, AddTeamResponse>(_configuration["App:PeopleApiUrl"] + "Team/add-business-team", request);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddTeamResponse();
            }
        }

        public async Task<RemoveTeamMemberResponse> RemoveBusinessTeamMember(int teamId, int businessId)
        {
            try
            {
                RemoveTeamMemberResponse response = await _apiManager.DeleteAsync<RemoveTeamMemberResponse>(_configuration["App:PeopleApiUrl"] + "Team/remove-business-team-member/" + teamId + "/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new RemoveTeamMemberResponse();
            }
        }

        public async Task<int> GenerateUserPin(Guid userGUID)
        {
            try
            {
                int response = await _apiManager.GetAsync<int>(_configuration["App:PeopleApiUrl"] + "Team/generate-pin/" + userGUID);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new int();
            }
        }
        public async Task<BaseResponse> SetDefaultBusinessTeam(int userId, int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "Team/set-default-business-team/" + userId + "/" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<TeamListByRoleResponse> GetBusinessTeamByRole(int businessId, int role)
        {
            try
            {
                return await _apiManager.GetAsync<TeamListByRoleResponse>(_configuration["App:PeopleApiUrl"] + $"Team/get-business-team-by-role?businessId={businessId}&role={role}");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new TeamListByRoleResponse();
            }
        }

        public async Task<ExportTeamMemberListResponse> ExportTeamMembers(int businessId)
        {
            try
            {
                ExportTeamMemberListResponse response = await _apiManager.GetAsync<ExportTeamMemberListResponse>(_configuration["App:PeopleApiUrl"] + "Team/export-team-members/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ExportTeamMemberListResponse();
            }
        }
        #region Invite User
        public async Task<InviteUserResponse> AddUpdateInviteUser(InviteUserRequestModel requestModel)
        {
            try
            {
                return await _apiManager.PostAsync<InviteUserResponse>(_configuration["App:PeopleApiUrl"] + "Team/add-update-invite-user", requestModel);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new InviteUserResponse();
            }
        }
        public async Task<InviteUserDetailResponse> GetInviteUserDetailById(Guid idGuid)
        {
            try
            {
                return await _apiManager.GetAsync<InviteUserDetailResponse>(_configuration["App:PeopleApiUrl"] + "Team/get-invite-user-detail/" + idGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new InviteUserDetailResponse();
            }
        }
        #endregion Invite User
    }
}
