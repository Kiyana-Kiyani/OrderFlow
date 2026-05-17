namespace OrderFlow.Application.Features.Admin.Common
{
    public record AdminUserDetailsDto(
        Guid UserId,
        string Email,
        string UserName,
        bool EmailConfirmed,
        IReadOnlyList<string> Roles
    );
}
