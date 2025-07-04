using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.DTO.User;
using SHARP.BusinessLogic.Interfaces;
using SHARP.BusinessLogic.Services;
using SHARP.Common.Constants;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Models;
using SHARP.DAL.Models;
using SHARP.ViewModels.Portal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SHARP.Controllers
{
    [Route("api/reportManagement/admin")]
    [ApiController]
    [Authorize(Roles = "Admin,Reviewer")]
    public class PortalReportManagementController : BaseController
    {
        private readonly IOrganizationService _organizationService;
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;
        private readonly IPortalReportService _reportService;
        private readonly IEmailClient _emailClient;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IFacilityService _facilityService;
        private readonly AppConfig _appConfig;
        public PortalReportManagementController(UserManager<ApplicationUser> userManager,IOrganizationService organizationService,
                                                IAuditService auditService,
                                                IPortalReportService reportService, 
                                                IMapper mapper,IFacilityService facilityService,
                                                IEmailClient emailClient, IUserInfoService userInfoService,
                                                IUserService userService,
                                                 IConfiguration configuration,
                                                 AppConfig appConfig) : base(userManager, userInfoService)
        {
            _organizationService = organizationService;
            _auditService = auditService;
            _reportService = reportService;
            _mapper = mapper;
            _emailClient = emailClient;
            _userService = userService;
            _configuration = configuration;
            this.userManager = userManager;
            _facilityService = facilityService;
            _appConfig = appConfig;
        }

        [Route("get")]
        [HttpPost]

        public async Task<IActionResult> GetPortalReports([FromBody] PortalReportFilterModel portalReportFilter)
        {
            var filter = _mapper.Map<PortalReportFilter>(portalReportFilter);

            var (reportsDto,totalCount) = await _reportService.GetPortalReportsAsync(filter);
           

            IReadOnlyCollection<PortalReportGridItemModel> result = _mapper.Map<IReadOnlyCollection<PortalReportGridItemModel>>(reportsDto);

            return Ok(new { items = result, totalCount = totalCount });
        }
        [Route("getByPage")]
        [HttpPost]

        public async Task<IActionResult> GetPortalReportsByPage([FromBody] PortalReportFilterModel portalReportFilter)
        {
            _appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();

            var filter = _mapper.Map<PortalReportFilter>(portalReportFilter);

            var (reportsDto, totalCount) = await _reportService.GetPortalReportsByPageAsync(filter);


            IReadOnlyCollection<PortalReportGridItemModel> result = _mapper.Map<IReadOnlyCollection<PortalReportGridItemModel>>(reportsDto);

            return Ok(new { items = result, totalCount = totalCount });
        }
        [HttpPost]
        [Route("sendReports")]
        public async Task<IActionResult> SendReports([FromBody] PortalReportSelectedModel selectedReportsModel)
        {
            var portalReportSelectedDto = _mapper.Map<SelectedDto>(selectedReportsModel);

            var ccEmails = _configuration.GetValue<string>("ReportPortalCCEmails");
            var ccEmailsArray = ccEmails.Split(",");

            string apiKey = _configuration.GetValue<string>("SendGridApiKey");
            string from = _configuration.GetValue<string>("EmailFrom");
            string url = _configuration.GetValue<string>("PortalURL") ;
            string message = selectedReportsModel.Message;

            var sendReportDto = await _reportService.AddToSending(string.Join("," ,portalReportSelectedDto.UserEmails), portalReportSelectedDto.SelectedIds.ToList(), "");
                 
            //check every email if user is created and active
            Dictionary<Common.Enums.UserStatus, List<Tuple<string, string>>> dictionaryStatusPerUsers = new Dictionary<UserStatus, List<Tuple<string, string>>>();

            foreach(var email in selectedReportsModel.UserEmails)
            {
                var userDto = await _userService.GetUserByEmail(email);

                if(userDto?.Status == UserStatus.Inactive)
                {
                    continue;
                }

                string password = _userService.GeneratePassword();
                try
                {
                    if (userDto == null)
                    {
                        userDto = await _userService.CreateUserFromEmail(email, password,
                            sendReportDto.Organization.Id, sendReportDto.Facility.Id);


                    }
                    else
                    {
                        if (userDto.Status != UserStatus.Active)
                            await _userService.ChangePassword(email, password);
                        //check facility and organization 
                        if (userDto.Organizations.Any(x => x.Id == sendReportDto.Organization.Id))
                        {
                            if (!userDto.Facilities.Any(facility => facility.Id == sendReportDto?.Facility.Id))
                            {
                                var newFacilities = new List<OptionDto>();
                                newFacilities.AddRange(userDto.Facilities);
                                newFacilities.Add(sendReportDto?.Facility);
                                userDto.Facilities = newFacilities;
                                try
                                {
                                    await _userService.UpdateUserFacilitiesAsync(userDto);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }



                }catch(Exception e)
                {
                    continue;
                }

                if (userDto == null)
                    continue;


                if (!dictionaryStatusPerUsers.ContainsKey(userDto.Status))
                {
                        var list = new List<Tuple<string, string>>();

                        list.Add(new Tuple<string,string>( userDto.Email , password ));
                        dictionaryStatusPerUsers.Add(userDto.Status,list);
                }
                else
                {
                        var list = dictionaryStatusPerUsers[userDto.Status];
                        list.Add(new Tuple<string, string>(userDto.Email, password));
                        dictionaryStatusPerUsers[userDto.Status] = list;
                }
              
            }
            url = url + "login";
            Uri uri = new Uri(url);
            Response respond = null;
            foreach (var status in dictionaryStatusPerUsers.Keys) { 
                if( status == UserStatus.Active)
                {
                  
                    var param = new Dictionary<string, string>() {{ "facility", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sendReportDto.Facility?.Name)) } };
                    string urlWithParams = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), param);


                    string subject = $"ACSPro: {sendReportDto.Facility.Name} ({sendReportDto.Organization.Name}) Clinical Reports";
                    var list = dictionaryStatusPerUsers[status];
                    var emails = list.Select(x => x.Item1);
                    if (!string.IsNullOrEmpty(sendReportDto.Organization.OperatorEmail))
                        from = sendReportDto.Organization.OperatorEmail;
                    respond = await _emailClient.SendReportsAsync(apiKey, subject, ccEmailsArray, from, emails.ToList(), url, urlWithParams, sendReportDto, message);

                    if(respond.StatusCode == HttpStatusCode.Forbidden && !string.IsNullOrEmpty(sendReportDto.Organization.OperatorEmail))
                    {
                        from = _configuration.GetValue<string>("EmailFrom");
                        respond = await _emailClient.SendReportsAsync(apiKey, subject, ccEmailsArray, from, emails.ToList(), url, urlWithParams, sendReportDto, message);
                    }

                }
                else
                {
                    var list = dictionaryStatusPerUsers[status];

                    foreach(var item in list)
                    {
                        var param = new Dictionary<string, string>() {
                            { "email", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(item.Item1)) },
                            {"portalClient" , "true"}};
                        var    urlWithParam = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), param);
                   
                        respond = await _emailClient.SendUserInvitationWithReportsAsync(apiKey, from, item.Item1, item.Item2, url, urlWithParam, sendReportDto);

                    }

                }
            }
          
                
    

            return Ok(respond);
        }

       
        [Route("{Id:int}/delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteReport(int Id)
        {
            bool result = await _reportService.DeletePortalReport(Id);

            return Ok(result);
            

        }
        public string GenerateToken(int length)
        {
            using (RNGCryptoServiceProvider cryptRNG = new RNGCryptoServiceProvider())
            {
                byte[] tokenBuffer = new byte[length];
                cryptRNG.GetBytes(tokenBuffer);
                return Convert.ToBase64String(tokenBuffer);
            }
        }


        [HttpPost]
        [Route("migrate")]
        public async Task<IActionResult> MigrateAuditToReports()
        {
            await _auditService.CreateReportForPortal();
            return Ok();
        }
        [HttpGet]
        [Route("exportRecipients")]
        public async Task<IActionResult> ExportFacilityRecipients()
        {
            await _facilityService.ExportFacilityRecipientsToAnotherDB();
            return Ok();

        }

        [HttpPost]
        [Route("uploadReport")]
        public async Task<IActionResult> UploadNewReport([FromForm] UploadNewReportModal uploadReportModal)
        {
            var reportUploadDto = _mapper.Map<UploadNewReportDto>(uploadReportModal);
            if(reportUploadDto == null)
            {
                return NoContent();
            }
            if(reportUploadDto.FileUpload == null)
            {
                return NoContent();
            }
            var (newReport,error) = await _reportService.UploadNewReportAsync(reportUploadDto);
           
            if (string.IsNullOrEmpty(error))
            {
                var reportViewModel = _mapper.Map<PortalReportGridItemModel>(newReport);
                return Ok(reportViewModel);
            }
           
            return Ok(new { report = newReport, error = error });
        }
    }
}
