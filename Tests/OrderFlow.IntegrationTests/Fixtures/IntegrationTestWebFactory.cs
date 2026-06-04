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
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace OrderFlow.IntegrationTests.Fixtures
{
    public class IntegrationTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        private readonly RabbitMqContainer _rabbitContainer = new RabbitMqBuilder("rabbitmq:3-management").Build();
        private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine").Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Serilog.Log.Logger = new Serilog.LoggerConfiguration().CreateLogger();
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(_dbContainer.GetConnectionString()));

                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = "TestAuth";
                    options.DefaultChallengeScheme = "TestAuth";
                    options.DefaultScheme = "TestAuth";
                });

                services.AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", options => { });

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
                    { "Redis:ConnectionString", _redisContainer.GetConnectionString() },
                    { "Redis__ConnectionString", _redisContainer.GetConnectionString() }
                });
            });

        }

        public async ValueTask InitializeAsync()
        {
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
