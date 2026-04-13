using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.MenuItems.RemoveMenuItemById;

public class RemoveMenuItemByIdCommandHandler
    : IRequestHandler<RemoveMenuItemByIdCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveMenuItemByIdCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(
        RemoveMenuItemByIdCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _dbContext.Restaurants
            .Include(r => r.MenuItems)
            .FirstOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);

        if (restaurant is null)
            throw new InvalidOperationException("Restaurant not found.");

        restaurant.RemoveMenuItem(request.MenuItemId);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
