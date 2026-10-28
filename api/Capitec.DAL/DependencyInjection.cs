using Capitec.DAL.Data;
using Capitec.DAL.Data.Entities;
using Capitec.DAL.Persistence.Abstractions;
using Capitec.DAL.Persistence.UoW;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Capitec.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDAL(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<CapitecDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Capitec")));
        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<CapitecDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        return services;
    }
}