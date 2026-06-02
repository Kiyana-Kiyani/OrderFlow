using OrderFlow.Domain.Enums;

namespace OrderFlow.Domain.Entities
{
    public class Courier
    {
        // سازنده اصلی بیزینسی (Parameterized Constructor)
        public Courier(Guid id, string name, VehicleType vehicleType)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("User ID is required to create a courier.", nameof(id));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Courier name cannot be empty.", nameof(name));

            Id = id; // شناسه دامین با UserId لایه Identity یکی می‌شود
            Name = name;
            VehicleType = vehicleType;
            IsAvailable = false; // پیک‌ها به صورت پیش‌فرض پس از ثبت‌نام Off-Duty هستند
            CreatedAt = DateTime.UtcNow;
        }

        // سازنده بدون پارامتر فیک صرفاً جهت استفاده لایه ORM (EF Core)
        private Courier() { }

        // پروپرتی‌ها با سطح دسترسی کاملاً کپسوله‌شده (Read-Only fields from outside)
        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;
        public VehicleType VehicleType { get; private set; }
        public bool IsAvailable { get; private set; }
        public double CurrentLatitude { get; private set; }
        public double CurrentLongitude { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // متدهای دامین برای مدیریت تغییرات وضعیت (Domain State Mutations)

        public void ToggleAvailability()
        {
            IsAvailable = !IsAvailable;
        }

        public void UpdateLocation(double latitude, double longitude)
        {
            // در آینده می‌توان کدهای نگهبان برای محدوده جغرافیایی مجاز (Geo-fencing) اینجا اضافه کرد
            CurrentLatitude = latitude;
            CurrentLongitude = longitude;
        }

        public void UpdateVehicleType(VehicleType vehicleType)
        {
            VehicleType = vehicleType;
        }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Courier name cannot be empty.", nameof(name));

            Name = name;
        }
    }
}