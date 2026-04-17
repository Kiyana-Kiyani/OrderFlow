using OrderFlow.Domain.Exceptions.OrderItem;

namespace OrderFlow.Domain.Entities
{
    public class OrderItem
    {
        internal OrderItem(int quantity, decimal unitPrice, Guid menuItemId, string menuItemName, Guid customerOrderId)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            if (unitPrice <= 0) throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be greater than zero.");
            if (menuItemId == Guid.Empty) throw new ArgumentException("Menu item is required.", nameof(menuItemId));
            if (customerOrderId == Guid.Empty) throw new ArgumentException("Customer order is required.", nameof(customerOrderId));
            if (string.IsNullOrWhiteSpace(menuItemName))
                throw new ArgumentException("Menu item name is required.", nameof(menuItemName));
            Id = Guid.NewGuid();
            Quantity = quantity;
            UnitPrice = unitPrice;
            MenuItemId = menuItemId;
            MenuItemName = menuItemName.Trim();
            CustomerOrderId = customerOrderId;
            LineTotal = quantity * unitPrice;
        }
        private OrderItem() { }

        public Guid Id { get; private set; }
        public Guid CustomerOrderId { get; private set; }
        public Guid MenuItemId { get; private set; }
        public string MenuItemName { get; private set; } = default!;
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal LineTotal { get; private set; }

        internal void ChangeQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new NegativeOrZeroQuantityException(quantity);

            Quantity = quantity;
        }

    }
}