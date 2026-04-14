namespace OrderFlow.Application.Features.Admin.GetAllRols
{
    public record GetAllRolsResponse
    (
        IReadOnlyList<string> Roles
        );
}
