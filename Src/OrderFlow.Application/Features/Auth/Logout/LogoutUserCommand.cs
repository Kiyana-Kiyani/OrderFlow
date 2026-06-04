using MediatR;
using OrderFlow.Application.Common.Models;

namespace OrderFlow.Application.Features.Auth.Logout
{
    public record LogoutUserCommand() : IRequest<AuthResult>;
}
