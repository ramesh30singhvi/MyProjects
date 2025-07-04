using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using ScottPlot.Control;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.Extensions;
using SHARP.BusinessLogic.Interfaces;
using SHARP.BusinessLogic.Services;
using SHARP.Common.Constants;
using SHARP.Common.Filtration;
using SHARP.Common.Models;
using SHARP.DAL.Models;
using SHARP.ViewModels;
using SHARP.ViewModels.Portal;
using SHARP.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;

namespace SHARP.Controllers
{
    [Route("api/highAlert")]
    [ApiController]
    public class HighAlertPortalController : BaseController
    {
        private readonly IHighAlertService _highAlertService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly AppConfig _appConfig;

        public HighAlertPortalController(UserManager<ApplicationUser> userManager, IUserInfoService userInfoService,
            IHighAlertService highAlertService, IMapper mapper, IUserService userService,
                                                 IConfiguration configuration,
                                                 AppConfig appConfig) : base(userManager, userInfoService)
        {
            _highAlertService = highAlertService;
            _mapper = mapper;
            _userService = userService;
            _configuration = configuration;
            _appConfig = appConfig;
        }
        [HttpPost]
        [Route("downloadPDFReport")]
        public async Task<IActionResult> DownloadPDFReport([FromBody] OptionModel optionModal)
        {
            var selectedDto = _mapper.Map<OptionDto>(optionModal);
            var pdf = await _highAlertService.DownloadReportForHighAlert(selectedDto.Id);
            if (pdf == null)
                return NoContent();
            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(pdf, MediaTypeNames.Application.Pdf);
        }
        [HttpPost]
        [Route("downloadExcelReport")]
        public async Task<IActionResult> DownloadExcelReport([FromBody] OptionModel optionModal)
        {
            var selectedDto = _mapper.Map<OptionDto>(optionModal);
            var excel = await _highAlertService.DownloadReportForHighAlertAsExcel(selectedDto.Id);
            if (excel == null)
                return NoContent();

            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        }
        [Route("get")]
        [HttpPost]
        public async Task<IActionResult> GetHighAlertsPortal([FromBody] HighAlertPortalFilterModel portalHighAlertFilter)
        {
            try
            {
                if (_userService.GetLoggedUserId() > 0)
                {
                    _appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();
                }
                else
                {


                }
            }
            catch (NotFoundException) { }


            var filter = _mapper.Map<HighAlertPortalFilter>(portalHighAlertFilter);

            var (highAlertDto, totalCount) = await _highAlertService.GetHighAlertsAsync(filter);

            IReadOnlyCollection<HighAlertGridItemModel> result = _mapper.Map<IReadOnlyCollection<HighAlertGridItemModel>>(highAlertDto);

            return Ok(new { items = result, totalCount = totalCount });
        }
        [Route("highAlertPotentialAreas")]
        [HttpGet]
        public async Task<IActionResult> GetHighAlertPotentialAreas()
        {
            var areas = await _highAlertService.GetHighAlertPotentialAreas();
            return Ok(areas);
        }

        [Route("potentialAreas/{id:int}")]
        [HttpPut]
        public async Task<IActionResult> GetHighAlertPotentialAreasForCategory(int categoryId)
        {
           // var areas = await _highAlertService.GetHighAlertPotentialAreas(categoryId);
            return Ok();
        }

        [Route("statusChange")]
        [HttpPost]

        public async Task<IActionResult> ChangeHighAlertStatus([FromBody] HighAlertStatusChangeModel alertStatusModel)
        {
            var statusDto = _mapper.Map<OptionDto>(alertStatusModel.Status);
            var highAlertDto = await _highAlertService.SetHighAlertStatus(alertStatusModel.HighAlertId, statusDto,alertStatusModel.UserNotes, alertStatusModel.ChangeBy);
            var model = _mapper.Map<HighAlertGridItemModel>(highAlertDto);
            return Ok(model);
        }

        [Route("statistics")]
        [HttpPost]

        public async Task<IActionResult> GetHighAlertStatistics([FromBody] OptionModel optionModal)
        {
            var facilityDto = _mapper.Map<OptionDto> (optionModal);
            var highAlertStatistics = await _highAlertService.GetHighAlertStatistics(facilityDto);
          //  var model = _mapper.Map<HighAlertGridItemModel>(highAlertDto);
            return Ok(highAlertStatistics);
        }
    }
}
