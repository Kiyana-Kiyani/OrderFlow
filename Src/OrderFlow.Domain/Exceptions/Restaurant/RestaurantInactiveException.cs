namespace OrderFlow.Domain.Exceptions.Restaurant
{
    internal class RestaurantInactiveException : DomainException
    {
        public RestaurantInactiveException()
                : base("Restaurant is inactive and its menu cannot be modified.")
        { }
    }

}
