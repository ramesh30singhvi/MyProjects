using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPReservationApi.WebApi.ViewModels;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.Services;
using Microsoft.Extensions.Options;
using System.Text;
using CPReservationApi.Common;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace CPReservationApi.WebApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/authenticate")]
    public class AuthenticateController : BaseController
    {

        public static IOptions<AppSettings> _appSetting;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appSetting"></param>
        public AuthenticateController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        [Route("authenticateuser")]
        [HttpGet]
        public IActionResult AuthenticateUser(int member_id, string username, string password, AppType app_type)
        {
            var clientResponse = new ClientResponse();
            string extra_info = "API Username and/or API Password does not match on record.";
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var clientModel = new ClientModel();
                if (member_id > 0)
                {
                    if (app_type == AppType.TABLEMANAGEMENTAPP)
                    {
                        var winery = new WineryModel();
                        winery = eventDAL.GetWineryById(member_id);

                        if (winery.SubscriptionPlan == 4)
                            clientModel = userDAL.ValidateLogin(member_id, username, password, app_type);
                        else
                            extra_info = "You are not allowed to use this app";
                    }
                    else
                    {
                        clientModel = userDAL.ValidateLogin(member_id, username, password, app_type);
                    }

                    var paymentConfig = eventDAL.GetPaymentConfigByWineryId(member_id);

                    if (paymentConfig != null)
                    {
                        clientModel.payment_gateway_id = (int)paymentConfig.PaymentGateway;

                        if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.AuthorizeNet)
                            clientModel.payment_gateway = "Authorize.Net";
                        else if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.Braintree)
                            clientModel.payment_gateway = "BrainTree";
                        else if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.CardConnect)
                            clientModel.payment_gateway = "CardConnect";
                        else if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.CenPos)
                            clientModel.payment_gateway = "CenPos";
                        else if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.Commrece7Payments)
                            clientModel.payment_gateway = "Commerce7 Payments";
                        else if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.Cybersource)
                            clientModel.payment_gateway = "Cybersource";
                        else if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.OpenEdge)
                            clientModel.payment_gateway = "OpenEdge";
                        else if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.PayFlowPro)
                            clientModel.payment_gateway = "PayFlowPro";
                        else if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.Shift4)
                            clientModel.payment_gateway = "Shift4";
                        else if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.Stripe)
                            clientModel.payment_gateway = "Stripe";
                        else if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.USAePay)
                            clientModel.payment_gateway = "USAePay";
                        else if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.WorldPayXML)
                            clientModel.payment_gateway = "WorldPay Express";
                        else if (paymentConfig.PaymentGateway == Common.Payments.Configuration.Gateway.Zeamster)
                            clientModel.payment_gateway = "Zeamster";
                    }

                    clientModel.detailed_address_info_required = eventDAL.IsDetailedAddressInfoRequired(member_id);
                }

                if (clientModel.client_id > 0)
                {
                    clientResponse.data = clientModel;
                    clientResponse.success = true;
                }
                else
                {
                    if ((clientModel.client_name + "").Length > 0)
                        extra_info = clientModel.client_name;

                    if (app_type == AppType.BOXOFFICEAPP)
                    {
                        var winery = new WineryModel();
                        winery = eventDAL.GetWineryById(member_id);
                        if (winery.AttendeeAppUsername == username.Trim())
                            extra_info = "API Username and/or API Password does not match on record";
                    }

                    clientResponse.success = false;
                    clientResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    clientResponse.error_info.extra_info = extra_info;
                }
            }
            catch (Exception ex)
            {
                clientResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                clientResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                clientResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetEventAddOns:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(clientResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        private string GenerateJwtToken(int UserId,string UserName)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sid,UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSetting.Value.JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_appSetting.Value.JwtExpireMinutes));

            var token = new JwtSecurityToken(
                _appSetting.Value.JwtIssuer,
                _appSetting.Value.JwtIssuer,
                claims,
                expires: expires,
                notBefore: DateTime.UtcNow,
                signingCredentials: creds
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;

        }

        [Route("login")]
        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            var clientResponse = new LoginResponse();

            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

            var user = new UserSessionModel();
            string errorMessage = string.Empty;

            user = userDAL.GetUser(model.username, model.password, out errorMessage);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                clientResponse.success = false;
                clientResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                clientResponse.error_info.extra_info = "Login failed. ";
                clientResponse.error_info.description = errorMessage;
                return new ObjectResult(clientResponse);
            }

            user.token_expiry = Convert.ToDouble(_appSetting.Value.JwtExpireMinutes);
            user.token = GenerateJwtToken(user.user_id,user.user_name);

            clientResponse.success = true;
            clientResponse.data = user;

            return new ObjectResult(clientResponse);
        }

        [Route("forgotpassword")]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            var forgotPasswordResponse = new ForgotPasswordResponse();
            try
            {
                if (model.user_name.Trim().Length > 0)
                {
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                    var userDetailModel = new UserDetailModel();

                    userDetailModel = userDAL.GetUserDetailsbyemail(model.user_name);

                    if (userDetailModel != null && userDetailModel.user_id > 0)
                    {
                        string PasswordChangeKey = Services.Utility.GenerateSimpleRandomNumber().ToString();
                        bool ret = userDAL.UpdatePasswordChangeKey(userDetailModel.user_id, PasswordChangeKey);

                        if (ret && PasswordChangeKey.Length > 0)
                        {
                            MailConfig config = new MailConfig
                            {
                                Domain = _appSetting.Value.MailGunPostUrl,
                                ApiKey = _appSetting.Value.MainGunApiKey
                            };

                            AuthMessageSender messageService = new AuthMessageSender();
                            await messageService.SendPasswordResetCodeEmail(PasswordChangeKey, userDetailModel, Common.Common.UserType.Guest, config);
                            forgotPasswordResponse.success = true;
                            forgotPasswordResponse.data.user_name = model.user_name;
                        }
                    }
                    else
                    {
                        forgotPasswordResponse.error_info.error_type = (int)Common.Common.ErrorType.Guest;
                        forgotPasswordResponse.error_info.extra_info = "Sorry, there is no account with that email address.";
                        forgotPasswordResponse.error_info.description = "";
                    }
                }
                else
                {
                    forgotPasswordResponse.error_info.error_type = (int)Common.Common.ErrorType.Guest;
                    forgotPasswordResponse.error_info.extra_info = "Please fill in your email address.";
                    forgotPasswordResponse.error_info.description = "";
                }
            }
            catch (Exception ex)
            {
                forgotPasswordResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                forgotPasswordResponse.error_info.extra_info = Common.Common.InternalServerError;
                forgotPasswordResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ForgotPassword:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(forgotPasswordResponse);
        }

        [Route("resetpassword")]
        [HttpPost]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest model)
        {
            var forgotPasswordResponse = new ForgotPasswordResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                UserDetailModel userModel = new UserDetailModel();

                userModel = userDAL.GetUserIdBybyPasswordKey(model.reset_code);

                if (userModel.user_id > 0)
                {
                    bool isUserLockedOut = userDAL.IsUserLockedOut(userModel.user_id);

                    if (isUserLockedOut)
                    {
                        forgotPasswordResponse.error_info.error_type = (int)Common.Common.ErrorType.Guest;
                        forgotPasswordResponse.error_info.extra_info = "We're sorry, but your account has been temporarily locked out because of too many invalid login attempts. As a security precaution, you must wait 30 minutes before attempting to sign in again or requesting to reset your account.";
                    }
                    else {
                        bool ret = userDAL.ResetPassword(userModel.user_id, StringHelpers.EncryptOneWay(model.new_password));

                        if (!ret)
                        {
                            forgotPasswordResponse.error_info.error_type = (int)Common.Common.ErrorType.Guest;
                            forgotPasswordResponse.error_info.extra_info = "Sorry, your password can not be reset. Please contact support if you feel this is an error.";
                        }
                        else
                        {
                            forgotPasswordResponse.success = true;
                            forgotPasswordResponse.data.user_name = userModel.email;
                        }
                    }

                }
                else
                {
                    forgotPasswordResponse.error_info.error_type = (int)Common.Common.ErrorType.Guest;
                    forgotPasswordResponse.error_info.extra_info = "We're sorry, but your account has been temporarily locked out because of too many invalid login attempts. As a security precaution, you must wait 30 minutes before attempting to sign in again or requesting to reset your account.";
                }
            }
            catch (Exception ex)
            {
                forgotPasswordResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                forgotPasswordResponse.error_info.extra_info = Common.Common.InternalServerError;
                forgotPasswordResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ResetPassword:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(forgotPasswordResponse);
        }
    }
}
