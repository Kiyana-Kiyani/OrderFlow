using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OrderFlow.Application.Behaviors;
using OrderFlow.Application.Configuration;
using OrderFlow.Infrastructure.DependencyInjection;
using OrderFlow.Infrastructure.Persistence.Seed;
using RabbitMQ.Client;

namespace OrderFlow.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    BearerFormat = "JWT",
                    Scheme = "Bearer",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,

                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyMarker).Assembly));

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("RabbitMQ"));

            var rabbit = builder.Configuration.GetSection("RabbitMQ").Get<RabbitOptions>() ??
                   throw new InvalidOperationException("RabbitMQ configuration is missing.");

            if (string.IsNullOrWhiteSpace(rabbit.Host) || string.IsNullOrWhiteSpace(rabbit.Username) ||
                   string.IsNullOrWhiteSpace(rabbit.Password) || rabbit.Port <= 0)
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

                await IdentityDataSeeder.RoleSeederAsync(app.Services);
                await IdentityDataSeeder.AdminSeederAsync(app.Services);
            }


            app.UseHttpsRedirection();

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