using OrderFlow.Domain.Enums;

namespace OrderFlow.Domain.Entities
{
    public class Courier
    {
        public Courier(Guid id, string name, VehicleType vehicleType)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("User ID is required to create a courier.", nameof(id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Courier name cannot be empty.", nameof(name));
            Id = id;
            Name = name;
            VehicleType = vehicleType;
            IsAvailable = false;
            CreatedAt = DateTime.UtcNow;
        }
        private Courier() { }

        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;
        public VehicleType VehicleType { get; private set; }
        public bool IsAvailable { get; private set; }
        public double CurrentLatitude { get; private set; }
        public double CurrentLongitude { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public void ToggleAvailability()
        {
            IsAvailable = !IsAvailable;
        }

        public void UpdateLocation(double latitude, double longitude)
        {
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