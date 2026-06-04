using MediatR;

namespace OrderFlow.Application.Features.Restaurant.DeactivateRestaurant
{
    public record DeactivateRestaurantCommand(Guid RestaurantId) : IRequest;
}