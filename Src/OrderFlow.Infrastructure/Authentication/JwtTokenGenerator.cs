using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrderFlow.Infrastructure.Authentication
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly JwtOptions _jwtOptions;

        public JwtTokenGenerator(UserManager<AppIdentityUser> userManager, IOptions<JwtOptions> jwtOptions)
        {
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
        }
        public async Task<string> GenerateTokenAsync(Guid id, string email, IEnumerable<string> roles)
        {
            var claims = await CreateClaimsAsync(id, email, roles);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<Claim[]> CreateClaimsAsync(Guid id, string email, IEnumerable<string> roles)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, id.ToString()));

            if (!string.IsNullOrWhiteSpace(email))
                claims.Add(new Claim(ClaimTypes.Email, email));

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            return claims.ToArray();
        }
    }
}
