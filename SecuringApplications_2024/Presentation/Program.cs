using Common.Models;
using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DataAccess.Repositories;
using Presentation.Utilities;
using Serilog;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<LibraryDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<CustomUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
                .AddDefaultUI()
                .AddEntityFrameworkStores<LibraryDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
                microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
            });

            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<BooksRepository>();
            builder.Services.AddScoped<CategoriesRepository>();
            builder.Services.AddScoped<PermissionsRepository>();
            builder.Services.AddScoped<Encryption>();

            Serilog.ILogger log = Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs/log.txt",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)
                .CreateLogger();

            builder.Logging.AddSerilog(log);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
}
