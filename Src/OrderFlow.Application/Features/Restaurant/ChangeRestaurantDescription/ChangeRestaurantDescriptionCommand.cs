using MediatR;

namespace OrderFlow.Application.Features.Restaurant.ChangeRestaurantDescription
{
    public record ChangeRestaurantDescriptionCommand(Guid RestaurantId, string? NewDescription) : IRequest;
}