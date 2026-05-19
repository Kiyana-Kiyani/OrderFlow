using MediatR;

namespace OrderFlow.Application.Features.Restaurant.GetRestaurants
{
    public record GetRestaurantsQuery() : IRequest<IReadOnlyList<GetRestaurantsResponse>>;
}
