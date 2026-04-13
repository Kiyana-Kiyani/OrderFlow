using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.MenuItems.MarkMenuItemUnavailable;

public class MarkMenuItemUnavailableCommandHandler
    : IRequestHandler<MarkMenuItemUnavailableCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public MarkMenuItemUnavailableCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(
        MarkMenuItemUnavailableCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _dbContext.Restaurants
            .Include(r => r.MenuItems)
            .FirstOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);

        if (restaurant is null)
            throw new InvalidOperationException("Restaurant not found.");

        restaurant.MarkMenuItemUnavailable(request.MenuItemId);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}