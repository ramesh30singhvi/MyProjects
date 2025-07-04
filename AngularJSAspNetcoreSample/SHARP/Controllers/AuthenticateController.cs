using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SHARP.Authentication;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Http;
using SHARP.ViewModels;
using SHARP.DAL.Models;
using Microsoft.AspNetCore.WebUtilities;
using SHARP.Common.Enums;
using SHARP.BusinessLogic;
using SHARP.Common.Constants;
using SHARP.BusinessLogic.DTO.UserActivity;
using System.Linq;
using AutoMapper;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.DAL;

namespace SHARP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IFEClientService _feClientService;
        private readonly IEmailClient _emailClient;
        private readonly IPortalReportService _reportService;
        private readonly IMapper _mapper;
        private readonly IFacilityService _facilityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly int maxFailedAccessAttempts;
        private readonly int minutes;

        public AuthenticateController(

        UserManager<ApplicationUser> userManager,
        IConfiguration configuration, IMapper mapper, IFacilityService facilityService,
        IUserService userService, IPortalReportService reportService, 
        IUnitOfWork unitOfWork,
        IFEClientService feClientService,
        IEmailClient emailClient)
        {
            this.userManager = userManager;
            _configuration = configuration;
            _userService = userService;
            _feClientService = feClientService;
            _emailClient = emailClient;
            _reportService = reportService;
            _mapper = mapper;
            _facilityService = facilityService;
            _unitOfWork = unitOfWork;
            maxFailedAccessAttempts = string.IsNullOrEmpty(_configuration["MaxFailedAccessAttempts"]) ? 3 : Int32.Parse(_configuration["MaxFailedAccessAttempts"]);
            minutes = string.IsNullOrEmpty(_configuration["DefaultLockoutTimeSpan"]) ? 5 : Int32.Parse(_configuration["DefaultLockoutTimeSpan"]);
        }

        private async Task<bool> IsUserLocked( ApplicationUser user)
        {
            if (user != null && user.LockoutEnabled)
            {
                var lockedTime = await userManager.GetLockoutEndDateAsync(user);
                int failedAttempts = await userManager.GetAccessFailedCountAsync(user);
                if (lockedTime != null && lockedTime > DateTime.UtcNow && failedAttempts == maxFailedAccessAttempts)
                {
                    return true;
                }
                else if (lockedTime != null && lockedTime <= DateTime.UtcNow)
                {
                    user.AccessFailedCount = 0;
                    await userManager.UpdateAsync(user);
                    await userManager.SetLockoutEndDateAsync(user, null);
                }
            }
            return false;
        }


        [HttpPost]
        [Route("loginPortal")]
        public async Task<IActionResult> LoginPortal([FromBody] LoginModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);

            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var dbUser = await _userService.GetUserAsync(user.Id);

                if (dbUser.Status == UserStatus.Inactive)
                {
                    return Unauthorized("The User is an inactive");
                }

                var userAgent = Request.GetUserAgent();
                var ip = Request.GetIP();
                var isTrusted = _feClientService.IsTrusted(userAgent, ip, dbUser.Id);
                if (isTrusted)
                {
                    var token = await GenerateTokenDataPortalAsync(user, dbUser);
                    return Ok(token);
                }

                string token2FaCode = await userManager.GenerateTwoFactorTokenAsync(user, TwoFATokenProvider.NAME);
                string apiKey = _configuration.GetValue<string>("SendGridApiKey");
                string from = _configuration.GetValue<string>("EmailFrom");
                string url = Request.Headers["Referer"].ToString();

                url = url + "login";
                Uri uri = new Uri(url);

                var param = new Dictionary<string, string>() { { "user", model.Username }, { "code", token2FaCode } };
                string urlWithParams = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), param);

                var templateName =  "CLIENT_PORTAL_ACCESS_OTP";
                await _emailClient.SendOTPAsync(templateName, apiKey, from, dbUser.Email, token2FaCode, urlWithParams);

                return BadRequest("2FAPortal required");
            }
            return Unauthorized("Username and / or password is incorrect");
        }
        [Route("2faPortal")]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> AccessPortalTwoStep([FromBody] TwoStepModel twoStepModel, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Unauthorized(twoStepModel);
            }

            var user = await userManager.FindByNameAsync(twoStepModel.Username);
            if (user == null)
            {
                return Unauthorized("User was not found");
            }

            bool boolRes = await userManager.VerifyTwoFactorTokenAsync(
                user,
                TwoFATokenProvider.NAME,
                twoStepModel.TwoFactorCode);
            if (boolRes)
            {
                var userAgent = Request.GetUserAgent();
                var ip = Request.GetIP();
                var dbUser = await _userService.GetUserAsync(user.Id);
                _feClientService.Trust(userAgent, ip, dbUser.Id);
                var tokenData = await GenerateTokenDataPortalAsync(user, dbUser);

                return Ok(tokenData);
            }
            else
            {
                return Unauthorized("Invalid Login Attempt");
            }
        }
     
        private async Task<object> GenerateTokenDataPortalAsync(ApplicationUser user, User dbUser)
        {
            var userRoles = await userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(CommonConstants.DB_USER_ID, dbUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(24),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );


            var userdetail = await _userService.GetUserDetailsAsync(dbUser.Id);
            IReadOnlyCollection<OptionDto> facilities = new List<OptionDto>();
            if( !userdetail.Facilities.Any() )
            {
                var facilitiesDto =  await _facilityService.GetFacilityOptionsAsync(userdetail.Organizations.FirstOrDefault()?.Id ?? 0);
                if(facilitiesDto.Any() )
                {
                    var facDTOs = facilitiesDto.ToList();

                    facDTOs.Insert(0, new FacilityOptionDto { Name = "All", Id = 0 });

                    facilities = _mapper.Map<IReadOnlyCollection<OptionDto>>(facDTOs);
                }
            }
            return new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                userData = new
                {
                    userId = user.Id,
                    username = user.UserName,
                    id = dbUser.Id,
                    email = dbUser.Email,
                    siteId = 2,
                    roles = userRoles,
                    organizations = userdetail.Organizations,
                    facilities = facilities,
                    firstName = dbUser != null ? dbUser.FirstName : string.Empty,
                    lastName = dbUser != null ? dbUser.LastName : string.Empty
                }
            };
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);

            var isLocked = await IsUserLocked(user);

            if (isLocked)
            {
               return Unauthorized("The Account is locked. Too much attempts. Try in 5 minutes");
            }

            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var dbUser = await _userService.GetUserAsync(user.Id);

                if(dbUser?.SiteId != 1)
                {
                    return Unauthorized();
                }
                if (dbUser.Status == UserStatus.Inactive)
                {
                    return BadRequest("User is inactive");
                }

                var userAgent = Request.GetUserAgent();
                var ip = Request.GetIP();
                var isTrusted = _feClientService.IsTrusted(userAgent, ip, dbUser.Id);
                if (isTrusted)
                {
                    var token = await GenerateTokenDataAsync(user, dbUser);

                    await _userService.AddUserActivityAsync(new AddUserActivityDto()
                    {
                        UserId = dbUser.Id,
                        ActionType = ActionType.Login,
                        UserAgent = userAgent,
                        IP = ip.ToString()
                    });

                    return Ok(token);
                }

                string token2FaCode = await userManager.GenerateTwoFactorTokenAsync(user, TwoFATokenProvider.NAME);
                string apiKey = _configuration.GetValue<string>("SendGridApiKey");
                string from = _configuration.GetValue<string>("EmailFrom");
                string url = Request.Headers["Referer"].ToString();

                url = url.Contains("sharp") ? url : url + "login";
                Uri uri = new Uri(url);
                
                var param = new Dictionary<string, string>() { { "user", model.Username }, { "code", token2FaCode } };
                string urlWithParams = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), param);

                var templateName = url.Contains("sharp") ? "ACS_OTP" : "ACS_OTP_PORTAL";
                await _emailClient.SendOTPAsync(templateName,apiKey, from, dbUser.Email, token2FaCode, urlWithParams);

                return BadRequest("2FA required");
            }
            else
            {
                if (user != null)
                {
                    int failedAttempts = await userManager.GetAccessFailedCountAsync(user);
                    if (failedAttempts < maxFailedAccessAttempts)
                    {
                        user.AccessFailedCount++;
                        await userManager.UpdateAsync(user);
                        if (user.AccessFailedCount == maxFailedAccessAttempts)
                        {
                            await userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.UtcNow.AddMinutes(minutes)));
                            return Unauthorized("The Account is locked. Too much attempts. Try in 5 minutes");
                        }
                    }
                }

            }
            return Unauthorized("Username and/or password is incorrect");
        }

        [Route("2fa")]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginTwoStep([FromBody] TwoStepModel twoStepModel, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(twoStepModel);
            }

            var user = await userManager.FindByNameAsync(twoStepModel.Username);
            if (user == null)
            {
                return BadRequest("User was not found");
            }

            bool boolRes = await userManager.VerifyTwoFactorTokenAsync(
                user,
                TwoFATokenProvider.NAME,
                twoStepModel.TwoFactorCode);
            if (boolRes)
            {
                var userAgent = Request.GetUserAgent();
                var ip = Request.GetIP();
                var dbUser = await _userService.GetUserAsync(user.Id);
                _feClientService.Trust(userAgent, ip, dbUser.Id);
                var tokenData = await GenerateTokenDataAsync(user, dbUser);

                await _userService.AddUserActivityAsync(new AddUserActivityDto() { 
                    UserId = dbUser.Id, 
                    ActionType = ActionType.Login, 
                    UserAgent = userAgent, 
                    IP = ip.ToString() });

                return Ok(tokenData);
            }
            else
            {
                return BadRequest("Invalid Login Attempt");
            }
        }

        private async Task<object> GenerateTokenDataAsync(ApplicationUser user, User dbUser)
        {
            var userRoles = await userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(CommonConstants.DB_USER_ID, dbUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(24),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                userData = new
                {
                    userId = user.Id,
                    username = user.UserName,
                    id = dbUser.Id,
                    email = dbUser.Email,
                    siteId = dbUser.SiteId,
                    roles = userRoles,
                    organizations = dbUser.UserOrganizations?.Select(userOrg => new { Id = userOrg.Organization.Id, Name = userOrg.Organization.Name}),
                    facilities = dbUser.UserFacilities?.Select(userFacility => new { Id = userFacility.Facility.Id, Name = userFacility.Facility.Name }),
                    firstName = dbUser != null ? dbUser.FirstName : string.Empty,
                    lastName = dbUser != null ? dbUser.LastName : string.Empty
                }
            };
        }
    }
}
