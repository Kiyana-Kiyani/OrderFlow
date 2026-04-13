using MediatR;

namespace OrderFlow.Application.Features.Auth.Register
{
    public record RegisterUserCommand
    (
        string Email,
        string Password
        ) : IRequest<RegisterUserResponse>;

}
