using MediatR;

namespace OrderFlow.Application.Features.Resturant.ChangeRestaurantAddress
{
    public record ChangeRestaurantAddressCommand(Guid RestaurantId, string NewAddress) : IRequest;

}
