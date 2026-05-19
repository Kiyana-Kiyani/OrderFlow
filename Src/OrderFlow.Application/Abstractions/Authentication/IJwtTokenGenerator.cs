namespace OrderFlow.Application.Abstractions.Authentication
{
    public interface IJwtTokenGenerator
    {
        Task<string> GenerateTokenAsync(Guid id, string email, IEnumerable<string> roles);
        string GenerateRefreshToken();
    }
}
