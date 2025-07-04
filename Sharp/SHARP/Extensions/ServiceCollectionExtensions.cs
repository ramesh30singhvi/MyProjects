using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SHARP.ApplicationInsightsRequestLogging.BodyReader;
using SHARP.ApplicationInsightsRequestLogging.Options;
using SHARP.ApplicationInsightsRequestLogging.TelemetryWriter;
using SHARP.BusinessLogic.Configuration.AutoMapper;
using SHARP.Common.Constants;
using SHARP.Configuration.AutoMapper;
using SHARP.Middlewares;
using System;
using System.Linq;

namespace SHARP.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(SharpBLLProfile).Assembly);
            services.AddAutoMapper(typeof(SharpApiProfile).Assembly);
        }

        public static IServiceCollection AddAppInsightsHttpBodyLogging(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            AddBodyLogger(services);

            return services;
        }

        public static IServiceCollection AddAppInsightsHttpBodyLogging(this IServiceCollection services, Action<BodyLoggerOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            AddBodyLogger(services, setupAction);

            return services;
        }

        public static void Configure(this BodyLoggerOptions option, IConfiguration configuration)
        {
            var httpVerbs = configuration.GetValue<string>("HttpVerbs");

            if (!string.IsNullOrEmpty(httpVerbs))
            {
                if (httpVerbs.ToLower() == "off")
                {
                    option.HttpVerbs?.Clear();
                }
                else
                {
                    var methods = httpVerbs.Split(CommonConstants.SLASH);

                    if (methods.Any())
                    {
                        option.HttpVerbs = methods.ToList();
                    }
                }
            }

            var httpCodes = configuration.GetValue<string>("HttpCodes");

            if (!string.IsNullOrEmpty(httpCodes))
            {
                if (httpCodes.ToLower() == "+200")
                {
                    option.HttpCodes.AddRange(StatusCodeRanges.Status2xx);
                }
                else
                {
                    var codes = httpCodes.Split(CommonConstants.SLASH).Select(code => int.Parse(code));

                    if (codes.Any())
                    {
                        option.HttpCodes = codes.ToList();
                    }
                }
            }

            var logRequestBody = configuration.GetValue<bool?>("LogRequestBody");

            if (logRequestBody.HasValue)
            {
                option.LogRequestBodyProperty = logRequestBody.Value;
            }

            var logResponseBody = configuration.GetValue<bool?>("LogResponseBody");

            if (logResponseBody.HasValue)
            {
                option.LogResponseBodyProperty = logResponseBody.Value;
            }
        }

        internal static void AddBodyLogger(IServiceCollection services)
        {
            services.AddScoped<BodyLoggerMiddleware>();
            services.AddScoped<IBodyReader, BodyReader>();
            services.AddScoped<ITelemetryWriter, TelemetryWriter>();
        }

        internal static void AddBodyLogger(IServiceCollection services, Action<BodyLoggerOptions> setupAction)
        {
            AddBodyLogger(services);
            services.Configure(setupAction);
        }
    }
}
