using OrderFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Features.Orders.PlaceOrder
{
    public sealed record PlaceOrderResponse(
        Guid OrderId,
        OrderStatus Status,
        decimal TotalAmount,
        DateTime CreatedAtUtc
    );
}
