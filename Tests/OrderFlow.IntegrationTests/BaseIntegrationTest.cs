using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.IntegrationTests.Auth;
using Respawn;
using System.Data.Common;

namespace OrderFlow.IntegrationTests
{
    public class BaseIntegrationTest : IClassFixture<IntegrationTestWebFactory>, IAsyncLifetime
    {
        private readonly ISender _sender;
        private readonly IServiceScope _scope;
        protected readonly HttpClient Client;
        private readonly DbConnection _connectionString;
        private Respawner _respawner = default!;
        protected readonly IntegrationTestWebFactory Factory;

        public BaseIntegrationTest(IntegrationTestWebFactory factory)
        {
            Factory = factory;
            _sender = factory.Services.GetRequiredService<ISender>();
            _scope = factory.Services.CreateScope();

            Client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("TestAuth")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", options => { });
                });
            }).CreateClient();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("TestAuth");

            _connectionString = factory.Services.GetRequiredService<ApplicationDbContext>()
                        .Database.GetDbConnection();
        }

        public async ValueTask DisposeAsync()
        {
            _respawner = await Respawner.CreateAsync(_connectionString, new RespawnerOptions
            {
                TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
            });
        }

        public async ValueTask InitializeAsync()
        {
            await _respawner.ResetAsync(_connectionString);
            _scope.Dispose();
        }
    }
}
