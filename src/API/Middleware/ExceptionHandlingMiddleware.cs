using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Domain.Exceptions;

namespace API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var message = "An internal server error occurred.";
            object? errors = null;

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    code = HttpStatusCode.NotFound;
                    message = notFoundEx.Message;
                    break;
                case UnauthorizedAccessException unauthorizedEx:
                    code = HttpStatusCode.Unauthorized;
                    message = unauthorizedEx.Message;
                    break;
                case InvalidOperationException invalidOpEx:
                    code = HttpStatusCode.BadRequest;
                    message = invalidOpEx.Message;
                    break;
                case ArgumentException argEx:
                    code = HttpStatusCode.BadRequest;
                    message = argEx.Message;
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = message,
                errors = errors
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}
