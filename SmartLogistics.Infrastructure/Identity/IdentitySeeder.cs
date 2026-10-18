using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        private static readonly string[] Roles = { "Admin", "WarehouseManager", "Driver" };

        public static async Task SeedAsync(IServiceProvider sp)
        {
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in Roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            const string email = "admin@smartlogistics.com";
            if (await userManager.FindByEmailAsync(email) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "System Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@12345");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
