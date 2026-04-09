using Microsoft.AspNetCore.Identity;

namespace OrderFlow.Infrastructure.Identity
{
    public class AppIdentityUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
