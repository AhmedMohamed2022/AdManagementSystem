using AdSystem.Data;
using AdSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdManagementSystem.Data
{
    public static class SeedData
    {
        private const string AdminEmail = "admin@adms.com";
        private const string AdminPassword = "Admin@123";
        private static readonly string[] Roles = new[] { "Admin", "Advertiser", "Publisher" };

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            try
            {
                // Don't run migrations in production - tables should already exist
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (env == "Development")
                {
                    await context.Database.MigrateAsync();
                }

                Console.WriteLine($"🔍 Seeding in {env ?? "Unknown"} environment...");

                // 1️⃣ Create roles if not exist
                foreach (var role in Roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(role));
                        if (result.Succeeded)
                        {
                            Console.WriteLine($"✅ Role '{role}' created successfully.");
                        }
                        else
                        {
                            Console.WriteLine($"❌ Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"ℹ️ Role '{role}' already exists.");
                    }
                }

                // 2️⃣ Create default admin if not exist
                var adminUser = await userManager.FindByEmailAsync(AdminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = AdminEmail,
                        Email = AdminEmail,
                        DisplayName = "System Administrator",
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = false,
                        IsActive= true,
                    };

                    var result = await userManager.CreateAsync(adminUser, AdminPassword);

                    if (result.Succeeded)
                    {
                        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                        if (roleResult.Succeeded)
                        {
                            Console.WriteLine("✅ Default Admin user created and role assigned successfully.");
                            Console.WriteLine($"   Email: {AdminEmail}");
                            Console.WriteLine($"   Password: {AdminPassword}");
                        }
                        else
                        {
                            Console.WriteLine($"❌ Failed to assign Admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"❌ Failed to create Admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️ Admin user already exists.");

                    // Make sure admin has the Admin role
                    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine("✅ Admin role added to existing admin user.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during seeding: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                throw;
            }
        }
    }
}