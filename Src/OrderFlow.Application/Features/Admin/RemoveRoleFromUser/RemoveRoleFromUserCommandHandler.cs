using MediatR;
using OrderFlow.Application.Abstractions.Authentication;

namespace OrderFlow.Application.Features.Admin.RemoveRoleFromUser
{
    public class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand>
    {
        private readonly IAdminService _adminService;

        public RemoveRoleFromUserCommandHandler(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
        {
            await _adminService.RemoveRoleFromUserAsync(request.UserId, request.Role, cancellationToken);
        }
    }
}
