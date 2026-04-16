using OrderFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderFlow.Infrastructure.Persistence.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(oi => oi.Id);
            builder.Property(oi => oi.MenuItemId).IsRequired();
            builder.Property(oi => oi.MenuItemName).IsRequired().HasMaxLength(100);
            builder.Property(oi => oi.Quantity).IsRequired();
            builder.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(oi => oi.LineTotal).HasColumnType("decimal(18,2)");
            builder.Property(oi => oi.CustomerOrderId).IsRequired();

        }
    }
}
