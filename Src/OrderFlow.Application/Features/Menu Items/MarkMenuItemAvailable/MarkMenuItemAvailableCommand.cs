using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Features.Menu_Items.MarkMenuItemAvailable
{
    public record MarkMenuItemAvailableCommand(
        Guid RestaurantId,
        Guid MenuItemId
    ) : IRequest;
}
