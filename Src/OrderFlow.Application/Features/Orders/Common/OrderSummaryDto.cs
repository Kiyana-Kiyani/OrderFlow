using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Features.Orders.Common
{
    public sealed record OrderSummaryDto(
        Guid OrderId,
        Guid RestaurantId,
        string RestaurantName,
        OrderStatus Status,
        decimal TotalAmount,
        DateTime CreatedAtUtc
    );
}
