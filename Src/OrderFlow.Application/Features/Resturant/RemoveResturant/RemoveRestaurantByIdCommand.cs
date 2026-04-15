using MediatR;

namespace OrderFlow.Application.Features.Resturant.RemoveResturant
{
    public record RemoveRestaurantByIdCommand(Guid Id) : IRequest;

}
