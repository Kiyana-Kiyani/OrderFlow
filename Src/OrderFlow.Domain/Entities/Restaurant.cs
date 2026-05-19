using OrderFlow.Domain.Exceptions.Restaurant;

namespace OrderFlow.Domain.Entities
{
    public class Restaurant
    {
        private readonly List<MenuItem> _menuItems = new();
        public Restaurant(Guid ownerUserId, string name, string address, string? description = null)
        {
            if (ownerUserId == Guid.Empty) throw new ArgumentException("Owner is required.", nameof(ownerUserId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Restaurant name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Restaurant address is required.", nameof(address));
            Id = Guid.NewGuid();
            OwnerUserId = ownerUserId;
            Name = name.Trim();
            Address = address.Trim();
            IsActive = true;
            CreatedAtUtc = DateTime.UtcNow;
            Description = NormalizeOptional(description);
        }
        private Restaurant() { }

        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;
        public string Address { get; private set; } = default!;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }
        public Guid OwnerUserId { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public IReadOnlyCollection<MenuItem> MenuItems => _menuItems;

        public void ChangeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Restaurant name is required.", nameof(name));
            Name = name.Trim();
        }
        public void ChangeAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Restaurant address is required.", nameof(address));
            Address = address.Trim();
        }
        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
        public void ChangeDescription(string? description) => Description = NormalizeOptional(description);
        public Guid AddMenuItem(string name, decimal price, string? description = null)
        {
            EnsureRestaurantIsActive();

            if (_menuItems.Any(x => x.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new DuplicateMenuItemException(name);

            var item = new MenuItem(name, price, Id, description);
            _menuItems.Add(item);
            return item.Id;
        }
        private void EnsureRestaurantIsActive()
        {
            if (!IsActive)
                throw new RestaurantInactiveException();
        }

        private MenuItem GetMenuItem(Guid menuItemId)
        {
            var item = _menuItems.FirstOrDefault(x => x.Id == menuItemId);
            if (item is null)
                throw new MenuItemNotFoundException(menuItemId);

            return item;
        }

        public void ChangeMenuItemName(Guid menuItemId, string newName)
        {
            var item = GetMenuItem(menuItemId);

            if (_menuItems.Any(x => x.Id != menuItemId &&
                    x.Name.Equals(newName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new DuplicateMenuItemException(newName);
            }

            item.ChangeName(newName);
        }
        public void ChangeMenuItemDescription(Guid menuItemId, string? newDescription)
        {
            var item = GetMenuItem(menuItemId);
            item.ChangeDescription(newDescription);
        }

        public void ChangeMenuItemPrice(Guid menuItemId, decimal newPrice)
        {
            var item = GetMenuItem(menuItemId);
            item.ChangePrice(newPrice);
        }
        public void MarkMenuItemAvailable(Guid menuItemId)
        {
            var item = GetMenuItem(menuItemId);
            item.MarkAsAvailable();
        }

        public void MarkMenuItemUnavailable(Guid menuItemId)
        {
            var item = GetMenuItem(menuItemId);
            item.MarkAsUnavailable();
        }

        public void RemoveMenuItem(Guid menuItemId)
        {
            var item = GetMenuItem(menuItemId);
            _menuItems.Remove(item);
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
