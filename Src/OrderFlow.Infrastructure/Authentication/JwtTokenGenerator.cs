using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Authentication
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _jwtOptions;
        private readonly ApplicationDbContext _dbContext;
        public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions, ApplicationDbContext dbContext)
        {
            _jwtOptions = jwtOptions.Value;
            _dbContext = dbContext;
        }
        public string GenerateTokenAsync(Guid id, string email, IEnumerable<string> roles, Dictionary<string, string>? customClaims = null)
        {
            var claims = CreateClaimsAsync(id, email, roles, customClaims);

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
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private Claim[] CreateClaimsAsync(Guid id, string email, IEnumerable<string> roles, Dictionary<string, string>? customClaims)
        {
            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, id.ToString())
            };

            if (!string.IsNullOrWhiteSpace(email))
                claims.Add(new Claim(ClaimTypes.Email, email));

            // اضافه کردن نقش‌ها
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // اضافه کردن هوشمند کلیم‌های اختصاصی بدون وابستگی به جدول خاص
            if (customClaims is not null)
            {
                foreach (var claim in customClaims)
                {
                    claims.Add(new Claim(claim.Key, claim.Value));
                }
            }

            return claims.ToArray();
        }
    }
}
