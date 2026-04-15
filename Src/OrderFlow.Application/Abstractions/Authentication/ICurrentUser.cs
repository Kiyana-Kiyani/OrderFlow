using System.Security.Claims;

namespace OrderFlow.Application.Abstractions.Authentication
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        IReadOnlyList<string> Roles { get; }
        ClaimsPrincipal User { get; }
    }
}
