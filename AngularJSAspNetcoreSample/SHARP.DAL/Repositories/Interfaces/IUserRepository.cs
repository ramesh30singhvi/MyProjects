using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        public User[] GetUsers(string name, IEnumerable<string> userIds, string state);

        Task<User> GetAsync(string userId);

        Task<User> GetUserWithOrganizationsAsync(int userId);

        Task<User> GetUserWithOrganizationsAsync(string userId);

        Task<User[]> GetAsync(UserFilter filter, Expression<Func<User, object>> orderBySelector);

        Task<List<FilterOptionQueryModel>> GetDistinctColumnAsync(
            UserFilterColumnSource<UserColumn> columnData,
            ColumnOptionQueryRule<User, FilterOptionQueryModel> columnQueryRule);

        Task<List<FilterOptionQueryModel>> GetDistinctAccessColumnAsync(UserFilterColumnSource<UserColumn> columnData);

        Task<List<FilterOptionQueryModel>> GetDistinctFacilityAccessColumnAsync(UserFilterColumnSource<UserColumn> columnData);

        Task<User> GetUserAsync(int id);

        Task<string> GetUserTimeZone(int userId);

        Task<User[]> GetUsersActiveForAllOrganizaitonsAsync();
        Task<User> GetUserByEmailAsync(string email);
    }
}
