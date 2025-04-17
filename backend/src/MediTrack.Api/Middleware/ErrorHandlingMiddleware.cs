using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MediTrack.Domain.Exceptions;
using MediTrack.Domain.Exceptions.Authentication;
using MediTrack.Domain.Exceptions.Domain;
using MediTrack.Domain.Exceptions.Validation;

namespace MediTrack.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred processing the request");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode;
            object errorResponse;

            if (exception is InvalidCredentialsException e)
            {
                statusCode = StatusCodes.Status401Unauthorized;
                errorResponse = new { code = e.Code, message = e.Message };
            }
            else if (exception is DuplicateUsernameException e2)
            {
                statusCode = StatusCodes.Status400BadRequest;
                errorResponse = new { code = e2.Code, message = e2.Message, username = e2.Username };
            }
            else if (exception is ResourceNotFoundException e3)
            {
                statusCode = StatusCodes.Status404NotFound;
                errorResponse = new { code = e3.Code, message = e3.Message };
            }
            else if (exception is UnauthorizedResourceAccessException e4)
            {
                statusCode = StatusCodes.Status403Forbidden;
                errorResponse = new { code = e4.Code, message = e4.Message };
            }
            else if (exception is ValidationException e5)
            {
                statusCode = StatusCodes.Status400BadRequest;
                errorResponse = new { code = e5.Code, message = e5.Message, failures = e5.Failures };
            }
            else
            {
                statusCode = StatusCodes.Status500InternalServerError;
                errorResponse = new { code = "SYS001", message = "An unexpected error occurred." };
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(result);
        }
    }
}
