namespace OrderFlow.Domain.Exceptions.OrderItem
{
    internal class NegativeOrZeroQuantityException : DomainException
    {
        public NegativeOrZeroQuantityException(decimal invalidPrice)
        : base($"A menu item quantity cannot be negative or zero. Attempted quantity: {invalidPrice}")
        {
        }
    }
}