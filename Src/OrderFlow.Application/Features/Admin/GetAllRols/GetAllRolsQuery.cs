using MediatR;

namespace OrderFlow.Application.Features.Admin.GetAllRols
{
    public record GetAllRolsQuery() : IRequest<GetAllRolsResponse>;
}
