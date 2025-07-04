using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.User;
using SHARP.BusinessLogic.DTO.UserActivity;
using SHARP.BusinessLogic.Extensions;
using SHARP.BusinessLogic.Helpers;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using ClosedXML.Excel;
using SHARP.Common.Helpers;

using SHARP.BusinessLogic.DTO.Dashboard;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Azure.Amqp.Framing;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Security.Principal;
using User = SHARP.DAL.Models.User;
using SHARP.BusinessLogic.DTO.Facility;

using SHARP.BusinessLogic.DTO.Portal;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;

namespace SHARP.BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRoleService _roleService;
        private readonly IConfiguration _configuration;
    

        public UserService(
            IUnitOfWork unitOfWork, IConfiguration configuration,
            IMapper mapper, IRoleService roleService,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _roleService = roleService;
            _configuration = configuration;

        }

        public async Task<IReadOnlyCollection<FilterOption>> GetFilterColumnSourceDataAsync(UserFilterColumnSource<UserColumn> columnData)
        {
            if (columnData.Column == UserColumn.Access)
            {
                var organizations = await _unitOfWork.UserRepository.GetDistinctAccessColumnAsync(columnData);

                return _mapper.Map<IReadOnlyCollection<FilterOption>>(organizations);
            }
            else if (columnData.Column == UserColumn.FacilityAccess)
            {
                var facilities = await _unitOfWork.UserRepository.GetDistinctFacilityAccessColumnAsync(columnData);

                return _mapper.Map<IReadOnlyCollection<FilterOption>>(facilities);
            }

            var queryRule = GetColumnQueryRule(columnData.Column);

            var columnValues = await _unitOfWork.UserRepository.GetDistinctColumnAsync(columnData, queryRule);

            return _mapper.Map<IReadOnlyCollection<FilterOption>>(columnValues);
        }

        public IEnumerable<User> GetUsers(string name, IEnumerable<string> userIds, string state) =>
            _unitOfWork.UserRepository.GetUsers(name, userIds, state);

        public async Task<IEnumerable<UserDto>> GetUsersAsync(UserFilter filter)
        {
            var orderBySelector = OrderByHelper.GetOrderBySelector<UserColumn, Expression<Func<User, object>>>(
                    filter.OrderBy,
                    GetOrderBySelector);
            var sharUsers = await _unitOfWork.UserRepository.GetAsync(filter, orderBySelector);

            var identityUserIds = sharUsers.Select(user => user.UserId);
            var identityUsers = (await _unitOfWork.AspNetUserRepository
                .GetAsync(identityUserIds))
                .ToDictionary(user => user.Id);

            return sharUsers.Select(shar =>
            {
                var dto = _mapper.Map<UserDto>(shar);
                dto.Roles = identityUsers[shar.UserId].UserRoles.Select(userRole => userRole.Role.Name);
                return dto;
            });
        }

        User IUserService.GetUser(string userId)
        {
            return _unitOfWork.UserRepository.Find(a => a.UserId == userId).SingleOrDefault();
        }

        async Task<User> IUserService.GetUserAsync(string userId)
        {
            return await _unitOfWork.UserRepository.GetUserWithOrganizationsAsync(userId);
        }

        private ColumnOptionQueryRule<User, FilterOptionQueryModel> GetColumnQueryRule(UserColumn columnName)
        {
            var columnQueryRuleMap = new Dictionary<UserColumn, ColumnOptionQueryRule<User, FilterOptionQueryModel>>
            {
                {
                    UserColumn.Name,
                    new ColumnOptionQueryRule<User, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.FullName }
                    }
                },
                {
                    UserColumn.Email,
                    new ColumnOptionQueryRule<User, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.Email }
                    }
                },
                {
                    UserColumn.Roles,
                    new ColumnOptionQueryRule<User, FilterOptionQueryModel>
                    {
                        ManySelector = i => i.IdentityUser.UserRoles
                        .Where(userRole =>
                            userRole.Role.Name != UserRoles.AutoTest.ToString() &&
                            userRole.Role.Name != UserRoles.Facility.ToString())
                        .Select(userRole => new FilterOptionQueryModel { Id = int.Parse(userRole.RoleId), Value = userRole.Role.Name })
                    }
                },
                {
                    UserColumn.Status,
                    new ColumnOptionQueryRule<User, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = (int)i.Status, Value = i.Status.ToString() }
                    }
                }
            };

            if (!columnQueryRuleMap.TryGetValue(columnName, out var queryRule))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return queryRule;
        }

        private Expression<Func<User, object>> GetOrderBySelector(UserColumn columnName)
        {
            var columnSelectorMap = new Dictionary<UserColumn, Expression<Func<User, object>>>
            {
                {
                    UserColumn.Name,
                    i => i.FullName
                },
                {
                    UserColumn.Email,
                    i => i.Email
                },
                {
                    UserColumn.Roles,
                    i => SqlFunctions.RoleOrderValue(i.UserId)
                },
                {
                    UserColumn.Access,
                    i => SqlFunctions.OrganizationOrderValue(i.Id)
                },
                {
                    UserColumn.Status,
                    i => i.Status
                }
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var selector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return selector;
        }

        public async Task<int> CreateAsync(CreateUserDto dto)
        {
            var identity = _mapper.Map<ApplicationUser>(dto);
            var identityResult = await _userManager.CreateAsync(identity, dto.Password);
            if (!identityResult.Succeeded)
            {
                var error = identityResult.Errors.First();
                throw new Exception(error.Description);
            }

            var shar = _mapper.Map<User>(dto);
            shar.UserId = identity.Id;
            shar.SiteId = 1;
            _unitOfWork.UserRepository.Add(shar);
            await _unitOfWork.SaveChangesAsync();

            return shar.Id;
        }

        public async Task EditAsync(EditUserDto userDto)
        {
            User user = await _unitOfWork.UserRepository.GetUserAsync(userDto.Id);

            if (user is null)
            {
                throw new NotFoundException("User is not found");
            }

            ApplicationUser applicationUser = await _userManager.FindByIdAsync(user.UserId);

            _mapper.Map(userDto, applicationUser);

            var identityResult = await _userManager.UpdateAsync(applicationUser);
            if (!identityResult.Succeeded)
            {
                var error = identityResult.Errors.First();
                throw new Exception(error.Description);
            }

            if (!string.IsNullOrEmpty(userDto.Password))
            {
                var result = await _userManager.RemovePasswordAsync(applicationUser);
                if (!result.Succeeded)
                {
                    var error = result.Errors.First();
                    throw new Exception(error.Description);
                }

                result = await _userManager.AddPasswordAsync(applicationUser, userDto.Password);
                if (!result.Succeeded)
                {
                    var error = result.Errors.First();
                    throw new Exception(error.Description);
                }
            }

            _mapper.Map(userDto, user);
            if (string.IsNullOrEmpty(user.FirstName))
                user.FirstName = "";
            if (string.IsNullOrEmpty(user.LastName))
                user.LastName = "";
            _unitOfWork.UserRepository.Update(user);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<UserDetailsDto> GetUserDetailsAsync(int id)
        {
            User user = await _unitOfWork.UserRepository.GetUserAsync(id);
            return _mapper.Map<UserDetailsDto>(user);
        }

        public async Task<bool> AddUserActivityAsync(AddUserActivityDto userActivityDto)
        {
            UserActivity userActivity = _mapper.Map<UserActivity>(userActivityDto);

            userActivity.ActionTime = DateTime.UtcNow;

            userActivity.UserId = userActivity.UserId == 0 ? null : userActivity.UserId;

            await _unitOfWork.UserActivityRepository.AddAsync(userActivity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        public async Task<User> GetLoggedInUserAsync()
        {
            int userId = GetLoggedUserId();
            User user = await _unitOfWork.UserRepository.GetAsync(userId);
            if (user is null)
            {
                throw new NotFoundException("User is not found");
            }
            return user;
        }
        public async Task<ICollection<int>> GetUserOrganizationIdsAsync()
        {
            string[] roles = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)?.Select(claim => claim.Value).ToArray();

            if (roles.Count() == 1 && roles[0] == UserRoles.Admin.ToString())
            {
                return new List<int>();
            }

            int userId = GetLoggedUserId();

            User user = await _unitOfWork.UserRepository.GetUserWithOrganizationsAsync(userId);

            if (user is null)
            {
                throw new NotFoundException("User is not found");
            }

            return user.UserOrganizations.Select(userOrg => userOrg.OrganizationId).ToList();
        }

        public async Task<ICollection<int>> GetUserFacilityIdsAsync()
        {
            string[] roles = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)?.Select(claim => claim.Value).ToArray();

            int userId = GetLoggedUserId();
            User user = await _unitOfWork.UserRepository.GetUserWithOrganizationsAsync(userId);

            if (roles.Count() == 1 && roles[0] == UserRoles.Facility.ToString())
            {
                if (user is null)
                {
                    throw new NotFoundException("User is not found");
                }

                return user.UserFacilities.Select(userFacility => userFacility.FacilityId).ToList();
            }

            return new List<int>();
        }


        public async Task<UserOrganizationsDto> GetUserOrganizationsAsync()
        {
            int userId = GetLoggedUserId();

            User user = await _unitOfWork.UserRepository.GetUserWithOrganizationsAsync(userId);

            if (user is null)
            {
                throw new NotFoundException("User is not found");
            }

            string[] roles = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)?.Select(claim => claim.Value).ToArray();

            if (roles.Count() == 1 && roles[0] == UserRoles.Admin.ToString() || !user.UserOrganizations.Any())
            {
                IReadOnlyCollection<Organization> organizations = await _unitOfWork.OrganizationRepository.GetListAsync(
                orderBySelector: organization => organization.Name,
                asNoTracking: true);

                return new UserOrganizationsDto() { Organizations = _mapper.Map<IReadOnlyCollection<OptionDto>>(organizations) };
            }

            return new UserOrganizationsDto()
            {
                Organizations = _mapper.Map<IReadOnlyCollection<OptionDto>>(user.UserOrganizations.Select(userOrg => userOrg.Organization).OrderBy(org => org.Name)),
                FilteredByUserId = userId
            };
        }

        public int? GetLoggedUserIdIfUserIsOnlyAuditor()
        {
            string[] roles = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)?.Select(claim => claim.Value).ToArray();

            if (!(roles.Count() == 1 && roles[0] == UserRoles.Auditor.ToString()))
            {
                return null;
            }

            return GetLoggedUserId();
        }

        public int GetLoggedUserId()
        {
            if (!int.TryParse(_httpContextAccessor.HttpContext.User.FindFirstValue(CommonConstants.DB_USER_ID), out int userId))
            {
                throw new NotFoundException($"{CommonConstants.DB_USER_ID} is not found");
            }

            return userId;
        }

        public string GetCurrentUserTimeZone()
        {
            return GetUserTimeZoneAsync(GetLoggedUserId()).Result;
        }

        public async Task<string> GetUserTimeZoneAsync(int userId)
        {
            return await _unitOfWork.UserRepository.GetUserTimeZone(userId);
        }

        //For AutoTests
        public async Task<bool> DeleteUserAsync(int id)
        {
            User user = await _unitOfWork.UserRepository.GetAll()
                .Include(user => user.Audits)
                .FirstOrDefaultAsync(user => user.Id == id);

            if (user is null)
            {
                throw new NotFoundException($"User with Id: {id} is not found");
            }

            ApplicationUser applicationUser = await _userManager.FindByIdAsync(user.UserId);

            foreach (var audit in user.Audits)
            {
                _unitOfWork.AuditRepository.Remove(audit);
            }

            _unitOfWork.UserRepository.Remove(user);
            await _userManager.DeleteAsync(applicationUser);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<byte[]> GetLogsExcel(string userIds, string FromDate, string ToDate, string baseUri)
        {
            var userActivitiesModel = _unitOfWork.UserActivityRepository.GetUserActivities(userIds: userIds, fromDate: FromDate, toDate: ToDate);

            var workbook = new XLWorkbook();
            //Sheet1
            {
                var ws = workbook.Worksheets.Add("Productivity Log");

                var currentLine = 1;
                ws.Cell(currentLine, 1).Value = "User";
                ws.Cell(currentLine, 2).Value = "Action Type";
                ws.Cell(currentLine, 3).Value = "Audit";
                ws.Cell(currentLine, 4).Value = "Audit Type";
                ws.Cell(currentLine, 5).Value = "Audit Name";
                ws.Cell(currentLine, 6).Value = "Date & Time";
                ws.Cell(currentLine, 7).Value = "Elapsed Time";
                var range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 7));
                range.Style.Font.Bold = true;
                range.Style.Font.FontSize = 12;
                range.Style.Fill.BackgroundColor = XLColor.FromHtml("#145196");
                range.Style.Font.FontColor = XLColor.White;


                DateTime? previousDate = null;

                foreach (var act in userActivitiesModel.UserActivity.Where(x => x.ActionType != ActionType.NewAccount && x.ActionType != ActionType.RoleChange && x.ActionType != ActionType.OrganizationChange && x.ActionType != ActionType.InactiveChange))
                {
                    currentLine++;
                    TimeSpan? timePassed = null;
                    if (previousDate != null)
                    {

                        DateTime _prevDate = (DateTime)previousDate;

                        if (_prevDate.Day != act.ActionTime.Day)
                        {
                            previousDate = null;
                            timePassed = null;
                        }
                        else
                        {
                            TimeSpan ts = act.ActionTime.Subtract(_prevDate);
                            timePassed = ts;
                        }

                    }

                    ws.Cell(currentLine, 1).Value = string.IsNullOrEmpty(act.User.FullName) ? act.LoginUsername : act.User.FullName;
                    ws.Cell(currentLine, 2).Value = act.ActionType.ToString();
                    if (act.AuditId != null)
                    {
                        var link = $"{baseUri}{act.AuditId}";
                        ws.Cell(currentLine, 3).Value = link;
                        ws.Cell(currentLine, 3).SetHyperlink(new XLHyperlink(@link));
                        ws.Cell(currentLine, 4).Value = act.Audit.FormVersion.Form.AuditType.Name;
                        ws.Cell(currentLine, 5).Value = act.Audit.FormVersion.Form.Name;
                    }

                    ws.Cell(currentLine, 6).Value = DateHelpers.ConvertDateTimeToDateTimeWithTimeZone(act.ActionTime, this.GetCurrentUserTimeZone());
                    ws.Cell(currentLine, 6).Style.DateFormat.Format = "dddd, dd MMMM yyyy HH:mm:ss";
                    if (timePassed != null)
                    {
                        ws.Cell(currentLine, 7).Value = timePassed;
                    }


                    previousDate = act.ActionTime;
                }
                ws.Columns().AdjustToContents();
            }

            //Sheet2
            {
                var ws = workbook.Worksheets.Add("Active Users");

                var currentLine = 1;
                ws.Cell(currentLine, 1).Value = "Id";
                ws.Cell(currentLine, 2).Value = "First Name";
                ws.Cell(currentLine, 3).Value = "Last Name";
                ws.Cell(currentLine, 4).Value = "Email";
                ws.Cell(currentLine, 5).Value = "FullName";
                ws.Cell(currentLine, 6).Value = "TimeZone";
                var range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 6));
                range.Style.Font.Bold = true;
                range.Style.Font.FontSize = 12;
                range.Style.Fill.BackgroundColor = XLColor.FromHtml("#145196");
                range.Style.Font.FontColor = XLColor.White;

                DateTime? previousDate = null;

                foreach (var user in userActivitiesModel.User)
                {
                    currentLine++;
                    ws.Cell(currentLine, 1).Value = user.Id.ToString();
                    ws.Cell(currentLine, 2).Value = user.FirstName;
                    ws.Cell(currentLine, 3).Value = user.LastName;
                    ws.Cell(currentLine, 4).Value = user.Email;
                    ws.Cell(currentLine, 5).Value = user.FullName;
                    ws.Cell(currentLine, 6).Value = user.TimeZone;
                }
                ws.Columns().AdjustToContents();
            }

            //Sheet3
            {
                var ws = workbook.Worksheets.Add("New Accounts");

                var currentLine = 1;
                ws.Cell(currentLine, 1).Value = "User";
                ws.Cell(currentLine, 2).Value = "New UserId";
                ws.Cell(currentLine, 3).Value = "New User";
                ws.Cell(currentLine, 4).Value = "Date & Time";
                var range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 4));
                range.Style.Font.Bold = true;
                range.Style.Font.FontSize = 12;
                range.Style.Fill.BackgroundColor = XLColor.FromHtml("#145196");
                range.Style.Font.FontColor = XLColor.White;

                foreach (var act in userActivitiesModel.UserActivity.Where(x => x.ActionType == ActionType.NewAccount))
                {
                    currentLine++;
                    ws.Cell(currentLine, 1).Value = act.User.FullName;
                    ws.Cell(currentLine, 2).Value = act.UpdatedUserId.ToString();
                    ws.Cell(currentLine, 3).Value = act.UpdatedUser.FullName;
                    ws.Cell(currentLine, 4).Value = DateHelpers.ConvertDateTimeToDateTimeWithTimeZone(act.ActionTime, this.GetCurrentUserTimeZone());
                    ws.Cell(currentLine, 4).Style.DateFormat.Format = "dddd, dd MMMM yyyy HH:mm:ss";
                }
                ws.Columns().AdjustToContents();
            }

            //Sheet4
            {
                var ws = workbook.Worksheets.Add("User Modifications");

                var currentLine = 1;
                ws.Cell(currentLine, 1).Value = "User";
                ws.Cell(currentLine, 2).Value = "Action Type";
                ws.Cell(currentLine, 3).Value = "Updated User Id";
                ws.Cell(currentLine, 4).Value = "Updated User";
                ws.Cell(currentLine, 5).Value = "Date & Time";
                var range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 5));
                range.Style.Font.Bold = true;
                range.Style.Font.FontSize = 12;
                range.Style.Fill.BackgroundColor = XLColor.FromHtml("#145196");
                range.Style.Font.FontColor = XLColor.White;


                foreach (var act in userActivitiesModel.UserActivity.Where(x => x.ActionType == ActionType.RoleChange || x.ActionType == ActionType.OrganizationChange))
                {
                    currentLine++;

                    ws.Cell(currentLine, 1).Value = act.User.FullName;
                    ws.Cell(currentLine, 2).Value = act.ActionType.ToString();
                    ws.Cell(currentLine, 3).Value = act.UpdatedUserId.ToString();
                    ws.Cell(currentLine, 4).Value = act.UpdatedUser.FullName;
                    ws.Cell(currentLine, 5).Value = DateHelpers.ConvertDateTimeToDateTimeWithTimeZone(act.ActionTime, this.GetCurrentUserTimeZone());
                    ws.Cell(currentLine, 5).Style.DateFormat.Format = "dddd, dd MMMM yyyy HH:mm:ss";
                }
                ws.Columns().AdjustToContents();
            }

            //Sheet5
            {
                var ws = workbook.Worksheets.Add("Inactive Users");

                var currentLine = 1;
                ws.Cell(currentLine, 1).Value = "User";
                ws.Cell(currentLine, 2).Value = "Updated User Id";
                ws.Cell(currentLine, 3).Value = "Updated User";
                ws.Cell(currentLine, 4).Value = "Date & Time";

                var range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 4));
                range.Style.Font.Bold = true;
                range.Style.Font.FontSize = 12;
                range.Style.Fill.BackgroundColor = XLColor.FromHtml("#145196");
                range.Style.Font.FontColor = XLColor.White;


                foreach (var act in userActivitiesModel.UserActivity.Where(x => x.ActionType == ActionType.InactiveChange))
                {
                    currentLine++;

                    ws.Cell(currentLine, 1).Value = act.User.FullName;
                    ws.Cell(currentLine, 2).Value = act.UpdatedUserId.ToString();
                    ws.Cell(currentLine, 3).Value = act.UpdatedUser.FullName;
                    ws.Cell(currentLine, 4).Value = DateHelpers.ConvertDateTimeToDateTimeWithTimeZone(act.ActionTime, this.GetCurrentUserTimeZone());
                    ws.Cell(currentLine, 4).Style.DateFormat.Format = "dddd, dd MMMM yyyy HH:mm:ss";
                }
                ws.Columns().AdjustToContents();
            }

            //Sheet6
            {
                var ws = workbook.Worksheets.Add("Audit Log");

                var currentLine = 1;
                ws.Cell(currentLine, 1).Value = "User";
                ws.Cell(currentLine, 2).Value = "Audit Id";
                ws.Cell(currentLine, 3).Value = "Name";
                ws.Cell(currentLine, 4).Value = "Status";
                ws.Cell(currentLine, 5).Value = "Auditor's Time";
                ws.Cell(currentLine, 6).Value = "Duration";

                var range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 6));
                range.Style.Font.Bold = true;
                range.Style.Font.FontSize = 12;
                range.Style.Fill.BackgroundColor = XLColor.FromHtml("#145196");
                range.Style.Font.FontColor = XLColor.White;


                foreach (var auditLog in userActivitiesModel.UserAuditLog)
                {
                    currentLine++;

                    ws.Cell(currentLine, 1).Value = auditLog.SubmittedByUser;
                    ws.Cell(currentLine, 2).Value = auditLog.AuditId.ToString();
                    ws.Cell(currentLine, 3).Value = auditLog.FormName;
                    ws.Cell(currentLine, 4).Value = auditLog.Status.ToString();
                    ws.Cell(currentLine, 5).Value = auditLog.AuditorsTime;
                    ws.Cell(currentLine, 6).Value = auditLog.Duration;
                }
                ws.Columns().AdjustToContents();
            }

            //Sheet7
            {
                var ws = workbook.Worksheets.Add("User Audit Summary");

                var currentLine = 1;
                ws.Cell(currentLine, 1).Value = "User";
                ws.Cell(currentLine, 2).Value = "No. of audits for Approval";

                var range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 2));
                range.Style.Font.Bold = true;
                range.Style.Font.FontSize = 12;
                range.Style.Fill.BackgroundColor = XLColor.FromHtml("#145196");
                range.Style.Font.FontColor = XLColor.White;


                foreach (var userSummary in userActivitiesModel.UserSummary)
                {
                    currentLine++;

                    ws.Cell(currentLine, 1).Value = userSummary.FullName;
                    ws.Cell(currentLine, 2).Value = userSummary.SentForApprovalCount;
                }
                ws.Columns().AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public async Task<IReadOnlyCollection<UserOptionDto>> GetUserOptionsAsync(int organizationId)
        {
            IList<UserOptionDto> userOptionDtos = new List<UserOptionDto>();

            var users = await _unitOfWork.UserRepository.GetUsersActiveForAllOrganizaitonsAsync();

            var userOrganizations = _unitOfWork.UserOrganizationRepository.GetUsersForOrganization(organizationId).ToList();
            if (!userOrganizations.Any())
                return userOptionDtos.ToArray();

            foreach (var userOrganization in userOrganizations)
            {
                if (userOrganization.User?.SiteId != 1)
                    continue;

                var userOptionDto = _mapper.Map<UserOptionDto>(userOrganization);
                userOptionDtos.Add(userOptionDto);
            }

            foreach (var user in users)
            {
                var userOptionDto = _mapper.Map<UserOptionDto>(user);
                userOptionDtos.Add(userOptionDto);
            }
            return userOptionDtos.ToArray();
        }

        public async Task CreatePortalDBUserAsync(CreatePortalUserDto user, string id)
        {
            var organization = await _unitOfWork.OrganizationRepository.GetAsync<int>(user.Organization.Id);

            if (organization == null)
                throw new Exception("Organization does not exist");
            var facilities = new List<UserFacility>();
            string timezone = "";
            var dbfacilities = await _unitOfWork.FacilityRepository.GetListAsync(
                 facility => facility.OrganizationId == organization.Id && facility.Active,
                 facility => facility.Name,
                 include: facility => facility.TimeZone,
                 asNoTracking: true);


            foreach (var fac in user.Facilities)
            {
                var dbFacility = dbfacilities.FirstOrDefault(x => x.Id == fac?.Id);
                if (dbFacility != null)
                {
                    facilities.Add(new UserFacility() { FacilityId = dbFacility.Id });
                    if (string.IsNullOrEmpty(timezone))
                    {
                        timezone = dbFacility.TimeZone?.Name;
                    }
                }
            }

            var shar = new User();
            shar.FirstName = string.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName;
            shar.LastName = string.IsNullOrEmpty(user.LastName) ? "" : user.LastName;
            shar.UserId = id;
            shar.Status = UserStatus.Invited;
            shar.SiteId = 2;
            shar.Email = user.Email;
            shar.TimeZone = timezone;
            shar.Position = user.Position;
            shar.UserOrganizations = new List<UserOrganization>
            {
                new UserOrganization() { OrganizationId = organization.Id }
            };
            shar.UserFacilities = facilities;

            _unitOfWork.UserRepository.Add(shar);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task CreatePortalUserFromPortalAsync(CreatePortalUserDto user)
        {
            var identity = new ApplicationUser();
            identity.Email = user.Email;
            identity.UserName = user.Email;
            var userRoles = new List<ApplicationUserRole>();
            var role = new ApplicationUserRole();
            var roles = await _roleService.GetAsync();
            role.RoleId = user.Role.Id.ToString();
            userRoles.Add(role);
            identity.UserRoles = userRoles;
            identity.EmailConfirmed = true;
            identity.TwoFactorEnabled = true;
            var identityResult = await _userManager.CreateAsync(identity, user.Password);
            if (!identityResult.Succeeded)
            {
                var error = identityResult.Errors.First();
                throw new Exception(error.Description);
            }
            var organization = await _unitOfWork.OrganizationRepository.GetAsync<int>(user.Organization.Id);

            if (organization == null)
                throw new Exception("Organization does not exist");
            var facilities = new List<UserFacility>();
            string timezone = "";
            var dbfacilities = await _unitOfWork.FacilityRepository.GetListAsync(
                 facility => facility.OrganizationId == organization.Id && facility.Active,
                 facility => facility.Name,
                 include: facility => facility.TimeZone,
                 asNoTracking: true);


            foreach (var fac in user.Facilities)
            {
                var dbFacility = dbfacilities.FirstOrDefault(x => x.Id == fac?.Id);
                if (dbFacility != null)
                {
                    facilities.Add(new UserFacility() { FacilityId = dbFacility.Id });
                    if (string.IsNullOrEmpty(timezone))
                    {
                        timezone = dbFacility.TimeZone?.Name;
                    }
                }
            }

            var shar = new User();
            shar.FirstName = string.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName;
            shar.LastName = string.IsNullOrEmpty(user.LastName) ? "" : user.LastName;
            shar.UserId = identity.Id;
            shar.Status = UserStatus.Invited;
            shar.Position = user.Position;
            shar.SiteId = 2;
            shar.Email = user.Email;
            shar.TimeZone = timezone;
            shar.UserOrganizations = new List<UserOrganization>
            {
                new UserOrganization() { OrganizationId = organization.Id }
            };
            shar.UserFacilities = facilities;

            _unitOfWork.UserRepository.Add(shar);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task CreatePortalUserAsync(CreatePortalUserDto user)
        {
            var identity = new ApplicationUser();
            identity.Email = user.Email;
            identity.UserName = user.Email;
            var userRoles = new List<ApplicationUserRole>();
            var role = new ApplicationUserRole();
            var roles = await _roleService.GetAsync();
            role.RoleId = roles.FirstOrDefault(x => x.Name == "User")?.Id;
            userRoles.Add(role);
            identity.UserRoles = userRoles;
            identity.EmailConfirmed = true;
            identity.TwoFactorEnabled = true;
            var identityResult = await _userManager.CreateAsync(identity);
            if (!identityResult.Succeeded)
            {
                var error = identityResult.Errors.First();
                throw new Exception(error.Description);
            }
            var organization = await _unitOfWork.OrganizationRepository.GetAsync<int>(user.Organization.Id);

            if (organization == null)
                throw new Exception("Organization does not exist");

            var facilities = await _unitOfWork.FacilityRepository.GetListAsync(
                facility => facility.OrganizationId == organization.Id && facility.Active,
                facility => facility.Name,
                include: facility => facility.TimeZone,
                asNoTracking: true);

            var facility = facilities.FirstOrDefault(x => x.Name.ToLower() == user.FacilityName.ToLower());
            if (facility == null)
                throw new Exception("Facility does not exist");

            var shar = new User();
            shar.FirstName = string.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName;
            shar.LastName = string.IsNullOrEmpty(user.LastName) ? "" : user.LastName;
            shar.UserId = identity.Id;
            shar.Status = UserStatus.Active;
            shar.SiteId = 2;
            shar.Email = user.Email;
            shar.UserId = identity.Id;
            shar.Position = user.Position;
            shar.TimeZone = facility.TimeZone.Name;
            shar.UserOrganizations = new List<UserOrganization>
            {
                new UserOrganization() { OrganizationId = organization.Id }
            };
            shar.UserFacilities = new List<UserFacility>
            {
                new UserFacility() { FacilityId = facility.Id }
            };

            _unitOfWork.UserRepository.Add(shar);
            await _unitOfWork.SaveChangesAsync();
        }


        public async Task ExportFacilityRecipientToPortalUser(IEmailClient emailclient)
        {
            var facilities = await _unitOfWork.FacilityRepository.GetListAsync(
               facility => facility.OrganizationId == 45 && facility.Active,
               facility => facility.Name,
               include: facility => facility.TimeZone,
               asNoTracking: true);

            var facIds = facilities.Select(facility => facility.Id).ToList();

            var recipients = await _unitOfWork.FacilityRecipientRepository.GetListAsync(recp => facIds.Contains(recp.FacilityId));
            //recipients = recipients.Where(x => x.FacilityId == 577).ToList();
            foreach (var rec in recipients)
            {
                if (rec.FacilityId == 575 || rec.FacilityId == 577)
                    continue;
                try { 
                    var user = await _userManager.FindByNameAsync(rec.Email);
                        if (user != null)
                        {
                            var dbUser = await _unitOfWork.UserRepository.GetUserByEmailAsync(user.Email);
                            if (dbUser != null)
                            {
                                if (!dbUser.UserFacilities.Any(x => x.FacilityId == rec.FacilityId))
                                {
                                    dbUser.UserFacilities.Add(new UserFacility() { FacilityId = rec.FacilityId });

                                    _unitOfWork.UserRepository.Update(dbUser);
                                    _unitOfWork.SaveChanges();

                                }
                            }
                            continue;
                        }
                        else
                        {
                            var identity = new ApplicationUser();
                            identity.Email = rec.Email;
                            identity.UserName = rec.Email;
                            var userRoles = new List<ApplicationUserRole>();
                            var role = new ApplicationUserRole();
                            var roles = await _roleService.GetAsync();
                            role.RoleId = roles.FirstOrDefault(x => x.Name == "User")?.Id;
                            userRoles.Add(role);
                            identity.UserRoles = userRoles;
                            identity.EmailConfirmed = true;
                            identity.TwoFactorEnabled = true;

                            var pass = GeneratePassword() + "!";
                            var identityResult = await _userManager.CreateAsync(identity, pass);
                            if (!identityResult.Succeeded)
                            {
                                var error = identityResult.Errors.First();
                                throw new Exception(error.Description);
                            }
                            var facility = facilities.FirstOrDefault(x => x.Id == rec.FacilityId);
                            var shar = new User();
                            shar.FirstName = "";
                            shar.LastName = "";
                            shar.UserId = identity.Id;
                            shar.Status = UserStatus.Invited;
                            shar.SiteId = 2;
                            shar.Email = rec.Email;
                            shar.UserId = identity.Id;
                            shar.TimeZone = facility?.TimeZone?.Name;
                            shar.UserOrganizations = new List<UserOrganization>
                        {
                            new UserOrganization() { OrganizationId = 45 }
                        };
                            shar.UserFacilities = new List<UserFacility>
                        {
                            new UserFacility() { FacilityId = facility.Id }
                        };

                            _unitOfWork.UserRepository.Add(shar);
                            await _unitOfWork.SaveChangesAsync();


                            var userDto = await GetUserByEmail(shar.Email);

                            string apiKey = _configuration["SendGridApiKey"];
                            string from = _configuration["EmailFrom"];
                            string urlWithParam = _configuration["PortalURL"];

                            urlWithParam = urlWithParam + "login";
                            Uri uri = new Uri(urlWithParam);

                            var param = new Dictionary<string, string>() {
                                { "email", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(shar.Email)) },
                                {"portalClient" , "true"}
                         };
                            urlWithParam = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), param);

                            string url = _configuration["PortalURL"];
                            var respond = await emailclient.SendUserInvitationAsync(apiKey, from, shar.Email, pass, url, urlWithParam, userDto?.Organizations.FirstOrDefault()?.Name);

                        }
                    }
                catch (Exception ex)
                {
                    Debug.WriteLine("sssssssssssssssssssssssssssss");
                    Debug.WriteLine(ex.ToString()  + rec.Email);
                }

            }


        }

        public async Task<IReadOnlyCollection<FacilityOptionDto>> GetUserFacilitiesAsync(int userId)
        {
            var dbUser = await _unitOfWork.UserRepository.GetUserWithOrganizationsAsync(userId);
            if (dbUser == null)
                return null;

            var facilities = dbUser.UserFacilities?.Select(x => x.Facility).ToList();

            return _mapper.Map<IReadOnlyCollection<FacilityOptionDto>>(facilities);
        }

        public async Task ChangePassword(string userEmail, string password)
        {
            ApplicationUser applicationUser = await _userManager.FindByNameAsync(userEmail);


            if (!string.IsNullOrEmpty(password))
            {
                var result = await _userManager.RemovePasswordAsync(applicationUser);
                if (!result.Succeeded)
                {
                    var error = result.Errors.First();
                    throw new Exception(error.Description);
                }

                result = await _userManager.AddPasswordAsync(applicationUser, password);
                if (!result.Succeeded)
                {
                    var error = result.Errors.First();
                    throw new Exception(error.Description);
                }
            }
        }

        public async Task SetUserStatus(int id, UserStatus status)
        {
            var dbUser = await _unitOfWork.UserRepository.GetUserAsync(id);
            if (dbUser == null) return;
            dbUser.Status = status;
            _unitOfWork.UserRepository.Update(dbUser);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<UserDetailsDto> GetUserByEmail(string email)
        {
            var dbUser = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
            return _mapper.Map<UserDetailsDto>(dbUser);
        }

        public async Task<UserDetailsDto> CreateUserFromEmail(string email, string password, int organizationId, int facilityId)
        {
            var identity = new ApplicationUser();
            identity.Email = email;
            identity.UserName = email;
            var userRoles = new List<ApplicationUserRole>();
            var role = new ApplicationUserRole();
            var roles = await _roleService.GetAsync();
            role.RoleId = roles.FirstOrDefault(x => x.Name == "User")?.Id;
            userRoles.Add(role);
            identity.UserRoles = userRoles;
            identity.EmailConfirmed = true;
            identity.TwoFactorEnabled = true;
            var identityResult = await _userManager.CreateAsync(identity, password);

            if (!identityResult.Succeeded)
            {

                password = GeneratePassword();
                identityResult = await _userManager.CreateAsync(identity, password);

                if (!identityResult.Succeeded)
                {

                    var error = identityResult.Errors.First();
                    throw new Exception(error.Description);
                }
            }

            var facilities = await _unitOfWork.FacilityRepository.GetListAsync(
                         facility => facility.OrganizationId == organizationId && facility.Active,
                         facility => facility.Name,
                         include: facility => facility.TimeZone,
                         asNoTracking: true);

            var facility = facilities.FirstOrDefault(facility => facility.Id == facilityId);
            var shar = new User();
            shar.FirstName = "";
            shar.LastName = "";
            shar.UserId = identity.Id;
            shar.Status = UserStatus.Invited;
            shar.SiteId = 2;
            shar.Email = email;
            shar.UserId = identity.Id;
            shar.Position = "";
            shar.TimeZone = facility?.TimeZone?.Name;
            shar.UserOrganizations = new List<UserOrganization>
                    {
                        new UserOrganization() { OrganizationId = organizationId }
                    };
            shar.UserFacilities = new List<UserFacility>
                    {
                        new UserFacility() { FacilityId = facilityId }
                    };

            _unitOfWork.UserRepository.Add(shar);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDetailsDto>(shar);
        }



        public string GeneratePassword()
        {
            //int length = 21; 
            //string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#";
            //var random = new Random();
            //string password = new string(Enumerable.Repeat(chars, length)
            //  .Select(s => s[random.Next(s.Length)]).ToArray());


            return PasswordGenerator.GeneratePassword("Welcome30");
        }

        public async Task<bool> UpdateUserFacilitiesAsync(UserDetailsDto userDto)
        {
            User user = await _unitOfWork.UserRepository.GetUserAsync(userDto.Id);

            if (user is null)
            {
                throw new NotFoundException("User is not found");
            }


            foreach (var facility in userDto.Facilities)
            {
                if (user.UserFacilities.Any(f => f.FacilityId == facility.Id))
                {
                    continue;
                }
                user.UserFacilities.Add(new UserFacility() { FacilityId = facility.Id });
            }
            _unitOfWork.UserRepository.Update(user);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

    



        public async Task<bool> AddUpdateLoginsTrackingAsync(int userId, string type)
        {
            if (type == "Login")
            {
                LoginsTracking loginsTracking = new LoginsTracking();

                loginsTracking.UserId = userId;
                loginsTracking.Login = DateTime.UtcNow;

                await _unitOfWork.LoginsTrackingRepository.AddAsync(loginsTracking);
                await _unitOfWork.SaveChangesAsync();
            }
            else if (type == "Logout")
            {
                LoginsTracking loginsTracking = await _unitOfWork.LoginsTrackingRepository.GetLatestLoginsTrackingByUserId(userId);

                loginsTracking.Logout = DateTime.UtcNow;

                int hours = Convert.ToInt32((loginsTracking.Logout - loginsTracking.Login).Value.TotalHours);
                int minutes = Convert.ToInt32((loginsTracking.Logout - loginsTracking.Login).Value.TotalMinutes);
                int seconds = Convert.ToInt32((loginsTracking.Logout - loginsTracking.Login).Value.TotalSeconds);
                string duration = string.Empty;

                if (hours > 0)
                    duration = hours.ToString() + "h";
                else if (hours <= 0 && minutes > 0)
                    duration = minutes.ToString() + "m";
                else if (hours <= 0 && minutes <= 0 && seconds > 0)
                    duration = seconds.ToString() + "s";

                loginsTracking.Duration = duration;

                _unitOfWork.LoginsTrackingRepository.Update(loginsTracking);
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        public async Task<Tuple<IReadOnlyCollection<LoginsTrackingDto>, int>> GetPortalLoginsTrackingAsync(PortalLoginsTrackingViewFilter filter)
        {
            Expression<Func<LoginsTracking, object>> orderBySelector =
                OrderByHelper.GetOrderBySelector<LoginsTrackingFilterColumn, Expression<Func<LoginsTracking, object>>>(filter.OrderBy, GetPortalLoginsTrackingColumnOrderSelector);

            var tuple = await _unitOfWork.LoginsTrackingRepository.GetPortalLoginsTrackingAsync(filter, orderBySelector);
            return new Tuple<IReadOnlyCollection<LoginsTrackingDto>, int>(_mapper.Map<IReadOnlyCollection<LoginsTrackingDto>>(tuple.Item1), tuple.Item2);
        }

        private Expression<Func<LoginsTracking, object>> GetPortalLoginsTrackingColumnOrderSelector(LoginsTrackingFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<LoginsTrackingFilterColumn, Expression<Func<LoginsTracking, object>>>
            {
                { LoginsTrackingFilterColumn.FullName, i => i.User.FullName },
                { LoginsTrackingFilterColumn.Email, i => i.User.Email },
                { LoginsTrackingFilterColumn.Login, i => i.Login },
                { LoginsTrackingFilterColumn.Duration, i => i.Duration },
                { LoginsTrackingFilterColumn.Logout, i => i.Logout }

            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }


        public async Task<IReadOnlyCollection<TeamDto>> GetTeamsAsync()
        {
            var teams = await _unitOfWork.TeamRepository.GetAll().ToArrayAsync();
            return _mapper.Map<IReadOnlyCollection<TeamDto>>(teams);
        }

        public async Task PortalUserEditAsync(EditUserDto userDto)
        {
            User user = await _unitOfWork.UserRepository.GetUserAsync(userDto.Id);

            if (user is null)
            {
                throw new NotFoundException("User is not found");
            }

            ApplicationUser applicationUser = await _userManager.FindByIdAsync(user.UserId);

            _mapper.Map(userDto, applicationUser);

            var identityResult = await _userManager.UpdateAsync(applicationUser);
            if (!identityResult.Succeeded)
            {
                var error = identityResult.Errors.First();
                throw new Exception(error.Description);
            }

            if (!string.IsNullOrEmpty(userDto.Password))
            {
                var result = await _userManager.RemovePasswordAsync(applicationUser);
                if (!result.Succeeded)
                {
                    var error = result.Errors.First();
                    throw new Exception(error.Description);
                }

                result = await _userManager.AddPasswordAsync(applicationUser, userDto.Password);
                if (!result.Succeeded)
                {
                    var error = result.Errors.First();
                    throw new Exception(error.Description);
                }
            }

            _mapper.Map(userDto, user);
            if (string.IsNullOrEmpty(user.FirstName))
                user.FirstName = "";
            if (string.IsNullOrEmpty(user.LastName))
                user.LastName = "";
            _unitOfWork.UserRepository.Update(user);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
