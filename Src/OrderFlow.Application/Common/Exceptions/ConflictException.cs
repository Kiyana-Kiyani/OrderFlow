namespace OrderFlow.Application.Common.Exceptions
{
    public class ConflictException : ApplicationException
    {
        public ConflictException() : base()
        {
        }

        public ConflictException(string? message) : base(message)
        {
        }
    }
}
