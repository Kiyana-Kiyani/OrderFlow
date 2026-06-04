namespace OrderFlow.Application.Features.Admin.GetUserById
{
    public record GetUserByIdResponse(
    Guid UserId,
    string Email,
    string UserName,
    bool EmailConfirmed,
    IReadOnlyList<string> Roles);
}
