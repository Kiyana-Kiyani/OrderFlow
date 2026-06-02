using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace OrderFlow.IntegrationTests.Features.Auth
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string UserIdHeader = "X-Test-UserId";
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // ۱. بررسی وجود هدر اصلی احراز هویت تست
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("TestAuth"))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // ۲. تنظیم مقدار پیش‌فرض برای آیدی کاربر
            var userId = "00000000-0000-0000-0000-000000000001";

            // ۳. قاپیدن آیدی داینامیک فرستاده شده از سمت کلاینتِ تست
            if (Context.Request.Headers.TryGetValue(UserIdHeader, out var customUserId))
            {
                userId = customUserId.ToString();
            }

            // ۴. 🔥 فیکس اصلی اینجاست: پاس دادن متغیر userId به جای متد راندوم ساز دات‌نت
            var claims = new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId), // 👈 آیدی داینامیک اینجا نشست
            new Claim(ClaimTypes.Name, "TestCourierUser"),
            new Claim(ClaimTypes.Role, "Courier")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestAuth");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
