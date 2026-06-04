using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Abstractions
{
    public interface IApplicationDbContext
    {
        DbSet<Restaurant> Restaurants { get; }
        DbSet<MenuItem> MenuItems { get; }
        DbSet<CustomerOrder> CustomerOrders { get; }
        DbSet<OrderItem> OrderItems { get; }
        DbSet<Courier> Couriers { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
