namespace OrderFlow.Application.Features.Resturant.GetResturants
{
    public record GetRestaurantsResponse(
    Guid Id,
    string Name,
    string Address,
    string? Description,
    bool IsActive);
}
