using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.DTO.Organization;
using SHARP.BusinessLogic.Interfaces;
using SHARP.BusinessLogic.Services;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;
using SHARP.Filters;
using SHARP.ViewModels;
using SHARP.ViewModels.Common;
using SHARP.ViewModels.Form;
using SHARP.ViewModels.Organization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SHARP.Controllers
{
    [Route("api/organizations/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class OrganizationsAdnimController : Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly IDasbhoardInputService _dashboardInputService;
        private readonly IMapper _mapper;
        private readonly IFormService _formService;



        public OrganizationsAdnimController(IOrganizationService organizationService, IDasbhoardInputService dasbhoardInputService,
             IFormService formService,IMapper mapper)
        {
            _organizationService = organizationService;
            _dashboardInputService = dasbhoardInputService;
            _mapper = mapper;
            _formService = formService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganizations()
        {
            var dto = await _organizationService.GetAsync();
            var model = _mapper.Map<IEnumerable<OrganizationModel>>(dto);
            return Ok(model);
        }

        [Route("detailed")]
        [HttpGet]
        public IActionResult GetOrganizationsDetailed()
        {
            var dto =  _organizationService.GetDetailed();
            var model = _mapper.Map<IEnumerable<OrganizationDetailedModel>>(dto);
            return Ok(model);
        }


        [Route("detailed")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> AddOrganization([FromBody] AddOrganizationModel organization)
        {
            var addOrganizationDto = _mapper.Map<AddOrganizationDto>(organization);
            if(_organizationService.OrganizationExists(organization.Name))
                throw new Exception("Organization Name must be unique");
            
            ValidateEmails(organization.Recipients);
            OrganizationDetailedDto organizationDetailedDto = await _organizationService.AddAsync(addOrganizationDto);

            var organizationDetailedModel = _mapper.Map<OrganizationDetailedModel>(organizationDetailedDto);


            return Ok(organizationDetailedModel);
        }
        [Route("addHighAlert/{id:int}")]
        [HttpPut]

        public async Task<IActionResult> AddHighAlert(int id)
        {
            var formFilter = new FormVersionFilter();
            var orgFilter = new List<FilterOption>();
            orgFilter.Add(new FilterOption() { Id = id });

            formFilter.Organizations = orgFilter;


            var formsDTO = await _formService.GetFormVersionsAsync(formFilter);

            if (!formsDTO.ToArray().Any(x => x.Status == FormVersionStatus.Published))
                return Ok(false);

            var onlyPublishedForms = formsDTO.ToArray().Where(x => x.Status == FormVersionStatus.Published);

            foreach(var formVersion in onlyPublishedForms)
            {

                var draftformVersion =  await _formService.EditFormVersionAsync(formVersion.Id);

                await _formService.PublishFormVersionAsync(draftformVersion.Id, formVersion.Form.AllowEmptyComment, formVersion.Form.DisableCompliance, !draftformVersion.Form.UseHighAlert, draftformVersion.Form.AHTime.GetValueOrDefault());
            }
            return Ok(true);
        }
        [Route("detailed/{id:int}")]
        [HttpPut]
        [ValidateModel]
        public async Task<IActionResult> EditOrganization(int id, [FromBody] EditOrganizationModel editOrganization)
        {
            var editOrganizationDto = _mapper.Map<EditOrganizationDto>(editOrganization);
            editOrganizationDto.Id = id;

            if (_organizationService.OrganizationExists(id, editOrganization.Name))
                throw new Exception("Organization Name must be unique");
            
            ValidateEmails(editOrganization.Recipients);
            bool result = await _organizationService.EditOrganizationAsync(editOrganizationDto);


            return Ok(result);
        }

        [Route("forms/get")]
        [HttpPost]
        public async Task<IActionResult> GetOrganizationForms([FromBody] FormFilterModel formFilter)
        {
            var filter = _mapper.Map<FormFilter>(formFilter);

            var forms = await _organizationService.GetOrganizationFormsAsync(filter);

            var result = _mapper.Map<IReadOnlyCollection<FormGridItemModel>>(forms);

            return Ok(result);
        }

        [Route("forms/filters")]
        [HttpPost]
        public async Task<IActionResult> GetOrganizationFormFilters([FromBody] OrganizationFormFilterColumnSourceModel columnData)
        {
            var columnFilter = _mapper.Map<OrganizationFormFilterColumnSource<OrganizationFormFilterColumn>>(columnData);

            IReadOnlyCollection<FilterOption> optionsDto = await _organizationService.GetOrganizationFormFilterColumnSourceDataAsync(columnFilter);

            var options = _mapper.Map<IReadOnlyCollection<FilterOptionModel>>(optionsDto);

            return Ok(options);
        }

        [Route("forms/settings")]
        [HttpPut]
        public async Task<IActionResult> SetFormSetting([FromBody] FormSettingModel formSetting)
        {
            var formSettingDto = _mapper.Map<FormSettingDto>(formSetting);

            bool result = await _organizationService.SetFormSettingAsync(formSettingDto);

            return Ok(result);
        }

        void ValidateEmails(IEnumerable<string> emails)
        {
            if (emails != null)
            {
                var emailValidator = new EmailAddressAttribute();
                foreach (string email in emails)
                {
                    if (!emailValidator.IsValid(email))
                        throw new Exception("Email address is not valid");
                }
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly IMapper _mapper;
        private readonly IPortalReportService _portalService;
        public OrganizationsController(IOrganizationService organizationService, IPortalReportService portalService, IMapper mapper)
        {
            _organizationService = organizationService;
            _mapper = mapper;
            _portalService = portalService;
        }

        [Route("options")]
        [HttpGet]
        public async Task<IActionResult> GetOrganizationOptions()
        {
            IReadOnlyCollection<OptionDto> organizationsDto = await _organizationService.GetOrganizationOptionsAsync();

            var organizationOptions = _mapper.Map<IReadOnlyCollection<OptionModel>>(organizationsDto);

            return Ok(organizationOptions);
        }
        [Route("portalFeatures")]
        [HttpGet]
        public async Task<IActionResult> GetPortalFeaturesAsync()
        {
            var dto = await _portalService.GetPortalFeatures();
            var model = _mapper.Map<IEnumerable<OptionModel>>(dto);
            return Ok(model);
        }
    }
}
