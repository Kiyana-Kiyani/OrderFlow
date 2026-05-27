using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Exceptions.CustomerOrder;

namespace OrderFlow.Domain.Entities
{
    public class CustomerOrder
    {
        private readonly List<OrderItem> _orderItems = new();

        public CustomerOrder(Guid customerUserId, Guid restaurantId, string restaurantName, string customerAddress, double customerLatitude, double customerLongitude)
        {
            if (customerUserId == Guid.Empty) throw new ArgumentException("Customer is required.", nameof(customerUserId));
            if (restaurantId == Guid.Empty) throw new ArgumentException("Restaurant is required.", nameof(restaurantId));
            Id = Guid.NewGuid();
            CustomerUserId = customerUserId;
            CustomerAddress = customerAddress;
            CustomerLatitude = customerLatitude;
            CustomerLongitude = customerLongitude;
            RestaurantId = restaurantId;
            RestaurantName = restaurantName;
            Status = OrderStatus.Created;
            CreatedAt = DateTime.UtcNow;
            Payment = PaymentStatus.Pending;
        }
        private CustomerOrder() { }

        public Guid Id { get; private set; }
        public Guid CustomerUserId { get; private set; }
        public Guid? CourierUserId { get; private set; }
        public Guid RestaurantId { get; private set; }
        public string RestaurantName { get; private set; }
        public OrderStatus Status { get; private set; }
        public PaymentStatus Payment { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public decimal TotalAmount { get; private set; }
        public string CustomerAddress { get; private set; } = default!;
        public double CustomerLatitude { get; private set; }
        public double CustomerLongitude { get; private set; }
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;


        private void RecalculateTotalAmount()
        {
            TotalAmount = _orderItems.Sum(x => x.LineTotal);
        }
        public void AddOrderItem(int quantity, decimal unitPrice, Guid menuItemId, string menuItemName)
        {
            EnsureEditable();

            if (menuItemId == Guid.Empty) throw new ArgumentException("Menu item is required.", nameof(menuItemId));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            if (unitPrice <= 0) throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be greater than zero.");

            var orderItem = _orderItems.FirstOrDefault(x => x.MenuItemId == menuItemId);

            if (orderItem != null)
                orderItem.ChangeQuantity(orderItem.Quantity + quantity);
            else
            {
                var newOrderItem = new OrderItem(quantity, unitPrice, menuItemId, menuItemName, Id);
                _orderItems.Add(newOrderItem);
            }

            RecalculateTotalAmount();
        }

        public void RemoveItem(Guid orderItemId)
        {
            EnsureEditable();

            var item = GetItem(orderItemId);
            _orderItems.Remove(item);
            RecalculateTotalAmount();
        }

        private OrderItem GetItem(Guid orderItemId)
        {
            var item = _orderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (item == null) throw new OrderItemNotFoundException(orderItemId);
            return item;
        }
        private void EnsureEditable()
        {
            if (Status != OrderStatus.Created)
                throw new OrderStateException("Order items can only be changed while order is in Created status.");

            if (Status == OrderStatus.Cancelled)
                throw new OrderStateException("Cannot modify a cancelled order.");
        }

        // Called by PaymentSucceededConsumer
        public void MarkPaymentAsSucceeded()
        {
            if (Payment != PaymentStatus.Pending)
                throw new OrderStateException("Payment is not pending.");

            Payment = PaymentStatus.Succeeded;
            // Status stays OrderStatus.Created! But now it's safe for the kitchen to see.
        }

        // Called by PaymentFailedConsumer
        public void MarkPaymentAsFailed()
        {
            if (Payment != PaymentStatus.Pending)
                throw new OrderStateException("Payment is not pending.");

            Payment = PaymentStatus.Failed;
            Status = OrderStatus.Cancelled; // Automatically turn the order off structurally
        }

        // Kitchen action guard check
        public void StartPreparing()
        {
            if (Payment != PaymentStatus.Succeeded)
                throw new OrderStateException("Cannot start preparation on an unpaid order.");

            if (Status != OrderStatus.Created)
                throw new OrderStateException("Order is not in a valid state to start preparing.");

            Status = OrderStatus.Preparing;
        }

        public void Cancel()
        {
            // Prevent manual cancellation entirely if it's already been funded
            if (Payment == PaymentStatus.Succeeded)
                throw new OrderStateException("Paid orders cannot be canceled due to no-refund policy constraints.");

            if (Status != OrderStatus.Created)
                throw new OrderStateException("Only pending orders can be canceled.");

            Status = OrderStatus.Cancelled;
        }

        public void TransitionToReadyForPickup()
        {
            if (Status != OrderStatus.Preparing)
                throw new OrderStateException("Only orders that are being prepared can be sent.");
            Status = OrderStatus.ReadyForPickup;
        }
        public void AssignCourier(Guid courierId)
        {
            if (courierId == Guid.Empty)
                throw new ArgumentException("Courier ID cannot be empty.", nameof(courierId));

            if (Status != OrderStatus.ReadyForPickup)
                throw new OrderStateException("An order must be ready for pickup before a courier can claim it.");

            if (CourierUserId.HasValue)
                throw new OrderStateException("This order has already been claimed by another courier.");

            CourierUserId = courierId;
        }
        public void TransitionToOutForDelivery(Guid courierId)
        {
            if (Status != OrderStatus.ReadyForPickup)
                throw new OrderStateException("Only orders ready for pickup can be transitioned to delivery.");

            if (CourierUserId != courierId)
                throw new OrderStateException("Only the assigned courier can pick up this order.");

            Status = OrderStatus.OutForDelivery;
        }

        public void TransitionToDelivered(Guid courierId)
        {
            if (Status != OrderStatus.OutForDelivery)
                throw new OrderStateException("Only orders that are out for delivery can be marked as delivered.");

            if (CourierUserId != courierId)
                throw new OrderStateException("Only the assigned courier can finalize this delivery.");

            Status = OrderStatus.Delivered;
        }
    }
}
