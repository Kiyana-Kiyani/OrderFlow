namespace OrderFlow.Domain.Exceptions
{
    public class NegativePriceException : DomainException
    {
        public NegativePriceException(double invalidPrice)
            : base($"A menu item price cannot be negative. Attempted price: {invalidPrice}")
        {
        }
    }
}
