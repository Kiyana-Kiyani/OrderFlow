using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Models;
using OrderFlow.Infrastructure.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrderFlow.Infrastructure.Identity
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly JwtOptions _jwtOptions;
        private readonly IApplicationDbContext _dbContext;

        public AuthService(UserManager<AppIdentityUser> userManager, SignInManager<AppIdentityUser> signInManager,
            IJwtTokenGenerator jwtTokenGenerator, IOptions<JwtOptions> jwtOptions, IApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _jwtOptions = jwtOptions.Value;
            _dbContext = dbContext;
        }

        public async Task<AuthResult> RegisterAsync(string email, string password, CancellationToken cancellationToken)
        {

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser is not null)
                return AuthResult.Failure("A user with this email already exists.");

            var user = new AppIdentityUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email
            };

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                return AuthResult.Failure(errors);
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            return await GenerateAuthResultForUserAsync(user);
        }

        public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return AuthResult.Failure("Invalid email or password.");

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (!result.Succeeded)
                return AuthResult.Failure("Invalid email or password.");

            return await GenerateAuthResultForUserAsync(user);

        }

        public async Task<AuthResult> LogoutAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return AuthResult.Failure("User not found.");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.MinValue;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                return AuthResult.Failure($"Failed to process logout: {errors}");
            }

            return AuthResult.SuccessfullLogout();
        }


        private async Task<AuthResult> GenerateAuthResultForUserAsync(AppIdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var customClaims = await BuildCustomClaimsAsync(user.Id, roles);

            var token = _jwtTokenGenerator.GenerateToken(user.Id, user.NormalizedEmail!, roles, customClaims);

            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);
            await _userManager.UpdateAsync(user);

            return AuthResult.Success(token, refreshToken, user.Id);
        }

        public async Task<AuthResult> RefreshTokenAsync(string expiredToken, string refreshToken, CancellationToken cancellationToken)
        {
            var principal = GetPrincipalFromExpiredToken(expiredToken);
            if (principal is null)
                return AuthResult.Failure("Invalid access token structure.");

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return AuthResult.Failure("Invalid claims context.");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return AuthResult.Failure("Invalid or expired refresh token request.");

            var roles = await _userManager.GetRolesAsync(user);
            var customClaims = await BuildCustomClaimsAsync(user.Id, roles);
            var newAccessToken = _jwtTokenGenerator.GenerateToken(user.Id, user.Email!, roles);
            var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return AuthResult.Success(newAccessToken, newRefreshToken, user.Id);
        }
        private async Task<Dictionary<string, string>> BuildCustomClaimsAsync(Guid userId, IEnumerable<string> roles)
        {
            var customClaims = new Dictionary<string, string>();

            if (roles.Contains("Owner"))
            {
                var restaurantId = await _dbContext.Restaurants
                    .Where(r => r.OwnerUserId == userId)
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync();

                if (restaurantId != Guid.Empty)
                {
                    customClaims.Add("RestaurantId", restaurantId.ToString());
                }
            }
            return customClaims;
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidAudience = _jwtOptions.Audience,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
    }
}
