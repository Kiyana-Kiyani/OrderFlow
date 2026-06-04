using MediatR;

namespace OrderFlow.Application.Common.Events
{
    public record UserRoleAssignedEvent(Guid UserId, string Role) : INotification;
}