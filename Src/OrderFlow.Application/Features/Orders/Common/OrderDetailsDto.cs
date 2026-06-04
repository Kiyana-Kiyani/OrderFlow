using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Features.Orders.Common
{
    public record OrderDetailsDto(
        Guid OrderId,
        Guid CustomerUserId,
        Guid RestaurantId,
        string RestaurantName,
        OrderStatus Status,
        decimal TotalAmount,
        DateTime CreatedAtUtc,
        IReadOnlyList<OrderItemDto> Items
    );
}
