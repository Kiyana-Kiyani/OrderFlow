using MediatR;

namespace OrderFlow.Application.Features.Auth.Login
{
    public record LoginUserCommand(string Email, string Password) : IRequest<LoginUserResponse>;
}
