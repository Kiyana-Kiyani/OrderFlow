namespace OrderFlow.Application.Features.Restaurant.GetRestaurantById
{
    public record GetRestaurantByIdResponse
  (
        Guid Id,
    string Name,
    string Address,
    string? Description,
    bool IsActive
   );
}
