using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Features.Admin.Common;
using OrderFlow.Infrastructure.Identity;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Authentication
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ApplicationDbContext _dbContext;

        public AdminService(UserManager<AppIdentityUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        public async Task AssignRoleToUserAsync(Guid userId, string role, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new NotFoundException("User",userId);

            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
                throw new NotFoundException("Role", role);

            if (await _userManager.IsInRoleAsync(user, role))
                return;

            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, string role, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new NotFoundException("User", userId);

            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
                throw new NotFoundException("Role", role);

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        public async Task<IReadOnlyList<AdminUserDto>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            var users = await _dbContext.Users
                .OrderBy(x => x.Email)
                .Select(x => new AdminUserDto
                (
                    x.Id,
                    x.Email ?? string.Empty,
                    x.UserName ?? string.Empty,
                    _dbContext.UserRoles.Where(ur => ur.UserId == x.Id)
                        .Join(_dbContext.Roles,
                            uri => uri.RoleId,
                            r => r.Id,
                            (uri, r) => r.Name!).ToList()

                )).ToListAsync(cancellationToken);

            return users;
        }

        public async Task<AdminUserDetailsDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                      .AsNoTracking()
                      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user is null)
                throw new NotFoundException("User", userId);

            var roles = await _userManager.GetRolesAsync(user);

            return new AdminUserDetailsDto(
                user.Id,
                user.Email ?? string.Empty,
                user.UserName ?? string.Empty,
                user.EmailConfirmed,
                roles.ToList());
        }

        public async Task<IReadOnlyList<string?>> GetAllRolesAsync(CancellationToken cancellationToken)
        {
            var roles = await _dbContext.Roles.Select(x => x.Name).ToListAsync();
            return roles.AsReadOnly();
        }
    }
}