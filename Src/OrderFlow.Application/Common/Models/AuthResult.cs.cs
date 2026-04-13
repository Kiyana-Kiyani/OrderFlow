namespace OrderFlow.Application.Common.Models
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public string? Error { get; set; }
        public string? Token { get; set; }
        public Guid? UserId { get; set; }

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
