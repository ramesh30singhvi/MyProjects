using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.ViewModels.Form;
using SHARP.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using TableauAPI;
using SHARP.ViewModels.Common;
using SHARP.Filters;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics;
using System.Linq;
using SHARP.Common.Enums;
using DocumentFormat.OpenXml.Spreadsheet;
using SHARP.ViewModels;
using SHARP.DAL.Models;

namespace SHARP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, Reviewer, Facility")]
    public class ReportsController : ControllerBase
    {
        private readonly ITableauReportService _tableauReportService;
        private readonly IReportService _reportService;

        private readonly IMapper _mapper;
        IConfiguration _configuration;
        public ReportsController(
            ITableauReportService tableauReportService,
            IReportService reportService,
            IMapper mapper,
            IConfiguration configuration)
        {
            _tableauReportService = tableauReportService;
            _reportService = reportService;
            _mapper = mapper;
            _configuration = configuration;
        }

        /*[HttpGet]
        public async Task<IActionResult> GetReportsUrls()
        {
            var url = await _tableauReportService.GetTableauReportUrlAsync();

            return Ok(url);
        }*/

        [HttpPost]
        public async Task<IActionResult> GetReports([FromBody] ReportFilterModel reportFiler)
        {
            var filter = _mapper.Map<ReportFilter>(reportFiler);

            var reports = await _tableauReportService.GetReportsAsync(filter);

            var result = _mapper.Map<IReadOnlyCollection<ReportModel>>(reports);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetReports([FromQuery] int id)
        {
            string tableauUserName = _configuration.GetValue<string>("TableauUsername");
            string tableauUrl = _configuration.GetValue<string>("TableauServerUrl");

            TableauClient client = new TableauClient();
            string token = await client.GetTokenAsync(tableauUrl, tableauUserName);
            
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest();
            }

            var report = await _tableauReportService.GetReportsByIdAsync(id);
            var result = _mapper.Map<ReportModel>(report);

            result.ReportUrl = $"{tableauUrl}/trusted/{token}/views/{report.TableauUrl}";

            return Ok(result);
        }

        [Route("filters")]
        [HttpGet]
        public async Task<IActionResult> GetFilters([FromQuery] FilterColumnSource<ReportColumn> columnData)
        {
            var data = await _tableauReportService.GetFilterColumnSourceDataAsync(columnData);
            return Ok(data);
        }
        [Route("fall")]
        [HttpPost]
        public async Task<IActionResult> GetFallReports([FromBody] ReportFallModel reportFallModel)
        {
            var data = await _reportService.GetFallReport(_mapper.Map<ReportFallFilterModel>(reportFallModel));


            return Ok(data);
        }

        [Route("downloadFall")]
        [HttpPost]
        public async Task<IActionResult> GetDownloadFallReports([FromBody] ReportFallModel reportFallModel)
        {
            var pdf = await _reportService.GetDownloadFallReport(_mapper.Map<ReportFallFilterModel>(reportFallModel));
            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(pdf, MediaTypeNames.Application.Pdf);
        }

        [Route("wound")]
        [HttpPost]
        public async Task<IActionResult> GetWoundReports([FromBody] ReportFallModel reportFallModel)
        {
            var data = await _reportService.GetWoundReport(_mapper.Map<ReportFallFilterModel>(reportFallModel));


            return Ok(data);
        }

        [Route("downloadWound")]
        [HttpPost]
        public async Task<IActionResult> GetDownloadWoundReports([FromBody] ReportFallModel reportFallModel)
        {
            var pdf = await _reportService.GetDownloadWoundReport(_mapper.Map<ReportFallFilterModel>(reportFallModel));
            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(pdf, MediaTypeNames.Application.Pdf);
        }

        [Route("downloadCriteria")]
        [HttpPost]
        public async Task<IActionResult> GetDownloadCriteriaReport([FromBody] ReportCriteriaModel reportCriteriaModel)
        {
            var excel = await _reportService.GetDownloadCriteriaReport(_mapper.Map<ReportCriteriaFilter>(reportCriteriaModel));
            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CriteriaReport.xlsx");
        }

    }


    [Route("api/reportAI")]
    [ApiController]
    [Authorize(Roles = "Admin, Reviewer, Auditor")]
    public class ReportAIController : Controller
    {
        IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IReportService _reportService;
        private readonly IReportAIService _reportAIService;
        private TelemetryClient _telemetryClient;
        public ReportAIController(IReportService reportService,
            IMapper mapper, IReportAIService reportsAIService,
            IConfiguration configuration)
        {
            _reportService = reportService;
            _mapper = mapper;
            _configuration = configuration;
            _reportAIService = reportsAIService;

            InitTelemetryClient();
        }
        private void InitTelemetryClient()
        {
            var keyNames = _configuration.AsEnumerable().ToDictionary(x => x.Key, x => x.Value);
            if (keyNames.ContainsKey("APPLICATIONINSIGHTS_CONNECTION_STRING"))
            {
                var connectionString = _configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var telemetryConfiguration = new TelemetryConfiguration();
                    telemetryConfiguration.ConnectionString = connectionString;
                    _telemetryClient = new TelemetryClient(telemetryConfiguration);
                }
            }
        }

        [Route("downloadReportbyAI")]
        [HttpPost]
        public async Task<IActionResult> DownloadReportbyAI([FromBody] AIResultViewModel viewmodel)
        {
            var reportDto = _mapper.Map<AzureReportProcessResultDto>(viewmodel);
            var pdf = await _reportService.Create24KeywordReportByAI(reportDto);
            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(pdf, MediaTypeNames.Application.Pdf);
        }

        [Route("uploadForAnalysis")]
        [HttpPost]
        public async Task<IActionResult> UploadForAnalysis([FromForm] UploadFileModel fileModel)
        {
            var telemetryProperties = new Dictionary<string, string>() { { "Start UploadForAnalysis action", $"Time: {DateTime.UtcNow}. OrganizationId: {fileModel.OrganizationId ?? 0.ToString()}. FacilityId: {fileModel.FacilityId ?? 0.ToString()}." } };
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {

                if (fileModel == null)
                {
                    stopWatch.Stop();
                    telemetryProperties.Add("End UploadForAnalysis action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Return: BadRequest.");
                    return BadRequest("No file selected");
                }

                if (fileModel.PdfFile == null)
                {
                    stopWatch.Stop();
                    telemetryProperties.Add("End UploadForAnalysis action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Return: BadRequest.");
                    return BadRequest("no PDF report selected");
                }

                if (fileModel.KeywordFileJson == null)
                {
                    stopWatch.Stop();
                    telemetryProperties.Add("End UploadForAnalysis action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Return: BadRequest.");
                    return BadRequest("no Keyword selected");
                }

                var reportUploadAzureDto = _mapper.Map<PdfReportUploadAzureDto>(fileModel);
                var pdfReportAnalysisDto = await _reportAIService.ParsePdfAndBuildIndex(reportUploadAzureDto);
                var model = _mapper.Map<PdfReportAnalysisModel>(pdfReportAnalysisDto);

                stopWatch.Stop();
                telemetryProperties.Add("End UploadForAnalysis action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Return: Ok.");
                return Ok(model);
            }
            catch (Exception exception)
            {
                stopWatch.Stop();
                telemetryProperties.Add("Error UploadForAnalysis action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Exception: {exception.ToString()}.");
                return Ok(false);
            }
            finally
            {
                // _telemetryClient?.TrackEvent(InsightConstants.AI_AUDITS_UPLOAD_FOR_ANALYSIS, telemetryProperties);
            }
            // file1 = Request.Form.Files[0];
            return Ok(true);
        }

        [Route("uploadForAnalysisV2")]
        [HttpPost]
        public async Task<IActionResult> UploadForAnalysisV2([FromForm] CreateAIReportViewModel fileModel)
        {
            var telemetryProperties = new Dictionary<string, string>() { { "Start UploadForAnalysis action", $"Time: {DateTime.UtcNow}. OrganizationId: {fileModel.OrganizationId ?? 0.ToString()}. FacilityId: {fileModel.FacilityId ?? 0.ToString()}." } };
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {

                if (fileModel == null)
                {
                    stopWatch.Stop();
                    telemetryProperties.Add("End UploadForAnalysis action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Return: BadRequest.");
                    return BadRequest("No file selected");
                }

                if (fileModel.PdfFile == null)
                {
                    stopWatch.Stop();
                    telemetryProperties.Add("End UploadForAnalysis action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Return: BadRequest.");
                    return BadRequest("no PDF report selected");
                }

                var reportUploadAzureDto = _mapper.Map<CreateAIReportDto>(fileModel);
                var auditAIV2ID = await _reportAIService.ParsePdfByPatients(reportUploadAzureDto);


                stopWatch.Stop();
                telemetryProperties.Add("End UploadForAnalysisV2 action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Return: Ok.");
                return Ok(auditAIV2ID);
            }
            catch (Exception exception)
            {
                stopWatch.Stop();
                telemetryProperties.Add("Error UploadForAnalysisV2 action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Exception: {exception.ToString()}.");
                return Ok(0);
            }
            finally
            {
                // _telemetryClient?.TrackEvent(InsightConstants.AI_AUDITS_UPLOAD_FOR_ANALYSIS, telemetryProperties);
            }
            // file1 = Request.Form.Files[0];
            return Ok(0);
        }

        //progressNotesFromAI
        [Route("progressNotesFromAI")]
        [HttpPost]
        public async Task<IActionResult> ProgressNotesFromAI([FromBody] PCCNotesViewModel viewmodel)
        {
            var notesDto = _mapper.Map<PCCNotesDto>(viewmodel);
            var result = await _reportAIService.SendToAnthropicAIService(notesDto);
            return Ok(new { items = result.Item1, error = result.Item2 });
        }

        [Route("uploadForProcess")]
        [HttpPost]
        public async Task<IActionResult> UploadForProcess([FromForm] UploadFileModel fileModel)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var telemetryProperties = new Dictionary<string, string>() { { "Start UploadForProcess action", $"Time: {DateTime.UtcNow}. OrganizationId: {fileModel.OrganizationId ?? 0.ToString()}. FacilityId: {fileModel.FacilityId ?? 0.ToString()}." } };
            try
            {
                if (fileModel == null)
                {
                    stopWatch.Stop();
                    telemetryProperties.Add("End UploadForProcess action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Return: BadRequest.");
                    return BadRequest("No file selected");
                }

                if (fileModel.BuidlWordIndex == null)
                {
                    stopWatch.Stop();
                    telemetryProperties.Add("End UploadForProcess action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Return: BadRequest.");
                    return BadRequest("It was the problem in parsing PDF or Keywords ");
                }

                var reportUploadAzureDto = _mapper.Map<PdfReportUploadAzureDto>(fileModel);
                var json = await _reportAIService.SendToAIAzureFunction(reportUploadAzureDto);
                if (json != null)
                {
                    stopWatch.Stop();
                    telemetryProperties.Add("End UploadForProcess action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Return: Ok(json).");
                    return Ok(json);
                }

                stopWatch.Stop();
                telemetryProperties.Add("End UploadForProcess action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Return: Ok(true).");
                return Ok(true);
            }
            catch (Exception exception)
            {
                stopWatch.Stop();
                telemetryProperties.Add("Error UploadForProcess action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Exception: {exception.ToString()}.");
                return Ok(false);
            }
            finally
            {
                _telemetryClient?.TrackEvent(InsightConstants.AI_AUDITS_UPLOAD_FOR_PROCESS, telemetryProperties);
            }

        }

        [Route("getReports")]
        [HttpPost]
        public async Task<IActionResult> GetReportAIContent([FromBody] AuditAIReportFilterModel reportFiler)
        {
            var filter = _mapper.Map<AuditAIReportFilter>(reportFiler);

            var reports = await _reportAIService.GetAIAuditV2ListAsync(filter);

            var result = _mapper.Map<IReadOnlyCollection<AIAuditViewModel>>(reports);

            return Ok(result);
        }
        [Route("{id:int}/getById")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var reportAIContent = await _reportAIService.GetAIAuditAsync(id);

                var viewModel = _mapper.Map<AIAuditReportV2ViewModel>(reportAIContent);
                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }

        [Route("downloadReportbyAIV2")]
        [HttpPost]
        public async Task<IActionResult> DownloadReportbyAIV2([FromBody] OptionModel viewmodel)
        {

            var pdf = await _reportService.Create24KeywordReportByAIV2(viewmodel.Id);
            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(pdf, MediaTypeNames.Application.Pdf);
        }

        [Route("addPatientKeySummary")]
        [HttpPost]
        public async Task<IActionResult> AddPatientKeywordSummaryToAIV2([FromBody] AuditAIPatientPdfNotesViewModel viewmodel)
        {

            var patientInfoDto = _mapper.Map<AuditAIPatientPdfNotesDto>(viewmodel);

            var savedDto = await _reportAIService.AddPatientInfoKeySummary(patientInfoDto);

            var SavedDtoviewModel = _mapper.Map<AuditAIPatientPdfNotesViewModel>(savedDto);
            return Ok(SavedDtoviewModel);

        }
        [Route("updatePatientKeySummary")]
        [HttpPost]
        public async Task<IActionResult> UpdatePatientKeywordSummaryToAIV2([FromBody] AuditAIPatientPdfNotesViewModel viewmodel)
        {

            var patientInfoDto = _mapper.Map<AuditAIPatientPdfNotesDto>(viewmodel);

            var savedDto = await _reportAIService.UpdatePatientInfoKeySummary(patientInfoDto);

            var SavedDtoviewModel = _mapper.Map<AuditAIPatientPdfNotesViewModel>(savedDto);
            return Ok(SavedDtoviewModel);

        }
        [Route("addKeyWordSummary")]
        [HttpPost]
        public async Task<IActionResult> AddKeyWordSummary([FromBody] AuditAIKeywordSummaryViewModel viewmodel)
        {

            var keySummary = _mapper.Map<AuditAIKeywordSummaryDto>(viewmodel);

            var savedDto = await _reportAIService.AddAuditKeySummary(keySummary);

            var SavedDtoviewModel = _mapper.Map<AuditAIKeywordSummaryViewModel>(savedDto);
            return Ok(SavedDtoviewModel);

        }

        [Route("updateKeyWordSummary")]
        [HttpPost]
        public async Task<IActionResult> UpdateKeyWordSummary([FromBody] AuditAIKeywordSummaryViewModel viewmodel)
        {

            var keySummary = _mapper.Map<AuditAIKeywordSummaryDto>(viewmodel);

            var savedDto = await _reportAIService.UpdateAuditKeySummary(keySummary);

            var SavedDtoviewModel = _mapper.Map<AuditAIKeywordSummaryViewModel>(savedDto);
            return Ok(SavedDtoviewModel);

        }

        [Route("updateAuditAIV2")]
        [HttpPost]
        public async Task<IActionResult> UpdateAuditAIV2([FromBody] AIAuditReportV2ViewModel viewmodel)
        {

            var auditDto = _mapper.Map<AuditAIReportV2Dto>(viewmodel);

            var savedDto = await _reportAIService.UpdateAuditReportV2(auditDto);

            var SavedDtoviewModel = _mapper.Map<AIAuditViewModel>(savedDto);
            return Ok(SavedDtoviewModel);

        }

        [Route("updateAIAuditV2Status")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> UpdateAIAuditV2Status([FromBody] ReportAIStatusRequestModel requestModel)
        {
            var status = await _reportAIService.UpdateAIAuditV2StatusAsync(requestModel.ReportAIContentId, requestModel.Status);

            if (status == null)
            {
                return BadRequest("Data not found in SetAIAuditStatus()");
            }

            return Ok(status);
        }

        [Route("{auditId:int}/{state:int}/updateState")]
        [HttpPost]
        public async Task<IActionResult> UpdateAIAuditV2State(int auditId, int state)
        {
            bool response = false;
            try
            {
                if (auditId > 0 && state > 0)
                {
                    response = await _reportAIService.UpdateAIAuditV2State(auditId, (AIAuditState)state);
                }
                else
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(response);
            }

        }

    }

    [Route("api/reportAIContent")]
    [ApiController]
    [Authorize(Roles = "Admin, Reviewer, Auditor")]
    public class ReportAIContentController : Controller
    {
        IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IReportService _reportService;
        private readonly IReportAIService _reportAIService;
        private TelemetryClient _telemetryClient;
        public ReportAIContentController(IReportService reportService,
            IMapper mapper, IReportAIService reportsAIService,
            IConfiguration configuration)
        {
            _reportService = reportService;
            _mapper = mapper;
            _configuration = configuration;
            _reportAIService = reportsAIService;
            InitTelemetryClient();

        }

        private void InitTelemetryClient()
        {
            var keyNames = _configuration.AsEnumerable().ToDictionary(x => x.Key, x => x.Value);
            if (keyNames.ContainsKey("APPLICATIONINSIGHTS_CONNECTION_STRING"))
            {
                var connectionString = _configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var telemetryConfiguration = new TelemetryConfiguration();
                    telemetryConfiguration.ConnectionString = connectionString;
                    _telemetryClient = new TelemetryClient(telemetryConfiguration);
                }
            }
        }
        [Route("getReport")]
        [HttpPost]
        public async Task<IActionResult> GetReportAIContent([FromBody] AuditAIReportFilterModel reportFiler)
        {
            var filter = _mapper.Map<AuditAIReportFilter>(reportFiler);

            var reports = await _reportService.GetReportAIContentAsync(filter);

            var result = _mapper.Map<IReadOnlyCollection<ReportAIContentModel>>(reports);

            return Ok(result);
        }

        [Route("filters")]
        [HttpPost]
        public async Task<IActionResult> GetFilters([FromBody] AuditAIReportFilterColumnSourceModel columnData)
        {
            var columnFilter = _mapper.Map<AuditAIReportFilterColumnSource<AuditAIReportFilterColumn>>(columnData);

            IReadOnlyCollection<FilterOption> optionsDto = await _reportService.GetFilterColumnSourceDataAsync(columnFilter);

            var options = _mapper.Map<IReadOnlyCollection<FilterOptionModel>>(optionsDto);

            return Ok(options);
        }

        [Route("{id:int}/getById")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                ReportAIContentDto reportAIContent = await _reportService.GetReportAIContentAsync(id);

                return Ok(reportAIContent);
            }catch(Exception ex)
            {
                return NoContent();
            }
        }

        [Route("downloadUpdatedReport")]
        [HttpPost]
        public async Task<IActionResult> downloadUpdatedReport([FromBody] AIResultViewModel viewmodel)
        {
            int Id = viewmodel.ReportAIContentId ?? 0;
            var reportAiDto = await _reportService.GetReportAIContentAsync(Id);
            if(reportAiDto == null) {
                return NoContent();
            }
            var pdf = await _reportService.CreateUpdated24KeywordReport(reportAiDto, Id);
            Response.Headers.Add(HeaderNames.ContentDisposition, DispositionTypeNames.Attachment);
            return File(pdf, MediaTypeNames.Application.Pdf);
        }

        [Route("updateAIAuditReport")]
        [HttpPost]
        public async Task<IActionResult> Update24KeywordReport([FromBody] AIResultViewModel viewmodel)
        {
            if(viewmodel.JsonResult == null)
                return BadRequest(0);

            if (viewmodel.JsonResult == "{}")
                return BadRequest(0);

            int Id = viewmodel.ReportAIContentId ?? 0;
            var reportDto = _mapper.Map<AzureReportProcessResultDto>(viewmodel);

            var reportAIContentDto = await _reportService.UpdateReportAIDataReportAsync(reportDto, Id);
            if (reportAIContentDto == null)
            {
                return BadRequest("Data not found in SetAIAuditStatus()");
            }
            return Ok(reportAIContentDto);
        }

        [Route("setAIAuditStatus")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> SetAIAuditStatus([FromBody] ReportAIStatusRequestModel requestModel)
        {
            var status = await _reportService.SetAIAuditStatusAsync(requestModel.ReportAIContentId, requestModel.Status);

            if (status == null)
            {
                return BadRequest("Data not found in SetAIAuditStatus()");
            }

            return Ok(status);
        }

        [Route("saveReportAIData")]
        [HttpPost]
        public async Task<IActionResult> SaveReportAIData([FromBody] AIResultViewModel viewmodel)
        {

            if (viewmodel.JsonResult == null)
                return BadRequest(0);

            if (viewmodel.JsonResult == "{}")
                return BadRequest(0);

            int id = 0;
            Stopwatch stopWatch = new Stopwatch();
            ReportAIContentDto reportContentDto = null;
            stopWatch.Start();
            var telemetryProperties = new Dictionary<string, string>() { { "Start SaveReportAIData action", $"Time: {DateTime.UtcNow}. OrganizationId: {viewmodel.Organization.Id.ToString()}. FacilityId: {viewmodel.Facility.Id.ToString()}." } };
            try
            {
                var reportDto = _mapper.Map<AzureReportProcessResultDto>(viewmodel);

                reportContentDto = await _reportService.SaveReportAIDataAsync(reportDto);

                stopWatch.Stop();
                telemetryProperties.Add("End SaveReportAIData action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Return: Ok. ID: " + reportContentDto?.Id.ToString() + ".");
            }
            catch(Exception ex)
            {
                stopWatch.Stop();
                telemetryProperties.Add("Error SaveReportAIData action", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Exception: {ex.ToString()}.");
                return BadRequest(0);
            }
            finally
            {
                _telemetryClient?.TrackEvent(InsightConstants.AI_AUDITS_SAVE_REPORT_AI_DATA, telemetryProperties);
            }
            return Ok(reportContentDto);
        }

        [Route("getAppSettingsValue/{key}")]
        [HttpGet]
        public async Task<IActionResult> GetAppSettingsValue(string key)
        {
            string value = string.Empty;
            try
            {
                value = _configuration[key] ?? string.Empty;
            }
            catch (Exception ex)
            {
                return BadRequest("");
            }
            return Ok(value);
        }


        [Route("{reportAIContentId:int}/{state:int}/updateState")]
        [HttpPost]
        public async Task<IActionResult> UpdateAIAuditState(int reportAIContentId, int state)
        {
            bool response = false;
            try
            {
                if (reportAIContentId > 0 && state > 0)
                {
                    response = await _reportService.UpdateAIAuditState(reportAIContentId, (AIAuditState)state);
                }
                else
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(response);
            }

        }

        [Route("checkOnlineStatus")]
        [HttpGet]
        public async Task<IActionResult> CheckOnlineStatus()
        {
            return Ok(true);
        }

    }

}
