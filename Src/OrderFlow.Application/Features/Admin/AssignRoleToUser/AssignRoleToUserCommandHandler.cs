using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions.Authentication;

namespace OrderFlow.Application.Features.Admin.AssignRoleToUser
{
    public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand>
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AssignRoleToUserCommandHandler> _logger;
        private readonly ICurrentUser _currentUser;

        public AssignRoleToUserCommandHandler(IAdminService adminService, ILogger<AssignRoleToUserCommandHandler> logger, ICurrentUser currentUser)
        {
            _adminService = adminService;
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
        {
            await _adminService.AssignRoleToUserAsync(request.UserId, request.Role, cancellationToken);
            _logger.LogWarning("Admin {AdminId} assigned role '{Role}' to User {TargetUserId}.",
                _currentUser.UserId, request.Role, request.UserId);
        }
    }
}
