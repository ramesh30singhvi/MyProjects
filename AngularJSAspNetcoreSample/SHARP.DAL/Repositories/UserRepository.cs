using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using SHARP.DAL.Extensions;
using SHARP.DAL.Models.QueryModels;
using SHARP.Common.Filtration.Enums;
using SHARP.Common.Enums;

namespace SHARP.DAL.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(SHARPContext context) : base(context)
        {
        }

        public User[] GetUsers(string name, IEnumerable<string> userIds, string state)
        {
            var users = _context.User.Where(user =>
                userIds.Contains(user.UserId));

            if (!string.IsNullOrWhiteSpace(name))
            {
                users = users.Where(user => user.FirstName.Contains(name) || user.LastName.Contains(name));
            }

            return users.ToArray();
        }

        public async Task<User> GetAsync(string userId)
        {
            return await _context.User.FirstOrDefaultAsync(user => user.UserId == userId);
        }

        public async Task<User> GetUserWithOrganizationsAsync(int userId)
        {
            return await _context.User
                .Include(user => user.UserOrganizations)
                    .ThenInclude(userOrg => userOrg.Organization)
                .Include(user => user.UserFacilities)
                    .ThenInclude(userFac => userFac.Facility)
                .FirstOrDefaultAsync(user => user.Id == userId);
        }

        public async Task<User> GetUserWithOrganizationsAsync(string userId)
        {
            return await _context.User
                .Include(user => user.UserOrganizations)
                    .ThenInclude(userOrg => userOrg.Organization)
                .Include(user => user.UserFacilities)
                    .ThenInclude(userFac => userFac.Facility)
                //.Include(user => user.UserTeams)
                //    .ThenInclude(uTeam => uTeam.Team)
                .FirstOrDefaultAsync(user => user.UserId == userId);
        }

        public async Task<User[]> GetAsync(UserFilter filter, Expression<Func<User, object>> orderBySelector)
        {
            var users = GetUserQuery()
                .Where(BuildFiltersCriteria(filter));

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                //var isOrganizationMatch = _context.Organization
                //    .Any(organization => organization.Name.Contains(filter.Search));

                //var isFacilityMatch = _context.Facility
                //    .Any(facility => facility.Name.Contains(filter.Search));

                users = users.Where(user =>
                    user.FullName.Contains(filter.Search)
                    || user.Email.Contains(filter.Search)
                    //|| user.UserOrganizations
                    //        .Any(userOrganization => userOrganization.Organization.Name.Contains(filter.Search))
                    //|| (!user.UserOrganizations.Any() && isOrganizationMatch)
                    //|| user.UserFacilities
                    //        .Any(userFacility => userFacility.Facility.Name.Contains(filter.Search))
                    //|| (!user.UserFacilities.Any() && isFacilityMatch)
                    );   //Commented above search for ticket ACS-139
            }

            return await users
                .QueryOrderBy(orderBySelector, filter.SortOrder)
                .GetPagedAsync(filter.SkipCount, filter.TakeCount);
        }

        public async Task<List<FilterOptionQueryModel>> GetDistinctColumnAsync(
            UserFilterColumnSource<UserColumn> columnData, 
            ColumnOptionQueryRule<User, FilterOptionQueryModel> columnQueryRule)
        {
            var users = GetUserQuery()
                .Where(BuildFiltersCriteria(columnData.UserFilter, columnData.Column));

            IQueryable<FilterOptionQueryModel> columnValues;
            if (columnQueryRule.SingleSelector != null)
            {
                columnValues = users.Select(columnQueryRule.SingleSelector);
            }
            else
            {
                columnValues = users.SelectMany(columnQueryRule.ManySelector);
            }

            var result = await columnValues
                .Distinct()
                .ToArrayAsync();

            return result.OrderBy(i => i.Value).ToList();
        }

        public async Task<List<FilterOptionQueryModel>> GetDistinctAccessColumnAsync(UserFilterColumnSource<UserColumn> columnData)
        {
            var queryRule = new ColumnOptionQueryRule<User, FilterOptionQueryModel>
            {
                ManySelector = i => i.UserOrganizations.Select(userOrganization => new FilterOptionQueryModel() 
                { 
                    Id = userOrganization.OrganizationId, Value = userOrganization.Organization.Name 
                })
            };

            var columnValues = await GetDistinctColumnAsync(columnData, queryRule);

            var isUnlimited = GetUserQuery()
                .Where(BuildFiltersCriteria(columnData.UserFilter, columnData.Column))
                .Any(user => !user.UserOrganizations.Any()); //_context.User.Any(user => !user.UserOrganizations.Any());

            if (!isUnlimited)
            {
                return columnValues;
            }

            columnValues.Insert(0, new FilterOptionQueryModel()
            {
                Value = "All"
            });

            return columnValues;
        }

        public async Task<List<FilterOptionQueryModel>> GetDistinctFacilityAccessColumnAsync(UserFilterColumnSource<UserColumn> columnData)
        {
            var queryRule = new ColumnOptionQueryRule<User, FilterOptionQueryModel>
            {
                ManySelector = i => i.UserFacilities.Select(userFacility => new FilterOptionQueryModel()
                {
                    Id = userFacility.FacilityId,
                    Value = userFacility.Facility.Name
                })
            };

            var columnValues = await GetDistinctColumnAsync(columnData, queryRule);

            var isUnlimited = GetUserQuery()
                .Where(BuildFiltersCriteria(columnData.UserFilter, columnData.Column))
                .Any(user => !user.UserFacilities.Any()); //_context.User.Any(user => !user.UserOrganizations.Any());

            if (!isUnlimited)
            {
                return columnValues;
            }

            columnValues.Insert(0, new FilterOptionQueryModel()
            {
                Value = "All"
            });

            return columnValues;
        }

        public async Task<User> GetUserAsync(int id)
        {
            return await GetUserQuery()
                .FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<string> GetUserTimeZone(int userId)
        {
            var u = _context.User
                .Where(user => user.Id == userId);
            return await _context.User
                .Where(user => user.Id == userId)
                .Select(user => user.TimeZone)
                .FirstOrDefaultAsync();
        }
    
        private IQueryable<User> GetUserQuery()
        {
            return _context.User
                .Include(user => user.UserOrganizations)
                    .ThenInclude(userOrganization => userOrganization.Organization)
                .Include(user => user.UserFacilities)
                    .ThenInclude(userFacility => userFacility.Facility)
                .Include(user => user.UserTeams)
                    .ThenInclude(userT => userT.Team)
                .Include(shar => shar.IdentityUser)
                    .ThenInclude(identity => identity.UserRoles)
                        .ThenInclude(userRole => userRole.Role);
        }

        private Expression<Func<User, bool>> BuildFiltersCriteria(UserFilter filter, UserColumn? column = null)
        {
            Expression<Func<User, bool>> userExpr = PredicateBuilder.True<User>();

            if (filter == null)
            {
                return userExpr;
            }

            return PredicateBuilder
                .True<User>()
                .AndIf(GetNameExpression(filter), column != UserColumn.Name && filter.Name != null && filter.Name.Any())
                .AndIf(GetEmailExpression(filter), column != UserColumn.Email && filter.Email != null && filter.Email.Any())
                .AndIf(GetRoleExpression(filter), column != UserColumn.Roles && filter.Role != null && filter.Role.Any())
                .AndIf(GetOrganizationExpression(filter), column != UserColumn.Access && filter.Access != null && filter.Access.Any())
                .AndIf(GetSiteIdExpression(filter),true)
                .AndIf(GetFacilityExpression(filter), column != UserColumn.FacilityAccess && filter.FacilityAccess != null && filter.FacilityAccess.Any())
                .AndIf(GetStatusExpression(filter), column != UserColumn.Status && filter.Status != null && filter.Status.Any());
        }

        private Expression<Func<User, bool>> GetSiteIdExpression(UserFilter filter)
        {
            return i => filter.SiteId == i.SiteId;
        }

        private Expression<Func<User, bool>> GetNameExpression(UserFilter filter)
        {
            return i => filter.Name.Select(option => option.Value).Contains(i.FullName);
        }

        private Expression<Func<User, bool>> GetEmailExpression(UserFilter filter)
        {
            return i => filter.Email.Select(option => option.Value).Contains(i.Email);
        }

        private Expression<Func<User, bool>> GetRoleExpression(UserFilter filter)
        {
            return i => i.IdentityUser.UserRoles.Any(userRole => filter.Role.Select(option => option.Id.ToString()).Contains(userRole.RoleId));
        }

        private Expression<Func<User, bool>> GetOrganizationExpression(UserFilter filter)
        {
            Expression<Func<User, bool>> expr = i => i.UserOrganizations
                .Any(userOrganization => filter.Access.Select(option => option.Id).Contains(userOrganization.OrganizationId));

            if (filter.Access.Select(option => option.Id).Contains(null))
            {
                expr = expr.Or(i => !i.UserOrganizations.Any());
            }

            return expr;
        }

        private Expression<Func<User, bool>> GetFacilityExpression(UserFilter filter)
        {
            Expression<Func<User, bool>> expr = i => i.UserFacilities
                .Any(userFacility => filter.FacilityAccess.Select(option => option.Id).Contains(userFacility.FacilityId));

            if (filter.FacilityAccess.Select(option => option.Id).Contains(null))
            {
                expr = expr.Or(i => !i.UserFacilities.Any());
            }

            return expr;
        }

        private Expression<Func<User, bool>> GetStatusExpression(UserFilter filter)
        {
            return i => filter.Status.Select(option => (UserStatus)option.Id).Contains(i.Status);
        }
        public async Task<User[]> GetUsersActiveForAllOrganizaitonsAsync()
        {
            var users = GetUserQuery();
            users = users.Where( x => x.UserOrganizations.Count() == 0 && x.Status == UserStatus.Active && x.SiteId == 1 && !string.IsNullOrEmpty(x.FirstName) && !string.IsNullOrEmpty(x.LastName) );
            return users.ToArray();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.User
                .Include(user => user.UserOrganizations)
                    .ThenInclude(userOrg => userOrg.Organization)
                .Include(user => user.UserFacilities)
                    .ThenInclude(userFac => userFac.Facility)
                .FirstOrDefaultAsync(user => user.Email.ToLower() == email.ToLower());
        }
    }
}
