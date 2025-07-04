using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHARP.BusinessLogic.Interfaces;
using SHARP.ViewModels.Role;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public RolesController(IMapper mapper, IRoleService roleService)
        {
            _mapper = mapper;
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var dto = await _roleService.GetAsync();
            var model = _mapper.Map<IEnumerable<RoleModel>>(dto);
            return Ok(model);
        }
    }
}
