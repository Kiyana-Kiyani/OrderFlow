using MediatR;

namespace OrderFlow.Application.Features.Admin.GetAllUsers
{
    public record GetAllUsersQuery() : IRequest<IReadOnlyList<GetAllUsersResponse>>;
}
