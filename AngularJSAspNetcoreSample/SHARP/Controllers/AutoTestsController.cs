using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHARP.BusinessLogic.DTO.Memo;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Filtration;
using SHARP.Filters;
using SHARP.ViewModels.Memo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "AutoTest")]
    public class AutoTestsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuditService _auditService;
        private readonly IFormService _formService;
        private readonly IOrganizationService _organizationService;
        private readonly IFacilityService _facilityService;

        public AutoTestsController(
            IUserService userService, 
            IAuditService auditService,
            IFormService formService,
            IOrganizationService organizationService,
            IFacilityService facilityService)
        {
            _userService = userService;
            _auditService = auditService;
            _formService = formService;
            _organizationService = organizationService;
            _facilityService = facilityService;
        }

#if DEBUG || TEST || STAGING

        [HttpDelete]
        [Route("user/{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            bool result = await _userService.DeleteUserAsync(id);

            return Ok(result);
        }

        [HttpDelete]
        [Route("audit/{id:int}")]
        public async Task<IActionResult> DeleteAudit(int id)
        {
            bool result = await _auditService.DeleteAuditAsync(id);

            return Ok(result);
        }

        [HttpDelete]
        [Route("form/{id:int}")]
        public async Task<IActionResult> DeleteForm(int id)
        {
            bool result = await _formService.DeleteFormAsync(id);

            return Ok(result);
        }

        [HttpDelete]
        [Route("organization/{id:int}")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            bool result = await _organizationService.DeleteOrganizationAsync(id);

            return Ok(result);
        }

        [HttpDelete]
        [Route("facility/{id:int}")]
        public async Task<IActionResult> DeleteFacility(int id)
        {
            bool result = await _facilityService.DeleteFacilityAsync(id);

            return Ok(result);
        }

#endif
    }
}
