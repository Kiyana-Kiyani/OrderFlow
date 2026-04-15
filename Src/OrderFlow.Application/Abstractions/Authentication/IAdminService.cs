using OrderFlow.Application.Features.Admin.Common;

namespace OrderFlow.Application.Abstractions.Authentication
{
    public interface IAdminService
    {
        Task AssignRoleToUserAsync(Guid userId, string role, CancellationToken cancellationToken);
        Task RemoveRoleFromUserAsync(Guid userId, string role, CancellationToken cancellationToken);
        Task<IReadOnlyList<AdminUserDto>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<AdminUserDetailsDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<string>> GetAllRolesAsync(CancellationToken cancellationToken);
    }
}
