using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SHARP.BusinessLogic.DTO.ReportRequest;
using SHARP.BusinessLogic.Interfaces;
using SHARP.BusinessLogic.Services;
using SHARP.Common.Constants;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.Common.Models;
using SHARP.Filters;
using SHARP.ViewModels.Audit;
using SHARP.ViewModels.Common;
using SHARP.ViewModels.ReportRequest;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;

namespace SHARP.Controllers
{
    [Route("api/report/request")]
    [ApiController]
    [Authorize]
    public class ReportRequestController : Controller
    {
        private readonly IReportRequestService _reportRequestService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AppConfig _appConfig;

        public ReportRequestController(
            IReportRequestService reportRequestService,
            IMapper mapper,
            IUserService userService,
            AppConfig appConfig)
        {
            _reportRequestService = reportRequestService;
            _mapper = mapper;
            _userService = userService;
            _appConfig = appConfig;
        }

        [Route("get")]
        [HttpPost]
        public async Task<IActionResult> GetReportRequests([FromBody] ReportRequestFilterModel filterModel)
        {
            _appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();

            var reportRequestFilter = _mapper.Map<ReportRequestFilter>(filterModel);

            IReadOnlyCollection<ReportRequestDto> requestsDto = await _reportRequestService.GetReportRequestsAsync(reportRequestFilter);

            IReadOnlyCollection<ReportRequestGridModel> requests = _mapper.Map<IReadOnlyCollection<ReportRequestGridModel>>(requestsDto);

            return Ok(requests);
        }

        [Route("filters")]
        [HttpPost]
        public async Task<IActionResult> GetFilters([FromBody] ReportRequestFilterColumnSourceModel columnData)
        {
            _appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();

            var columnFilter = _mapper.Map<ReportRequestFilterColumnSource<ReportRequestFilterColumn>>(columnData);

            IReadOnlyCollection<FilterOption> optionsDto = await _reportRequestService.GetFilterColumnSourceDataAsync(columnFilter);

            var options = _mapper.Map<IReadOnlyCollection<FilterOptionModel>>(optionsDto);

            return Ok(options);
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddReportRequest([FromBody] PdfFilterModel filterModel)
        {
            var addReportRequestDto = _mapper.Map<AddReportRequestDto>(filterModel);

            MessageResponse result = await _reportRequestService.AddReportRequestAsync(addReportRequestDto);

            return Ok(result);
        }

        [Route("{report}")]
        [HttpGet]
        public async Task<IActionResult> DownloadReport(string report)
        {
            byte[] pdf = await _reportRequestService.GetReportAsync(report);

            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(pdf, MediaTypeNames.Application.Pdf);
        }

        [Route("{id:int}/resend")]
        [HttpPut]
        public async Task<IActionResult> ResendReportRequest(int id)
        {
            await _reportRequestService.ResendReportRequestAsync(id);

            return NoContent();
        }
    }
}
