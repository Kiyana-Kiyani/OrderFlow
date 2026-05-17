using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OrderFlow.Api.Consumers;
using OrderFlow.Api.Middleware;
using OrderFlow.Api.Swagger;
using OrderFlow.Application;
using OrderFlow.Infrastructure.DependencyInjection;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Persistence.Seed;
using Serilog;

namespace OrderFlow.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting web application...");

                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog((context, services, configuration) => configuration
                        .MinimumLevel.Information()
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Application", "OrderApi")
                        .ReadFrom.Services(services)
                        .WriteTo.Console()
                        .WriteTo.Seq(context.Configuration["Seq:Url"]!));

                builder.Services.AddInfrastructure(builder.Configuration,
                    configureConsumers: x =>
                    {
                        x.AddConsumer<PaymentSucceededConsumer>();
                        x.AddConsumer<PaymentFailedConsumer>();
                    },

                    configureRabbitMqEndpoints: (context, cfg) =>
                    {
                        cfg.ReceiveEndpoint("orderflow-payment-queue", e =>
                        {
                            e.SetQuorumQueue();
                            e.ConfigureConsumeTopology = false;
                            e.Bind("Payment.Result", s =>
                            {
                                s.RoutingKey = "order.placed.*";
                                s.ExchangeType = "topic";
                            });

                            e.ConfigureConsumer<PaymentSucceededConsumer>(context);
                            e.ConfigureConsumer<PaymentFailedConsumer>(context);
                        });
                    });

                builder.Services.AddApplication();

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

                    options.OperationFilter<GlobalExceptionOperationFilter>();
                });

                builder.Services.AddHealthChecks()
                    .AddSqlServer(
                        builder.Configuration.GetConnectionString("Default")!,
                        failureStatus: HealthStatus.Unhealthy,
                        name: "sqlserver",
                        tags: new[] { "ready" });

                var app = builder.Build();

                app.UseMiddleware<GlobalExceptionMiddleware>();

                app.UseSerilogRequestLogging();

                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    if (app.Environment.IsDevelopment())
                    {
                        dbContext.Database.Migrate();
                    }
                }

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();

                    await IdentityDataSeeder.RoleSeederAsync(app.Services);
                    await IdentityDataSeeder.AdminSeederAsync(app.Services);
                }
                if (app.Environment.IsEnvironment("Testing"))
                {
                    using (var scope = app.Services.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        await dbContext.Database.MigrateAsync();
                    }

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
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}