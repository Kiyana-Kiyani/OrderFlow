using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Api;
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
        private DbConnection _connection = default!;
        private Respawner _respawner = default!;
        protected readonly WebApplicationFactory<Program> Factory;

        public BaseIntegrationTest(WebApplicationFactory<Program> factory)
        {
            // 1. Setup the customized factory once
            Factory = factory;

            // 2. Create the client and scope from the SAME customized factory
            Client = Factory.CreateClient();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("TestAuth");

            _scope = Factory.Services.CreateScope();
            _sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        }

        public async ValueTask InitializeAsync()
        {
            var dbContext = Factory.Services.GetRequiredService<ApplicationDbContext>();
            _connection = dbContext.Database.GetDbConnection();
            await _connection.OpenAsync();

            _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
            {
                TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
            });
            //   await dbContext.Database.MigrateAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _respawner.ResetAsync(_connection);
            _scope.Dispose();
            await _connection.CloseAsync();
        }
    }
}
