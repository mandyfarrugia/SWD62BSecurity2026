using DataAccess.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<TicketBookingSystemDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Lockout.MaxFailedAccessAttempts = 3; //Lock the account after 3 consecutive failed login attempts to prevent brute-force attacks.
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30); //Lock the account for half an hour following 3 consecutive failed attempts.
                    options.Password.RequireNonAlphanumeric = true; //Non-alphanumeric characters are symbols and punctuation marks that are not letters (A-Z/a-z) or digits (0-9).
                    options.Password.RequireUppercase = true; //The password requires at least one uppercase letter (A-Z).
                    options.Password.RequiredUniqueChars = 5; //The password must contain at least 5 unique characters which helps to ensure that the password is not easily guessable and encourages users to create more complex passwords.
                    options.Password.RequiredLength = 8; //The password must be at least 8 characters long which is a common minimum length requirement to enhance security by making passwords harder to guess or brute-force.
                    options.User.RequireUniqueEmail = true; //This setting ensures that each user must have a unique email address, preventing multiple accounts from being registered with the same email and enhancing account security and management.
                })
                .AddEntityFrameworkStores<TicketBookingSystemDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            builder.Services.AddAuthentication()
                .AddGoogle(option =>
                {
                    option.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google Client ID not found in configuration!");
                    option.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not found in configuration!");
                });

            builder.Services.AddControllersWithViews();

            WebApplication? app = builder.Build();

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

            app.UseAuthentication(); //Vital for Identity (external authentication, login, and logout).
            app.UseAuthorization(); //Vital for role management and access control.

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
