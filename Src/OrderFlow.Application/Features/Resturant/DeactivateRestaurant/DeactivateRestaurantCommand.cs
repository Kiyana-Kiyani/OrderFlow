using MediatR;

namespace OrderFlow.Application.Features.Resturant.DeactivateRestaurant
{
    public record DeactivateRestaurantCommand(Guid RestaurantId) : IRequest;
}