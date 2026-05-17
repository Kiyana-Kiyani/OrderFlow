
using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate requestDelegate, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = requestDelegate;
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
                var traceId = context.TraceIdentifier;
                _logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}, Path: {Path}",
                        traceId, context.Request.Path);

                var statusCode = ex switch
                {
                    ValidationException or
                    DomainException or
                    ArgumentException or
                    InvalidOperationException => HttpStatusCode.BadRequest,

                    NotFoundException or
                    KeyNotFoundException => HttpStatusCode.NotFound,

                    ConflictException => HttpStatusCode.Conflict,

                    ForbiddenException or
                    UnauthorizedAccessException => HttpStatusCode.Forbidden,

                    _ => HttpStatusCode.InternalServerError
                };

                var detail = statusCode == HttpStatusCode.InternalServerError
                     ? "An unexpected error occurred on the server."
                     : ex.Message;


                var pd = new ProblemDetails()
                {
                    Status = (int)statusCode,
                    Instance = context.Request.Path,
                    Detail = detail,
                    Title = GetTitle(statusCode),
                };

                pd.Extensions["traceId"] = traceId;

                context.Response.StatusCode = (int)statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(pd);
            }
        }
        private string GetTitle(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.BadRequest => "Bad Request",
                HttpStatusCode.NotFound => "Not Found",
                HttpStatusCode.Conflict => "Conflict",
                HttpStatusCode.Forbidden => "Forbidden",
                HttpStatusCode.InternalServerError => "Server Error",
                _ => "An error occurred"
            };
        }
    }
}

