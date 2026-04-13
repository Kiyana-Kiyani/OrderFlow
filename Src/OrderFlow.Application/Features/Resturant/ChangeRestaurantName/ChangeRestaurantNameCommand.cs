using MediatR;

namespace OrderFlow.Application.Features.Resturant.ChangeRestaurantName
{
    public record ChangeRestaurantNameCommand(Guid RestaurantId, string NewName) : IRequest;
}