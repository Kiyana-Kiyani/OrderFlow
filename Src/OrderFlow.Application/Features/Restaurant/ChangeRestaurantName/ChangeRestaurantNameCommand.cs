using MediatR;

namespace OrderFlow.Application.Features.Restaurant.ChangeRestaurantName
{
    public record ChangeRestaurantNameCommand(Guid RestaurantId, string NewName) : IRequest;
}