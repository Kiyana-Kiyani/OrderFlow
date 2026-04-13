namespace OrderFlow.Application.Features.Auth.Register
{
    public record RegisterUserResponse(
        Guid UserId,
        string Token
    );
}
