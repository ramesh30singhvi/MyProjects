using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.Portal;
using SHARP.BusinessLogic.DTO.User;
using SHARP.BusinessLogic.DTO.UserActivity;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IUserService
    {
        public User GetUser(string userId);

        Task<User> GetUserAsync(string userId);

        Task<User> GetLoggedInUserAsync();

        public IEnumerable<User> GetUsers(string name, IEnumerable<string> userIds, string state);

        public Task<IEnumerable<UserDto>> GetUsersAsync(UserFilter filter);

        public Task<IReadOnlyCollection<FilterOption>> GetFilterColumnSourceDataAsync(
            UserFilterColumnSource<UserColumn> columnData);

        public Task<int> CreateAsync(CreateUserDto user);

        Task EditAsync(EditUserDto userDto);

        Task<UserDetailsDto> GetUserDetailsAsync(int id);

        Task<bool> AddUserActivityAsync(AddUserActivityDto userActivityDto);

        Task<ICollection<int>> GetUserOrganizationIdsAsync();

        Task<ICollection<int>> GetUserFacilityIdsAsync();

        Task<bool> DeleteUserAsync(int id);

        int? GetLoggedUserIdIfUserIsOnlyAuditor();

        int GetLoggedUserId();

        Task<UserOrganizationsDto> GetUserOrganizationsAsync();

        Task<bool> UpdateUserFacilitiesAsync(UserDetailsDto user);

        string GetCurrentUserTimeZone();

        Task<string> GetUserTimeZoneAsync(int userId);

        Task<byte[]> GetLogsExcel(string userIds, string FromDate, string ToDate, string baseUri);
        Task<IReadOnlyCollection<UserOptionDto>> GetUserOptionsAsync(int organizationId);


        Task CreatePortalUserFromPortalAsync(CreatePortalUserDto user);
        Task CreatePortalUserAsync(CreatePortalUserDto user);

        Task CreatePortalDBUserAsync(CreatePortalUserDto dto, string id);

        Task ExportFacilityRecipientToPortalUser(IEmailClient emailclient);
        Task<IReadOnlyCollection<FacilityOptionDto>> GetUserFacilitiesAsync(int userId);
        Task ChangePassword(string userEmail, string password);
        Task SetUserStatus(int id,UserStatus status);

        Task<UserDetailsDto> GetUserByEmail(string email);
        Task<UserDetailsDto> CreateUserFromEmail(string email,string password , int organizationId, int facilityId);
        string GeneratePassword();

        Task<bool> AddUpdateLoginsTrackingAsync(int userId, string type);
        Task<Tuple<IReadOnlyCollection<LoginsTrackingDto>, int>> GetPortalLoginsTrackingAsync(PortalLoginsTrackingViewFilter filter);
        Task<IReadOnlyCollection<TeamDto>> GetTeamsAsync();
        Task PortalUserEditAsync(EditUserDto userDto);
    }
}
