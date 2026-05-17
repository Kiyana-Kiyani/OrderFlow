using MediatR;

namespace OrderFlow.Application.Features.Admin.AssignRoleToUser
{
    public record AssignRoleToUserCommand(
        Guid UserId,
        string Role
    ) : IRequest;
}
