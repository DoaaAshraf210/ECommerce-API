using ECommerce.Application.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            foreach (var role in new[] { Roles.Admin, Roles.Customer })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}
