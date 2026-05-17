using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions.Authentication;

namespace OrderFlow.Application.Features.Admin.RemoveRoleFromUser
{
    public class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand>
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<RemoveRoleFromUserCommandHandler> _logger;
        private readonly ICurrentUser _currentUser;


        public RemoveRoleFromUserCommandHandler(IAdminService adminService, ILogger<RemoveRoleFromUserCommandHandler> logger, ICurrentUser currentUser)
        {
            _adminService = adminService;
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
        {
            await _adminService.RemoveRoleFromUserAsync(request.UserId, request.Role, cancellationToken);
            _logger.LogWarning("Admin {AdminId} removed role '{Role}' from User {TargetUserId}.",
                  _currentUser.UserId, request.Role, request.UserId);
        }
    }
}
