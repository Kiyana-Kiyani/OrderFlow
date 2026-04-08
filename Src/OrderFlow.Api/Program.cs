using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderFlow.Application.Configuration;
using RabbitMQ.Client;

namespace OrderFlow.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("RabbitMQ"));
            var rabbit = builder.Configuration.GetSection("RabbitMQ").Get<RabbitOptions>() ??  throw new InvalidOperationException("RabbitMQ configuration is missing.");
            if (string.IsNullOrWhiteSpace(rabbit.Host) ||
           string.IsNullOrWhiteSpace(rabbit.Username) ||
           string.IsNullOrWhiteSpace(rabbit.Password) ||
           rabbit.Port <= 0)
            {
                throw new InvalidOperationException("RabbitMQ configuration is invalid.");
            }
            builder.Services.AddSingleton<IConnection>(sp =>
              {
                  var factory = new ConnectionFactory
                  {
                      HostName = rabbit.Host,
                      Port = rabbit.Port,
                      UserName = rabbit.Username,
                      Password = rabbit.Password
                  };
                  return factory.CreateConnectionAsync().GetAwaiter().GetResult();
              });


            builder.Services.AddHealthChecks()
                .AddSqlServer(
                    builder.Configuration.GetConnectionString("Default")!,
                    failureStatus: HealthStatus.Unhealthy,
                    name: "sqlserver",
                    tags: new[] { "ready" })
                .AddRabbitMQ(
                    sp => sp.GetRequiredService<IConnection>(),
                    failureStatus: HealthStatus.Unhealthy,
                    name: "rabbitmq",
                    tags: new[] { "ready" }
                );



            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();


            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains("ready")
            });

            app.Run();
        }
    }
}