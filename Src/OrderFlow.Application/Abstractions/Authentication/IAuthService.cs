using OrderFlow.Application.Common.Models;

namespace OrderFlow.Application.Abstractions.Authentication
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(string email, string password, CancellationToken cancellationToken);
        Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken);
    }
}
