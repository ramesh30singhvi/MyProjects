using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPReservationApi.WebApi.ViewModels;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.Services;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Cryptography;
using static CPReservationApi.Common.Common;
using CPReservationApi.Common;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/emaillist")]
    public class EmailListController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        public EmailListController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        [Route("addtolist")]
        [HttpPost]
        public async Task<IActionResult> AddToList([FromBody] AddToListRequest reqmodel)
        {
            var resp = new BaseResponse2();

            try
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)SettingGroup.mailchimp_cp);
                string mcAPIKey = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_key);
                string mcStore = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_adminList);

                //call routine and pass data
                MailChimpAPI mailChimpAPI = new MailChimpAPI(mcAPIKey, mcStore, "", "", "", "", "");
                await mailChimpAPI.CheckAndCreateList(reqmodel.email);
                resp.success = true;
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "AddToList:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(resp);
        }

        [Route("addtags")]
        [HttpPost]
        public async Task<IActionResult> AddTags([FromBody] AddTagsRequest reqmodel)
        {
            var resp = new BaseResponse2();

            try
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)SettingGroup.mailchimp_cp);
                string mcAPIKey = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_key);
                string mcStore = Settings.GetStrValue(settingsGroup, SettingKey.mailchimp_cp_adminList);

                //call routine and pass data
                MailChimpAPI mailChimpAPI = new MailChimpAPI(mcAPIKey, mcStore, "", "", "", "", "");
                await mailChimpAPI.CheckAndCreateTag(reqmodel.tag, reqmodel.email);
                resp.success = true;
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "AddTags:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(resp);
        }
    }
}