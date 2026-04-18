using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Common.Exceptions;
using System.Net;

namespace OrderFlow.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public GlobalExceptionMiddleware(RequestDelegate requestDelegate, ILogger logger)
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
                var statusCode = switch (ex)
                {
                    ValidationException => CreateResponse(ex, context, (int)HttpStatusCode.BadRequest),
                    NotFoundException => CreateResponse(ex, context, (int)HttpStatusCode.NotFound),
                    ConflictException => CreateResponse(ex, context, (int)HttpStatusCode.Conflict),
                    ForbiddenException => CreateResponse(ex, context, (int)HttpStatusCode.Forbidden),
                    DomainException => CreateResponse(ex, context, (int)HttpStatusCode.BadRequest),
                    ArgumentNullException => CreateResponse(ex, context, (int)HttpStatusCode.BadRequest),
                    ArgumentException => CreateResponse(ex, context, (int)HttpStatusCode.BadRequest),
                    ArgumentOutOfRangeException => CreateResponse(ex, context, (int)HttpStatusCode.BadRequest),
                    KeyNotFoundException => CreateResponse(ex, context, (int)HttpStatusCode.NotFound),
                    UnauthorizedAccessException => CreateResponse(ex, context, (int)HttpStatusCode.Unauthorized),
                    InvalidOperationException => CreateResponse(ex, context, (int)HttpStatusCode.BadRequest),
                    _ => CreateResponse(ex, context, (int)HttpStatusCode.InternalServerError)

                };

             }
        }
    }
}

