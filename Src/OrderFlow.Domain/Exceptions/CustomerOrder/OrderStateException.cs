namespace OrderFlow.Domain.Exceptions.CustomerOrder
{
    internal class OrderStateException : DomainException
    {
        public OrderStateException(string message) : base(message)
        {
        }
    }
}