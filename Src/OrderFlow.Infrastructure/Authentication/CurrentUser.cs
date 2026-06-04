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

        public ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User ?? throw new UnauthorizedAccessException("User is not authenticated.");

        public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

        public string? Email => User.FindFirstValue(ClaimTypes.Email);

        public Guid UserId
        {
            get
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
            }
        }

        public IReadOnlyList<string> Roles =>
            User.FindAll(ClaimTypes.Role)
            .Select(x => x.Value).
            ToList().AsReadOnly();
    }
}