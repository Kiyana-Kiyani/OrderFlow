using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderFlow.Application.Configuration;
using OrderFlow.Infrastructure.Identity;
using OrderFlow.Infrastructure.Persistence;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace OrderFlow.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            //Cnfiguration 
            var rabbit = builder.Configuration.GetSection("RabbitMQ").Get<RabbitOptions>() ??
                throw new InvalidOperationException("RabbitMQ configuration is missing.");

            //Services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default")!));

            builder.Services.AddIdentity<AppIdentityUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            }).AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
                        ClockSkew = TimeSpan.FromMinutes(1),
                    };
                });

            builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("RabbitMQ"));


            if (string.IsNullOrWhiteSpace(rabbit.Host) || string.IsNullOrWhiteSpace(rabbit.Username) ||
                   string.IsNullOrWhiteSpace(rabbit.Password) || rabbit.Port <= 0 )
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