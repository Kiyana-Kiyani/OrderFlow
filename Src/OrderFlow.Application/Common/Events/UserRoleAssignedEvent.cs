using MediatR;

namespace OrderFlow.Application.Common.Events
{
    // این یک Notification است، یعنی می‌تواند چندین Handler همزمان داشته باشد
    public record UserRoleAssignedEvent(Guid UserId, string Role) : INotification;
}