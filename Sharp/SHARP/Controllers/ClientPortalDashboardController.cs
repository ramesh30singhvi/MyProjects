using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using SHARP.Authentication;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Filtration;
using SHARP.ViewModels;
using SHARP.ViewModels.Portal;
using SHARP.Common.Models;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using SHARP.ViewModels.Facility;
using SHARP.BusinessLogic.Services;
using SHARP.ViewModels.Organization;
using SHARP.ViewModels.User;

namespace SHARP.Controllers
{
    [Route("api/clientportal")]
    [ApiController]
    public class ClientPortalDashboardController : Controller
    {
        private readonly IPortalReportService _reportService;
        private readonly IFormService _formService;
        private readonly IHighAlertService _highAlertService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IFacilityService _facilityService;
        private readonly AppConfig _appConfig;
        private readonly IEmailClient _emailClient;
        private readonly IConfiguration _configuration;
        private readonly IOrganizationService _organizationService;

        public ClientPortalDashboardController(IPortalReportService reportService,
            IFormService formService, IMapper mapper,IHighAlertService highAlertService,
            IUserService userService,IOrganizationService organizationService, AppConfig appConfig,IFacilityService facilityService,
            IEmailClient emailClient,IConfiguration configuration)
        {
            _reportService = reportService;
            _mapper = mapper;
            _formService = formService;
            _highAlertService = highAlertService;
            _userService = userService;
            _appConfig = appConfig;
            _facilityService = facilityService;
            _emailClient = emailClient;
            _configuration = configuration;
            _organizationService = organizationService;
        }

        [HttpPost]
        [Route("hasAccess")]
        public async Task<IActionResult> HasAccess([FromBody] FacilityAccessModel model)
        {
            var accessDto = _mapper.Map<FacilityAccessDto>(model);
            var (hasAccess,error,facilityID) = await _reportService.HasAccess(accessDto);
            return Ok(new HasFacilityAccessViewModel() { Status = hasAccess.ToString(), Message = error , FacilityId = facilityID });
        }
        [Route("get")]
        [HttpPost]

        public async Task<IActionResult> GetHighAlertsPortalForFacility([FromBody] HighAlertPortalFilterModel portalHighAlertFilter)
        {
            //_appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();

            var filter = _mapper.Map<HighAlertPortalFilter>(portalHighAlertFilter);

            var (highAlertDto, totalCount) = await _highAlertService.GetHighAlertsAsync(filter);

            IReadOnlyCollection<HighAlertGridItemModel> result = _mapper.Map<IReadOnlyCollection<HighAlertGridItemModel>>(highAlertDto);

            return Ok(new { items = result, totalCount = totalCount });
        }




        [HttpPost]
        [Route("facilityReportsByPage")]
        public async Task<IActionResult> GetReportsByPage([FromBody] PortalReportFacilityViewFilterModel facilityViewFilter)
        {
            var filter = _mapper.Map<PortalReportFacilityViewFilter>(facilityViewFilter);

            var (reportsDto, totalCount) = await _reportService.GetPortalReportsForFacilityAsync(filter);

            IReadOnlyCollection<PortalReportGridItemModel> result = _mapper.Map<IReadOnlyCollection<PortalReportGridItemModel>>(reportsDto);

            return Ok(new { items = result, totalCount = totalCount });
        }

        [HttpPost]
        [Route("facilityInfo")]
        public async Task<IActionResult> GetFacilityInfo([FromBody]FacilityDetailsModel facility)
        {
            var facilityDto = await _facilityService.GetFacilityByNameAsync(facility?.Name,

                facility.Organization != null ? facility.Organization.Id : 0 );
            var facilityModel = _mapper.Map<FacilityDetailsModel>(facilityDto);
            return Ok(facilityModel);
        }

        [HttpPost]
        [Route("downloadPDFReport")]
        public async Task<IActionResult> DownloadPDFReport([FromBody] PortalReportSelectedModel selectedReportsModel)
        {
            var portalReportSelectedDto = _mapper.Map<SelectedDto>(selectedReportsModel);
            var pdf = await _reportService.DownloadPortalReport(portalReportSelectedDto);
            if (pdf == null)
                return NoContent();
            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(pdf, MediaTypeNames.Application.Pdf);
        }
        [HttpPost]
        [Route("downloadExcelReport")]
        public async Task<IActionResult> DownloadExcelReport([FromBody] PortalReportSelectedModel selectedReportsModel)
        {
            var portalReportSelectedDto = _mapper.Map<SelectedDto>(selectedReportsModel);
            var excel = await _reportService.DownloadPortalReport(portalReportSelectedDto);
            if(excel == null)
                return NoContent();

            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        }
        [HttpGet]
        [Route("reportTypes")]
        public async Task<IActionResult> ReportTypes()
        {
            var reportTypes = await _reportService.GetReportTypes();
            var types = _mapper.Map<OptionModel[]>(reportTypes);
            return Ok(types);
        }

        [HttpGet]
        [Route("reportRanges")]
        public async Task<IActionResult> ReportRanges()
        {
            var reportRanges = await _reportService.GetReportRanges();
            var ranges = _mapper.Map<OptionModel[]>(reportRanges);
            return Ok(ranges);
        }
        [HttpGet]
        [Route("reportCategories")]
        public async Task<IActionResult> ReportCategories()
        {
            var reportCategories = await _reportService.GetReportCategories();
            var categories = _mapper.Map<OptionModel[]>(reportCategories);
            return Ok(categories);
        }
        [HttpGet]
        [Route("auditTypes")]
        public async Task<IActionResult> AuditTypes()
        {
            var type = await _formService.GetAuditTypes();
            var typeModel = _mapper.Map<OptionModel[]>(type);
            return Ok(typeModel);
        }
        [HttpGet]
        [Route("highAlertStatuses")]
        public async Task<IActionResult> HighAlertStatuses()
        {
            var type = await _highAlertService.GetHighAlertStatuses();
            var typeModel = _mapper.Map<OptionModel[]>(type);
            return Ok(typeModel);
        }
        [Route("highAlertCategories")]
        [HttpGet]
        public async Task<IActionResult> GetHighAlertCategories()
        {
            var categories = await _highAlertService.GetHighAlertCategories();
            return Ok(categories);
        }
        [Route("highAlertCategoriesAndPotentialAreas")]
        [HttpGet]
        public async Task<IActionResult> GetHighAlertCategoriesAndPotentialAreas()
        {
            var categoriesWithArea = await _highAlertService.GetHighAlertCategoriesWithPotentialArea();

            var categoriesNameIncludeArea = _mapper.Map<HighAlertCategoryViewModel[]>(categoriesWithArea);
            return Ok(categoriesNameIncludeArea);
        }

        [Route("potentialAreas")]
        [HttpGet]
        public async Task<IActionResult> GetPotentialsAreas()
        {
            var areas = await _highAlertService.GetHighAlertPotentialAreas();

            var areasModel = _mapper.Map<OptionModel[]>(areas);
            return Ok(areasModel);
        }
        [Route("{id:int}/details")]
        [HttpGet]
        public async Task<IActionResult> GetOrganizationsDetailedAsync(int id)
        {
            var dto = await  _organizationService.GetDetailedAsync(id);
            var model = _mapper.Map<OrganizationDetailedModel>(dto);
            return Ok(model);
        }
        [HttpPost]
        [Route("contactUs")]
        public async Task<IActionResult> ContactFormMessage([FromBody] ContactFormModel contactForm)
        {
            var emails = _configuration.GetValue<string>("PortalNeedHelpEmail");

            if (string.IsNullOrEmpty(emails))
            {
                var user = await _userService.GetLoggedInUserAsync();
                if (user == null)
                    return BadRequest("Please define support email");

                emails = user.Email;
            }
            var emailsArray = emails.Split(",");
            string from = _configuration.GetValue<string>("EmailFrom");
            string apiKey = _configuration.GetValue<string>("SendGridApiKey");
            var respond = await _emailClient.SendNeedHelpEmailAsync(apiKey, emailsArray, from, contactForm.Email, contactForm.Name, contactForm.Message);
            return Ok(respond);

        }


        [Route("getFacilitySurveyData/{facilityName}")]
        [HttpGet]
        public async Task<IActionResult> GetFacilitySurveyData(string facilityName)
        {
            try
            {
                string providerInformationData = await _facilityService.GetProviderInformationByFacilityAsync(facilityName);
                string healthDeficienciesData = await _facilityService.GetHealthDeficienciesByFacilityAsync(facilityName);

                if (string.IsNullOrWhiteSpace(providerInformationData) || string.IsNullOrWhiteSpace(healthDeficienciesData))
                {
                    return BadRequest("Error occurred or invalid response from GetFacilitySurveyData()");
                }
                else
                {
                    return Ok(new
                    {
                        providerInformationData,
                        healthDeficienciesData
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("getFacilityQualityMeasuresData/{facilityName}")]
        [HttpGet]
        public async Task<IActionResult> GetFacilityQualityMeasuresData(string facilityName)
        {
            try
            {
                string mdsQualityMeasuresData = await _facilityService.GetMDSQualityMeasuresDataByFacilityAsync(facilityName);
                string medicareClaimsQualityMeasuresData = await _facilityService.GetMedicareClaimsQualityMeasuresDataByFacilityAsync(facilityName);

                if (string.IsNullOrWhiteSpace(mdsQualityMeasuresData) || string.IsNullOrWhiteSpace(medicareClaimsQualityMeasuresData))
                {
                    return BadRequest("Error occurred or invalid response from GetFacilityQualityMeasuresData()");
                }
                else
                {
                    return Ok(new
                    {
                        mdsQualityMeasuresData,
                        medicareClaimsQualityMeasuresData
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("auditSummary/{facilityId:int}/{fromDate}/{toDate}")]
        [HttpGet]
        public async Task<IActionResult> GetAuditSummary(int facilityId, string fromDate, string toDate)
        {
            var auditSummary = await _facilityService.GetAuditSummaryAsync(facilityId, fromDate, toDate);
            return Ok(auditSummary);

        }

        [Route("updateLoginsTracking")]
        [HttpPost]
        public async Task<IActionResult> AddUpdateLoginsTracking([FromBody] AddUpdateLoginsTrackingModel model)
        {
            int userId = _userService.GetLoggedUserId();
            bool result = await _userService.AddUpdateLoginsTrackingAsync(userId, model.type);
            return Ok(result);
        }

        [HttpPost]
        [Route("getLoginsTracking")]
        public async Task<IActionResult> GetPortalLoginsTracking([FromBody] PortalLoginsTrackingViewFilterModel facilityViewFilter)
        {
            var filter = _mapper.Map<PortalLoginsTrackingViewFilter>(facilityViewFilter);

            var (loginsTrackingDto, totalCount) = await _userService.GetPortalLoginsTrackingAsync(filter);

            return Ok(new { items = loginsTrackingDto, totalCount = totalCount });
        }

        [HttpPost]
        [Route("getDownloadsTracking")]
        public async Task<IActionResult> GetPortalDownloadsTracking([FromBody] PortalDownloadsTrackingViewFilterModel facilityViewFilter)
        {
            var filter = _mapper.Map<PortalDownloadsTrackingViewFilter>(facilityViewFilter);

            var (downloadsTrackingDto, totalCount) = await _reportService.GetPortalDownloadsTrackingAsync(filter);

            return Ok(new { items = downloadsTrackingDto, totalCount = totalCount });

        }

        //[Route("moveRecip")]
        //[HttpPost]
        //public async Task<IActionResult> MoveFacilitiesRecipientToPortalUser([FromBody] CreatePortalUserModel model)
        //{

        //    await _userService.ExportFacilityRecipientToPortalUser();
        //    return Ok();
        //}
    }
}
