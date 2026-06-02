using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations
{
    public class CourierConfiguration : IEntityTypeConfiguration<Courier>
    {
        public void Configure(EntityTypeBuilder<Courier> builder)
        {
            // ۱. تنظیم کلید اصلی
            builder.HasKey(c => c.Id);

            // 🚀 نکته مهندسی: چون آیدی پیک از سمت لایه Identity (کاربر لاگین شده) می‌آید،
            // به EF Core می‌گوییم دیتابیس نباید خودش آیدی هولد کند یا خودکار بسازد.
            builder.Property(c => c.Id)
                .ValueGeneratedNever();

            // ۲. تنظیمات فیلدهای متنی و عمومی
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // ۳. تبدیل Enum وسیله نقلیه به رشته در دیتابیس (مشابه فلو کامند سفارش شما)
            builder.Property(c => c.VehicleType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(c => c.IsAvailable)
                .IsRequired();

            // ۴. تنظیمات فیلدهای لکیشن و جغرافیا
            // فیلدهای double در SQL Server به طور پیش‌فرض به float(53) مپ می‌شوند و غیرقابل نال (Required) هستند.
            builder.Property(c => c.CurrentLatitude)
                .IsRequired();

            builder.Property(c => c.CurrentLongitude)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();
        }
    }
}