namespace OrderFlow.Domain.Exceptions.MenuItem
{
    public class NegativeOrZeroPriceException : DomainException
    {
        public NegativeOrZeroPriceException(decimal invalidPrice)
            : base($"A menu item price cannot be negative or zero. Attempted price: {invalidPrice}")
        {
        }
    }
}
