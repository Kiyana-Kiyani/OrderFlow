using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Menu_Items.MarkMenuItemAvailable
{
    public class MarkMenuItemAvailableCommandHandler
        : IRequestHandler<MarkMenuItemAvailableCommand>
    {
        private readonly IApplicationDbContext _dbContext;

        public MarkMenuItemAvailableCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(MarkMenuItemAvailableCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants
              .Include(r => r.MenuItems)
              .FirstOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);

            if (restaurant is null)
                throw new InvalidOperationException("Restaurant not found.");

            restaurant.MarkMenuItemAvailable(request.MenuItemId);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
