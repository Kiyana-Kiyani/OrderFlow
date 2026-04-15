namespace OrderFlow.Application.Features.Admin.Common
{
    public record AdminUserDto(
        Guid UserId,
        string Email,
        string UserName,
        IReadOnlyList<string> Roles
    );
}
