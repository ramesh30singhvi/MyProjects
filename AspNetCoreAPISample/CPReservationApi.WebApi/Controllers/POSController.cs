using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPReservationApi.WebApi.Services;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CPReservationApi.Model;
using CPReservationApi.DAL;
using CPReservationApi.Common;
using System.Net.Http.Headers;
using System.IO;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/pos")]
    public class POSController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        public POSController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        [Route("updatereceiptsetting")]
        [HttpPost]
        public IActionResult UpdateReceiptSetting([FromBody]WineryReceiptSetting model)
        {
            var settingResponse = new UpdateReceiptSettingResponse();
            if (model == null)
            {
                settingResponse.success = false;
                settingResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                settingResponse.error_info.extra_info = "No data passed";
                settingResponse.error_info.description = "No data passed in request";
                return new ObjectResult(settingResponse);
            }
            if (model.member_id <= 0)
            {
                settingResponse.success = false;
                settingResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                settingResponse.error_info.extra_info = "Invalid Member Id";
                settingResponse.error_info.description = "Invalid Member Id";
                return new ObjectResult(settingResponse);
            }
            POSDAL posDAL = new POSDAL(Common.Common.ConnectionString);

            try
            {
                bool isSucess =  posDAL.UpdateReceiptSettings(model);

                if (isSucess)
                {
                    settingResponse.success = true;
                    settingResponse.data.member_id = model.member_id;
                    //APN Notification
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var model11 = new CreateDeltaRequest();
                    model11.item_id = model.member_id;
                    model11.item_type = (int)ItemType.ReceiptSetting;
                    model11.location_id = 0;  //TODO
                    model11.member_id = model.member_id;
                    model11.action_date = DateTime.UtcNow;
                    model11.floor_plan_id = 0;
                    notificationDAL.SaveDelta(model11);
                }
                else
                {
                    settingResponse.success = false;
                    settingResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    settingResponse.error_info.extra_info = "Error updating Receipt Settings";
                }
            }
            catch (Exception ex)
            {
                settingResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                settingResponse.error_info.extra_info = Common.Common.InternalServerError;
                settingResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateReceiptSetting:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(settingResponse);
        }

        [Route("getreceiptsetting")]
        [HttpGet]
        public IActionResult GetReceiptSetting(int member_id)
        {
            var response = new GetReceiptSettingResponse();
            if (member_id <= 0)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Invalid Member Id";
                response.error_info.description = "Invalid Member Id";
                return new ObjectResult(response);
            }
            POSDAL posDAL = new POSDAL(Common.Common.ConnectionString);

            try
            {
                var model = new WineryReceiptSettingsWithLogo();

                model = posDAL.GetReceiptSettingByMember(member_id);

                if (model != null)
                {
                    response.success = true;
                    if (!string.IsNullOrWhiteSpace(model.logo_url))
                    {
                        model.logo_url= string.Format("{0}/{1}", StringHelpers.GetImagePath(ImageType.ReceiptLogo, ImagePathType.azure), model.logo_url);
                        //model.logo_url = Path.Combine(Utility.GetImagePath(ImageType.ReceiptLogo, ImagePathType.azure), model.logo_url);
                    }
                    response.data = model;
                }
                else
                {
                    response.success = true;
                    response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    response.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetReceiptSetting" + ":  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(response);
        }

        [Route("updatereceiptlogo")]
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult UpdateReceiptLogo([FromForm]int member_id)
        {
            var settingResponse = new UploadReceiptLogoResponse();
            if (member_id <= 0)
            {
                settingResponse.success = false;
                settingResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                settingResponse.error_info.extra_info = "Invalid Member Id";
                settingResponse.error_info.description = "Invalid Member Id";
                return new ObjectResult(settingResponse);
            }
            var file = Request.Form.Files[0];
            if (file == null || file.Length == 0)
            {
                settingResponse.success = false;
                settingResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                settingResponse.error_info.extra_info = "No logo file found";
                settingResponse.error_info.description = "No logo file found";
                return new ObjectResult(settingResponse);
            }
            string fileName = string.Format("receipt_logo_{0}.jpg", member_id);
            string logoURL = "";
            using (var memoryStream = new MemoryStream())
            {
                file.OpenReadStream().CopyTo(memoryStream);
                byte[] bytes = memoryStream.ToArray();
                logoURL = Utility.UploadFileToStorage(bytes, fileName, ImageType.ReceiptLogo);
            }

            POSDAL posDAL = new POSDAL(Common.Common.ConnectionString);

            try
            {
                bool isSucess = posDAL.UpdateReceiptLogoUrl(member_id, fileName);

                if (isSucess)
                {
                    settingResponse.success = true;
                    settingResponse.data.logo_url = logoURL;
                    //APN Notification
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var model11 = new CreateDeltaRequest();
                    model11.item_id = member_id;
                    model11.item_type = (int)ItemType.ReceiptSetting;
                    model11.location_id = 0;  //TODO
                    model11.member_id = member_id;
                    model11.action_date = DateTime.UtcNow;
                    model11.floor_plan_id = 0;
                    notificationDAL.SaveDelta(model11);
                }
                else
                {
                    settingResponse.success = false;
                    settingResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    settingResponse.error_info.extra_info = "Error uploading Receipt logo";
                }
            }
            catch (Exception ex)
            {
                settingResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                settingResponse.error_info.extra_info = Common.Common.InternalServerError;
                settingResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateReceiptLogo:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(settingResponse);
        }

        [Route("addordersignature")]
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult AddOrderSignature([FromForm]int order_id, [FromForm]int signature_type, [FromForm]int payment_id)
        {
            var orderSignatureResponse = new OrderSignatureResponse();
            if (order_id <= 0)
            {
                orderSignatureResponse.success = false;
                orderSignatureResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                orderSignatureResponse.error_info.extra_info = "Invalid Order Id";
                orderSignatureResponse.error_info.description = "Invalid Order Id";
                return new ObjectResult(orderSignatureResponse);
            }
            var file = Request.Form.Files[0];
            if (file == null || file.Length == 0)
            {
                orderSignatureResponse.success = false;
                orderSignatureResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                orderSignatureResponse.error_info.extra_info = "No Signature file found";
                orderSignatureResponse.error_info.description = "No Signature file found";
                return new ObjectResult(orderSignatureResponse);
            }

            Guid signatureGUID = Guid.NewGuid();

            string fileName = string.Format("{0}.jpg", signatureGUID.ToString().Replace("-", ""));
            string logoURL = "";
            using (var memoryStream = new MemoryStream())
            {
                file.OpenReadStream().CopyTo(memoryStream);
                byte[] bytes = memoryStream.ToArray();
                logoURL = Utility.UploadFileToStorage(bytes, fileName, ImageType.OrderSignature);
            }

            POSDAL pOSDAL = new POSDAL(Common.Common.ConnectionString);

            try
            {
                int Id = pOSDAL.AddOrderSignature(order_id, payment_id,fileName,signature_type, signatureGUID);

                if (Id > 0)
                {
                    orderSignatureResponse.success = true;

                    OrderSignatureResponseModel model = new OrderSignatureResponseModel();

                    model.signature_guid = signatureGUID;

                    orderSignatureResponse.data = model;
                }
                else
                {
                    orderSignatureResponse.success = false;
                    orderSignatureResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    orderSignatureResponse.error_info.extra_info = "Error uploading Signature";
                }
            }
            catch (Exception ex)
            {
                orderSignatureResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                orderSignatureResponse.error_info.extra_info = Common.Common.InternalServerError;
                orderSignatureResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "AddOrderSignature:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(orderSignatureResponse);
        }

    }
}