using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CPReservationApi.WebApi.Services
{
    public class BaseController : Controller
    {
        private AppSettings _appSettings;
        public BaseController(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
        }
        // GET: /<controller>/ check paramters value for authentication is reaquired or not from appsettings
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            bool authorized = false;
            string authorizationToken = filterContext.HttpContext.Request.Headers["AuthenticateKey"];

            if (!string.IsNullOrEmpty(_appSettings.certificatePassword))
            {
                filterContext.HttpContext.Request.Headers["CertificatePassword"] = _appSettings.certificatePassword;
            }

            if (_appSettings.AuthenticationRequired)
            {
                if (!String.IsNullOrEmpty(authorizationToken))
                {
                    if (authorizationToken == null)
                    {
                        authorized = false;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(_appSettings.AuthToken_Backoffice) && string.IsNullOrEmpty(_appSettings.AuthToken_BoxOffice) && string.IsNullOrEmpty(_appSettings.AuthToken_BoxOfficeV2) && string.IsNullOrEmpty(_appSettings.AuthToken_TablePro) && string.IsNullOrEmpty(_appSettings.AuthToken_CheckIn) && string.IsNullOrEmpty(_appSettings.AuthToken_CellarpassAccount))
                        {
                            authorized = false;
                        }
                        //Need to look this up from DB and cache in the future.
                        string validToken = _appSettings.AuthToken_Backoffice;

                        if ((validToken != null))
                        {
                            if (authorizationToken.Trim().Replace("=", "") == validToken)
                            {
                                filterContext.HttpContext.Request.Headers["AuthenticateKey"] = Common.Common.GetEnumDescription(Common.Common.AurthorizedUserType.BackOffice); //"Backoffice User";
                                filterContext.HttpContext.Request.Headers.Add("IsAdmin", "True");
                                authorized = true;
                            }
                        }

                        //Additional authorizations, only checked if authorization is still false

                        if (authorized == false)
                        {
                            //2nd Check for other authorized API users
                            string thirdPartyToken = _appSettings.AuthToken_BoxOffice;
                            if ((thirdPartyToken != null))
                            {
                                if (authorizationToken.Trim().Replace("=", "") == thirdPartyToken)
                                {
                                    filterContext.HttpContext.Request.Headers["AuthenticateKey"] = Common.Common.GetEnumDescription(Common.Common.AurthorizedUserType.BoxOffice);//"BoxOffice User";
                                    filterContext.HttpContext.Request.Headers.Add("IsAdmin", "False");
                                    authorized = true;
                                }
                            }
                        }

                        if (authorized == false)
                        {
                            //2nd Check for other authorized API users
                            string thirdPartyToken = _appSettings.AuthToken_BoxOfficeV2;
                            if ((thirdPartyToken != null))
                            {
                                if (authorizationToken.Trim().Replace("=", "") == thirdPartyToken)
                                {
                                    filterContext.HttpContext.Request.Headers["AuthenticateKey"] = Common.Common.GetEnumDescription(Common.Common.AurthorizedUserType.BoxOfficeV2);//"BoxOffice User";
                                    filterContext.HttpContext.Request.Headers.Add("IsAdmin", "True");
                                    authorized = true;
                                }
                            }
                        }

                        if (authorized == false)
                        {
                            //Affiliates
                            //3rd Check for other authorized API users
                            string affiliateToken = _appSettings.AuthToken_TablePro;
                            if ((affiliateToken != null))
                            {
                                if (authorizationToken.Trim().Replace("=", "") == affiliateToken)
                                {
                                    filterContext.HttpContext.Request.Headers["AuthenticateKey"] = Common.Common.GetEnumDescription(Common.Common.AurthorizedUserType.TablePro);//"TablePro User";
                                    filterContext.HttpContext.Request.Headers.Add("IsAdmin", "True");
                                    authorized = true;
                                }
                            }
                        }
                        if (authorized == false)
                        {
                            //Affiliates
                            //4th Check for other authorized API users
                            string affiliateToken = _appSettings.AuthToken_CheckIn;
                            if ((affiliateToken != null))
                            {
                                if (authorizationToken.Trim().Replace("=", "") == affiliateToken)
                                {
                                    filterContext.HttpContext.Request.Headers["AuthenticateKey"] = Common.Common.GetEnumDescription(Common.Common.AurthorizedUserType.CheckIn);//"CheckIn User";
                                    filterContext.HttpContext.Request.Headers.Add("IsAdmin", "False");
                                    authorized = true;
                                }
                            }
                        }

                        if (authorized == false)
                        {
                            //Affiliates
                            //5th Check for other authorized API users
                            string affiliateToken = _appSettings.AuthToken_CellarpassAccount;
                            if ((affiliateToken != null))
                            {
                                if (authorizationToken.Trim().Replace("=", "") == affiliateToken)
                                {
                                    filterContext.HttpContext.Request.Headers["AuthenticateKey"] = Common.Common.GetEnumDescription(Common.Common.AurthorizedUserType.CellarpassAccount);//"Cellarpass Account";
                                    filterContext.HttpContext.Request.Headers.Add("IsAdmin", "False");
                                    authorized = true;
                                }
                            }
                        }

                    }
                    if (authorized)
                        base.OnActionExecuting(filterContext);
                    else
                        filterContext.Result = new JsonResult(new { HttpStatusCode.Unauthorized });
                }
                else
                {
                    filterContext.Result = new JsonResult(new { HttpStatusCode.Unauthorized });
                }
            }
            else
            {
                filterContext.HttpContext.Request.Headers["AuthenticateKey"] = Common.Common.GetEnumDescription(Common.Common.AurthorizedUserType.CoreApi);//"CoreApi User";
                filterContext.HttpContext.Request.Headers.Add("IsAdmin", "False");
            }
        }
    }
}
