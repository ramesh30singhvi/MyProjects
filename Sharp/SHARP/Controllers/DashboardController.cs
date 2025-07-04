using AutoMapper;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SHARP.BusinessLogic.DTO.AuditorProductivityDashboard;
using SHARP.BusinessLogic.DTO.Dashboard;
using SHARP.BusinessLogic.Interfaces;
using SHARP.BusinessLogic.Services;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;
using SHARP.ViewModels;
using SHARP.ViewModels.AuditorProductivityDashboard;
using SHARP.ViewModels.Common;
using SHARP.ViewModels.Dashboard;
using SHARP.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly IDasbhoardInputService _dasbhoardInputService;

        private readonly IMapper _mapper;

        public DashboardController(
            IAuditService auditService,
            IDasbhoardInputService dasbhoardInputService,
            IMapper mapper)
        {
            _auditService = auditService;
            _dasbhoardInputService = dasbhoardInputService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> GetAuditsKPI([FromBody] DashboardFilterModel dashboardFilter)
        {

            var filter = _mapper.Map<DashboardFilter>(dashboardFilter);

            IReadOnlyCollection<AuditKPIDto> auditsCountDto = await _auditService.GetAuditsKPIAsync(filter);

            IReadOnlyCollection<AuditKPIModel> auditsCountModel = _mapper.Map<IReadOnlyCollection<AuditKPIModel>>(auditsCountDto);

            return Ok(auditsCountModel);
        }

        [Route("duedates")]
        [HttpPost]
        public async Task<IActionResult> GetDueDatesCount([FromBody] DashboardFilterModel dashboardFilter)
        {
            var filter = _mapper.Map<DashboardFilter>(dashboardFilter);

            AuditsDueDateCountDto auditsDueDateCountDto = await _auditService.GetAuditsDueDateCountAsync(filter);

            var auditsDueDateCount = _mapper.Map<AuditsDueDateCountModel>(auditsDueDateCountDto);

            return Ok(auditsDueDateCount);
        }

        
    }

    [Route("api/dashboard/input")]
    [ApiController]
    public class DashboardInputController : Controller
    {
        private readonly IDasbhoardInputService _dasbhoardInputService;


        public DashboardInputController(IDasbhoardInputService dasbhoardInputService)
        {
            _dasbhoardInputService = dasbhoardInputService;
        }
        [Route("data")]
        [HttpPost]
        public async Task<IActionResult> GetDashboardInput([FromBody] DashboardInputFilterDto dashboardInputFilter)
        {
            return Ok(await _dasbhoardInputService.GetDashboardInputAsync(dashboardInputFilter));
        }

        [Route("auditSummary")]
        [HttpGet]
        public async Task<IActionResult> GetDashboardInputAuditSummary()
        {
            return Ok(await _dasbhoardInputService.GetDashboardInputAuditSummary());
        }

        [HttpPost]
        public async Task<IActionResult> SaveDashboardInputValues([FromBody] SaveDashboardInputValuesDto[] saveDashboardInputValuesDtos)
        {
            await _dasbhoardInputService.SaveDashboardInputValues(saveDashboardInputValuesDtos);
            return Ok();
        }

        [Route("table")]
        [HttpPost]
        public async Task<IActionResult> AddTableToDashboardInput([FromBody] AddTableDto addTableDto)
        {
            return Ok(await _dasbhoardInputService.AddTableToDashboardInput(addTableDto));
        }

        [Route("table")]
        [HttpPut]
        public async Task<IActionResult> EditTableToDashboardInput([FromBody] EditDashboardInputDto editDashboardInputDto)
        {
            return Ok(await _dasbhoardInputService.EditTableToDashboardInput(editDashboardInputDto));
        }

        [Route("table/{id:int}/{organizationId:int}")]
        [HttpDelete]
        public async Task<IActionResult> DeleTableToDashboardInput(int id, int organizationId)
        {
            var editDashboardInputDto = new EditDashboardInputDto();
            editDashboardInputDto.Id = id;
            editDashboardInputDto.OrganizationId = organizationId;
            return Ok(await _dasbhoardInputService.DeleTableToDashboardInput(editDashboardInputDto));
        }

        [Route("group")]
        [HttpPost]
        public async Task<IActionResult> AddGroupToDashboardInput([FromBody] AddGroupDto addGroupDto)
        {
            return Ok(await _dasbhoardInputService.AddGroupToDashboardInput(addGroupDto));
        }

        [Route("group")]
        [HttpPut]
        public async Task<IActionResult> EditGroupToDashboardInput([FromBody] EditDashboardInputDto editDashboardInputDto)
        {
            return Ok(await _dasbhoardInputService.EditGroupToDashboardInput(editDashboardInputDto));
        }

        [Route("group/{id:int}/{organizationId:int}")]
        [HttpDelete]
        public async Task<IActionResult> DeleGroupToDashboardInput(int id, int organizationId)
        {
            var editDashboardInputDto = new EditDashboardInputDto();
            editDashboardInputDto.Id = id;
            editDashboardInputDto.OrganizationId = organizationId;
            return Ok(await _dasbhoardInputService.DeleteGroupToDashboardInput(editDashboardInputDto));
        }

        [Route("element")]
        [HttpPost]
        public async Task<IActionResult> AddElementToDashboardInput([FromBody] AddElementDto addElementDto)
        {
            return Ok(await _dasbhoardInputService.AddElementToDashboardInput(addElementDto));
        }

        [Route("element")]
        [HttpPut]
        public async Task<IActionResult> EditElementToDashboardInput([FromBody] EditDashboardInputDto editDashboardInputDto)
        {
            return Ok(await _dasbhoardInputService.EditElementToDashboardInput(editDashboardInputDto));
        }

        [Route("element/{id:int}/{organizationId:int}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteElementToDashboardInput(int id, int organizationId)
        {
            var editDashboardInputDto = new EditDashboardInputDto();
            editDashboardInputDto.Id = id;
            editDashboardInputDto.OrganizationId = organizationId;
            return Ok(await _dasbhoardInputService.DeleteElementToDashboardInput(editDashboardInputDto));
        }

        [Route("upload")]
        [HttpPost]
        public async Task<IActionResult> UploadData([FromForm] int organizationId, [FromForm] int elementId, [FromForm] IFormFile file)
        {
            long size = file.Length;
            var filePath = "";
            Dictionary<String, int> Pairs = new Dictionary<String, int>();
            try
            {
                DashboardInputElement dashboardInputElement = await _dasbhoardInputService.GetDashboardInputElement(elementId);
                if (file.Length > 0)
                {
                    filePath = Path.GetTempFileName();
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var reader = new PdfReader(@filePath);

                    byte[] contentBytes = null;

                    for (var pageNum = 1; pageNum <= reader.NumberOfPages; pageNum++)
                    {
                        var pageBytes = reader.GetPageContent(pageNum);

                        if (contentBytes == null)
                            contentBytes = pageBytes;
                        else
                            contentBytes = contentBytes.Concat(pageBytes).ToArray();
                    }

                    var tokenizer = new PrTokeniser(new RandomAccessFileOrArray(contentBytes));
                    var stringsList = new List<string>();
                    var facilitiesList = new List<string>();
                    
                    while (tokenizer.NextToken())
                    {
                        if (tokenizer.TokenType == PrTokeniser.TK_STRING)
                        {
                            stringsList.Add(tokenizer.StringValue);
                        }
                    }

                    string hyphenSummary = stringsList.FirstOrDefault(stringToCheck => stringToCheck.Contains(" - Summary"));
                    string onlySummary = string.Empty;

                    if (string.IsNullOrWhiteSpace(hyphenSummary))
                    {
                        onlySummary = stringsList.FirstOrDefault(stringToCheck => stringToCheck == "Summary");
                    }

                    string facilityName = string.Empty;
                    int facilityTotalResidents = 0;
                    bool isDischargesFound = false;

                    if (!string.IsNullOrWhiteSpace(hyphenSummary))
                    {
                        for (int i = 0; i < stringsList.Count; i++)
                        {
                            if (stringsList[i].Contains(" - Summary"))
                            {
                                facilityName = stringsList[i].Replace(" - Summary", "").Trim();
                            }
                            else if(stringsList[i].Contains("Total Admissions:"))
                            {
                                string admissionsCount = stringsList[i + 1];
                                Pairs.Add(facilityName, int.Parse(admissionsCount));
                            }
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(onlySummary))
                    {
                        for (int i = 0; i < stringsList.Count; i++)
                        {
                            if (stringsList[i].Contains("Total Admissions:"))
                            {
                                string admissionsCount = stringsList[i + 1];
                                facilityTotalResidents = int.Parse(admissionsCount);
                            }
                            else if (stringsList[i].Contains("Facilities") && stringsList[i + 1].StartsWith(":"))
                            {
                                if (facilityTotalResidents > 0)
                                    Pairs.Add(stringsList[i + 1].Replace(":", "").Trim(), facilityTotalResidents);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < stringsList.Count; i++)
                        {
                            if (!isDischargesFound && stringsList[i].Contains("Total:"))
                            {
                                string admissionsCount = stringsList[i + 1];
                                facilityTotalResidents += int.Parse(admissionsCount);
                            }
                            else if (!string.IsNullOrWhiteSpace(stringsList[i]) && stringsList[i].Trim() == "Discharges")
                            {
                                isDischargesFound = true;
                            }
                            else if (stringsList[i].Contains("Facilities") && stringsList[i + 1].StartsWith(":"))
                            {
                                if (facilityTotalResidents > 0)
                                    Pairs.Add(stringsList[i + 1].Replace(":", "").Trim(), facilityTotalResidents);
                            }
                        }
                    }


                    reader.Close();
                }

                if (Pairs.Count == 0)
                {
                    return BadRequest("No facilities or admission data found in uploaded file.");
                }

                int updateCount = await _dasbhoardInputService.FillElementsWithData(organizationId, dashboardInputElement, Pairs);

                if (updateCount == 0)
                {
                    return BadRequest("No facilities found in organization to update.");
                }

                return Ok(updateCount);
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid file or error occurred while uploading data.");
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AuditorProductivityDashboardController : ControllerBase
    {
        private readonly IAuditorProductivityDashboardService _auditorProductivityDashboardService;
        private readonly IDasbhoardInputService _dasbhoardInputService;

        private readonly IMapper _mapper;

        public AuditorProductivityDashboardController(
            IAuditorProductivityDashboardService auditorProductivityDashboardService,
            IMapper mapper)
        {
            _auditorProductivityDashboardService = auditorProductivityDashboardService;
            _mapper = mapper;
        }

        [Route("getInputData")]
        [HttpPost]
        public async Task<IActionResult> GetInputData([FromBody] AuditorProductivityInputFilterModel filterModel)
        {
            var filter = _mapper.Map<AuditorProductivityInputFilter>(filterModel);

            var data = await _auditorProductivityDashboardService.GetAuditorProductivityInputAsync(filter);

            var result = _mapper.Map<IReadOnlyCollection<AuditorProductivityInputDto>>(data);

            return Ok(result);
        }

        [Route("getInputfilters")]
        [HttpPost]
        public async Task<IActionResult> GetInputFilters([FromBody] AuditorProductivityInputFilterColumnSourceModel columnData)
        {
            var columnFilter = _mapper.Map<AuditorProductivityInputFilterColumnSource<AuditorProductivityInputFilterColumn>>(columnData);

            IReadOnlyCollection<FilterOption> optionsDto = await _auditorProductivityDashboardService.GetInputFilterColumnSourceDataAsync(columnFilter);

            var options = _mapper.Map<IReadOnlyCollection<FilterOptionModel>>(optionsDto);

            return Ok(options);
        }

        [Route("getAHTPerAuditTypeData")]
        [HttpPost]
        public async Task<IActionResult> GetAHTPerAuditTypeData([FromBody] AuditorProductivityAHTPerAuditTypeFilterModel filterModel)
        {
            var filter = _mapper.Map<AuditorProductivityAHTPerAuditTypeFilter>(filterModel);

            var data = await _auditorProductivityDashboardService.GetAuditorProductivityAHTPerAuditTypeAsync(filter);

            var result = _mapper.Map<IReadOnlyCollection<AuditorProductivityAHTPerAuditTypeDto>>(data);

            return Ok(result);
        }

        [Route("getAHTPerAuditTypefilters")]
        [HttpPost]
        public async Task<IActionResult> GetAHTPerAuditTypeFilters([FromBody] AuditorProductivityAHTPerAuditTypeFilterColumnSourceModel columnData)
        {
            var columnFilter = _mapper.Map<AuditorProductivityAHTPerAuditTypeFilterColumnSource<AuditorProductivityAHTPerAuditTypeFilterColumn>>(columnData);

            IReadOnlyCollection<FilterOption> optionsDto = await _auditorProductivityDashboardService.GetAHTPerAuditTypeFilterColumnSourceDataAsync(columnFilter);

            var options = _mapper.Map<IReadOnlyCollection<FilterOptionModel>>(optionsDto);

            return Ok(options);
        }

        [Route("getSummaryPerAuditorData")]
        [HttpPost]
        public async Task<IActionResult> GetSummaryPerAuditorData([FromBody] AuditorProductivitySummaryPerAuditorFilterModel filterModel)
        {
            var filter = _mapper.Map<AuditorProductivitySummaryPerAuditorFilter>(filterModel);

            var data = await _auditorProductivityDashboardService.GetAuditorProductivitySummaryPerAuditorAsync(filter);

            var result = _mapper.Map<IReadOnlyCollection<AuditorProductivitySummaryPerAuditorDto>>(data);

            return Ok(result);
        }

        [Route("getSummaryPerAuditorfilters")]
        [HttpPost]
        public async Task<IActionResult> GetSummaryPerAuditorFilters([FromBody] AuditorProductivitySummaryPerAuditorFilterColumnSourceModel columnData)
        {
            var columnFilter = _mapper.Map<AuditorProductivitySummaryPerAuditorFilterColumnSource<AuditorProductivitySummaryPerAuditorFilterColumn>>(columnData);

            IReadOnlyCollection<FilterOption> optionsDto = await _auditorProductivityDashboardService.GetSummaryPerAuditorFilterColumnSourceDataAsync(columnFilter);

            var options = _mapper.Map<IReadOnlyCollection<FilterOptionModel>>(optionsDto);

            return Ok(options);
        }


        [Route("getSummaryPerFacilityData")]
        [HttpPost]
        public async Task<IActionResult> GetSummaryPerFacilityData([FromBody] AuditorProductivitySummaryPerFacilityFilterModel filterModel)
        {
            if (!ModelState.IsValid)
            {
                return NoContent();
            }

            if(filterModel?.Organization.Id.GetValueOrDefault() == 0)
            {
                return NoContent();
            }
         
            var filter = _mapper.Map<AuditorProductivitySummaryPerFacilityFilter>(filterModel);

            var data = await _auditorProductivityDashboardService.GetAuditorProductivitySummaryPerFacilityAsync(filter);

          //  var result = _mapper.Map<IReadOnlyCollection<AuditorProductivitySummaryPerFacilityDto>>(data);

            return Ok(data);
        }

        [Route("getFormTags")]
        [HttpGet]
        public async Task<IActionResult> GetAuditFormTags()
        {
            var tags = await _auditorProductivityDashboardService.GetFormTags();
            return Ok(tags);
        }


    }
}
