namespace OrderFlow.Api.Contracts.Auth;

public record RefreshTokenRequest(string ExpiredToken, string RefreshToken);