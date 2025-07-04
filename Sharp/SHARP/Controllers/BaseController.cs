using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SHARP.BusinessLogic.Interfaces;
using SHARP.DAL;
using SHARP.DAL.Models;
using SHARP.Common.Enums;

namespace SHARP.Controllers
{
    public class BaseController: ControllerBase
    {
        protected UserManager<ApplicationUser> _userManager = null;
        protected IUserInfoService _userInfoService = null;
        public BaseController(UserManager<ApplicationUser> userManager, IUserInfoService userService)
        {
            this._userManager = userManager;
            this._userInfoService = userService;
        }
        protected async Task<string> GetCurrentAspnetUserID()
        {
            var user = await this._userManager.FindByNameAsync(this.User.Identity.Name);
            return user.Id;
        }
        protected async Task<ApplicationUser> GetCurrentAspnetUser()
        {
            var user = await this._userManager.FindByNameAsync(this.User.Identity.Name);
            return user;
        }

        protected async Task<int> GetCurrentUserID()
        {
            var aspnetUser = await GetCurrentAspnetUser();
            var user = this._userInfoService.GetUserByAspnetUserID(aspnetUser.Id);
            return user.Id;
        }
        protected async Task<User> GetCurrentUser()
        {
            var aspnetUser = await GetCurrentAspnetUser();
            var user = this._userInfoService.GetUserByAspnetUserID(aspnetUser.Id);
            return user;
        }

        protected async Task<bool> IsUserInRole(UserRoles role)
        {
            var user = await GetCurrentAspnetUser();
            return await this._userManager.IsInRoleAsync(user, role.ToString());
        }
    }
}
