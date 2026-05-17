using MediatR;
using OrderFlow.Application.Abstractions.Authentication;

namespace OrderFlow.Application.Features.Admin.GetAllRols
{
    public class GetAllRolsQueryHandler : IRequestHandler<GetAllRolsQuery, GetAllRolsResponse>
    {
        private readonly IAdminService _adminService;

        public GetAllRolsQueryHandler(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<GetAllRolsResponse> Handle(GetAllRolsQuery request, CancellationToken cancellationToken)
        {
            var roles = await _adminService.GetAllRolesAsync(cancellationToken);
            return new GetAllRolsResponse(roles);
        }
    }
}
