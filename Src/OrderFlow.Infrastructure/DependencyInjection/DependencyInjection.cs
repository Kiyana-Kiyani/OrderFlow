using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Configuration;
using OrderFlow.Infrastructure.Authentication;
using OrderFlow.Infrastructure.Consumers;
using OrderFlow.Infrastructure.Identity;
using OrderFlow.Infrastructure.Notifications;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Services;
using StackExchange.Redis;


namespace OrderFlow.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ICourierTrackerService, CourierTrackerService>();
            services.AddHttpContextAccessor();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default")!));

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());


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

            services.AddScoped<RoleManager<IdentityRole<Guid>>>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/orders"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();

            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));

            var rabbitMq = configuration.GetSection("RabbitMQ");

            var redisConnectionString = configuration.GetSection("Redis")["ConnectionString"] ?? "localhost:6379";

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(redisConnectionString);
                configuration.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(configuration);
            });


            services.AddSignalR(options =>
            {
                options.AddFilter<OrderHubFilter>();
            }).AddStackExchangeRedis(redisConnectionString, options =>
                {
                    options.Configuration.ChannelPrefix = RedisChannel.Literal("OrderFlow_WebSockets");
                });


            services.AddMassTransit(x =>
                   {
                       x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
                       {
                           o.UseSqlServer();
                           o.UseBusOutbox();
                       });

                       x.AddConsumer<PaymentSucceededConsumer>();
                       x.AddConsumer<PaymentFailedConsumer>();
                       x.AddConsumer<OrderPickedUpConsumer>();

                       x.SetKebabCaseEndpointNameFormatter();

                       x.UsingRabbitMq((context, cfg) =>
                       {
                           cfg.Host(rabbitMq["Host"], rabbitMq["VirtualHost"], h =>
                           {
                               h.Username(rabbitMq["Username"]!);
                               h.Password(rabbitMq["Password"]!);
                           });
                           cfg.UseMessageRetry(r =>
                           {
                               r.Interval(3, TimeSpan.FromSeconds(2));
                           });

                           cfg.ConfigureEndpoints(context);

                       });
                   });

            services.AddSingleton<IOrderNotificationService, OrderNotificationService>();

            return services;
        }
    }
}
