using OrderFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderFlow.Infrastructure.Persistence.Configurations
{
    public class CustomerOrderConfiguration : IEntityTypeConfiguration<CustomerOrder>
    {
        public void Configure(EntityTypeBuilder<CustomerOrder> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.CustomerUserId).IsRequired();
            builder.Property(c => c.RestaurantId).IsRequired();
            builder.Property(c => c.TotalAmount).HasColumnType("decimal(18,2)");

            builder.Navigation(c => c.OrderItems).UsePropertyAccessMode(PropertyAccessMode.Field);
            
            builder.HasMany(c => c.OrderItems)
                .WithOne()
                .HasForeignKey(oi => oi.CustomerOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(c => c.TotalAmount);

        }
    }
}
