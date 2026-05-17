using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Models;

namespace OrderFlow.Application.Features.Auth.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResult>
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(IAuthService authService, ILogger<RegisterUserCommandHandler> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        public async Task<AuthResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request.Email, request.Password, cancellationToken);
            if (result.Succeeded)
                _logger.LogInformation("User with Email {Email} registered successfully.", request.Email);
            else
                _logger.LogWarning("Failed registration attempt for Email {Email}.", request.Email);

            return result;
        }
    }
}
