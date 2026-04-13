using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Abstractions
{
    public interface IApplicationDbContext
    {
        DbSet<Restaurant> Restaurants { get; }
        DbSet<MenuItem> MenuItems { get; }
        DbSet<CustomerOrder> CustomerOrders { get; }
        DbSet<OrderItem> OrderItems { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
