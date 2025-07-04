using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using SHARP.Common.Constants;
using System.Net;
using System.Security.Claims;

namespace SHARP.Http
{
    public static class HttpRequestExtensions
    {
        public static string GetUserAgent(this HttpRequest request) => request.Headers[HeaderNames.UserAgent].ToString();

        public static IPAddress GetIP(this HttpRequest request) => request.HttpContext.Connection.RemoteIpAddress;

        public static string GetUserTimeZone(this HttpContext context) => context.User.FindFirstValue(CommonConstants.USER_TIME_ZONE);
    }
}
