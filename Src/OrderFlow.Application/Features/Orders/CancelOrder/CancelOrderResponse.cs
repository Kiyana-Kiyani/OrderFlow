using OrderFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Features.Orders.CancelOrder
{
    public record CancelOrderResponse(
        Guid OrderId,
        OrderStatus Status
    );
}
