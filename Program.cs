//using AdManagementSystem.Data;
//using AdManagementSystem.Services;
//using AdSystem.Data;
//using AdSystem.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.EntityFrameworkCore;
//using System;

//namespace AdManagementSystem
//{
//    public class Program
//    {
//        public static async Task Main(string[] args)
//        {
//            var builder = WebApplication.CreateBuilder(args);

//            // Add services to the container.
//            builder.Services.AddControllersWithViews();
//            builder.Services.AddRazorPages();

//            // Configure EF connection
//            builder.Services.AddDbContext<AppDbContext>(options =>
//                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


//            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//      .AddEntityFrameworkStores<AppDbContext>()
//      .AddDefaultTokenProviders();
//            builder.Services.ConfigureApplicationCookie(options =>
//            {
//                options.LoginPath = "/Identity/Account/Login";
//                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
//            });

//            builder.Services.AddScoped<IEmailSender, FakeEmailSender>();
//            builder.Services.AddScoped<IAdService, AdService>();



//            builder.Services.AddCors(options =>
//            {
//                options.AddPolicy("AllowAll", policy =>
//                {
//                    policy
//                        .AllowAnyOrigin()
//                        .AllowAnyHeader()
//                        .AllowAnyMethod();
//                });
//            });

//            var app = builder.Build();

//            // Configure the HTTP request pipeline.
//            if (!app.Environment.IsDevelopment())
//            {
//                app.UseExceptionHandler("/Home/Error");
//                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//                app.UseHsts();
//            }

//            app.UseHttpsRedirection();
//            app.UseStaticFiles();

//            app.UseRouting();
//            app.UseCors("AllowAll");

//            app.UseAuthentication();
//            app.UseAuthorization();
//            app.MapControllers();

//            //default route
//            app.MapControllerRoute(
//                name: "default",
//                pattern: "{controller=Home}/{action=Index}/{id?}");
         
//            app.MapRazorPages(); // enable built-in login/register pages
//            using (var scope = app.Services.CreateScope())
//            {
//                var services = scope.ServiceProvider;
//                await SeedData.InitializeAsync(services);
//            }
//            app.Run();
//        }
//    }
//}
using AdManagementSystem.Data;
using AdManagementSystem.Services;
using AdSystem.Data;
using AdSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;

namespace AdManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // Configure EF connection
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.SlidingExpiration = true;

                // Important for production
                if (!builder.Environment.IsDevelopment())
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                }
            });

            builder.Services.AddScoped<IEmailSender, FakeEmailSender>();
            builder.Services.AddScoped<IAdService, AdService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                // Show detailed errors in development
                app.UseDeveloperExceptionPage();
            }

            // IMPORTANT: Comment out HTTPS redirection for Somee free tier
            // Somee free tier doesn't support HTTPS properly
            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Default route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages(); // Enable built-in login/register pages

            // Seed data only if not in production or if needed
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    // Only seed in development or first run
                    if (app.Environment.IsDevelopment())
                    {
                        await SeedData.InitializeAsync(services);
                        logger.LogInformation("Database seeded successfully");
                    }
                    else
                    {
                        // In production, just check if roles exist
                        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                        if (!await roleManager.RoleExistsAsync("Admin"))
                        {
                            logger.LogWarning("Roles not found in production. Run seed manually.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            app.Run();
        }
    }
}