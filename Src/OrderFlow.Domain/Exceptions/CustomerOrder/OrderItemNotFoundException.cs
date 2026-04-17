namespace OrderFlow.Domain.Exceptions.CustomerOrder
{
    internal class OrderItemNotFoundException : DomainException
    {
        public OrderItemNotFoundException(Guid orderItemId)
            : base($"Order item with ID {orderItemId} was not found.")
        {
        }
    }
}
