using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations
{
    public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            builder.HasKey(mi => mi.Id);
            builder.Property(mi => mi.Name).IsRequired().HasMaxLength(100);
            builder.Property(mi => mi.Description).HasMaxLength(500);
            builder.Property(mi => mi.Price).HasColumnType("decimal(18,2)");
            builder.Property(mi => mi.RestaurantId).IsRequired();

            builder.HasIndex(mi => new { mi.RestaurantId, mi.Name }).IsUnique();

        }
    }
}
