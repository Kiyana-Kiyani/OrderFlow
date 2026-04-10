using OrderFlow.Domain.Enums;

namespace OrderFlow.Domain.Entities
{
    public class CustomerOrder
    {
        private readonly List<OrderItem> _orderItems = new();

        private CustomerOrder() { }

        public CustomerOrder(Guid customerUserId, Guid restaurantId)
        {
            if (customerUserId == Guid.Empty) throw new ArgumentException("Customer is required.", nameof(customerUserId));
            if (restaurantId == Guid.Empty) throw new ArgumentException("Restaurant is required.", nameof(restaurantId));
            Id = Guid.NewGuid();
            CustomerUserId = customerUserId;
            RestaurantId = restaurantId;
            Status = OrderStatus.Created;
            CreatedAt = DateTime.UtcNow;

        }

        public Guid Id { get; private set; }
        public Guid CustomerUserId { get; private set; }
        public Guid RestaurantId { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public decimal TotalAmount => _orderItems.Sum(x => x.TotalPrice);
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        public void AddOrderItem(int quantity, decimal unitPrice, Guid menuItemId, string menuItemName)
        {
            if (menuItemId == Guid.Empty) throw new ArgumentException("Menu item is required.", nameof(menuItemId));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            if (unitPrice <= 0) throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be greater than zero.");

            var orderItem = _orderItems.FirstOrDefault(x => x.MenuItemId == menuItemId);
            if (orderItem != null)
            {
                orderItem.ChangeQuantity(orderItem.Quantity + quantity);
            }
            else
            {
                var newOrderItem = new OrderItem(quantity, unitPrice, menuItemId, menuItemName, Id);
                _orderItems.Add(newOrderItem);
            }
        }

        public void ChangeQuantity(Guid orderItemId, int quantity)
        {
            EnsureEditable();
            var orderItem = GetItem(orderItemId);
            orderItem.ChangeQuantity(quantity);
        }
       
        public void RemoveItem(Guid orderItemId)
        {
            EnsureEditable();

            var item = GetItem(orderItemId);
            _orderItems.Remove(item);
        }

        private OrderItem GetItem(Guid orderItemId)
        {
            var item = _orderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (item == null) throw new ArgumentException("Order item not found.", nameof(orderItemId));
            return item;
        }
        private void EnsureEditable()
        {
            if (Status != OrderStatus.Created)
                throw new InvalidOperationException("Order items can only be changed while order is in Created status.");
        }

        public void Accept()
        {

            if (Status != OrderStatus.Created)
                throw new InvalidOperationException("Only created orders can be accepted.");

            Status = OrderStatus.Accepted;

        }

        public void Reject()
        {
            if (Status != OrderStatus.Created)
                throw new InvalidOperationException("Only created orders can be rejected.");
            Status = OrderStatus.Rejected;
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Delivered)
                throw new InvalidOperationException("Delivered orders cannot be cancelled.");

            if (Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Order is already cancelled.");
            if (Status == OrderStatus.OutForDelivery)
                throw new InvalidOperationException("Orders that are being sent cannot be cancelled.");

            Status = OrderStatus.Cancelled;
        }

        public void Dispatch()
        {
            if (Status != OrderStatus.Accepted)
                throw new InvalidOperationException("Only accepted orders can be sent.");
            Status = OrderStatus.OutForDelivery;
        }

        public void Deliver()
        {
            if (Status != OrderStatus.OutForDelivery)
                throw new InvalidOperationException("Only orders that are being sent can be marked as delivered.");
            Status = OrderStatus.Delivered;
        }

    }
}
