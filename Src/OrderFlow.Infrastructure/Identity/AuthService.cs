using Microsoft.AspNetCore.Identity;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Models;

namespace OrderFlow.Infrastructure.Identity
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(UserManager<AppIdentityUser> userManager, SignInManager<AppIdentityUser> signInManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResult> RegisterAsync( string email, string password, CancellationToken cancellationToken)
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

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _jwtTokenGenerator.GenerateTokenAsync(user.Id, user.NormalizedEmail!, roles);

            return AuthResult.Success(token, user.Id);
        }

        public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return AuthResult.Failure("Invalid email or password.");

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (!result.Succeeded)
                return AuthResult.Failure("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _jwtTokenGenerator.GenerateTokenAsync(user.Id, user.NormalizedEmail!, roles);

            return AuthResult.Success(token, user.Id);

        }
    }
}
