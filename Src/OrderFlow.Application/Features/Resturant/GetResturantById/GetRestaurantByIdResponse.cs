namespace OrderFlow.Application.Features.Resturant.GetResturantById
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
