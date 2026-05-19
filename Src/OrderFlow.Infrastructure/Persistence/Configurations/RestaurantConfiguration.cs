using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations
{
    public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
    {
        public void Configure(EntityTypeBuilder<Restaurant> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
            builder.Property(r => r.Address).IsRequired().HasMaxLength(200);
            builder.Property(r => r.Description).HasMaxLength(500);

            builder.Navigation(r => r.MenuItems)
                .HasField("_menuItems")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(r => r.MenuItems)
                .WithOne()
                .HasForeignKey(mi => mi.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
