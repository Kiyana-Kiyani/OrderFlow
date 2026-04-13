using MediatR;
using OrderFlow.Application.Abstractions.Authentication;

namespace OrderFlow.Application.Features.Auth.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
    {
        private readonly IAuthService _authService;

        public RegisterUserCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync( request.Email, request.Password, cancellationToken);

            if (!result.Succeeded || result.UserId is null || string.IsNullOrEmpty(result.Token))
                throw new InvalidOperationException(result.Error ?? "Registration failed.");

            var response = new RegisterUserResponse(
                UserId: result.UserId.Value,
                Token: result.Token
                );

            return response;
        }
    }
}
