using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Domain.Constants;
using OrderFlow.Infrastructure.Identity;

namespace OrderFlow.Infrastructure.Persistence.Seed
{
    public static class IdentityDataSeeder
    {
        public static async Task RoleSeederAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            foreach (var roleName in Roles.All)
            {
                var exists = await roleManager.RoleExistsAsync(roleName);

                if (!exists)
                {
                    var role = new IdentityRole<Guid>(roleName);
                    await roleManager.CreateAsync(role);
                }
            }
        }

        public static async Task AdminSeederAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppIdentityUser>>();

            var email = "admin@orderflow.com";
            var password = "Admin123!";

            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new AppIdentityUser
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user, password);
            }

            if (!await userManager.IsInRoleAsync(user, Roles.Admin))
            {
                await userManager.AddToRoleAsync(user, Roles.Admin);
            }
        }


    }
}
