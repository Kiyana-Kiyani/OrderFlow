namespace OrderFlow.Application.Features.Restaurant.GetRestaurants
{
    public record GetRestaurantsResponse(
    Guid Id,
    string Name,
    string Address,
    string? Description,
    bool IsActive);
}
