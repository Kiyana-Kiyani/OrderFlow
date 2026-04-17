using MediatR;
using OrderFlow.Application.Common.Models;

namespace OrderFlow.Application.Features.Auth.Login
{
    public record LoginUserCommand(string Email, string Password) : IRequest<AuthResult>;
}
