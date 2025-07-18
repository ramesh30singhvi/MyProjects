﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System;
using SHARP.ApplicationInsightsRequestLogging.BodyReader;
using SHARP.ApplicationInsightsRequestLogging.Options;
using SHARP.ApplicationInsightsRequestLogging.TelemetryWriter;

namespace SHARP.Middlewares
{
    public class BodyLoggerMiddleware : IMiddleware
    {
        private readonly IOptions<BodyLoggerOptions> _options;
        private readonly IBodyReader _bodyReader;
        private readonly ITelemetryWriter _telemetryWriter;

        public BodyLoggerMiddleware(IOptions<BodyLoggerOptions> options, IBodyReader bodyReader, ITelemetryWriter telemetryWriter)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _bodyReader = bodyReader ?? throw new ArgumentNullException(nameof(bodyReader));
            _telemetryWriter = telemetryWriter ?? throw new ArgumentNullException(nameof(telemetryWriter));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestBody = string.Empty;
            if (_options.Value.HttpVerbs.Contains(context.Request.Method))
            {
                // Temporarily store request body
                requestBody = await _bodyReader.ReadRequestBodyAsync(context, _options.Value.MaxBytes, _options.Value.Appendix);

                _bodyReader.PrepareResponseBodyReading(context);
            }

            // hand over to the next middleware and wait for the call to return
            await next(context);

            if (!_options.Value.HttpVerbs.Contains(context.Request.Method))
            {
                return;
            }

            if (_options.Value.HttpCodes.Contains(context.Response.StatusCode))
            {
                if (_options.Value.LogRequestBodyProperty)
                {
                    _telemetryWriter.Write(context, _options.Value.RequestBodyPropertyKey, requestBody);
                }

                if (_options.Value.LogResponseBodyProperty)
                {
                    var responseBody = await _bodyReader.ReadResponseBodyAsync(context, _options.Value.MaxBytes, _options.Value.Appendix);
                    _telemetryWriter.Write(context, _options.Value.ResponseBodyPropertyKey, responseBody);
                }
            }

            // Copy back so response body is available for the user agent
            // prevent 500 error when Not StatusCode of Interest
            await this._bodyReader.RestoreOriginalResponseBodyStreamAsync(context);
        }
    }
}
