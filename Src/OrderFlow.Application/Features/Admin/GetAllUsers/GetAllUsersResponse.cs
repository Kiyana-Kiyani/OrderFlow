namespace OrderFlow.Application.Features.Admin.GetAllUsers
{
    public record GetAllUsersResponse
(
    Guid UserId,
    string Email,
    string UserName,
    IReadOnlyList<string> Roles
);
}
