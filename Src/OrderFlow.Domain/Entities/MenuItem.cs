using OrderFlow.Domain.Exceptions.MenuItem;

namespace OrderFlow.Domain.Entities
{
    public class MenuItem
    {
        private MenuItem() { }
        internal MenuItem(string name, decimal price, Guid restaurantId, string? description)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
            if (price <= 0) throw new NegativeOrZeroPriceException(price);
            Name = name.Trim();
            Id = Guid.Empty;
            Description = NormalizeOptional(description);
            Price = price;
            IsAvailable = true;
            RestaurantId = restaurantId;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;
        public string? Description { get; private set; }
        public decimal Price { get; private set; }
        public bool IsAvailable { get; private set; }
        public Guid RestaurantId { get; private set; }

        internal void ChangeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Menu item name is required.", nameof(name));
            Name = name.Trim();
        }

        internal void ChangeDescription(string? description)
        {
            Description = NormalizeOptional(description);
        }

        internal void ChangePrice(decimal price)
        {
            if (price <= 0)
                throw new NegativeOrZeroPriceException(price);
            Price = price;
        }
        internal void MarkAsAvailable() => IsAvailable = true;
        internal void MarkAsUnavailable() => IsAvailable = false;
        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
