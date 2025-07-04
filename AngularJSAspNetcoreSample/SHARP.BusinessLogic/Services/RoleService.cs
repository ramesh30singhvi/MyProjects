using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SHARP.BusinessLogic.DTO.Role;
using SHARP.BusinessLogic.Interfaces;
using SHARP.DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Services
{
    public class RoleService : IRoleService
    {
        private readonly IMapper _mapper;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RoleService(IMapper mapper, RoleManager<ApplicationRole> roleManager)
        {
            _mapper = mapper;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<RoleDto>> GetAsync()
        {
            var roles = await _roleManager.Roles.ToArrayAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles.Where(role => role.Name != "AutoTest"));
        }
    }
}
