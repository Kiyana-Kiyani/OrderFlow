using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Models;

namespace OrderFlow.Application.Features.Auth.Logout
{
    public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, AuthResult>
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LogoutUserCommandHandler> _logger;
        private readonly ICurrentUser _currentUser;

        public LogoutUserCommandHandler(IAuthService authService, ILogger<LogoutUserCommandHandler> logger, ICurrentUser currentUser)
        {
            _authService = authService;
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<AuthResult> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.LogoutAsync(_currentUser.UserId, cancellationToken);

            if (result.Succeeded)
                _logger.LogInformation("User with ID {UserId} logged out successfully.", _currentUser.UserId);
            else
                _logger.LogWarning("Failed logout attempt for User ID {UserId}.", _currentUser.UserId);

            return result;
        }
    }
}
