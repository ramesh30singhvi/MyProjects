using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using ScottPlot.Drawing.Colormaps;
using ScottPlot.Renderable;
using SHARP.BusinessLogic;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.User;
using SHARP.BusinessLogic.DTO.UserActivity;
using SHARP.BusinessLogic.Extensions;
using SHARP.BusinessLogic.Interfaces;
using SHARP.BusinessLogic.Services;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;
using SHARP.Filters;
using SHARP.Http;
using SHARP.ViewModels;
using SHARP.ViewModels.Common;
using SHARP.ViewModels.Facilitty;
using SHARP.ViewModels.Portal;
using SHARP.ViewModels.Role;
using SHARP.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SHARP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }



        [Route("get")]
        [HttpPost]
        public async Task<IActionResult> GetUsers([FromBody] UserFilterModel userFiler)
        {
            var filter = _mapper.Map<UserFilter>(userFiler);
            filter.SiteId = 1;
            var users = await _userService.GetUsersAsync(filter);

            var result = _mapper.Map<IReadOnlyCollection<UserModel>>(users);

            return Ok(result);
        }

        [Route("filters")]
        [HttpPost]
        public async Task<IActionResult> GetFilters([FromBody] UserFilterColumnSourceModel columnData)
        {
            var columnFilter = _mapper.Map<UserFilterColumnSource<UserColumn>>(columnData);

            IReadOnlyCollection<FilterOption> optionsDto = await _userService.GetFilterColumnSourceDataAsync(columnFilter);

            var options = _mapper.Map<IReadOnlyCollection<FilterOptionModel>>(optionsDto);

            return Ok(options);
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserModel model)
        {
            try
            {
                var dto = _mapper.Map<CreateUserDto>(model);
                int Id = await _userService.CreateAsync(dto);
                return Ok(Id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorMessage = ex.Message });
            }
        }

        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditUser([FromBody] EditUserModel model)
        {
            try
            {
                var userDto = _mapper.Map<EditUserDto>(model);
                await _userService.EditAsync(userDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorMessage = ex.Message });
            }
        }

        [Route("details/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> GetUserDetails(int id)
        {
            UserDetailsDto user = await _userService.GetUserDetailsAsync(id);

            var result = _mapper.Map<UserDetailsModel>(user);

            return Ok(result);
        }

        [Route("timezones")]
        [HttpGet]
        public IActionResult GetTimezones()
        {
            IReadOnlyCollection<TimeZoneInfo> timeZoneInfos = TimeZoneInfo.GetSystemTimeZones();

            var timeZones = _mapper.Map<IReadOnlyCollection<TimeZoneModel>>(timeZoneInfos);

            return Ok(timeZones);
        }
    }

    [Route("api/users/activities")]
    [ApiController]
    public class UserActivitiesController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserActivitiesController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> AddActivity([FromBody] AddUserActivityModel model)
        {
            var activityDto = _mapper.Map<AddUserActivityDto>(model);
            try
            {
                var userId = _userService.GetLoggedUserId();
                activityDto.UserId = userId;
            }
            catch (NotFoundException ex)
            {
                activityDto.UserId = default;
            }
            activityDto.UserAgent = Request.GetUserAgent();
            activityDto.IP = Request.GetIP().ToString();

            bool result = await _userService.AddUserActivityAsync(activityDto);

            return Ok(result);
        }

        [Route("download-logs")]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadLogs([FromBody] DownloadUserActivityModel model)
        {
            List<int> ids = new List<int>();
            string userIds = string.Empty;
            var baseUri = $"{Request.Scheme}://{Request.Host}/audits/";

            if (model.Type == "multiple")
            {

                var userFilters = model.Filters;
                userFilters.TakeCount = 999999;

                var filter = _mapper.Map<UserFilter>(userFilters);
                filter.SiteId = 1;
                var users = await _userService.GetUsersAsync(filter);
                var result = _mapper.Map<IReadOnlyCollection<UserModel>>(users);
                ids = result.ToList().ConvertAll<int>(user => user.Id);
                userIds = "All";

            }
            else
            {
                if (model.UserId != null)
                {
                    ids.Add((int)model.UserId);
                    userIds = string.Join(",", ids);
                }

            }

            var excel = await _userService.GetLogsExcel(userIds, model.FromDate, model.ToDate, baseUri);
            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UserProductivity.xlsx");
          
        }
    }

    [Route("api/users")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [Route("organizations/options")]
        [HttpGet]
        public async Task<IActionResult> GetOrganizationOptions()
        {
            UserOrganizationsDto organizationsDto = await _userService.GetUserOrganizationsAsync();

            var organizationOptions = _mapper.Map<UserOrganizationsModel>(organizationsDto);

            return Ok(organizationOptions);
        }

        [Route("{userId:int}/timezone")]
        [HttpGet]
        public async Task<IActionResult> GetUserTimeZone(int userId)
        {
            string userTimeZone = await _userService.GetUserTimeZoneAsync(userId);

            var userTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
            var userTimeZoneDateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, userTimeZone);

            return Ok(new { UserTimeZoneInfo = userTimeZoneInfo, UserTimeZoneDateTime = userTimeZoneDateTime });
        }

        [Route("organization/{organizationId:int}")]
        [HttpGet]
        public async Task<IActionResult> GetUsersForOrganization(int organizationId)
        {
       
            IReadOnlyCollection<UserOptionDto> userOptionDto = await _userService.GetUserOptionsAsync(organizationId);

            var usesOptions = _mapper.Map<IReadOnlyCollection<UserOptionModel>>(userOptionDto);

            return Ok(usesOptions);
        }
        //roles
        [HttpGet]
        [Route("teams")]
        public async Task<IActionResult> GetTeams()
        {
            var dto = await _userService.GetTeamsAsync();
            var model = _mapper.Map<IEnumerable<TeamModel>>(dto);
            return Ok(model);
        }
    }
    [Route("api/portalusers")]
    [ApiController]
    public class PortalUserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRoleService _roleService;
        private readonly IEmailClient _emailClient;
        private readonly IConfiguration _configuration;
        private readonly IOrganizationService _organizationService;
        private readonly IFacilityService _facilityService;

        public PortalUserController(IUserService userService,  
            IConfiguration configuration ,IOrganizationService orgService,IFacilityService facilityService,
        IEmailClient emailClient, IRoleService roleService ,IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _mapper = mapper;
            this.userManager = userManager;
            _roleService = roleService;
            _configuration = configuration;
            _emailClient = emailClient;
            _organizationService = orgService;
            _facilityService = facilityService;
        }

        [Route("get")]
        [HttpPost]
        public async Task<IActionResult> GetPortalUsers([FromBody] UserFilterModel userFiler)
        {
            var filter = _mapper.Map<UserFilter>(userFiler);
            filter.SiteId = 2;

            var users = await _userService.GetUsersAsync(filter);

            var result = _mapper.Map<IReadOnlyCollection<UserModel>>(users);

            return Ok(result);
        }
        [Route("moveRecip")]
        [HttpPost]
        public async Task<IActionResult> MoveFacilitiesRecipientToPortalUser([FromBody] CreatePortalUserModel model)
        {
            
            await _userService.ExportFacilityRecipientToPortalUser(_emailClient);
            return Ok();
        }

        private async  Task<IReadOnlyCollection<FacilityOptionDto>> GetAllFacilitiesForOrganization(int organizationId)
        {
            IReadOnlyCollection<FacilityOptionDto> facilitiesDto = await _facilityService.GetFacilityOptionsAsync(organizationId);
            if(!facilitiesDto.Any())
            {
                return facilitiesDto;
            }
            var facDTOs = facilitiesDto.ToList();
            facDTOs.Insert(0, new FacilityOptionDto { Name = "All", Id = 0 });
            return facDTOs;
        }
        [Route("facilities/{userId:int}")]
        [HttpGet]
        public async Task<IActionResult> GetUserFacilities(int userId)
        {

            IReadOnlyCollection<FacilityOptionDto> facilitiesDto = await _userService.GetUserFacilitiesAsync(userId);
            if (!facilitiesDto.Any())
            {
                var userDetails = await  _userService.GetUserDetailsAsync(userId);
                facilitiesDto = await GetAllFacilitiesForOrganization(userDetails.Organizations.FirstOrDefault().Id);
            }
            var facilitiesModel = _mapper.Map<IReadOnlyCollection<FacilityOptionModel>>(facilitiesDto);

            return Ok(facilitiesModel);
        }
        [HttpGet]
        [Route("roles")]
        public async Task<IActionResult> GetPortalRoles()
        {
            var dto = await _roleService.GetAsync();
            var model = _mapper.Map<IEnumerable<RoleModel>>(dto.Where(x=> x.Name.Contains("User") ));
            return Ok(model);
        }


        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditUser([FromBody] EditUserModel model)
        {
            try
            {
                var userDto = _mapper.Map<EditUserDto>(model);
                await _userService.PortalUserEditAsync(userDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorMessage = ex.Message });
            }
        }

        [Route("details/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> GetUserDetails(int id)
        {
            UserDetailsDto user = await _userService.GetUserDetailsAsync(id);
            if(!user.Facilities.Any() && user.Organizations.Any())
            {
                var facilities = await GetAllFacilitiesForOrganization(user.Organizations.FirstOrDefault().Id);
                user.Facilities = _mapper.Map<IEnumerable<OptionDto>>(facilities);
            }
            var result = _mapper.Map<UserDetailsModel>(user);

            return Ok(result);
        }

        [Route("inactive/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> SetUserInactive(int id)
        {
            await _userService.SetUserStatus(id,UserStatus.Inactive);
            var user = await _userService.GetUserDetailsAsync(id);
            var result = _mapper.Map<UserDetailsModel>(user);

            return Ok(result);
        }

        [Route("active/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> SetUserActive(int id)
        {
            await _userService.SetUserStatus(id, UserStatus.Active);
            var user = await _userService.GetUserDetailsAsync(id);
            var result = _mapper.Map<UserDetailsModel>(user);

            return Ok(result);
        }

        [HttpPost]
        [Route("resetPassword")]
        public async Task<IActionResult> ResetPassword(ForgotPasswordModel model)
        {
            var user = await userManager.FindByNameAsync(model.Email.Trim());
            if (user == null)
                return BadRequest("User does not exist");

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            string apiKey = _configuration.GetValue<string>("SendGridApiKey");
            string from = _configuration.GetValue<string>("EmailFrom");
            string url = _configuration.GetValue<string>("PortalURL");


            Debug.WriteLine(token);
            string urlWithParam = url + "reset-password";
            var param = new Dictionary<string, string>() {
                    { "email", model.Email.Trim() },
                    { "token",token },
                };
            Uri uri = new Uri(urlWithParam);
            urlWithParam = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), param);

            await _emailClient.SendUserResetPasswordAsync(apiKey, from, model.Email.Trim(), urlWithParam);


            return Ok();
        }
        [HttpPost]
        [ValidateModel]
        [Route("changePassword")]
        public async Task<IActionResult> ChangePassword(ResetPasswordModel model)
        {
            try
            {
                var user = await userManager.FindByNameAsync(model.Email.Trim());
                if (user == null)
                    return BadRequest("User does not exist");

                if (!string.IsNullOrEmpty(model.Token))
                {
                    var identityResult = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (!identityResult.Succeeded)
                    {
                        var error = identityResult.Errors.First();
                        throw new Exception(error.Description);
                    }
                }
                else
                {
                    await _userService.ChangePassword(model.Email.Trim(), model.Password);

                    var dbUser = await _userService.GetUserAsync(user.Id);
                    if(dbUser == null)
                        return BadRequest("User does not exist");
                    await _userService.SetUserStatus(dbUser.Id, UserStatus.Active);
                }
               

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorMessage = ex.Message });
            }
        }


        //inactivateUsers
        [Route("inactivateUsers")]
        [HttpPost]
        public async Task<IActionResult> SetInactivateUsers(SelectedIdsModel model)
        {
            try
            {
                foreach (var id in model.SelectedIds)
                {
                    await _userService.SetUserStatus(id, UserStatus.Inactive);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorMessage = ex.Message });
            }
          
        }
        [HttpPost]
        [ValidateModel]
        [Route("sendInvite")]
        public async Task<IActionResult> SendInvite([FromBody] InvitationPortalUserModel model)
        {
            try
            {
                await _userService.ChangePassword(model.UserEmail,model.Password);

                var dbUser = await _userService.GetUserDetailsAsync(model.UserId);
                // send email for user 
                string apiKey = _configuration.GetValue<string>("SendGridApiKey");
                string from = _configuration.GetValue<string>("EmailFrom");
                string urlWithParam = _configuration.GetValue<string>("PortalURL");

                urlWithParam = urlWithParam + "login";
                Uri uri = new Uri(urlWithParam);

                var param = new Dictionary<string, string>() {
                    { "email", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.UserEmail)) },
                    {"portalClient" , "true"}
                };
                urlWithParam = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), param);

                string url = _configuration.GetValue<string>("PortalURL");
                var respond = await _emailClient.SendUserInvitationAsync(apiKey, from, model.UserEmail, model.Password, url, urlWithParam, dbUser?.Organizations.FirstOrDefault()?.Name);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorMessage = ex.Message });
            }
        }


        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> CreateUser([FromBody] CreatePortalUserModel model)
        {
            try
            {
             
                var dto = _mapper.Map<CreatePortalUserDto>(model);

                await _userService.CreatePortalUserFromPortalAsync(dto);
  

                // send email for user 
                string apiKey = _configuration.GetValue<string>("SendGridApiKey");
                string from = _configuration.GetValue<string>("EmailFrom");
                string urlWithParam = _configuration.GetValue<string>("PortalURL");

                urlWithParam = urlWithParam + "login";
                Uri uri = new Uri(urlWithParam);

                var param = new Dictionary<string, string>() { 
                    { "email", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.Email)) },
                    {"portalClient" , "true"}
                };
                urlWithParam = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), param);

                string url = _configuration.GetValue<string>("PortalURL");
                var respond = await _emailClient.SendUserInvitationAsync(apiKey, from,model.Email, model.Password,url,urlWithParam,model.Organization?.Name);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorMessage = ex.Message });
            }
        }
    }
}
