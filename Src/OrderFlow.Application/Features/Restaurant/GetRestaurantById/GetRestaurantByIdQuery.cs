using MediatR;

namespace OrderFlow.Application.Features.Restaurant.GetRestaurantById
{
    public record GetRestaurantByIdQuery(Guid Id) : IRequest<GetRestaurantByIdResponse>;
}
