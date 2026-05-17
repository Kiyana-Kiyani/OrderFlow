using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Models;

namespace OrderFlow.Application.Features.Auth.Login
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResult>
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginUserCommandHandler> _logger;
        public LoginUserCommandHandler(IAuthService authService, ILogger<LoginUserCommandHandler> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        public async Task<AuthResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request.Email, request.Password, cancellationToken);
            if (result.Succeeded)
                _logger.LogInformation("User with Email {Email} logged in successfully.", request.Email);
            else
                _logger.LogWarning("Failed login attempt for Email {Email}.", request.Email);
            return result;
        }
    }
}
