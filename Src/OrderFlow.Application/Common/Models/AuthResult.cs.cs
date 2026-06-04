namespace OrderFlow.Application.Common.Models
{
    public class AuthResult
    {
        public bool Succeeded { get; private set; }
        public string? Error { get; private set; }
        public string? Token { get; private set; }
        public string? RefreshToken { get; private set; }
        public Guid? UserId { get; private set; }

        private AuthResult() { }
        public static AuthResult Success(string token, string refreshToken, Guid userId)
        {
            return new AuthResult
            {
                Succeeded = true,
                Token = token,
                RefreshToken = refreshToken,
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

        public static AuthResult SuccessfullLogout() => new AuthResult
        {
            Succeeded = true,
        };
    }
}
