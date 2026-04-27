using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using OrderFlow.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace OrderFlow.IntegrationTests
{
    public class IntegrationTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
       private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
             
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(_dbContainer.GetConnectionString()));
            });


        }

        public async ValueTask InitializeAsync() => await _dbContainer.StartAsync();
    }
}
