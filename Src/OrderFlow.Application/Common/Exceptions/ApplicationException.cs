namespace OrderFlow.Application.Common.Exceptions
{
    public abstract class ApplicationException : Exception
    {
        public ApplicationException()
        {
        }

        public ApplicationException(string? message) : base(message)
        {
        }
    }
}
