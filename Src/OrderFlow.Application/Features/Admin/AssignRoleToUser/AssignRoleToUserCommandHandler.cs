using MediatR;
using OrderFlow.Application.Abstractions.Authentication;

namespace OrderFlow.Application.Features.Admin.AssignRoleToUser
{
    public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand>
    {
        private readonly IAdminService _adminService;

        public AssignRoleToUserCommandHandler(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
        {
            await _adminService.AssignRoleToUserAsync(request.UserId, request.Role, cancellationToken);
        }
    }
}
