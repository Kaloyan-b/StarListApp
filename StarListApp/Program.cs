using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StarListApp.Data;
using StarListApp.Models;
using System;

namespace StarListApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Get connection string from configuration
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Add services to the container
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add identity services
        builder.Services.AddDefaultIdentity<StarListUser>(options =>
                options.SignIn.RequireConfirmedAccount = false) // Change if you want confirmed accounts
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Add other necessary services
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddControllersWithViews();

        // Build and configure the app
        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}
