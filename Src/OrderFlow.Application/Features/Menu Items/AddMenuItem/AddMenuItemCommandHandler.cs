using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Menu_Items.AddMenuItem
{
    public class AddMenuItemCommandHandler : IRequestHandler<AddMenuItemCommand, AddMenuItemResponse>
    {
        private readonly IApplicationDbContext _dbContext;

        public AddMenuItemCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<AddMenuItemResponse> Handle(AddMenuItemCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants
                 .Include(x => x.MenuItems)
                 .FirstOrDefaultAsync(x => x.Id == request.RestaurantId, cancellationToken);


            if (restaurant is null)
                throw new InvalidOperationException("Restaurant not found.");

            var menuItemId = restaurant.AddMenuItem(
                request.Name,
                request.Price,
                request.Description);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new AddMenuItemResponse(menuItemId);
        }
    }
}
