namespace OrderFlow.Domain.Exceptions.Restauramt
{
    internal class MenuItemNotFoundException : DomainException
    {
        public MenuItemNotFoundException(Guid menuItemId)
                : base($"Menu item with ID {menuItemId} was not found in this restaurant.")
        { }
    }
}
