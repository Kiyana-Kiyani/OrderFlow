using MediatR;

namespace OrderFlow.Application.Features.Resturant.GetResturants
{
    public record GetRestaurantsQuery() : IRequest<IReadOnlyList<GetRestaurantsResponse>>;
}
