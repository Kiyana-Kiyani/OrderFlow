using MediatR;

namespace OrderFlow.Application.Features.Admin.RemoveRoleFromUser
{
    public record RemoveRoleFromUserCommand(Guid UserId, string Role) : IRequest;
}
