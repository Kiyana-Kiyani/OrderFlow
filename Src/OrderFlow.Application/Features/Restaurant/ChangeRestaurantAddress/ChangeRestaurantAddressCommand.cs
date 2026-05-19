using MediatR;

namespace OrderFlow.Application.Features.Restaurant.ChangeRestaurantAddress
{
    public record ChangeRestaurantAddressCommand(Guid RestaurantId, string NewAddress) : IRequest;

}
