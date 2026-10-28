using Capitec.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace Capitec.Business.Constants
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[]
            {
                Roles.Admin,
                Roles.User,
                Roles.Manager,
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
