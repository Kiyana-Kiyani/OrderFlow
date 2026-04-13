using MediatR;

namespace OrderFlow.Application.Features.Resturant.ActivateRestaurant
{
    public record ActivateRestaurantCommand(Guid RestaurantId) : IRequest;

}
