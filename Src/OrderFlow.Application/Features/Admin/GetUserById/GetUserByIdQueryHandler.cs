using MediatR;
using OrderFlow.Application.Abstractions.Authentication;

namespace OrderFlow.Application.Features.Admin.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdResponse>
    {
        private readonly IAdminService _adminService;

        public GetUserByIdQueryHandler(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<GetUserByIdResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _adminService.GetUserByIdAsync(request.UserId, cancellationToken);
            if (user == null) throw new InvalidOperationException("User not found.");

            return new GetUserByIdResponse(
                user.UserId,
                user.Email,
                user.UserName,
                user.EmailConfirmed,
                user.Roles
                );

        }
    }
}
