using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.Organization;
using SHARP.BusinessLogic.Interfaces;
using SHARP.BusinessLogic.Services;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.Filters;
using SHARP.ViewModels.Common;
using SHARP.ViewModels.Facilitty;
using SHARP.ViewModels.Facility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SHARP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class FacilitiesController : Controller
    {
        private readonly IFacilityService _facilityService;
        private readonly IMapper _mapper;

        public FacilitiesController(IFacilityService facilityService, IMapper mapper)
        {
            _facilityService = facilityService;
            _mapper = mapper;
        }

        [Route("get")]
        [HttpPost]
        public async Task<IActionResult> GetFacilities([FromBody] FacilityFilterModel facilityFilter)
        {
            var filter = _mapper.Map<FacilityFilter>(facilityFilter);

            var facilities = await _facilityService.GetFacilitiesAsync(filter);

            var result = _mapper.Map<IReadOnlyCollection<FacilityGridItemModel>>(facilities);

            return Ok(result);
        }

        [Route("filters")]
        [HttpPost]
        public async Task<IActionResult> GetFilters([FromBody] FacilityFilterColumnSourceModel columnData)
        {
            var columnFilter = _mapper.Map<FacilityFilterColumnSource<FacilityFilterColumn>>(columnData);

            IReadOnlyCollection<FilterOption> optionsDto = await _facilityService.GetFilterColumnSourceDataAsync(columnFilter);

            var options = _mapper.Map<IReadOnlyCollection<FilterOptionModel>>(optionsDto);

            return Ok(options);
        }

        [Route("timezones")]
        [HttpGet]
        public async Task<IActionResult> GetTimeZoneOptions()
        {
            IReadOnlyCollection<TimeZoneOptionDto> timeZonesDto = await _facilityService.GetTimeZoneOptionsAsync();

            var timeZones = _mapper.Map<IReadOnlyCollection<TimeZoneOptionModel>>(timeZonesDto.OrderBy(timeZone => timeZone.TimeZoneOffset));

            return Ok(timeZones);
        }

        [Route("{id:int}/details")]
        [HttpGet]
        public async Task<IActionResult> GetFacilityDetails(int id)
        {
            FacilityDetailsDto facilityDto = await _facilityService.GetFacilityDetailsAsync(id);

            var result = _mapper.Map<FacilityDetailsModel>(facilityDto);

            return Ok(result);
        }
        [Route("{id:int}/emailRecipients")]
        [HttpGet]
        public async Task<IActionResult> GetEmailRecipients(int id)
        {
            var emails = await _facilityService.GetEmailRecipients(id);

            if(emails == null)
                return NoContent();

            return Ok(emails);
        }
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddFacility([FromBody] AddFacilityModel addFacility)
        {
            var addFacilityDto = _mapper.Map<AddFacilityDto>(addFacility);

            bool result = await _facilityService.AddFacilityAsync(addFacilityDto);

            return Ok(result);
        }
        [HttpPost]
        [Route("addemailsfacility")]
        public async Task<IActionResult> AddEmailsFacility([FromBody] AddRecipientsEmail addEmailsFacility)
        {
            var addEmailRecipientsFacilityDto = _mapper.Map<AddEmailRecipientsDto>(addEmailsFacility);

            bool result = await _facilityService.AddFacilityRecipientsAsync(addEmailRecipientsFacilityDto);

            return Ok(result);
        }
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditFacility([FromBody] EditFacilityModel editFacility)
        {
            var editFacilityDto = _mapper.Map<EditFacilityDto>(editFacility);

            bool result = await _facilityService.EditFacilityAsync(editFacilityDto);

            return Ok(result);
        }
    }

    [Route("api/facility/options")]
    [ApiController]
    public class FacilityOptionsController : Controller
    {
        private readonly IFacilityService _facilityService;
        private readonly IMapper _mapper;

        public FacilityOptionsController(IFacilityService facilityService, IMapper mapper)
        {
            _facilityService = facilityService;
            _mapper = mapper;
        }

        [Route("organization/{organizationId:int}")]
        [HttpGet]
        public async Task<IActionResult> GetFacilityOptions(int organizationId)
        {
            IReadOnlyCollection<FacilityOptionDto> facilitiesDto = await _facilityService.GetFacilityOptionsAsync(organizationId);

            var facilityOptions = _mapper.Map<IReadOnlyCollection<FacilityOptionModel>>(facilitiesDto);

            return Ok(facilityOptions);
        }

        [Route("facility/{facilityId:int}")]
        [HttpGet]
        public async Task<IActionResult> GetFacilityDetails(int facilityID)
        {
            var facilityDto = await _facilityService.GetFacilityDetailsAsync(facilityID);


            var facility = _mapper.Map<FacilityDetailsDto>(facilityDto);

            return Ok(facility);
        }

        [HttpPost]
        public async Task<IActionResult> GetFacilityFilteredOptions([FromBody] FacilityOptionFilterModel facilityFilter)
        {
            var filter = _mapper.Map<FacilityOptionFilter>(facilityFilter);

            IReadOnlyCollection<FacilityOptionDto> facilitiesDto = await _facilityService.GetFacilityOptionsAsync(filter);

            var facilityOptions = _mapper.Map<IReadOnlyCollection<FacilityOptionModel>>(facilitiesDto);

            return Ok(facilityOptions);
        }
        [Route("organizationPortal/{organizationId:int}")]
        [HttpGet]
        public async Task<IActionResult> GetFacilityOptionsForPortal(int organizationId)
        {
            IReadOnlyCollection<FacilityOptionDto> facilitiesDto = await _facilityService.GetFacilityOptionsAsync(organizationId);
            IReadOnlyCollection<FacilityOptionModel> facilityOptions = null;
            if (!facilitiesDto.Any())
            {
                facilityOptions = _mapper.Map<IReadOnlyCollection<FacilityOptionModel>>(facilitiesDto);

                return Ok(facilityOptions);
            }
            var facDTOs = facilitiesDto.ToList();

            facDTOs.Insert(0, new FacilityOptionDto { Name = "All" , Id = 0 });
            facilityOptions = _mapper.Map<IReadOnlyCollection<FacilityOptionModel>>(facDTOs);

            return Ok(facilityOptions);
        }
    }
}
