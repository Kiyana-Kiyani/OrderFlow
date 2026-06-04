namespace OrderFlow.Application.Features.Auth.Login
{
    public record LoginUserResponse(Guid UserId, string Token, string RefreshToken);
}
