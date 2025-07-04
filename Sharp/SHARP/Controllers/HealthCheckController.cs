using Microsoft.AspNetCore.Mvc;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly IFormService _formService;

        public HealthCheckController(IAuditService auditService, IFormService formService)
        {
            _auditService = auditService;
            _formService = formService;
        }

        [HttpGet]
        public IActionResult TestGet()
        {
            bool result = _auditService.TestGet();

            return Ok(result);
        }

        [Route("audit/types")]
        [HttpGet]
        public async Task<IActionResult> GetAuditTypes()
        {
            IReadOnlyCollection<OptionDto> types = await _formService.GetTypeOptionsAsync();

            return Ok(types);
        }
    }
}
