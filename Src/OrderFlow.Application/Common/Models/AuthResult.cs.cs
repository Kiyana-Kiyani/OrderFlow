namespace OrderFlow.Application.Common.Models
{
    public class AuthResult
    {
        public bool Succeeded { get; private set; }
        public string? Error { get; private set; }
        public string? Token { get; private set; }
        public Guid? UserId { get; private set; }

        private AuthResult() { }
        public static AuthResult Success(string token, Guid userId)
        {
            return new AuthResult
            {
                Succeeded = true,
                Token = token,
                UserId = userId
            };
        }

        public static AuthResult Failure(string error)
        {
            return new AuthResult
            {
                Succeeded = false,
                Error = error
            };
        }
    }
}
