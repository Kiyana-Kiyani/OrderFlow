namespace OrderFlow.Domain.Exceptions.Restaurant
{
    internal class DuplicateMenuItemException : DomainException
    {
        public DuplicateMenuItemException(string name)
                : base($"A menu item with the name '{name}' already exists in this restaurant.")
        { }
    }
}
