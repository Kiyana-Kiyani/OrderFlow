using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Configuration;
using OrderFlow.Contracts.IntegrationEvents;
using OrderFlow.Infrastructure.Authentication;
using OrderFlow.Infrastructure.Identity;
using OrderFlow.Infrastructure.Notifications;
using OrderFlow.Infrastructure.Persistence;
using StackExchange.Redis;


namespace OrderFlow.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration,
            Action<IBusRegistrationConfigurator>? configureConsumers = null,
            Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureRabbitMqEndpoints = null)
        {
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ApplicationDbContext>();
            services.AddScoped<ICourierTrackerService, CourierTrackerService>();
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
                // ۳. تنظیمات احراز هویت (JWT) به همراه پشتیبانی از SignalR
                // این بخش برای خواندن توکن SignalR از کامپوننت وب‌سوکت حیاتی است:
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.Request.Path;

                        // اگر درخواست سمت هاب سیگنال‌آر بود، توکن را از کوئری استرینگ بخوان
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
            services.AddMassTransit(x =>
            {
                //Tell MassTransit to use your existing DbContext for storing outbox rows
                x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
                {
                    // Tells MassTransit to use your specific DB provider rules
                    o.UseSqlServer();
                    // CRITICAL: Automatically intercepts your IPublishEndpoint.Publish() calls 
                    // and diverts the messages into your local database outbox tables instead of RabbitMQ.
                    o.UseBusOutbox();// Automates message dispatching from the outbox table to RabbitMQ
                });
                configureConsumers?.Invoke(x);

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMq["Host"], rabbitMq["VirtualHost"], h =>
                    {
                        h.Username(rabbitMq["Username"]!);
                        h.Password(rabbitMq["Password"]!);
                    });

                    cfg.Message<OrderPlacedIntegrationEvent>(x => x.SetEntityName("orderflow.events"));
                    cfg.Publish<OrderPlacedIntegrationEvent>(x =>
                    {
                        x.ExchangeType = "topic";
                        x.Durable = true;
                    });
                    configureRabbitMqEndpoints?.Invoke(context, cfg);
                });
            });

            // ۱. ثبت سرویس SignalR
            // 1. Add SignalR and service dependency mapping to the container builder
            services.AddSignalR();
            // ۲. ثبت سرویس نوتیفیکیشن در DI Container
            services.AddScoped<IOrderNotificationService, OrderNotificationService>();




            // Extract your connection string from appsettings.json
            var redisConnectionString = configuration.GetSection("Redis")["ConnectionString"] ?? "localhost:6379";
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
            // -----------------------------------------------------------------------------
            // TYPE 1 REGISTERED HERE: The Shared Whiteboard (Distributed Cache)
            // -----------------------------------------------------------------------------
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "OrderFlow:"; // Keeps your keys grouped together cleanly
            });

            // -----------------------------------------------------------------------------
            // TYPE 2 REGISTERED HERE: The Radio Tower (SignalR Redis Pub/Sub Backplane)
            // -----------------------------------------------------------------------------
            services.AddSignalR()
                            .AddStackExchangeRedis(redisConnectionString, options =>
                            {
                                // SignalR automatically uses Redis Pub/Sub channels here 
                                // to link your web servers together like a radio network.
                                options.Configuration.ChannelPrefix = "OrderFlow_SignalR";
                            });


            //services.AddSingleton<IConnection>(sp =>
            //{
            //    var rabbit = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            //    if (string.IsNullOrWhiteSpace(rabbit.Host) || string.IsNullOrWhiteSpace(rabbit.Username) ||
            //        string.IsNullOrWhiteSpace(rabbit.Password) || rabbit.Port <= 0)
            //    {
            //        throw new InvalidOperationException("RabbitMQ configuration is invalid.");
            //    }

            //    var factory = new ConnectionFactory
            //    {
            //        HostName = rabbit.Host,
            //        Port = rabbit.Port,
            //        UserName = rabbit.Username,
            //        Password = rabbit.Password
            //    };

            //    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            //});
            //       services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();


            return services;
        }
    }
}
