using MediatR;
using OrderFlow.Application.Common.Models;

namespace OrderFlow.Application.Features.Auth.Register
{
    public record RegisterUserCommand
    (
        string Email,
        string Password
        ) : IRequest<AuthResult>;
}
