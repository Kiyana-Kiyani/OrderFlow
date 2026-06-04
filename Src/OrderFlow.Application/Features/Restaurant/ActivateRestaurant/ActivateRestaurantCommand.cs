using MediatR;

namespace OrderFlow.Application.Features.Restaurant.ActivateRestaurant
{
    public record ActivateRestaurantCommand(Guid RestaurantId) : IRequest;

}
