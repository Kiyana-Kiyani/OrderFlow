using Microsoft.AspNetCore.Authorization;
using OrderFlow.Domain.Entities;
using System.Security.Claims;

namespace OrderFlow.Application.Security.Authorization
{
    public class RestaurantOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, Restaurant>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceOwnerRequirement requirement, Restaurant resource)
        {

            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var userIdString = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdString is null)
                return Task.CompletedTask;

            if (Guid.TryParse(userIdString.Value, out var userGuid) && userGuid == resource.OwnerUserId)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
