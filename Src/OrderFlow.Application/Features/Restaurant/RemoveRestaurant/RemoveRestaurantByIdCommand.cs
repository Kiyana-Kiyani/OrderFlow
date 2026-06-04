using MediatR;

namespace OrderFlow.Application.Features.Restaurant.RemoveRestaurant
{
    public record RemoveRestaurantByIdCommand(Guid Id) : IRequest;

}
