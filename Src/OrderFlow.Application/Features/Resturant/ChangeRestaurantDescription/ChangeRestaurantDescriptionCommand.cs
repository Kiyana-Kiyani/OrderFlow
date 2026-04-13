using MediatR;

namespace OrderFlow.Application.Features.Resturant.ChangeRestaurantDescription
{
    public record ChangeRestaurantDescriptionCommand(Guid RestaurantId, string? NewDescription) : IRequest;
}