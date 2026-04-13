using MediatR;

namespace OrderFlow.Application.Features.Resturant.GetResturantById
{
    public record GetRestaurantByIdQuery(Guid Id) : IRequest<GetRestaurantByIdResponse>;
}
