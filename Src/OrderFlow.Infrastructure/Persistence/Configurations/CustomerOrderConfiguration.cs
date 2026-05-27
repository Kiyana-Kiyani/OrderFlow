using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations
{
    public class CustomerOrderConfiguration : IEntityTypeConfiguration<CustomerOrder>
    {
        public void Configure(EntityTypeBuilder<CustomerOrder> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.CustomerUserId).IsRequired();
            builder.Property(c => c.RestaurantId).IsRequired();
            builder.Property(c => c.RestaurantName).IsRequired().HasMaxLength(100);

            builder.Property(c => c.TotalAmount).HasColumnType("decimal(18,2)");

            builder.Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(c => c.Payment)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Navigation(c => c.OrderItems)
                .HasField("_orderItems")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(c => c.OrderItems)
                .WithOne()
                .HasForeignKey(oi => oi.CustomerOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
