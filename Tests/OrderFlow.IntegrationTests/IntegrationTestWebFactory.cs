using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Api;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.IntegrationTests.Auth;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace OrderFlow.IntegrationTests
{
    public class IntegrationTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
         .Build();

        private readonly RabbitMqContainer _rabbitContainer = new RabbitMqBuilder("rabbitmq:3-management")
         .Build();


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");


            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

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
            });


            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RabbitMQ:Host", _rabbitContainer.Hostname },
                    { "RabbitMQ:Port", _rabbitContainer.GetMappedPublicPort(5672).ToString() },
                    { "RabbitMQ:Username", RabbitMqBuilder.DefaultUsername },
                    { "RabbitMQ:Password", RabbitMqBuilder.DefaultPassword },
                    { "ConnectionStrings:Default", _dbContainer.GetConnectionString() }
                });
            });

        }

        public async ValueTask InitializeAsync()
        {
            // 4. Start BOTH containers
            await Task.WhenAll(_dbContainer.StartAsync(), _rabbitContainer.StartAsync());

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await dbContext.Database.MigrateAsync();
        }

        public override async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_dbContainer.DisposeAsync().AsTask(), _rabbitContainer.DisposeAsync().AsTask());
            await base.DisposeAsync();
        }
    }
}
