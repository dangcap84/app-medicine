using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MediTrack.Api.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log the request
            var request = await FormatRequest(context.Request);
            _logger.LogInformation("HTTP Request Information: {@Request}", request);

            // Temporarily replace the response body with a memory stream
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Continue down the middleware pipeline
            await _next(context);

            // Format and log the response
            var response = await FormatResponse(context.Response);
            _logger.LogInformation("HTTP Response Information: {@Response}", response);

            // Copy the response body to the original stream and restore it
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private async Task<object> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();

            var body = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Position = 0;

            return new
            {
                Time = DateTime.UtcNow,
                RemoteIP = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                Scheme = request.Scheme,
                Host = request.Host.ToString(),
                Path = request.Path.ToString(),
                QueryString = request.QueryString.ToString(),
                Method = request.Method,
                ContentType = request.ContentType,
                UserAgent = request.Headers["User-Agent"].ToString(),
                Body = body
            };
        }

        private async Task<object> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return new
            {
                Time = DateTime.UtcNow,
                StatusCode = response.StatusCode,
                ContentType = response.ContentType,
                Body = text
            };
        }
    }
}
