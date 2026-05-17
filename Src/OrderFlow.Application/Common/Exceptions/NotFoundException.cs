namespace OrderFlow.Application.Common.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException() : base()
        {
        }

        public NotFoundException(string? message) : base(message)
        {
        }
        public NotFoundException(string entityName, object key)
            : base($"{entityName} Not Found. Key: {key}")
        {
        }
    }
}
