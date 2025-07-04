using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SHARP.BusinessLogic.Extensions;

namespace SHARP.Middlewares
{
    public sealed class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException)
            {
                await WriteExceptionToResponseAsync(context, (int)HttpStatusCode.NotFound, null);
            }
            catch (Exception ex)
            {
                string message = string.IsNullOrEmpty(ex?.Message) ? "An unhandled error has occured:" : ex.Message;

                if(ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    message = $"{message}. InnerException: {ex.InnerException.Message}";
                }

                _logger.LogError(ex, message);

                await WriteExceptionToResponseAsync(
                    context,
                    (int)HttpStatusCode.InternalServerError,
                    new
                    {
                        errorCode = "InternalServerError",
                        errorMessage = ex.Message
                    });
            }
        }

        private async Task WriteExceptionToResponseAsync(HttpContext context, int statusCode, object response)
        {
            context.Response.StatusCode = statusCode;

            if (response != null)
            {
                string json = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                context.Response.ContentType = new MediaTypeHeaderValue("application/json").ToString();

                await context.Response.WriteAsync(json, Encoding.UTF8);
            }
        }
    }
}
