namespace OrderFlow.Application.Abstractions.Authentication
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Guid id, string email, IEnumerable<string> roles, Dictionary<string, string>? customClaims = null);
        string GenerateRefreshToken();
    }
}
