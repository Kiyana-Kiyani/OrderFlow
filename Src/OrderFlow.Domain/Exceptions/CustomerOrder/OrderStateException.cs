namespace OrderFlow.Domain.Exceptions.CustomerOrder
{
    public class OrderStateException : DomainException
    {
        public OrderStateException(string message) : base(message)
        {
        }
    }
}