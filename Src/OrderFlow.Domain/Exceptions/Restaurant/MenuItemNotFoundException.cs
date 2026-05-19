namespace OrderFlow.Domain.Exceptions.Restaurant
{
    internal class MenuItemNotFoundException : DomainException
    {
        public MenuItemNotFoundException(Guid menuItemId)
                : base($"Menu item with ID {menuItemId} was not found in this restaurant.")
        { }
    }
}
