namespace OrderFlow.Domain.Exceptions.Restauramt
{
    internal class DuplicateMenuItemException : DomainException
    {
        public DuplicateMenuItemException(string name)
                : base($"A menu item with the name '{name}' already exists in this restaurant.")
        { }
    }
}
