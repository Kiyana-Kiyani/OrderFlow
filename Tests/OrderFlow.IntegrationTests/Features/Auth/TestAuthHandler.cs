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
        public const string RoleHeader = "X-Test-Role"; // 🚀 هدر جدید برای کنترل داینامیک نقش‌ها

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
            if (Context.Request.Headers.TryGetValue(UserIdHeader, out var customUserId))
            {
                userId = customUserId.ToString();
            }

            // ۳. ساخت لیست کلیم‌ها به صورت داینامیک
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "TestUser")
            };

            // ۴. 🔥 فیکس بیمه تغییرات:
            // اگر تستِ جاری هدر X-Test-Role را فرستاده بود، همان را اعمال کن؛
            // در غیر این صورت، پیش‌فرض را همان "Courier" قبلی بذار تا بقیه تست‌ها اصلاً دست‌نخورده بمانند.
            if (Context.Request.Headers.TryGetValue(RoleHeader, out var customRoles))
            {
                foreach (var role in customRoles.ToString().Split(','))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "Courier")); // 👈 حفظ هماهنگی با تست‌های قدیمی شما
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestAuth");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}