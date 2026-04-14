using MediatR;
using OrderFlow.Application.Abstractions.Authentication;

namespace OrderFlow.Application.Features.Admin.GetAllUsers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IReadOnlyList<GetAllUsersResponse>>
    {
        private readonly IAdminService _adminService;

        public GetAllUsersQueryHandler(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IReadOnlyList<GetAllUsersResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _adminService.GetAllUsersAsync(cancellationToken);
            return users.Select(x => new GetAllUsersResponse(
                x.UserId,
                x.Email,
                x.UserName,
                x.Roles))
            .ToList();

        }
    }
}
