using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Menu_Items.ChangeMenuItemDescription
{
    public class ChangeMenuItemDescriptionCommandHandler : IRequestHandler<ChangeMenuItemDescriptionCommand>
    {
        private readonly IApplicationDbContext _dbContext;

        public ChangeMenuItemDescriptionCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(ChangeMenuItemDescriptionCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);

            if (restaurant is null)
                throw new InvalidOperationException("Restaurant not found.");

            restaurant.ChangeMenuItemDescription(request.MenuItemId, request.Description);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
