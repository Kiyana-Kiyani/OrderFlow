using MediatR;
using OrderFlow.Application.Abstractions.Authentication;

namespace OrderFlow.Application.Features.Auth.Login
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
    {
        private readonly IAuthService _authService;
        public LoginUserCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request.Email, request.Password, cancellationToken);
            if (!result.Succeeded || result.UserId is null || string.IsNullOrEmpty(result.Token)    )
            {
                throw new UnauthorizedAccessException(result.Error ?? "Login failed.");
            }

            return new LoginUserResponse
            (
                UserId : result.UserId.Value ,
                Token :  result.Token
            );

        }
    }
}
