using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OrderFlow.Api.Swagger
{
    public class GlobalExceptionOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Responses ??= new OpenApiResponses();

            var problemDetailsSchema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository);

            var problemDetailsContent = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = problemDetailsSchema }
            };

            if (!operation.Responses.ContainsKey("500"))
            {
                operation.Responses.Add("500", new OpenApiResponse
                {
                    Description = "Server Error - An unexpected error occurred on the server.",
                    Content = problemDetailsContent
                });
            }

            var hasAuthorize = context.ApiDescription.ActionDescriptor.EndpointMetadata
                .Any(item => item is Microsoft.AspNetCore.Authorization.AuthorizeAttribute);

            if (hasAuthorize)
            {
                if (!operation.Responses.ContainsKey("401"))
                {
                    operation.Responses.Add("401", new OpenApiResponse
                    {
                        Description = "Unauthorized - User is not authenticated.",
                        Content = problemDetailsContent
                    });
                }

                if (!operation.Responses.ContainsKey("403"))
                {
                    operation.Responses.Add("403", new OpenApiResponse
                    {
                        Description = "Forbidden - User does not have the required permissions/roles.",
                        Content = problemDetailsContent
                    });
                }
            }
        }
    }
}
