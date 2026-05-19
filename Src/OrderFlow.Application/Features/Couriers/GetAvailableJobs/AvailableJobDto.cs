namespace OrderFlow.Application.Features.Couriers.GetAvailableJobs
{
    public record AvailableJobDto(
        Guid OrderId,
        Guid RestaurantId,
        string RestaurantName,
        decimal TotalAmount,
        DateTime ReadyAt);
}
