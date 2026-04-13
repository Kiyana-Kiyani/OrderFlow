using Microsoft.AspNetCore.Http;
using OrderFlow.Application.Abstractions.Authentication;
using System.Security.Claims;

namespace OrderFlow.Infrastructure.Authentication
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Guid UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                var id = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (id is null)
                    throw new UnauthorizedAccessException("User is not authenticated.");

                return Guid.Parse(id);
            }
        }
    }
}