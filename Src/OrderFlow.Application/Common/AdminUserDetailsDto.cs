namespace OrderFlow.Application.Common
{
    public record AdminUserDetailsDto(
        Guid UserId,
        string Email,
        string UserName,
        bool EmailConfirmed,
        IReadOnlyList<string> Roles
    );
}
