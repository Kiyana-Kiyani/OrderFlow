using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Api;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.IntegrationTests.Features.Auth;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace OrderFlow.IntegrationTests.Fixtures
{
    public class IntegrationTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        private readonly RabbitMqContainer _rabbitContainer = new RabbitMqBuilder("rabbitmq:3-management").Build();
        private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine").Build(); // 👈 اضافه شد

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");


            builder.ConfigureTestServices(services =>
            {
                // ۱. جایگزینی دیتابیس با کانتینر واقعی SQL
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(_dbContainer.GetConnectionString()));

                // ۲. جایگزینی احراز هویت با TestAuth
                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = "TestAuth";
                    options.DefaultChallengeScheme = "TestAuth";
                    options.DefaultScheme = "TestAuth";
                });

                services.AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", options => { });

                // 🚀 ۳. تزریق هارنسِ تستِ مس‌ترنزیت برای رهگیری Outbox
                services.AddMassTransitTestHarness();
            });

            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RabbitMQ:Host", _rabbitContainer.Hostname },
                    { "RabbitMQ:Port", _rabbitContainer.GetMappedPublicPort(5672).ToString() },
                    { "RabbitMQ:Username", RabbitMqBuilder.DefaultUsername },
                    { "RabbitMQ:Password", RabbitMqBuilder.DefaultPassword },
                    { "ConnectionStrings:Default", _dbContainer.GetConnectionString() },
                    { "Redis:ConnectionString", _redisContainer.GetConnectionString() }, // 👈 جلوگیری از کرش SignalR
                    // 🚀 فیکس طلایی: تزریق آیدی پویا با هر دو فرمت آدرس‌دهی دات‌نت برای تضمین پایداری کانکشن
        { "Redis__ConnectionString", _redisContainer.GetConnectionString() }
                });
            });

        }

        public async ValueTask InitializeAsync()
        {
            // استارت همزمان هر سه کانتینر
            await Task.WhenAll(
                _dbContainer.StartAsync(),
                _rabbitContainer.StartAsync(),
                _redisContainer.StartAsync());

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        public override async ValueTask DisposeAsync()
        {
            await Task.WhenAll(
                            _dbContainer.DisposeAsync().AsTask(),
                            _rabbitContainer.DisposeAsync().AsTask(),
                            _redisContainer.DisposeAsync().AsTask());

            await base.DisposeAsync();
        }
    }
}
