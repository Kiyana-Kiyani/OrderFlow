using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Infrastructure.Authentication;
using OrderFlow.Infrastructure.Identity;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

            services.AddHttpContextAccessor();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default")!));

            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() ??
                             throw new InvalidOperationException("JWT configuration is missing.");

            services.AddIdentity<AppIdentityUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            }).AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,

                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens
                    .SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });

            services.AddAuthorization();
            
            return services;
        }
    }
}
