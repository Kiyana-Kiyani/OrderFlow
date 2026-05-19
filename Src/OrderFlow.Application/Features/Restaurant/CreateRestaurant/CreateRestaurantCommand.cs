using MediatR;

namespace OrderFlow.Application.Features.Restaurant.CreateRestaurant
{
    public record CreateRestaurantCommand(string Name, string Address, string? Description, Guid OwnerId)
        : IRequest<CreateRestaurantResponse>;
}
