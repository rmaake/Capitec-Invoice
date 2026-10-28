using Capitec.Business.Services.Options;
using Capitec.DAL.Data.Entities;
using Capitec.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Business.Constants
{
    public static class IdentitySeeder
    {
        public static async Task SeedAdminUserAsync(IServiceProvider services)
        {

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var adminUserConfig = services.GetRequiredService<IConfiguration>().GetSection("AdminUser").Get<DefaultAdminUserOptions>();

            const string adminRole = Roles.Admin;

            // Create role
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // Create admin user
            var adminUser = await userManager.FindByEmailAsync(adminUserConfig.Username);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    FirstName = "System",
                    LastName = "Administrator",
                    UserName = adminUserConfig.Username,
                    Email = adminUserConfig.Username,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    adminUser,
                    adminUserConfig.Password);

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(",",
                        result.Errors.Select(e => e.Description)));
                }

                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
    }
}
