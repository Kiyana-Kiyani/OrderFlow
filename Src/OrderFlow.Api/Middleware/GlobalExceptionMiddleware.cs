using FluentValidation;
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


                if(ex is ValidationException)
                {
                    var pronblemDetail = new ProblemDetails()
                    {
                        Detail = ex.Message,
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "Validation Failed",
                        Instance = context.Request.Path

                    };
                }
                else if(ex is NotFoundException)
                {
                    var pronblemDetail = new ProblemDetails()
                    {
                        Detail = ex.Message,
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "Validation Failed",
                        Instance = context.Request.Path

                    };
                }
                else if(ex is NotFoundException)
                {
                    var pronblemDetail = new ProblemDetails()
                    {
                        Detail = ex.Message,
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "Validation Failed",
                        Instance = context.Request.Path

                    };
                }
                else if(ex is KeyNotFoundException)
                {
                    var pronblemDetail = new ProblemDetails()
                    {
                        Detail = ex.Message,
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "Validation Failed",
                        Instance = context.Request.Path

                    };
                }
                else if(ex is UnauthorizedAccessException)
                {
                    var pronblemDetail = new ProblemDetails()
                    {
                        Detail = ex.Message,
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "Validation Failed",
                        Instance = context.Request.Path

                    };
                }
                else if(ex is ForbiddenException)
                {
                    var pronblemDetail = new ProblemDetails()
                    {
                        Detail = ex.Message,
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "Validation Failed",
                        Instance = context.Request.Path

                    };
                }
                else if(ex is ConflictException)
                {
                    var pronblemDetail = new ProblemDetails()
                    {
                        Detail = ex.Message,
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "Validation Failed",
                        Instance = context.Request.Path

                    };
                }
                else if(ex is ArgumentNullException)
                {
                    var pronblemDetail = new ProblemDetails()
                    {
                        Detail = ex.Message,
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "Validation Failed",
                        Instance = context.Request.Path

                    };
                }
                else if(ex is ArgumentOutOfRangeException)
                {
                    var pronblemDetail = new ProblemDetails()
                    {
                        Detail = ex.Message,
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "Validation Failed",
                        Instance = context.Request.Path

                    };
                }
                else if(ex is InvalidOperationException)
                {
                    var pronblemDetail = new ProblemDetails()
                    {
                        Detail = ex.Message,
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "Validation Failed",
                        Instance = context.Request.Path

                    };
                }
                else 
                {
                    var pronblemDetail = new ProblemDetails()
                    {
                        Detail = ex.Message,
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "Validation Failed",
                        Instance = context.Request.Path

                    };
                }






            }

        }
    }
}
