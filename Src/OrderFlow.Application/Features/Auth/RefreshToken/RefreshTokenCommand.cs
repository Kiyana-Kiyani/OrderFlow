using MediatR;
using OrderFlow.Application.Common.Models;

namespace OrderFlow.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string ExpiredToken, string RefreshToken) : IRequest<AuthResult>;
